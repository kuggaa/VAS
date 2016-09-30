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
using VAS.Core.Interfaces;
using VAS.Core.MVVMC;
using VAS.Core.Store.Playlists;

namespace VAS.Services.ViewModel
{
	/// <summary>
	/// ViewModel for a Playlist. Contains a Collection of PlaylistElements
	/// </summary>
	public class PlaylistVM : ViewModelBase<Playlist> // FIXME: Implement INested... from RA-138
	{
		public CollectionViewModel<IPlaylistElement, PlaylistElementVM> ViewModels {
			get;
			set;
		} = new CollectionViewModel<IPlaylistElement, PlaylistElementVM>();

		public override Playlist Model {
			get {
				return base.Model;
			}
			set {
				base.Model = value;
				if (value != null) {
					ViewModels.Model = Model.Elements;
				}
			}
		}

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

