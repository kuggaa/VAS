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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using VAS.Core.Common;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;

namespace VAS.Core.Interfaces.MVVMC
{

	/// <summary>
	/// Interface that the player View Model should use
	/// </summary>
	public interface IVideoPlayerViewModel : IViewModel
	{
		bool ControlsSensitive {
			get;
			set;
		}

		bool ShowControls {
			get;
			set;
		}

		bool Compact {
			get;
			set;
		}

		bool ShowDrawingIcon {
			get;
			set;
		}

		double Volume {
			get;
			set;
		}

		float Rate {
			get;
			set;
		}

		bool CloseButtonVisible {
			get;
			set;
		}

		bool Playing {
			get;
			set;
		}

		bool HasNext {
			get;
			set;
		}

		Time CurrentTime {
			get;
			set;
		}

		Time Duration {
			get;
			set;
		}

		bool Seekable {
			get;
			set;
		}

		MediaFileSetVM FileSet {
			get;
		}

		FrameDrawing FrameDrawing {
			get;
			set;
		}

		Image CurrentFrame {
			get;
		}

		object PlayElement {
			get;
			set;
		}

		PlayerViewOperationMode Mode {
			set;
			get;
		}

		bool SupportsMultipleCameras {
			get;
			set;
		}

		bool ShowDetachButton {
			set;
			get;
		}

		IVideoPlayerController Player {
			get;
			set;
		}

		bool PlayerAttached {
			set;
			get;
		}

		bool IgnoreTicks {
			set;
		}

		Time Step {
			set;
		}

		List<IViewPort> ViewPorts {
			set;
		}

		bool PrepareView {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the cameras config. Use this Call to update CameraConfig in Views that uses
		/// this ViewModel
		/// </summary>
		/// <value>The cameras config.</value>
		ObservableCollection<CameraConfig> CamerasConfig {
			get;
			set;
		}

		bool Opened {
			get;
		}

		bool PresentationMode {
			set;
		}

		void Expose ();

		void Ready (bool ready);

		void Play ();

		void Pause ();

		void Stop ();

		void Seek (double pos);

		void Seek (Time time, bool accurate = false, bool synchronous = false, bool throttled = false);

		void Previous ();

		void Next ();

		void TogglePlay ();

		void SeekToPreviousFrame ();

		void SeekToNextFrame ();

		void StepBackward ();

		void StepForward ();

		void OpenFileSet (MediaFileSetVM fileset, bool play = false);

		void ApplyROI (CameraConfig cameraConfig);

		/// <summary>
		/// Sets the cameras config in PlayerController, use this call to set CameraConfig just in
		/// PlayerController
		/// </summary>
		/// <param name="cameras">Cameras.</param>
		void SetCamerasConfig (ObservableCollection<CameraConfig> cameras);

		void LoadEvent (TimelineEvent e, bool playing);

		void LoadEvents (List<TimelineEvent> events, bool playing);

		void LoadPlay (TimelineEvent e, Time seekTime, bool playing);

		void LoadPlaylistEvent (Playlist playlist, IPlaylistElement element, bool playing);

		/// <summary>
		/// Sends Event to draw in the Current Frame
		/// </summary>
		void DrawFrame ();
	}
}
