//
//  Copyright (C) 2016 
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
using System.ComponentModel;
using Gtk;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Resources.Styles;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Drawing.Cairo;
using VAS.Drawing.Widgets;
using VAS.UI.Menus;
using Timer = VAS.Core.Store.Timer;
using VASDrawing = VAS.Drawing;

namespace VAS.UI.Component
{
	/// <summary>
	/// VAS Timeline.
	/// </summary>
	[System.ComponentModel.ToolboxItem (true)]
	public partial class Timeline : Gtk.Bin, IView<IAnalysisViewModel>
	{
		protected TimelineLabels labels;
		protected PlaysMenu menu;
		protected PlaysTimeline timeline;
		const int SAMPLING_INTERVAL_MS = 80;
		Timerule timerule;
		Time currentTime;
		double secondsPerPixel;
		Stopwatch stopWatch;
		IAnalysisViewModel viewModel;

		public Timeline ()
		{
			this.Build ();
			Initialize ();
		}

		public override void Dispose ()
		{
			Dispose (true);
			base.Dispose ();
		}

		protected virtual void Dispose (bool disposing)
		{
			if (Disposed) {
				return;
			}
			if (disposing) {
				Destroy ();
			}
			Disposed = true;
		}

		protected override void OnDestroyed ()
		{
			Log.Verbose ($"Destroying {GetType ()}");

			timerule?.Dispose ();
			timeline?.Dispose ();
			labels?.Dispose ();
			menu?.Dispose ();

			scrolledwindow1.Vadjustment.ValueChanged -= HandleScrollEvent;
			scrolledwindow1.Hadjustment.ValueChanged -= HandleScrollEvent;


			base.OnDestroyed ();

			Disposed = true;
		}

		protected bool Disposed { get; private set; }

		public IAnalysisViewModel ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.PropertyChanged -= HandleVideoPlayerPropertyChanged;
				}
				stopWatch.Reset ();
				stopWatch.Stop ();
				viewModel = value;
				labels.SetViewModel (viewModel);
				timeline.SetViewModel (viewModel);
				timerule.ViewModel = viewModel?.VideoPlayer;
				if (viewModel != null) {
					UpdateTime ();
					focusscale.Value = 6;
					viewModel.PropertyChanged += HandleVideoPlayerPropertyChanged;
					QueueDraw ();
				}
			}
		}

		VideoPlayerVM Player => ViewModel?.VideoPlayer;

		Time CurrentTime {
			get => currentTime;
			set {
				currentTime = value;
				QueueDraw ();
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (IAnalysisViewModel)viewModel;
		}

		void Initialize ()
		{
			stopWatch = new Stopwatch ();

			timerule = new Timerule (new WidgetWrapper (timerulearea));
			timerule.UseAbsoluteDuration = true;
			timerule.CenterPlayheadClicked += HandleFocusClicked;
			timeline = App.Current.ViewLocator.Retrieve ("PlaysTimelineView") as PlaysTimeline;
			timeline.SetWidget (new WidgetWrapper (timelinearea));
			timeline.ShowMenuEvent += HandleShowMenu;
			timeline.ShowTimersMenuEvent += HandleShowTimersMenu;
			labels = App.Current.ViewLocator.Retrieve ("TimelineLabelsView") as TimelineLabels;
			labels.SetWidget (new WidgetWrapper (labelsarea));

			focusbuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-dash-center-view", 13);

			focusbutton.CanFocus = false;
			focusbutton.Clicked += HandleFocusClicked;
			focusscale.CanFocus = false;
			focusscale.Adjustment.Lower = 0;
			focusscale.Adjustment.Upper = 12;
			focusscale.ValueChanged += HandleValueChanged;
			focusscale.ButtonPressEvent += HandleFocusScaleButtonPress;
			focusscale.ButtonReleaseEvent += HandleFocusScaleButtonRelease;
			timerulearea.HeightRequest = VASDrawing.Constants.TIMERULE_HEIGHT;
			leftbox.WidthRequest = Sizes.TimelineLabelsWidth;
			labelsarea.SizeRequested += (o, args) => {
				leftbox.WidthRequest = args.Requisition.Width;
			};
			hbox1.HeightRequest = VASDrawing.Constants.TIMERULE_HEIGHT;
			scrolledwindow1.Vadjustment.ValueChanged += HandleScrollEvent;
			scrolledwindow1.Hadjustment.ValueChanged += HandleScrollEvent;

			zoominimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-zoom-in", 14);
			zoomoutimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-zoom-out", 14);

			// Synchronize the zoom widget height with scrolledwindow's scrollbar's.
			scrolledwindow1.HScrollbar.SizeAllocated += (object o, SizeAllocatedArgs args) => {
				int spacing = (int)scrolledwindow1.StyleGetProperty ("scrollbar-spacing");
				zoomhbox.HeightRequest = args.Allocation.Height + spacing;
			};
		}

		/// <summary>
		/// Fit this instance.
		/// </summary>
		public virtual void Fit ()
		{
			focusbutton.Click ();
		}

		/// <summary>
		/// Zooms in.
		/// </summary>
		public virtual void ZoomIn ()
		{
			focusscale.Adjustment.Value += focusscale.Adjustment.StepIncrement;
		}

		/// <summary>
		/// Zooms out.
		/// </summary>
		public virtual void ZoomOut ()
		{
			focusscale.Adjustment.Value -= focusscale.Adjustment.StepIncrement;
		}

		protected void UpdateTime ()
		{
			currentTime = Player.AbsoluteCurrentTime;
			timeline.CurrentTime = currentTime;
			timerule.CurrentTime = currentTime;
		}

		protected void HandleScrollEvent (object sender, System.EventArgs args)
		{
			if (sender == scrolledwindow1.Vadjustment)
				labels.Scroll = scrolledwindow1.Vadjustment.Value;
			else if (sender == scrolledwindow1.Hadjustment)
				timerule.Scroll = scrolledwindow1.Hadjustment.Value;
			QueueDraw ();
		}

		protected void HandleFocusClicked (object sender, EventArgs e)
		{
			// Align the position to 40% of the scrolled width
			double pos = CurrentTime.TotalSeconds / secondsPerPixel;
			pos -= 0.4 * scrolledwindow1.Allocation.Width;
			double maxPos = timelinearea.Allocation.Width - scrolledwindow1.Allocation.Width;

			pos = Math.Min (pos, maxPos);
			scrolledwindow1.Hadjustment.Value = pos;
		}

		[GLib.ConnectBefore]
		protected void HandleFocusScaleButtonPress (object o, Gtk.ButtonPressEventArgs args)
		{
			if (args.Event.Button == 1) {
				args.Event.SetButton (2);
			} else {
				args.Event.SetButton (1);
			}
		}

		[GLib.ConnectBefore]
		protected void HandleFocusScaleButtonRelease (object o, Gtk.ButtonReleaseEventArgs args)
		{
			if (args.Event.Button == 1) {
				args.Event.SetButton (2);
			} else {
				args.Event.SetButton (1);
			}
		}

		protected void HandleValueChanged (object sender, EventArgs e)
		{
			double secondsPer100Pixels, value;

			value = focusscale.Adjustment.Upper - Math.Round (focusscale.Value);
			if (value == 0) {
				secondsPer100Pixels = 1;
			} else if (value <= 6) {
				secondsPer100Pixels = value * 10;
			} else {
				secondsPer100Pixels = (value - 5) * 60;
			}

			secondsPerPixel = secondsPer100Pixels / 100;
			timerule.SecondsPerPixel = secondsPerPixel;
			timeline.SecondsPerPixel = secondsPerPixel;
			QueueDraw ();
		}

		protected void HandleShowMenu (List<TimelineEventVM> playVMs, EventType eventType, Time time)
		{
			menu.ShowTimelineMenu (viewModel.Project.Model, playVMs, eventType, time);
		}

		protected void HandleShowTimersMenu (List<TimeNode> nodes)
		{
			Menu m = new Gtk.Menu ();
			MenuItem item = new MenuItem (Catalog.GetString ("Delete"));
			item.Activated += (object sender, EventArgs e) => {
				foreach (Timer t in viewModel.Project.Model.Timers) {
					t.Nodes.RemoveAll (nodes.Contains);
				}
				//timeline.RemoveTimers (nodes);
			};
			m.Add (item);
			m.ShowAll ();
			m.Popup ();
		}

		void HandleVideoPlayerPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			// We use AbsoluteCurrentTime but it's configured as PropertyChanged.DoNotNotify
			if (Player.NeedsSync (e, nameof (Player.CurrentTime))) {
				if (!stopWatch.IsRunning || stopWatch.ElapsedMilliseconds >= SAMPLING_INTERVAL_MS) {
					stopWatch.Reset ();
					stopWatch.Start ();
					UpdateTime ();
				}
			}
		}
	}
}
