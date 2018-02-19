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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.MVVMC;
using VAS.Core;
using VAS.Core.ViewModel;
using VAS.Core.Store;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Filters;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// Generic base class ViewModel for a templates manager View, like the Dashboards Manager or the Teams Manager
	/// </summary>
	public class TemplatesManagerViewModel<TModel, TViewModel, TChildModel, TChildViewModel> : ManagerBaseVM<TModel, TViewModel>
		where TModel : StorableBase, ITemplate<TChildModel>
		where TViewModel : TemplateViewModel<TModel, TChildModel, TChildViewModel>, new()
		where TChildModel : BindableBase
		where TChildViewModel : IViewModel<TChildModel>, new()
	{
		public TemplatesManagerViewModel ()
		{
			LoadedTemplate = new TViewModel ();
			NewCommand = new AsyncCommand (New) { IconName = "vas-plus" };
			SaveCommand = new AsyncCommand<bool> (Save, () => LoadedTemplate.Model != null && LoadedTemplate.Edited);
			DeleteCommand = new AsyncCommand<TViewModel> (Delete, CanDelete);
			ExportCommand = new AsyncCommand (Export, () => LoadedTemplate.Model != null);
			ImportCommand = new AsyncCommand (Import);
			OpenCommand = new AsyncCommand<TModel> (Open);
			VisibleViewModels = new VisibleRangeObservableProxy<TViewModel> (ViewModels);
		}

		/// <summary>
 		/// Gets or sets the visible view models, viewmodels that has boolean Visible property setted to true.
 		/// </summary>
 		/// <value>The visible view models.</value>
 		public VisibleRangeObservableProxy<TViewModel> VisibleViewModels { get; protected set; }

		/// <summary>
		/// Gets or sets the View Model for the template loaded. This view model does not change, instead the model
		/// is updated so the View displaying the loaded ViewModel should only listen to the Model property changed.
		/// </summary>
		/// <value>The loaded template.</value>
		public TViewModel LoadedTemplate {
			get;
			protected set;
		}

		/// <summary>
		/// Command to create a new template.
		/// </summary>
		/// <value>The new command.</value>
		[PropertyChanged.DoNotNotify]
		public Command NewCommand {
			get;
			protected set;
		}

		/// <summary>
		/// Command to open a template.
		/// </summary>
		/// <value>The open command.</value>
		[PropertyChanged.DoNotNotify]
		public Command OpenCommand {
			get;
			protected set;
		}

		/// <summary>
		/// Command to delete a template.
		/// </summary>
		/// <value>The delete command.</value>
		[PropertyChanged.DoNotNotify]
		public Command<TViewModel> DeleteCommand {
			get;
			protected set;
		}

		/// <summary>
		/// Command to save a template.
		/// </summary>
		/// <value>The save command.</value>
		[PropertyChanged.DoNotNotify]
		public Command<bool> SaveCommand {
			get;
			protected set;
		}

		/// <summary>
		/// Command to export a template.
		/// </summary>
		/// <value>The export command.</value>
		[PropertyChanged.DoNotNotify]
		public Command ExportCommand {
			get;
			protected set;
		}

		/// <summary>
		/// Command to import a template.
		/// </summary>
		/// <value>The import command.</value>
		[PropertyChanged.DoNotNotify]
		public Command ImportCommand {
			get;
			protected set;
		}

		/// <summary>
		/// Command to change the name of a template
		/// </summary>
		/// <param name="templateVM">The template ViewModel</param>
		/// <param name="newName">The new name.</param>
		public Task<bool> ChangeName (TViewModel templateVM, string newName)
		{
			return App.Current.EventsBroker.PublishWithReturn (new ChangeNameEvent<TModel> {
				Object = templateVM.Model,
				NewName = newName
			});
		}

		/// <summary>
		/// Command to export the currently loaded template.
		/// </summary>
		protected virtual Task<bool> Export ()
		{
			TViewModel templateVM = Selection.FirstOrDefault ();
			if (templateVM != null && templateVM.Model != null) {
				return App.Current.EventsBroker.PublishWithReturn (new ExportEvent<TModel> { Object = templateVM.Model });
			}
			return AsyncHelpers.Return (false);
		}

		/// <summary>
		/// Command to import a template.
		/// </summary>
		protected virtual Task<bool> Import ()
		{
			return App.Current.EventsBroker.PublishWithReturn (new ImportEvent<TModel> ());
		}

		/// <summary>
		/// Command to create a new a template.
		/// </summary>
		protected virtual Task New ()
		{
			return App.Current.EventsBroker.Publish (new CreateEvent<TModel> ());
		}

		/// <summary>
		/// Send the event to open a template.
		/// </summary>
		protected virtual Task<bool> Open (TModel model)
		{
			return App.Current.EventsBroker.PublishWithReturn (new OpenEvent<TModel> () { Object = model });
		}

		/// <summary>
		/// Command to delete the currently loaded template.
		/// </summary>
		protected virtual async Task Delete (TViewModel viewModel)
		{
			if (viewModel != null) {
				ObservableCollection<TModel> objects = new ObservableCollection<TModel> ();
				objects.Add (viewModel.Model);
				await App.Current.EventsBroker.Publish (
						new DeleteEvent<ObservableCollection<TModel>> { Object = objects });
			} else if (Selection != null) {
				ObservableCollection<TModel> objects = new ObservableCollection<TModel>
					(Selection.Where (x => x.Model != null).Select (x => x.Model));
				if (objects.Any ()) {
					await App.Current.EventsBroker.Publish (
						new DeleteEvent<ObservableCollection<TModel>> { Object = objects });
				}
			}
		}

		/// <summary>
		/// Command to save the currently loaded template.
		/// </summary>
		/// <param name="force">If set to <c>true</c> does not prompt to save.</param>
		protected virtual Task<bool> Save (bool force)
		{
			TModel template = LoadedTemplate.Model;
			if (template != null) {
				return App.Current.EventsBroker.PublishWithReturn (
					new UpdateEvent<TModel> { Object = template, Force = force });
			}
			return AsyncHelpers.Return (false);
		}

		protected bool CanDelete (TViewModel viewModel) {
			if (viewModel != null) {
				return viewModel.Editable;
			}
			
			return LoadedTemplate.Model != null && LoadedTemplate.Editable;
		}
	}
}

