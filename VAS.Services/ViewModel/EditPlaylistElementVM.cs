//
//  Copyright (C) 2017 Fluendo S.A.
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
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Services.ViewModel
{
	/// <summary>
	/// ViewModel for edit playlist element view, it wrapps any PlaylistElementVM, can be the base class
	/// for images, etc. or PlaylistPlayElementVM, in that case the title should be visible to edit.
	/// </summary>
	public class EditPlaylistElementVM : ViewModelBase <PlaylistElementVM>
    {
		/// <summary>
		/// Gets a value indicating whether this <see cref="T:VAS.Services.ViewModel.EditPlaylistElementVM"/> title visible.
		/// </summary>
		/// <value><c>true</c> if title visible; otherwise, <c>false</c>.</value>
		public bool TitleVisible {
			get {
				return (Model is PlaylistPlayElementVM);
			}
		}

		/// <summary>
		/// Gets the duration.
		/// </summary>
		/// <value>The duration.</value>
		public Time Duration {
			get {
				return Model.Duration;
			}
		}

		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		/// <value>The title.</value>
		public string Title {
			get {
				return (Model as PlaylistPlayElementVM)?.Title;
			}
			set {
				(Model as PlaylistPlayElementVM).Title = value;
			}
		}
    }
}
