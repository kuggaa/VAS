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
//

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.MVVMC;
using VAS.Core.Store.Playlists;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// ViewModel for a collection of PlaylistVM, each with a Playlist as a Model.
	/// </summary>
	public class PlaylistCollectionVM : LimitedCollectionViewModel<Playlist, PlaylistVM>
	{
		static Image deleteIcon;
		static Image newIcon;

		static PlaylistCollectionVM ()
		{
			deleteIcon = Resources.LoadIcon ("vas-delete", App.Current.Style.IconSmallWidth);
			newIcon = Resources.LoadIcon ("vas-new-playlist", App.Current.Style.IconSmallWidth);
		}

		public PlaylistCollectionVM ()
		{
			DeleteCommand = new Command (Delete, () => HasItemsSelected ());
			DeleteCommand.Icon = deleteIcon;
			DeleteCommand.ToolTipText = Catalog.GetString ("Delete Playlists");
			NewCommand = new Command (New, () => Limitation == null || Limitation.Count < Limitation.Maximum);
			NewCommand.Icon = newIcon;
			NewCommand.ToolTipText = Catalog.GetString ("New Playlist");
		}

		/// <summary>
		/// Sets the limitation view model.
		/// </summary>
		/// <value>The limitation.</value>
		public override LicenseLimitationVM Limitation {
			set {
				if (Limitation != null) {
					Limitation.PropertyChanged -= HandleLimitationChanged;
				}
				base.Limitation = value;
				if (Limitation != null) {
					Limitation.PropertyChanged += HandleLimitationChanged;
					HandleLimitationChanged ();
				}
			}
		}

		/// <summary>
		/// Gets or sets the command to create playlists
		/// </summary>
		/// <value>The new command.</value>
		[PropertyChanged.DoNotNotify]
		public Command NewCommand {
			get;
			protected set;
		}

		/// <summary>
		/// Gets or sets the command to delete playlists
		/// </summary>
		/// <value>The delete command.</value>
		[PropertyChanged.DoNotNotify]
		public Command DeleteCommand {
			get;
			protected set;
		}
		/// <summary>
		/// Loads the playlist into the VideoPlayer
		/// </summary>
		/// <param name="playlist">Playlist ViewModel</param>
		/// <param name="elementToStart">PlaylistElementVM to start playing</param>
		/// <param name="playing">If set to <c>true</c> playing.</param>
		public void LoadPlaylist (PlaylistVM playlist, PlaylistElementVM elementToStart, bool playing)
		{
			App.Current.EventsBroker.Publish (new LoadPlaylistElementEvent {
				Playlist = playlist.Model,
				Element = elementToStart.Model,
				Playing = playing
			});
		}

		/// <summary>
		/// Moves the playlist elements, from different PlaylistVM to a unique destination PlaylistVM
		/// </summary>
		/// <param name="elementsToRemove">Elements to remove.</param>
		/// <param name="elementsToAdd">Elements to add.</param>
		/// <param name="index">Index.</param>
		public void MovePlaylistElements (Dictionary<PlaylistVM, IEnumerable<PlaylistElementVM>> elementsToRemove,
										  KeyValuePair<PlaylistVM, IEnumerable<PlaylistElementVM>> elementsToAdd, int index)
		{
			App.Current.EventsBroker.Publish (new MoveElementsEvent<PlaylistVM, PlaylistElementVM> {
				ElementsToAdd = elementsToAdd,
				ElementsToRemove = elementsToRemove,
				Index = index
			});
		}

		protected bool HasItemsSelected ()
		{
			bool selection = false;

			selection = Selection.Any ();
			if (!selection) {
				foreach (var playlist in ViewModels) {
					if (playlist.Selection.Any ()) {
						selection = true;
						break;
					}
				}
			}
			return selection;
		}

		void New ()
		{
			App.Current.EventsBroker.Publish (new CreateEvent<Playlist> ());
		}

		void Delete ()
		{
			App.Current.EventsBroker.Publish (new DeleteEvent<Playlist> ());
		}

		/// <summary>
		/// Handles a property changed in the limitation view model.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		void HandleLimitationChanged (object sender = null, PropertyChangedEventArgs e = null)
		{
			NewCommand.EmitCanExecuteChanged ();
		}
	}
}

