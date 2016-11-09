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
using System.Threading.Tasks;
using VAS.Core.Events;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Core.ViewModel
{
	public class ProjectVM<T> : ViewModelBase<T> where T : Project
	{

		/// <summary>
		/// Gets a value indicating whether the project has been edited.
		/// </summary>
		/// <value><c>true</c> if edited; otherwise, <c>false</c>.</value>
		public bool Edited {
			get {
				return Model?.IsChanged == true;
			}
		}

		/// <summary>
		/// Command to export a project.
		/// </summary>
		public Task<bool> Export ()
		{
			return App.Current.EventsBroker.PublishWithReturn (new ExportEvent<T> { Object = Model });
		}

		/// <summary>
		/// Command to delete a project.
		/// </summary>
		public Task<bool> Delete ()
		{
			return App.Current.EventsBroker.PublishWithReturn (new DeleteEvent<T> { Object = Model });
		}

		/// <summary>
		/// Command to save a project.
		/// </summary>
		/// <param name="force">If set to <c>true</c> does not prompt to save.</param>
		public Task<bool> Save (bool force)
		{
			return App.Current.EventsBroker.PublishWithReturn (new UpdateEvent<T> { Object = Model, Force = force });
		}
	}
}

