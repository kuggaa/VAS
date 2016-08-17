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
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Events;

namespace VAS.Services.ViewModel
{
	public class ProjectsManagerVM<TModel, TViewModel> : CollectionViewModel<TModel, TViewModel>
		where TModel : Project
		where TViewModel : ProjectVM<TModel>, new()
	{
		public ProjectsManagerVM ()
		{
			LoadedProject = new TViewModel ();
		}

		[PropertyChanged.DoNotNotify]
		public TViewModel LoadedProject {
			get;
			private set;
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
		/// Command to export the currently loaded project.
		/// </summary>
		public void Export (string format = null)
		{
			TModel template = Selection.FirstOrDefault ()?.Model;
			if (template != null) {
				App.Current.EventsBroker.Publish (new ExportEvent<TModel> { Object = template, Format = format });
			}
		}

		/// <summary>
		/// Command to import a project.
		/// </summary>
		public void Import ()
		{
			App.Current.EventsBroker.Publish (new ImportEvent<TModel> ());
		}

		/// <summary>
		/// Command to create a new project.
		/// </summary>
		public void New ()
		{
			App.Current.EventsBroker.Publish (new CreateEvent<TModel> { });
		}

		/// <summary>
		/// Command to delete the selected projects.
		/// </summary>
		public void Delete ()
		{
			foreach (TModel project in Selection.Select (vm => vm.Model)) {
				App.Current.EventsBroker.Publish (new DeleteEvent<TModel> { Object = project });
			}
		}

		/// <summary>
		/// Command to save the currently loaded project.
		/// </summary>
		/// <param name="force">If set to <c>true</c> does not prompt to save.</param>
		public void Save (bool force)
		{
			TModel project = LoadedProject.Model;
			if (project != null) {
				App.Current.EventsBroker.Publish (new UpdateEvent<TModel> { Object = project, Force = force });
			}
		}
	}
}

