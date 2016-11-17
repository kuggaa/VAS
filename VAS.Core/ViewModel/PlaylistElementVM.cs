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

using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// ViewModel for PlaylistElements, with an IPlaylistElement as Model.
	/// </summary>
	public class PlaylistElementVM : ViewModelBase<IPlaylistElement>
	{
		/// <summary>
		/// Gets the description of the playlist element
		/// </summary>
		/// <value>The description.</value>
		public string Description {
			get {
				return Model.Description;
			}
		}

		/// <summary>
		/// Gets a miniature image for the playlist element.
		/// </summary>
		/// <value>The miniature.</value>
		public Image Miniature {
			get {
				return Model.Miniature;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.PlaylistElementVM"/> is selected.
		/// </summary>
		/// <value><c>true</c> if selected; otherwise, <c>false</c>.</value>
		public bool Selected {
			get;
			set;
		}

		/// <summary>
		/// Gets the duration of the playlist element.
		/// </summary>
		/// <value>The duration.</value>
		public Time Duration {
			get {
				return Model.Duration;
			}
		}
	}
}
