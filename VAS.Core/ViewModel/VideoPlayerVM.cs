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

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// Player View Model, Creates it's own instance of player controller.
	/// Every view that needs to control the player should use this ViewModel instead of the
	/// PlayerController.
	/// </summary>
	public class VideoPlayerVM : ViewModelBase, IViewModel
	{

		public VideoPlayerVM ()
		{
			CamerasConfig = new ObservableCollection<CameraConfig> ();
			CurrentTime = new Time (0);
			Step = new Time { TotalSeconds = 10 };
			ShowDetachButton = true;
			ShowCenterPlayHeadButton = true;

			ShowZoomCommand = new LimitationCommand (VASFeature.Zoom.ToString (),
													 () => {
														 ShowZoom = true;
													 }) {
				Icon = App.Current.ResourcesLocator.LoadIcon ("vas-zoom", 15),
				ToolTipText = Catalog.GetString ("Zoom"),
			};

			SetZoomCommand = new LimitationCommand<float> (VASFeature.Zoom.ToString (), SetZoom);

			EditEventDurationCommand = new Command<bool> (b => Player.SetEditEventDurationMode (b)) {
				Icon = App.Current.ResourcesLocator.LoadIcon (StyleConf.PlayerControlTrim, StyleConf.PlayerCapturerIconSize),
				Text = Catalog.GetString ("Edit event duration"),
				ToolTipText = Catalog.GetString ("Edit event duration"),
				Executable = false,
			};

			InitializeCommands ();
			Duration = new Time (0);
			CurrentTime = new Time (0);
			EditEventDurationTimeNode = new TimeNodeVM ();
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			Player.Dispose ();
			Player = null;
		}

		public Command CloseCommand { get; set; }
		public Command PreviousCommand { get; set; }
		public Command NextCommand { get; set; }
		public Command PlayCommand { get; set; }
		public Command PauseCommand { get; set; }
		public Command DrawCommand { get; set; }
		public Command VolumeCommand { get; set; }
		public Command RateCommand { get; set; }
		public Command JumpsCommand { get; set; }
		public Command DetachCommand { get; set; }
		public Command ViewPortsSwitchToggleCommand { get; set; }

		/// <summary>
		/// Gets or sets the show zoom Limitation command.
		/// </summary>
		/// <value>The show zoom command.</value>
		public LimitationCommand ShowZoomCommand {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the set zoom Limitation command.
		/// </summary>
		/// <value>The set zoom command.</value>
		public LimitationCommand<float> SetZoomCommand {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the edit event duration command.
		/// This command enables or disables the duration edition mode that allows to edit the duration of the loaded
		/// <see cref="IPlaylistEventElement"/>.
		/// </summary>
		/// <value>The edit event duration command.</value>
		public Command EditEventDurationCommand { get; set; }

		public bool ViewPortsSwitchActive {
			get;
			set;
		} = true;

		public bool ControlsSensitive {
			get;
			set;
		}

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

		/// <summary>
		/// Gets or sets the current zoom level.
		/// This value is only used for display in the view. To change the zoom use <see cref="SetZoom"/>
		/// </summary>
		/// <value>The new zoom.</value>
		public float Zoom {
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

		/// <summary>
		/// The duration of the currently active element in the video player,
		/// which can be the whole media file's duration, the duration of the currently loaded
		/// timeline element or the duration of the playlist. This should be used in Views where the
		/// seekable region is limitied to the duration of the loaded element, for example a video
		/// player that wants to restrict seeks relative to the start and stop time of the loaded
		/// element.
		/// </summary>
		/// <value>The duration.</value>
		public Time Duration {
			get;
			set;
		}

		/// <summary>
		/// The absolute seekable duration, regardless of the currently loaded element. This should
		/// be used by Views that needs to display the full duration of the clip, like a timeline.
		/// </summary>
		/// <value>The duration.</value>
		public Time AbsoluteDuration {
			get;
			set;
		}

		public bool Seekable {
			get;
			set;
		}

		public MediaFileSetVM FileSet {
			get;
			set;
		}

		public FrameDrawing FrameDrawing {
			get;
			set;
		}

		[PropertyChanged.DoNotNotify]
		public Image CurrentFrame {
			get {
				return Player.CurrentFrame;
			}
		}

		public IPlaylistElement LoadedElement {
			get;
			set;
		}

		public PlayerViewOperationMode ViewMode {
			set;
			get;
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

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.VideoPlayerVM"/> show
		/// center playhead button is shown.
		/// </summary>
		/// <value><c>true</c> if show multiplayer button; otherwise, <c>false</c>.</value>
		public bool ShowCenterPlayHeadButton {
			get;
			set;
		}

		public IVideoPlayerController Player {
			get;
			set;
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

		/// <summary>
		/// Gets or sets the current step level.
		/// This value is only used for display in the view. To change the steps use <see cref="SetStep"/>
		/// </summary>
		/// <value>The new steps jump.</value>
		public Time Step {
			get;
			set;
		}

		public List<IViewPort> ViewPorts {
			set {
				Player.ViewPorts = value;
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

		/// <summary>
		/// Gets or sets the cameras layout.
		/// </summary>
		/// <value>The cameras layout.</value>
		public object CamerasLayout {
			get;
			set;
		}


		[PropertyChanged.DoNotNotify]
		public bool Opened {
			get {
				return Player.Opened;
			}
		}

		[PropertyChanged.DoNotCheckEquality]
		public bool ShowZoom {
			get;
			set;
		} = false;


		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.VideoPlayerVM"/> is
		/// in editing the loaded event duration.
		/// </summary>
		/// <value><c>true</c> if editing the event duration mode; otherwise, <c>false</c>.</value>
		[PropertyChanged.DoNotCheckEquality]
		public bool EditEventDurationModeEnabled {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the time node that defines the lower and upper boundaries used to edit the duration of
		/// the <see cref="IPlaylistEventElement"/> loaded.
		/// </summary>
		/// <value>The time node to edit event duration.</value>
		public TimeNodeVM EditEventDurationTimeNode {
			get;
			set;
		}

		#region methods

		#region Public Methods

		public void Expose ()
		{
			Player.Expose ();
		}

		public void Ready (bool ready)
		{
			Player.Ready (ready);
		}

		public void Play ()
		{
			Player.Play ();
		}

		public void Pause (bool synchronous = false)
		{
			Player.Pause (synchronous);
		}

		public void Stop ()
		{
			Player.Stop (false);
		}

		public void Seek (double pos)
		{
			Player.Seek (pos);
		}

		public void Seek (Time time, bool accurate = false, bool synchronous = false, bool throttled = false)
		{
			Player.Seek (time, accurate, synchronous, throttled);
		}

		public void Previous ()
		{
			Player.Previous ();
		}

		public void Next ()
		{
			Player.Next ();
		}

		public void TogglePlay ()
		{
			Player.TogglePlay ();
		}

		public void SeekToPreviousFrame ()
		{
			Player.SeekToPreviousFrame ();
		}

		public void SeekToNextFrame ()
		{
			Player.SeekToNextFrame ();
		}

		public void StepBackward ()
		{
			Player.StepBackward ();
		}

		public void StepForward ()
		{
			Player.StepForward ();
		}

		public void OpenFileSet (MediaFileSetVM fileset, bool play = false)
		{
			Player.Open (fileset?.Model, play);
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
			Player.LoadEvent (e, seekTime, playing);
		}

		public void LoadEvents (IEnumerable<TimelineEvent> events, bool playing)
		{
			Playlist playlist = new Playlist ();

			List<IPlaylistElement> list = events
				.Select (evt => new PlaylistPlayElement (evt))
				.OfType<IPlaylistElement> ()
				.ToList ();

			playlist.Elements = new RangeObservableCollection<IPlaylistElement> (list);
			Player.LoadPlaylistEvent (playlist, list.FirstOrDefault (), playing);
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
			Player.DrawFrame ();
		}

		/// <summary>
		/// Changes the current zoom value using a value that goes from 1 (100%) to 2(200%)
		/// </summary>
		/// <param name="zoomLevel">Zoom level.</param>
		void SetZoom (float zoomLevel)
		{
			Player.SetZoom (zoomLevel);
		}

		/// <summary>
		/// Moves the current RegionOfInterest for the active camera by the vector expressed between
		/// the origin of coordinaties and the <paramref name="diff"/> point.
		/// </summary>
		/// <param name="diff">Diff.</param>
		public void MoveROI (Point diff)
		{
			Player.MoveROI (diff);
		}

		/// <summary>
		/// Changes the cameras config in the player.
		/// </summary>
		/// <param name="cameras">Cameras.</param>
		public void SetCamerasConfig (ObservableCollection<CameraConfig> cameras)
		{
			Player.CamerasConfig = cameras;
		}

		/// <summary>
		/// Change the playback rate of the player.
		/// </summary>
		/// <param name="rate">Rate.</param>
		public void SetRate (double rate)
		{
			Player.Rate = rate;
		}

		/// <summary>
		/// Change the volume of the player.
		/// </summary>
		/// <param name="volume">Volume.</param>
		public void SetVolume (double volume)
		{
			Player.Volume = volume;
		}

		/// <summary>
		/// Changes the current player step value.
		/// </summary>
		/// <param name="steps">Steps.</param>
		public void SetStep (Time step)
		{
			Player.SetStep (step);
		}

		#endregion

		#region private Methods

		private void InitializeCommands ()
		{
			ShowZoomCommand = new LimitationCommand (VASFeature.Zoom.ToString (), () => { ShowZoom = true; });
			ShowZoomCommand.Icon = VideoPlayerResourceManager.LoadIcon (VideoPlayerResourceManager.Constants.Icons.ZOOM);
			ShowZoomCommand.ToolTipText = VideoPlayerResourceManager.LoadToolTip (VideoPlayerResourceManager.Constants.Tooltips.ZOOM);

			SetZoomCommand = new LimitationCommand<float> (VASFeature.Zoom.ToString (), SetZoom);

			//TODO: ChangeVolumeCommand 

			CloseCommand = new Command (() => {
				LoadEvent (null, Playing);
			});
			CloseCommand.Icon = VideoPlayerResourceManager.LoadIcon (VideoPlayerResourceManager.Constants.Icons.CLOSE);
			CloseCommand.ToolTipText = VideoPlayerResourceManager.LoadToolTip (VideoPlayerResourceManager.Constants.Tooltips.CLOSE);

			PreviousCommand = new Command (() => {
				Previous ();
			});
			PreviousCommand.Icon = VideoPlayerResourceManager.LoadIcon (VideoPlayerResourceManager.Constants.Icons.PREVIOUS);
			PreviousCommand.ToolTipText = VideoPlayerResourceManager.LoadToolTip (VideoPlayerResourceManager.Constants.Tooltips.PREVIOUS);

			NextCommand = new Command (() => {
				Next ();
			});
			NextCommand.Icon = VideoPlayerResourceManager.LoadIcon (VideoPlayerResourceManager.Constants.Icons.NEXT);
			NextCommand.ToolTipText = VideoPlayerResourceManager.LoadToolTip (VideoPlayerResourceManager.Constants.Tooltips.NEXT);

			PlayCommand = new Command (() => {
				Play ();
			});

			PlayCommand.Icon = VideoPlayerResourceManager.LoadIcon (VideoPlayerResourceManager.Constants.Icons.PLAY);
			PlayCommand.ToolTipText = VideoPlayerResourceManager.LoadToolTip (VideoPlayerResourceManager.Constants.Tooltips.PLAY);

			PauseCommand = new Command (() => {
				Pause ();
			});
			PauseCommand.Icon = VideoPlayerResourceManager.LoadIcon (VideoPlayerResourceManager.Constants.Icons.PAUSE);
			PauseCommand.ToolTipText = VideoPlayerResourceManager.LoadToolTip (VideoPlayerResourceManager.Constants.Tooltips.PAUSE);

			DrawCommand = new Command (() => {
				DrawFrame ();
			});
			DrawCommand.Icon = VideoPlayerResourceManager.LoadIcon (VideoPlayerResourceManager.Constants.Icons.DRAW);
			DrawCommand.ToolTipText = VideoPlayerResourceManager.LoadToolTip (VideoPlayerResourceManager.Constants.Tooltips.DRAW);

			VolumeCommand = new Command<ISliderView> (Show);
			VolumeCommand.Icon = VideoPlayerResourceManager.LoadIcon (VideoPlayerResourceManager.Constants.Icons.VOLUME);
			VolumeCommand.ToolTipText = VideoPlayerResourceManager.LoadToolTip (VideoPlayerResourceManager.Constants.Tooltips.VOLUME);

			RateCommand = new Command<ISliderView> (Show);
			RateCommand.Icon = VideoPlayerResourceManager.LoadIcon (VideoPlayerResourceManager.Constants.Icons.RATE);
			RateCommand.ToolTipText = VideoPlayerResourceManager.LoadToolTip (VideoPlayerResourceManager.Constants.Tooltips.RATE);

			JumpsCommand = new Command<ISliderView> (Show);
			JumpsCommand.Icon = VideoPlayerResourceManager.LoadIcon (VideoPlayerResourceManager.Constants.Icons.JUMPS);
			JumpsCommand.ToolTipText = VideoPlayerResourceManager.LoadToolTip (VideoPlayerResourceManager.Constants.Tooltips.JUMPS);

			DetachCommand = new Command (() => {
				App.Current.EventsBroker.Publish (new DetachEvent ());
			});
			DetachCommand.Icon = VideoPlayerResourceManager.LoadIcon (VideoPlayerResourceManager.Constants.Icons.DETACH);
			DetachCommand.ToolTipText = VideoPlayerResourceManager.LoadToolTip (VideoPlayerResourceManager.Constants.Tooltips.DETACH);

			ViewPortsSwitchToggleCommand = new Command (() => {
				ViewPortsSwitchActive = !ViewPortsSwitchActive;
			});
			ViewPortsSwitchToggleCommand.Icon = VideoPlayerResourceManager.LoadIcon (VideoPlayerResourceManager.Constants.Icons.VIEWPORTSSWITCH);

			//centerplayheadbutton.Clicked += HandleCenterPlayheadClicked;
			//timerule.CenterPlayheadClicked += HandleCenterPlayheadClicked;
		}

		private void Show (ISliderView sliderview)
		{
			sliderview.Show ();
		}

		#endregion

		#endregion
	}

	public static class VideoPlayerResourceManager
	{
		static Dictionary<string, Image> _iconDictionary = new Dictionary<string, Image> () {
			{Constants.Icons.ZOOM, App.Current.ResourcesLocator.LoadIcon (Constants.Icons.ZOOM, StyleConf.PlayerCloseIconSize)},
			{Constants.Icons.CLOSE, App.Current.ResourcesLocator.LoadIcon (Constants.Icons.CLOSE, StyleConf.PlayerCapturerIconSize)},
			{Constants.Icons.PREVIOUS, App.Current.ResourcesLocator.LoadIcon (Constants.Icons.PREVIOUS, StyleConf.PlayerCapturerIconSize)},
			{Constants.Icons.NEXT, App.Current.ResourcesLocator.LoadIcon (Constants.Icons.NEXT, StyleConf.PlayerCapturerIconSize)},
			{Constants.Icons.PLAY, App.Current.ResourcesLocator.LoadIcon (Constants.Icons.PLAY, StyleConf.PlayerCapturerIconSize)},
			{Constants.Icons.PAUSE, App.Current.ResourcesLocator.LoadIcon (Constants.Icons.PAUSE, StyleConf.PlayerCapturerIconSize)},
			{Constants.Icons.DRAW, App.Current.ResourcesLocator.LoadIcon (Constants.Icons.DRAW, StyleConf.PlayerCapturerIconSize)},
			{Constants.Icons.VOLUME, App.Current.ResourcesLocator.LoadIcon (Constants.Icons.VOLUME, StyleConf.PlayerCapturerIconSize)},
			{Constants.Icons.RATE, App.Current.ResourcesLocator.LoadIcon (Constants.Icons.RATE, StyleConf.PlayerRateIconSize)},
			{Constants.Icons.JUMPS, App.Current.ResourcesLocator.LoadIcon (Constants.Icons.JUMPS, StyleConf.PlayerJumpsIconSize)},
			{Constants.Icons.DETACH, App.Current.ResourcesLocator.LoadIcon (Constants.Icons.DETACH, StyleConf.PlayerCapturerIconSize)},
			{Constants.Icons.VIEWPORTSSWITCH, App.Current.ResourcesLocator.LoadIcon (Constants.Icons.VIEWPORTSSWITCH, StyleConf.ViewPortsSwitchIconSize)},

		};

		public static Image LoadIcon (string iconConstant)
		{
			return (_iconDictionary.ContainsKey (iconConstant)) ?
				_iconDictionary [iconConstant]
					: null;
		}

		public static string LoadToolTip (string tooltipConstant)
		{
			return Catalog.GetString (tooltipConstant);
		}

		public struct Constants
		{
			public struct Icons
			{
				public const string ZOOM = "vas-zoom";
				public const string CLOSE = "vas-cancel-rec";
				public const string PREVIOUS = "vas-control-rw";
				public const string NEXT = "vas-control-ff";
				public const string PLAY = "vas-control-play";
				public const string PAUSE = "vas-control-pause";
				public const string DRAW = "vas-control-draw";
				public const string VOLUME = "vas-control-volume-hi";
				public const string RATE = "vas-speed";
				public const string JUMPS = "vas-jumps";
				public const string DETACH = "vas-control-detach";
				public const string VIEWPORTSSWITCH = "vas-multicam";

			}

			public struct Tooltips
			{
				public const string ZOOM = "Zoom";
				public const string CLOSE = "Close loaded event";
				public const string PREVIOUS = "Previous";
				public const string NEXT = "Next";
				public const string PLAY = "Play";
				public const string PAUSE = "Pause";
				public const string DRAW = "Draw Frame";
				public const string VOLUME = "Volume";
				public const string RATE = "Playback speed";
				public const string JUMPS = "Jump in seconds. Hold the Shift key with the direction keys to activate it.";
				public const string DETACH = "Detach window";
			}
		}
	}
}

