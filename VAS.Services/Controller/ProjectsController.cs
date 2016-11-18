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
using System.ComponentModel;
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
using VAS.Services.ViewModel;

namespace VAS.Services.Controller
{
	public class ProjectsController<TModel, TViewModel> : DisposableBase, IController
		where TModel : Project
		where TViewModel : ProjectVM<TModel>, new()
	{
		bool started;
		ProjectsManagerVM<TModel, TViewModel> viewModel;

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			Stop ();
		}

		protected ProjectsManagerVM<TModel, TViewModel> ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.Selection.CollectionChanged -= HandleSelectionChanged;
				}
				viewModel = value;
				if (started) {
					viewModel.Selection.CollectionChanged += HandleSelectionChanged;
				}
			}
		}

		#region IController implementation

		public virtual void SetViewModel (IViewModel viewModel)
		{
			ViewModel = (ProjectsManagerVM<TModel, TViewModel>)viewModel;
		}

		public virtual void Start ()
		{
			if (started) {
				throw new InvalidOperationException ("The controller is already running");
			}
			App.Current.EventsBroker.SubscribeAsync<ExportEvent<TModel>> (HandleExport);
			App.Current.EventsBroker.SubscribeAsync<ImportEvent<TModel>> (HandleImport);
			App.Current.EventsBroker.SubscribeAsync<UpdateEvent<TModel>> (HandleSave);
			App.Current.EventsBroker.SubscribeAsync<CreateEvent<TModel>> (HandleNew);
			App.Current.EventsBroker.SubscribeAsync<DeleteEvent<TModel>> (HandleDelete);
			if (viewModel != null) {
				viewModel.Selection.CollectionChanged += HandleSelectionChanged;
			}
			started = true;
		}

		public virtual void Stop ()
		{
			if (!started) {
				throw new InvalidOperationException ("The controller is already stopped");
			}
			App.Current.EventsBroker.UnsubscribeAsync<ExportEvent<TModel>> (HandleExport);
			App.Current.EventsBroker.UnsubscribeAsync<ImportEvent<TModel>> (HandleImport);
			App.Current.EventsBroker.UnsubscribeAsync<UpdateEvent<TModel>> (HandleSave);
			App.Current.EventsBroker.UnsubscribeAsync<CreateEvent<TModel>> (HandleNew);
			App.Current.EventsBroker.UnsubscribeAsync<DeleteEvent<TModel>> (HandleDelete);
			if (viewModel != null) {
				viewModel.Selection.CollectionChanged -= HandleSelectionChanged;
			}
			started = false;
		}

		public virtual IEnumerable<KeyAction> GetDefaultKeyActions ()
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
					viewModel.Select (viewModel.Model.FirstOrDefault ());
					evt.ReturnValue = true;
				}
			}
		}

		async Task HandleSave (UpdateEvent<TModel> evt)
		{
			TModel project = evt.Object;
			if (project == null) {
				return;
			}
			evt.ReturnValue = await Save (project, true);
		}

		async void HandleSelectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			TModel loadedProject = null;

			ProjectVM<TModel> projectVM = ViewModel.Selection.FirstOrDefault ();

			if (projectVM != null) {
				if (ViewModel.LoadedProject.Edited == true) {
					await Save (ViewModel.LoadedProject.Model, false);
				}

				// Load the model, creating a copy of the Project to edit changes in a different model in case the user
				// does not want to save them.
				TModel project = projectVM.Model;
				loadedProject = project.Clone (SerializationType.Json);
				project.IsChanged = false;
				ViewModel.LoadedProject.Model = loadedProject;

				// Update controls visiblity
				ViewModel.DeleteSensitive = loadedProject != null;
				ViewModel.ExportSensitive = loadedProject != null;
				ViewModel.SaveSensitive = false;
			}
		}

		async Task<bool> Save (TModel project, bool force)
		{
			if (!force && project.IsChanged) {
				string msg = Catalog.GetString ("Do you want to save the current project?");
				if (!(await App.Current.Dialogs.QuestionMessage (msg, null, this))) {
					return false;
				}
			}
			try {
				IBusyDialog busy = App.Current.Dialogs.BusyDialog (Catalog.GetString ("Saving project..."), null);
				busy.ShowSync (() => App.Current.DatabaseManager.ActiveDB.Store<TModel> (project));
				// Update the ViewModel with the model clone used for editting.
				ViewModel.ViewModels.FirstOrDefault (vm => vm.Model.Equals (project)).Model = project;
				ViewModel.SaveSensitive = false;
				return true;
			} catch (Exception ex) {
				Log.Exception (ex);
				App.Current.Dialogs.ErrorMessage (Catalog.GetString ("Error saving project:") + "\n" + ex.Message);
				return false;
			}
		}
	}
}
