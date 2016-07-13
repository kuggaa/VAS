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
	public class TemplatesManagerViewModel<T, W>: CollectionViewModel<T, W> where T: ITemplate<T> where W:TemplateViewModel<T>, new()
	{
		public TemplatesManagerViewModel ()
		{
			LoadedTemplate = new W ();
		}

		[PropertyChanged.DoNotNotify]
		public W LoadedTemplate {
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
			T template = Selection.FirstOrDefault ()?.Model;
			if (template != null) {
				App.Current.EventsBroker.Publish (new ExportEvent<T> { Object = template });
			}
		}

		/// <summary>
		/// Command to import a template.
		/// </summary>
		public void Import ()
		{
			App.Current.EventsBroker.Publish (new ImportEvent<T> ());
		}

		/// <summary>
		/// Command to create a new a template.
		/// </summary>
		public void New ()
		{
			App.Current.EventsBroker.Publish (new CreateEvent<T> { });
		}

		/// <summary>
		/// Command to delete the currently loaded template.
		/// </summary>
		public void Delete ()
		{
			T template = Selection.FirstOrDefault ()?.Model;
			if (template != null) {
				App.Current.EventsBroker.Publish (new DeleteEvent<T> { Object = template });
			}
		}

		/// <summary>
		/// Command to save the currently loaded template.
		/// </summary>
		/// <param name="force">If set to <c>true</c> does not prompt to save.</param>
		public void Save (bool force)
		{
			T template = LoadedTemplate.Model;
			if (template != null) {
				App.Current.EventsBroker.Publish (
					new UpdateEvent<T> { Object = template, Force = force });
			}
		}

		/// <summary>
		/// Command to change the name of a template
		/// </summary>
		/// <param name="templateVM">The template ViewModel</param>
		/// <param name="newName">The new name.</param>
		public void ChangeName (W templateVM, string newName)
		{
			App.Current.EventsBroker.Publish (new ChangeNameEvent<T> {
				Object = templateVM.Model,
				NewName = newName
			});
		}
	}
}

