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

using System.Collections.Specialized;
using System.Linq;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Drawing.CanvasObjects.Timeline
{
	[View ("EventTypeTimelineView")]
	public class EventTypeTimelineView : TimelineView, ICanvasObjectView<EventTypeTimelineVM>
	{
		EventTypeTimelineVM viewModel;

		public EventTypeTimelineVM ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.ViewModels.CollectionChanged -= HandleEventsCollectionChanged;
				}
				viewModel = value;
				ClearObjects ();
				if (viewModel != null) {
					viewModel.ViewModels.CollectionChanged += HandleEventsCollectionChanged;
					foreach (TimelineEventVM eventVM in viewModel.ViewModels) {
						AddTimelineEvent (eventVM);
					}
				}
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (EventTypeTimelineVM)viewModel;
		}

		/// <summary>
		/// Gets the timeline event view used for the <paramref name="evt"/>.
		/// </summary>
		/// <returns>The view.</returns>
		/// <param name="evt">The timeline event.</param>
		public TimelineEventView GetView (TimelineEvent evt)
		{
			return (TimelineEventView)nodes.FirstOrDefault (e => (e as TimelineEventView).TimelineEvent.Model == evt);
		}

		/// <summary>
		/// Adds a new the timeline event view to the timeline.
		/// </summary>
		/// <param name="timelineEvent">Timeline event.</param>
		protected virtual void AddTimelineEvent (TimelineEventVM timelineEvent)
		{
			TimelineEventView po = (TimelineEventView)App.Current.ViewLocator.Retrieve ("TimelineEventView");
			po.TimelineEvent = timelineEvent;
			po.SelectionLeft = selectionBorderL;
			po.SelectionRight = selectionBorderR;
			po.OffsetY = OffsetY;
			po.Height = Height;
			po.SecondsPerPixel = SecondsPerPixel;
			po.MaxTime = Duration;
			AddNode (po);
		}

		protected virtual void HandleEventsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add: {
					foreach (TimelineEventVM timelineEvent in e.NewItems) {
						AddTimelineEvent (timelineEvent);
					}
					break;
				}
			case NotifyCollectionChangedAction.Remove: {
					foreach (TimelineEventVM timelineEvent in e.OldItems) {
						RemoveNode (timelineEvent);
					}
					break;
				}
			case NotifyCollectionChangedAction.Reset: {
					ClearObjects ();
					break;
				}
			}
			ReDraw ();
		}
	}
}
