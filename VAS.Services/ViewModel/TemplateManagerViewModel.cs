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
using System.Linq;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.MVVMC;

namespace VAS.Services.ViewModel
{
	/// <summary>
	/// Generic base class ViewModel for a templates manager View, like the Dashboards Manager or the Teams Manager
	/// </summary>
	public class TemplatesManagerViewModel<TModel, TViewModel> : CollectionViewModel<TModel, TViewModel>
		where TModel : ITemplate<TModel>
		where TViewModel : TemplateViewModel<TModel>, new()
	{
		public TemplatesManagerViewModel ()
		{
			LoadedTemplate = new TViewModel ();
		}

		[PropertyChanged.DoNotNotify]
		public TViewModel LoadedTemplate {
			get;
			set;
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
		public void Export ()
		{
			TViewModel templateVM = Selection.FirstOrDefault ();
			if (templateVM != null && templateVM.Model != null) {
				App.Current.EventsBroker.Publish (new ExportEvent<TModel> { Object = templateVM.Model });
			}
		}

		/// <summary>
		/// Command to import a template.
		/// </summary>
		public void Import ()
		{
			App.Current.EventsBroker.Publish (new ImportEvent<TModel> ());
		}

		/// <summary>
		/// Command to create a new a template.
		/// </summary>
		public void New ()
		{
			App.Current.EventsBroker.Publish (new CreateEvent<TModel> { });
		}

		/// <summary>
		/// Command to delete the currently loaded template.
		/// </summary>
		public void Delete ()
		{
			TViewModel templateVM = Selection.FirstOrDefault ();
			if (templateVM != null && templateVM.Model != null) {
				App.Current.EventsBroker.Publish (new DeleteEvent<TModel> { Object = templateVM.Model });
			}
		}

		/// <summary>
		/// Command to save the currently loaded template.
		/// </summary>
		/// <param name="force">If set to <c>true</c> does not prompt to save.</param>
		public void Save (bool force)
		{
			TModel template = LoadedTemplate.Model;
			if (template != null) {
				App.Current.EventsBroker.Publish (
					new UpdateEvent<TModel> { Object = template, Force = force });
			}
		}

		/// <summary>
		/// Command to change the name of a template
		/// </summary>
		/// <param name="templateVM">The template ViewModel</param>
		/// <param name="newName">The new name.</param>
		public void ChangeName (TViewModel templateVM, string newName)
		{
			App.Current.EventsBroker.Publish (new ChangeNameEvent<TModel> {
				Object = templateVM.Model,
				NewName = newName
			});
		}
	}
}

