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
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Newtonsoft.Json;
using VAS.Core.Common;
using VAS.Core.Filters;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// A nested collection of all the timeline's <see cref="TimelineEvent"/> grouped by <see cref="EventType"/>,
	/// where the first level of the of the collection is a list of the <see cref="EventType"/> view models.
	/// Each <see cref="EventType"/> view model in the collection contains a collection of all the
	/// <see cref="TimelineEvent"/> view models from the same event type.
	/// This type of collection is used to represent timeline events in a tree view grouped by event types or in
	/// timeline widget where each row show the timeline events for a given a event type.
	/// This timeline also contains a <see cref="CollectionViewModel`1"/> with all the timeline events.
	/// </summary>
	public class TimelineVM : NestedViewModel<EventTypeTimelineVM>, IViewModel<RangeObservableCollection<TimelineEvent>>
	{
		RangeObservableCollection<TimelineEvent> model;
		Dictionary<EventType, EventTypeTimelineVM> eventTypeToTimeline;

		public TimelineVM ()
		{
			Filters = new AndPredicate<TimelineEventVM> ();
			eventTypeToTimeline = new Dictionary<EventType, EventTypeTimelineVM> ();
			ViewModels.CollectionChanged += HandleEventTypesCollectionChanged;
			FullTimeline = CreateFullTimeline ();
			FullTimeline.ViewModels.CollectionChanged += HandleTimelineCollectionChanged;
		}

		protected override void Dispose (bool disposing)
		{
			if (Disposed)
				return;
			base.Dispose (disposing);
			FullTimeline.ViewModels.CollectionChanged -= HandleTimelineCollectionChanged;
			ViewModels.CollectionChanged -= HandleEventTypesCollectionChanged;
		}

		public RangeObservableCollection<TimelineEvent> Model {
			get {
				return model;
			}
			set {
				model = value;
				FullTimeline.Model = value;
			}
		}

		/// <summary>
		/// Gets or sets a collection ViewModel with all the events in the timeline.
		/// </summary>
		/// <value>The full timeline.</value>
		public CollectionViewModel<TimelineEvent, TimelineEventVM> FullTimeline {
			get;
			protected set;
		}

		public PlaylistCollectionVM Playlists {
			get;
			set;
		}

		/// <summary>
		/// The event currently loaded.
		/// </summary>
		/// <value>The loaded event.</value>
		public TimelineEventVM LoadedEvent {
			get;
			set;
		}

		/// <summary>
		/// Filters to apply to the contained events.
		/// This AndPredicate typically contains OrPredicates, 
		/// which in turn contain the actual filters.
		/// </summary>
		/// <value>The filters.</value>
		public AndPredicate<TimelineEventVM> Filters {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		protected bool Disposed {
			get;
			private set;
		}

		/// <summary>
		/// Creates the child event type timelines from the list of the project's event types.
		/// </summary>
		/// <param name="eventTypes">Event types.</param>
		public void CreateEventTypeTimelines (CollectionViewModel<EventType, EventTypeVM> eventTypes)
		{
			ViewModels.Clear ();
			ViewModels.AddRange (eventTypes.Select (e => new EventTypeTimelineVM (e)));
		}

		/// <summary>
		/// Adds a TimelineEvent
		/// </summary>
		/// <param name="timelineEvent">TimelineEvent.</param>
		public void AddTimelineEvent (TimelineEvent timelineEvent)
		{
			FullTimeline.ViewModels.Add (new TimelineEventVM { Model = timelineEvent });
		}

		/// <summary>
		/// Creates the timeline view model with all the timeline events.
		/// </summary>
		protected virtual CollectionViewModel<TimelineEvent, TimelineEventVM> CreateFullTimeline ()
		{
			return new CollectionViewModel<TimelineEvent, TimelineEventVM> ();
		}

		/// <summary>
		/// Synchronizes the event types timeline with the project timeline. When a new event is added or removed in the
		/// timeline it adds the event to the child timeline from the same <see cref="EventType"/>
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		protected virtual void HandleTimelineCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add: {
					foreach (TimelineEventVM viewModel in e.NewItems) {
						AddTimelineEventVM (viewModel);
					}
					break;
				}
			case NotifyCollectionChangedAction.Remove: {
					foreach (TimelineEventVM viewModel in e.OldItems) {
						RemoveTimelineEventVM (viewModel);
					}
					break;
				}
			case NotifyCollectionChangedAction.Reset: {
					foreach (var viewModel in ViewModels) {
						viewModel.ViewModels.Clear ();
					}
					break;
				}
			}
		}

		/// <summary>
		/// Synchronizes the internal dictionary to retrieve a ViewModel from the <see cref="EventType"/>
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		protected virtual void HandleEventTypesCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			eventTypeToTimeline.Clear ();
			foreach (EventTypeTimelineVM timeline in ViewModels) {
				eventTypeToTimeline [timeline.Model] = timeline;
			}
		}

		void AddTimelineEventVM (TimelineEventVM viewModel)
		{
			if (!eventTypeToTimeline.ContainsKey (viewModel.Model.EventType)) {
				ViewModels.Add (new EventTypeTimelineVM { Model = viewModel.Model.EventType });
			}
			eventTypeToTimeline [viewModel.Model.EventType].ViewModels.Add (viewModel);
		}

		void RemoveTimelineEventVM (TimelineEventVM viewModel)
		{
			eventTypeToTimeline [viewModel.Model.EventType].ViewModels.Remove (viewModel);
		}
	}
}
