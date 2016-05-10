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

///TODO: WIP

//using System;
//using System.Collections.Generic;
//using Gtk;
//
////using LongoMatch.Core.Filters;
////using LongoMatch.Core.Store;
//using VAS.Drawing.Widgets;
//using VAS.UI.Menus;
//using VAS;
//using VAS.Core;
//using VAS.Core.Common;
//using VAS.Core.Interfaces;
//using VAS.Core.Store;
//using VAS.Drawing.Cairo;
//using Helpers = VAS.UI.Helpers;
//
//using LMCommon = VAS.Core.Common;
//using VAS.Core.Filters;
//
//namespace VAS.UI.Component
//{
//	//[System.ComponentModel.ToolboxItem (true)]
//	public abstract class Timeline : Gtk.Bin
//	{
//		protected const uint TIMEOUT_MS = 100;
//		protected PlaysTimeline timeline;
//		protected Timerule timerule;
//		protected TimelineLabels labels;
//		protected double secondsPerPixel;
//		protected uint timeoutID;
//		protected Time currentTime, nextCurrentTime;
//		protected PlaysMenu menu;
//		protected Project project;
//		protected PeriodsMenu periodsmenu;
//		protected IPlayerController player;
//
//		protected override void OnDestroyed ()
//		{
//			if (timeoutID != 0) {
//				GLib.Source.Remove (timeoutID);
//				timeoutID = 0;
//			}
//			timerule.Dispose ();
//			timeline.Dispose ();
//			labels.Dispose ();
//			base.OnDestroyed ();
//		}
//
//		public virtual Time CurrentTime {
//			set {
//				nextCurrentTime = value;
//			}
//			protected get {
//				return currentTime;
//			}
//		}
//
//		public virtual IPlayerController Player {
//			get {
//				return player;
//			}
//			set {
//				player = value;
//				timerule.Player = player;
//			}
//		}
//
//		public virtual void Fit ()
//		{
//			focusbutton.Click ();
//		}
//
//		public virtual void ZoomIn ()
//		{
//			focusscale.Adjustment.Value -= focusscale.Adjustment.StepIncrement;
//		}
//
//		public virtual void ZoomOut ()
//		{
//			focusscale.Adjustment.Value += focusscale.Adjustment.StepIncrement;
//		}
//
//		public virtual void SetProject (Project project, EventsFilter filter)
//		{
//			this.project = project;
//			timeline.LoadProject (project, filter);
//			labels.LoadProject (project, filter);
//
//			if (project == null) {
//				if (timeoutID != 0) {
//					GLib.Source.Remove (timeoutID);
//					timeoutID = 0;
//				}
//				return;
//			}
//
//			if (timeoutID == 0) {
//				timeoutID = GLib.Timeout.Add (TIMEOUT_MS, UpdateTime);
//			}
//			focusscale.Value = 6;
//			timerule.Duration = project.FileSet.Duration;
//
//			timeline.ShowMenuEvent += HandleShowMenu;
//			timeline.ShowTimersMenuEvent += HandleShowTimersMenu;
//			timeline.ShowTimerMenuEvent += HandleShowTimerMenuEvent;
//			QueueDraw ();
//		}
//
//		public virtual void LoadPlay (TimelineEvent evt)
//		{
//			timeline.LoadPlay (evt);
//		}
//
//		public virtual void AddPlay (TimelineEvent play)
//		{
//			timeline.AddPlay (play);
//			QueueDraw ();
//		}
//
//		public virtual void RemovePlays (List<TimelineEvent> plays)
//		{
//			timeline.RemovePlays (plays);
//			QueueDraw ();
//		}
//
//		public virtual void AddTimerNode (Timer timer, TimeNode tn)
//		{
//			timeline.AddTimerNode (timer, tn);
//		}
//
//		protected bool UpdateTime ()
//		{
//			if (nextCurrentTime != currentTime) {
//				currentTime = nextCurrentTime;
//				timeline.CurrentTime = currentTime;
//				timerule.CurrentTime = currentTime;
//			}
//			return true;
//		}
//
//		protected void HandleScrollEvent (object sender, System.EventArgs args)
//		{
//			if (sender == scrolledwindow1.Vadjustment)
//				labels.Scroll = scrolledwindow1.Vadjustment.Value;
//			else if (sender == scrolledwindow1.Hadjustment)
//				timerule.Scroll = scrolledwindow1.Hadjustment.Value;
//			QueueDraw ();
//		}
//
//		protected void HandleFocusClicked (object sender, EventArgs e)
//		{
//			// Align the position to 40% of the scrolled width
//			double pos = CurrentTime.TotalSeconds / secondsPerPixel;
//			pos -= 0.4 * scrolledwindow1.Allocation.Width;
//			double maxPos = timelinearea.Allocation.Width - scrolledwindow1.Allocation.Width;
//
//			pos = Math.Min (pos, maxPos);
//			scrolledwindow1.Hadjustment.Value = pos;
//		}
//
//		[GLib.ConnectBefore]
//		protected void HandleFocusScaleButtonPress (object o, Gtk.ButtonPressEventArgs args)
//		{
//			if (args.Event.Button == 1) {
//				args.Event.SetButton (2);
//			} else {
//				args.Event.SetButton (1);
//			}
//		}
//
//		[GLib.ConnectBefore]
//		protected void HandleFocusScaleButtonRelease (object o, Gtk.ButtonReleaseEventArgs args)
//		{
//			if (args.Event.Button == 1) {
//				args.Event.SetButton (2);
//			} else {
//				args.Event.SetButton (1);
//			}
//		}
//
//		protected void HandleValueChanged (object sender, EventArgs e)
//		{
//			double secondsPer100Pixels, value;
//
//			value = Math.Round (focusscale.Value);
//			if (value == 0) {
//				secondsPer100Pixels = 1;
//			} else if (value <= 6) {
//				secondsPer100Pixels = value * 10;
//			} else {
//				secondsPer100Pixels = (value - 5) * 60;
//			}
//
//			secondsPerPixel = secondsPer100Pixels / 100;
//			timerule.SecondsPerPixel = secondsPerPixel;
//			timeline.SecondsPerPixel = secondsPerPixel;
//			QueueDraw ();
//		}
//
//		protected void HandleShowMenu (List<TimelineEvent> plays, EventType eventType, Time time)
//		{
//			menu.ShowTimelineMenu (project, plays, eventType, time);
//		}
//
//		protected void HandleShowTimersMenu (List<TimeNode> nodes)
//		{
//			Menu m = new Menu ();
//			MenuItem item = new MenuItem (Catalog.GetString ("Delete"));
//			item.Activated += (object sender, EventArgs e) => {
//				foreach (Timer t in project.Timers) {
//					t.Nodes.RemoveAll (nodes.Contains);
//				}
//				timeline.RemoveTimers (nodes);
//			};
//			m.Add (item);
//			m.ShowAll ();
//			m.Popup ();
//		}
//
//		protected void HandleShowTimerMenuEvent (Timer timer, Time time)
//		{
//			periodsmenu.ShowMenu (project, timer, time, timeline.PeriodsTimeline, timeline);
//		}
//
//		protected void HandleTimeruleSeek (Time pos, bool accurate, bool synchronous = false, bool throttled = false)
//		{
//			(Config.EventsBroker).EmitLoadEvent (null);
//			(Config.EventsBroker).EmitSeekEvent (pos, accurate, synchronous, throttled);
//		}
//	}
//}
