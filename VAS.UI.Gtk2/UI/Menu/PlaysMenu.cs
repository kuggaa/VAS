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
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.Common;
using VAS.Core;

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
				//			} else if (plays.Count == 1) {
				//				isLineup = plays [0] is LineupEvent;
				//				isSubstitution = plays [0] is SubstitutionEvent;
			}
		

			del.Visible = plays.Count () > 0;

			if (plays.Count () > 0) {
				string label = String.Format ("{0} ({1})", Catalog.GetString ("Delete"), plays.Count ());
				del.SetLabel (label);
			}
		
			/*if (moveCat.Visible) {
				Menu catMenu = new Menu ();
				foreach (EventType c in eventTypes) {
					if (plays.FirstOrDefault ().EventType == c)
						continue;
					var item = new MenuItem (c.Name);
					catMenu.Append (item);
					item.Activated += (sender, e) => {
//						Config.EventsBroker.EmitMoveToEventType (plays [0], c);
					}; 
				}
				catMenu.ShowAll ();
				moveCat.Submenu = catMenu;
			}*/
		
			/*if (drawings.Visible) {
				Menu drawingsMenu = new Menu ();
				for (int i = 0; i < plays.FirstOrDefault ().Drawings.Count; i++) {
					int index = i;
					MenuItem drawingItem = new MenuItem (Catalog.GetString ("Drawing ") + (i + 1));
					MenuItem editItem = new MenuItem (Catalog.GetString ("Edit"));
					MenuItem deleteItem = new MenuItem (Catalog.GetString ("Delete"));
					Menu drawingMenu = new Menu ();
		
					drawingsMenu.Append (drawingItem);
					drawingMenu.Append (editItem);
					drawingMenu.Append (deleteItem);
					editItem.Activated += (sender, e) => {
						Config.EventsBroker.EmitDrawFrame (plays.FirstOrDefault (), index,
							plays.FirstOrDefault ().Drawings [index].CameraConfig, false);
					}; 
					deleteItem.Activated += (sender, e) => {
						plays.FirstOrDefault ().Drawings.RemoveAt (index);
						plays.FirstOrDefault ().UpdateMiniature ();
					}; 
					drawingItem.Submenu = drawingMenu;
					drawingMenu.ShowAll ();
				}
				drawingsMenu.ShowAll ();
				drawings.Submenu = drawingsMenu;
			}
		
			FillAddToPlaylistMenu (addPLN, project, this.plays);
			*/
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
					item.Activated += (sender, e) => {
//						IEnumerable<IPlaylistElement> elements = events.Select (p => new PlaylistPlayElement (p));
//						((LMCommon.EventsBroker)Config.EventsBroker).EmitAddPlaylistElement (pl, elements.ToList ());
					};
				}
				item = new MenuItem (Catalog.GetString ("Create new playlist..."));
				plMenu.Append (item);
				item.Activated += (sender, e) => {
//					IEnumerable<IPlaylistElement> elements = events.Select (p => new PlaylistPlayElement (p));
//					((LMCommon.EventsBroker)Config.EventsBroker).EmitAddPlaylistElement (null, elements.ToList ());
				};
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
			del.Activated += (sender, e) => Config.EventsBroker.EmitEventsDeleted (plays);
			Add (del);

			ShowAll ();
		}

		void HandleNewPlayActivated (object sender, EventArgs e)
		{
			Config.EventsBroker.EmitNewEvent (eventType,
				eventTime: time,
				start: time - new Time { TotalSeconds = 10 },
				stop: time + new Time { TotalSeconds = 10 });
		}

		void EmitRenderPlaylist (List<TimelineEvent> plays)
		{
			Playlist pl = new Playlist ();
			foreach (TimelineEvent p in plays) {
				pl.Elements.Add (new PlaylistPlayElement (p));
			}
			Config.EventsBroker.EmitRenderPlaylist (pl);
		}
	}
}
