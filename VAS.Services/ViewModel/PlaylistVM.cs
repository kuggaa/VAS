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
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store.Playlists;
using VAS.Core.Common;

namespace VAS.Services.ViewModel
{
	/// <summary>
	/// ViewModel for a Playlist. Contains a Collection of PlaylistElements
	/// </summary>
	public class PlaylistVM : ViewModelBase<Playlist>, INestedViewModel<PlaylistElementVM>
	{
		#region INestedViewModel implementation

		public IEnumerator<PlaylistElementVM> GetEnumerator ()
		{
			return SubViewModel.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return SubViewModel.GetEnumerator ();
		}

		public INotifyCollectionChanged GetNotifyCollection ()
		{
			return SubViewModel.GetNotifyCollection ();
		}

		public RangeObservableCollection<PlaylistElementVM> ViewModels {
			get {
				return SubViewModel.ViewModels;
			}
		}

		public RangeObservableCollection<PlaylistElementVM> Selection {
			get;
			private set;
		}

		public void SelectionReplace (IEnumerable<PlaylistElementVM> selection)
		{
			Selection.Replace (selection);
		}

		#endregion

		public override Playlist Model {
			get {
				return base.Model;
			}
			set {
				base.Model = value;
				if (value != null) {
					SubViewModel.Model = Model.Elements;
				}
			}
		}

		public CollectionViewModel<IPlaylistElement, PlaylistElementVM> SubViewModel {
			get;
			set;
		} = new CollectionViewModel<IPlaylistElement, PlaylistElementVM> ();

		public string Name {
			get {
				return Model.Name;
			}
			set {
				Model.Name = value;
			}
		}

		public DateTime CreationDate {
			get {
				return Model.CreationDate;
			}
		}

		public DateTime LastModified {
			get {
				return Model.LastModified;
			}
			set {
				Model.LastModified = value;
			}
		}
	}
}

