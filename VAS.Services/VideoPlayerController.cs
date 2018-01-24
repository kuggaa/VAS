//
//  Copyright (C) 2015 Fluendo S.A.
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Handlers;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;
using Timer = System.Threading.Timer;

namespace VAS.Services
{
	// FIXME: This should inherit from ControllerBase<VideoPlayerVM> in order to manage events correctly
	// but we need to refactor the way AnalysisHelperState is working.
	public class VideoPlayerController : ControllerBase, IVideoPlayerController
	{
		public event TimeChangedHandler TimeChangedEvent;
		public event StateChangeHandler PlaybackStateChangedEvent;
		public event LoadDrawingsHandler LoadDrawingsEvent;
		public event PlaybackRateChangedHandler PlaybackRateChangedEvent;
		public event VolumeChangedHandler VolumeChangedEvent;
		public event ElementLoadedHandler ElementLoadedEvent;
		public event MediaFileSetLoadedHandler MediaFileSetLoadedEvent;
		public event PrepareViewHandler PrepareViewEvent;

		const int TIMEOUT_MS = 20;
		const int MSECONDS_STEP = 20;

		IVideoPlayer player;
		IMultiVideoPlayer multiPlayer;
		List<IViewPort> viewPorts;
		RangeObservableCollection<CameraConfig> camerasConfig;
		RangeObservableCollection<CameraConfig> clonedCamerasConfig;
		RangeObservableCollection<CameraConfig> defaultCamerasConfig;
		object defaultCamerasLayout;
		MediaFileSet defaultFileSet;
		MediaFileSet mediafileSet;
		MediaFileSet mediaFileSetCopy;
		Time duration, videoTS, imageLoadedTS;
		bool readyToSeek, stillimageLoaded, ready;
		bool disposed, skipApplyCamerasConfig;
		Action delayedOpen;
		ISeeker seeker;
		Segment loadedSegment;
		PendingSeek pendingSeek;
		readonly ITimer timer;
		bool active;

		readonly Time editDurationOffset = new Time { TotalSeconds = 10 };
		VideoPlayerVM playerVM;
		TimeNode visibleRegion;
		VideoPlayerOperationMode mode;
		PlaylistVM loadedPlaylistVM;
		TimelineEventVM loadedEvent;
		IPlayableEvent loadedPlaylistEvent;
		IPlayable loadedPlaylistElement;
		object camerasLayout;
		bool supportsMultipleCameras;
		Time drawingCurrentTime;

		protected struct Segment
		{
			public Time Start;
			public Time Stop;
		}

		protected class PendingSeek
		{
			public Time time;
			public float rate;
			public bool playing;
			public bool accurate;
			public bool syncrhonous;
			public bool throttled;
		}

		#region Constructors

		public VideoPlayerController (ISeeker testSeeker = null, ITimer testTimer = null)
		{
			// Injected seeker and timer should only be used for unit tests
			seeker = testSeeker ?? new Seeker ();
			seeker.SeekEvent += HandleSeekEvent;
			loadedSegment.Start = new Time (-1);
			loadedSegment.Stop = new Time (int.MaxValue);
			videoTS = new Time (0);
			imageLoadedTS = new Time (0);
			duration = new Time (0);
			timer = testTimer ?? App.Current.DependencyRegistry.Retrieve<ITimer> ();
			timer.Elapsed += HandleTimeout;
			ready = false;
			CreatePlayer ();
			Active = true;
			Mode = VideoPlayerOperationMode.Normal;
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			ReconfigureTimeout (0);
			IgnoreTicks = true;
			seeker.Dispose ();
			timer.Stop ();
			timer.Dispose ();
			player.Error -= HandleError;
			player.StateChange -= HandleStateChange;
			player.Eos -= HandleEndOfStream;
			player.ReadyToSeek -= HandleReadyToSeek;
			player.Dispose ();
			player = null;
			FileSet = null;
			loadedPlaylistVM = null;
		}

		#endregion

		#region IPlayerController implementation

		public virtual bool IgnoreTicks {
			get;
			set;
		}

		public virtual RangeObservableCollection<CameraConfig> CamerasConfig {
			set {
				Log.Debug ("Updating cameras configuration: " + value);
				camerasConfig = value;
				clonedCamerasConfig = null;

				if (camerasConfig != null) {
					//Limit Region of interest if Open Zoom for Events/Filesets is limited
					if (!App.Current.LicenseLimitationsService.CanExecute
						(VASFeature.OpenZoom.ToString ())) {
						clonedCamerasConfig = camerasConfig.Clone ();
						foreach (var camConfig in clonedCamerasConfig) {
							camConfig.RegionOfInterest = new Area (0, 0, 0, 0);
						}
					}
				}

				PlayerVM.CamerasConfig = camerasConfig;
				if (defaultCamerasConfig == null) {
					defaultCamerasConfig = camerasConfig;
				}
				if (LoadedTimelineEvent != null && !(LoadedTimelineEvent.CamerasConfig.SequenceEqualSafe (camerasConfig))) {
					LoadedTimelineEvent.CamerasConfig.Replace (camerasConfig);
				}
				if (multiPlayer != null) {
					multiPlayer.CamerasConfig = camerasConfig;
				}
				if (!skipApplyCamerasConfig && Opened) {
					ApplyCamerasConfig ();
				}
			}
			get {
				if (clonedCamerasConfig != null) {
					return clonedCamerasConfig;
				}
				return camerasConfig;
			}
		}

		public virtual object CamerasLayout {
			get {
				return camerasLayout;
			}

			set {
				camerasLayout = value;
				PlayerVM.CamerasLayout = value;
			}
		}

		public List<IViewPort> ViewPorts {
			protected get {
				return viewPorts;
			}
			set {
				if (value != null) {
					if (multiPlayer == null) {
						player.WindowHandle = value [0].WindowHandle;
					} else {
						multiPlayer.WindowHandles = value.Select (v => v.WindowHandle ?? IntPtr.Zero).ToList ();
					}
				} else {
					if (multiPlayer == null && player != null) {
						player.WindowHandle = IntPtr.Zero;
					} else if (multiPlayer != null && viewPorts != null) {
						List<object> nullWindowHandles = new List<object> ();
						viewPorts.ForEach ((vp) => nullWindowHandles.Add (IntPtr.Zero));
						multiPlayer.WindowHandles = nullWindowHandles;
					}
				}
				viewPorts = value;
			}
		}

		public virtual double Volume {
			get {
				return player.Volume;
			}
			set {
				player.Volume = value;
				EmitVolumeChanged (value);
			}
		}

		public virtual double Rate {
			set {
				float newRate = (float)App.Current.RateList.OrderBy (n => Math.Abs (n - value)).First ();
				SetRate (newRate);
				Log.Debug ("Rate set to " + value);
			}
			get {
				return player.Rate;
			}
		}

		public virtual Time CurrentTime {
			get {
				if (StillImageLoaded) {
					return new Time (imageLoadedTS.MSeconds);
				} else {
					return player.CurrentTime;
				}
			}
		}

		public virtual Time StreamLength {
			get {
				return player.StreamLength;
			}
		}

		public virtual Image CurrentMiniatureFrame {
			get {
				return player.GetCurrentFrame (Constants.MAX_THUMBNAIL_SIZE, Constants.MAX_THUMBNAIL_SIZE);
			}
		}

		public virtual Image CurrentFrame {
			get {
				return player.GetCurrentFrame ();
			}
		}

		public virtual bool Playing {
			get {
				return PlayerVM.Playing;
			}
			set {
				PlayerVM.Playing = value;
			}
		}

		public virtual MediaFileSet FileSet {
			get {
				return mediafileSet;
			}
			protected set {
				if (mediafileSet != null) {
					mediafileSet.PropertyChanged -= HandleMediaFileSetPropertyChanged;
				}
				mediafileSet = value;
				mediaFileSetCopy = value?.Clone ();
				if (mediafileSet != null) {
					visibleRegion = FileSet.VisibleRegion;
					mediafileSet.PropertyChanged += HandleMediaFileSetPropertyChanged;
				} else {
					visibleRegion = null;
				}
				PlayerVM.FileSet = new MediaFileSetVM { Model = mediafileSet };
			}
		}

		public virtual bool Opened {
			get {
				return FileSet != null;
			}
		}

		public virtual bool Active {
			get { return active; }
			set {
				active = value;
				if (!value && Playing) {
					Pause ();
				}
			}
		}

		public virtual PlaylistVM LoadedPlaylist {
			get {
				return loadedPlaylistVM;
			}

			set {
				if (loadedPlaylistVM != null) {
					loadedPlaylistVM.PropertyChanged -= HandlePlaylistDurationChanged;
				}
				loadedPlaylistVM = value;
				if (loadedPlaylistVM != null) {
					loadedPlaylistVM.PropertyChanged += HandlePlaylistDurationChanged;
				}
			}
		}

		public virtual VideoPlayerOperationMode Mode {
			get {
				return mode;
			}
			set {
				mode = value;
				if (FileSet?.FirstOrDefault () != null) {
					visibleRegion = FileSet.VisibleRegion;
					Time currentTime = CurrentTime;
					UnloadCurrentEvent ();
					if (Mode == VideoPlayerOperationMode.Stretched) {
						LoadSegment (mediafileSet, visibleRegion.Start, visibleRegion.Stop,
									 CurrentTime.Clamp (visibleRegion.Start, visibleRegion.Stop), (float)Rate,
									 CamerasConfig, CamerasLayout, Playing);
					}
					UpdateDuration ();
				}
			}
		}

		public VideoPlayerVM PlayerVM {
			get {
				return playerVM;
			}

			set {
				if (playerVM != null) {
					playerVM.PropertyChanged -= HandlePlayerVMPropertyChanged;
				}
				playerVM = value;
				if (playerVM != null) {
					playerVM.PropertyChanged += HandlePlayerVMPropertyChanged;
				}
			}
		}

		public override void SetViewModel (IViewModel viewModel)
		{
			PlayerVM = viewModel as VideoPlayerVM;
			if (PlayerVM == null) {
				PlayerVM = ((IVideoPlayerDealer)viewModel).VideoPlayer;
			}
			PlayerVM.Player = this;
			PlayerVM.SupportsMultipleCameras = supportsMultipleCameras;
		}

		public virtual void Ready (bool ready)
		{
			if (ready) {
				Log.Debug ("Player ready");
				this.ready = true;
				if (delayedOpen != null) {
					Log.Debug ("Calling delayed open");
					delayedOpen ();
					delayedOpen = null;
				} else if (FileSet == null || !FileSet.Any ()) {
					ShowMessageInViewPorts (Catalog.GetString ("No video available"), true);
				} else {
					Log.Debug ("Open previous fileset");
					Open (FileSet, false);
				}
			} else {
				Log.Debug ("Player unready, closing player");
				player?.Close ();
				this.ready = false;
				delayedOpen = null;
			}
		}

		public virtual void Open (MediaFileSet fileSet, bool play = false)
		{
			Log.Debug ("Opening file set");
			if (fileSet == null || !fileSet.Any () || !fileSet.CheckFiles ()) {
				Stop (false);
				EmitTimeChanged (new Time (0), new Time (0));
				FileSet = fileSet;
				IgnoreTicks = true;
				PlayerVM.ControlsSensitive = false;
				ShowMessageInViewPorts (Catalog.GetString ("No video loaded"), true);
				UpdateDuration ();
				delayedOpen = null;
				return;
			}

			IgnoreTicks = false;
			ShowMessageInViewPorts (null, false);
			if (ready) {
				InternalOpen (fileSet, true, true, play, true);
			} else {
				Log.Debug ("Player is not ready, delaying ...");
				delayedOpen = () => InternalOpen (fileSet, true, true, play, true);
				FileSet = fileSet;
			}
		}

		public virtual void Stop (bool synchronous)
		{
			Log.Debug ("Stop");
			Pause (synchronous);
		}

		public virtual void Play (bool synchronous = false)
		{
			Log.Debug ("Play");
			if (StillImageLoaded) {
				ReconfigureTimeout (TIMEOUT_MS);
				EmitPlaybackStateChanged (this, true);
			} else {
				EmitLoadDrawings (null);
				if (pendingSeek != null) {
					pendingSeek.playing = true;
				} else {
					player.Play (synchronous);
				}
			}
			Playing = true;
		}

		public virtual void Pause (bool synchronous = false)
		{
			Log.Debug ("Pause");
			if (StillImageLoaded) {
				ReconfigureTimeout (0);
				EmitPlaybackStateChanged (this, false);
			} else {
				if (pendingSeek != null) {
					pendingSeek.playing = false;
				} else {
					player.Pause (synchronous);
				}
			}
			Playing = false;
		}

		public virtual void TogglePlay ()
		{
			Log.Debug ("Toggle playback");
			if (Playing)
				Pause ();
			else
				Play ();
		}

		public virtual bool Seek (Time time, bool accurate = false, bool synchronous = false, bool throttled = false)
		{
			Log.Debug (string.Format ("PlayerController::Seek (time: {0}, accurate: {1}, synchronous: {2}, throttled: {3}", time, accurate, synchronous, throttled));

			if (Mode == VideoPlayerOperationMode.Presentation) {
				return PlaylistSeek (time, accurate, synchronous, throttled);
			} else {
				if (SegmentLoaded) {
					time += loadedSegment.Start;
					accurate = true;
					Log.Debug ("Segment loaded - seek accurate");
				}
				return AbsoluteSeek (time, accurate, synchronous, throttled);
			}
		}

		public virtual bool Seek (Time time, bool accurate, bool synchronous)
		{
			return Seek (time, accurate, synchronous, false);
		}

		public void Seek (double pos)
		{
			Time seekPos;
			bool accurate;
			bool throthled;

			Log.Debug (string.Format ("Seek relative to {0}", pos));
			if (SegmentLoaded) {
				Time duration = loadedSegment.Stop - loadedSegment.Start;
				seekPos = duration * pos;
				accurate = true;
				throthled = true;
			} else {
				seekPos = duration * pos;
				accurate = false;
				throthled = false;
			}
			Seek (seekPos, accurate, false, throthled);
		}

		public virtual bool SeekToNextFrame ()
		{
			Log.Debug ("Seek to next frame");
			if (!StillImageLoaded) {
				EmitLoadDrawings (null);
				if (CurrentTime < loadedSegment.Stop) {
					player.SeekToNextFrame ();
					Tick ();
				}
			}
			return true;
		}

		public virtual bool SeekToPreviousFrame ()
		{
			Log.Debug ("Seek to previous frame");
			if (!StillImageLoaded) {
				EmitLoadDrawings (null);
				if (CurrentTime > loadedSegment.Start) {
					player.SeekToPreviousFrame ();
					Tick ();
				}
			}
			return true;
		}

		public virtual void StepForward ()
		{
			Log.Debug ("Step forward");
			if (StillImageLoaded) {
				return;
			}
			PerformStep (PlayerVM.Step);
		}

		public virtual void StepBackward ()
		{
			Log.Debug ("Step backward");
			if (StillImageLoaded) {
				return;
			}
			PerformStep (new Time (-PlayerVM.Step.MSeconds));
		}

		public virtual void FramerateUp ()
		{
			if (!StillImageLoaded) {
				float rate;

				EmitLoadDrawings (null);
				rate = (float)Rate;
				if (rate >= (float)App.Current.RateList.Last ()) {
					return;
				}
				Log.Debug ("Framerate up");
				int index = App.Current.RateList.FindIndex (p => (float)p == rate);
				if (index < App.Current.RateList.Count - 1) {
					SetRate ((float)App.Current.RateList [index + 1]);
				}
			}
		}

		public virtual void FramerateDown ()
		{
			if (!StillImageLoaded) {
				float rate;

				EmitLoadDrawings (null);
				rate = (float)Rate;
				if (rate == (float)App.Current.RateList.First ()) {
					return;
				}
				Log.Debug ("Framerate down");
				int index = App.Current.RateList.FindIndex (p => (float)p == rate);
				if (index > 0) {
					SetRate ((float)App.Current.RateList [index - 1]);
				}
			}
		}

		public virtual void FramerateUpper ()
		{
			if (!StillImageLoaded) {
				float rate;

				EmitLoadDrawings (null);
				rate = (float)Rate;
				if (rate >= (float)App.Current.RateList.Last ()) {
					return;
				}
				Log.Debug ("Framerate upper");
				SetRate ((float)App.Current.RateList.Last ());
			}
		}

		public virtual void FramerateLower ()
		{
			if (!StillImageLoaded) {
				float rate;

				EmitLoadDrawings (null);
				rate = (float)Rate;
				if (rate == (float)App.Current.RateList [(int)App.Current.DefaultRate - (int)App.Current.LowerRate]) {
					return;
				}
				Log.Debug ("Framerate lower");
				SetRate ((float)App.Current.RateList [(int)App.Current.DefaultRate - (int)App.Current.LowerRate]);
			}
		}

		public virtual void Expose ()
		{
			player.Expose ();
		}

		public void Switch (TimelineEventVM play, PlaylistVM playlist, IPlayable element)
		{
			UpdatePlayingState (false);

			drawingCurrentTime = null;
			loadedEvent = play;
			loadedPlaylistElement = element;
			PlayerVM.FrameDrawing = null;
			LoadedPlaylist = playlist;
			LoadedTimelineEvent = play ?? element as IPlayableEvent;

			UpdatePlayingState (true);
		}

		public virtual void LoadPlaylistEvent (PlaylistVM playlist, IPlayable element, bool playing)
		{
			Log.Debug (string.Format ("Loading playlist element \"{0}\"", element.Description));

			if (!ready) {
				EmitPrepareViewEvent (element.CamerasConfig, element.CamerasLayout);
				delayedOpen = () => LoadPlaylistEvent (playlist, element, playing);
				return;
			}

			if (playlist == null) {
				return;
			}

			Switch (null, playlist, element);

			switch (element) {
			case PlaylistPlayElementVM ple:
				LoadSegment (ple.Play.FileSet, ple.Play.Start, ple.Play.Stop,
							 ple.Play.Start, ple.Play.Rate, ple.CamerasConfig,
							ple.CamerasLayout, playing);
				break;
			case PlaylistVideoVM video:
				LoadVideo (video, playing);
				break;
			case PlaylistImageVM image:
				LoadStillImage (image, playing);
				break;
			case PlaylistDrawingVM drawing:
				LoadFrameDrawing (drawing, playing);
				break;
			}

			UpdateDuration ();
			UpdatePlayingState (true);
			LoadedPlaylist.SetActive ((PlaylistElementVM)element);
			EmitElementLoaded (element, playlist);
		}

		public virtual void LoadEvent (TimelineEventVM evt, Time seekTime, bool playing)
		{
			playerVM.LoadedElement = evt;

			MediaFileSet fileSet = evt.FileSet;
			Log.Debug (string.Format ("Loading event \"{0}\" seek:{1} playing:{2}", evt.Name, seekTime, playing));

			if (!ready) {
				EmitPrepareViewEvent (evt.CamerasConfig, evt.CamerasLayout);
				delayedOpen = () => LoadEvent (evt, seekTime, playing);
				return;
			}

			Switch (evt, null, null);

			if (evt.Start != null && evt.Stop != null) {
				LoadSegment (fileSet, evt.Start, evt.Stop, evt.Start + seekTime, evt.Rate,
							 evt.CamerasConfig, evt.CamerasLayout, playing);
				if (LoadedTimelineEvent == null) { // LoadSegment sometimes removes the loadedEvent
					LoadedTimelineEvent = evt;
				}
			} else if (evt.EventTime != null) {
				AbsoluteSeek (evt.EventTime, true);
			} else {
				Log.Error ("Event does not have timing info: " + evt);
			}
			UpdateDuration ();

			EmitElementLoaded (playerVM.LoadedElement, null);
		}

		public virtual void UnloadCurrentEvent ()
		{
			Log.Debug ("Unload current event");
			if (loadedPlaylistElement == null && loadedEvent == null) {
				Reset ();
				UpdateDuration ();
				return;
			}
			Reset ();
			loadedEvent = null;
			LoadedTimelineEvent = null;
			PlayerVM.FrameDrawing = null;

			if (defaultFileSet != null && !defaultFileSet.Equals (FileSet)) {
				UpdateCamerasConfig (defaultCamerasConfig, defaultCamerasLayout);
				EmitEventUnloaded ();
				Open (defaultFileSet);
			} else {
				CamerasConfig = defaultCamerasConfig;
				EmitEventUnloaded ();
			}

			if (Mode == VideoPlayerOperationMode.Stretched) {
				LoadSegment (mediafileSet, visibleRegion.Start, visibleRegion.Stop,
							 CurrentTime.Clamp (visibleRegion.Start, visibleRegion.Stop), (float)Rate,
							 CamerasConfig, CamerasLayout, Playing);
			}
			UpdateDuration ();
		}

		public virtual void Next ()
		{
			Log.Debug ("Next");
			if (loadedPlaylistElement != null) {
				if (LoadedPlaylist.HasNext ()) {
					LoadPlaylistEvent (LoadedPlaylist, LoadedPlaylist.Next (), Playing);
				} else {
					Pause ();
					Seek (new Time (0), true);
				}
			} else {
				Pause ();
			}
		}

		public virtual void Previous (bool force = false)
		{
			Log.Debug ("Previous");

			/* Select the start of the element if it's a regular play */
			if (loadedEvent != null) {
				Seek (new Time (0), true);
			} else if (loadedPlaylistElement != null) {
				/* Select the start of the element if we haven't played 500ms, unless forced */
				Time start = new Time (0);
				if (loadedPlaylistElement is PlaylistPlayElementVM) {
					start = (loadedPlaylistElement as PlaylistPlayElementVM).Play.Start;
				}
				if (!force && (CurrentTime - start).MSeconds > 500) {
					LoadPlaylistEvent (LoadedPlaylist, loadedPlaylistElement, Playing);
					return;
				}
				if (LoadedPlaylist.HasPrev ()) {
					LoadPlaylistEvent (LoadedPlaylist, LoadedPlaylist.Prev (), Playing);
				}
			} else {
				Seek (new Time (0), true);
			}
		}

		public virtual void ApplyROI (CameraConfig camConfig)
		{

			int index = camerasConfig.IndexOf (camConfig);
			camerasConfig [index] = camConfig;

			if (multiPlayer != null) {
				multiPlayer.ApplyROI (camConfig);
			}
			UpdateZoom ();
		}

		public virtual void DrawFrame ()
		{
			// FIXME: Drawing tool could use IPlaylistEventElement

			TimelineEventVM evt = playerVM.LoadedElement as TimelineEventVM ??
										   (loadedPlaylistElement as PlaylistPlayElementVM)?.Play;

			App.Current.EventsBroker.Publish (
				new DrawFrameEvent {
					Play = evt,
					DrawingIndex = -1,
					CamConfig = evt == null ? null : evt.Model.CamerasConfig [0],
					Frame = CurrentFrame
				}
			);
		}

		public void SetZoom (float zoomLevel)
		{
			if (zoomLevel < 1 || zoomLevel > App.Current.ZoomLevels.Max ()) {
				Log.Error ("Zoom level is not between the supported boundaries : " + zoomLevel);
				return;
			}
			if (CamerasConfig == null) {
				return;
			}
			CameraConfig cfg = CamerasConfig [0];
			Point origin = cfg.RegionOfInterest.Center;
			MediaFile file = FileSet [cfg.Index];

			cfg.RegionOfInterest.Width = file.VideoWidth / zoomLevel;
			cfg.RegionOfInterest.Height = file.VideoHeight / zoomLevel;

			// Center with regards to previous origin
			cfg.RegionOfInterest.Start.X = origin.X - cfg.RegionOfInterest.Width / 2;
			cfg.RegionOfInterest.Start.Y = origin.Y - cfg.RegionOfInterest.Height / 2;

			ClipRoi (cfg.RegionOfInterest, file);

			ApplyROI (cfg);
		}

		/// <summary>
		/// Sets the steps to perform jumps in the video player.
		/// </summary>
		/// <param name="step">Steps.</param>
		public void SetStep (Time step)
		{
			if (step.TotalSeconds < App.Current.StepList.Min () || step.TotalSeconds > App.Current.StepList.Max ()) {
				Log.Error ("Steps are not between the supported boundaries : " + step.TotalSeconds);
				return;
			}
			PlayerVM.Step = new Time { TotalSeconds = (int)step.TotalSeconds };
		}

		public void MoveROI (Point diff)
		{
			CameraConfig cfg = CamerasConfig [0];
			MediaFile file = FileSet [cfg.Index];

			cfg.RegionOfInterest = new Area (cfg.RegionOfInterest.Start.X - diff.X,
											 cfg.RegionOfInterest.Start.Y - diff.Y,
											 cfg.RegionOfInterest.Width,
											 cfg.RegionOfInterest.Height);
			ClipRoi (cfg.RegionOfInterest, FileSet [cfg.Index]);
			ApplyROI (cfg);
		}

		public void SetEditEventDurationMode (bool enabled)
		{
			if (enabled && LoadedTimelineEvent == null) {
				return;
			}
			if (enabled) {
				PlayerVM.EditEventDurationTimeNode.Model = new TimeNode {
					Start = (LoadedTimelineEvent.Start - editDurationOffset).Clamp (new Time (0), FileSet.Duration),
					Stop = (LoadedTimelineEvent.Stop + editDurationOffset).Clamp (new Time (0), FileSet.Duration)
				};
			} else {
				PlayerVM.EditEventDurationTimeNode.Model = null;
			}
			PlayerVM.EditEventDurationModeEnabled = enabled;
		}

		#endregion

		#region IController

		public override async Task Start ()
		{
			await base.Start ();
		}

		public override async Task Stop ()
		{
			await base.Stop ();
			Stop (true);
		}

		public override IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return new KeyAction [] {
				new KeyAction (
					App.Current.HotkeysService.GetByName(PlaybackHotkeys.PLAYER_RATE_INCREMENT),
					FramerateUp
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName(PlaybackHotkeys.PLAYER_RATE_DECREMENT),
					FramerateDown
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName(PlaybackHotkeys.PLAYER_RATE_MAX),
					FramerateUpper
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName(PlaybackHotkeys.PLAYER_RATE_DEFAULT),
					FramerateLower
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName(PlaybackHotkeys.PLAYER_TOGGLE_PLAY),
					TogglePlay
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName(PlaybackHotkeys.PLAYER_SEEK_LEFT_SHORT),
					() => SeekToPreviousFrame()
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName(PlaybackHotkeys.PLAYER_SEEK_LEFT_LONG),
					() => StepBackward()
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName(PlaybackHotkeys.PLAYER_SEEK_RIGHT_SHORT),
					() => SeekToNextFrame()
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName(PlaybackHotkeys.PLAYER_SEEK_RIGHT_LONG),
					() => StepForward()
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName(PlaybackHotkeys.PLAYER_NEXT_ELEMENT),
					Next
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName(PlaybackHotkeys.PLAYER_PREVIOUS_ELEMENT),
					() => Previous()
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName(PlaybackHotkeys.OPEN_DRAWING_TOOL),
					() => PlayerVM.DrawCommand.Execute ()
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName(PlaybackHotkeys.ZOOM_RESTORE),
					() => PlayerVM.SetZoomCommand.Execute (App.Current.ZoomLevels[0])
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName(PlaybackHotkeys.ZOOM_INCREASE),
					IncreaseZoom
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName(PlaybackHotkeys.ZOOM_DECREASE),
					DecreaseZoom
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName(PlaybackHotkeys.EDIT_EVENT_DURATION),
					() => {
						if (PlayerVM.EditEventDurationCommand.CanExecute ()) {
							SetEditEventDurationMode (!PlayerVM.EditEventDurationModeEnabled);
						}
					}
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName(PlaybackHotkeys.CLOSE_LOADED_EVENT),
					UnloadCurrentEvent
				),
				new KeyAction (new KeyConfig {
					Name = "",
					Key = App.Current.Keyboard.ParseName ("Right"),
				}, () => HandleKeyPressed (true)),
				new KeyAction (new KeyConfig {
					Name = "",
					Key = App.Current.Keyboard.ParseName ("Left"),
				}, () => HandleKeyPressed (false))
			};
		}

		#endregion

		#region Signals

		void EmitLoadDrawings (FrameDrawing drawing = null)
		{
			PlayerVM.FrameDrawing = drawing;
			if (LoadDrawingsEvent != null && !disposed) {
				LoadDrawingsEvent (drawing);
			}
		}

		void EmitPrepareViewEvent (RangeObservableCollection<CameraConfig> camConfig, object camLayout)
		{
			PlayerVM.PrepareView = true;
			if (camConfig != null) {
				UpdateCamerasConfig (camConfig, camLayout);
			}
			if (PrepareViewEvent != null && !disposed) {
				PrepareViewEvent ();
			}
		}

		void EmitElementLoaded (IPlayable element, PlaylistVM playlist)
		{
			PlayerVM.NextCommand.Executable = playlist != null ? playlist.HasNext () : false;
			PlayerVM.LoadedElement = element;

			if (element == null || element is TimelineEventVM) {
				App.Current.EventsBroker.Publish (
					new EventLoadedEvent {
						TimelineEvent = element as TimelineEventVM,
					}
				);
			} else {
				App.Current.EventsBroker.Publish (
					new PlaylistElementLoadedEvent {
						Playlist = playlist,
						Element = element,
					}
				);
			}
			if (ElementLoadedEvent != null && !disposed) {
				ElementLoadedEvent (element, PlayerVM.NextCommand.CanExecute ());
			}
		}

		void EmitEventUnloaded ()
		{
			EmitElementLoaded (null, null);
		}

		void EmitRateChanged (float rate)
		{
			PlayerVM.Rate = rate;
			if (PlaybackRateChangedEvent != null && !disposed) {
				PlaybackRateChangedEvent (rate);
			}
		}

		void EmitVolumeChanged (double volume)
		{
			PlayerVM.Volume = volume;
			if (VolumeChangedEvent != null && !disposed) {
				VolumeChangedEvent (volume);
			}
		}

		void EmitTimeChanged (Time currentTime, Time relativeTime)
		{
			if (Mode == VideoPlayerOperationMode.Stretched) {
				currentTime = currentTime - visibleRegion.Start;
			}

			PlayerVM.AbsoluteCurrentTime = currentTime;
			PlayerVM.CurrentTime = relativeTime;
			PlayerVM.SeekCommand.Executable = !StillImageLoaded;


			if (TimeChangedEvent != null && !disposed) {
				TimeChangedEvent (relativeTime, PlayerVM.Duration, !StillImageLoaded);
			}
			App.Current.EventsBroker.Publish (
				new PlayerTickEvent {
					Time = currentTime,
					RelativeTime = relativeTime
				}
			);
		}

		void EmitPlaybackStateChanged (object sender, bool playing)
		{
			PlayerVM.Playing = playing;
			if (PlaybackStateChangedEvent != null && !disposed) {
				PlaybackStateChangedEvent playbackStateChangedEvent = new PlaybackStateChangedEvent {
					Sender = sender,
					Playing = playing
				};
				PlaybackStateChangedEvent (playbackStateChangedEvent);
				App.Current.EventsBroker.Publish<PlaybackStateChangedEvent> (playbackStateChangedEvent);
			}
		}

		#endregion

		#region Private Properties

		/// <summary>
		/// Indicates if a still image is loaded instead of a video segment.
		/// </summary>
		bool StillImageLoaded {
			set {
				stillimageLoaded = value;
				if (stillimageLoaded) {
					EmitPlaybackStateChanged (this, Playing);
					player.Pause ();
					imageLoadedTS = new Time (0);
					ReconfigureTimeout (TIMEOUT_MS);
				}
			}
			get {
				return stillimageLoaded;
			}
		}

		/// <summary>
		/// Inidicates if a video segment is loaded.
		/// </summary>
		bool SegmentLoaded {
			get {
				return loadedSegment.Start.MSeconds != -1;
			}
		}

		IPlayableEvent LoadedTimelineEvent {
			set {
				if (loadedPlaylistEvent != null) {
					loadedPlaylistEvent.PropertyChanged -= HandleLoadedTimelineEventPropertyChangedEventHandler;
					loadedPlaylistEvent.Drawings.CollectionChanged -= HandlePlaylistEventDrawingsCollectionChanged;
				}
				loadedPlaylistEvent = value;
				if (loadedPlaylistEvent != null) {
					PlayerVM.EditEventDurationCommand.Executable = true;
					loadedPlaylistEvent.PropertyChanged += HandleLoadedTimelineEventPropertyChangedEventHandler;
					loadedPlaylistEvent.Drawings.CollectionChanged += HandlePlaylistEventDrawingsCollectionChanged;
				} else {
					PlayerVM.EditEventDurationCommand.Executable = false;
				}
				SetEditEventDurationMode (false);
			}
			get {
				return loadedPlaylistEvent;
			}
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Seeks absolutely. This seek is the one that will go to the real player, made over the video file.
		/// </summary>
		/// <returns><c>true</c>, if seek was made correctly, <c>false</c> otherwise.</returns>
		/// <param name="time">Time in the video to seek.</param>
		/// <param name="accurate">If set to <c>true</c>, accurate seek.</param>
		/// <param name="synchronous">If set to <c>true</c>, synchronous seek.</param>
		/// <param name="throttled">If set to <c>true</c>, throttled seek.</param>
		bool AbsoluteSeek (Time time, bool accurate, bool synchronous = false, bool throttled = false)
		{
			if (StillImageLoaded) {
				imageLoadedTS = time;
				Tick ();
			} else {
				EmitLoadDrawings (null);
				if (readyToSeek) {
					if (throttled) {
						Log.Debug ("Throttled seek");
						seeker.Seek (accurate ? SeekType.Accurate : SeekType.Keyframe, time, (float)Rate);
					} else {
						Log.Debug (string.Format ("Seeking to {0} accurate:{1} synchronous:{2} throttled:{3}",
							time, accurate, synchronous, throttled));
						player.Seek (time, accurate, synchronous);
						Tick (time);
					}
				} else {
					Log.Debug ("Delaying seek until player is ready");
					pendingSeek = new PendingSeek {
						time = time,
						rate = 1.0f,
						accurate = accurate,
						syncrhonous = synchronous,
						throttled = throttled
					};
				}
			}
			return true;
		}

		bool PlaylistSeek (Time time, bool accurate = false, bool synchronous = false, bool throttled = false)
		{
			if (loadedPlaylistElement == null) {
				return AbsoluteSeek (time, accurate, synchronous, throttled);
			}

			// if time is outside the currently loaded event
			var elementTuple = LoadedPlaylist.GetElementAtTime (time);
			var elementAtTime = elementTuple.Item1;
			var elementStart = elementTuple.Item2;

			if (elementAtTime?.Model != (loadedPlaylistElement as PlaylistElementVM)?.Model || (elementStart > time ||
				elementStart + elementAtTime.Duration < time)) {
				if (elementAtTime == null) {
					Log.Debug (String.Format ("There is no playlist element at {0}.", time));
					return false;
				}

				LoadPlaylistEvent (LoadedPlaylist, elementAtTime, false);
			}

			time -= elementStart;

			var play = loadedPlaylistElement as PlaylistPlayElementVM;
			if (play != null) {
				time += play.Play.Start;
				if (time > play.Play.FileSet.Duration) {
					Log.Warning (String.Format ("Attempted seek to {0}, which is longer than the fileSet", time));
					return false;
				}
			}
			Log.Debug (string.Format ("New time: {0}", time));

			return AbsoluteSeek (time, accurate, synchronous, throttled);
		}

		/// <summary>
		/// Updates the cameras configuration internally without applying the new
		/// configuration in the <see cref="IMultiVideoPlayer"/>.
		/// </summary>
		/// <param name="camerasConfig">The cameras configuration.</param>
		/// <param name="layout">The cameras layout.</param>
		void UpdateCamerasConfig (RangeObservableCollection<CameraConfig> camerasConfig, object layout)
		{
			skipApplyCamerasConfig = true;
			CamerasConfig = camerasConfig;
			CamerasLayout = layout;
			skipApplyCamerasConfig = false;
		}

		/// <summary>
		/// Applies the current cameras configuration.
		/// </summary>
		void ApplyCamerasConfig ()
		{
			ValidateVisibleCameras ();
			UpdateZoom ();
			UpdatePar ();
			if (multiPlayer != null) {
				multiPlayer.ApplyCamerasConfig ();
			}
		}

		/// <summary>
		/// Updates the current zoom value.
		/// </summary>
		void UpdateZoom ()
		{
			if (CamerasConfig.Count == 0) {
				PlayerVM.Zoom = 1;
				return;
			}

			CameraConfig cfg = CamerasConfig [0];
			MediaFile file = FileSet [cfg.Index];

			if (cfg.RegionOfInterest.Empty) {
				PlayerVM.Zoom = 1;
			} else {
				PlayerVM.Zoom = (float)(file.VideoWidth / cfg.RegionOfInterest.Width);
			}
		}

		/// <summary>
		/// Validates that the list of visible cameras indexes are consistent with fileset
		/// </summary>
		void ValidateVisibleCameras ()
		{
			if (FileSet == null || FileSet.Count == 0) {
				Log.Error ("Invalid FileSet, cannot validate cameras");
				return;
			}
			bool changed = false;

			List<CameraConfig> cameras = CamerasConfig?.ToList ();
			int maxCameras = Math.Min (4, FileSet.Count);
			if (cameras == null) {
				Log.Warning ("Empty cameras configuration, creating list of cameras from FileSet");
				cameras = new List<CameraConfig> ();
				for (int i = 0; i < maxCameras; i++) {
					cameras.Add (new CameraConfig (i));
				}
				changed = true;
			}

			bool invalidIndexes = cameras.Any (c => c.Index >= FileSet.Count ());
			if (cameras.Count < maxCameras || invalidIndexes) {
				Log.Warning ("Invalid cameras configuration, fixing list of cameras");
				if (invalidIndexes) {
					cameras = cameras.Where (i => i.Index < FileSet.Count).ToList ();
				}
				for (int i = cameras.Count (); i < maxCameras; i++) {
					cameras.Add (new CameraConfig (i));
				}
				changed = true;
			}

			if (changed) {
				UpdateCamerasConfig (
					new RangeObservableCollection<CameraConfig> (cameras),
					CamerasLayout);
			}

		}

		/// <summary>
		/// Updates the pixel aspect ration in all the view ports.
		/// </summary>
		void UpdatePar ()
		{
			if (ViewPorts == null) {
				return;
			}

			for (int i = 0; i < Math.Min (CamerasConfig.Count, ViewPorts.Count); i++) {
				int index = CamerasConfig [i].Index;
				MediaFile file = FileSet [index];
				float par = 1;
				if (file.VideoHeight != 0) {
					par = (float)(file.VideoWidth * file.Par / file.VideoHeight);
				}
				ViewPorts [i].Ratio = par;
			}
		}

		/// <summary>
		/// Open the specified file set.
		/// </summary>
		/// <param name="fileSet">the files to open.</param>
		/// <param name="seek">If set to <c>true</c>, seeks to the beginning of the stream.</param>
		/// <param name="force">If set to <c>true</c>, opens the fileset even if it was already set.</param>
		/// <param name="play">If set to <c>true</c>, sets the player to play.</param>
		/// <param name="defaultFile">If set to <c>true</c>, store this as the default file set to use.</param>
		void InternalOpen (MediaFileSet fileSet, bool seek, bool force = false, bool play = false, bool defaultFile = false)
		{
			Reset ();

			ShowMessageInViewPorts (null, false);
			if (fileSet == null || !fileSet.Any ()) {
				ShowMessageInViewPorts (Catalog.GetString ("No video available"), true);
				FileSet = fileSet;
				return;
			}

			if (defaultFile) {
				defaultFileSet = fileSet;
			}

			if ((fileSet != null && (!fileSet.Equals (FileSet) || fileSet.CheckMediaFilesModified (mediaFileSetCopy))) || force) {
				readyToSeek = false;
				FileSet = fileSet;
				UpdateDuration ();
				UpdateZoom ();
				UpdatePar ();
				PlayerVM.ControlsSensitive = true;
				try {
					Log.Debug ("Opening new file set " + fileSet);
					if (multiPlayer != null) {
						multiPlayer.Open (fileSet);
					} else {
						player.Open (fileSet [0]);
					}
				} catch (Exception ex) {
					Log.Exception (ex);
					//We handle this error async
				}
			}
			if (seek) {
				AbsoluteSeek (new Time (0), true);
			}
			if (play) {
				Play ();
			}
		}

		/// <summary>
		/// Reset the player segment information.
		/// </summary>
		void Reset ()
		{
			UpdatePlayingState (false);
			SetRate (1);
			StillImageLoaded = false;
			loadedSegment.Start = new Time (-1);
			loadedSegment.Stop = new Time (int.MaxValue);
		}

		/// <summary>
		/// Sets the rate and notifies the change.
		/// </summary>
		void SetRate (float rate)
		{
			if (rate < 0)
				rate = 1;
			player.Rate = rate;

			SetEventRate (rate);
			EmitRateChanged (rate);
		}

		/// <summary>
		/// Sets the event rate.
		/// </summary>
		/// <param name="rate">Rate.</param>
		void SetEventRate (float rate)
		{
			var evt = LoadedTimelineEvent;
			if (evt != null) {
				evt.Rate = rate;
			}
		}

		/// <summary>
		/// Loads a video segment defined by a <see cref="TimelineEvent"/> in the player.
		/// </summary>
		/// <param name="fileSet">File set.</param>
		/// <param name="start">Start time.</param>
		/// <param name="stop">Stop time.</param>
		/// <param name="seekTime">Position to seek after loading the segment.</param>
		/// <param name="rate">Playback rate.</param>
		/// <param name="camerasConfig">Cameras configuration.</param>
		/// <param name="camerasLayout">Cameras layout.</param>
		/// <param name="playing">If set to <c>true</c> starts playing.</param>
		void LoadSegment (MediaFileSet fileSet, Time start, Time stop, Time seekTime,
											float rate, RangeObservableCollection<CameraConfig> camerasConfig, object camerasLayout,
											bool playing)
		{
			Log.Debug (String.Format ("Update player segment {0} {1} {2}",
				start, stop, rate));
			if (!SegmentLoaded) {
				defaultCamerasConfig = CamerasConfig;
				defaultCamerasLayout = CamerasLayout;
			}

			UpdateCamerasConfig (camerasConfig, camerasLayout);

			if (fileSet != null && (!fileSet.Equals (mediafileSet) || fileSet.CheckMediaFilesModified (mediaFileSetCopy))) {
				InternalOpen (fileSet, false);
			} else {
				ApplyCamerasConfig ();
			}

			Pause ();
			loadedSegment.Start = start;
			loadedSegment.Stop = stop;
			StillImageLoaded = false;
			if (readyToSeek) {
				Log.Debug ("Player is ready to seek, seeking to " +
				seekTime.ToMSecondsString ());
				SetRate (rate);
				AbsoluteSeek (seekTime, true);
				if (playing) {
					Play ();
				}
			} else {
				Log.Debug ("Delaying seek until player is ready");
				pendingSeek = new PendingSeek {
					time = seekTime,
					rate = 1.0f,
					playing = playing,
					accurate = true,
				};
			}
		}

		void LoadStillImage (PlaylistImageVM image, bool playing)
		{
			loadedPlaylistElement = image;
			PlayerVM.ControlsSensitive = true;
			StillImageLoaded = true;
			if (playing) {
				Play ();
			}
		}

		void LoadFrameDrawing (PlaylistDrawingVM drawing, bool playing)
		{
			loadedPlaylistElement = drawing;
			StillImageLoaded = true;
			if (playing) {
				Play ();
			}
		}

		void LoadVideo (PlaylistVideoVM video, bool playing)
		{
			loadedPlaylistElement = video;
			MediaFileSet fileSet = new MediaFileSet ();
			fileSet.Add ((video.Model as PlaylistVideo).File);
			EmitLoadDrawings (null);
			UpdateCamerasConfig (video.CamerasConfig, video.CamerasLayout);
			InternalOpen (fileSet, true, true, playing);
		}

		void LoadPlayDrawing (FrameDrawing drawing)
		{
			Pause ();
			IgnoreTicks = true;
			player.Seek (drawing.Render, true, true);
			IgnoreTicks = false;
			EmitLoadDrawings (drawing);
		}

		/// <summary>
		/// Performs a step using the configured <see cref="Step"/> time.
		/// </summary>
		void PerformStep (Time step)
		{
			Time pos = CurrentTime + step;
			if (SegmentLoaded) {
				if (pos < loadedSegment.Start) {
					pos = loadedSegment.Start;
				} else if (pos > loadedSegment.Stop) {
					pos = loadedSegment.Stop;
				}
			} else {
				if (pos.MSeconds < 0) {
					pos.MSeconds = 0;
				} else if (pos >= duration) {
					pos = duration;
				}
			}
			Log.Debug (String.Format ("Stepping {0} seconds from {1} to {2}",
				step, CurrentTime, pos));
			AbsoluteSeek (pos, true);
		}

		/// <summary>
		/// Creates the backend video player.
		/// </summary>
		void CreatePlayer ()
		{
			try {
				player = multiPlayer = App.Current.MultimediaToolkit.GetMultiPlayer ();
				multiPlayer.ScopeChangedEvent += HandleScopeChangedEvent;
				supportsMultipleCameras = true;
			} catch {
				Log.Error ("Player with support for multiple cameras not found");
				player = App.Current.MultimediaToolkit.GetPlayer ();
				supportsMultipleCameras = false;
			}

			player.Error += HandleError;
			player.StateChange += HandleStateChange;
			player.Eos += HandleEndOfStream;
			player.ReadyToSeek += HandleReadyToSeek;
		}

		/// <summary>
		/// Reconfigures the timeout for the timer emitting the timming events.
		/// If set to <code>0</code>, the timer is topped
		/// </summary>
		/// <param name="mseconds">Mseconds.</param>
		void ReconfigureTimeout (uint mseconds)
		{
			if (mseconds == 0) {
				timer.Stop ();
			} else {
				timer.Interval = mseconds;
				timer.Start ();
			}
		}

		/// <summary>
		/// Called periodically to update the current time and check if and has reached
		/// its stop time, or drawings must been shonw.
		/// </summary>
		bool Tick (Time currentTime = null)
		{
			if (currentTime == null) {
				currentTime = CurrentTime;
			}

			if (StillImageLoaded) {
				Time relativeTime = currentTime;

				if (Mode == VideoPlayerOperationMode.Presentation) {
					relativeTime += LoadedPlaylist.GetCurrentStartTime ();
				}

				EmitTimeChanged (currentTime, relativeTime);

				if (imageLoadedTS >= loadedPlaylistElement.Duration) {
					Next ();
				} else {
					if (Playing) {
						imageLoadedTS.MSeconds += TIMEOUT_MS;
					}
				}

				return true;
			} else {
				Time relativeTime = currentTime;
				if (Mode == VideoPlayerOperationMode.Presentation) {
					relativeTime += LoadedPlaylist.GetCurrentStartTime ();
				}

				if (SegmentLoaded) {
					relativeTime -= loadedSegment.Start;

					EmitTimeChanged (currentTime, relativeTime);

					// When editing the duration of a PlaylistPlayElement we can go outside the boundaries
					// of the segment while seeking, so we need to ignore them to prevent jumping to the next
					// playlist element.
					if (!PlayerVM.EditEventDurationModeEnabled) {
						if (currentTime > loadedSegment.Stop) {
							/* Check if the segment is now finished and jump to next one */
							Next ();
						} else {
							var drawings = LoadedTimelineEvent?.Drawings;
							if (drawings != null) {
								/* Check if the event has drawings to display */
								var frameDrawing = drawings.FirstOrDefault (f => IsDrawingVisibleForCurrentTime (f, currentTime));
								if (frameDrawing != null) {
									LoadPlayDrawing (frameDrawing);
									drawingCurrentTime = CurrentTime;
								}
							} else {
								drawingCurrentTime = null;
							}
						}
					}
				} else {
					EmitTimeChanged (currentTime, relativeTime);
				}
				videoTS = currentTime;
				return true;
			}
		}

		bool IsDrawingVisibleForCurrentTime (FrameDrawing f, Time currentTime)
		{
			return f.CameraConfig.Index == camerasConfig [0].Index && (
				(currentTime == videoTS && f.Render == currentTime) ||
				f.Render > videoTS && f.Render <= currentTime) &&
					f != PlayerVM.FrameDrawing &&
					drawingCurrentTime != currentTime;
		}

		void UpdateDuration ()
		{
			Time absoluteDuration;
			if (Mode == VideoPlayerOperationMode.Presentation) {
				absoluteDuration = duration = LoadedPlaylist.Duration;
			} else {
				if (Mode == VideoPlayerOperationMode.Stretched) {
					absoluteDuration = FileSet?.VisibleRegion.Duration;
				} else {
					absoluteDuration = (FileSet?.CheckFiles () == true) ? FileSet.Duration : null;
				}

				if (StillImageLoaded) {
					duration = loadedPlaylistElement.Duration;
				} else if (SegmentLoaded) {
					duration = loadedSegment.Stop - loadedSegment.Start;
				} else {
					duration = absoluteDuration;
				}
			}
			if (PlayerVM != null) {
				PlayerVM.Duration = duration;
				PlayerVM.AbsoluteDuration = absoluteDuration;
			}
		}

		void ClipRoi (Area roi, MediaFile file)
		{
			Point st = roi.Start;
			st.X = Math.Max (st.X, 0);
			st.Y = Math.Max (st.Y, 0);
			st.X = Math.Min (st.X, (file.VideoWidth - roi.Width));
			st.Y = Math.Min (st.Y, (file.VideoHeight - roi.Height));
		}

		void IncreaseZoom ()
		{
			float? newLevel = App.Current.ZoomLevels.Where (l => l > PlayerVM.Zoom).OrderBy (l => l).FirstOrDefault ();
			if (newLevel != 0) {
				PlayerVM.SetZoomCommand.Execute (newLevel);
			}
		}

		void DecreaseZoom ()
		{
			float? newLevel = App.Current.ZoomLevels.Where (l => l < PlayerVM.Zoom).OrderByDescending (l => l).FirstOrDefault ();
			if (newLevel != 0) {
				PlayerVM.SetZoomCommand.Execute (newLevel);
			}
		}

		#endregion

		#region Backend Callbacks

		/* These callbacks are triggered by the multimedia backend and need to
		 * be deferred to the UI main thread */
		void HandleStateChange (PlaybackStateChangedEvent e)
		{
			App.Current.GUIToolkit.Invoke (delegate {
				if (e.Playing) {
					ReconfigureTimeout (TIMEOUT_MS);
				} else {
					if (!StillImageLoaded) {
						ReconfigureTimeout (0);
					}
				}
				if (!StillImageLoaded) {
					EmitPlaybackStateChanged (this, e.Playing);
				}
			});
		}

		void HandleReadyToSeek (object sender)
		{
			App.Current.GUIToolkit.Invoke (delegate {
				readyToSeek = true;
				if (pendingSeek != null) {
					SetRate (pendingSeek.rate);
					player.Seek (pendingSeek.time, pendingSeek.accurate, pendingSeek.syncrhonous);
					var playing = pendingSeek.playing;
					pendingSeek = null;
					if (playing) {
						Play ();
					}
				}
				Tick ();
				player.Expose ();
			});
		}

		void HandleEndOfStream (object sender)
		{
			App.Current.GUIToolkit.Invoke (delegate {
				if (loadedPlaylistElement is PlaylistVideo) {
					Next ();
				} else {
					Time position = null;
					if (loadedEvent != null) {
						Log.Debug ("Seeking back to event start");
						position = loadedEvent.Start;
					} else {
						Log.Debug ("Seeking back to 0");
						position = new Time (0);
					}
					AbsoluteSeek (position, true);
					Pause ();
				}
			});
		}

		void HandleError (object sender, string message)
		{
			App.Current.GUIToolkit.Invoke (delegate {
				App.Current.EventsBroker.Publish<MultimediaErrorEvent> (
					new MultimediaErrorEvent {
						Sender = sender,
						Message = message
					}
				);
			});
		}

		void HandleScopeChangedEvent (int index, bool visible)
		{
			if (!visible) {
				ViewPorts [index].Message = Catalog.GetString ("Out of scope");
			}
			ViewPorts [index].MessageVisible = !visible;
		}

		#endregion

		#region Callbacks

		void HandleKeyPressed (bool isRight)
		{
			if (playerVM.LoadedElement is TimelineEventVM) {
				var timelineEvent = playerVM.LoadedElement as TimelineEventVM;

				Time time = timelineEvent.SelectedGrabber ==
										   SelectionPosition.Left ? timelineEvent.Start : timelineEvent.Stop;

				playerVM.PauseCommand.Execute (false);

				if (isRight) {
					time.MSeconds += MSECONDS_STEP;
				} else {
					time.MSeconds -= MSECONDS_STEP;
				}
			}
		}

		void HandleTimeout (object sender, EventArgs e)
		{
			App.Current.GUIToolkit.Invoke (delegate {
				if (!IgnoreTicks) {
					Tick ();
				}
			});
		}

		void HandleSeekEvent (SeekType type, Time start, float rate)
		{
			App.Current.GUIToolkit.Invoke (delegate {
				EmitLoadDrawings (null);
				/* We only use it for backwards framestepping for now */
				if (type == SeekType.StepDown || type == SeekType.StepUp) {
					if (player.Playing)
						Pause ();
					if (type == SeekType.StepDown)
						player.SeekToPreviousFrame ();
					else
						player.SeekToNextFrame ();
					Tick ();
				}
				if (type == SeekType.Accurate || type == SeekType.Keyframe) {
					SetRate (rate);
					AbsoluteSeek (start, type == SeekType.Accurate, false, false);
				}
			});
		}

		void HandleMediaFileSetPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IsStretched" || e.PropertyName.StartsWith ("Collection")) {
				UpdateDuration ();
			}
		}

		void HandlePlaylistDurationChanged (object sender, PropertyChangedEventArgs e)
		{
			if (Mode == VideoPlayerOperationMode.Presentation) {
				UpdateDuration ();
			}
		}

		void HandleLoadedTimelineEventPropertyChangedEventHandler (object sender, PropertyChangedEventArgs e)
		{
			var seekTime = sender as Time;
			if (seekTime == null) {
				if (e.PropertyName == nameof (TimelineEvent.Start)) {
					seekTime = LoadedTimelineEvent.Start;
				} else if (e.PropertyName == nameof (TimelineEvent.Stop)) {
					seekTime = LoadedTimelineEvent.Stop;
				}
			}
			if (seekTime != null) {
				loadedSegment.Start = LoadedTimelineEvent.Start;
				loadedSegment.Stop = LoadedTimelineEvent.Stop;
				UpdateDuration ();
				AbsoluteSeek (seekTime, true, false, true);
			}
		}

		/// <summary>
		/// Handles the playlist event drawings collection changed, drawing on the player the changed drawing frame
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">CollectionChangedEventArgs.</param>
		void HandlePlaylistEventDrawingsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if ((e.Action == NotifyCollectionChangedAction.Add
				|| e.Action == NotifyCollectionChangedAction.Replace)
				&& e.NewItems.Count == 1) {
				Tick ();
			}
		}

		#endregion

		void ShowMessageInViewPorts (string message, bool show)
		{
			if (ViewPorts != null) {
				foreach (var viewPort in ViewPorts) {
					viewPort.Message = message;
					viewPort.MessageVisible = show;
				}
			}
		}

		void UpdatePlayingState (bool playing)
		{
			if (LoadedTimelineEvent != null) {
				LoadedTimelineEvent.Playing = playing;
			}
		}

		void HandlePlayerVMPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (PlayerVM.NeedsSync (e, nameof (PlayerVM.FileSet)) && PlayerVM.FileSet != null) {
				ValidateVisibleCameras ();
			}
		}
	}
}
