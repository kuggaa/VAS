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
using System.Linq;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Resources;
using VAS.Core.Resources.Styles;
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
		double previousVolumeLevel = 100.0;

		public VideoPlayerVM ()
		{
			ViewPortsSwitchActive = true;
			CamerasConfig = new ObservableCollection<CameraConfig> ();
			AbsoluteCurrentTime = new Time (0);
			CurrentTime = new Time (0);
			Step = new Time { TotalSeconds = 10 };
			ShowDetachButton = true;
			ShowCenterPlayHeadButton = true;

			ShowZoomCommand = new LimitationCommand (VASFeature.Zoom.ToString (),
													 () => {
														 ShowZoom = true;
													 }) {
				Icon = App.Current.ResourcesLocator.LoadIcon ("vas-control-zoom", 15),
				ToolTipText = Catalog.GetString ("Zoom"),
			};

			SetZoomCommand = new LimitationCommand<float> (VASFeature.Zoom.ToString (), zoomLevel => Player.SetZoom (zoomLevel));

			EditEventDurationCommand = new Command<bool> (b => Player.SetEditEventDurationMode (b)) {
				Icon = App.Current.ResourcesLocator.LoadIcon (Icons.PlayerControlTrim, Sizes.PlayerCapturerIconSize),
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
		#region Commands
		/// <summary>
		/// Changes the volume of <see cref="VideoPlayerController"/>
		/// </summary>
		public Command<double> ChangeVolumeCommand { get; set; }

		/// <summary>
		/// Changes step value of <see cref="VideoPlayerController"/>
		/// </summary>
		public Command<double> ChangeStepCommand { get; set; }

		/// <summary>
		/// Sets rate value of <see cref="VideoPlayerController"/>, if it is different than 1, <see cref="VideoPlayerController"/> will be muted
		/// </summary>
		public Command<double> ChangeRateCommand { get; set; }

		/// <summary>
		/// Closes <see cref="VideoPlayerController"/>
		/// </summary>
		public Command CloseCommand { get; set; }

		/// <summary>
		/// Jump to the previous element / to the begining if a <see cref="IPlaylistElement"/> is loaded,
		/// to the beginning of the event if a <see cref="TimelineEvent"/> is loaded or
		/// to the beginning of the stream of no element is loaded.
		/// </summary>
		public Command PreviousCommand { get; set; }

		/// <summary>
		/// Invokes <see cref="Next"/> Method
		/// </summary>
		/// <value>The previous command.</value>
		public Command NextCommand { get; set; }

		/// <summary>
		/// Invokes <see cref="Play"/> Method
		/// </summary>
		/// <value>The play command.</value>
		public Command PlayCommand { get; set; }

		/// <summary>
		/// Invokes <see cref="Pause"/> Method
		/// </summary>
		/// <value>The pause command.</value>
		public Command<bool> PauseCommand { get; set; }

		/// <summary>
		/// Invokes <see cref="DrawFrame"/> Method
		/// </summary>
		/// <value>The draw command.</value>
		public Command DrawCommand { get; set; }

		/// <summary>
		/// Publishes DetachEvent
		/// </summary>
		/// <value>The detach command.</value>
		public Command DetachCommand { get; set; }

		/// <summary>
		/// Toggles ViewPortsSwitchActive boolean.
		/// </summary>
		/// <value>The view ports switch toggle command.</value>
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
		/// Seek the specified position. This position should be relative to whatever is loaded.
		/// There are 3 options:
		/// - Playing a video, no events loaded -> A seek needs to be relative
		/// 	to the video file, no adjustments needed.
		/// - Playing a loaded event (a presentation event or a single event)
		///  -> A seek needs to be relative to the current event.
		/// 	Event start is Time(0)
		/// - Playing a playlist (with presentationMode on) -> A seek needs to
		/// 	be relative to the full playlist duration.
		/// </summary>
		/// <value>The seek command.</value>
		public Command<VideoPlayerSeekOptions> SeekCommand { get; set; }

		/// <summary>
		/// Force a redraw of the last frame.
		/// </summary>
		/// <value>The expose command.</value>
		public Command ExposeCommand { get; set; }

		/// <summary>
		/// Change the Ready state of a player indicating that the View has configured correctly the ViewPorts and the player can start playback now.
		/// </summary>
		/// <value>The ready command.</value>
		public Command<bool> ReadyCommand { get; set; }

		/// <summary>
		///  Stops the <see cref="VideoPlayer"/>
		/// </summary>
		/// <value>The stop command.</value>
		public Command StopCommand { get; set; }

		/// <summary>
		/// Gets or sets the edit event duration command.
		/// This command enables or disables the duration edition mode that allows to edit the duration of the loaded
		/// <see cref="IPlaylistEventElement"/>.
		/// </summary>
		/// <value>The edit event duration command.</value>
		public Command EditEventDurationCommand { get; set; }

		/// <summary>
		/// Changes the playback state pause/playing.
		/// </summary>
		/// <value>The toggle play command.</value>
		public Command TogglePlayCommand { get; set; }

		/// <summary>
		/// Performs a seek to the previous frame.
		/// </summary>
		/// <value>The seek to previous frame command.</value>
		public Command SeekToPreviousFrameCommand { get; set; }

		/// <summary>
		/// Performs a seek to the next frame.
		/// </summary>
		/// <value>The seek to next frame command.</value>
		public Command SeekToNextFrameCommand { get; set; }

		/// <summary>
		/// Step the amount in <see cref="Step"/> backward.
		/// </summary>
		/// <value>The step backward command.</value>
		public Command StepBackwardCommand { get; set; }

		/// <summary>
		/// Step the amount in <see cref="Step"/> forward.
		/// </summary>
		/// <value>The step forward command.</value>
		public Command StepForwardCommand { get; set; }
		#endregion
		/// <summary>
		/// Indicates that viewports are shown or not
		/// </summary>
		/// <value><c>true</c> if view ports switch active; otherwise, <c>false</c>.</value>
		public bool ViewPortsSwitchActive {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.VideoPlayerVM"/> controls are sensitive.
		/// </summary>
		/// <value><c>true</c> if controls sensitive; otherwise, <c>false</c>.</value>
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

		/// <summary>
		/// Indicates if VideoPlayer is playing.
		/// </summary>
		/// <value><c>true</c> if playing; otherwise, <c>false</c>.</value>
		public bool Playing {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the current time, based on the current loaded segment.
		/// </summary>
		/// <value>The current time of the loaded segment</value>
		public Time CurrentTime {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the current absolute time, regardless of the currently loaded element.
		/// This changes at the same time CurrentTime changes, and does not emit changes to avoid
		/// too much cpu overhead. So, you should listen for CurrentTime property changes but access
		/// AbsoluteCurrentTime instead.
		/// </summary>
		/// <value>The current absolute time.</value>
		[PropertyChanged.DoNotNotify]
		public Time AbsoluteCurrentTime {
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

		/// <summary>
		/// Current Media File Set ViewModel
		/// </summary>
		/// <value>The file set.</value>
		public MediaFileSetVM FileSet {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the frame drawing.
		/// </summary>
		/// <value>The frame drawing.</value>
		public FrameDrawing FrameDrawing {
			get;
			set;
		}

		/// <summary>
		/// Gets the current frame shown in the video player.
		/// </summary>
		/// <value>The current frame.</value>
		[PropertyChanged.DoNotNotify]
		public Image CurrentFrame {
			get {
				return Player.CurrentFrame;
			}
		}

		/// <summary>
		/// Gets the <see cref="LoadedElement"/> loaded
		/// </summary>
		/// <value>The loaded element.</value>
		public IPlayable LoadedElement {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the view mode.
		/// </summary>
		/// <value>The view mode.</value>
		public PlayerViewOperationMode ViewMode {
			set;
			get;
		}

		/// <summary>
		/// Gets or sets the videoplayer mode.
		/// </summary>
		/// <value>The player mode.</value>
		public VideoPlayerOperationMode PlayerMode {
			set {
				Player.Mode = value;
			}
			get {
				return Player.Mode;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.VideoPlayerVM"/> supports multiple cameras.
		/// </summary>
		/// <value><c>true</c> if supports multiple cameras; otherwise, <c>false</c>.</value>
		public bool SupportsMultipleCameras {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.VideoPlayerVM"/> show detach button.
		/// </summary>
		/// <value><c>true</c> if show detach button; otherwise, <c>false</c>.</value>
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

		/// <summary>
		/// Gets or sets the video player controller.
		/// </summary>
		/// <value>The player.</value>
		public IVideoPlayerController Player {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.VideoPlayerVM"/> player attached.
		/// </summary>
		/// <value><c>true</c> if player attached; otherwise, <c>false</c>.</value>
		public bool PlayerAttached {
			set;
			get;
		}

		/// <summary>
		/// Sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.VideoPlayerVM"/> ignore ticks.
		/// </summary>
		/// <value><c>true</c> if ignore ticks; otherwise, <c>false</c>.</value>
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

		/// <summary>
		/// Sets the list of view ports in video player controller.
		/// </summary>
		/// <value>The view ports.</value>
		public List<IViewPort> ViewPorts {
			set {
				Player.ViewPorts = value;
			}
		}
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.VideoPlayerVM"/> prepare view.
		/// </summary>
		/// <value><c>true</c> if prepare view; otherwise, <c>false</c>.</value>
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

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:VAS.Core.ViewModel.VideoPlayerVM"/> is opened.
		/// </summary>
		/// <value><c>true</c> if opened; otherwise, <c>false</c>.</value>
		[PropertyChanged.DoNotNotify]
		public bool Opened {
			get {
				return Player.Opened;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.VideoPlayerVM"/> show zoom.
		/// </summary>
		/// <value><c>true</c> if show zoom; otherwise, <c>false</c>.</value>
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
		/// <summary>
		/// Open the specified fileSet.
		/// </summary>
		public void OpenFileSet (MediaFileSetVM fileset, bool play = false)
		{
			Player.Open (fileset?.Model, play);
		}

		/// <summary>
		/// Loads a timeline event.
		/// The file set for this event comes from <see cref="e.Fileset"/>
		/// </summary>
		/// <param name="evt">The timeline event.</param>
		/// <param name="playing">If set to <c>true</c> playing.</param>
		public void LoadEvent (TimelineEventVM evt, bool playing)
		{
			LoadedElement = evt as IPlayable;

			if (evt?.Duration.MSeconds == 0) {
				// These events don't have duration, we start playing as if it was a seek
				Player.Switch (null, null, null);
				Player.UnloadCurrentEvent ();
				Player.Seek (evt.EventTime, true);
				Player.Play ();
			} else {
				if (evt != null) {
					LoadEvent (evt, new Time (0), playing);
				} else if (Player != null) {
					Player.UnloadCurrentEvent ();
				}
			}
		}
		/// <summary>
		/// Loads a timeline event.
		/// The file set for this event comes from <see cref="e.Fileset"/>
		/// </summary>
		/// <param name="evt">The timeline event.</param>
		/// <param name="seekTime">Seek time.</param>
		/// <param name="playing">If set to <c>true</c> playing.</param>
		public void LoadEvent (TimelineEventVM evt, Time seekTime, bool playing)
		{
			LoadedElement = evt as IPlayable;
			Player.LoadEvent (evt, seekTime, playing);
		}

		/// <summary>
		/// Loads all <see cref="IPlayListElement"/> events
		/// </summary>
		/// <param name="events">Events.</param>
		/// <param name="playing">If set to <c>true</c> playing.</param>
		public void LoadEvents (IEnumerable<TimelineEventVM> events, bool playing)
		{
			PlaylistVM playlist = new PlaylistVM { Model = new Playlist () };

			var plays = events.Select (vm => new PlaylistPlayElementVM { Model = new PlaylistPlayElement (vm.Model) });

			playlist.ViewModels.AddRange (plays);

			Player.LoadPlaylistEvent (playlist, plays.FirstOrDefault (), playing);
		}

		/// <summary>
		/// Loads the specified playlist event.
		/// </summary>
		/// <param name="playlist">Playlist.</param>
		/// <param name="evt">Event.</param>
		/// <param name="playing">If set to <c>true</c> playing.</param>
		public void LoadPlaylistEvent (PlaylistVM playlist, IPlayable evt, bool playing)
		{
			Player?.LoadPlaylistEvent (playlist, evt, playing);
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

		#endregion

		#region private Methods

		void InitializeCommands ()
		{
			ShowZoomCommand = new LimitationCommand (VASFeature.Zoom.ToString (), () => { ShowZoom = true; });
			ConfigureCommand (ShowZoomCommand, Icons.PlayerControlZoom, Sizes.PlayerCapturerSmallIconSize, StyleConf.PlayerTooltipZoom);

			SetZoomCommand = new LimitationCommand<float> (VASFeature.Zoom.ToString (), zoomLevel => Player.SetZoom (zoomLevel));

			ChangeVolumeCommand = new Command<double> (level => {
				Player.Volume = level / 100;
			});
			ConfigureCommand (ChangeVolumeCommand, Icons.PlayerControlHigh, Sizes.PlayerCapturerIconSize, StyleConf.PlayerTooltipVolume);

			ChangeStepCommand = new Command<double> (val => {
				Player.SetStep (new Time { TotalSeconds = App.Current.StepList [(int)val] });
			});
			ConfigureCommand (ChangeStepCommand, Icons.PlayerControlJumps, Sizes.PlayerCapturerSmallIconSize, StyleConf.PlayerTooltipJumps);

			ChangeRateCommand = new Command<double> (val => {
				double rateLevel = App.Current.RateList [(int)val];
				// Mute for rate != 1
				if (rateLevel != 1 && Volume != 0) {
					previousVolumeLevel = Volume;
					Player.Volume = 0;
				} else if (rateLevel == 1)
					Player.Volume = previousVolumeLevel;

				Player.Rate = rateLevel;
			});
			ConfigureCommand (ChangeRateCommand, Icons.PlayerControlSpeedRate, Sizes.PlayerCapturerSmallIconSize, StyleConf.PlayerTooltipRate);

			CloseCommand = new Command (() => { LoadEvent (null, Playing); });
			ConfigureCommand (CloseCommand, Icons.PlayerControlCancelRec, Sizes.PlayerCapturerIconSize, StyleConf.PlayerTooltipClose);

			PreviousCommand = new Command (() => { Player.Previous (); });
			ConfigureCommand (PreviousCommand, Icons.PlayerControlRewind, Sizes.PlayerCapturerIconSize, StyleConf.PlayerTooltipPrevious);

			NextCommand = new Command (() => { Player.Next (); });
			ConfigureCommand (NextCommand, Icons.PlayerControlFastForward, Sizes.PlayerCapturerIconSize, StyleConf.PlayerTooltipNext);

			PlayCommand = new Command (() => { Player.Play (); });
			ConfigureCommand (PlayCommand, Icons.PlayerControlPlay, Sizes.PlayerCapturerIconSize, StyleConf.PlayerTooltipPlay);

			PauseCommand = new Command<bool> (sync => Player.Pause (sync));
			ConfigureCommand (PauseCommand, Icons.PlayerControlPause, Sizes.PlayerCapturerIconSize, StyleConf.PlayerTooltipPause);

			DrawCommand = new Command (() => Player.DrawFrame ());
			ConfigureCommand (DrawCommand, Icons.PlayerControlDraw, Sizes.PlayerCapturerIconSize, StyleConf.PlayerTooltipDraw);

			DetachCommand = new LimitationCommand (VASFeature.VideoDetach.ToString(), () => { App.Current.EventsBroker.Publish (new DetachEvent ()); });
			ConfigureCommand (DetachCommand, Icons.PlayerControlDetach, Sizes.PlayerCapturerIconSize, StyleConf.PlayerTooltipDetach);

			ViewPortsSwitchToggleCommand = new Command (() => {
				ViewPortsSwitchActive = !ViewPortsSwitchActive;
			});
			ConfigureCommand (ViewPortsSwitchToggleCommand, Icons.PlayerControlMulticam, Sizes.PlayerCapturerIconSize);

			SeekCommand = new Command<VideoPlayerSeekOptions> (seekOptions => {
				Player.Seek (seekOptions.Time, seekOptions.Accurate, seekOptions.Synchronous, seekOptions.Throttled);
			});

			ExposeCommand = new Command (() => Player.Expose ());

			ReadyCommand = new Command<bool> (ready => Player.Ready (ready));

			StopCommand = new Command (() => Player.Stop ());

			TogglePlayCommand = new Command (() => Player.TogglePlay ());

			SeekToPreviousFrameCommand = new Command (() => Player.SeekToPreviousFrame ());

			SeekToNextFrameCommand = new Command (() => Player.SeekToNextFrame ());

			StepBackwardCommand = new Command (() => Player.StepBackward ());

			StepForwardCommand = new Command (() => Player.StepForward ());
		}

		/// <summary>
		/// Sets the Icon found on ResourceLocator named <paramref name="icon"/> with size <paramref name="iconSize"/>
		/// Sets the tooltip text with string <paramref name="tooltip"/>
		/// </summary>
		/// <param name="icon">Icon.</param>
		/// <param name="iconSize">Icon size.</param>
		/// <param name="tooltip">Tooltip.</param>
		void ConfigureCommand (Command command, string icon, int iconSize = 0, string tooltip = "")
		{
			if (command == null) return;
			command.Icon = App.Current.ResourcesLocator.LoadIcon (icon, iconSize);
			command.ToolTipText = tooltip;
		}

		#endregion

		#endregion
	}
}

