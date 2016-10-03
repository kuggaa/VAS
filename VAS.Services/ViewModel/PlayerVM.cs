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
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core;

namespace VAS.Services.ViewModel
{
	public class PlayerVM : BindableBase, IPlayerViewModel
	{
		IPlayerController playerController;
		PlayerViewOperationMode mode;
		MediaFileSet fileset;


		public PlayerVM ()
		{
			playerController = new PlayerController (true);
			playerController.CamerasConfig = new ObservableCollection<CameraConfig> { new CameraConfig (0) };
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

		public bool DrawingsVisible {
			get;
			set;
		}

		[PropertyChanged.DoNotNotify]
		public double Volume {
			get {
				return playerController.Volume;
			}
			set {
				playerController.Volume = value;
			}
		}

		public float Rate {
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

		[PropertyChanged.DoNotNotify]
		public MediaFileSet FileSet {
			get {
				return fileset;
			}
			set {
				fileset = value;
				if (fileset == null || !fileset.Any ()) {
					ControlsSensitive = false;
				} else {
					ControlsSensitive = true;
				}
			}
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

		public PlayerViewOperationMode Mode {
			set {
				mode = value;
				switch (mode) {
				case PlayerViewOperationMode.Analysis:
					Compact = false;
					ShowControls = true;
					break;
				case PlayerViewOperationMode.LiveAnalysisReview:
					Compact = true;
					ShowControls = true;
					break;
				case PlayerViewOperationMode.Synchronization:
					Compact = false;
					ShowControls = false;
					break;
				}
			}
			get {
				return mode;
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

		public IPlayerController Player {
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

		public ObservableCollection<CameraConfig> CamerasConfig {
			get {
				return playerController.CamerasConfig;
			}
			set {
				playerController.CamerasConfig = value;
			}
		}

		[PropertyChanged.DoNotNotify]
		public bool Opened {
			get {
				return playerController.Opened;
			}
		}

		public bool PresentationMode {
			set {
				playerController.PresentationMode = value;
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

		public void Ready ()
		{
			playerController.Ready ();
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

		public void SetRate (float rate)
		{
			playerController.Rate = rate;
			App.Current.EventsBroker.Publish<PlaybackRateChangedEvent> (
				new PlaybackRateChangedEvent {
					Value = rate
				}
			);
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

		public void OpenFileSet (MediaFileSet fileset)
		{
			FileSet = fileset;
			playerController.Open (fileset);
		}

		public void ResetCounter ()
		{
			//FIXME: Should be fixed?
			(playerController as VAS.Services.PlayerController).ResetCounter ();
			App.Current.EventsBroker.Publish<ChangeVideoMessageEvent> (
				new ChangeVideoMessageEvent () {
					message = Catalog.GetString ("No video loaded")
				});
		}

		public void ApplyROI (CameraConfig cameraConfig)
		{
			playerController.ApplyROI (cameraConfig);
		}

		public void LoadEvent (TimelineEvent e)
		{
			if (e?.Duration.MSeconds == 0) {
				// These events don't have duration, we start playing as if it was a seek
				Player.Switch (null, null, null);
				Player.UnloadCurrentEvent ();
				Player.Seek (e.EventTime, true);
				Player.Play ();
			} else {
				if (e != null) {
					LoadPlay (e, new Time (0), true);
				} else if (Player != null) {
					Player.UnloadCurrentEvent ();
				}
			}
		}

		public void LoadEvents (List<TimelineEvent> events)
		{
			Playlist playlist = new Playlist ();

			List<IPlaylistElement> list = events
				.Select (evt => new PlaylistPlayElement (evt))
				.OfType<IPlaylistElement> ()
				.ToList ();
			
			playlist.Elements = new ObservableCollection<IPlaylistElement> (list);
			playerController.LoadPlaylistEvent (playlist, list.FirstOrDefault (), true);			
		}

		public void LoadPlay (TimelineEvent e, Time seekTime, bool playing)
		{
			e.Selected = true;
			Player.LoadEvent (
				e, seekTime, playing);
			if (playing) {
				Player.Play ();
			}
		}

		#endregion
	}
}

