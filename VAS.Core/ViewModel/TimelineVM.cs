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
	public class TimelineVM : BindableBase, IViewModel<RangeObservableCollection<TimelineEvent>>
	{
		RangeObservableCollection<TimelineEvent> model;
		Dictionary<string, EventTypeTimelineVM> eventTypeToTimeline;
		Dictionary<Player, PlayerTimelineVM> playerToTimeline;

		public TimelineVM ()
		{
			Filters = new AndPredicate<TimelineEventVM> ();
			eventTypeToTimeline = new Dictionary<string, EventTypeTimelineVM> ();
			playerToTimeline = new Dictionary<Player, PlayerTimelineVM> ();
			EventTypesTimeline = new NestedViewModel<EventTypeTimelineVM> ();
			EventTypesTimeline.ViewModels.CollectionChanged += HandleEventTypesCollectionChanged;
			TeamsTimeline = new NestedViewModel<TeamTimelineVM> ();
			FullTimeline = CreateFullTimeline ();
			FullTimeline.ViewModels.CollectionChanged += HandleTimelineCollectionChanged;
			FullTimeline.PropertyChanged += FullTimeline_PropertyChanged;
			EditionCommand = new Command<TimelineEvent> (HandleEditPlay, (arg) => true);
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			FullTimeline.ViewModels.CollectionChanged -= HandleTimelineCollectionChanged;
			EventTypesTimeline.ViewModels.CollectionChanged -= HandleEventTypesCollectionChanged;
			Filters.Dispose ();
			EventTypesTimeline.Dispose ();
			FullTimeline.Dispose ();
		}

		/// <summary>
		/// Gets or sets the edition command.
		/// </summary>
		/// <value>The edition command.</value>
		public Command EditionCommand { get; set; }

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

		/// <summary>
		/// Gets or sets the event types timeline.
		/// </summary>
		/// <value>The event types timeline.</value>
		public NestedViewModel<EventTypeTimelineVM> EventTypesTimeline {
			get;
			protected set;
		}

		/// <summary>
		/// Gets or sets the event types timeline.
		/// </summary>
		/// <value>The event types timeline.</value>
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
		/// Filters to apply to the contained events.
		/// This AndPredicate typically contains OrPredicates, 
		/// which in turn contain the actual filters.
		/// </summary>
		/// <value>The filters.</value>
		public AndPredicate<TimelineEventVM> Filters {
			get;
			set;
		}

		public void Clear ()
		{
			EventTypesTimeline.ViewModels.Clear ();
			FullTimeline.ViewModels.Clear ();
		}

		/// <summary>
		/// Creates the child event type timelines from the list of the project's event types.
		/// </summary>
		/// <param name="eventTypes">Event types.</param>
		public void CreateEventTypeTimelines (CollectionViewModel<EventType, EventTypeVM> eventTypes)
		{
			EventTypesTimeline.ViewModels.Clear ();
			EventTypesTimeline.ViewModels.AddRange (eventTypes.Select (e => new EventTypeTimelineVM (e)));
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
			EventTypesTimeline.SelectionReplace (new RangeObservableCollection<EventTypeTimelineVM> ());
			foreach (var eventType in EventTypesTimeline.ViewModels) {
				eventType.SelectionReplace (new RangeObservableCollection<TimelineEventVM> ());
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
			vm.SelectionReplace (vm.ViewModels);
			foreach (var timelineEvent in vm.Selection) {
				timelineEvent.Selected = true;
			}
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
					foreach (var viewModel in EventTypesTimeline.ViewModels) {
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
			RecreateInternalDictionary ();
		}

		protected override void ForwardPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			base.ForwardPropertyChanged (sender, e);
			if (sender is AnalysisEventType && e.PropertyName == nameof (EventType.Name)) {
				RecreateInternalDictionary ();
			}
		}

		void HandleEditPlay (TimelineEvent playEvent)
		{
			App.Current.EventsBroker.Publish (
				new EditEventEvent {
					TimelineEvent = playEvent
				}
			);
		}

		void RecreateInternalDictionary ()
		{
			eventTypeToTimeline.Clear ();
			foreach (EventTypeTimelineVM timeline in EventTypesTimeline.ViewModels) {
				eventTypeToTimeline [timeline.Model.Name] = timeline;
			}
		}

		void AddTimelineEventVM (TimelineEventVM viewModel)
		{
			if (!eventTypeToTimeline.ContainsKey (viewModel.Model.EventType.Name)) {
				EventTypesTimeline.ViewModels.Add (new EventTypeTimelineVM { Model = viewModel.Model.EventType });
			}
			eventTypeToTimeline [viewModel.Model.EventType.Name].ViewModels.Add (viewModel);
			AddToPlayersTimeline (viewModel);
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

		void RemoveTimelineEventVM (TimelineEventVM viewModel)
		{
			foreach (Player player in viewModel.Model.Players) {
				playerToTimeline [player].ViewModels.Remove (viewModel);
			}
			eventTypeToTimeline [viewModel.Model.EventType.Name].ViewModels.Remove (viewModel);
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
	}
}
