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
using System.Linq;
using VAS.Core.Events;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// A ViewModel for a timeline row with a collection of <see cref="TimelineEvent"/>
	/// from the same <see cref="EventType"/> 
	/// </summary>
	public class EventTypeTimelineVM : NestedViewModel<TimelineEventVM>, IViewModel<EventType>
	{
		public EventTypeTimelineVM (EventTypeVM eventTypeVM)
		{
			EventTypeVM = eventTypeVM;
		}

		public EventTypeTimelineVM () : this (new EventTypeVM ())
		{
		}

		public EventType Model {
			get {
				return EventTypeVM.Model;
			}
			set {
				EventTypeVM.Model = value;
			}
		}

		/// <summary>
		/// Gets or sets the ViewModel for the <see cref="EventType"/> of the timeline.
		/// </summary>
		/// <value>The event type vm.</value>
		public EventTypeVM EventTypeVM {
			get;
			protected set;
		}

		/// <summary>
		/// Gets the total visible events inside this EventType
		/// </summary>
		/// <value>The visible events.</value>
		public int VisibleEvents {
			get {
				return ViewModels.OfType<IVisible> ().Count (vm => vm.Visible);
			}
		}

		public void LoadEventType ()
		{
			App.Current.EventsBroker.Publish (new LoadTimelineEvent<EventTypeVM> {
				Object = EventTypeVM,
				Playing = true
			});
		}

		public override int GetHashCode ()
		{
			return Model.Name.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			EventTypeTimelineVM e = obj as EventTypeTimelineVM;
			if (e == null) {
				return false;
			}
			return Model.Name == e.Model.Name;
		}
	}
}

