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
using System.ComponentModel;
using System.Linq;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Filters;
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
	public class TimelineVM : ViewModelBase<RangeObservableCollection<TimelineEvent>>
	{
		RangeObservableCollection<TimelineEvent> model;
		Dictionary<EventType, EventTypeTimelineVM> eventTypeToTimeline;
		Dictionary<Player, PlayerTimelineVM> playerToTimeline;
		CountLimitationBarChartVM limitationChart;
		CollectionViewModel<EventType, EventTypeVM> eventTypes;

		public TimelineVM ()
		{
			eventTypeToTimeline = new Dictionary<EventType, EventTypeTimelineVM> ();
			playerToTimeline = new Dictionary<Player, PlayerTimelineVM> ();
			EventTypesTimeline = new NestedViewModel<EventTypeTimelineVM> ();
			TeamsTimeline = new NestedViewModel<TeamTimelineVM> ();
			FullTimeline = CreateFullTimeline ();
			FullTimeline.ViewModels.CollectionChanged += HandleTimelineCollectionChanged;
			FullTimeline.PropertyChanged += FullTimeline_PropertyChanged;
			EditionCommand = new Command<TimelineEventVM> (HandleEditPlay);
			GroupEventsByEventTypeName = false;
			Filters = new AndPredicate<TimelineEventVM> ();
			EventsPredicate = new AndPredicate<TimelineEventVM> {
				Name = Catalog.GetString ("Events")
			};
			PeriodsPredicate = new OrPredicate<TimelineEventVM> {
				Name = Catalog.GetString ("Periods")
			};
			TimersPredicate = new OrPredicate<TimelineEventVM> {
				Name = Catalog.GetString ("Timers")
			};
			CommonTagsPredicate = new AndPredicate<TimelineEventVM> {
				Name = Catalog.GetString ("Common Tags")
			};
			EventTypesPredicate = new OrPredicate<TimelineEventVM> {
				Name = Catalog.GetString ("Event Types")
			};
			TeamsPredicate = new OrPredicate<TimelineEventVM> {
				Name = Catalog.GetString ("Teams"),
			};
		}

		protected override void DisposeManagedResources ()
		{
			Filters.IgnoreEvents = true;
			EventTypesTimeline.IgnoreEvents = true;
			FullTimeline.IgnoreEvents = true;
			if (Model != null) {
				Model.IgnoreEvents = true;
			}
			base.DisposeManagedResources ();
			if (Model != null) {
				Model.Clear ();
			}
			if (eventTypes != null) {
				eventTypes.ViewModels.CollectionChanged -= HandleEventTypesCollectionChanged;
			}
			Filters.Dispose ();
			EventTypesTimeline.Dispose ();
			FullTimeline.ViewModels.CollectionChanged -= HandleTimelineCollectionChanged;
			FullTimeline.Dispose ();
			TeamsTimeline.Dispose ();

			Model = null;
			Filters = null;
			EventTypesTimeline = null;
			FullTimeline = null;
			TeamsTimeline = null;
		}

		/// <summary>
		/// Gets or sets the edition command.
		/// </summary>
		/// <value>The edition command.</value>
		public Command EditionCommand { get; set; }

		public new RangeObservableCollection<TimelineEvent> Model {
			get {
				return model;
			}
			set {
				model = value;
				FullTimeline.Model = value;
			}
		}

		/// <summary>
		/// ViewModel for the Bar chart used to display count limitations in the Limitation Widget
		/// </summary>
		public CountLimitationBarChartVM LimitationChart {
			get {
				return limitationChart;
			}

			set {
				limitationChart = value;
				FullTimeline.Limitation = limitationChart?.Limitation;
			}
		}

		/// <summary>
		/// Gets or sets a collection ViewModel with all the events in the timeline.
		/// </summary>
		/// <value>The full timeline.</value>
		public LimitedCollectionViewModel<TimelineEvent, TimelineEventVM> FullTimeline {
			get;
			protected set;
		}

		/// <summary>
		/// Gets the agrupation of timeline events by EventTypes, this is only used for viewing purposes
		/// do not modify that collection, to Add or Remove timeline events use FullTimeline
		/// If you want to force a Remove or Add of a EventTypeTimelineVM remove first the models related to
		/// the EventType
		/// </summary>
		/// <value>The event types timeline.</value>
		public NestedViewModel<EventTypeTimelineVM> EventTypesTimeline {
			get;
			protected set;
		}

		/// <summary>
		/// Gets the agrupation of timeline events by Teams, this is only used for viewing purposes
		/// do not modify that collection, to Add or Remove timeline events use FullTimeline
		/// If you want to force a Remove or Add of a TeamTimelineVM remove first the models related to
		/// the EventType
		/// </summary>
		/// <value>The team timeline.</value>
		public NestedViewModel<TeamTimelineVM> TeamsTimeline {
			get;
			set;
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
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.TimelineVM"/> groups events by
		/// event type name instead of creating one timeline foreach each event type. When set to <c>true</c>, events
		/// with their event type name identical will be grouped in the same timeline.
		/// This option does not support yet scenarios in which the event type name can change and it should only
		/// be used when a name change never happens.
		/// </summary>
		/// <value><c>true</c> if group events by event type name; otherwise, <c>false</c>.</value>
		public bool GroupEventsByEventTypeName {
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

		/// <summary>
		/// Gets or sets the event types predicate.
		/// </summary>
		/// <value>The event types predicate.</value>
		public OrPredicate<TimelineEventVM> EventTypesPredicate {
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets the periods predicate.
		/// </summary>
		/// <value>The periods predicate.</value>
		public OrPredicate<TimelineEventVM> PeriodsPredicate {
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets the timers predicate.
		/// </summary>
		/// <value>The timers predicate.</value>
		public OrPredicate<TimelineEventVM> TimersPredicate {
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets the common tags predicate.
		/// </summary>
		/// <value>The common tags predicate.</value>
		public AndPredicate<TimelineEventVM> CommonTagsPredicate {
			get;
			private set;
		}

		/// <summary>
		/// Gets the events predicate.
		/// This is normally the parent for Periods, Timers, Common tags and Event types
		/// </summary>
		/// <value>The events predicate.</value>
		public AndPredicate<TimelineEventVM> EventsPredicate {
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets the teams predicate.
		/// </summary>
		/// <value>The teams predicate.</value>
		public OrPredicate<TimelineEventVM> TeamsPredicate {
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the filters have been initialized.
		/// </summary>
		/// <value><c>true</c> if filters initialized; otherwise, <c>false</c>.</value>
		public bool FiltersInitialized { get; set; }

		public void Clear ()
		{
			FullTimeline.Model.Clear ();
			EventTypesTimeline.ViewModels.Clear ();
			eventTypeToTimeline.Clear ();
		}

		/// <summary>
		/// Creates the child event type timelines from the list of the project's event types.
		/// </summary>
		/// <param name="projectEventTypes">Event types.</param>
		public void CreateEventTypeTimelines (CollectionViewModel<EventType, EventTypeVM> projectEventTypes)
		{
			if (projectEventTypes == eventTypes) {
				return;
			}
			if (eventTypes != null) {
				throw new InvalidOperationException ("Event types collection is already set.");
			}
			eventTypes = projectEventTypes;
			eventTypes.ViewModels.CollectionChanged += HandleEventTypesCollectionChanged;
			foreach (EventTypeVM evtType in eventTypes) {
				FindTimelineForEventType (evtType.Model);
			}
		}

		public void CreateTeamsTimelines (IEnumerable<TeamVM> teams)
		{
			if (teams == null) {
				return;
			}
			foreach (TeamVM team in teams) {
				TeamTimelineVM teamTimeline = new TeamTimelineVM (team);
				foreach (PlayerVM player in team) {
					PlayerTimelineVM playerTimeline = new PlayerTimelineVM (player);
					playerToTimeline [player.Model] = playerTimeline;
					teamTimeline.ViewModels.Add (playerTimeline);
				}
				TeamsTimeline.ViewModels.Add (teamTimeline);
			}
		}

		/// <summary>
		/// Load a TimelineEvent to the player to start playing it. The EventsController should be the responsible
		/// to Add the Events to the player
		/// </summary>
		/// <param name="viewModel">RATimelineEventVM ViewModel</param>
		/// <param name="playing">If set to <c>true</c> playing. Else starts paused</param>
		public void LoadEvent (TimelineEventVM viewModel, bool playing)
		{
			App.Current.EventsBroker.Publish (new LoadTimelineEventEvent<TimelineEventVM> {
				Object = viewModel,
				Playing = playing
			});
		}

		/// <summary>
		/// Loads a List of Events to the player in order to start playing them, The EventsController should be the responsible
		/// to Add the Events to the player
		/// </summary>
		/// <param name="viewModels">A list of RATimelineEventVM</param>
		/// <param name="playing">If set to <c>true</c> playing. Else starts paused</param>
		public void LoadEvents (IEnumerable<TimelineEventVM> viewModels, bool playing)
		{
			App.Current.EventsBroker.Publish (new LoadTimelineEventEvent<IEnumerable<TimelineEventVM>> {
				Object = viewModels,
				Playing = playing
			});
		}

		/// <summary>
		/// Unloads the events from the player
		/// </summary>
		public void UnloadEvents ()
		{
			App.Current.EventsBroker.Publish (new LoadTimelineEventEvent<TimelineEventVM> {
				Object = null,
				Playing = false
			});
		}

		/// <summary>
		/// Unselects everything from the selection as well as its children
		/// </summary>
		public void UnselectAll ()
		{
			EventTypesTimeline.Selection.Replace (new RangeObservableCollection<EventTypeTimelineVM> ());
			foreach (var eventType in EventTypesTimeline.ViewModels) {
				eventType.Selection.Replace (new RangeObservableCollection<TimelineEventVM> ());
				foreach (var timelineEvent in eventType.ViewModels) {
					timelineEvent.Selected = false;
				}
			}
		}

		/// <summary>
		/// Selects all childs ViewModels <see cref="TimelineEventVM"/> of a <see cref="EventTypeTimelineVM"/>
		/// </summary>
		/// <param name="vm">The EventTypeTimelineVM element in the collection</param>
		public void SelectAllFrom (EventTypeTimelineVM vm)
		{
			EventTypesTimeline.Select (vm);
			vm.Selection.Replace (vm.ViewModels);
			foreach (var timelineEvent in vm.Selection) {
				timelineEvent.Selected = true;
			}
		}

		/// <summary>
		/// Creates the timeline view model with all the timeline events.
		/// </summary>
		protected virtual LimitedCollectionViewModel<TimelineEvent, TimelineEventVM> CreateFullTimeline ()
		{
			return new LimitedCollectionViewModel<TimelineEvent, TimelineEventVM> (false);
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
					ReplaceTimelineEvents (FullTimeline.ViewModels);
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
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add: {
					foreach (EventTypeVM evtType in e.NewItems) {
						FindTimelineForEventType (evtType.Model);
					}
					break;
				}
			case NotifyCollectionChangedAction.Remove: {
					foreach (EventTypeVM evtType in e.OldItems) {
						RemoveTimelineForEventType (evtType.Model);
					}
					break;
				}
			case NotifyCollectionChangedAction.Reset: {
					break;
				}
			}
		}

		void HandleEditPlay (TimelineEventVM eventVM)
		{
			App.Current.EventsBroker.Publish (
				new EditEventEvent {
					TimelineEventVM = eventVM
				}
			);
		}

		void AddTimelineEventVM (TimelineEventVM viewModel)
		{
			AddToEventTypesTimeline (viewModel);
			AddToPlayersTimeline (viewModel);
		}

		void ReplaceTimelineEvents (IEnumerable<TimelineEventVM> viewModels)
		{
			ReplaceToEventTypesTimeline (viewModels);
			ReplaceToPlayersTimeline (viewModels);
		}

		EventTypeTimelineVM FindTimelineForEventType (EventType evtType)
		{
			EventTypeTimelineVM timeline = null;

			if (!eventTypeToTimeline.TryGetValue (evtType, out timeline)) {
				var eventType = evtType;
				if (GroupEventsByEventTypeName) {
					timeline = eventTypeToTimeline.FirstOrDefault (kv => kv.Key.Name == evtType.Name).Value;
				}
				if (timeline == null) {
					timeline = new EventTypeTimelineVM { Model = evtType };
					EventTypesTimeline.ViewModels.Add (timeline);
				}
				eventTypeToTimeline [evtType] = timeline;
			}
			return timeline;
		}

		void RemoveTimelineForEventType (EventType evtType)
		{
			EventTypeTimelineVM timeline = FindTimelineForEventType (evtType);
			// If the timeline is empty and it does not have an associated dashboard
			// button, get rid of it
			if (!timeline.Any () && !eventTypes.Any (evt => evt.Model == evtType)) {
				EventTypesTimeline.ViewModels.Remove (timeline);
				timeline.Dispose ();
				eventTypeToTimeline.Remove (evtType);
			}
		}

		void AddToEventTypesTimeline (TimelineEventVM timelineEventVM)
		{
			var timeline = FindTimelineForEventType (timelineEventVM.Model.EventType);
			timeline.ViewModels.Add (timelineEventVM);
		}

		void ReplaceToEventTypesTimeline (IEnumerable<TimelineEventVM> viewModels)
		{
			var groupVMs = viewModels.GroupBy (ev => ev.Model.EventType);
			foreach (var grouping in groupVMs) {
				EventTypeTimelineVM timeline = FindTimelineForEventType (grouping.Key);
				var timelineEvents = timeline.ViewModels;
				timelineEvents.Replace (grouping);
			}
			//Clear the Rest of EventTypeTimelines if necessary
			foreach (var eventType in eventTypeToTimeline.Keys.ToList ().Except (groupVMs.Select (g => g.Key))) {
				if (eventTypeToTimeline [eventType].ViewModels.Any ()) {
					eventTypeToTimeline [eventType].ViewModels.Clear ();
				}
			}
		}

		void AddToPlayersTimeline (TimelineEventVM timelineEventVM)
		{
			foreach (Player player in timelineEventVM.Model.Players) {
				if (!playerToTimeline.ContainsKey (player)) {
					// FIXME: We are calling this a thousand times. This fix works because the first times
					// we don't have the teams, but the next ones we do. We should call this only when we do.
					continue;
				}
				playerToTimeline [player].ViewModels.Add (timelineEventVM);
			}
		}

		void ReplaceToPlayersTimeline (IEnumerable<TimelineEventVM> viewModels)
		{
			Dictionary<Player, List<TimelineEventVM>> playerToEvent = new Dictionary<Player, List<TimelineEventVM>> ();
			foreach (var timeline in viewModels) {
				foreach (var player in timeline.Model.Players) {
					if (!playerToEvent.ContainsKey (player)) {
						playerToEvent.Add (player, new List<TimelineEventVM> { timeline });
					} else {
						playerToEvent [player].Add (timeline);
					}
				}
			}

			foreach (var player in playerToEvent) {
				var timelineEvents = playerToTimeline [player.Key].ViewModels;
				timelineEvents.Replace (player.Value);
			}

			//Clear the Rest of PlayersTimeline if necessary
			foreach (var player in playerToTimeline.Keys.ToList ().Except (playerToEvent.Keys.ToList ())) {
				if (playerToTimeline [player].ViewModels.Any ()) {
					playerToTimeline [player].ViewModels.Clear ();
				}
			}
		}

		void RemoveTimelineEventVM (TimelineEventVM viewModel)
		{
			foreach (Player player in viewModel.Model.Players) {
				if (playerToTimeline.ContainsKey (player)) {
					playerToTimeline [player].ViewModels.Remove (viewModel);
				}
			}
			eventTypeToTimeline [viewModel.Model.EventType].ViewModels.Remove (viewModel);
		}

		void FullTimeline_PropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			TimelineEventVM timelineEvent = sender as TimelineEventVM;
			if (timelineEvent != null && e.PropertyName == $"Collection_{nameof (TimelineEvent.Players)}") {
				// It's a bit faster to remove all the existing events and add them again in AddToPlayersTimeline
				// than traversing the whole tree to add only the new ones.
				foreach (PlayerTimelineVM timeline in playerToTimeline.Values) {
					if (timeline.ViewModels.Contains (timelineEvent)) {
						timeline.ViewModels.Remove (timelineEvent);
					}
				}
				AddToPlayersTimeline (timelineEvent);
			}
		}

		//void HandleDashboardChanged (object sender, PropertyChangedEventArgs e)
		//{
		//	if (sender is CollectionViewModel<Tag, TagVM> && e.PropertyName == "Collection_ViewModels") {
		//		CreateEventTypeTimelines (Dashboard.ViewModels.OfType<AnalysisEventButtonVM> ().Select (button => button.EventType));
		//	}
		//}
	}
}
