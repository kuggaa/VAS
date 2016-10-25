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
using System.Linq;
using Gtk;
using VAS.Core.Store;
using VAS.UI.Menus;

namespace VAS.UI.Menus
{
	public class EventsMenu : Gtk.Menu
	{
		protected MenuItem render;
		protected List<TimelineEvent> plays;

		public EventsMenu ()
		{
			CreateMenu ();
		}

		public void ShowMenu (Project project, List<TimelineEvent> plays)
		{
			ShowMenu (project, plays, null, null, null, false);
		}

		protected void ShowMenu (Project project, IEnumerable<TimelineEvent> plays, EventType eventType, Time time,
								 IList<EventType> eventTypes, bool editableName)
		{

			PrepareMenu (project, plays, eventType, time, eventTypes, editableName);
			Popup ();
		}

		protected virtual void PrepareMenu (Project project, IEnumerable<TimelineEvent> plays, EventType eventType, Time time,
						 IList<EventType> eventTypes, bool editableName)
		{
			this.plays = plays.ToList ();
			if (plays == null) {
				plays = new List<TimelineEvent> ();
			}
			MenuHelpers.FillExportToVideoFileMenu (render, null, plays, true);
		}

		protected virtual void CreateMenu ()
		{
			render = new MenuItem ("");
			Add (render);
			render.Activated += (sender, e) => MenuHelpers.EmitRenderPlaylist (plays);

			ShowAll ();
		}
	}
}
