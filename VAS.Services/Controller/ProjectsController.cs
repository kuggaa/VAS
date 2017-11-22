//
//  Copyright (C) 2016 Fluendo S.A.
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Interfaces.Plugins;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services.ViewModel;

namespace VAS.Services.Controller
{
	public class ProjectsController<TModel, TViewModel> : ControllerBase<ProjectsManagerVM<TModel, TViewModel>>
		where TModel : Project
		where TViewModel : ProjectVM<TModel>, new()
	{
		MediaFileSet originalMediaFileSet;

		protected override void DisposeManagedResources ()
		{
			ViewModel.IgnoreEvents = true;
			base.DisposeManagedResources ();
			ViewModel = null;
		}

		#region IController implementation

		public override void SetViewModel (IViewModel viewModel)
		{
			ViewModel = (ProjectsManagerVM<TModel, TViewModel>)viewModel;
		}

		public override async Task Start ()
		{
			await base.Start ();
			App.Current.EventsBroker.SubscribeAsync<ExportEvent<TModel>> (HandleExport);
			App.Current.EventsBroker.SubscribeAsync<ImportEvent<TModel>> (HandleImport);
			App.Current.EventsBroker.SubscribeAsync<UpdateEvent<TViewModel>> (HandleSave);
			App.Current.EventsBroker.SubscribeAsync<CreateEvent<TModel>> (HandleNew);
			App.Current.EventsBroker.SubscribeAsync<DeleteEvent<TModel>> (HandleDelete);
			if (ViewModel != null) {
				ViewModel.Selection.CollectionChanged += HandleSelectionChanged;
			}
		}

		public override async Task Stop ()
		{
			await base.Stop ();
			App.Current.EventsBroker.UnsubscribeAsync<ExportEvent<TModel>> (HandleExport);
			App.Current.EventsBroker.UnsubscribeAsync<ImportEvent<TModel>> (HandleImport);
			App.Current.EventsBroker.UnsubscribeAsync<UpdateEvent<TViewModel>> (HandleSave);
			App.Current.EventsBroker.UnsubscribeAsync<CreateEvent<TModel>> (HandleNew);
			App.Current.EventsBroker.UnsubscribeAsync<DeleteEvent<TModel>> (HandleDelete);
			if (ViewModel != null) {
				ViewModel.Selection.CollectionChanged -= HandleSelectionChanged;
			}
		}

		public override IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

		#endregion

		async Task HandleExport (ExportEvent<TModel> evt)
		{
			Project project = evt.Object;
			IProjectExporter exporter;

			if (project == null) {
				return;
			}

			try {
				exporter = App.Current.DependencyRegistry.Retrieve<IProjectExporter> (InstanceType.Default);
			} catch (Exception ex) {
				Log.Exception (ex);
				App.Current.Dialogs.ErrorMessage (string.Format (
					Catalog.GetString ("No project exporter found for format {0}"), evt.Format));
				return;
			}

			await exporter.Export (project);
		}

		Task HandleImport (ImportEvent<TModel> evt)
		{
			evt.ReturnValue = false;
			return AsyncHelpers.Return ();
		}

		Task HandleNew (CreateEvent<TModel> evt)
		{
			evt.ReturnValue = false;
			return AsyncHelpers.Return ();
		}

		async Task HandleDelete (DeleteEvent<TModel> evt)
		{
			TModel project = evt.Object;

			if (project == null) {
				return;
			}

			string msg = Catalog.GetString ("Do you really want to delete:") + "\n" + project.ShortDescription;
			if (await App.Current.Dialogs.QuestionMessage (msg, null)) {
				IBusyDialog busy = App.Current.Dialogs.BusyDialog (Catalog.GetString ("Deleting project..."), null);
				bool success = true;
				busy.ShowSync (() => {
					try {
						App.Current.DatabaseManager.ActiveDB.Delete<TModel> (project);
					} catch (StorageException ex) {
						success = false;
						App.Current.Dialogs.ErrorMessage (ex.Message);
					}
				});
				if (success) {
					ViewModel.Model.Remove (project);
					ViewModel.Select (ViewModel.Model.FirstOrDefault ());
					evt.ReturnValue = true;
				}
			}
		}

		async Task HandleSave (UpdateEvent<TViewModel> evt)
		{
			TViewModel project = evt.Object;
			if (project == null) {
				return;
			}
			evt.ReturnValue = await Save (project, evt.Force);
		}

		protected virtual async void HandleSelectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			TViewModel selectedVM = ViewModel.Selection.FirstOrDefault ();

			if (selectedVM != null) {
				if (ViewModel.LoadedProject.Model != null && ViewModel.LoadedProject.IsChanged) {
					await Save (ViewModel.LoadedProject, false);
				}

				// Load the model, creating a copy to edit changes in a different viewmodel in case the user
				// does not want to save them.
				ViewModel.LoadedProject = new TViewModel { Model = selectedVM.Model, Stateful = true };
				originalMediaFileSet = ViewModel.LoadedProject.FileSet.Model.Clone ();
				ViewModel.LoadedProject.IsChanged = false;
			}

			//Update commands
			ViewModel.ExportCommand.EmitCanExecuteChanged ();
			ViewModel.SaveCommand.EmitCanExecuteChanged ();
			ViewModel.OpenCommand.EmitCanExecuteChanged ();
			ViewModel.DeleteCommand.EmitCanExecuteChanged ();
		}

		protected async Task<bool> Save (TViewModel project, bool force)
		{
			if (!project.IsChanged) {
				return false;
			}
			if (!force) {
				string msg = Catalog.GetString ("Do you want to save the current project?");
				if (!(await App.Current.Dialogs.QuestionMessage (msg, null, this))) {
					ViewModel.LoadedProject.FileSet.Model.MediaFiles.Reset (originalMediaFileSet.MediaFiles);
					return false;
				}
			}
			try {
				IBusyDialog busy = App.Current.Dialogs.BusyDialog (Catalog.GetString ("Saving project..."), null);
				busy.ShowSync (() => {
					project.CommitState ();
					App.Current.DatabaseManager.ActiveDB.Store (project.Model);
					project.IsChanged = false;
					project.Model.IsChanged = false;
				});
				ViewModel.SaveCommand.EmitCanExecuteChanged ();
				return true;
			} catch (Exception ex) {
				Log.Exception (ex);
				App.Current.Dialogs.ErrorMessage (Catalog.GetString ("Error saving project:") + "\n" + ex.Message);
				return false;
			}
		}
	}
}
