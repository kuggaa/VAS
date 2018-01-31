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
using System.Threading.Tasks;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
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
			LoadIcons ();
		}

		static void LoadIcons ()
		{
			deleteIcon = App.Current.ResourcesLocator.LoadIcon ("vas-delete", App.Current.Style.IconSmallWidth);
			newIcon = App.Current.ResourcesLocator.LoadIcon ("vas-new-playlist", App.Current.Style.IconSmallWidth);
		}

		public PlaylistCollectionVM ()
		{
			if (deleteIcon == null || deleteIcon.Width == -1) {
				LoadIcons ();
			}
			DeleteCommand = new Command (Delete, HasItemsSelected);
			DeleteCommand.Icon = deleteIcon;
			DeleteCommand.ToolTipText = Catalog.GetString ("Delete Playlists");
			NewCommand = new Command (New, () => Limitation == null || Limitation.Count < Limitation.Maximum);
			NewCommand.Icon = newIcon;
			NewCommand.ToolTipText = Catalog.GetString ("New Playlist");
			PlaylistMenu = CreatePlaylistMenu ();
			PlaylistElementMenu = CreatePlaylistElementMenu ();
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			PlaylistMenu.Dispose ();
			PlaylistMenu = null;
			PlaylistElementMenu.Dispose ();
			PlaylistElementMenu = null;
			DeleteCommand = null;
			NewCommand = null;
		}

		/// <summary>
		/// Sets the limitation view model.
		/// </summary>
		/// <value>The limitation.</value>
		public override CountLimitationVM Limitation {
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
		/// Gets the playlist menu.
		/// </summary>
		/// <value>The playlist menu.</value>
		public MenuVM PlaylistMenu {
			get;
			private set;
		}

		/// <summary>
		/// Gets the playlist element menu.
		/// </summary>
		/// <value>The playlist element menu.</value>
		public MenuVM PlaylistElementMenu {
			get;
			private set;
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
				Playlist = playlist,
				Element = elementToStart,
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

		bool HasChildsItemsSelected ()
		{
			if (!Selection.Any ()) {
				foreach (var playlist in ViewModels) {
					if (playlist.Selection.Any ()) {
						return true;
					}
				}
			}
			return false;
		}

		bool CheckJustOneElementSelectedAndIsNotVideo ()
		{
			List<PlaylistElementVM> elements = new List<PlaylistElementVM> ();
			if (!Selection.Any ()) {
				foreach (var playlist in ViewModels) {
					elements.AddRange (playlist.Selection);
				}
			}
			return (elements.Count == 1 && !(elements [0] is PlaylistVideoVM));
		}

		void New ()
		{
			App.Current.EventsBroker.Publish (new CreateEvent<PlaylistVM> ());
		}

		void Delete ()
		{
			App.Current.EventsBroker.Publish (new DeleteEvent<PlaylistVM> ());
		}

		void Edit ()
		{
			App.Current.EventsBroker.Publish (new EditEvent<PlaylistVM> { Object = Selection.First () });
		}

		void Render ()
		{
			App.Current.EventsBroker.Publish (
				new RenderPlaylistEvent {
					Playlist = Selection.First ()
				}
			);
		}

		void InsertVideo (PlaylistPosition position)
		{
			App.Current.EventsBroker.Publish (
				new InsertVideoInPlaylistEvent {
					Position = position
				}
			);
		}

		Task InsertImage (PlaylistPosition position)
		{
			return App.Current.EventsBroker.Publish (
				new InsertImageInPlaylistEvent {
					Position = position
				}
			);
		}

		void EditPlaylistElement ()
		{
			App.Current.EventsBroker.Publish (new EditEvent<PlaylistElementVM> {
				Object = GetFirstSelectedPlaylistElement ()
			});
		}

		PlaylistElementVM GetFirstSelectedPlaylistElement ()
		{
			foreach (var playlist in ViewModels) {
				if (playlist.Selection.Any ()) {
					return (PlaylistElementVM)playlist.Selection.First ();
				}
			}
			return null;
		}

		MenuVM CreatePlaylistMenu ()
		{
			var editCommand = new Command (Edit, () => { return Selection.Count == 1; });
			editCommand.Text = Catalog.GetString ("Edit Name");
			var renderCommand = new Command (Render, () => { return Selection.Count == 1; });
			renderCommand.Text = Catalog.GetString ("Render");
			var menu = new MenuVM ();
			menu.ViewModels.AddRange (new List<MenuNodeVM> {
					new MenuNodeVM (editCommand),
					new MenuNodeVM (renderCommand),
					new MenuNodeVM (DeleteCommand, name:Catalog.GetString("Delete"))
				});
			return menu;
		}

		MenuVM CreatePlaylistElementMenu ()
		{
			var insertVideoCommand = new Command<PlaylistPosition> (InsertVideo, HasChildsItemsSelected);
			insertVideoCommand.Text = Catalog.GetString ("External Video");
			var insertImageCommand = new AsyncCommand<PlaylistPosition> (InsertImage, HasChildsItemsSelected);
			insertImageCommand.Text = Catalog.GetString ("External Image");
			var editPlaylistElementCommand = new Command (EditPlaylistElement, CheckJustOneElementSelectedAndIsNotVideo);
			editPlaylistElementCommand.Text = Catalog.GetString ("Edit Properties");

			var menu = new MenuVM ();
			var menuInsertBefore = new MenuVM ();
			var menuInsertAfter = new MenuVM ();
			menuInsertBefore.ViewModels.AddRange (new List<MenuNodeVM> {
					new MenuNodeVM (insertVideoCommand, PlaylistPosition.Before),
					new MenuNodeVM (insertImageCommand, PlaylistPosition.Before)
				});
			menuInsertAfter.ViewModels.AddRange (new List<MenuNodeVM> {
					new MenuNodeVM (insertVideoCommand, PlaylistPosition.After),
					new MenuNodeVM (insertImageCommand, PlaylistPosition.After)
				});
			menu.ViewModels.AddRange (new List<MenuNodeVM> {
					new MenuNodeVM (editPlaylistElementCommand),
					new MenuNodeVM (menuInsertBefore, Catalog.GetString("Insert before")),
					new MenuNodeVM (menuInsertAfter, Catalog.GetString("Insert after")),
					new MenuNodeVM (DeleteCommand, name:Catalog.GetString("Delete"))
				});
			return menu;
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

