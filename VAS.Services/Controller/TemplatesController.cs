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
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Serialization;
using VAS.Services.ViewModel;

namespace VAS.Services.Controller
{

	/// <summary>
	/// Base Controller for working with <see cref="ITemplate"/> like dashboards and teams.
	/// </summary>
	public abstract class TemplatesController<TModel, TViewModel> : IController
		where TModel : BindableBase, ITemplate<TModel>, new()
		where TViewModel : TemplateViewModel<TModel>, new()
	{
		TemplatesManagerViewModel<TModel, TViewModel> viewModel;
		ITemplateProvider<TModel> provider;
		bool started;

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		~TemplatesController ()
		{
			Dispose (false);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (Disposed)
				return;

			Disposed = true;
		}

		public TemplatesManagerViewModel<TModel, TViewModel> ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					throw new InvalidOperationException ("The ViewModel is already set");
				}
				viewModel = value;
				viewModel.PropertyChanged += HandleSelectionChanged;
				viewModel.LoadedTemplate.PropertyChanged += HandleTemplateChanged;
				viewModel.Select (viewModel.Model.FirstOrDefault ());
			}
		}

		protected bool Disposed { get; private set; } = false;

		protected string FilterText { get; set; }

		protected string AlreadyExistsText { get; set; }

		protected string ExportedCorrectlyText { get; set; }

		protected string CountText { get; set; }

		protected string NewText { get; set; }

		protected string OverwriteText { get; set; }

		protected string ErrorSavingText { get; set; }

		protected string ConfirmDeleteText { get; set; }

		protected string CouldNotLoadText { get; set; }

		protected string NotEditableText { get; set; }

		protected string ConfirmSaveText { get; set; }

		protected string ImportText { get; set; }

		protected string NameText { get; set; }

		protected string TemplateName { get; set; }

		protected string Extension { get; set; }

		protected ITemplateProvider<TModel> Provider {
			get {
				return provider;
			}
			set {
				provider = value;
				provider.CollectionChanged += HandleProviderCollectionChanged;
			}
		}

		#region IController implementation

		public virtual void SetViewModel (IViewModel viewModel)
		{
			ViewModel = (TemplatesManagerViewModel<TModel, TViewModel>)viewModel;
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
			App.Current.EventsBroker.Subscribe<ChangeNameEvent<TModel>> (HandleChangeName);
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
			App.Current.EventsBroker.Unsubscribe<ChangeNameEvent<TModel>> (HandleChangeName);
			started = false;
		}

		public IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

		#endregion

		async void HandleExport (ExportEvent<TModel> evt)
		{
			string fileName, filterName;
			string [] extensions;

			TModel template = evt.Object;
			Log.Debug ("Exporting " + TemplateName);
			filterName = FilterText;
			extensions = new [] { "*" + Extension };
			/* Show a file chooser dialog to select the file to export */
			fileName = App.Current.Dialogs.SaveFile (Catalog.GetString ("Export dashboard"),
				System.IO.Path.ChangeExtension (template.Name, Extension),
				App.Current.HomeDir, filterName, extensions);

			if (fileName != null) {
				bool succeeded = true;
				fileName = System.IO.Path.ChangeExtension (fileName, Extension);
				if (System.IO.File.Exists (fileName)) {
					string msg = AlreadyExistsText + " " + OverwriteText;
					succeeded = await App.Current.Dialogs.QuestionMessage (msg, null);
				}

				if (succeeded) {
					Serializer.Instance.Save (template, fileName);
					string msg = ExportedCorrectlyText;
					App.Current.Dialogs.InfoMessage (msg);
				}
			}
		}

		async void HandleImport (ImportEvent<TModel> evt)
		{
			string fileName, filterName;
			string [] extensions;

			TModel template = evt.Object;
			Log.Debug ("Importing dashboard");
			filterName = Catalog.GetString (FilterText);
			extensions = new [] { "*" + Extension };
			/* Show a file chooser dialog to select the file to import */
			fileName = App.Current.Dialogs.OpenFile (ImportText, null, App.Current.HomeDir,
				filterName, extensions);

			if (fileName == null)
				return;

			try {
				TModel newTemplate = Provider.LoadFile (fileName);

				if (newTemplate != null) {
					bool abort = false;

					while (Provider.Exists (newTemplate.Name) && !abort) {
						string name = await App.Current.Dialogs.QueryMessage (NameText,
										  Catalog.GetString ("Name conflict"), newTemplate.Name + "#");
						if (name == null) {
							abort = true;
						} else {
							newTemplate.Name = name;
						}
					}

					if (!abort) {
						Provider.Save (newTemplate);
						ViewModel.Select (newTemplate);
					}
				}
			} catch (Exception ex) {
				App.Current.Dialogs.ErrorMessage (Catalog.GetString ("Error importing file:") +
				"\n" + ex.Message);
				Log.Exception (ex);
				return;
			}
		}

		protected async virtual void HandleNew (CreateEvent<TModel> evt)
		{
			TModel template, templateToDelete;

			if (ViewModel.LoadedTemplate.Edited) {
				HandleSave (new UpdateEvent<TModel> { Force = false, Object = ViewModel.LoadedTemplate.Model });
			}

			if (!await App.Current.GUIToolkit.CreateNewTemplate<TModel> (ViewModel.Model.ToList (),
					NewText, CountText, Catalog.GetString ("The name is empty."), evt)) {
				return;
			}

			templateToDelete = Provider.Templates.FirstOrDefault (t => t.Name == evt.Name);
			if (templateToDelete != null) {
				var msg = AlreadyExistsText + " " + OverwriteText;
				if (!await App.Current.Dialogs.QuestionMessage (msg, null, msg)) {
					return;
				}
			}

			if (evt.Source != null) {
				try {
					template = Provider.Copy (evt.Source, evt.Name);
				} catch (InvalidTemplateFilenameException ex) {
					App.Current.Dialogs.ErrorMessage (ex.Message, this);
					return;
				}
			} else {
				template = Provider.Create (evt.Name, evt.Count);
				if (!SaveTemplate (template)) {
					App.Current.Dialogs.ErrorMessage (ErrorSavingText);
					return;
				}
			}
			if (templateToDelete != null) {
				Provider.Delete (templateToDelete);
			}
			ViewModel.Select (template);
		}

		async void HandleDelete (DeleteEvent<TModel> evt)
		{
			TModel template = evt.Object;

			if (template != null) {
				string msg = ConfirmDeleteText + template.Name;
				if (await App.Current.Dialogs.QuestionMessage (msg, null)) {
					Provider.Delete (template);
					viewModel.Select (viewModel.Model.FirstOrDefault ());
				}
			}
		}

		async void HandleSave (UpdateEvent<TModel> evt)
		{
			TModel template = evt.Object;
			bool force = evt.Force;

			if (template == null || !template.IsChanged) {
				return;
			}

			if (template.Static) {
				/* prompt=false when we click the save button */
				if (force) {
					SaveStatic (template);
				}
			} else {
				string msg = ConfirmSaveText;
				if (force || await App.Current.Dialogs.QuestionMessage (msg, null, this)) {
					SaveTemplate (template);
					// Update the ViewModel with the model clone used for editting.
					ViewModel.ViewModels.FirstOrDefault (vm => vm.Model.Equals (template)).Model = template;
					ViewModel.SaveSensitive = false;
				}
			}
		}

		void HandleSelectionChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != "Selection") {
				return;
			}

			TViewModel selectedVM = ViewModel.Selection.FirstOrDefault ();
			TModel loadedTemplate = default (TModel);

			if (ViewModel.LoadedTemplate.Edited == true) {
				HandleSave (new UpdateEvent<TModel> { Force = false, Object = ViewModel.LoadedTemplate.Model });
			}

			if (selectedVM != null) {
				TModel template = selectedVM.Model;
				try {
					// Create a clone of the template and set it in the DashboardViewModel to edit
					// changes in a different model.
					loadedTemplate = template.Clone (SerializationType.Json);
					loadedTemplate.IsChanged = false;
					loadedTemplate.Static = template.Static;
				} catch (Exception ex) {
					Log.Exception (ex);
					App.Current.Dialogs.ErrorMessage (CouldNotLoadText);
					return;
				}
			}
			// Load the model
			ViewModel.LoadedTemplate.Model = loadedTemplate;
			// Update controls visiblity
			ViewModel.DeleteSensitive = loadedTemplate != null && ViewModel.LoadedTemplate.Editable;
			ViewModel.ExportSensitive = loadedTemplate != null;
			ViewModel.SaveSensitive = false;
		}

		void HandleChangeName (ChangeNameEvent<TModel> evt)
		{
			TModel template = evt.Object;
			string newName = evt.NewName;

			if (String.IsNullOrEmpty (newName)) {
				App.Current.Dialogs.ErrorMessage (Catalog.GetString ("The name is empty."));
				return;
			}
			if (template.Name == newName) {
				return;
			}
			if (Provider.Exists (newName)) {
				App.Current.Dialogs.ErrorMessage (AlreadyExistsText, this);
			} else {
				template.Name = newName;
				Provider.Save (template);
			}
		}

		void HandleTemplateChanged (object sender, PropertyChangedEventArgs e)
		{
			ViewModel.SaveSensitive = true;
		}


		void HandleProviderCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				foreach (TModel template in e.NewItems)
					ViewModel.Model.Add (template);
				break;
			case NotifyCollectionChangedAction.Remove:
				foreach (TModel template in e.OldItems)
					ViewModel.Model.Remove (template);
				break;
			case NotifyCollectionChangedAction.Replace:
				break;
			}
		}

		async void SaveStatic (TModel template)
		{
			string msg = NotEditableText;
			if (await App.Current.Dialogs.QuestionMessage (msg, null, this)) {
				string newName;
				while (true) {
					newName = await App.Current.Dialogs.QueryMessage (Catalog.GetString ("Name:"), null,
						template.Name + "_copy", this);
					if (newName == null)
						break;
					if (Provider.Exists (newName)) {
						msg = AlreadyExistsText;
						App.Current.Dialogs.ErrorMessage (msg, this);
					} else {
						break;
					}
				}
				if (newName == null) {
					return;
				}
				TModel newtemplate = template.Copy (newName);
				newtemplate.Static = false;
				SaveTemplate (newtemplate);
			}
		}

		bool SaveTemplate (TModel template)
		{
			try {
				Provider.Save (template);
				return true;
			} catch (InvalidTemplateFilenameException ex) {
				App.Current.Dialogs.ErrorMessage (ex.Message, this);
				return false;
			}
		}
	}
}
