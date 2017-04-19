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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Gdk;
using Gtk;
using Pango;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Handlers;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Drawing.Cairo;
using VAS.Drawing.Widgets;
using DConstants = VAS.Drawing.Constants;
using Image = VAS.Core.Common.Image;
using Misc = VAS.UI.Helpers.Misc;
using Point = VAS.Core.Common.Point;

namespace VAS.UI
{
	[Category ("LongoMatch")]
	[ToolboxItem (true)]
	[View ("VideoPlayerView")]
	public partial class VideoPlayerView : Bin, IView<VideoPlayerVM>, IVideoPlayerView
	{
		public event ClickedHandler CenterPlayheadClicked;

		const int SCALE_FPS = 25;
		bool muted, ignoreRate, drawingsVisible;
		bool roiMoved, wasPlaying;
		double previousVLevel = 1;
		uint dragTimerID;
		Blackboard blackboard;
		List<double> rateList;
		List<CameraConfig> cameraConfigsOutOfScreen;
		Point moveStart;
		Timerule timerule;
		VideoPlayerVM playerVM;
		VolumeWindow vwin;
		ZoomLevel zoomLevel;
		ZoomMenu zoomMenu;

		#region Constructors

		public VideoPlayerView ()
		{
			this.Build ();

			timerulearea.HeightRequest = DConstants.TIMERULE_PLAYER_HEIGHT;
			timerule = new Timerule (new WidgetWrapper (timerulearea)) {
				PlayerMode = true,
				ContinuousSeek = true,
				AutoUpdate = true,
				AdjustSizeToDuration = true,
			};

			closebuttonimage.Pixbuf = Misc.LoadIcon ("longomatch-cancel-rec",
				StyleConf.PlayerCapturerIconSize);
			drawbuttonimage.Pixbuf = Misc.LoadIcon ("longomatch-control-draw",
				StyleConf.PlayerCapturerIconSize);
			playbuttonimage.Pixbuf = Misc.LoadIcon ("longomatch-control-play",
				StyleConf.PlayerCapturerIconSize);
			pausebuttonimage.Pixbuf = Misc.LoadIcon ("longomatch-control-pause",
				StyleConf.PlayerCapturerIconSize);
			prevbuttonimage.Pixbuf = Misc.LoadIcon ("longomatch-control-rw",
				StyleConf.PlayerCapturerIconSize);
			nextbuttonimage.Pixbuf = Misc.LoadIcon ("longomatch-control-ff",
				StyleConf.PlayerCapturerIconSize);
			volumebuttonimage.Pixbuf = Misc.LoadIcon ("longomatch-control-volume-hi",
				StyleConf.PlayerCapturerIconSize);
			detachbuttonimage.Pixbuf = Misc.LoadIcon ("longomatch-control-detach",
				StyleConf.PlayerCapturerIconSize);
			viewportsSwitchImage.Pixbuf = Misc.LoadIcon ("longomatch-video-device",
				22);
			zoomLevelImage.Pixbuf = Misc.LoadIcon ("longomatch-search",
				22);
			centerplayheadbuttonimage.Pixbuf = Misc.LoadIcon ("longomatch-dash-center-view",
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
			centerplayheadbutton.TooltipMarkup = Catalog.GetString ("Center Playhead");

			vwin = new VolumeWindow ();
			zoomMenu = new ZoomMenu ();
			blackboard = new Blackboard (new WidgetWrapper (blackboarddrawingarea));
			blackboarddrawingarea.Visible = false;
			blackboarddrawingarea.NoShowAll = true;

			//Set ratescale specific values
			ratescale.Adjustment.Upper = App.Current.UpperRate;
			ratescale.Adjustment.Lower = App.Current.LowerRate;
			ratescale.Adjustment.PageIncrement = App.Current.RatePageIncrement;
			ratescale.Adjustment.Value = App.Current.DefaultRate;
			rateList = App.Current.RateList;

			ConnectSignals ();

			totalbox.NoShowAll = true;
			Misc.SetFocus (totalbox, false);
			mainviewport.CanFocus = true;

			maineventbox.ModifyBg (StateType.Normal, Misc.ToGdkColor (App.Current.Style.PaletteBackground));

			ratescale.ModifyFont (FontDescription.FromString (App.Current.Style.Font + " 8"));
			controlsbox.HeightRequest = StyleConf.PlayerCapturerControlsHeight;

			cameraConfigsOutOfScreen = new List<CameraConfig> ();

			CreateWindows ();
		}

		#endregion

		protected override void OnDestroyed ()
		{
			if (dragTimerID != 0) {
				GLib.Source.Remove (dragTimerID);
				dragTimerID = 0;
			}

			blackboard.Dispose ();
			timerule.Dispose ();

			base.OnDestroyed ();
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (VideoPlayerVM)viewModel;
		}

		public VideoPlayerVM ViewModel {
			get {
				return playerVM;
			}
			set {
				if (playerVM != null) {
					playerVM.PropertyChanged -= PlayerVMPropertyChanged;
				}
				playerVM = value;
				if (playerVM != null) {
					playerVM.Step = new Time { TotalSeconds = jumpspinbutton.ValueAsInt };
					timerule.ViewModel = playerVM;
					playerVM.PropertyChanged += PlayerVMPropertyChanged;
					Reset ();
					playerVM.Sync ();
				}
			}
		}

		#region Properties

		// FIXME: Used in presentations while MVVM is not implemented there
		public string VideoMessage {
			set {
				mainviewport.Message = value;
				subviewport1.Viewport.Message = value;
				subviewport2.Viewport.Message = value;
				subviewport3.Viewport.Message = value;
			}
		}

		// FIXME: Used in presentations while MVVM is not implemented there
		public bool ShowMessage {
			set {
				if (value == true) {
					DrawingsVisible = false;
				}
				mainviewport.MessageVisible = value;
				subviewport1.Viewport.MessageVisible = value;
				subviewport2.Viewport.MessageVisible = value;
				subviewport3.Viewport.MessageVisible = value;
			}
		}

		#endregion

		#region Private methods

		public bool SubViewPortsVisible {
			set {
				bool b = value && (ViewModel.FileSet == null || ViewModel.FileSet.ViewModels.Count > 1);
				viewportsSwitchButton.Visible = b;
				subviewportsbox.Visible = b && viewportsSwitchButton.Active;
			}
		}

		bool DrawingsVisible {
			set {
				drawingsVisible = value;
				mainviewport.Visible = !value;
				blackboarddrawingarea.Visible = value;
			}
			get {
				return drawingsVisible;
			}
		}

		void Reset ()
		{
			zoomLevelButton.Visible = false;
			DrawingsVisible = false;
			timelabel.Text = "";
			muted = false;
			ignoreRate = false;
			viewportsSwitchButton.Active = true;
			SubViewPortsVisible = true;
			zoomLevel = ZoomLevel.Original;
		}

		void ConnectSignals ()
		{
			vwin.VolumeChanged += HandleVolumeChanged;
			zoomMenu.ZoomChanged += HandleZoomChanged;
			closebutton.Clicked += HandleClosebuttonClicked;
			prevbutton.Clicked += HandlePrevbuttonClicked;
			nextbutton.Clicked += HandleNextbuttonClicked;
			playbutton.Clicked += HandlePlaybuttonClicked;
			pausebutton.Clicked += HandlePausebuttonClicked;
			drawbutton.Clicked += HandleDrawButtonClicked;
			volumebutton.Clicked += HandleVolumebuttonClicked;
			ratescale.FormatValue += HandleRateFormatValue;
			ratescale.ValueChanged += HandleRateValueChanged;
			ratescale.ButtonPressEvent += HandleRatescaleButtonPress;
			ratescale.ButtonReleaseEvent += HandleRatescaleButtonRelease;
			jumpspinbutton.ValueChanged += HandleJumpValueChanged;
			viewportsSwitchButton.Toggled += HandleViewPortsToggled;
			zoomLevelButton.Clicked += HandleZoomClicked;
			centerplayheadbutton.Clicked += HandleCenterPlayheadClicked;
			timerule.CenterPlayheadClicked += HandleCenterPlayheadClicked;
			detachbutton.Clicked += (sender, e) =>
				App.Current.EventsBroker.Publish (new DetachEvent ());
		}

		void LoadImage (Image image, FrameDrawing drawing)
		{
			if (image == null) {
				DrawingsVisible = false;
				return;
			}
			blackboard.Background = image;
			blackboard.Drawing = drawing;
			if (drawing != null) {
				blackboard.RegionOfInterest = drawing.RegionOfInterest;
			}
			DrawingsVisible = true;
			blackboarddrawingarea.QueueDraw ();
		}

		float GetRateFromScale ()
		{
			return (float)rateList [(int)ratescale.Value - (int)App.Current.LowerRate];
		}

		void ConnectWindow (VideoWindow window)
		{
			window.ButtonPressEvent += OnSubViewportButtonPressEvent;
			window.ReadyEvent += HandleReady;
			window.UnReadyEvent += HandleUnReady;
			window.ExposeEvent += HandleExposeEvent;
		}

		void CreateWindows ()
		{
			mainviewport.ButtonReleaseEvent += OnMainViewportButtonReleaseEvent;
			ConnectWindow (mainviewport);
			mainviewport.CanFocus = true;

			subviewport1.Index = 1;
			ConnectWindow (subviewport1.Viewport);
			subviewport1.Combo.Changed += OnSubViewportChangedEvent;

			subviewport2.Index = 2;
			ConnectWindow (subviewport2.Viewport);

			subviewport3.Index = 3;
			ConnectWindow (subviewport3.Viewport);
		}

		void SetVolumeIcon (string name)
		{
			volumebuttonimage.Pixbuf = Misc.LoadIcon (name, IconSize.Button, 0);
		}

		void UpdateComboboxes ()
		{
			UpdateCombo (subviewport1);
			UpdateCombo (subviewport2);
			UpdateCombo (subviewport3);
		}

		void ValidateCameras (ObservableCollection<CameraConfig> cameras)
		{
			bool changed = false;

			// As no camera configuration has been defined yet, we should suggest one
			if (cameras == null) {
				cameras = new ObservableCollection<CameraConfig> ();
				for (int i = 0; i < Math.Min (4, ViewModel.FileSet.ViewModels.Count); i++) {
					changed = true;
					cameras.Add (new CameraConfig (i));
				}
			} else if (cameras.Count < ViewModel.FileSet.ViewModels.Count) {
				for (int i = cameras.Count; i < ViewModel.FileSet.ViewModels.Count; i++) {
					changed = true;
					cameras.Add (new CameraConfig (i));
				}
			}
			// If a ROI is defined we need to allow dragging
			if (!cameras [0].RegionOfInterest.Empty) {
				mainviewport.VideoDragStarted += OnMainViewportVideoDragStartedEvent;
				mainviewport.VideoDragStopped += OnMainViewportVideoDragStoppedEvent;
			}
			if (changed) {
				playerVM.SetCamerasConfig (cameras);
			}
		}

		void UpdateTime ()
		{
			if (playerVM.CurrentTime != null && playerVM.Duration != null) {
				timelabel.Text = playerVM.CurrentTime.ToMSecondsString (true) + "/" + playerVM.Duration.ToMSecondsString ();
			}
		}

		void DebugCamerasVisible ()
		{
			string str = "CamerasConfig =";
			foreach (var i in playerVM.CamerasConfig)
				str += " " + i.Index;
			Log.Debug (str);
		}

		void UpdateCombo (SubViewport viewport)
		{
			if (ViewModel.FileSet != null && viewport.Index < ViewModel.FileSet.ViewModels.Count &&
				viewport.Index < playerVM.CamerasConfig.Count) {
				viewport.Visible = true;
				viewport.Combo.Clear ();
				var cell = new CellRendererText ();
				viewport.Combo.PackStart (cell, false);
				viewport.Combo.AddAttribute (cell, "text", 0);
				var store = new ListStore (typeof (string), typeof (MediaFile));
				foreach (var f in ViewModel.FileSet) {
					store.AppendValues (f.Name, f.Model);
				}
				viewport.Combo.Model = store;
				/* Do not trigger the Changed callback from here */
				viewport.Combo.Changed -= OnSubViewportChangedEvent;
				viewport.Combo.Active = playerVM.CamerasConfig [viewport.Index].Index;
				viewport.Combo.Changed += OnSubViewportChangedEvent;
			} else {
				viewport.Visible = false;
			}
		}

		void ChangeCursor (string cursor)
		{
			Cursor c = null;
			if (cursor != null) {
				Image img = VAS.Core.Resources.LoadImage ("images/cursors/" + cursor);
				c = new Cursor (this.Display, img.Value, 0, 0);
			}

			mainviewport.Cursor = c;
		}

		SubViewport FindSubViewportFromVideoWindow (object o)
		{
			if (o == subviewport1.Viewport) {
				return subviewport1;
			} else if (o == subviewport2.Viewport) {
				return subviewport2;
			} else if (o == subviewport3.Viewport) {
				return subviewport3;
			}

			/* This should not happen */
			Log.Error ("Received SubViewport Press event from unknown viewport");
			return null;
		}

		SubViewport FindSubViewportFromComboBox (object o)
		{
			if (o == subviewport1.Combo) {
				return subviewport1;
			} else if (o == subviewport2.Combo) {
				return subviewport2;
			} else if (o == subviewport3.Combo) {
				return subviewport3;
			}

			/* This should not happen */
			Log.Error ("Received SubViewport Change event from unknown combobox");
			return null;
		}

		#endregion

		#region UI Callbacks

		// FIXME: This should be done through the ViewModel
		void HandleCenterPlayheadClicked (object sender, EventArgs e)
		{
			if (CenterPlayheadClicked != null) {
				CenterPlayheadClicked (sender, null);
			}
		}

		void HandleExposeEvent (object sender, ExposeEventArgs args)
		{
			playerVM.Expose ();
			/* The player draws over the eventbox when it's resized
			 * so make sure that we queue a draw in the event box after
			 * the expose */
			lightbackgroundeventbox.QueueDraw ();
		}

		void HandlePlaybuttonClicked (object sender, System.EventArgs e)
		{
			playerVM.Play ();
		}

		void HandleVolumebuttonClicked (object sender, System.EventArgs e)
		{
			vwin.SetLevel (playerVM.Volume);
			vwin.Show ();
		}

		void HandleVolumeChanged (double level)
		{
			double prevLevel;

			prevLevel = playerVM.Volume;
			if (prevLevel > 0 && level == 0) {
				SetVolumeIcon ("longomatch-control-volume-off");
			} else if (prevLevel > 0.5 && level <= 0.5) {
				SetVolumeIcon ("longomatch-control-volume-low");
			} else if (prevLevel <= 0.5 && level > 0.5) {
				SetVolumeIcon ("longomatch-control-volume-med");
			} else if (prevLevel < 1 && level == 1) {
				SetVolumeIcon ("longomatch-control-volume-hi");
			}
			playerVM.SetVolume (level);
			if (level == 0)
				muted = true;
			else
				muted = false;
		}

		void HandlePausebuttonClicked (object sender, EventArgs e)
		{
			playerVM.Pause ();
		}

		void HandleClosebuttonClicked (object sender, EventArgs e)
		{
			playerVM.LoadEvent (null, playerVM.Playing);
		}

		void HandlePrevbuttonClicked (object sender, EventArgs e)
		{
			playerVM.Previous ();
		}

		void HandleNextbuttonClicked (object sender, EventArgs e)
		{
			playerVM.Next ();
		}

		void HandleRateFormatValue (object o, FormatValueArgs args)
		{
			int val = (int)args.Value - (int)App.Current.LowerRate;
			if (rateList != null && val < rateList.Count) {
				args.RetVal = rateList [val] + "X";
			}
		}

		void HandleRateValueChanged (object sender, EventArgs e)
		{
			float val = GetRateFromScale ();

			// Mute for rate != 1
			if (val != 1 && playerVM.Volume != 0) {
				previousVLevel = playerVM.Volume;
				playerVM.SetVolume (0);
			} else if (val != 1 && muted)
				previousVLevel = 0;
			else if (val == 1)
				playerVM.SetVolume (previousVLevel);

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
		protected virtual void HandleRatescaleButtonPress (object o, ButtonPressEventArgs args)
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
		protected virtual void HandleRatescaleButtonRelease (object o, ButtonReleaseEventArgs args)
		{
			if (args.Event.Button == 1) {
				args.Event.SetButton (2);
			} else {
				args.Event.SetButton (1);
			}
		}

		void OnMainViewportVideoDragStartedEvent (object o, ButtonPressEventArgs args)
		{
			dragTimerID = GLib.Timeout.Add (200, delegate {
				mainviewport.VideoDragged += OnMainViewportVideoDraggedEvent;
				return false;
			});

			moveStart = new Point (args.Event.X, args.Event.Y);
			roiMoved = false;
			wasPlaying = false;
		}

		void OnMainViewportVideoDragStoppedEvent (object o, ButtonReleaseEventArgs args)
		{
			if (dragTimerID != 0) {
				GLib.Source.Remove (dragTimerID);
				dragTimerID = 0;
			}

			mainviewport.VideoDragged -= OnMainViewportVideoDraggedEvent;
			moveStart = null;

			ChangeCursor (null);

			// If we actually dragged the ROI we don't want the default button release handler
			// to be called as this would toggle play pause. On the other hand if the ROI was
			// unchanged, the user expects the normal behaviour of play/pause when clicking the video.
			if (roiMoved == true) {
				args.RetVal = true;
			}
			if (wasPlaying == true) {
				playerVM.Play ();
			}
		}

		void OnMainViewportVideoDraggedEvent (object o, MotionNotifyEventArgs args)
		{
			if (roiMoved == false) {
				ChangeCursor ("hand_closed");
				wasPlaying = playerVM.Playing;
				playerVM.Pause ();
			}
			Point newStart = new Point (args.Event.X, args.Event.Y);
			Point diff = newStart - moveStart;
			moveStart = newStart;
			playerVM.CamerasConfig [0].RegionOfInterest.Start -= diff;
			ClipRoi (playerVM.CamerasConfig [0].RegionOfInterest,
					 ViewModel.FileSet.ViewModels [playerVM.CamerasConfig [0].Index]);
			playerVM.ApplyROI (playerVM.CamerasConfig [0]);
			roiMoved = true;
		}

		void OnMainViewportButtonReleaseEvent (object o, ButtonReleaseEventArgs args)
		{
			/* FIXME: The pointer is grabbed when the event box is clicked.
			 * Make sure to ungrab it in order to avoid clicks outisde the window
			 * triggering this callback. This should be fixed properly.*/
			Gdk.Pointer.Ungrab (Gtk.Global.CurrentEventTime);
			playerVM.TogglePlay ();
		}

		void ClipRoi (Area roi, MediaFileVM file)
		{
			Point st = roi.Start;
			st.X = Math.Max (st.X, 0);
			st.Y = Math.Max (st.Y, 0);
			st.X = Math.Min (st.X, (file.VideoWidth - roi.Width));
			st.Y = Math.Min (st.Y, (file.VideoHeight - roi.Height));
		}

		void OnSubViewportChangedEvent (object o, EventArgs args)
		{
			mainviewport.GrabFocus ();
			if (ViewModel.FileSet == null)
				return;
			SubViewport subviewport = FindSubViewportFromComboBox (o);
			if (subviewport == null)
				return;

			int ndx = subviewport.Index;

			Log.Debug ("Selected camera " + subviewport.Combo.Active +
			" on subviewport " + ndx);

			// Store in a dropped CameraConfig List the CameraConfig that is going to be drop (in order to reuse it later)
			CameraConfig cam = playerVM.CamerasConfig.FirstOrDefault (x => x.Index == playerVM.CamerasConfig [ndx].Index);
			if (playerVM.CamerasConfig.Count (x => x.Index == playerVM.CamerasConfig [ndx].Index) == 1) {
				cameraConfigsOutOfScreen.Add (cam);
			}
			// If the camera in the principal viewport is selected, use his configuration
			cam = playerVM.CamerasConfig.FirstOrDefault (x => x.Index == subviewport.Combo.Active);
			if (cam != null) {
				playerVM.CamerasConfig [ndx] = cam;
			} else {// If the selected camera was previously on screen, use his configuration and delete it from the dropped CameraConfig list
				cam = cameraConfigsOutOfScreen.FirstOrDefault (x => x.Index == subviewport.Combo.Active);
				if (cam != null) {
					playerVM.CamerasConfig [ndx] = cam;
					cameraConfigsOutOfScreen.RemoveAll (x => x.Index == subviewport.Combo.Active);
				} else { // In other case, create a new CameraConfig
					playerVM.CamerasConfig [ndx] = new CameraConfig (subviewport.Combo.Active);
				}
			}

			playerVM.SetCamerasConfig (playerVM.CamerasConfig);
			var file = ViewModel.FileSet.ViewModels [playerVM.CamerasConfig [ndx].Index];
			subviewport.Viewport.Ratio = (float)file.Par * file.VideoWidth / (float)file.VideoHeight;

			DebugCamerasVisible ();
		}

		void OnSubViewportButtonPressEvent (object o, ButtonPressEventArgs args)
		{
			/* FIXME: The pointer is grabbed when the event box is clicked.
			 * Make sure to ungrab it in order to avoid clicks outisde the window
			 * triggering this callback. This should be fixed properly.*/
			Gdk.Pointer.Ungrab (Gtk.Global.CurrentEventTime);

			/* Do not allow switching ports when is a drawing is displayed */
			if (DrawingsVisible) {
				return;
			}

			SubViewport subviewport = FindSubViewportFromVideoWindow (o);
			if (subviewport == null)
				return;

			int ndx = subviewport.Index;

			Log.Debug ("Clicked on subviewport " + ndx);

			/* Swap main and clicked viewport */
			CameraConfig tmp = playerVM.CamerasConfig [0];
			playerVM.CamerasConfig [0] = playerVM.CamerasConfig [ndx];
			playerVM.CamerasConfig [ndx] = tmp;
			playerVM.SetCamerasConfig (playerVM.CamerasConfig);
			/* Make the combo box match the current camera in the viewport */
			UpdateCombo (subviewport);

			DebugCamerasVisible ();
		}

		void OnVideoboxScrollEvent (object o, ScrollEventArgs args)
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

		void HandleDrawButtonClicked (object sender, EventArgs e)
		{
			playerVM.DrawFrame ();
		}

		void HandleJumpValueChanged (object sender, EventArgs e)
		{
			playerVM.Step = new Time (jumpspinbutton.ValueAsInt * 1000);
		}

		void HandleReady (object sender, EventArgs e)
		{
			playerVM.ViewPorts = new List<IViewPort> { mainviewport, subviewport1.Viewport, subviewport2.Viewport, subviewport3.Viewport };
			playerVM.Ready (true);
		}

		protected virtual void HandleUnReady (object sender, EventArgs e)
		{
			playerVM.Ready (false);
			playerVM.ViewPorts = null;
		}

		void HandleViewPortsToggled (object sender, EventArgs e)
		{
			subviewportsbox.Visible = viewportsSwitchButton.Active;
		}

		void HandleZoomClicked (object sender, EventArgs e)
		{
			zoomMenu.Popup ();
		}

		void HandleZoomChanged (ZoomLevel level)
		{
			CameraConfig cfg = playerVM.CamerasConfig [0];
			MediaFileVM file = ViewModel.FileSet.ViewModels [cfg.Index];
			double zoomFactor = 1.0;

			zoomLevel = level;

			switch (zoomLevel) {
			case ZoomLevel.Original:
				zoomFactor = 1;
				break;
			case ZoomLevel.Level1:
				zoomFactor = 1.2;
				break;
			case ZoomLevel.Level2:
				zoomFactor = 1.4;
				break;
			case ZoomLevel.Level3:
				zoomFactor = 1.6;
				break;
			case ZoomLevel.Level4:
				zoomFactor = 1.8;
				break;
			case ZoomLevel.Level5:
				zoomFactor = 2;
				break;
			}

			Point origin = cfg.RegionOfInterest.Center;

			cfg.RegionOfInterest.Width = file.VideoWidth / zoomFactor;
			cfg.RegionOfInterest.Height = file.VideoHeight / zoomFactor;

			// Center with regards to previous origin
			cfg.RegionOfInterest.Start.X = origin.X - cfg.RegionOfInterest.Width / 2;
			cfg.RegionOfInterest.Start.Y = origin.Y - cfg.RegionOfInterest.Height / 2;

			ClipRoi (cfg.RegionOfInterest, file);

			playerVM.SetCamerasConfig (playerVM.CamerasConfig);

			if (zoomLevel != ZoomLevel.Original) {
				mainviewport.VideoDragStarted += OnMainViewportVideoDragStartedEvent;
				mainviewport.VideoDragStopped += OnMainViewportVideoDragStoppedEvent;
			} else {
				mainviewport.VideoDragged -= OnMainViewportVideoDraggedEvent;
				mainviewport.VideoDragStarted -= OnMainViewportVideoDragStartedEvent;
				mainviewport.VideoDragStopped -= OnMainViewportVideoDragStoppedEvent;
			}
		}

		void HandleKeyPressed (KeyPressedEvent e)
		{
			/*
			try {
				action = App.Current.Config.Hotkeys.ActionsHotkeys.GetKeyByValue (e.Key);
			} catch (Exception ex) {
				// The dictionary contains 2 equal values for different keys
				Log.Exception (ex);
				return;
			}

			if (action != LMCommon.KeyAction.None) {
				switch (action) {
				case LMCommon.KeyAction.VideoZoomOriginal:
					HandleZoomChanged (ZoomLevel.Original);
					break;
				case LMCommon.KeyAction.VideoZoomIn:
					// Increase zoom level with clamping to max level
					HandleZoomChanged ((ZoomLevel)Math.Min ((int)ZoomLevel.Level5, (int)zoomLevel + 1));
					break;
				case LMCommon.KeyAction.VideoZoomOut:
					// Decrease zoom level with clamping to original
					HandleZoomChanged ((ZoomLevel)Math.Max ((int)ZoomLevel.Original, (int)zoomLevel - 1));
					break;
				}
			}
		*/
		}

		#endregion

		void PlayerVMPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (ViewModel.NeedsSync (e, nameof (ViewModel.ViewMode))) {
				HandleModeChanged ();
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.ControlsSensitive))) {
				controlsbox.Sensitive = ratescale.Sensitive = playerVM.ControlsSensitive;
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.PlayerAttached))) {
				HandlePlayerAttachedChanged ();
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.ShowDetachButton))) {
				detachbutton.Visible = playerVM.ShowDetachButton;
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.Playing))) {
				HandlePlayingChanged ();
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.HasNext))) {
				nextbutton.Sensitive = playerVM.HasNext;
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.CloseButtonVisible))) {
				closebutton.Visible = playerVM.CloseButtonVisible;
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.Rate))) {
				ignoreRate = true;
				int index = App.Current.RateList.FindIndex (p => (float)p == playerVM.Rate);
				ratescale.Value = index + App.Current.LowerRate;
				ignoreRate = false;
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.Seekable))) {
				timerule.ObjectsCanMove = playerVM.Seekable;
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.Duration)) ||
				ViewModel.NeedsSync (e, nameof (ViewModel.CurrentTime)) ||
				ViewModel.NeedsSync (e, nameof (ViewModel.PlayerMode))) {
				UpdateTime ();
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.FrameDrawing))) {
				if (playerVM.FrameDrawing != null) {
					LoadImage (playerVM.CurrentFrame, playerVM.FrameDrawing);
				} else {
					DrawingsVisible = false;
				}
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.CamerasConfig))) {
				HandleCamerasConfigChanged ();
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.FileSet))) {
				HandleCamerasConfigChanged ();
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.SupportsMultipleCameras))) {
				HandleCamerasConfigChanged ();
			}
		}

		void HandleModeChanged ()
		{
			PlayerViewOperationMode mode = playerVM.ViewMode;

			controlsbox.Visible =
				mode == PlayerViewOperationMode.Analysis ||
				mode == PlayerViewOperationMode.LiveAnalysisReview ||
				mode == PlayerViewOperationMode.SimpleWithControls;

			center_playhead_box.Visible =
				mode == PlayerViewOperationMode.SimpleWithControls;

			camerasbox.Visible =
				mode == PlayerViewOperationMode.Analysis ||
				mode == PlayerViewOperationMode.SimpleWithControls;

			drawbutton.Visible =
				mode == PlayerViewOperationMode.Analysis ||
				mode == PlayerViewOperationMode.LiveAnalysisReview ||
				mode == PlayerViewOperationMode.Synchronization;

			timelabel.Visible =
				mode == PlayerViewOperationMode.Analysis ||
				mode == PlayerViewOperationMode.LiveAnalysisReview ||
				mode == PlayerViewOperationMode.Synchronization;

			detachbutton.Visible =
				mode == PlayerViewOperationMode.Synchronization ||
				mode == PlayerViewOperationMode.Analysis ||
				mode == PlayerViewOperationMode.SimpleWithControls;

			timerulearea.Visible =
				mode == PlayerViewOperationMode.SimpleWithControls ||
				mode == PlayerViewOperationMode.Analysis;

			timerule.AdjustSizeToDuration =
				mode == PlayerViewOperationMode.SimpleWithControls ||
				mode == PlayerViewOperationMode.Analysis ||
				mode == PlayerViewOperationMode.LiveAnalysisReview;

			timerule.ContinuousSeek =
				mode == PlayerViewOperationMode.Analysis ||
				mode == PlayerViewOperationMode.LiveAnalysisReview ||
				mode == PlayerViewOperationMode.Synchronization;

			prevbutton.Visible = nextbutton.Visible =
				prevbutton.Visible ||
				mode == PlayerViewOperationMode.SimpleWithControls;
		}

		void HandlePlayerAttachedChanged ()
		{
			if (playerVM.PlayerAttached) {
				detachbuttonimage.Pixbuf = Misc.LoadIcon ("longomatch-control-attach",
					StyleConf.PlayerCapturerIconSize);
				detachbutton.TooltipMarkup = Catalog.GetString ("Attach window");
			} else {
				detachbuttonimage.Pixbuf = Misc.LoadIcon ("longomatch-control-detach",
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

		void HandleCamerasConfigChanged ()
		{
			if (ViewModel.FileSet != null) {
				ValidateCameras (playerVM.CamerasConfig);
				mainviewport.Visible = ViewModel.FileSet.ViewModels.Count > 0;
				UpdateComboboxes ();
				DebugCamerasVisible ();
				SubViewPortsVisible = ViewModel.SupportsMultipleCameras;
				zoomLevelButton.Visible = true;
			} else {
				SubViewPortsVisible = false;
				zoomLevelButton.Visible = false;
			}
		}
	}
}

