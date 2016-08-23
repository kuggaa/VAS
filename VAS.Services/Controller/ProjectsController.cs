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
using VAS.Core.Store;
using VAS.Services.ViewModel;

namespace VAS.Services.Controller
{
	public class ProjectsController<TModel, TViewModel> : IController
		where TModel : Project
		where TViewModel : ProjectVM<TModel>, new()
	{
		bool started;
		ProjectsManagerVM<TModel, TViewModel> viewModel;

		ProjectsManagerVM<TModel, TViewModel> ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
				viewModel.PropertyChanged += HandleSelectionChanged;
				viewModel.Select (viewModel.Model.FirstOrDefault ());
			}
		}

		#region IController implementation

		public void SetViewModel (IViewModel viewModel)
		{
			ViewModel = (ProjectsManagerVM<TModel, TViewModel>)viewModel;
		}

		public virtual void Start ()
		{
			if (started) {
				throw new InvalidOperationException ("The controller is already running");
			}
			App.Current.EventsBroker.Subscribe<ExportEvent<TModel>> (HandleExport);
			App.Current.EventsBroker.Subscribe<ImportEvent<TModel>> (HandleImport);
			App.Current.EventsBroker.Subscribe<UpdateEvent<TModel>> (HandleSave);
			App.Current.EventsBroker.Subscribe<CreateEvent<TModel>> (HandleNew);
			App.Current.EventsBroker.Subscribe<DeleteEvent<TModel>> (HandleDelete);
			started = true;
		}

		public virtual void Stop ()
		{
			if (!started) {
				throw new InvalidOperationException ("The controller is already stopped");
			}
			App.Current.EventsBroker.Unsubscribe<ExportEvent<TModel>> (HandleExport);
			App.Current.EventsBroker.Unsubscribe<ImportEvent<TModel>> (HandleImport);
			App.Current.EventsBroker.Unsubscribe<UpdateEvent<TModel>> (HandleSave);
			App.Current.EventsBroker.Unsubscribe<CreateEvent<TModel>> (HandleNew);
			App.Current.EventsBroker.Unsubscribe<DeleteEvent<TModel>> (HandleDelete);
			started = false;
		}

		public virtual IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

		#endregion

		async void HandleExport (ExportEvent<TModel> evt)
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

		void HandleImport (ImportEvent<TModel> evt)
		{
		}

		void HandleNew (CreateEvent<TModel> evt)
		{
		}

		async void HandleDelete (DeleteEvent<TModel> evt)
		{
			TModel project = evt.Object;

			if (project == null) {
				return;
			}

			string msg = Catalog.GetString ("Do you really want to delete:") + "\n" + project.ShortDescription;
			if (await App.Current.Dialogs.QuestionMessage (msg, null)) {
				IBusyDialog busy = App.Current.Dialogs.BusyDialog (Catalog.GetString ("Deleting project..."), null);
				busy.ShowSync (() => {
					try {
						App.Current.DatabaseManager.ActiveDB.Delete<TModel> (project);
						ViewModel.Model.Remove (project);
						viewModel.Select (viewModel.Model.FirstOrDefault ());
					} catch (StorageException ex) {
						App.Current.Dialogs.ErrorMessage (ex.Message);
					}
				});
			}
		}

		async void HandleSave (UpdateEvent<TModel> evt)
		{
			TModel project = evt.Object;
			if (project == null) {
				return;
			}
			await Save (project, true);
		}

		async void HandleSelectionChanged (object sender, PropertyChangedEventArgs e)
		{
			TModel loadedProject = null;

			if (e.PropertyName != "Selection") {
				return;
			}

			ProjectVM<TModel> projectVM = ViewModel.Selection.FirstOrDefault ();

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

		async Task Save (TModel project, bool force)
		{
			if (!force && project.IsChanged) {
				string msg = Catalog.GetString ("Do you want to save the current project?");
				if (!(await App.Current.Dialogs.QuestionMessage (msg, null, this))) {
					return;
				}
			}
			try {
				IBusyDialog busy = App.Current.Dialogs.BusyDialog (Catalog.GetString ("Saving project..."), null);
				busy.ShowSync (() => App.Current.DatabaseManager.ActiveDB.Store<TModel> (project));
				// Update the ViewModel with the model clone used for editting.
				ViewModel.ViewModels.FirstOrDefault (vm => vm.Model.Equals (project)).Model = project;
				ViewModel.SaveSensitive = false;
			} catch (Exception ex) {
				Log.Exception (ex);
				App.Current.Dialogs.ErrorMessage (Catalog.GetString ("Error saving project:") + "\n" + ex.Message);
				return;
			}
		}
	}
}
