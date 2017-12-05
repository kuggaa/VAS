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
using VAS.Core.Interfaces;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;

namespace VAS.UI.Menus
{
	public class PlaysMenu : Gtk.Menu
	{
		public event EventHandler EditPlayEvent;

		protected MenuItem edit, newPlay, del, addPLN, snapshot, render;
		protected MenuItem duplicate, moveCat, drawings;
		protected List<TimelineEventVM> playVMs;
		protected EventType eventType;
		protected Time time;
		// FIXME: Use ProjectVM
		protected Project project;

		public PlaysMenu ()
		{
			CreateMenu ();
		}

		public override void Dispose ()
		{
			Dispose (true);
			base.Dispose ();
		}

		protected virtual void Dispose (bool disposing)
		{
			if (Disposed) {
				return;
			}
			if (disposing) {
				Destroy ();
			}
			Disposed = true;
		}

		protected override void OnDestroyed ()
		{
			Log.Verbose ($"Destroying {GetType ()}");

			base.OnDestroyed ();

			Disposed = true;
		}

		protected bool Disposed { get; private set; } = false;

		public void ShowListMenu (Project project, IEnumerable<TimelineEventVM> playVMs)
		{
			ShowMenu (project, playVMs, null, null, project.EventTypes, true);
		}

		public virtual void ShowMenu (Project project, IEnumerable<TimelineEventVM> playVMs)
		{
			ShowMenu (project, playVMs, null, null, project.EventTypes, false);
		}

		public void ShowTimelineMenu (Project project, IEnumerable<TimelineEventVM> playVMs, EventType eventType, Time time)
		{
			ShowMenu (project, playVMs, eventType, time, project.EventTypes, false);
		}

		protected void ShowMenu (Project project, IEnumerable<TimelineEventVM> playVMs, EventType eventType, Time time,
										 IList<EventType> eventTypes, bool editableName)
		{
			PrepareMenu (project, playVMs, eventType, time, eventTypes, editableName);
			Popup ();
		}

		protected virtual void PrepareMenu (Project project, IEnumerable<TimelineEventVM> playVMs, EventType eventType, Time time,
										 IList<EventType> eventTypes, bool editableName)
		{
			this.playVMs = playVMs.ToList ();
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

			if (playVMs == null) {
				playVMs = new List<TimelineEventVM> ();
			}

			del.Visible = playVMs.Count () > 0;

			if (playVMs.Count () > 0) {
				string label = String.Format ("{0} ({1})", Catalog.GetString ("Delete"), playVMs.Count ());
				del.SetLabel (label);
			}
		}

		protected void EmitEditPlayEvent (object source, EventArgs eventArgs)
		{
			if (EditPlayEvent != null)
				EditPlayEvent (source, eventArgs);
		}

		protected virtual void CreateMenu ()
		{
			newPlay = new MenuItem ("");
			Add (newPlay);
			newPlay.Activated += HandleNewPlayActivated;

			del = new MenuItem ("");
			del.Activated += (sender, e) => {
				App.Current.EventsBroker.Publish<EventsDeletedEvent> (
					new EventsDeletedEvent {
						TimelineEventVMs = playVMs
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

		void EmitRenderPlaylist (List<TimelineEventVM> playVMs)
		{
			Playlist pl = new Playlist ();
			foreach (TimelineEventVM playVM in playVMs) {
				pl.Elements.Add (new PlaylistPlayElement (playVM.Model));
			}
			App.Current.EventsBroker.Publish<RenderPlaylistEvent> (
				new RenderPlaylistEvent {
					Playlist = new PlaylistVM { Model = pl }
				}
			);
		}
	}
}
