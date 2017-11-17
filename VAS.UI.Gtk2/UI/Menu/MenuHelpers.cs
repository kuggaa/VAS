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
using VAS.Core.Interfaces;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;

namespace VAS.UI.Menus
{
	public static class MenuHelpers
	{
		/// <summary>
		/// Emits an event to the render a list of events as a playlist.
		/// </summary>
		/// <param name="eventVMs">List of events to render.</param>
		public static void EmitRenderPlaylist (IEnumerable<TimelineEventVM> eventVMs)
		{
			Playlist pl = new Playlist ();
			foreach (TimelineEventVM p in eventVMs) {
				pl.Elements.Add (new PlaylistPlayElement (p.Model));
			}
			App.Current.EventsBroker.Publish (
				new RenderPlaylistEvent {
					PlaylistVM = new PlaylistVM { Model = pl }
				}
			);
		}

		/// <summary>
		/// Fills the menu item "Add to playlist" with all the playlist options
		/// </summary>
		/// <param name="addToPlaylistMenu">Add to playlist menu.</param>
		/// <param name = "playlistList">List of playlists to show in the menu</param>
		/// <param name="events">Timeline events.</param>
		//FIXME: Convert this to ViewModels (both Playlist & TimelineEvent)
		static public void FillAddToPlaylistMenu (MenuItem addToPlaylistMenu, IEnumerable<PlaylistVM> playlistList,
												  IEnumerable<IPlaylistElement> events)
		{
			if (!events.Any ()) {
				addToPlaylistMenu.Visible = false;
				return;
			}

			addToPlaylistMenu.Visible = true;
			var label = String.Format ("{0} ({1})", Catalog.GetString ("Add to playlist"), events.Count ());
			addToPlaylistMenu.SetLabel (label);

			Menu plMenu = new Menu ();
			MenuItem item;
			foreach (PlaylistVM pl in playlistList) {
				item = new MenuItem (pl.Name);
				plMenu.Append (item);
				item.Activated += (sender, e) => {
					item.PublishEvent (
						new AddPlaylistElementEvent {
							PlaylistVM = pl,
							PlaylistElements = events.ToList ()
						}
					);
				};
			}
			item = new MenuItem (Catalog.GetString ("Create new playlist..."));

			// FIXME: Longomatch has not implemented the limitation service, remove the null check when it is done
			if (App.Current.LicenseLimitationsService != null) {
				//FIXME: Longomatch can have playlist at project level and application level, this should be reworked when count limitation
				//applies to project (playlist) and application level (presentations)
				CountLimitationVM limitation = App.Current.LicenseLimitationsService.Get<CountLimitationVM> ("Presentations");
				if (limitation != null) {
					item.Sensitive = limitation.Count < limitation.Maximum;
				}
			}

			plMenu.Append (item);
			item.Activated += (sender, e) => {
				item.PublishEvent (
					new AddPlaylistElementEvent {
						PlaylistVM = null,
						PlaylistElements = events.ToList ()
					}
				);
			};
			plMenu.ShowAll ();
			addToPlaylistMenu.Submenu = plMenu;
		}

		/// <summary>
		/// Fills the menu item "Export to video file" with a list of events.
		/// </summary>
		/// <param name="exportMenu">Export menu.</param>
		/// <param name="project">Project.</param>
		/// <param name="eventVMs">Timeline events.</param>
		static public void FillExportToVideoFileMenu (MenuItem exportMenu, Project project, IEnumerable<TimelineEventVM> eventVMs,
													 string exportLabel)
		{
			string label;
			exportMenu.Visible = eventVMs.Any () &&
				((project == null) || ((project != null) && (project.ProjectType != ProjectType.FakeCaptureProject)));
			label = string.Format ("{0} ({1})", exportLabel, eventVMs.Count ());
			exportMenu.SetLabel (label);
		}
	}
}
