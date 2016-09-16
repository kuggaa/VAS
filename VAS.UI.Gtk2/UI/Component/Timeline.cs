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
using Gtk;
using VAS;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Filters;
using VAS.Core.Interfaces;
using VAS.Core.Store;
using VAS.Drawing.Cairo;
using VAS.Drawing.Widgets;
using VAS.UI.Menus;
using Helpers = VAS.UI.Helpers;
using LMCommon = VAS.Core.Common;
using VASDrawing = VAS.Drawing;

namespace VAS.UI.Component
{
	/// <summary>
	/// VAS Timeline.
	/// </summary>
	[System.ComponentModel.ToolboxItem (true)]
	public partial class Timeline : Gtk.Bin
	{
		protected const uint TIMEOUT_MS = 100;
		protected PlaysTimeline timeline;
		protected Timerule timerule;
		protected TimelineLabels labels;
		protected double secondsPerPixel;
		protected uint timeoutID;
		protected Time currentTime, nextCurrentTime, relativeTime;
		protected PlaysMenu menu;
		protected Project project;
		protected IPlayerController player;
		protected bool isTimeLineEvent;

		public Timeline ()
		{
			this.Build ();
			Initialization ();
		}

		void Initialization ()
		{
			timerule = new Timerule (new WidgetWrapper (timerulearea));
			timerule.CenterPlayheadClicked += HandleFocusClicked;
			timerule.SeekEvent += HandleTimeruleSeek;
			timeline = createPlaysTimeline ();
			labels = createTimelineLabels ();

			focusbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-dash-center-view", Gtk.IconSize.Menu, 0);

			focusbutton.CanFocus = false;
			focusbutton.Clicked += HandleFocusClicked;
			focusscale.CanFocus = false;
			focusscale.Adjustment.Lower = 0;
			focusscale.Adjustment.Upper = 12;
			focusscale.ValueChanged += HandleValueChanged;
			focusscale.ButtonPressEvent += HandleFocusScaleButtonPress;
			focusscale.ButtonReleaseEvent += HandleFocusScaleButtonRelease;
			timerulearea.HeightRequest = VASDrawing.Constants.TIMERULE_HEIGHT;
			leftbox.WidthRequest = StyleConf.TimelineLabelsWidth;
			labelsarea.SizeRequested += (o, args) => {
				leftbox.WidthRequest = args.Requisition.Width;
			};
			hbox1.HeightRequest = VASDrawing.Constants.TIMERULE_HEIGHT;
			scrolledwindow1.Vadjustment.ValueChanged += HandleScrollEvent;
			scrolledwindow1.Hadjustment.ValueChanged += HandleScrollEvent;
			timeoutID = 0;

			zoominimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-zoom-in", 14);
			zoomoutimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-zoom-out", 14);

			// Synchronize the zoom widget height with scrolledwindow's scrollbar's.
			scrolledwindow1.HScrollbar.SizeAllocated += (object o, SizeAllocatedArgs args) => {
				int spacing = (int)scrolledwindow1.StyleGetProperty ("scrollbar-spacing");
				zoomhbox.HeightRequest = args.Allocation.Height + spacing;
			};
			App.Current.EventsBroker.Subscribe<PlayerTickEvent> (HandlePlayerTick);
			App.Current.EventsBroker.Subscribe<LoadEventEvent> (HandleLoadPlayEvent);
		}

		protected override void OnDestroyed ()
		{
			if (timeoutID != 0) {
				GLib.Source.Remove (timeoutID);
				timeoutID = 0;
			}
			// Unsubscribe events
			App.Current.EventsBroker.Unsubscribe<PlayerTickEvent> (HandlePlayerTick);
			App.Current.EventsBroker.Unsubscribe<LoadEventEvent> (HandleLoadPlayEvent);
			Player = null;

			base.OnDestroyed ();
		}

		public override void Dispose ()
		{
			Destroy ();
			timerule.Dispose ();
			timeline.Dispose ();
			labels.Dispose ();
			base.Dispose ();
		}

		/// <summary>
		/// Gets or sets the current time.
		/// </summary>
		/// <value>The current time.</value>
		public virtual Time CurrentTime {
			set {
				nextCurrentTime = value;
			}
			protected get {
				return currentTime;
			}
		}

		/// <summary>
		/// Gets or sets the player.
		/// </summary>
		/// <value>The player.</value>
		public virtual IPlayerController Player {
			get {
				return player;
			}
			set {
				player = value;
				timerule.Player = player;
				timeline.Player = player;
			}
		}

		/// <summary>
		/// Gets or sets the labels area.
		/// </summary>
		/// <value>The labels area.</value>
		public virtual DrawingArea LabelsArea {
			get {
				return labelsarea;
			}
			set {
				labelsarea = value;
			}
		}

		/// <summary>
		/// Gets or sets the timeline area.
		/// </summary>
		/// <value>The timeline area.</value>
		public virtual DrawingArea TimelineArea {
			get {
				return timelinearea;
			}
			set {
				timelinearea = value;
			}
		}

		/// <summary>
		/// Gets or sets the focus scale.
		/// </summary>
		/// <value>The focus scale.</value>
		protected HScale FocusScale {
			get {
				return focusscale;
			}
			set {
				focusscale = value;
			}
		}

		/// <summary>
		/// Gets or sets the left box.
		/// </summary>
		/// <value>The left box.</value>
		protected VBox LeftBox {
			get {
				return leftbox;
			}
			set {
				leftbox = value;
			}
		}

		/// <summary>
		/// Gets or sets the timerule area.
		/// </summary>
		/// <value>The timerule area.</value>
		protected virtual DrawingArea TimeruleArea {
			get {
				return timerulearea;
			}
			set {
				timerulearea = value;
			}
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
			focusscale.Adjustment.Value -= focusscale.Adjustment.StepIncrement;
		}

		/// <summary>
		/// Zooms out.
		/// </summary>
		public virtual void ZoomOut ()
		{
			focusscale.Adjustment.Value += focusscale.Adjustment.StepIncrement;
		}

		/// <summary>
		/// Fits the zoom tot the Camera timeline width.
		/// </summary>
		public virtual void FitZoom ()
		{
			double width = timeline.GetCameraWidth ();
			if (Math.Truncate ((double)(TimeruleArea.Allocation.Width)) < Math.Truncate (width)) {
				while (Math.Truncate ((double)(TimeruleArea.Allocation.Width)) < Math.Truncate (width)
				       && this.FocusScale.Adjustment.Value < 12) {
					ZoomOut ();
					width = timeline.GetCameraWidth ();
				}
			} else {
				while (Math.Truncate ((double)(TimeruleArea.Allocation.Width)) > Math.Truncate (width)
				       && this.FocusScale.Adjustment.Value > 0) {
					ZoomIn ();
					width = timeline.GetCameraWidth ();
				}
				ZoomOut ();
			}
		}

		/// <summary>
		/// Creates a PlaysTimeline.
		/// </summary>
		/// <returns>The playsTimeline.</returns>
		protected virtual PlaysTimeline createPlaysTimeline ()
		{
			return new PlaysTimeline (new WidgetWrapper (timelinearea), Player);
		}

		/// <summary>
		/// Creates the timeline labels.
		/// </summary>
		/// <returns>The timeline labels.</returns>
		protected virtual TimelineLabels createTimelineLabels ()
		{
			return new TimelineLabels (new WidgetWrapper (labelsarea));
		}


		/// <summary>
		/// Sets the project in the timeline.
		/// </summary>
		/// <param name="project">Project.</param>
		/// <param name="filter">Filter.</param>
		public virtual void SetProject (Project project, EventsFilter filter)
		{
			this.project = project;
			timeline.LoadProject (project, filter);
			labels.LoadProject (project, filter);

			if (project == null) {
				if (timeoutID != 0) {
					GLib.Source.Remove (timeoutID);
					timeoutID = 0;
				}
				return;
			}

			if (timeoutID == 0) {
				timeoutID = GLib.Timeout.Add (TIMEOUT_MS, UpdateTime);
			}
			focusscale.Value = 6;
			timerule.Duration = project.FileSet.Duration;

			timeline.ShowMenuEvent += HandleShowMenu;
			timeline.ShowTimersMenuEvent += HandleShowTimersMenu;
			QueueDraw ();
		}

		/// <summary>
		/// Loads the play.
		/// </summary>
		/// <param name="evt">Evt.</param>
		public virtual void LoadPlay (TimelineEvent evt)
		{
			timeline.LoadPlay (evt);
		}

		/// <summary>
		/// Adds the play.
		/// </summary>
		/// <param name="play">Play.</param>
		public virtual void AddPlay (TimelineEvent play)
		{
			timeline.AddPlay (play);
			QueueDraw ();
		}

		/// <summary>
		/// Removes the plays.
		/// </summary>
		/// <param name="plays">Plays.</param>
		public virtual void RemovePlays (List<TimelineEvent> plays)
		{
			timeline.RemovePlays (plays);
			QueueDraw ();
		}

		/// <summary>
		/// Adds the timer node.
		/// </summary>
		/// <param name="timer">Timer.</param>
		/// <param name="tn">Tn.</param>
		public virtual void AddTimerNode (Timer timer, TimeNode tn)
		{
			timeline.AddTimerNode (timer, tn);
		}

		protected bool UpdateTime ()
		{
			if (nextCurrentTime != currentTime) {
				currentTime = nextCurrentTime;
				if (isTimeLineEvent) {
					timeline.CurrentTime = currentTime;
					timerule.CurrentTime = currentTime;
				} else {
					timeline.CurrentTime = relativeTime;
					timerule.CurrentTime = relativeTime;
				}

			}
			return true;
		}

		protected virtual void HandleLoadPlayEvent (LoadEventEvent e)
		{
			isTimeLineEvent = e.TimelineEvent != null && e.TimelineEvent.Selected;
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

			value = Math.Round (focusscale.Value);
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

		protected void HandleShowMenu (List<TimelineEvent> plays, EventType eventType, Time time)
		{
			menu.ShowTimelineMenu (project, plays, eventType, time);
		}

		protected void HandleShowTimersMenu (List<TimeNode> nodes)
		{
			Menu m = new Gtk.Menu ();
			MenuItem item = new MenuItem (Catalog.GetString ("Delete"));
			item.Activated += (object sender, EventArgs e) => {
				foreach (Timer t in project.Timers) {
					t.Nodes.RemoveAll (nodes.Contains);
				}
				timeline.RemoveTimers (nodes);
			};
			m.Add (item);
			m.ShowAll ();
			m.Popup ();
		}

		protected void HandleTimeruleSeek (Time pos, bool accurate, bool synchronous = false, bool throttled = false)
		{
			App.Current.EventsBroker.Publish<LoadEventEvent> (new LoadEventEvent ());
			player.Seek (pos, accurate, synchronous, throttled);
		}

		void HandlePlayerTick (PlayerTickEvent e)
		{
			CurrentTime = e.Time;
			relativeTime = e.RelativeTime;
		}

		/// <summary>
		///  Temporal, until we have a panel style as in Longomatch (AnalysisComponent, CodingWidget, PlaylistManager ...)
		/// </summary>
		/// <param name="mf">Mf.</param>
		public virtual void SetMediaFile (MediaFile mf)
		{
			timeoutID = GLib.Timeout.Add (TIMEOUT_MS, UpdateTime);
			timerule.Duration = mf.Duration;

			QueueDraw ();
		}
	}
}
