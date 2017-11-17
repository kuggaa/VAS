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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Handlers;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;
using VAS.Core.ViewModel;
using VAS.Drawing.CanvasObjects;
using VAS.Drawing.CanvasObjects.Timeline;

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

		protected int labelWidth, labelHeight, eventTypesStartIndex;
		protected double secondsPerPixel;
		protected Time duration, currentTime;
		protected TimelineEvent loadedEvent;
		protected bool movingTimeNode;
		protected Dictionary<IViewModel, TimelineView> viewModelToView;
		IAnalysisViewModel viewModel;
		CursorType cursor;
		DrawTool drawTool;
		bool cursorIsDrawTool;

		public PlaysTimeline (IWidget widget) : base (widget)
		{
			viewModelToView = new Dictionary<IViewModel, TimelineView> ();
			secondsPerPixel = 0.1;
			Accuracy = Constants.TIMELINE_ACCURACY;
			SelectionMode = MultiSelectionMode.MultipleWithModifier;
			SingleSelectionObjects.Add (typeof (TimerTimeNodeView));
			currentTime = new Time (0);
			duration = new Time (0);
			cursor = CursorType.LeftArrow;
			cursorIsDrawTool = false;
		}

		public PlaysTimeline () : this (null)
		{
		}

		public IAnalysisViewModel ViewModel {
			get {
				return viewModel;
			}
			protected set {
				UpdateModel (value);
			}
		}

		public virtual void SetViewModel (object viewModel)
		{
			ViewModel = (IAnalysisViewModel)viewModel;
		}

		/// <summary>
		/// Gets or sets the player.
		/// </summary>
		/// <value>The player.</value>
		public IVideoPlayerController Player {
			get {
				return ViewModel?.VideoPlayer.Player;
			}
		}

		/// <summary>
		/// Sets the current time.
		/// </summary>
		/// <value>The current time.</value>
		public Time CurrentTime {
			set {
				Area area;
				double start, stop;

				foreach (TimelineView tl in Objects) {
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
		public TimerTimelineView PeriodsTimeline {
			get;
			set;
		}

		protected override void ClearObjects ()
		{
			base.ClearObjects ();
			foreach (var vm in viewModelToView.Keys.ToList ()) {
				RemoveTimeline (vm);
			}
		}

		protected void Update ()
		{
			double width = duration == null ? 0 : duration.TotalSeconds / SecondsPerPixel;
			widget.Width = width + 10;
			foreach (TimelineView tl in Objects) {
				tl.Width = width + 10;
				tl.Duration = duration;
				tl.SecondsPerPixel = SecondsPerPixel;
			}
		}

		protected void RemoveTimeline (IViewModel viewModel)
		{
			RemoveObject (viewModelToView [viewModel]);
			viewModelToView.Remove (viewModel);
			var nestedVM = viewModel as INestedViewModel;
			if (nestedVM != null) {
				nestedVM.GetNotifyCollection ().CollectionChanged -= HandleChildsVMCollectionChanged;
			}
		}

		protected void AddTimeline (TimelineView timelineView, IViewModel viewModel)
		{
			AddObject (timelineView);
			if (timelineView is EventTypeTimelineView) {
				viewModelToView [viewModel] = timelineView;
			}
			var nestedVM = viewModel as INestedViewModel;
			if (nestedVM != null) {
				nestedVM.GetNotifyCollection ().CollectionChanged += HandleChildsVMCollectionChanged;
			}
		}

		protected override void CursorMoved (Point coords)
		{
			base.CursorMoved (coords);
			if (HighlightedObject is TimeNodeView) {
				Selection sel = GetSelection (coords, true);
				switch ((HighlightedObject as TimeNodeView).DraggingMode) {
				case NodeDraggingMode.None:
					SetCursor (CursorType.LeftArrow);
					break;
				case NodeDraggingMode.Borders:
					if (sel.Position == SelectionPosition.Right || sel.Position == SelectionPosition.Left) {
						SetCursor (CursorType.DoubleArrow);
					} else {
						SetCursor (CursorType.LeftArrow);
					}
					break;
				case NodeDraggingMode.Segment:
					if (sel.Position == SelectionPosition.Right || sel.Position == SelectionPosition.Left) {
						SetCursor (CursorType.LeftArrow);
					} else {
						SetCursorForTool (DrawTool.CanMove);
					}
					break;
				case NodeDraggingMode.All:
					if (sel.Position == SelectionPosition.Right || sel.Position == SelectionPosition.Left) {
						SetCursor (CursorType.DoubleArrow);
					} else {
						SetCursorForTool (DrawTool.CanMove);
					}
					break;
				}
			} else {
				SetCursor (CursorType.LeftArrow);
			}
		}

		void UpdateRowsOffsets ()
		{
			int i = 0;
			foreach (TimelineView timeline in Objects.OfType<TimelineView> ()) {
				if (timeline.Visible) {
					timeline.OffsetY = i * timeline.Height;
					timeline.BackgroundColor = Utils.ColorForRow (i);
					i++;
				}
			}
			widget.ReDraw ();
		}

		protected virtual void FillCanvas (ref int line)
		{
			FillCanvasForTimers (ref line);
			FillCanvasForEventTypes (ref line);

			UpdateRowsOffsets ();
			Update ();
			HeightRequest = Objects.Count * StyleConf.TimelineCategoryHeight;
		}

		protected virtual void FillCanvasForTimers (ref int line)
		{
			foreach (TimerVM timerVM in ViewModel.Project.Timers) {
				var timelineView = new TimerTimelineView {
					ShowName = false,
					DraggingMode = NodeDraggingMode.All,
					Duration = duration,
					OffsetY = line * StyleConf.TimelineCategoryHeight,
					Height = StyleConf.TimelineCategoryHeight,
					LineColor = App.Current.Style.PaletteBackgroundDark,
					BackgroundColor = Utils.ColorForRow (line),
				};
				timelineView.ViewModel = timerVM;
				AddTimeline (timelineView, timerVM);
				line++;
			}
		}

		protected virtual void FillCanvasForEventTypes (ref int line)
		{
			foreach (EventTypeTimelineVM timelineVM in ViewModel.Project.Timeline.EventTypesTimeline) {
				EventTypeTimelineView timelineView = AddEventTypeTimeline (timelineVM);
				timelineView.OffsetY = line * StyleConf.TimelineCategoryHeight;
				timelineView.BackgroundColor = Utils.ColorForRow (line);
				timelineView.ViewModel = timelineVM;
				line++;
			}
		}

		protected virtual EventTypeTimelineView AddEventTypeTimeline (EventTypeTimelineVM timelineVM)
		{
			var timelineView = new EventTypeTimelineView {
				Duration = duration,
				Height = StyleConf.TimelineCategoryHeight,
			};
			timelineView.ViewModel = timelineVM;
			AddTimeline (timelineView, timelineVM);
			return timelineView;
		}

		protected virtual void RemoveEventTypeTimeline (EventTypeTimelineVM timelineVM)
		{
			RemoveTimeline (timelineVM);
			UpdateRowsOffsets ();
		}

		protected void ShowTimersMenu (Point coords)
		{
			List<TimeNodeVM> nodes = Selections.Select (p => (p.Drawable as TimeNodeView).TimeNode).ToList ();
			if (nodes.Count > 0 && ShowTimersMenuEvent != null) {
				ShowTimersMenuEvent (nodes.Select (n => n.Model).ToList ());
			}
		}

		protected void ShowPlaysMenu (Point coords, EventTypeTimelineView catTimeline)
		{
			EventType ev = null;
			List<TimelineEvent> plays;

			plays = Selections.Select (p => (p.Drawable as TimelineEventView).TimelineEvent.Model).ToList ();

			ev = catTimeline.ViewModel.EventTypeVM.Model;
			if (ev != null && ShowMenuEvent != null) {
				ShowMenuEvent (plays, ev, Utils.PosToTime (coords, SecondsPerPixel));
			}
		}

		protected override void SelectionChanged (List<Selection> selections)
		{
			TimelineEventVM ev = null;

			if (selections.Count > 0) {
				CanvasObject d = selections.Last ().Drawable as CanvasObject;
				if (d is TimelineEventView) {
					ev = (d as TimelineEventView).TimelineEvent;
					// If event is in selections list, must be selected but
					// in the first time it is incorrectly marked as false
					ev.Playing = true;
					loadedEvent = ev.Model;
				}
			}
			App.Current.EventsBroker.Publish (
				new LoadEventEvent {
					TimelineEvent = ev?.Model
				}
			);
		}

		protected override void StartMove (Selection sel)
		{
			if (sel == null)
				return;

			if (sel.Position != SelectionPosition.All) {
				SetCursor (CursorType.DoubleArrow);
			}
			if (sel.Drawable is TimeNodeView) {
				movingTimeNode = true;
				if (cursorIsDrawTool) {
					SetCursorForTool (drawTool == DrawTool.CanMove ? DrawTool.Move : drawTool);
				}
				App.Current.EventsBroker.Publish (
					new TogglePlayEvent {
						Playing = false
					}
				);
			}
		}

		protected override void StopMove (bool moved)
		{
			if (cursorIsDrawTool) {
				SetCursorForTool (drawTool == DrawTool.Move ? DrawTool.CanMove : drawTool);
			} else {
				SetCursor (cursor);
			}
			if (movingTimeNode) {
				App.Current.EventsBroker.Publish (
					new TogglePlayEvent {
						Playing = true
					}
				);
				movingTimeNode = false;
			}
		}

		protected override void ShowMenu (Point coords)
		{
			TimelineView timeline = GetTimeline (coords);

			EventTypeTimelineView catTimeline = timeline as EventTypeTimelineView;
			if (catTimeline != null) {
				ShowPlaysMenu (coords, catTimeline);
			} else if (timeline as TimerTimelineView != null) {
				ShowTimersMenu (coords);
			}
		}

		protected override void SelectionMoved (Selection sel)
		{
			Time moveTime;
			CanvasObject co;

			co = (sel.Drawable as CanvasObject);

			if (co is CameraView) {
				TimeNodeVM to = (co as CameraView).TimeNode;

				if (sel.Position == SelectionPosition.Right) {
					moveTime = to.Stop;
				} else {
					moveTime = to.Start;
				}
				Player?.Seek (moveTime, true);
			}
		}

		protected virtual void HandleEventTypesPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != "Visible") {
				return;
			}
			UpdateRowsOffsets ();
		}

		protected virtual void HandleEventTypesCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add: {
					foreach (EventTypeTimelineVM viewModel in e.NewItems.OfType<EventTypeTimelineVM> ()) {
						AddEventTypeTimeline (viewModel);
					}
					break;
				}
			case NotifyCollectionChangedAction.Remove: {
					foreach (EventTypeTimelineVM viewModel in e.OldItems.OfType<EventTypeTimelineVM> ()) {
						RemoveEventTypeTimeline (viewModel);
					}
					break;
				}
			case NotifyCollectionChangedAction.Reset: {
					UpdateModel (ViewModel);
					break;
				}
			}
			UpdateRowsOffsets ();
		}

		protected TimelineView GetTimeline (Point coords)
		{
			return Objects.OfType<TimelineView> ().Where (
				t => t.Visible &&
				coords.Y >= t.OffsetY &&
				coords.Y < t.OffsetY + t.Height).FirstOrDefault ();
		}

		void SetCursor (CursorType cursorType)
		{
			if (cursorIsDrawTool || cursor != cursorType) {
				cursor = cursorType;
				cursorIsDrawTool = false;
				widget.SetCursor (cursor);
			}
		}

		void SetCursorForTool (DrawTool cursorType)
		{
			if (!cursorIsDrawTool || drawTool != cursorType) {
				drawTool = cursorType;
				cursorIsDrawTool = true;
				widget.SetCursorForTool (drawTool);
			}
		}

		void UpdateModel (IAnalysisViewModel value)
		{
			if (viewModel != null) {
				viewModel.Project.Timeline.EventTypesTimeline.ViewModels.CollectionChanged -= HandleEventTypesCollectionChanged;
				viewModel.Project.FileSet.PropertyChanged -= HandleFileSetChanged; ;
			}
			viewModel = value;
			ClearObjects ();
			if (viewModel != null) {
				viewModel.Project.Timeline.EventTypesTimeline.ViewModels.CollectionChanged += HandleEventTypesCollectionChanged;
				viewModel.Project.FileSet.PropertyChanged += HandleFileSetChanged;
				duration = viewModel.Project.FileSet.Duration;
				int i = 0;
				FillCanvas (ref i);
				if (widget != null) {
					widget.Height = Objects.Count * StyleConf.TimelineCategoryHeight;
				}
			}
		}

		void HandleFileSetChanged (object sender, PropertyChangedEventArgs e)
		{
			if (ViewModel.Project.FileSet.Duration != duration) {
				duration = ViewModel.Project.FileSet.Duration;
				Update ();
			}
		}

		void HandleChildsVMCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Remove) {
				foreach (TimeNodeVM viewModel in e.OldItems.OfType<TimeNodeVM> ()) {
					Selections.RemoveAll (s => (s.Drawable as TimeNodeView).TimeNode == viewModel);
				}
			} else if (e.Action == NotifyCollectionChangedAction.Reset) {
				Selections.Clear ();
			}
		}

	}
}
