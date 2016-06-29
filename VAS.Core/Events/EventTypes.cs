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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VAS.Core.Store;
using VAS.Core.Store.Templates;

namespace VAS.Core.Events
{
	public class Event
	{
		public object Sender { get; set; }
	}

	/* A new event has been created */
	public class NewTagEvent : Event
	{
		public EventType EventType { get; set; }

		public List<Player> Players { get; set; }

		public ObservableCollection<Team> Teams { get; set; }

		public List<Tag> Tags { get; set; }

		public Time Start { get; set; }

		public Time Stop { get; set; }

		public Time EventTime { get; set; }

		public DashboardButton Button { get; set; }
	}

	public class KeyPressedEvent : Event
	{
		public HotKey Key;

		public Gdk.ModifierType State;
	}
}
