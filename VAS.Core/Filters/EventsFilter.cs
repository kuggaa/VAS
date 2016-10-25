// 
//  Copyright (C) 2012 Andoni Morales Alastruey
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
using System.Linq;
using VAS.Core.Handlers;
using VAS.Core.Store;

namespace VAS.Core.Filters
{
	public abstract class EventsFilter
	{
		public event FilterUpdatedHandler FilterUpdated;

		protected Dictionary<EventType, List<Tag>> eventsFilter;
		protected List<Tag> tagsFilter;
		protected List<Player> playersFilter;
		protected List<Period> periodsFilter;
		protected List<Timer> timersFilter;
		protected Project project;

		public EventsFilter (Project project)
		{
			this.project = project;
			eventsFilter = new Dictionary<EventType, List<Tag>> ();
			playersFilter = new List<Player> ();
			periodsFilter = new List<Period> ();
			tagsFilter = new List<Tag> ();
			timersFilter = new List<Timer> ();
			ClearAll ();
			UpdateFilters ();
		}

		public virtual bool Silent {
			set;
			get;
		}

		public virtual bool IgnoreUpdates {
			set;
			get;
		}

		public List<EventType> VisibleEventTypes {
			get;
			protected set;
		}

		public List<Player> VisiblePlayers {
			get;
			protected set;
		}

		public List<TimelineEvent> VisiblePlays {
			get;
			protected set;
		}

		public virtual void ClearAll (bool update = true)
		{
			eventsFilter.Clear ();
			playersFilter.Clear ();
			periodsFilter.Clear ();
			timersFilter.Clear ();
			tagsFilter.Clear ();
			if (update)
				Update ();
		}

		public virtual void FilterPlayer (Player player, bool visible)
		{
			if (visible) {
				if (!playersFilter.Contains (player))
					playersFilter.Add (player);
			} else {
				if (playersFilter.Contains (player))
					playersFilter.Remove (player);
			}
			Update ();
		}

		public virtual void FilterEventType (EventType evType, bool visible)
		{
			if (visible) {
				if (!eventsFilter.ContainsKey (evType))
					eventsFilter [evType] = new List<Tag> ();
			} else {
				if (eventsFilter.ContainsKey (evType))
					eventsFilter.Remove (evType);
			}
			Update ();
		}

		public virtual void FilterPeriod (Period period, bool visible)
		{
			if (visible) {
				if (!periodsFilter.Contains (period))
					periodsFilter.Add (period);
			} else {
				if (periodsFilter.Contains (period))
					periodsFilter.Remove (period);
			}
			Update ();
		}

		public virtual void FilterTag (Tag tag, bool visible)
		{
			if (visible) {
				if (!tagsFilter.Contains (tag))
					tagsFilter.Add (tag);
			} else {
				if (tagsFilter.Contains (tag))
					tagsFilter.Remove (tag);
			}
		}

		public virtual void FilterTimer (Timer timer, bool visible)
		{
			if (visible) {
				if (!timersFilter.Contains (timer))
					timersFilter.Add (timer);
			} else {
				if (timersFilter.Contains (timer))
					timersFilter.Remove (timer);
			}
			Update ();
		}

		public virtual void FilterEventTag (EventType evType, Tag tag, bool visible)
		{
			List<Tag> tags;

			if (visible) {
				FilterEventType (evType, true);
				tags = eventsFilter [evType];
				if (!tags.Contains (tag))
					tags.Add (tag);
			} else {
				if (eventsFilter.ContainsKey (evType)) {
					tags = eventsFilter [evType];
					if (tags.Contains (tag))
						tags.Remove (tag);
				}
			}
			Update ();
		}

		public virtual bool IsVisible (object o)
		{
			if (o is Player) {
				return VisiblePlayers.Contains (o as Player);
			} else if (o is TimelineEvent) {
				return VisiblePlays.Contains (o as TimelineEvent);
			}
			return true;
		}

		public virtual void Update ()
		{
			if (!IgnoreUpdates) {
				UpdateFilters ();
				EmitFilterUpdated ();
			}
		}

		protected virtual void UpdateFilters ()
		{
			UpdateVisiblePlayers ();
			UpdateVisibleCategories ();
			UpdateVisiblePlays ();
		}

		protected abstract void UpdateVisiblePlayers ();

		protected virtual void UpdateVisibleCategories ()
		{
			if (eventsFilter.Count == 0) {
				VisibleEventTypes = project.EventTypes.ToList ();
			} else {
				VisibleEventTypes = eventsFilter.Keys.ToList ();
			}
		}

		protected virtual void UpdateVisiblePlays ()
		{
			VisiblePlays = new List<TimelineEvent> ();

			foreach (TimelineEvent play in project.Timeline) {
				if (IsVisibleByPlayer (play) && IsVisibleByCategory (play) && IsVisibleByPeriod (play) &&
					IsVisibleByTag (play) && IsVisibleByTimer (play)) {
					VisiblePlays.Add (play);
				}
			}
		}

		protected virtual void EmitFilterUpdated ()
		{
			if (!Silent && FilterUpdated != null)
				FilterUpdated ();
		}

		protected virtual bool IsVisibleByCategory (TimelineEvent play)
		{
			if (!VisibleEventTypes.Contains (play.EventType))
				return false;
			if (!eventsFilter.ContainsKey (play.EventType))
				return true;

			List<Tag> tags = eventsFilter [play.EventType];
			if (tags.Count == 0 || tags.Intersect (play.Tags).Any () ||
				(!play.Tags.Any () && tags.Contains (Tag.EmptyTag))) {
				return true;
			} else {
				return false;
			}
		}

		protected virtual bool IsVisibleByTag (TimelineEvent play)
		{
			if (tagsFilter.Count <= 0)
				return true;

			if (!tagsFilter.Intersect (play.Tags).Any ()) {
				return false;
			} else {
				return true;
			}
		}

		protected abstract bool IsVisibleByPlayer (TimelineEvent play);

		protected virtual bool IsVisibleByTimer (TimelineEvent play)
		{
			if (timersFilter.Count == 0) {
				return true;
			}
			bool timer_match = false;
			foreach (Timer t in timersFilter) {
				foreach (TimeNode tn in t.Nodes) {
					if (tn.Join (play) != null) {
						timer_match = true;
					}
				}
			}
			return timer_match;
		}

		protected abstract bool IsVisibleByPeriod (TimelineEvent play);
	}
}

