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
using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using VAS;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;

namespace VAS.UI.Menus
{
	public class PlaysMenu : Gtk.Menu
	{
		public event EventHandler EditPlayEvent;

		protected MenuItem edit, newPlay, del, addPLN, snapshot, render;
		protected MenuItem duplicate, moveCat, drawings;
		protected List<TimelineEvent> plays;
		protected EventType eventType;
		protected Time time;
		protected Project project;

		public PlaysMenu ()
		{
			CreateMenu ();
		}

		public void ShowListMenu (Project project, List<TimelineEvent> plays)
		{
			ShowMenu (project, plays, null, null, project.EventTypes, true);
		}

		public void ShowMenu (Project project, List<TimelineEvent> plays)
		{
			ShowMenu (project, plays, null, null, null, false);
		}

		public void ShowTimelineMenu (Project project, List<TimelineEvent> plays, EventType eventType, Time time)
		{
			ShowMenu (project, plays, eventType, time, null, false);
		}

		protected virtual void ShowMenu (Project project, IEnumerable<TimelineEvent> plays, EventType eventType, Time time,
		                                 IList<EventType> eventTypes, bool editableName)
		{
			bool isLineup = false, isSubstitution = false;
		
			this.plays = plays.ToList ();
			this.eventType = eventType;
			this.time = time;
			this.project = project;
		
			if (eventType != null) {
				string label = String.Format ("{0} in {1}", Catalog.GetString ("Add new event"), eventType.Name);
				newPlay.SetLabel (label);
				newPlay.Visible = true;
			} else {
				newPlay.Visible = false;
			}
		
			if (plays == null) {
				plays = new List<TimelineEvent> ();
			}
		

			del.Visible = plays.Count () > 0;

			if (plays.Count () > 0) {
				string label = String.Format ("{0} ({1})", Catalog.GetString ("Delete"), plays.Count ());
				del.SetLabel (label);
			}

			Popup ();
		}

		/// <summary>
		/// Fills the menu item "Add to playlist" with all the playlist options
		/// </summary>
		/// <param name="addToPlaylistMenu">Add to playlist menu.</param>
		/// <param name="project">Project.</param>
		/// <param name="events">Timeline events.</param>
		static public void FillAddToPlaylistMenu (MenuItem addToPlaylistMenu, Project project, IList<TimelineEvent> events)
		{
			if (events.Count == 0) {
				addToPlaylistMenu.Visible = false;
				return;
			}
		
			addToPlaylistMenu.Visible = true;
			var label = String.Format ("{0} ({1})", Catalog.GetString ("Add to playlist"), events.Count);
			addToPlaylistMenu.SetLabel (label);
		
			if (project.Playlists != null) {
				Menu plMenu = new Menu ();
				MenuItem item;
				foreach (Playlist pl in project.Playlists) {
					item = new MenuItem (pl.Name);
					plMenu.Append (item);
				}
				item = new MenuItem (Catalog.GetString ("Create new playlist..."));
				plMenu.Append (item);
				plMenu.ShowAll ();
				addToPlaylistMenu.Submenu = plMenu;
			}
		}

		void CreateMenu ()
		{
			newPlay = new MenuItem ("");
			Add (newPlay);
			newPlay.Activated += HandleNewPlayActivated;

			del = new MenuItem ("");
			del.Activated += (sender, e) => {
				App.Current.EventsBroker.Publish <EventsDeletedEvent> (
					new EventsDeletedEvent {
						TimelineEvents = plays
					}
				);
			};
			Add (del);

			ShowAll ();
		}

		void HandleNewPlayActivated (object sender, EventArgs e)
		{
			App.Current.EventsBroker.Publish<NewEventEvent> (
				new NewEventEvent {
					EventType = eventType,
					EventTime = time,
					Start = time - new Time { TotalSeconds = 10 },
					Stop = time + new Time { TotalSeconds = 10 }
				}
			);
		}

		void EmitRenderPlaylist (List<TimelineEvent> plays)
		{
			Playlist pl = new Playlist ();
			foreach (TimelineEvent p in plays) {
				pl.Elements.Add (new PlaylistPlayElement (p));
			}
			App.Current.EventsBroker.Publish<RenderPlaylistEvent> (
				new RenderPlaylistEvent {
					Playlist = pl
				}
			);
		}
	}
}
