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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
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
	public abstract class TemplatesController<T, W>: IController
		where T:BindableBase, ITemplate<T>, new()  where W:TemplateViewModel<T>, new()
	{
		TemplatesManagerViewModel<T, W> viewModel;
		ITemplateProvider<T> provider;
		bool started;

		public TemplatesManagerViewModel<T, W> ViewModel {
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

		protected ITemplateProvider<T> Provider {
			get {
				return provider;
			}
			set {
				provider = value;
				provider.CollectionChanged += HandleProviderCollectionChanged;
			}
		}

		#region IController implementation

		public void SetViewModel (IViewModel viewModel)
		{
			ViewModel = (TemplatesManagerViewModel<T, W>)viewModel; 
		}

		public void Start ()
		{
			if (started) {
				throw new InvalidOperationException ("The controller is already running");
			}
			App.Current.EventsBroker.Subscribe<ExportEvent<T>> (HandleExport);
			App.Current.EventsBroker.Subscribe<ImportEvent<T>> (HandleImport);
			App.Current.EventsBroker.Subscribe<UpdateEvent<T>> (HandleSave);
			App.Current.EventsBroker.Subscribe<CreateEvent<T>> (HandleNew);
			App.Current.EventsBroker.Subscribe<DeleteEvent<T>> (HandleDelete);
			App.Current.EventsBroker.Subscribe<ChangeNameEvent<T>> (HandleChangeName);
		}

		public void Stop ()
		{
			if (!started) {
				throw new InvalidOperationException ("The controller is already stopped");
			}
			App.Current.EventsBroker.Unsubscribe<ExportEvent<T>> (HandleExport);
			App.Current.EventsBroker.Unsubscribe<ImportEvent<T>> (HandleImport);
			App.Current.EventsBroker.Unsubscribe<UpdateEvent<T>> (HandleSave);
			App.Current.EventsBroker.Unsubscribe<CreateEvent<T>> (HandleNew);
			App.Current.EventsBroker.Unsubscribe<DeleteEvent<T>> (HandleDelete);
			App.Current.EventsBroker.Unsubscribe<ChangeNameEvent<T>> (HandleChangeName);
			started = false;
		}

		#endregion

		async void HandleExport (ExportEvent<T> evt)
		{
			string fileName, filterName;
			string[] extensions;

			T template = evt.Object;
			Log.Debug ("Exporting " + TemplateName);
			filterName = FilterText;
			extensions = new [] { "*" + Extension };
			/* Show a file chooser dialog to select the file to export */
			fileName = App.Current.GUIToolkit.SaveFile (Catalog.GetString ("Export dashboard"),
				System.IO.Path.ChangeExtension (template.Name, Extension),
				App.Current.HomeDir, filterName, extensions);

			if (fileName != null) {
				bool succeeded = true;
				fileName = System.IO.Path.ChangeExtension (fileName, Extension);
				if (System.IO.File.Exists (fileName)) {
					string msg = AlreadyExistsText + " " + OverwriteText;
					succeeded = await App.Current.GUIToolkit.QuestionMessage (msg, null);
				}

				if (succeeded) {
					Serializer.Instance.Save (template, fileName);
					string msg = ExportedCorrectlyText;
					App.Current.GUIToolkit.InfoMessage (msg);
				}
			}
		}

		async void HandleImport (ImportEvent<T> evt)
		{
			string fileName, filterName;
			string[] extensions;

			T template = evt.Object;
			Log.Debug ("Importing dashboard");
			filterName = Catalog.GetString (FilterText);
			extensions = new [] { "*" + Extension };
			/* Show a file chooser dialog to select the file to import */
			fileName = App.Current.GUIToolkit.OpenFile (ImportText, null, App.Current.HomeDir,
				filterName, extensions);

			if (fileName == null)
				return;

			try {
				T newTemplate = Provider.LoadFile (fileName);

				if (newTemplate != null) {
					bool abort = false;

					while (Provider.Exists (newTemplate.Name) && !abort) {
						string name = await App.Current.GUIToolkit.QueryMessage (NameText,
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
				App.Current.GUIToolkit.ErrorMessage (Catalog.GetString ("Error importing file:") +
				"\n" + ex.Message);
				Log.Exception (ex);
				return;
			}
		}

		async void HandleNew (CreateEvent<T> evt)
		{
			T template, templateToDelete;

			if (ViewModel.LoadedTemplate.Edited) {
				HandleSave (new UpdateEvent<T> { Force = false, Object = ViewModel.LoadedTemplate.Model }); 
			}

			if (!await App.Current.GUIToolkit.CreateNewTemplate<T> (ViewModel.Model.ToList (),
				    NewText, CountText, Catalog.GetString ("The name is empty."), evt)) {
				return;
			} 

			templateToDelete = Provider.Templates.FirstOrDefault (t => t.Name == evt.Name);
			if (templateToDelete != null) {
				var msg = AlreadyExistsText + " " + OverwriteText;
				if (!await App.Current.GUIToolkit.QuestionMessage (msg, null, msg)) {
					return;
				}
			}

			if (evt.Source != null) {
				try {
					template = Provider.Copy (evt.Source, evt.Name);
				} catch (InvalidTemplateFilenameException ex) {
					App.Current.GUIToolkit.ErrorMessage (ex.Message, this);
					return;
				}
			} else {
				template = Provider.Create (evt.Name, evt.Count);
				if (!SaveTemplate (template)) {
					App.Current.GUIToolkit.ErrorMessage (ErrorSavingText);
					return;
				}
			}
			if (templateToDelete != null) {
				Provider.Delete (templateToDelete);
			}
			ViewModel.Select (template);
		}

		async void HandleDelete (DeleteEvent<T> evt)
		{
			T template = evt.Object;

			if (template != null) {
				string msg = ConfirmDeleteText + template.Name;
				if (await App.Current.GUIToolkit.QuestionMessage (msg, null)) {
					Provider.Delete (template);
					viewModel.Select (viewModel.Model.FirstOrDefault ());
				}
			}
		}

		async void HandleSave (UpdateEvent<T> evt)
		{
			T template = evt.Object;
			bool force = evt.Force;

			if (template == null) {
				return;
			}

			if (template.Static) {
				/* prompt=false when we click the save button */
				if (force) {
					SaveStatic (template);
				}
			} else {
				string msg = ConfirmSaveText;
				if (force || await App.Current.GUIToolkit.QuestionMessage (msg, null, this)) {
					SaveTemplate (template);
					// Update the ViewModel with the model clone used for editting.
					ViewModel.ViewModels.FirstOrDefault (vm => vm.Model.Equals (template)).Model = template;
					ViewModel.SaveSensitive = false;
				}
			}
		}

		async void HandleSelectionChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != "Selection") {
				return;
			}

			W selectedVM = ViewModel.Selection.FirstOrDefault ();
			T loadedTemplate = default(T);

			if (ViewModel.LoadedTemplate.Edited == true) {
				HandleSave (new UpdateEvent<T> { Force = false, Object = ViewModel.LoadedTemplate.Model }); 
			}

			if (selectedVM != null) {
				T template = selectedVM.Model;
				try {
					// Create a clone of the template and set it in the DashboardViewModel to edit
					// changes in a different model.
					loadedTemplate = template.Clone (SerializationType.Json);
					loadedTemplate.IsChanged = false;
				} catch (Exception ex) {
					Log.Exception (ex);
					App.Current.GUIToolkit.ErrorMessage (CouldNotLoadText);
					return;
				}
			}
			// Load the model
			ViewModel.LoadedTemplate.Model = loadedTemplate;
			// Update controls visiblity
			ViewModel.DeleteSensitive = loadedTemplate != null && ViewModel.LoadedTemplate.Editable;
			ViewModel.ExportSensitive = loadedTemplate != null;
			ViewModel.SaveSensitive = true;
		}

		void HandleChangeName (ChangeNameEvent<T> evt)
		{
			T template = evt.Object;
			string newName = evt.NewName;

			if (String.IsNullOrEmpty (newName)) {
				App.Current.GUIToolkit.ErrorMessage (Catalog.GetString ("The name is empty."));
				return;
			}
			if (template.Name == newName) {
				return;
			}
			if (Provider.Exists (newName)) {
				App.Current.GUIToolkit.ErrorMessage (AlreadyExistsText, this);
			} else {
				template.Name = newName;
				Provider.Save (template);
			}
		}

		void HandleTemplateChanged (object sender, PropertyChangedEventArgs e)
		{
			// FIXME: Objects do not emit yet PropertyChanged for all its children.
			ViewModel.SaveSensitive = true;
		}


		void HandleProviderCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				foreach (T template in e.NewItems)
					ViewModel.Model.Add (template);
				break;
			case NotifyCollectionChangedAction.Remove:
				foreach (T template in e.OldItems)
					ViewModel.Model.Remove (template);
				break;
			case NotifyCollectionChangedAction.Replace:
				break;
			}
		}

		async void SaveStatic (T template)
		{
			string msg = NotEditableText;
			if (await App.Current.GUIToolkit.QuestionMessage (msg, null, this)) {
				string newName;
				while (true) {
					newName = await App.Current.GUIToolkit.QueryMessage (Catalog.GetString ("Name:"), null,
						template.Name + "_copy", this);
					if (newName == null)
						break;
					if (Provider.Exists (newName)) {
						msg = AlreadyExistsText;
						App.Current.GUIToolkit.ErrorMessage (msg, this);
					} else {
						break;
					}
				}
				if (newName == null) {
					return;
				}
				T newtemplate = template.Copy (newName);
				newtemplate.Static = false;
				SaveTemplate (newtemplate);
			}
		}

		bool SaveTemplate (T template)
		{
			try {
				Provider.Save (template);
				return true;
			} catch (InvalidTemplateFilenameException ex) {
				App.Current.GUIToolkit.ErrorMessage (ex.Message, this);
				return false;
			}
		}
	}
}
