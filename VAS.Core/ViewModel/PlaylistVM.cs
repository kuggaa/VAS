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
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.MVVMC;
using VAS.Core.Store.Playlists;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// ViewModel for a Playlist containing a collection of <see cref="PlaylistElementVM"/>.
	/// </summary>
	public class PlaylistVM : NestedSubViewModel<Playlist, PlaylistVM, IPlaylistElement, PlaylistElementVM>
	{
		/// <summary>
		/// Gets or sets the name of the playlist.
		/// </summary>
		/// <value>The name.</value>
		public string Name {
			get {
				return Model.Name;
			}
			set {
				Model.Name = value;
			}
		}

		/// <summary>
		/// Gets the creation date of the playlist.
		/// </summary>
		/// <value>The creation date.</value>
		public DateTime CreationDate {
			get {
				return Model.CreationDate;
			}
		}

		/// <summary>
		/// Gets or sets the last modification time of the playlist.
		/// </summary>
		/// <value>The last modified.</value>
		public DateTime LastModified {
			get {
				return Model.LastModified;
			}
			set {
				Model.LastModified = value;
			}
		}

		/// <summary>
		/// Gets the list of <see cref="IPlaylistElement"/> children.
		/// </summary>
		/// <value>The child models.</value>
		public override RangeObservableCollection<IPlaylistElement> ChildModels {
			get {
				return Model.Elements;
			}
		}
	}
}