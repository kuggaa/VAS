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
using Gtk;
using VAS.Core;
using VAS.Core.Events;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;

namespace VAS.UI.Menus
{
	public class PlaylistMenu : Gtk.Menu
	{
		MenuItem edit, delete, render;
		Project project;
		Playlist playlist;

		public PlaylistMenu ()
		{
			CreateMenu ();
		}

		void CreateMenu ()
		{
			edit = new MenuItem (Catalog.GetString ("Edit name"));
			edit.Activated += (sender, e) => {
				string name = App.Current.Dialogs.QueryMessage (Catalog.GetString ("Name:"), null,
								  playlist.Name).Result;
				if (!String.IsNullOrEmpty (name)) {
					playlist.Name = name;
				}
			};
			Append (edit);

			render = new MenuItem (Catalog.GetString ("Render"));
			render.Activated += (sender, e) => App.Current.EventsBroker.Publish<RenderPlaylistEvent> (
				new RenderPlaylistEvent {
					Playlist = playlist
				}
			);
			Append (render);

			delete = new MenuItem (Catalog.GetString ("Delete"));
			delete.Activated += (sender, e) => project.Playlists.Remove (playlist);
			Append (delete);
		}

		protected virtual void ShowMenu (Project project, Playlist playlist, bool editableName)
		{
			if (playlist == null)
				return;

			this.playlist = playlist;
			this.project = project;
			delete.Visible = (project != null);

			ShowAll ();
			Popup ();
		}
	}
}
