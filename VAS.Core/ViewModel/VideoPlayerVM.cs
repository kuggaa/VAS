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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// Player View Model, Creates it's own instance of player controller.
	/// Every view that needs to control the player should use this ViewModel instead of the
	/// PlayerController.
	/// </summary>
	public class VideoPlayerVM : BindableBase, IViewModel
	{
		IVideoPlayerController playerController;
		PlayerViewOperationMode mode;
		MediaFileSet fileset;

		public VideoPlayerVM (IVideoPlayerController controller)
		{
			playerController = controller;
			playerController.SetViewModel (this);
			playerController.Start ();
		}

		public bool ControlsSensitive {
			get;
			set;
		}

		public bool ShowControls {
			get;
			set;
		}

		public bool Compact {
			get;
			set;
		}

		public bool ShowDrawingIcon {
			get;
			set;
		} = true;

		/// <summary>
		/// Gets or sets the current audio volume.
		/// This value is only used for display in the view. To change the volume use <see cref="SetVolume"/>
		/// </summary>
		/// <value>The new volume.</value>
		public double Volume {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the current playback rate.
		/// This value is only used for display in the view. To change the rate use <see cref="SetRate"/>
		/// </summary>
		/// <value>The new rate.</value>
		public double Rate {
			get;
			set;
		}

		public bool CloseButtonVisible {
			get;
			set;
		}

		public bool Playing {
			get;
			set;
		}

		public bool HasNext {
			get;
			set;
		}

		public Time CurrentTime {
			get;
			set;
		}

		public Time Duration {
			get;
			set;
		}

		public bool Seekable {
			get;
			set;
		}

		public MediaFileSetVM FileSet {
			get;
			protected set;
		}

		public FrameDrawing FrameDrawing {
			get;
			set;
		}

		[PropertyChanged.DoNotNotify]
		public Image CurrentFrame {
			get {
				return playerController.CurrentFrame;
			}
		}

		public object PlayElement {
			get;
			set;
		}

		public PlayerViewOperationMode ViewMode {
			set {
				mode = value;
				switch (mode) {
				case PlayerViewOperationMode.Analysis:
					Compact = false;
					ShowControls = true;
					ShowDrawingIcon = true;
					break;
				case PlayerViewOperationMode.LiveAnalysisReview:
					Compact = true;
					ShowControls = true;
					ShowDrawingIcon = true;
					break;
				case PlayerViewOperationMode.Synchronization:
					Compact = false;
					ShowControls = false;
					ShowDrawingIcon = false;
					break;
				case PlayerViewOperationMode.SimpleWithControls:
					Compact = false;
					ShowControls = true;
					ShowDrawingIcon = false;
					break;
				}
			}
			get {
				return mode;
			}
		}

		public VideoPlayerOperationMode PlayerMode {
			set {
				Player.Mode = value;
			}
			get {
				return Player.Mode;
			}
		}

		public bool SupportsMultipleCameras {
			get;
			set;
		}

		public bool ShowDetachButton {
			set;
			get;
		}

		public IVideoPlayerController Player {
			get {
				return playerController;
			}
			set {
				if (playerController != null) {
					playerController.Dispose ();
				}
				playerController = value;
			}
		}

		public bool PlayerAttached {
			set;
			get;
		}

		public bool IgnoreTicks {
			set {
				Player.IgnoreTicks = value;
			}
		}

		public Time Step {
			set {
				playerController.Step = value;
			}
		}

		public List<IViewPort> ViewPorts {
			set {
				playerController.ViewPorts = value;
			}
		}

		public bool PrepareView {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the cameras config. Use this Call to update CameraConfig in Views that uses
		/// this ViewModel
		/// </summary>
		/// <value>The cameras config.</value>
		public ObservableCollection<CameraConfig> CamerasConfig {
			get;
			set;
		}

		[PropertyChanged.DoNotNotify]
		public bool Opened {
			get {
				return playerController.Opened;
			}
		}

		#region methods

		public void Dispose ()
		{
			playerController.Dispose ();
		}

		public void Expose ()
		{
			playerController.Expose ();
		}

		public void Ready (bool ready)
		{
			playerController.Ready (ready);
		}

		public void Play ()
		{
			playerController.Play ();
		}

		public void Pause ()
		{
			playerController.Pause ();
		}

		public void Stop ()
		{
			((IPlayback)playerController).Stop ();
		}

		public void Seek (double pos)
		{
			playerController.Seek (pos);
		}

		public void Seek (Time time, bool accurate = false, bool synchronous = false, bool throttled = false)
		{
			playerController.Seek (time, accurate, synchronous, throttled);
		}

		public void Previous ()
		{
			playerController.Previous ();
		}

		public void Next ()
		{
			playerController.Next ();
		}

		public void TogglePlay ()
		{
			playerController.TogglePlay ();
		}

		public void SeekToPreviousFrame ()
		{
			playerController.SeekToPreviousFrame ();
		}

		public void SeekToNextFrame ()
		{
			playerController.SeekToNextFrame ();
		}

		public void StepBackward ()
		{
			playerController.StepBackward ();
		}

		public void StepForward ()
		{
			playerController.StepForward ();
		}

		public void OpenFileSet (MediaFileSetVM fileset, bool play = false)
		{
			FileSet = fileset;
			playerController.Open (fileset?.Model, play);
		}

		public void ApplyROI (CameraConfig cameraConfig)
		{
			playerController.ApplyROI (cameraConfig);
		}

		//FIXME: This setter is strange, but we need it to correctly set the CamerasConfig
		// to the PlayerController
		/// <summary>
		/// Sets the cameras config in PlayerController, use this call to set CameraConfig just in
		/// PlayerController
		/// </summary>
		/// <param name="cameras">Cameras.</param>
		public void SetCamerasConfig (ObservableCollection<CameraConfig> cameras)
		{
			playerController.CamerasConfig = cameras;
		}

		public void LoadEvent (TimelineEvent e, bool playing)
		{
			if (e?.Duration.MSeconds == 0) {
				// These events don't have duration, we start playing as if it was a seek
				Player.Switch (null, null, null);
				Player.UnloadCurrentEvent ();
				Player.Seek (e.EventTime, true);
				Player.Play ();
			} else {
				if (e != null) {
					LoadEvent (e, new Time (0), playing);
				} else if (Player != null) {
					Player.UnloadCurrentEvent ();
				}
			}
		}

		public void LoadEvent (TimelineEvent e, Time seekTime, bool playing)
		{
			e.Playing = true;
			Player.LoadEvent (e, seekTime, playing);
		}

		public void LoadEvents (List<TimelineEvent> events, bool playing)
		{
			Playlist playlist = new Playlist ();

			List<IPlaylistElement> list = events
				.Select (evt => new PlaylistPlayElement (evt))
				.OfType<IPlaylistElement> ()
				.ToList ();

			playlist.Elements = new RangeObservableCollection<IPlaylistElement> (list);
			playerController.LoadPlaylistEvent (playlist, list.FirstOrDefault (), playing);
		}

		public void LoadPlaylistEvent (Playlist playlist, IPlaylistElement element, bool playing)
		{
			Player?.LoadPlaylistEvent (playlist, element, playing);
		}

		/// <summary>
		/// Sends Event to draw in the Current Frame
		/// </summary>
		public void DrawFrame ()
		{
			App.Current.EventsBroker.Publish<DrawFrameEvent> (
				new DrawFrameEvent {
					Play = null,
					DrawingIndex = -1,
					CamConfig = CamerasConfig [0],
					Current = true
				}
			);
		}

		public void SetRate (double rate)
		{
			playerController.Rate = rate;
		}

		public void SetVolume (double volume)
		{
			playerController.Volume = volume;
		}
		#endregion
	}
}

