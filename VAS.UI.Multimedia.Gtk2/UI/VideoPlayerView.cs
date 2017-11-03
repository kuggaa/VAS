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
using VAS.Bindings;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Handlers;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;
using VAS.Drawing.Cairo;
using VAS.Drawing.Widgets;
using VAS.UI.Helpers.Bindings;
using DConstants = VAS.Drawing.Constants;
using Image = VAS.Core.Common.Image;
using Misc = VAS.UI.Helpers.Misc;
using Point = VAS.Core.Common.Point;

namespace VAS.UI
{
	[Category ("VAS")]
	[ToolboxItem (true)]
	[View ("VideoPlayerView")]
	public partial class VideoPlayerView : Bin, IView<VideoPlayerVM>, IVideoPlayerView
	{
		public event ClickedHandler CenterPlayheadClicked;

		const int SCALE_FPS = 25;
		bool drawingsVisible, updatingView;
		bool roiMoved, wasPlaying;
		uint dragTimerID;
		Blackboard blackboard;
		List<CameraConfig> cameraConfigsOutOfScreen;
		Point moveStart;
		Timerule timerule;
		EventEditionTimeruleView eventEditionTimerule;
		VideoPlayerVM playerVM;
		SliderView volumeWindow;
		SliderView jumpsWindow;
		SliderView zoomWindow;
		SliderView rateWindow;
		int zoomLevel;
		BindingContext ctx;

		#region Constructors

		public VideoPlayerView ()
		{
			this.Build ();

			// The editor keeps changing that value to false.
			var panedChild = ((Paned.PanedChild)(this.videohpaned [this.videobox]));
			panedChild.Resize = true;

			timerulearea.HeightRequest = DConstants.TIMERULE_PLAYER_HEIGHT;
			timerule = new Timerule (new WidgetWrapper (timerulearea)) {
				PlayerMode = true,
				ContinuousSeek = true,
				AutoUpdate = true,
				AdjustSizeToDuration = true,
			};

			editeventtimeruledrawingarea.HeightRequest = DConstants.TIMERULE_PLAYER_HEIGHT;
			eventEditionTimerule = new EventEditionTimeruleView (new WidgetWrapper (editeventtimeruledrawingarea));

			rateLabel.ModifyFont (FontDescription.FromString (App.Current.Style.Font + " 8px"));
			jumpsLabel.ModifyFont (FontDescription.FromString (App.Current.Style.Font + " 8px"));
			zoomLabel.ModifyFont (FontDescription.FromString (App.Current.Style.Font + " 8px"));
			totalTimeLabel.ModifyFont (FontDescription.FromString (App.Current.Style.Font + " 10px"));
			totalTimeLabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (App.Current.Style.TextBaseSecondary));
			eventNameLabel.Ellipsize = EllipsizeMode.End;



			centerplayheadbuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-dash-center-view",
				StyleConf.PlayerCapturerIconSize);
			DurationButtonImage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-duration", 15);



			// Force tooltips to be translatable as there seems to be a bug in stetic 
			// code generation for translatable tooltips.

			centerplayheadbutton.TooltipMarkup = Catalog.GetString ("Center Playhead");

			volumeWindow = new SliderView (0, 101, 1, 1);
			volumeWindow.FormatValue = (double val) => { return val + "%"; };
			jumpsWindow = new SliderView (0, App.Current.StepList.Count, 1, 1);
			jumpsWindow.FormatValue = (double val) => { return App.Current.StepList [(int)val] + "s"; };
			zoomWindow = new SliderView (0, App.Current.ZoomLevels.Count, 1, 1);
			zoomWindow.FormatValue = (double val) => { return (App.Current.ZoomLevels [(int)val] * 100) + "%"; };
			rateWindow = new SliderView (0, App.Current.RateList.Count, 1, 1);
			rateWindow.FormatValue = (double val) => { return App.Current.RateList [(int)val] + "X"; };

			blackboard = new Blackboard (new WidgetWrapper (blackboarddrawingarea));
			blackboarddrawingarea.Visible = false;
			blackboarddrawingarea.NoShowAll = true;

			ConnectSignals ();

			totalbox.NoShowAll = true;
			Misc.SetFocus (totalbox, false);
			mainviewport.CanFocus = true;

			maineventbox.ModifyBg (StateType.Normal, Misc.ToGdkColor (App.Current.Style.ScreenBase));

			controlsbox.HeightRequest = StyleConf.PlayerCapturerControlsHeight;

			cameraConfigsOutOfScreen = new List<CameraConfig> ();

			CreateWindows ();
			Bind ();
		}

		#endregion

		protected override void OnDestroyed ()
		{
			if (dragTimerID != 0) {
				GLib.Source.Remove (dragTimerID);
				dragTimerID = 0;
			}

			DisconnectSignals ();
			blackboard.Dispose ();
			blackboard = null;
			timerule.Dispose ();
			timerule = null;
			eventEditionTimerule.Dispose ();
			eventEditionTimerule = null;

			volumeWindow.Dispose ();
			volumeWindow = null;
			jumpsWindow.Dispose ();
			jumpsWindow = null;
			zoomWindow.Dispose ();
			zoomWindow = null;
			rateWindow.Dispose ();
			rateWindow = null;
			ViewModel = null;

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
					timerule.ViewModel = playerVM;
					eventEditionTimerule.ViewModel = playerVM;
					playerVM.PropertyChanged += PlayerVMPropertyChanged;
					Reset ();
					playerVM.Sync ();
					ctx.UpdateViewModel (playerVM);
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

		// FIXME: Used in presentations while MVVM is not implemented there
		public bool SubViewPortsVisible {
			set {
				bool b = value && (ViewModel.FileSet == null || ViewModel.FileSet.ViewModels.Count > 1);
				viewportsBox.Visible = b;
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

		#endregion

		#region Private methods

		void Reset ()
		{
			zoomBox.Visible = false;
			DrawingsVisible = false;
			totalTimeLabel.Text = "";
			timeLabel.Text = "";
			viewportsSwitchButton.Active = true;
			SubViewPortsVisible = true;
			zoomLevel = 0;
			zoomLabel.Text = "100%";
			jumpsLabel.Text = $"10s";
		}

		void ConnectSignals ()
		{
			zoomWindow.ValueChanged += HandleZoomChanged;
			centerplayheadbutton.Clicked += HandleCenterPlayheadClicked;
			timerule.CenterPlayheadClicked += HandleCenterPlayheadClicked;
			mainviewport.VideoDragStarted += OnMainViewportVideoDragStartedEvent;
			mainviewport.VideoDragStopped += OnMainViewportVideoDragStoppedEvent;
		}

		void DisconnectSignals ()
		{
			zoomWindow.ValueChanged -= HandleZoomChanged;
			centerplayheadbutton.Clicked -= HandleCenterPlayheadClicked;
			timerule.CenterPlayheadClicked -= HandleCenterPlayheadClicked;
			mainviewport.VideoDragStarted -= OnMainViewportVideoDragStartedEvent;
			mainviewport.VideoDragStopped -= OnMainViewportVideoDragStoppedEvent;
		}

		void Bind ()
		{
			ctx = new BindingContext ();
			ctx.Add (zoomLevelButton.Bind ((vm) => ((VideoPlayerVM)vm).ShowZoomCommand));
			ctx.Add (editDurationButton.Bind ((vm) => ((VideoPlayerVM)vm).EditEventDurationCommand, true, false, true));
			ctx.Add (volumeWindow.Bind (volumebutton, vm => ((VideoPlayerVM)vm).ChangeVolumeCommand, vm => ((VideoPlayerVM)vm).Volume, HandleVolumeChanged));
			ctx.Add (jumpsWindow.Bind (jumpsButton, vm => ((VideoPlayerVM)vm).ChangeStepCommand));
			ctx.Add (rateWindow.Bind (rateLevelButton, vm => ((VideoPlayerVM)vm).ChangeRateCommand));
			ctx.Add (closebutton.Bind ((vm) => ((VideoPlayerVM)vm).CloseCommand));
			ctx.Add (prevbutton.Bind ((vm) => ((VideoPlayerVM)vm).PreviousCommand));
			ctx.Add (nextbutton.Bind ((vm) => ((VideoPlayerVM)vm).NextCommand));
			ctx.Add (playbutton.Bind ((vm) => ((VideoPlayerVM)vm).PlayCommand));
			ctx.Add (pausebutton.Bind ((vm) => ((VideoPlayerVM)vm).PauseCommand, false));
			ctx.Add (drawbutton.Bind ((vm) => ((VideoPlayerVM)vm).DrawCommand));
			ctx.Add (detachbutton.Bind ((vm) => ((VideoPlayerVM)vm).DetachCommand));
			ctx.Add (viewportsSwitchButton.Bind ((vm) => ((VideoPlayerVM)vm).ViewPortsSwitchToggleCommand));
		}

		void LoadImage (Image image, FrameDrawing drawing)
		{
			if (image == null) {
				DrawingsVisible = false;
				return;
			}
			blackboard.Background = image;
			blackboard.Drawing = drawing;
			//FIXME: this should show a warning message here?
			if (drawing != null && App.Current.LicenseLimitationsService.CanExecute (
				VASFeature.OpenZoom.ToString ())) {
				blackboard.RegionOfInterest = drawing.RegionOfInterest;
			}
			DrawingsVisible = true;
			blackboarddrawingarea.QueueDraw ();
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
			mainviewport.ScrollEvent += OnVideoboxScrollEvent;
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
			volumebuttonimage.Image = App.Current.ResourcesLocator.LoadIcon (name, StyleConf.PlayerCapturerIconSize);
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
			if (changed) {
				playerVM.SetCamerasConfig (cameras);
			}
		}

		void UpdateTime ()
		{
			if (playerVM.CurrentTime != null && playerVM.Duration != null) {
				timeLabel.Text = playerVM.CurrentTime.ToMSecondsString (true);
				totalTimeLabel.Text = playerVM.Duration.ToSecondsString ();
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
				Image img = App.Current.ResourcesLocator.LoadImage ("images/cursors/" + cursor);
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

		void SetVisibility (Widget widget, bool visible)
		{
			if (widget == closebutton && playerVM.ViewMode == PlayerViewOperationMode.Presentation) {
				visible = false;
			}
			widget.Visible = visible;
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
			playerVM.ExposeCommand.Execute ();
			/* The player draws over the eventbox when it's resized
			 * so make sure that we queue a draw in the event box after
			 * the expose */
			lightbackgroundeventbox.QueueDraw ();
		}


		void HandleVolumeChanged (double level)
		{
			if (level <= 0) {
				SetVolumeIcon (StyleConf.PlayerControlVolumeOff);
			} else if (level > 0 && level < 0.5) {
				SetVolumeIcon (StyleConf.PlayerControlVolumeLow);
			} else if (level >= 0.5 && level < 1) {
				SetVolumeIcon (StyleConf.PlayerControlVolumeMedium);
			} else if (level >= 1) {
				SetVolumeIcon (StyleConf.PlayerControlVolumeHigh);
			}
		}

		void OnMainViewportVideoDragStartedEvent (object o, ButtonPressEventArgs args)
		{
			if (ViewModel.Zoom == 1) {
				return;
			}
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
			if (ViewModel.Zoom == 1) {
				return;
			}
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
				playerVM.PlayCommand.Execute ();
			}
		}

		void OnMainViewportVideoDraggedEvent (object o, MotionNotifyEventArgs args)
		{
			if (roiMoved == false) {
				ChangeCursor ("hand_closed");
				wasPlaying = playerVM.Playing;
				playerVM.PauseCommand.Execute (false);
			}
			Point newStart = new Point (args.Event.X, args.Event.Y);
			Point diff = newStart - moveStart;
			moveStart = newStart;
			playerVM.MoveROI (diff);
			roiMoved = true;
		}

		void OnMainViewportButtonReleaseEvent (object o, ButtonReleaseEventArgs args)
		{
			/* FIXME: The pointer is grabbed when the event box is clicked.
			 * Make sure to ungrab it in order to avoid clicks outisde the window
			 * triggering this callback. This should be fixed properly.*/
			Gdk.Pointer.Ungrab (Gtk.Global.CurrentEventTime);
			playerVM.TogglePlayCommand.Execute ();
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

			Log.Debug ("Selected camera " + subviewport.Combo.Active + " on subviewport " + ndx);

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
				playerVM.SeekToPreviousFrameCommand.Execute ();
				break;
			case ScrollDirection.Up:
				playerVM.SeekToNextFrameCommand.Execute ();
				break;
			case ScrollDirection.Left:
				playerVM.StepBackwardCommand.Execute ();
				break;
			case ScrollDirection.Right:
				playerVM.StepForwardCommand.Execute ();
				break;
			}
		}

		void HandleReady (object sender, EventArgs e)
		{
			playerVM.ViewPorts = new List<IViewPort> { mainviewport, subviewport1.Viewport, subviewport2.Viewport, subviewport3.Viewport };
			playerVM.ReadyCommand.Execute (true);
		}

		void HandleUnReady (object sender, EventArgs e)
		{
			playerVM.ReadyCommand.Execute (false);
			playerVM.ViewPorts = null;
		}

		void HandleViewPortsToggled (object sender, EventArgs e)
		{
			subviewportsbox.Visible = viewportsSwitchButton.Active;
		}

		void HandleZoomChanged (double level)
		{
			if (updatingView || zoomLevel == (int)level) {
				return;
			}
			zoomLevel = (int)level;
			playerVM.SetZoomCommand.Execute (App.Current.ZoomLevels [(int)level]);
		}

		#endregion

		void PlayerVMPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (ViewModel.NeedsSync (e, nameof (ViewModel.ViewMode))) {
				HandleModeChanged ();
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.ControlsSensitive))) {
				controlsbox.Sensitive = rateLevelButton.Sensitive = playerVM.ControlsSensitive;
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.PlayerAttached))) {
				HandlePlayerAttachedChanged ();
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.Playing))) {
				HandlePlayingChanged ();
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.LoadedElement))) {
				HandlePlayElementChanged ();
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.Rate))) {
				rateWindow.SetValue (App.Current.RateList.IndexOf (playerVM.Rate));
				rateLabel.Text = $"{playerVM.Rate}X";
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.Volume))) {
				volumeWindow.SetValue ((float)(playerVM.Volume * 100));
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.Zoom))) {
				updatingView = true;
				zoomWindow.SetValue (App.Current.ZoomLevels.IndexOf (playerVM.Zoom));
				zoomLabel.Text = $"{playerVM.Zoom * 100}%";
				updatingView = false;
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.ShowZoom))) {
				if (ViewModel.ShowZoom) {
					zoomWindow.Show ();
					ViewModel.ShowZoom = false;
				}
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.Step))) {
				jumpsWindow.SetValue (App.Current.StepList.IndexOf (playerVM.Step.TotalSeconds));
				jumpsLabel.Text = $"{playerVM.Step.TotalSeconds}s";
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
			if (ViewModel.NeedsSync (e, nameof (ViewModel.CamerasConfig)) ||
				ViewModel.NeedsSync (e, nameof (ViewModel.FileSet)) ||
				ViewModel.NeedsSync (e, nameof (ViewModel.SupportsMultipleCameras))) {
				HandleCamerasConfigChanged ();
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.ShowDetachButton))) {
				detachbutton.Visible = ViewModel.ShowDetachButton;
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.ShowCenterPlayHeadButton))) {
				center_playhead_box.Visible = ViewModel.ShowCenterPlayHeadButton;
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.EditEventDurationModeEnabled))) {
				editeventtimeruledrawingarea.Visible = ViewModel.EditEventDurationModeEnabled;
				timerulearea.Visible = !ViewModel.EditEventDurationModeEnabled;
				editDurationButton.Active = ViewModel.EditEventDurationModeEnabled;

				//FIXME  We need Property binding like -> ctx.Add (control.Bind (c => c.Prop, vm => vm.Prop));
				if (ViewModel.NeedsSync (e, nameof (ViewModel.ViewPortsSwitchActive))) {
					subviewportsbox.Visible = ViewModel.ViewPortsSwitchActive;
				}
			}

			void HandlePlayElementChanged ()
			{
				if (playerVM.LoadedElement is IPlaylistEventElement) {
					DrawingsVisible = false;
					SetVisibility (closebutton, true);
					SetVisibility (eventNameLabel, true);
					// FIME: IPlaylistElement.Description shouldn't return formated string
					// or the interface should provide a Name property.
					eventNameLabel.Text = (playerVM.LoadedElement as TimelineEvent)?.Name ??
						(playerVM.LoadedElement as PlaylistPlayElement)?.Play?.Name;
				} else if (playerVM.LoadedElement is PlaylistDrawing) {
					PlaylistDrawing drawing = (PlaylistDrawing)playerVM.LoadedElement;
					LoadImage (null, drawing.Drawing);
				} else if (playerVM.LoadedElement is PlaylistImage) {
					PlaylistImage image = (PlaylistImage)playerVM.LoadedElement;
					LoadImage (image.Image, null);
					SetVisibility (closebutton, true);
					SetVisibility (eventNameLabel, true);
					eventNameLabel.Text = image.Description;
				} else {
					DrawingsVisible = false;
					SetVisibility (closebutton, false);
					SetVisibility (eventNameLabel, false);
				}
			}
			//FIXME  We need Property binding like -> ctx.Add (control.Bind (c => c.Prop, vm => vm.Prop));
			if (ViewModel.NeedsSync (e, nameof (ViewModel.ViewPortsSwitchActive))) {
				subviewportsbox.Visible = ViewModel.ViewPortsSwitchActive;

			}
		}

		void HandlePlayElementChanged ()
		{
			if (playerVM.LoadedElement is IPlaylistEventElement) {
				DrawingsVisible = false;
				SetVisibility (closebutton, true);
				SetVisibility (eventNameLabel, true);
				// FIME: IPlaylistElement.Description shouldn't return formated string
				// or the interface should provide a Name property.
				eventNameLabel.Text = (playerVM.LoadedElement as TimelineEvent)?.Name ??
					(playerVM.LoadedElement as PlaylistPlayElement)?.Play?.Name;
			} else if (playerVM.LoadedElement is PlaylistDrawing) {
				PlaylistDrawing drawing = (PlaylistDrawing)playerVM.LoadedElement;
				LoadImage (null, drawing.Drawing);
			} else if (playerVM.LoadedElement is PlaylistImage) {
				PlaylistImage image = (PlaylistImage)playerVM.LoadedElement;
				LoadImage (image.Image, null);
				SetVisibility (closebutton, true);
				SetVisibility (eventNameLabel, true);
				eventNameLabel.Text = image.Description;
			} else {
				DrawingsVisible = false;
				SetVisibility (closebutton, false);
				SetVisibility (eventNameLabel, false);
			}
		}

		void HandleModeChanged ()
		{
			PlayerViewOperationMode mode = playerVM.ViewMode;

			controlsbox.Visible =
				mode == PlayerViewOperationMode.Analysis ||
				mode == PlayerViewOperationMode.LiveAnalysisReview ||
				mode == PlayerViewOperationMode.SimpleWithControls ||
				mode == PlayerViewOperationMode.Presentation;

			viewportsBox.Visible =
				mode == PlayerViewOperationMode.Analysis ||
				mode == PlayerViewOperationMode.Presentation ||
				mode == PlayerViewOperationMode.SimpleWithControls;

			jumpsbox.Visible =
				mode == PlayerViewOperationMode.Analysis ||
				mode == PlayerViewOperationMode.Presentation ||
				mode == PlayerViewOperationMode.SimpleWithControls;

			rateBox.Visible =
				mode == PlayerViewOperationMode.Analysis ||
				mode == PlayerViewOperationMode.Presentation ||
				mode == PlayerViewOperationMode.SimpleWithControls;

			zoomBox.Visible =
				mode == PlayerViewOperationMode.Analysis ||
				mode == PlayerViewOperationMode.Presentation ||
				mode == PlayerViewOperationMode.SimpleWithControls;

			drawbutton.Visible =
				mode == PlayerViewOperationMode.Analysis ||
				mode == PlayerViewOperationMode.Presentation ||
				mode == PlayerViewOperationMode.LiveAnalysisReview ||
				mode == PlayerViewOperationMode.Synchronization;

			timeHBox.Visible =
				mode == PlayerViewOperationMode.Analysis ||
				mode == PlayerViewOperationMode.Presentation ||
				mode == PlayerViewOperationMode.LiveAnalysisReview ||
				mode == PlayerViewOperationMode.Synchronization;

			timerulearea.Visible =
				mode == PlayerViewOperationMode.SimpleWithControls ||
				mode == PlayerViewOperationMode.Presentation ||
				mode == PlayerViewOperationMode.Analysis;

			timerule.AdjustSizeToDuration =
				mode == PlayerViewOperationMode.SimpleWithControls ||
				mode == PlayerViewOperationMode.Analysis ||
				mode == PlayerViewOperationMode.Presentation ||
				mode == PlayerViewOperationMode.LiveAnalysisReview;

			timerule.ContinuousSeek =
				mode == PlayerViewOperationMode.Analysis ||
				mode == PlayerViewOperationMode.LiveAnalysisReview ||
				mode == PlayerViewOperationMode.Synchronization;

			prevbutton.Visible = nextbutton.Visible =
				mode == PlayerViewOperationMode.Analysis ||
				mode == PlayerViewOperationMode.Presentation ||
				mode == PlayerViewOperationMode.SimpleWithControls;
		}

		void HandlePlayerAttachedChanged ()
		{
			if (playerVM.PlayerAttached) {
				detachbuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-control-attach",
					StyleConf.PlayerCapturerIconSize);
				detachbutton.TooltipMarkup = Catalog.GetString ("Attach window");
			} else {
				detachbuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-control-detach",
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
				zoomBox.Visible = true;
			} else {
				SubViewPortsVisible = false;
				zoomBox.Visible = false;
			}
		}
	}
}

