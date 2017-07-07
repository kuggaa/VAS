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
using System.ComponentModel;
using System.Linq;
using System.Threading;
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

		protected const int TIMEOUT_MS = 20;
		protected IVideoPlayer player;
		protected IMultiVideoPlayer multiPlayer;
		protected TimelineEvent loadedEvent;
		protected IPlaylistElement loadedPlaylistElement;
		protected List<IViewPort> viewPorts;
		protected ObservableCollection<CameraConfig> camerasConfig;
		protected ObservableCollection<CameraConfig> defaultCamerasConfig;
		protected object defaultCamerasLayout;
		protected MediaFileSet defaultFileSet;
		protected MediaFileSet mediafileSet;
		protected MediaFileSet mediaFileSetCopy;
		protected Time duration, videoTS, imageLoadedTS;
		protected bool readyToSeek, stillimageLoaded, ready;
		protected bool disposed, skipApplyCamerasConfig;
		protected Action delayedOpen;
		protected Seeker seeker;
		protected Segment loadedSegment;
		protected PendingSeek pendingSeek;
		protected readonly Timer timer;
		protected readonly ManualResetEvent TimerDisposed;
		protected bool active;

		VideoPlayerVM playerVM;
		TimeNode visibleRegion;
		VideoPlayerOperationMode mode;
		Playlist loadedPlaylist;
		object camerasLayout;
		bool supportsMultipleCameras;


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

		public VideoPlayerController ()
		{
			seeker = new Seeker ();
			seeker.SeekEvent += HandleSeekEvent;
			loadedSegment.Start = new Time (-1);
			loadedSegment.Stop = new Time (int.MaxValue);
			videoTS = new Time (0);
			imageLoadedTS = new Time (0);
			duration = new Time (0);
			timer = new Timer (HandleTimeout);
			TimerDisposed = new ManualResetEvent (false);
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
			timer.Dispose (TimerDisposed);
			TimerDisposed.WaitOne (200);
			TimerDisposed.Dispose ();
			player.Error -= HandleError;
			player.StateChange -= HandleStateChange;
			player.Eos -= HandleEndOfStream;
			player.ReadyToSeek -= HandleReadyToSeek;
			player.Dispose ();
			player = null;
			FileSet = null;
		}

		#endregion

		#region IPlayerController implementation

		public virtual bool IgnoreTicks {
			get;
			set;
		}

		public virtual ObservableCollection<CameraConfig> CamerasConfig {
			set {
				Log.Debug ("Updating cameras configuration: " + value);
				camerasConfig = value;
				playerVM.CamerasConfig = camerasConfig;
				if (defaultCamerasConfig == null) {
					defaultCamerasConfig = value;
				}
				if (loadedEvent != null && !(loadedEvent.CamerasConfig.SequenceEqualSafe (value))) {
					loadedEvent.CamerasConfig = new ObservableCollection<CameraConfig> (value);
				} else if (loadedPlaylistElement is PlaylistPlayElement) {
					(loadedPlaylistElement as PlaylistPlayElement).CamerasConfig =
						new ObservableCollection<CameraConfig> (value);
				}
				if (multiPlayer != null) {
					multiPlayer.CamerasConfig = camerasConfig;
				}
				if (!skipApplyCamerasConfig && Opened) {
					ApplyCamerasConfig ();
				}
			}
			get {
				if (camerasConfig != null) {
					return camerasConfig.Clone ();
				} else {
					return null;
				}
			}
		}

		public virtual object CamerasLayout {
			get {
				return camerasLayout;
			}

			set {
				camerasLayout = value;
				playerVM.CamerasLayout = value;
			}
		}

		public virtual List<IViewPort> ViewPorts {
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
			protected get {
				return viewPorts;
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
				return playerVM.Playing;
			}
			set {
				playerVM.Playing = value;
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
				mediaFileSetCopy = value.Clone ();
				if (mediafileSet != null) {
					visibleRegion = FileSet.VisibleRegion;
					mediafileSet.PropertyChanged += HandleMediaFileSetPropertyChanged;
				} else {
					visibleRegion = null;
				}
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

		public virtual Playlist LoadedPlaylist {
			get {
				return loadedPlaylist;
			}

			set {
				if (loadedPlaylist != null) {
					loadedPlaylist.PropertyChanged -= HandlePlaylistDurationChanged;
				}
				loadedPlaylist = value;
				if (loadedPlaylist != null) {
					loadedPlaylist.PropertyChanged += HandlePlaylistDurationChanged;
				}
			}
		}

		public virtual VideoPlayerOperationMode Mode {
			get {
				return mode;
			}
			set {
				mode = value;
				if (Mode == VideoPlayerOperationMode.Presentation && LoadedPlaylist == null) {
					throw new InvalidOperationException (VideoPlayerOperationMode.Presentation +
														 " mode can only be used with a playlist loaded");
				}
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

		public override void SetViewModel (IViewModel viewModel)
		{
			playerVM = viewModel as VideoPlayerVM;
			if (playerVM == null) {
				playerVM = ((IVideoPlayerDealer)viewModel).VideoPlayer;
			}
			playerVM.Player = this;
			playerVM.SupportsMultipleCameras = supportsMultipleCameras;
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
				playerVM.ControlsSensitive = false;
				ShowMessageInViewPorts (Catalog.GetString ("No video loaded"), true);
				UpdateDuration ();
				return;
			}

			IgnoreTicks = false;
			ShowMessageInViewPorts (null, false);
			if (ready) {
				InternalOpen (fileSet, true, true, play, true);
			} else {
				Log.Debug ("Player is not ready, delaying ...");
				delayedOpen = () => InternalOpen (FileSet, true, true, play, true);
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

		/// <summary>
		/// Seeks absolutely. This seek is the one that will go to the real player, made over the video file.
		/// </summary>
		/// <returns><c>true</c>, if seek was made correctly, <c>false</c> otherwise.</returns>
		/// <param name="time">Time in the video to seek.</param>
		/// <param name="accurate">If set to <c>true</c>, accurate seek.</param>
		/// <param name="synchronous">If set to <c>true</c>, synchronous seek.</param>
		/// <param name="throttled">If set to <c>true</c>, throttled seek.</param>
		protected virtual bool AbsoluteSeek (Time time, bool accurate, bool synchronous = false, bool throttled = false)
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

		protected virtual bool PlaylistSeek (Time time, bool accurate = false, bool synchronous = false, bool throttled = false)
		{
			if (loadedPlaylistElement == null) {
				return AbsoluteSeek (time, accurate, synchronous, throttled);
			}

			// if time is outside the currently loaded event
			var elementTuple = LoadedPlaylist.GetElementAtTime (time);
			var elementAtTime = elementTuple.Item1;
			var elementStart = elementTuple.Item2;
			if (elementAtTime != loadedPlaylistElement || (elementStart > time || elementStart + elementAtTime.Duration < time)) {
				if (elementAtTime == null) {
					Log.Debug (String.Format ("There is no playlist element at {0}.", time));
					return false;
				}
				LoadPlaylistEvent (LoadedPlaylist, elementAtTime, false);
			}

			time -= elementStart;

			var play = loadedPlaylistElement as PlaylistPlayElement;
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
			PerformStep (playerVM.Step);
		}

		public virtual void StepBackward ()
		{
			Log.Debug ("Step backward");
			if (StillImageLoaded) {
				return;
			}
			PerformStep (new Time (-playerVM.Step.MSeconds));
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

		public void UpdatePlayingState (bool playing)
		{
			if (loadedPlaylistElement != null) {
				loadedPlaylistElement.Playing = playing;
			}

			if (loadedEvent != null) {
				loadedEvent.Playing = playing;
			}
		}

		public virtual void Switch (TimelineEvent play, Playlist playlist, IPlaylistElement element)
		{
			UpdatePlayingState (false);

			loadedEvent = play;
			LoadedPlaylist = playlist;
			loadedPlaylistElement = element;

			UpdatePlayingState (true);
		}

		public virtual void LoadPlaylistEvent (Playlist playlist, IPlaylistElement element, bool playing)
		{
			Log.Debug (string.Format ("Loading playlist element \"{0}\"", element?.Description));

			if (!ready) {
				EmitPrepareViewEvent ();
				delayedOpen = () => LoadPlaylistEvent (playlist, element, playing);
				return;
			}

			if (playlist == null) {
				return;
			}

			Switch (null, playlist, element);

			if (element is PlaylistPlayElement) {
				PlaylistPlayElement ple = element as PlaylistPlayElement;
				LoadSegment (ple.Play.FileSet, ple.Play.Start, ple.Play.Stop,
					ple.Play.Start, ple.Rate, ple.CamerasConfig,
					ple.CamerasLayout, playing);
			} else if (element is PlaylistVideo) {
				LoadVideo (element as PlaylistVideo, playing);
			} else if (element is PlaylistImage) {
				LoadStillImage (element as PlaylistImage, playing);
			} else if (element is PlaylistDrawing) {
				LoadFrameDrawing (element as PlaylistDrawing, playing);
			}
			UpdateDuration ();
			UpdatePlayingState (true);
			LoadedPlaylist.SetActive (element);
			EmitElementLoaded (element, playlist);
		}

		public virtual void LoadEvent (TimelineEvent evt, Time seekTime, bool playing)
		{
			MediaFileSet fileSet = evt.FileSet;
			Log.Debug (string.Format ("Loading event \"{0}\" seek:{1} playing:{2}", evt.Name, seekTime, playing));

			if (!ready) {
				EmitPrepareViewEvent ();
				delayedOpen = () => LoadEvent (evt, seekTime, playing);
				return;
			}

			Switch (evt, null, null);

			if (evt.Start != null && evt.Stop != null) {
				LoadSegment (fileSet, evt.Start, evt.Stop, evt.Start + seekTime, evt.Rate,
					evt.CamerasConfig, evt.CamerasLayout, playing);
				if (loadedEvent == null) { // LoadSegment sometimes removes the loadedEvent
					loadedEvent = evt;
				}
			} else if (evt.EventTime != null) {
				AbsoluteSeek (evt.EventTime, true);
			} else {
				Log.Error ("Event does not have timing info: " + evt);
			}
			UpdateDuration ();
			EmitElementLoaded (evt, null);
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
				if (loadedPlaylistElement is PlaylistPlayElement) {
					start = (loadedPlaylistElement as PlaylistPlayElement).Play.Start;
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
			camerasConfig [camConfig.Index] = camConfig;
			if (multiPlayer != null) {
				multiPlayer.ApplyROI (camConfig);
			}
			UpdateZoom ();
		}

		public virtual void DrawFrame ()
		{
			TimelineEvent evt = loadedEvent;
			if (evt == null && loadedPlaylistElement is PlaylistPlayElement) {
				evt = (loadedPlaylistElement as PlaylistPlayElement).Play;
			}
			if (evt != null) {
				App.Current.EventsBroker.Publish<DrawFrameEvent> (
					new DrawFrameEvent {
						Play = evt,
						DrawingIndex = -1,
						CamConfig = CamerasConfig [0],
						Current = true
					}
				);
			} else {
				App.Current.EventsBroker.Publish<DrawFrameEvent> (
					new DrawFrameEvent {
						Play = null,
						DrawingIndex = -1,
						CamConfig = null,
						Current = true
					}
				);
			}
		}

		public void SetZoom (double zoomLevel)
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
			playerVM.Step = new Time { TotalSeconds = (int)step.TotalSeconds };
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

		#endregion

		#region IController

		public override void Start ()
		{
			base.Start ();
		}

		public override void Stop ()
		{
			base.Stop ();
			Stop (true);
		}

		public override IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return new KeyAction [] {
				new KeyAction (
					App.Current.HotkeysService.GetByName("PLAYER_RATE_INCREMENT"),
					FramerateUp
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName("PLAYER_RATE_DECREMENT"),
					FramerateDown
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName("PLAYER_RATE_MAX"),
					FramerateUpper
				),
				new KeyAction (App.Current.HotkeysService.GetByName("PLAYER_RATE_DEFAULT"),
					FramerateLower
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName("PLAYER_TOGGLE_PLAY"),
					TogglePlay
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName("PLAYER_SEEK_LEFT_SHORT"),
					() => SeekToPreviousFrame()
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName("PLAYER_SEEK_LEFT_LONG"),
					() => StepBackward()
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName("PLAYER_SEEK_RIGHT_SHORT"),
					() => SeekToNextFrame()
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName("PLAYER_SEEK_RIGHT_LONG"),
					() => StepForward()
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName("PLAYER_NEXT_ELEMENT"),
					Next
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName("PLAYER_PREVIOUS_ELEMENT"),
					() => Previous()
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName("OPEN_DRAWING_TOOL"),
					playerVM.DrawFrame
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName(PlaybackHotkeys.ZOOM_RESTORE),
					() => SetZoom (App.Current.ZoomLevels[0])
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName(PlaybackHotkeys.ZOOM_INCREASE),
					IncreaseZoom
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName(PlaybackHotkeys.ZOOM_INCREASE),
					DecreaseZoom
				),
			};
		}

		#endregion

		#region Signals

		protected virtual void EmitLoadDrawings (FrameDrawing drawing = null)
		{
			playerVM.FrameDrawing = drawing;
			if (LoadDrawingsEvent != null && !disposed) {
				LoadDrawingsEvent (drawing);
			}
		}

		protected virtual void EmitPrepareViewEvent ()
		{
			playerVM.PrepareView = true;
			if (PrepareViewEvent != null && !disposed) {
				PrepareViewEvent ();
			}
		}

		protected virtual void EmitElementLoaded (object element, Playlist playlist)
		{
			playerVM.HasNext = playlist != null ? playlist.HasNext () : false;
			playerVM.PlayElement = element;
			if (element is IPlaylistElement) {
				App.Current.EventsBroker.Publish (
					new PlaylistElementLoadedEvent {
						Playlist = playlist,
						Element = element as IPlaylistElement,
					}
				);
			} else {
				App.Current.EventsBroker.Publish (
					new EventLoadedEvent {
						TimelineEvent = element as TimelineEvent,
					}
				);
			}
			if (ElementLoadedEvent != null && !disposed) {
				ElementLoadedEvent (element, playerVM.HasNext);
			}
		}

		protected virtual void EmitEventUnloaded ()
		{
			EmitElementLoaded (null, null);
		}

		protected virtual void EmitRateChanged (float rate)
		{
			playerVM.Rate = rate;
			if (PlaybackRateChangedEvent != null && !disposed) {
				PlaybackRateChangedEvent (rate);
			}
		}

		protected virtual void EmitVolumeChanged (double volume)
		{
			playerVM.Volume = volume;
			if (VolumeChangedEvent != null && !disposed) {
				VolumeChangedEvent (volume);
			}
		}

		protected virtual void EmitTimeChanged (Time currentTime, Time relativeTime)
		{
			if (Mode == VideoPlayerOperationMode.Stretched) {
				currentTime = currentTime - visibleRegion.Start;
			}

			playerVM.CurrentTime = relativeTime;
			playerVM.Seekable = !StillImageLoaded;


			if (TimeChangedEvent != null && !disposed) {
				TimeChangedEvent (relativeTime, playerVM.Duration, !StillImageLoaded);
			}
			App.Current.EventsBroker.Publish (
				new PlayerTickEvent {
					Time = currentTime,
					RelativeTime = relativeTime
				}
			);
		}

		protected virtual void EmitPlaybackStateChanged (object sender, bool playing)
		{
			playerVM.Playing = playing;
			if (PlaybackStateChangedEvent != null && !disposed) {
				PlaybackStateChangedEvent playbackStateChangedEvent = new PlaybackStateChangedEvent {
					Sender = sender,
					Playing = playing
				};
				PlaybackStateChangedEvent (playbackStateChangedEvent);
				App.Current.EventsBroker.Publish<PlaybackStateChangedEvent> (playbackStateChangedEvent);
			}
		}

		protected virtual void EmitMediaFileSetLoaded (MediaFileSet fileSet, ObservableCollection<CameraConfig> camerasVisible)
		{
			playerVM.FileSet = new MediaFileSetVM { Model = fileSet };
			playerVM.CamerasConfig = camerasVisible;
			if (MediaFileSetLoadedEvent != null && !disposed) {
				MediaFileSetLoadedEvent (fileSet, camerasVisible);
			}
		}

		#endregion

		#region Private Properties

		/// <summary>
		/// Indicates if a still image is loaded instead of a video segment.
		/// </summary>
		protected virtual bool StillImageLoaded {
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
		protected virtual bool SegmentLoaded {
			get {
				return loadedSegment.Start.MSeconds != -1;
			}
		}

		/// <summary>
		/// Gets the list of drawing for the loaded event.
		/// </summary>
		protected virtual ObservableCollection<FrameDrawing> EventDrawings {
			get {
				if (loadedEvent != null) {
					return loadedEvent.Drawings;
				} else if (loadedPlaylistElement is PlaylistPlayElement) {
					return (loadedPlaylistElement as PlaylistPlayElement).Play.Drawings;
				}
				return null;
			}
		}


		#endregion

		#region Private methods

		/// <summary>
		/// Updates the cameras configuration internally without applying the new
		/// configuration in the <see cref="IMultiVideoPlayer"/>.
		/// </summary>
		/// <param name="camerasConfig">The cameras configuration.</param>
		/// <param name="layout">The cameras layout.</param>
		protected virtual void UpdateCamerasConfig (ObservableCollection<CameraConfig> camerasConfig, object layout)
		{
			skipApplyCamerasConfig = true;
			CamerasConfig = camerasConfig;
			CamerasLayout = layout;
			skipApplyCamerasConfig = false;
		}

		/// <summary>
		/// Applies the current cameras configuration.
		/// </summary>
		protected virtual void ApplyCamerasConfig ()
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
		protected virtual void UpdateZoom ()
		{
			if (CamerasConfig.Count == 0) {
				playerVM.Zoom = 1;
				return;
			}

			CameraConfig cfg = CamerasConfig [0];
			MediaFile file = FileSet [cfg.Index];

			if (cfg.RegionOfInterest.Empty) {
				playerVM.Zoom = 1;
			} else {
				playerVM.Zoom = file.VideoWidth / cfg.RegionOfInterest.Width;
			}
		}

		/// <summary>
		/// Validates that the list of visible cameras indexes are consistent with fileset
		/// </summary>
		protected virtual void ValidateVisibleCameras ()
		{
			if (FileSet != null && camerasConfig != null && camerasConfig.Select (c => c.Index).DefaultIfEmpty ().Max () >= FileSet.Count) {
				Log.Error ("Invalid cameras configuration, fixing list of cameras");
				UpdateCamerasConfig (
					new ObservableCollection<CameraConfig> (camerasConfig.Where (i => i.Index < FileSet.Count)),
					CamerasLayout);
			}
		}

		/// <summary>
		/// Updates the pixel aspect ration in all the view ports.
		/// </summary>
		protected virtual void UpdatePar ()
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
		protected virtual void InternalOpen (MediaFileSet fileSet, bool seek, bool force = false, bool play = false, bool defaultFile = false)
		{
			Reset ();

			// This event gives a chance to the view to define camera visibility.
			// As there might already be a configuration defined (loading an event for example), the view
			// should adapt if needed.
			skipApplyCamerasConfig = true;
			EmitMediaFileSetLoaded (fileSet, camerasConfig);
			skipApplyCamerasConfig = false;

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
				// Check if the view failed to configure a proper cam config
				if (CamerasConfig == null) {
					App.Current.EventsBroker.Publish<MultimediaErrorEvent> (
						new MultimediaErrorEvent {
							Sender = this,
							Message = Catalog.GetString ("Invalid camera configuration")
						}
					);
					FileSet = null;
					return;
				}
				ValidateVisibleCameras ();
				UpdateZoom ();
				UpdatePar ();
				playerVM.ControlsSensitive = true;
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
		protected virtual void Reset ()
		{
			UpdatePlayingState (false);
			SetRate (1);
			StillImageLoaded = false;
			loadedSegment.Start = new Time (-1);
			loadedSegment.Stop = new Time (int.MaxValue);
			loadedEvent = null;
		}

		/// <summary>
		/// Sets the rate and notifies the change.
		/// </summary>
		protected virtual void SetRate (float rate)
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
		protected virtual void SetEventRate (float rate)
		{
			if (loadedPlaylistElement is PlaylistPlayElement) {
				(loadedPlaylistElement as PlaylistPlayElement).Rate = rate;
			} else if (loadedEvent != null) {
				loadedEvent.Rate = rate;
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
		protected virtual void LoadSegment (MediaFileSet fileSet, Time start, Time stop, Time seekTime,
											float rate, ObservableCollection<CameraConfig> camerasConfig, object camerasLayout,
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

		protected virtual void LoadStillImage (PlaylistImage image, bool playing)
		{
			Reset ();
			loadedPlaylistElement = image;
			StillImageLoaded = true;
			if (playing) {
				Play ();
			}
		}

		protected virtual void LoadFrameDrawing (PlaylistDrawing drawing, bool playing)
		{
			loadedPlaylistElement = drawing;
			StillImageLoaded = true;
			if (playing) {
				Play ();
			}
		}

		protected virtual void LoadVideo (PlaylistVideo video, bool playing)
		{
			loadedPlaylistElement = video;
			MediaFileSet fileSet = new MediaFileSet ();
			fileSet.Add (video.File);
			EmitLoadDrawings (null);
			UpdateCamerasConfig (new ObservableCollection<CameraConfig> { new CameraConfig (0) }, null);
			InternalOpen (fileSet, true, true, playing);
		}

		protected virtual void LoadPlayDrawing (FrameDrawing drawing)
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
		protected virtual void PerformStep (Time step)
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
		protected virtual void CreatePlayer ()
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
		protected virtual void ReconfigureTimeout (uint mseconds)
		{
			if (mseconds == 0) {
				timer.Change (Timeout.Infinite, Timeout.Infinite);
			} else {
				timer.Change (mseconds, mseconds);
			}
		}

		/// <summary>
		/// Called periodically to update the current time and check if and has reached
		/// its stop time, or drawings must been shonw.
		/// </summary>
		protected virtual bool Tick (Time currentTime = null)
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

					if (currentTime > loadedSegment.Stop) {
						/* Check if the segment is now finished and jump to next one */
						Next ();
					} else {
						var drawings = EventDrawings;
						if (drawings != null) {
							/* Check if the event has drawings to display */
							FrameDrawing fd = drawings.FirstOrDefault (f => f.Render > videoTS &&
											  f.Render <= currentTime &&
											  f.CameraConfig.Index == CamerasConfig [0].Index);
							if (fd != null) {
								LoadPlayDrawing (fd);
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
			if (playerVM != null) {
				playerVM.Duration = duration;
				playerVM.AbsoluteDuration = absoluteDuration;
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
			double? newLevel = App.Current.ZoomLevels.Where (l => l > playerVM.Zoom).OrderBy (l => l).FirstOrDefault ();
			if (newLevel != 0) {
				SetZoom ((double)newLevel);
			}
		}

		void DecreaseZoom ()
		{
			double? newLevel = App.Current.ZoomLevels.Where (l => l < playerVM.Zoom).OrderByDescending (l => l).FirstOrDefault ();
			if (newLevel != 0) {
				SetZoom ((double)newLevel);
			}
		}

		#endregion

		#region Backend Callbacks

		/* These callbacks are triggered by the multimedia backend and need to
		 * be deferred to the UI main thread */
		protected virtual void HandleStateChange (PlaybackStateChangedEvent e)
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

		protected virtual void HandleReadyToSeek (object sender)
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

		protected virtual void HandleEndOfStream (object sender)
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

		protected virtual void HandleError (object sender, string message)
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

		protected virtual void HandleScopeChangedEvent (int index, bool visible)
		{
			if (!visible) {
				ViewPorts [index].Message = Catalog.GetString ("Out of scope");
			}
			ViewPorts [index].MessageVisible = !visible;
		}

		#endregion

		#region Callbacks

		protected virtual void HandleTimeout (Object state)
		{
			App.Current.GUIToolkit.Invoke (delegate {
				if (!IgnoreTicks) {
					Tick ();
				}
			});
		}

		protected virtual void HandleSeekEvent (SeekType type, Time start, float rate)
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

		protected void HandleMediaFileSetPropertyChanged (object sender, PropertyChangedEventArgs e)
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
	}
}
