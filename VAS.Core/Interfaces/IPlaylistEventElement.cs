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
using System.Collections.ObjectModel;
using VAS.Core.Common;
using VAS.Core.Store;

namespace VAS.Core.Interfaces
{
	/// <summary>
	/// Interface for events that can be laoded in a video player backed up by a <see cref="TimelineEvent"/>
	/// </summary>
	public interface IPlaylistEventElement : IPlaylistElement
	{
		/// <summary>
		/// Gets the playback start time.
		/// </summary>
		/// <value>The playback start time.</value>
		Time Start {
			get;
		}

		/// <summary>
		/// Gets the stop time.
		/// </summary>
		/// <value>The playback stop time.</value>
		Time Stop {
			get;
		}

		/// <summary>
		/// Gets or sets the playback rate.
		/// </summary>
		/// <value>The playback rate.</value>
		float Rate {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the cameras layout.
		/// </summary>
		/// <value>The cameras layout.</value>
		object CamerasLayout {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the cameras config.
		/// </summary>
		/// <value>The cameras config.</value>
		ObservableCollection<CameraConfig> CamerasConfig {
			get;
			set;
		}

		/// <summary>
		/// Gets the drawings.
		/// </summary>
		/// <value>The drawings.</value>
		RangeObservableCollection<FrameDrawing> Drawings {
			get;
		}
	}
}
