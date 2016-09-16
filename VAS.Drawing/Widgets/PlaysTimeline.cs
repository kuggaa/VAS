//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System.Linq;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Filters;
using VAS.Core.Handlers;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;
using VAS.Drawing;
using VAS.Drawing.CanvasObjects;
using VAS.Drawing.CanvasObjects.Timeline;
using LMCommon = VAS.Core.Common;
using VASDrawing = VAS.Drawing;

namespace VAS.Drawing.Widgets
{
	/// <summary>
	/// Plays timeline.
	/// </summary>
	public class PlaysTimeline : SelectionCanvas
	{
		public event ShowTimelineMenuHandler ShowMenuEvent;
		public event ShowTimersMenuHandler ShowTimersMenuEvent;
		public event ShowTimerMenuHandler ShowTimerMenuEvent;

		protected Project project;
		protected EventsFilter playsFilter;
		protected double secondsPerPixel;
		protected Time duration, currentTime;
		protected TimelineEvent loadedEvent;
		protected bool movingTimeNode;
		protected Dictionary<TimelineObject, object> timelineToFilter;
		protected Dictionary<EventType, CategoryTimeline> eventsTimelines;

		public PlaysTimeline (IWidget widget, IPlayerController player) : base (widget)
		{
			eventsTimelines = new Dictionary<EventType, CategoryTimeline> ();
			timelineToFilter = new Dictionary<TimelineObject, object> ();
			secondsPerPixel = 0.1;
			Accuracy = Constants.TIMELINE_ACCURACY;
			SelectionMode = MultiSelectionMode.MultipleWithModifier;
			SingleSelectionObjects.Add (typeof(TimerTimeNodeObject));
			currentTime = new Time (0);
			Player = player;

			App.Current.EventsBroker.Subscribe<LoadVideoEvent> (HandleLoadVideoMessage);
			App.Current.EventsBroker.Subscribe<CloseVideoEvent> (HandleCloseVideoEvent);
		}

		public PlaysTimeline () : this (null, null)
		{
		}

		protected override void Dispose (bool disposing)
		{
			App.Current.EventsBroker.Unsubscribe<LoadVideoEvent> (HandleLoadVideoMessage);
			App.Current.EventsBroker.Unsubscribe<CloseVideoEvent> (HandleCloseVideoEvent);
			foreach (CategoryTimeline ct in eventsTimelines.Values) {
				ct.Dispose ();
			}
			CameraNode.Dispose ();
			base.Dispose (disposing);
		}

		/// <summary>
		/// Gets or sets the player.
		/// </summary>
		/// <value>The player.</value>
		public IPlayerController Player {
			get;
			set;
		}

		/// <summary>
		/// Sets the current time.
		/// </summary>
		/// <value>The current time.</value>
		public Time CurrentTime {
			set {
				Area area;
				double start, stop;

				foreach (TimelineObject tl in Objects) {
					tl.CurrentTime = value;
				}
				if (currentTime < value) {
					start = Utils.TimeToPos (currentTime, SecondsPerPixel);
					stop = Utils.TimeToPos (value, SecondsPerPixel);
				} else {
					start = Utils.TimeToPos (value, SecondsPerPixel);
					stop = Utils.TimeToPos (currentTime, SecondsPerPixel);
				}
				currentTime = value;
				if (widget != null) {
					area = new Area (new Point (start - 1, 0), stop - start + 2, widget.Height);
					widget.ReDraw (area);
				}
			}
		}

		/// <summary>
		/// Gets or sets the seconds per pixel.
		/// </summary>
		/// <value>The seconds per pixel.</value>
		public double SecondsPerPixel {
			set {
				secondsPerPixel = value;
				Update ();
			}
			get {
				return secondsPerPixel;
			}
		}

		/// <summary>
		/// Gets or sets the periods timeline.
		/// </summary>
		/// <value>The periods timeline.</value>
		public TimerTimeline PeriodsTimeline {
			get;
			set;
		}

		/// <summary>
		/// Loads the project.
		/// </summary>
		/// <param name="project">Project.</param>
		/// <param name="filter">Filter.</param>
		public void LoadProject (Project project, EventsFilter filter)
		{
			this.project = project;
			ClearObjects ();
			eventsTimelines.Clear ();
			duration = project.FileSet.Duration;
			playsFilter = filter;
			FillCanvas ();
			filter.FilterUpdated += UpdateVisibleCategories;
			if (widget != null) {
				widget.Height = Objects.Count * StyleConf.TimelineCategoryHeight;
			}
		}

		/// <summary>
		/// Loads the play.
		/// </summary>
		/// <param name="play">Play.</param>
		public void LoadPlay (TimelineEvent play)
		{
			if (play == this.loadedEvent) {
				return;
			}

			foreach (CategoryTimeline tl in eventsTimelines.Values) {
				TimelineEventObjectBase loaded = tl.Load (play);
				if (loaded != null) {
					ClearSelection ();
					UpdateSelection (new Selection (loaded, SelectionPosition.All, 0), false);
					break;
				}
			}
		}

		/// <summary>
		/// Adds the play.
		/// </summary>
		/// <param name="play">Play.</param>
		public void AddPlay (TimelineEvent play)
		{
			eventsTimelines [play.EventType].AddPlay (play);
		}

		/// <summary>
		/// Removes the timers.
		/// </summary>
		/// <param name="nodes">Nodes.</param>
		public void RemoveTimers (List<TimeNode> nodes)
		{
			foreach (TimerTimeline tl in Objects.OfType<TimerTimeline>()) {
				foreach (TimeNode node in nodes) {
					tl.RemoveNode (node);
				}
			}
			widget?.ReDraw ();
		}

		/// <summary>
		/// Adds the timer node.
		/// </summary>
		/// <param name="timer">Timer.</param>
		/// <param name="tn">Tn.</param>
		public void AddTimerNode (Timer timer, TimeNode tn)
		{
			TimerTimeline tl = Objects.OfType<TimerTimeline> ().FirstOrDefault (t => t.HasTimer (timer));
			if (tl != null) {
				tl.AddTimeNode (timer, tn);
				widget?.ReDraw ();
			}
		}

		/// <summary>
		/// Removes the plays.
		/// </summary>
		/// <param name="plays">Plays.</param>
		public void RemovePlays (List<TimelineEvent> plays)
		{
			foreach (TimelineEvent p in plays) {
				eventsTimelines [p.EventType].RemoveNode (p);
				Selections.RemoveAll (s => (s.Drawable as TimelineEventObjectBase).Event == p);
			}
		}

		/// <summary>
		/// Gets the width of the camera expressed in position.
		/// </summary>
		/// <returns>The camera width.</returns>
		public double GetCameraWidth ()
		{
			if (!project.FileSet.Any ()) {
				return 0;
			}

			CameraObject node = ((CameraObject)timelineToFilter
				.FirstOrDefault (x => x.Key.GetType () == typeof(CameraTimeline)).Key.GetNodeAtPosition (0));

			if (node == null) {
				return 0;
			}

			if (node.IsStretched ()) {
				return node.GetWidthPosition ();
			} else {
				return node.GetMaxTimePosition ();
			}
		}

		protected void Update ()
		{
			double width = duration.TotalSeconds / SecondsPerPixel;
			widget.Width = width + 10;
			foreach (TimelineObject tl in Objects) {
				tl.Width = width + 10;
				tl.SecondsPerPixel = SecondsPerPixel;
			}
		}

		protected void AddTimeline (TimelineObject tl, object filter)
		{
			AddObject (tl);
			timelineToFilter [tl] = filter;
			if (tl is CategoryTimeline) {
				eventsTimelines [filter as EventType] = tl as CategoryTimeline;
			} 
		}

		protected virtual void FillCanvas ()
		{
			TimelineObject tl;
			int i = 0;

			FillCanvasForTimers (ref i);
			FillCanvasForEventTypes (ref i);

			UpdateVisibleCategories ();
			Update ();
			HeightRequest = Objects.Count * StyleConf.TimelineCategoryHeight;
		}

		protected virtual void FillCanvasForTimers (ref int line)
		{
			TimelineObject tl;

			foreach (Timer t in project.Timers) {
				tl = new TimerTimeline (new List<Timer> { t }, false, NodeDraggingMode.All, false, duration,
					line * StyleConf.TimelineCategoryHeight,
					Utils.ColorForRow (line), App.Current.Style.PaletteBackgroundDark);
				AddTimeline (tl, t);
			}
		}

		protected virtual void FillCanvasForEventTypes (ref int line)
		{
			TimelineObject tl;

			foreach (EventType type in project.EventTypes) {
				List<TimelineEvent> timelineEventList = project.EventsByType (type);
				var timelineEventLongoMatchList = new List<TimelineEvent> ();
				timelineEventList.ForEach (x => timelineEventLongoMatchList.Add (x));
				tl = new CategoryTimeline (project, timelineEventLongoMatchList, duration,
					line * StyleConf.TimelineCategoryHeight,
					Utils.ColorForRow (line), playsFilter);
				AddTimeline (tl, type);
				line++;
			}
		}

		protected void UpdateVisibleCategories ()
		{
			int i = 0;
			foreach (TimelineObject timeline in Objects) {
				if (playsFilter.IsVisible (timelineToFilter [timeline])) {
					timeline.OffsetY = i * timeline.Height;
					timeline.Visible = true;
					timeline.BackgroundColor = Utils.ColorForRow (i);
					i++;
				} else {
					timeline.Visible = false;
				}
			}
			widget.ReDraw ();
		}

		protected void ShowTimersMenu (Point coords)
		{
			if (PeriodsTimeline != null &&
			    coords.Y >= PeriodsTimeline.OffsetY &&
			    coords.Y < PeriodsTimeline.OffsetY + PeriodsTimeline.Height) {
				Timer t = Selections.Select (p => (p.Drawable as TimerTimeNodeObject).Timer).FirstOrDefault ();
				if (ShowTimerMenuEvent != null) {
					ShowTimerMenuEvent (t, Utils.PosToTime (coords, SecondsPerPixel));
				}
			} else {
				List<TimeNode> nodes = Selections.Select (p => (p.Drawable as TimeNodeObject).TimeNode).ToList ();
				if (nodes.Count > 0 && ShowTimersMenuEvent != null) {
					ShowTimersMenuEvent (nodes);
				}
			}
		}

		protected void ShowPlaysMenu (Point coords, CategoryTimeline catTimeline)
		{
			EventType ev = null;
			List<TimelineEvent> plays;
			
			plays = Selections.Select (p => (p.Drawable as TimelineEventObjectBase).Event).ToList ();

			ev = eventsTimelines.GetKeyByValue (catTimeline);
			if (ev != null && ShowMenuEvent != null) {
				ShowMenuEvent (plays, ev, Utils.PosToTime (coords, SecondsPerPixel));
			}
		}

		protected override void SelectionChanged (List<Selection> selections)
		{
			TimelineEvent ev = null;
			if (selections.Count > 0) {
				CanvasObject d = selections.Last ().Drawable as CanvasObject;
				if (d is TimelineEventObjectBase) {
					ev = (d as TimelineEventObjectBase).Event;
					// If event is in selections list, must be selected but
					// in the first time it is incorrectly marked as false
					ev.Selected = true;
					loadedEvent = ev;
				}
			}
			App.Current.EventsBroker.Publish<LoadEventEvent> (
				new LoadEventEvent { 
					TimelineEvent = ev
				}
			);
		}

		protected override void StartMove (Selection sel)
		{
			if (sel == null)
				return;

			if (sel.Position != SelectionPosition.All) {
				widget.SetCursor (CursorType.DoubleArrow);
			}
			if (sel.Drawable is TimeNodeObject) {
				movingTimeNode = true;
				App.Current.EventsBroker.Publish<TogglePlayEvent> (
					new TogglePlayEvent {
						Playing = false
					}
				);
			}
		}

		protected override void StopMove (bool moved)
		{
			widget.SetCursor (CursorType.Arrow);
			if (movingTimeNode) {
				App.Current.EventsBroker.Publish<TogglePlayEvent> (
					new TogglePlayEvent {
						Playing = true
					}
				);
				movingTimeNode = false;
			}
		}

		protected override void ShowMenu (Point coords)
		{
			CategoryTimeline catTimeline = eventsTimelines.Values.Where (
				                               t => t.Visible &&
				                               coords.Y >= t.OffsetY &&
				                               coords.Y < t.OffsetY + t.Height).FirstOrDefault (); 

			if (catTimeline != null) {
				ShowPlaysMenu (coords, catTimeline);
			} else {
				ShowTimersMenu (coords);
			}
		}

		protected override void SelectionMoved (Selection sel)
		{
			Time moveTime;
			CanvasObject co;
			TimelineEvent play;
			
			co = (sel.Drawable as CanvasObject);
			
			if (co is TimelineEventObjectBase) {
				play = (co as TimelineEventObjectBase).Event;
				
				if (sel.Position == SelectionPosition.Right) {
					moveTime = play.Duration;
				} else {
					moveTime = new Time (0);
				}
				App.Current.EventsBroker.Publish<TimeNodeChangedEvent> (
					new TimeNodeChangedEvent {
						TimeNode = play,
						Time = moveTime
					}
				);
			} else if (co is TimeNodeObject) {
				TimeNode to = (co as TimeNodeObject).TimeNode;
				
				if (sel.Position == SelectionPosition.Right) {
					moveTime = to.Stop;
				} else {
					moveTime = to.Start;
				}
				Player?.Seek (moveTime, true);
			}
		}

		protected void HandleLoadVideoMessage (LoadVideoEvent changeVideoMessageEvent)
		{
			if (this.project != null) {
				this.project.FileSet = changeVideoMessageEvent.mfs;
				ClearObjects ();
				duration = changeVideoMessageEvent.mfs.Duration;
				FillCanvas ();
			}
		}

		protected void HandleCloseVideoEvent (CloseVideoEvent closeVideoEvent)
		{
			if (this.project != null) {
				this.project.FileSet.Clear ();
				ClearObjects ();
				duration = new Time (0);
				FillCanvas ();
			}
		}
	}
}
