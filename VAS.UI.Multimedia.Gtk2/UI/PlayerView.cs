// PlayerBin.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Gdk;
using Gtk;
using Pango;
using VAS;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Handlers;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Drawing.Cairo;
using VAS.Drawing.Widgets;
using VAS.Services;
using VAS.Core.Hotkeys;
using Helpers = VAS.UI.Helpers;
using VASCommon = VAS.Core.Common;


namespace VAS.UI
{
	[System.ComponentModel.Category ("VAS")]
	[System.ComponentModel.ToolboxItem (true)]

	public partial class PlayerView : Gtk.Bin, IPlayerView
	{
		protected const int SCALE_FPS = 25;
		protected IPlayerController player;
		protected bool seeking, IsPlayingPrevState, muted, ignoreRate, ignoreVolume;
		protected double previousVLevel = 1;
		protected VolumeWindow vwin;
		protected Blackboard blackboard;
		protected PlayerViewOperationMode mode;
		protected Time duration;
		List<double> rateList;
		KeyContext keycontext;

		#region Constructors

		public PlayerView ()
		{
			this.Build ();

			closebuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-cancel-rec",
				StyleConf.PlayerCapturerIconSize);
			drawbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-draw",
				StyleConf.PlayerCapturerIconSize);
			playbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-play",
				StyleConf.PlayerCapturerIconSize);
			pausebuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-pause",
				StyleConf.PlayerCapturerIconSize);
			prevbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-rw",
				StyleConf.PlayerCapturerIconSize);
			nextbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-ff",
				StyleConf.PlayerCapturerIconSize);
			volumebuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-volume-hi",
				StyleConf.PlayerCapturerIconSize);
			detachbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-detach",
				StyleConf.PlayerCapturerIconSize);

			// Force tooltips to be translatable as there seems to be a bug in stetic 
			// code generation for translatable tooltips.
			ratescale.TooltipMarkup = Catalog.GetString ("Playback speed");
			closebutton.TooltipMarkup = Catalog.GetString ("Close loaded event");
			drawbutton.TooltipMarkup = Catalog.GetString ("Draw frame");
			playbutton.TooltipMarkup = Catalog.GetString ("Play");
			pausebutton.TooltipMarkup = Catalog.GetString ("Pause");
			prevbutton.TooltipMarkup = Catalog.GetString ("Previous");
			nextbutton.TooltipMarkup = Catalog.GetString ("Next");
			jumpspinbutton.TooltipMarkup = Catalog.GetString ("Jump in seconds. Hold the Shift key with the direction keys to activate it.");
			volumebutton.TooltipMarkup = Catalog.GetString ("Volume");
			detachbutton.TooltipMarkup = Catalog.GetString ("Detach window");

			//Set ratescale specific values
			ratescale.Adjustment.Upper = App.Current.UpperRate;
			ratescale.Adjustment.Lower = App.Current.LowerRate;
			ratescale.Adjustment.PageIncrement = App.Current.RatePageIncrement;
			ratescale.Adjustment.Value = App.Current.DefaultRate;
			rateList = App.Current.RateList;

			vwin = new VolumeWindow ();
			ConnectSignals ();
			blackboard = new Blackboard (new WidgetWrapper (blackboarddrawingarea));
			vbox3.NoShowAll = true;
			timescale.Adjustment.PageIncrement = 0.01;
			timescale.Adjustment.StepIncrement = 0.0001;
			Helpers.Misc.SetFocus (vbox3, false);
			videowindow.CanFocus = true;
			detachbutton.Clicked += (sender, e) => 
				App.Current.EventsBroker.Publish<DetachEvent> (new DetachEvent ());
			ratescale.ModifyFont (FontDescription.FromString (App.Current.Style.Font + " 8"));
			controlsbox.HeightRequest = StyleConf.PlayerCapturerControlsHeight;

			Player = new PlayerController ();
			Player.CamerasConfig = new ObservableCollection<CameraConfig> { new CameraConfig (0) };
			Player.Step = new Time { TotalSeconds = jumpspinbutton.ValueAsInt };
			Mode = PlayerViewOperationMode.Analysis;
			TogglePlayOnClick = true;
			CreateWindows ();
			ResetGui ();
		}

		#endregion

		protected override void OnUnrealized ()
		{
			player.Stop ();
			base.OnUnrealized ();
		}

		protected override void OnDestroyed ()
		{
			blackboard.Dispose ();
			player.Dispose ();
			base.OnDestroyed ();
		}

		#region Properties

		public virtual bool SupportsMultipleCameras {
			get {
				return false;
			}
		}

		public virtual bool PlayerAttached {
			set {
				if (value) {
					detachbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-attach",
						StyleConf.PlayerCapturerIconSize);
					detachbutton.TooltipMarkup = Catalog.GetString ("Attach window");
				} else {
					detachbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-detach",
						StyleConf.PlayerCapturerIconSize);
					detachbutton.TooltipMarkup = Catalog.GetString ("Detach window");
				}
			}
		}

		public virtual IPlayerController Player {
			get {
				return player;
			}
			set {
				if (player != null) {
					player.ElementLoadedEvent -= HandleElementLoadedEvent;
					player.LoadDrawingsEvent -= HandleLoadDrawingsEvent;
					player.PlaybackRateChangedEvent -= HandlePlaybackRateChangedEvent;
					player.PlaybackStateChangedEvent -= HandlePlaybackStateChangedEvent;
					player.TimeChangedEvent -= HandleTimeChangedEvent;
					player.VolumeChangedEvent -= HandleVolumeChangedEvent;
				}
				player = value;
				if (player != null) {
					player.ElementLoadedEvent += HandleElementLoadedEvent;
					player.LoadDrawingsEvent += HandleLoadDrawingsEvent;
					player.PlaybackRateChangedEvent += HandlePlaybackRateChangedEvent;
					player.PlaybackStateChangedEvent += HandlePlaybackStateChangedEvent;
					player.TimeChangedEvent += HandleTimeChangedEvent;
					player.VolumeChangedEvent += HandleVolumeChangedEvent;
				}
			}
		}

		public virtual PlayerViewOperationMode Mode {
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

		public virtual object CamerasLayout {
			get {
				return 0;
			}
			set {
			}
		}

		public virtual List<CameraConfig> CamerasConfig {
			get {
				return new List<CameraConfig> { new CameraConfig (0) };
			}
			set {
			}
		}

		public virtual bool TogglePlayOnClick {
			get;
			set;
		}

		#endregion

		#region Private methods

		protected virtual bool ControlsSensitive {
			set {
				controlsbox.Sensitive = value;
				ratescale.Sensitive = value;
			}
		}

		protected virtual bool ShowControls {
			set {
				controlsbox.Visible = value;
				ratescale.Visible = value;
			}
		}

		protected virtual bool Compact {
			set {
				prevbutton.Visible = nextbutton.Visible = jumplabel.Visible =
					jumpspinbutton.Visible = tlabel.Visible = timelabel.Visible =
						detachbutton.Visible = ratescale.Visible = !value;
			}
		}

		protected virtual bool DrawingsVisible {
			set {
				videowindow.Visible = !value;
				blackboarddrawingarea.Visible = value;
			}
		}

		protected virtual void ResetGui ()
		{
			if (mode != PlayerViewOperationMode.LiveAnalysisReview) {
				closebutton.Visible = false;
			}
			ControlsSensitive = true;
			DrawingsVisible = false;
			timescale.Value = 0;
			timelabel.Text = "";
			seeking = false;
			IsPlayingPrevState = false;
			muted = false;
			ignoreRate = false;
			ignoreVolume = false;
			videowindow.Visible = true;
		}

		protected virtual void ConnectSignals ()
		{
			vwin.VolumeChanged += new VolumeChangedHandler (OnVolumeChanged);
			closebutton.Clicked += HandleClosebuttonClicked;
			prevbutton.Clicked += HandlePrevbuttonClicked;
			nextbutton.Clicked += HandleNextbuttonClicked;
			playbutton.Clicked += HandlePlaybuttonClicked;
			pausebutton.Clicked += HandlePausebuttonClicked;
			drawbutton.Clicked += HandleDrawButtonClicked;
			volumebutton.Clicked += HandleVolumebuttonClicked;
			timescale.ValueChanged += HandleTimescaleValueChanged;
			timescale.ButtonPressEvent += HandleTimescaleButtonPress;
			timescale.ButtonReleaseEvent += HandleTimescaleButtonRelease;
			ratescale.FormatValue += HandleRateFormatValue;
			ratescale.ValueChanged += HandleRateValueChanged;
			jumpspinbutton.ValueChanged += HandleJumpValueChanged;
		}

		protected virtual void LoadImage (VAS.Core.Common.Image image, FrameDrawing drawing)
		{
			if (image == null) {
				DrawingsVisible = false;
				return;
			}
			blackboard.Background = image;
			blackboard.Drawing = drawing;
			DrawingsVisible = true;
			blackboarddrawingarea.QueueDraw ();
		}

		float GetRateFromScale ()
		{
			double val = rateList [(int)ratescale.Value - (int)App.Current.LowerRate];
			return (float)val;
		}

		protected virtual void CreateWindows ()
		{
			videowindow.ButtonPressEvent += OnVideoboxButtonPressEvent;
			videowindow.ScrollEvent += OnVideoboxScrollEvent;
			videowindow.ReadyEvent += HandleReady;
			videowindow.ExposeEvent += HandleExposeEvent;
			videowindow.CanFocus = true;
		}

		protected virtual void SetVolumeIcon (string name)
		{
			volumebuttonimage.Pixbuf = Helpers.Misc.LoadIcon (name, IconSize.Button, 0);
		}

		protected virtual void UpdateTime (Time currentTime, Time duration)
		{
			timelabel.Text = currentTime.ToMSecondsString (true) + "/" + duration.ToMSecondsString ();
			if (duration.MSeconds == 0) {
				timescale.Value = 0;
			} else {
				timescale.Value = (double)currentTime.MSeconds / duration.MSeconds;
			}
		}

		#endregion

		#region ControllerCallbacks

		protected virtual void HandleVolumeChangedEvent (double level)
		{
			/* Unused: volume is retrieved before launching the volume window */
		}

		protected virtual void HandleTimeChangedEvent (Time currentTime, Time duration, bool seekable)
		{
			if (seeking)
				return;

			this.duration = duration;
			UpdateTime (currentTime, duration);
			timescale.Sensitive = seekable;
		}

		protected virtual void HandlePlaybackStateChangedEvent (PlaybackStateChangedEvent e)
		{
			if (e.Playing) {
				playbutton.Hide ();
				pausebutton.Show ();
			} else {
				playbutton.Show ();
				pausebutton.Hide ();
			}
		}

		protected virtual void HandlePlaybackRateChangedEvent (float rate)
		{
			ignoreRate = true;
			int index = App.Current.RateList.FindIndex (p => (float)p == rate);
			ratescale.Value = index + App.Current.LowerRate;				
			ignoreRate = false;
		}

		protected virtual void HandleLoadDrawingsEvent (FrameDrawing frameDrawing)
		{
			if (frameDrawing != null) {
				LoadImage (Player.CurrentFrame, frameDrawing);
			} else {
				DrawingsVisible = false;
			}
		}

		protected virtual void HandleElementLoadedEvent (object element, bool hasNext)
		{
			if (element == null) {
				DrawingsVisible = false;
				if (Mode != PlayerViewOperationMode.LiveAnalysisReview) {
					closebutton.Visible = false;
				}
			} else {
				nextbutton.Sensitive = hasNext;
				closebutton.Visible = true;
				if (element is PlaylistDrawing) {
					PlaylistDrawing drawing = element as PlaylistDrawing;
					LoadImage (null, drawing.Drawing);
				} else if (element is PlaylistImage) {
					PlaylistImage image = element as PlaylistImage;
					LoadImage (image.Image, null);
				}
			}
		}

		#endregion

		#region UI Callbacks

		protected virtual void HandleExposeEvent (object sender, ExposeEventArgs args)
		{
			Player.Expose ();
			/* The player draws over the eventbox when it's resized
			 * so make sure that we queue a draw in the event box after
			 * the expose */
			lightbackgroundeventbox.QueueDraw ();
		}

		[GLib.ConnectBefore]
		protected virtual void HandleTimescaleButtonPress (object o, Gtk.ButtonPressEventArgs args)
		{
			if (args.Event.Button == 1) {
				args.Event.SetButton (2);
			} else {
				args.Event.SetButton (1);
			}

			if (!seeking) {
				seeking = true;
				IsPlayingPrevState = Player.Playing;
				Player.IgnoreTicks = true;
				Player.Pause ();
			}
		}

		[GLib.ConnectBefore]
		protected virtual void HandleTimescaleButtonRelease (object o, Gtk.ButtonReleaseEventArgs args)
		{
			if (args.Event.Button == 1) {
				args.Event.SetButton (2);
			} else {
				args.Event.SetButton (1);
			}

			if (seeking) {
				seeking = false;
				Player.IgnoreTicks = false;
				if (IsPlayingPrevState)
					Player.Play ();
			}
		}

		protected virtual void HandleTimescaleValueChanged (object sender, System.EventArgs e)
		{
			if (seeking) {
				Player.Seek (timescale.Value);
				UpdateTime (duration * timescale.Value, duration); 
			}
		}

		protected virtual void HandlePlaybuttonClicked (object sender, System.EventArgs e)
		{
			Player.Play ();
		}

		protected virtual void HandleVolumebuttonClicked (object sender, System.EventArgs e)
		{
			vwin.SetLevel (Player.Volume);
			vwin.Show ();
		}

		protected virtual void OnVolumeChanged (double level)
		{
			double prevLevel;

			prevLevel = Player.Volume;
			if (prevLevel > 0 && level == 0) {
				SetVolumeIcon ("longomatch-control-volume-off");
			} else if (prevLevel > 0.5 && level <= 0.5) {
				SetVolumeIcon ("longomatch-control-volume-low");
			} else if (prevLevel <= 0.5 && level > 0.5) {
				SetVolumeIcon ("longomatch-control-volume-med");
			} else if (prevLevel < 1 && level == 1.0) {
				SetVolumeIcon ("longomatch-control-volume-hi");
			}
			Player.Volume = level;
			if (level == 0)
				muted = true;
			else
				muted = false;
		}

		protected virtual void HandlePausebuttonClicked (object sender, System.EventArgs e)
		{
			Player.Pause ();
		}

		protected virtual void HandleClosebuttonClicked (object sender, System.EventArgs e)
		{
			App.Current.EventsBroker.Publish<LoadEventEvent> (new LoadEventEvent ());
		}

		protected virtual void HandlePrevbuttonClicked (object sender, System.EventArgs e)
		{
			Player.Previous ();
		}

		protected virtual void HandleNextbuttonClicked (object sender, System.EventArgs e)
		{
			Player.Next ();
		}

		protected virtual void HandleRateFormatValue (object o, Gtk.FormatValueArgs args)
		{
			int val = (int)args.Value - (int)App.Current.LowerRate;
			if (rateList != null && val < rateList.Count) {
				args.RetVal = rateList [val] + "X";
			}
		}

		protected virtual void HandleRateValueChanged (object sender, System.EventArgs e)
		{
			float val = GetRateFromScale ();

			// Mute for rate != 1
			if (val != 1 && Player.Volume != 0) {
				previousVLevel = Player.Volume;
				Player.Volume = 0;
			} else if (val != 1 && muted)
				previousVLevel = 0;
			else if (val == 1)
				Player.Volume = previousVLevel;

			if (!ignoreRate) {
				Player.Rate = val;
				App.Current.EventsBroker.Publish<PlaybackRateChangedEvent> (
					new PlaybackRateChangedEvent {
						Value = val
					}
				);
			}
		}

		protected virtual void OnVideoboxButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
		{
			/* FIXME: The pointer is grabbed when the event box is clicked.
			 * Make sure to ungrab it in order to avoid clicks outisde the window
			 * triggering this callback. This should be fixed properly.*/
			Pointer.Ungrab (Gtk.Global.CurrentEventTime);
			if (TogglePlayOnClick) {
				Player.TogglePlay ();
			}
		}

		protected virtual void OnVideoboxScrollEvent (object o, Gtk.ScrollEventArgs args)
		{
			switch (args.Event.Direction) {
			case ScrollDirection.Down:
				Player.SeekToPreviousFrame ();
				break;
			case ScrollDirection.Up:
				Player.SeekToNextFrame ();
				break;
			case ScrollDirection.Left:
				Player.StepBackward ();
				break;
			case ScrollDirection.Right:
				Player.StepForward ();
				break;
			}
		}

		protected virtual void HandleDrawButtonClicked (object sender, System.EventArgs e)
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

		protected virtual void HandleJumpValueChanged (object sender, EventArgs e)
		{
			Player.Step = new Time (jumpspinbutton.ValueAsInt * 1000);
		}

		protected virtual void HandleReady (object sender, EventArgs e)
		{
			Player.ViewPorts = new List<IViewPort> { videowindow };
			Player.Ready ();
		}

		public virtual void ShowDetachButton (bool show)
		{
			this.detachbutton.Visible = show;
		}

		#endregion
	}
}
