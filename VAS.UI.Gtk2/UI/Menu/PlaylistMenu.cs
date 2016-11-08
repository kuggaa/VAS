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
		public PlaylistMenu ()
		{
			CreateMenu ();
		}

		protected MenuItem Edit { get; set; }

		protected MenuItem Delete { get; set; }

		protected MenuItem Render { get; set; }

		protected Project Project { get; set; }

		protected Playlist Playlist { get; set; }

		public void ShowMenu (Project project, Playlist playlist, bool editableName)
		{
			PrepareMenu (project, playlist, editableName);
			Popup ();
		}

		protected virtual void PrepareMenu (Project project, Playlist playlist, bool editableName)
		{
			if (playlist == null)
				return;

			Playlist = playlist;
			Project = project;
			Delete.Visible = (project != null);
		}

		void CreateMenu ()
		{
			CreateEdit ();
			CreateRender ();
			CreateDelete ();

			ShowAll ();
		}

		protected virtual void CreateEdit ()
		{
			Edit = new MenuItem (Catalog.GetString ("Edit name"));
			Edit.Activated += (sender, e) => {
				string name = App.Current.Dialogs.QueryMessage (Catalog.GetString ("Name:"), null,
								  Playlist.Name).Result;
				if (!String.IsNullOrEmpty (name)) {
					Playlist.Name = name;
				}
			};
			Append (Edit);
		}

		protected virtual void CreateRender ()
		{
			Render = new MenuItem (Catalog.GetString ("Render"));
			Render.Activated += (sender, e) => App.Current.EventsBroker.Publish<RenderPlaylistEvent> (
				new RenderPlaylistEvent {
					Playlist = Playlist
				}
			);
			Append (Render);
		}

		protected virtual void CreateDelete ()
		{
			Delete = new MenuItem (Catalog.GetString ("Delete"));
			Delete.Activated += (sender, e) => Project.Playlists.Remove (Playlist);
			Append (Delete);
		}
	}
}
