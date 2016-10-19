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
using VAS.Services.ViewModel;

namespace VAS.UI.Menus
{
	public static class MenuHelpers
	{
		/// <summary>
		/// Emits an event to the render a list of events as a playlist.
		/// </summary>
		/// <param name="events">List of events to render.</param>
		public static void EmitRenderPlaylist (IEnumerable<TimelineEvent> events)
		{
			Playlist pl = new Playlist ();
			foreach (TimelineEvent p in events) {
				pl.Elements.Add (new PlaylistPlayElement (p));
			}
			App.Current.EventsBroker.Publish (
				new RenderPlaylistEvent {
					Playlist = pl
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
		static public void FillAddToPlaylistMenu (MenuItem addToPlaylistMenu, IEnumerable<Playlist> playlistList, IEnumerable<TimelineEvent> events)
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
			foreach (Playlist pl in playlistList) {
				item = new MenuItem (pl.Name);
				plMenu.Append (item);
				item.Activated += (sender, e) => {
					IEnumerable<IPlaylistElement> elements = events.Select (p => new PlaylistPlayElement (p));
					item.PublishEvent (
						new AddPlaylistElementEvent {
							Playlist = pl,
							PlaylistElements = elements.ToList ()
						}
					);
				};
			}
			item = new MenuItem (Catalog.GetString ("Create new playlist..."));
			plMenu.Append (item);
			item.Activated += (sender, e) => {
				IEnumerable<IPlaylistElement> elements = events.Select (p => new PlaylistPlayElement (p));
				item.PublishEvent (
					new AddPlaylistElementEvent {
						Playlist = null,
						PlaylistElements = elements.ToList ()
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
		/// <param name="events">Timeline events.</param>
		static public void FillExportToVideoFileMenu (MenuItem exportMenu, Project project, IEnumerable<TimelineEvent> events)
		{
			exportMenu.Visible = events.Any () && project.ProjectType != ProjectType.FakeCaptureProject;
			var label = string.Format ("{0} ({1})", Catalog.GetString ("Export to video file"), events.Count ());
			exportMenu.SetLabel (label);
		}

		/// <summary>
		/// Fills the menu item "Render" with all selected PlayLists and Events
		/// </summary>
		/// <param name="addToRenderMenu">Add to render menuItem.</param>
		/// <param name="events">Timeline events to render.</param>
		//FIXME: Convert this to ViewModels (both Playlist & TimelineEvent)
		static public void FillAddToRenderMenu (MenuItem addToRenderMenu, IEnumerable<TimelineEvent> events)
		{
			if (!events.Any ()) {
				addToRenderMenu.Visible = false;
				return;
			}

			addToRenderMenu.Visible = true;
			var label = String.Format ("{0} ({1})", Catalog.GetString ("Render"), events.Count ());
			addToRenderMenu.SetLabel (label);

			addToRenderMenu.Activated += (sender, e) => {
				IEnumerable<IPlaylistElement> elements = events.Select (p => new PlaylistPlayElement (p));
				Playlist pl = new Playlist ();
				pl.Elements.AddRange (elements);
				addToRenderMenu.PublishEvent<RenderPlaylistEvent> (
					new RenderPlaylistEvent {
						Playlist = pl
					}
				);
			};
		}
	}
}
