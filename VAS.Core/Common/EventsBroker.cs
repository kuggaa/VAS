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
using VAS.Core.Handlers;
using VAS.Core.Store;

namespace VAS.Core.Common
{
	public class EventsBroker
	{
		public event TimeNodeStartedHandler TimeNodeStartedEvent;
		public event TimeNodeStoppedHandler TimeNodeStoppedEvent;
		public event DatabaseCreatedHandler DatabaseCreatedEvent;

		public void EmitTimeNodeStartedEvent (TimeNode node, TimerButton btn, List<DashboardButton> from)
		{
			if (TimeNodeStartedEvent != null) {
				if (from == null)
					from = new List<DashboardButton> ();
				TimeNodeStartedEvent (node, btn, from);
			}
		}

		public void EmitTimeNodeStoppedEvent (TimeNode node, TimerButton btn, List<DashboardButton> from)
		{
			if (TimeNodeStoppedEvent != null) {
				if (from == null)
					from = new List<DashboardButton> ();
				TimeNodeStoppedEvent (node, btn, from);
			}
		}

		public void EmitDatabaseCreated (string name)
		{
			if (DatabaseCreatedEvent != null) {
				DatabaseCreatedEvent (name);
			}
		}
	}
}
