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

namespace VAS.Services.ViewModel
{
	/// <summary>
	/// Generic base class ViewModel for a templates manager View, like the Dashboards Manager or the Teams Manager
	/// </summary>
	public class TemplatesManagerViewModel<TModel, TViewModel, TChildModel, TChildViewModel> : CollectionViewModel<TModel, TViewModel>
		where TModel : StorableBase, ITemplate<TChildModel>
		where TViewModel : TemplateViewModel<TModel, TChildModel, TChildViewModel>, new()
		where TChildModel : BindableBase
		where TChildViewModel : IViewModel<TChildModel>, new()
	{
		public TemplatesManagerViewModel ()
		{
			LoadedTemplate = new TViewModel ();
			NewCommand = new Command (New, () => true);
			DeleteCommand = new Command (Delete, () => Selection.Any ());
		}

		[PropertyChanged.DoNotNotify]
		public TViewModel LoadedTemplate {
			get;
			set;
		}

		[PropertyChanged.DoNotNotify]
		public Command NewCommand {
			get;
			protected set;
		}

		[PropertyChanged.DoNotNotify]
		public Command OpenCommand {
			get;
			protected set;
		}

		[PropertyChanged.DoNotNotify]
		public Command DeleteCommand {
			get;
			protected set;
		}

		/// <summary>
		/// Control whether the save button is clickable or not.
		/// </summary>
		/// <value><c>true</c> if save clickable; otherwise, <c>false</c>.</value>
		public bool SaveSensitive {
			get;
			set;
		}

		/// <summary>
		/// Control whether the delete button is clickable or not.
		/// </summary>
		/// <value><c>true</c> if delete clickable; otherwise, <c>false</c>.</value>
		public bool DeleteSensitive {
			get;
			set;
		}

		/// <summary>
		/// Control whether the export button is clickable or not.
		/// </summary>
		/// <value><c>true</c> if delete clickable; otherwise, <c>false</c>.</value>
		public bool ExportSensitive {
			get;
			set;
		}

		/// <summary>
		/// Command to export the currently loaded template.
		/// </summary>
		public Task<bool> Export ()
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
		public Task<bool> Import ()
		{
			return App.Current.EventsBroker.PublishWithReturn (new ImportEvent<TModel> ());
		}

		/// <summary>
		/// Command to create a new a template.
		/// </summary>
		protected virtual void New ()
		{
			App.Current.EventsBroker.Publish (new CreateEvent<TModel> ());
		}

		/// <summary>
		/// Send the event to open a template.
		/// </summary>
		public Task<bool> Open (TModel model)
		{
			return App.Current.EventsBroker.PublishWithReturn (new OpenEvent<TModel> () { Object = model });
		}

		/// <summary>
		/// Command to delete the currently loaded template.
		/// </summary>
		protected virtual void Delete ()
		{
			if (Selection != null) {
				ObservableCollection<TModel> objects = new ObservableCollection<TModel>
					(Selection.Where (x => x.Model != null).Select (x => x.Model));
				if (objects.Any ()) {
					App.Current.EventsBroker.Publish (
						new DeleteEvent<ObservableCollection<TModel>> { Object = objects });
				}
			}
		}

		/// <summary>
		/// Command to save the currently loaded template.
		/// </summary>
		/// <param name="force">If set to <c>true</c> does not prompt to save.</param>
		public Task<bool> Save (bool force)
		{
			TModel template = LoadedTemplate.Model;
			if (template != null) {
				return App.Current.EventsBroker.PublishWithReturn (
					new UpdateEvent<TModel> { Object = template, Force = force });
			}
			return AsyncHelpers.Return (false);
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
	}
}

