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
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Store;
using VAS.UI.Menus;

namespace VAS.UI.Menus
{
	public class EventsMenu : Gtk.Menu
	{
		protected MenuItem render, drawings;
		protected List<TimelineEvent> plays;

		public EventsMenu ()
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
			MenuHelpers.FillExportToVideoFileMenu (render, null, plays, Catalog.GetString ("Render"));

			drawings.Visible = this.plays.Count == 1 && this.plays.FirstOrDefault ().Drawings.Count > 0;

			if (drawings.Visible) {
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
						var play = plays.FirstOrDefault ();
						App.Current.EventsBroker.Publish (
							new DrawFrameEvent {
								Play = play,
								DrawingIndex = index,
								CamConfig = play.Drawings [index].CameraConfig,
							}
						);
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
		}

		protected virtual void CreateMenu ()
		{
			render = new MenuItem ("");
			Add (render);
			render.Activated += (sender, e) => MenuHelpers.EmitRenderPlaylist (plays);
			drawings = new MenuItem (Catalog.GetString ("Drawings"));
			Add (drawings);

			ShowAll ();
		}
	}
}
