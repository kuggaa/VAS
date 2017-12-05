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

using System.Collections.ObjectModel;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// ViewModel for PlaylistElements, with an IPlaylistElement as Model.
	/// </summary>
	public class PlaylistElementVM : PlayableElementVM<IPlaylistElement>
	{
		/// <summary>
		/// Gets the description of the playlist element
		/// </summary>
		/// <value>The description.</value>
		public string Description
		{
			get
			{
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
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.PlaylistElementVM"/> is playing.
		/// </summary>
		/// <value><c>true</c> if playing; otherwise, <c>false</c>.</value>
		public bool Playing {
			get {
				return Model.Playing;
			}
			set {
				Model.Playing = value;
			}
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

		public ObservableCollection<CameraConfig> CamerasConfig => Model.CamerasConfig;

		public object CamerasLayout => Model.CamerasLayout;
	}

	public class PlaylistPlayElementVM : PlaylistElementVM
	{
		public PlaylistPlayElementVM()
		{
			Play = new TimelineEventVM();
		}

		/// <summary>
		/// Gets or sets the model.
		/// </summary>
		/// <value>The model.</value>
		public PlaylistPlayElement TypedModel
		{
			get
			{
				return (PlaylistPlayElement)base.Model;
			}
		}

		[PropertyChanged.DoNotCheckEquality]
		public override IPlaylistElement Model
		{
			get
			{
				return TypedModel;
			}
			set
			{
				base.Model = value;
				Play.Model = ((PlaylistPlayElement)value).Play;
			}
		}



		public string Title
		{
			get
			{
				return TypedModel.Title;
			}
			set
			{
				TypedModel.Title = value;
			}
		}

		public TimelineEventVM Play
		{
			get;
			set;
		}

	}

	public class PlaylistVideoVM : PlaylistElementVM
	{
	}

	public class PlaylistImageVM : PlaylistElementVM
	{
	}

	public class PlaylistDrawingVM : PlaylistElementVM
	{
	}
}
