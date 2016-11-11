// PlayerView.cs
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
using System.Linq;
using Gdk;
using Gtk;
using Pango;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Handlers;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Drawing.Cairo;
using VAS.Drawing.Widgets;
using VAS.Services.ViewModel;


namespace VAS.UI
{
	[System.ComponentModel.Category ("VAS")]
	[System.ComponentModel.ToolboxItem (true)]

	public partial class PlayerView : Gtk.Bin, IView<PlayerVM>, IPlayerView
	{
		protected const int SCALE_FPS = 25;
		protected IPlayerController player;
		protected bool seeking, isPlayingPrevState, muted, ignoreRate, ignoreVolume;
		protected double previousVLevel = 1;
		protected VolumeWindow vwin;
		protected Blackboard blackboard;
		protected Time duration;
		List<double> rateList;
		KeyContext keycontext;
		List<IViewPort> viewPortsBackup;
		PlayerVM playerVM;

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

			TogglePlayOnClick = true;
			CreateWindows ();

		}

		#endregion

		protected override void OnUnrealized ()
		{
			playerVM.Stop ();
			base.OnUnrealized ();
		}

		protected override void OnRealized ()
		{
			if (playerVM != null) {
				playerVM.ViewPorts = viewPortsBackup;
			}
			base.OnRealized ();
		}

		protected override void OnDestroyed ()
		{
			blackboard.Dispose ();
			playerVM.Dispose ();
			base.OnDestroyed ();
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (PlayerVM)viewModel;
			ViewModel.SupportsMultipleCameras = false;
			SyncVMValues ();
		}

		public PlayerVM ViewModel {
			get {
				return playerVM;
			}
			set {
				if (playerVM != null) {
					playerVM.PropertyChanged -= PlayerVMPropertyChanged;
				}
				playerVM = value;
				if (playerVM != null) {
					playerVM.PropertyChanged += PlayerVMPropertyChanged;
					playerVM.Mode = PlayerViewOperationMode.Analysis;
					playerVM.Step = new Time { TotalSeconds = jumpspinbutton.ValueAsInt };
					playerVM.ViewPorts = viewPortsBackup;
					playerVM.SetCamerasConfig (new ObservableCollection<CameraConfig> { new CameraConfig (0) });
					ResetGui ();
				}
			}
		}



		#region Properties

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



		protected virtual bool DrawingsVisible {
			set {
				videowindow.Visible = !value;
				blackboarddrawingarea.Visible = value;
			}
		}

		protected virtual void ResetGui ()
		{
			if (playerVM.Mode != PlayerViewOperationMode.LiveAnalysisReview) {
				closebutton.Visible = false;
			}

			prevbutton.Visible = nextbutton.Visible = jumplabel.Visible =
				jumpspinbutton.Visible = tlabel.Visible = timelabel.Visible =
					detachbutton.Visible = ratescale.Visible = !playerVM.Compact;

			controlsbox.Sensitive = ratescale.Sensitive = playerVM.ControlsSensitive;
			drawbutton.Visible = playerVM.ShowDrawingIcon;
			DrawingsVisible = false;
			timescale.Value = 0;
			timelabel.Text = "";
			seeking = false;
			isPlayingPrevState = false;
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
			ratescale.ButtonPressEvent += HandleRatescaleButtonPress;
			ratescale.ButtonReleaseEvent += HandleRatescaleButtonRelease;
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

		protected virtual void UpdateTime ()
		{
			if (playerVM.CurrentTime != null && playerVM.Duration != null) {
				timelabel.Text = playerVM.CurrentTime.ToMSecondsString (true) + "/" + playerVM.Duration.ToMSecondsString ();
				if (playerVM.Duration.MSeconds == 0) {
					timescale.Value = 0;
				} else {
					timescale.Value = (double)playerVM.CurrentTime.MSeconds / playerVM.Duration.MSeconds;
				}
			}
		}

		#endregion

		#region UI Callbacks

		protected virtual void HandleExposeEvent (object sender, ExposeEventArgs args)
		{
			playerVM.Expose ();
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
				isPlayingPrevState = playerVM.Playing;
				playerVM.IgnoreTicks = true;
				playerVM.Pause ();
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
				playerVM.IgnoreTicks = false;
				if (isPlayingPrevState)
					playerVM.Play ();
			}
		}

		protected virtual void HandleTimescaleValueChanged (object sender, System.EventArgs e)
		{
			if (seeking) {
				playerVM.Seek (timescale.Value);
				playerVM.CurrentTime = playerVM.Duration * timescale.Value;
				UpdateTime ();
			}
		}

		protected virtual void HandlePlaybuttonClicked (object sender, System.EventArgs e)
		{
			playerVM.Play ();
		}

		protected virtual void HandleVolumebuttonClicked (object sender, System.EventArgs e)
		{
			vwin.SetLevel (playerVM.Volume);
			vwin.Show ();
		}

		protected virtual void OnVolumeChanged (double level)
		{
			double prevLevel;
			prevLevel = playerVM.Volume;
			if (prevLevel > 0 && level == 0) {
				SetVolumeIcon ("longomatch-control-volume-off");
			} else if (prevLevel > 0.5 && level <= 0.5) {
				SetVolumeIcon ("longomatch-control-volume-low");
			} else if (prevLevel <= 0.5 && level > 0.5) {
				SetVolumeIcon ("longomatch-control-volume-med");
			} else if (prevLevel < 1 && level == 1.0) {
				SetVolumeIcon ("longomatch-control-volume-hi");
			}
			playerVM.Volume = level;
			if (level == 0)
				muted = true;
			else
				muted = false;
		}

		protected virtual void HandlePausebuttonClicked (object sender, System.EventArgs e)
		{
			playerVM.Pause ();
		}

		protected virtual void HandleClosebuttonClicked (object sender, System.EventArgs e)
		{
			playerVM.LoadEvent (null, playerVM.Playing);
		}

		protected virtual void HandlePrevbuttonClicked (object sender, System.EventArgs e)
		{
			playerVM.Previous ();
		}

		protected virtual void HandleNextbuttonClicked (object sender, System.EventArgs e)
		{
			playerVM.Next ();
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
			if (val != 1 && playerVM.Volume != 0) {
				previousVLevel = playerVM.Volume;
				playerVM.Volume = 0;
			} else if (val != 1 && muted)
				previousVLevel = 0;
			else if (val == 1)
				playerVM.Volume = previousVLevel;

			if (!ignoreRate) {
				playerVM.SetRate (val);

			}
		}

		/// <summary>
		/// Handles the ratescale button press.
		/// Default button 1 action is used in button 2 and button 3 
		/// </summary>
		/// <param name="o">source</param>
		/// <param name="args">Arguments.</param>
		[GLib.ConnectBefore]
		protected virtual void HandleRatescaleButtonPress (object o, Gtk.ButtonPressEventArgs args)
		{
			if (args.Event.Button == 1) {
				args.Event.SetButton (2);
			} else {
				args.Event.SetButton (1);
			}
		}

		/// <summary>
		/// Handles the ratescale button release.
		/// Default button 1 action is used in button 2 and button 3 
		/// </summary>
		/// <param name="o">source</param>
		/// <param name="args">Arguments.</param>
		[GLib.ConnectBefore]
		protected virtual void HandleRatescaleButtonRelease (object o, Gtk.ButtonReleaseEventArgs args)
		{
			if (args.Event.Button == 1) {
				args.Event.SetButton (2);
			} else {
				args.Event.SetButton (1);
			}
		}

		protected virtual void OnVideoboxButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
		{
			/* FIXME: The pointer is grabbed when the event box is clicked.
			 * Make sure to ungrab it in order to avoid clicks outisde the window
			 * triggering this callback. This should be fixed properly.*/
			Pointer.Ungrab (Gtk.Global.CurrentEventTime);
			if (TogglePlayOnClick) {
				playerVM.TogglePlay ();
			}
		}

		protected virtual void OnVideoboxScrollEvent (object o, Gtk.ScrollEventArgs args)
		{
			switch (args.Event.Direction) {
			case ScrollDirection.Down:
				playerVM.SeekToPreviousFrame ();
				break;
			case ScrollDirection.Up:
				playerVM.SeekToNextFrame ();
				break;
			case ScrollDirection.Left:
				playerVM.StepBackward ();
				break;
			case ScrollDirection.Right:
				playerVM.StepForward ();
				break;
			}
		}

		protected virtual void HandleDrawButtonClicked (object sender, System.EventArgs e)
		{
			playerVM.DrawFrame ();
		}

		protected virtual void HandleJumpValueChanged (object sender, EventArgs e)
		{
			playerVM.Step = new Time (jumpspinbutton.ValueAsInt * 1000);
		}

		protected virtual void HandleReady (object sender, EventArgs e)
		{
			viewPortsBackup = new List<IViewPort> { videowindow };
			playerVM.ViewPorts = viewPortsBackup;
			playerVM.Ready ();
		}

		#endregion

		void PlayerVMPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			SyncVMValues (e.PropertyName);
		}

		void HandlePlayerAttachedChanged ()
		{
			if (playerVM.PlayerAttached) {
				detachbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-attach",
					StyleConf.PlayerCapturerIconSize);
				detachbutton.TooltipMarkup = Catalog.GetString ("Attach window");
			} else {
				detachbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-detach",
					StyleConf.PlayerCapturerIconSize);
				detachbutton.TooltipMarkup = Catalog.GetString ("Detach window");
			}
		}

		void HandlePlayingChanged ()
		{
			if (playerVM.Playing) {
				playbutton.Hide ();
				pausebutton.Show ();
			} else {
				playbutton.Show ();
				pausebutton.Hide ();
			}
		}

		void HandlePlayElementChanged ()
		{
			if (playerVM.PlayElement == null) {
				DrawingsVisible = false;
				if (playerVM.Mode != PlayerViewOperationMode.LiveAnalysisReview) {
					playerVM.CloseButtonVisible = false;
				}
			} else {
				playerVM.CloseButtonVisible = true;
				if (playerVM.PlayElement is PlaylistDrawing) {
					PlaylistDrawing drawing = playerVM.PlayElement as PlaylistDrawing;
					LoadImage (null, drawing.Drawing);
				} else if (playerVM.PlayElement is PlaylistImage) {
					PlaylistImage image = playerVM.PlayElement as PlaylistImage;
					LoadImage (image.Image, null);
				}
			}
		}

		void SyncVMValues (string propertyName = null)
		{
			if (propertyName == null || propertyName == "ShowControls") {
				controlsbox.Visible = ratescale.Visible = playerVM.ShowControls;
			}
			if (propertyName == null || propertyName == "ControlsSensitive") {
				controlsbox.Sensitive = ratescale.Sensitive = playerVM.ControlsSensitive;
			}
			if (propertyName == null || propertyName == "Compact") {
				prevbutton.Visible = nextbutton.Visible = jumplabel.Visible =
					jumpspinbutton.Visible = tlabel.Visible = timelabel.Visible =
						detachbutton.Visible = ratescale.Visible = !playerVM.Compact;
			}
			if (propertyName == null || propertyName == "PlayerAttached") {
				HandlePlayerAttachedChanged ();
			}
			if (propertyName == null || propertyName == "ShowDetachButton") {
				detachbutton.Visible = playerVM.ShowDetachButton;
			}
			if (propertyName == null || propertyName == "ShowDrawingIcon") {
				drawbutton.Visible = playerVM.ShowDrawingIcon;
			}
			if (propertyName == null || propertyName == "Playing") {
				HandlePlayingChanged ();
			}
			if (propertyName == null || propertyName == "HasNext") {
				nextbutton.Sensitive = playerVM.HasNext;
			}
			if (propertyName == null || propertyName == "CloseButtonVisible") {
				closebutton.Visible = playerVM.CloseButtonVisible;
			}
			if (propertyName == null || propertyName == "Rate") {
				ignoreRate = true;
				int index = App.Current.RateList.FindIndex (p => (float)p == playerVM.Rate);
				ratescale.Value = index + App.Current.LowerRate;
				ignoreRate = false;
			}
			if (propertyName == null || propertyName == "Seekable") {
				timescale.Sensitive = playerVM.Seekable;
			}
			if (propertyName == null || propertyName == "Duration" || propertyName == "CurrentTime") {
				UpdateTime ();
			}
			if (propertyName == null || propertyName == "FrameDrawing") {
				if (playerVM.FrameDrawing != null) {
					LoadImage (playerVM.CurrentFrame, playerVM.FrameDrawing);
				} else {
					DrawingsVisible = false;
				}
			}
			if (propertyName == null || propertyName == "PlayElement") {
				HandlePlayElementChanged ();
			}
			if (propertyName == null || propertyName == "FileSet") {
				if (playerVM.FileSet == null || !playerVM.FileSet.Any ()) {
					playerVM.ControlsSensitive = false;
				} else {
					playerVM.ControlsSensitive = true;
				}
			}
		}
	}
}
