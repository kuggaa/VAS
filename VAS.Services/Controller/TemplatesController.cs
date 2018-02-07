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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Dynamic;
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
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services.Interfaces;
using VAS.Services.ViewModel;

namespace VAS.Services.Controller
{
	/// <summary>
	/// Base Controller for working with <see cref="ITemplate"/> like dashboards and teams.
	/// </summary>
	public abstract class TemplatesController<TModel, TViewModel, TChildModel, TChildViewModel> : ControllerBase
		where TModel : StorableBase, ITemplate<TChildModel>
		where TViewModel : TemplateViewModel<TModel, TChildModel, TChildViewModel>, new()
		where TChildModel : BindableBase
		where TChildViewModel : IViewModel<TChildModel>, new()
	{
		protected LimitationAsyncCommand<CreateEvent<TModel>> newTemplateCommand;
		protected LimitationAsyncCommand<ImportEvent<TModel>> importTemplateCommand;
		protected LimitationAsyncCommand<TModel> saveStaticTemplateCommand;

		TemplatesManagerViewModel<TModel, TViewModel, TChildModel, TChildViewModel> viewModel;
		ITemplateProvider<TModel> provider;

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			ViewModel = null;
		}

		public TemplatesManagerViewModel<TModel, TViewModel, TChildModel, TChildViewModel> ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.PropertyChanged -= HandleSelectionChanged;
					viewModel.LoadedTemplate.PropertyChanged -= HandleTemplateChanged;
				}
				viewModel = value;
				if (viewModel != null) {
					viewModel.PropertyChanged += HandleSelectionChanged;
					viewModel.LoadedTemplate.PropertyChanged += HandleTemplateChanged;
					if (viewModel.Selection.Count == 0) {
						viewModel.Select (viewModel.Model.FirstOrDefault ());
					}
				}
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

		protected string ConfirmDeleteListText { get; set; }

		protected string ConfirmDeleteChildText { get; set; }

		protected string ConfirmDeleteChildListText { get; set; }

		protected string CouldNotLoadText { get; set; }

		protected string NotEditableText { get; set; }

		protected string ConfirmSaveText { get; set; }

		protected string ImportText { get; set; }

		protected string NameText { get; set; }

		protected string TemplateName { get; set; }

		protected string Extension { get; set; }

		/// <summary>
		/// Gets or sets the name of the transition used in HandleOpen
		/// </summary>
		/// <value>The name of the open transition.</value>
		protected string OpenTransitionName { get; set; }

		protected ITemplateProvider<TModel> Provider {
			get {
				return provider;
			}
			set {
				provider = value;
			}
		}

		#region IController implementation

		public override void SetViewModel (IViewModel viewModel)
		{
			if (viewModel is ITeamCollectionDealer) {
				ViewModel = ((ITeamCollectionDealer)viewModel).Teams as dynamic;
			} else {
				ViewModel = (TemplatesManagerViewModel<TModel, TViewModel, TChildModel, TChildViewModel>)viewModel;
			}
		}

		public override async Task Start ()
		{
			await base.Start ();
			provider.CollectionChanged += HandleProviderCollectionChanged;
			App.Current.EventsBroker.SubscribeAsync<ExportEvent<TModel>> (HandleExport);
			App.Current.EventsBroker.SubscribeAsync<ImportEvent<TModel>> (async (ev) => await importTemplateCommand.ExecuteAsync (ev));
			App.Current.EventsBroker.SubscribeAsync<UpdateEvent<TModel>> (HandleSave);
			App.Current.EventsBroker.SubscribeAsync<OpenEvent<TModel>> (HandleOpen);
			App.Current.EventsBroker.SubscribeAsync<CreateEvent<TModel>> (async (ev) => await newTemplateCommand.ExecuteAsync (ev));
			App.Current.EventsBroker.SubscribeAsync<ChangeNameEvent<TModel>> (HandleChangeName);
			App.Current.EventsBroker.SubscribeAsync<DeleteEvent<ObservableCollection<TModel>>> (HandleDelete);
			App.Current.EventsBroker.Subscribe<SearchEvent> (HandleSearchEvent);
		}

		public override async Task Stop ()
		{
			await base.Stop ();
			provider.CollectionChanged -= HandleProviderCollectionChanged;
			App.Current.EventsBroker.UnsubscribeAsync<ExportEvent<TModel>> (HandleExport);
			App.Current.EventsBroker.UnsubscribeAsync<ImportEvent<TModel>> (async (ev) => await importTemplateCommand.ExecuteAsync (ev));
			App.Current.EventsBroker.UnsubscribeAsync<UpdateEvent<TModel>> (HandleSave);
			App.Current.EventsBroker.UnsubscribeAsync<OpenEvent<TModel>> (HandleOpen);
			App.Current.EventsBroker.UnsubscribeAsync<CreateEvent<TModel>> (async (ev) => await newTemplateCommand.ExecuteAsync (ev));
			App.Current.EventsBroker.UnsubscribeAsync<ChangeNameEvent<TModel>> (HandleChangeName);
			App.Current.EventsBroker.UnsubscribeAsync<DeleteEvent<ObservableCollection<TModel>>> (HandleDelete);
			App.Current.EventsBroker.Unsubscribe<SearchEvent> (HandleSearchEvent);
		}

		public override IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

		#endregion

		protected abstract bool SaveValidations (TModel model);

		/// <summary>
		/// Removes the selected children.
		/// </summary>
		/// <param name="vm">Vm.</param>
		protected virtual void RemoveChildsFromTemplate (TViewModel template, IEnumerable<TChildViewModel> childs)
		{
			foreach (var childVM in childs) {
				template.ViewModels.Remove (childVM);
			}
		}

		protected virtual void RemoveTemplates (IEnumerable<TModel> templates)
		{
			foreach (TModel template in templates.ToList ()) {
				Provider.Delete (template);
			}
		}

		/// <summary>
		/// Get the message to delete an item. If there is a single child selection, it uses the name of child
		/// instead of number.
		/// </summary>
		/// <returns>The question.</returns>
		/// <param name="templates">Templates to remove.</param>
		protected string GetDeleteChildrenQuestion (ObservableCollection<TModel> templates)
		{
			string msg = ConfirmDeleteChildListText;
			if (templates.Count () == 1) {
				var childSelection = ViewModel.Selection.First (x => x.Model.Equals (templates.First ())).Selection;
				if (childSelection.Count () == 1) {
					msg = string.Format (ConfirmDeleteChildText, childSelection.First ());
				}
			}
			return msg;
		}

		/// <summary>
		/// Check if it's a selection of child items or parents.
		/// </summary>
		/// <returns><c>true</c>, if it's a child selection, <c>false</c> otherwise.</returns>
		/// <param name="selectedViewModels">Selected view models.</param>
		protected bool IsChildSelection (IEnumerable<TViewModel> selectedViewModels)
		{
			return selectedViewModels.FirstOrDefault ()?.Selection.Any () ?? false;
		}

		#region Handle Events

		async Task HandleExport (ExportEvent<TModel> evt)
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
				fileName = System.IO.Path.ChangeExtension (fileName, Extension);
				if (App.Current.FileSystemManager.FileExists (fileName)) {
					string msg = AlreadyExistsText + " " + OverwriteText;
					evt.ReturnValue = await App.Current.Dialogs.QuestionMessage (msg, null);
					if (!evt.ReturnValue) {
						return;
					}
				}
				Serializer.Instance.Save (template, fileName);
				App.Current.Dialogs.InfoMessage (ExportedCorrectlyText);
			}
		}

		protected async Task Import (ImportEvent<TModel> evt)
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
						evt.ReturnValue = true;
					}
				}
			} catch (Exception ex) {
				App.Current.Dialogs.ErrorMessage (Catalog.GetString ("Error importing file:") +
				"\n" + ex.Message);
				Log.Exception (ex);
			}
		}

		async protected virtual Task HandleOpen (OpenEvent<TModel> evt)
		{
			dynamic properties = new ExpandoObject ();
			properties.Template = evt.Object.Clone ();
			properties.Templates = ViewModel;
			await App.Current.StateController.MoveToModal (OpenTransitionName, properties);
		}

		protected async virtual Task HandleNew (CreateEvent<TModel> evt)
		{
			TModel template, templateToDelete;

			if (ViewModel.LoadedTemplate.Edited) {
				await HandleSave (new UpdateEvent<TModel> { Force = false, Object = ViewModel.LoadedTemplate.Model });
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
			evt.ReturnValue = true;
		}

		async protected virtual Task HandleDelete (DeleteEvent<ObservableCollection<TModel>> evt)
		{
			ObservableCollection<TModel> templates = evt.Object;
			IEnumerable<Guid> ids = evt.Object.Select (x => x.ID).Intersect (ViewModel.Select (x => x.Model.ID));
			IEnumerable<TViewModel> selectedViewModels = ViewModel.Selection.Where (x => ids.Contains (x.Model.ID));

			if (templates != null && templates.Any ()) {
				if (!IsChildSelection (selectedViewModels)) {
					string msg = templates.Count () == 1 ?
							String.Format (ConfirmDeleteText, templates.FirstOrDefault ().Name) : ConfirmDeleteListText;
					if (await App.Current.Dialogs.QuestionMessage (msg, null)) {
						RemoveTemplates (templates);
						ViewModel.Select (ViewModel.Model.FirstOrDefault ());
						evt.ReturnValue = true;
					}
				} else {
					string msg = GetDeleteChildrenQuestion (templates);
					if (await App.Current.Dialogs.QuestionMessage (msg, null)) {
						foreach (var vm in selectedViewModels.ToList ()) {
							var updateEvent = new UpdateEvent<TModel> ();
							RemoveChildsFromTemplate (vm, vm.Selection);
							updateEvent.Object = vm.Model;
							updateEvent.Force = true;
							await HandleSave (updateEvent);
						}
						evt.ReturnValue = true;
					}
				}
			}
		}

		async protected Task HandleSave (UpdateEvent<TModel> evt)
		{
			TModel template = evt.Object;
			bool force = evt.Force;
			TViewModel templateViewmodel = ViewModel.ViewModels.FirstOrDefault (vm => vm.Model.Equals (template));

			if (template == null || !template.IsChanged || !SaveValidations (template)) {
				return;
			}

			if (template.Static) {
				string msg = NotEditableText;
				if (await App.Current.Dialogs.QuestionMessage (msg, null, this)) {
					await saveStaticTemplateCommand.ExecuteAsync (template);
				}
			} else {
				string msg = ConfirmSaveText;
				if (force || await App.Current.Dialogs.QuestionMessage (msg, null, this)) {
					evt.ReturnValue = SaveTemplate (template);
					if (evt.ReturnValue) {
						if (templateViewmodel == null) {
							// When is a new template, we should get the VM again, because previously was null
							templateViewmodel = ViewModel.ViewModels.FirstOrDefault (vm => vm.Model.Equals (template));
						} else {
							// Update the ViewModel with the model clone used for editing. If it was a new one isn't necessary
							templateViewmodel.Model = template;
						}
						ViewModel.SaveCommand.EmitCanExecuteChanged ();
					}
				}
			}
		}

		protected virtual async void HandleSelectionChanged (object sender, PropertyChangedEventArgs e)
		{
			if (!ViewModel.NeedsSync (e.PropertyName, "Collection_" + nameof (viewModel.Selection), sender, ViewModel)) {
				return;
			}

			TViewModel selectedVM = ViewModel.Selection.FirstOrDefault ();
			TModel loadedTemplate = null;

			if (ViewModel.LoadedTemplate.Edited) {
				await HandleSave (new UpdateEvent<TModel> { Force = false, Object = ViewModel.LoadedTemplate.Model });
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
			viewModel.PropertyChanged -= HandleSelectionChanged;
			ViewModel.LoadedTemplate.Model = loadedTemplate;
			viewModel.PropertyChanged += HandleSelectionChanged;

			// Update controls visiblity
			ViewModel.DeleteCommand.EmitCanExecuteChanged ();
			ViewModel.ExportCommand.EmitCanExecuteChanged ();
			ViewModel.SaveCommand.EmitCanExecuteChanged ();

			//Update commands
			ViewModel.DeleteCommand.EmitCanExecuteChanged ();
		}

		async Task HandleChangeName (ChangeNameEvent<TModel> evt)
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
				evt.ReturnValue = true;
			}
			await AsyncHelpers.Return ();
		}

		protected virtual void HandleTemplateChanged (object sender, PropertyChangedEventArgs e)
		{
			ViewModel.SaveCommand.EmitCanExecuteChanged ();
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

		protected virtual bool IsTemplateVisibleForSearchCriteria (TViewModel template, string search)
		{
			return template.Name.ToUpper ().Contains (search.ToUpper ());
		}

		protected virtual void HandleSearchEvent (SearchEvent searchEvent)
		{
			foreach (var template in ViewModel.ViewModels) {
				template.Visible = string.IsNullOrEmpty (searchEvent.TextFilter) ||
					IsTemplateVisibleForSearchCriteria (template, searchEvent.TextFilter);
			}
			ViewModel.VisibleViewModels.ApplyPropertyChanges ();
			ViewModel.NoResults = !ViewModel.VisibleViewModels.Any () &&
				!string.IsNullOrEmpty (searchEvent.TextFilter) &&
				!(ViewModel.ViewModels.Count == 0);
		}

		#endregion

		protected async Task<bool> HandleSaveStatic (TModel template)
		{
			bool saveOk = false;
			string newName;
			while (true) {
				newName = await App.Current.Dialogs.QueryMessage (Catalog.GetString ("Name:"), null,
					template.Name + "_copy", this);
				if (newName == null)
					break;
				if (Provider.Exists (newName)) {
					App.Current.Dialogs.ErrorMessage (AlreadyExistsText, this);
				} else {
					break;
				}
			}
			if (newName == null) {
				return false;
			}
			TModel newtemplate = (TModel)template.Copy (newName);
			newtemplate.Static = false;
			saveOk = SaveTemplate (newtemplate);

			return saveOk;
		}

		bool SaveTemplate (TModel template)
		{
			try {
				if (template != null) {
					template.Preview = App.Current.PreviewService.CreatePreview (template);
				}

				Provider.Save (template);
				return true;
			} catch (InvalidTemplateFilenameException ex) {
				App.Current.Dialogs.ErrorMessage (ex.Message, this);
				return false;
			}
		}
	}
}
