//
//  Copyright (C) 2016 Fluendo S.A.
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
			playerController = new PlayerController (false);
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

		public IPlaylistElement PlayListElement {
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

		public bool ShowDetachButton {
			set;
			get;
		}

		public IPlayerController Player {
			get {
				return playerController;
			}
			set {
				playerController = value;
			}
		}

		public bool SupportsMultipleCameras {
			get {
				return false;
			}
		}

		public bool PlayerAttached {
			set;
			get;
		}

		public object CamerasLayout {
			get {
				return 0;
			}
			set {
			}
		}

		public bool IgnoreTicks {
			set {
				Player.IgnoreTicks = value;
			}
		}

		[PropertyChanged.DoNotNotify]
		public Time Step {
			set {
				playerController.Step = value;
			}
		}

		[PropertyChanged.DoNotNotify]
		public List<IViewPort> ViewPorts {
			set {
				playerController.ViewPorts = value;
			}
		}

		public bool PrepareView {
			get;
			set;
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

		public void Seek (double time)
		{
			playerController.Seek (time);
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

