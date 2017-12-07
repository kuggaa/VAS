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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;
using VAS.Services.State;

namespace VAS.Services.Controller
{
	/// <summary>
	/// Playlist controller.
	/// </summary>
	//FIXME: Until Presentations is migrated to MVVM, this can't inherit ControllerBase<PlaylistCollectionVM> easily.
	//		 Meanwhile in Presentations the ViewModel property is not set
	public class PlaylistController : ControllerBase//<PlaylistCollectionVM>
	{
		string confirmDeletePlaylist =
			Catalog.GetString ("Do you really want to delete the selected playlist/s?");
		string confirmDeletePlaylistElements =
			Catalog.GetString ("Do you really want to delete the selected playlist element/s");

		public PlaylistCollectionVM ViewModel {
			get;
			set;
		}

		public VideoPlayerVM PlayerVM {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the project view model.
		/// Set this ViewModel to make <see cref="PlaylistController"/> save the playlist to a project.
		/// </summary>
		/// <value>The project view model.</value>
		protected ProjectVM ProjectViewModel {
			get;
			set;
		}

		#region IController implementation

		public override async Task Start ()
		{
			await base.Start ();
			if (ViewModel != null) {
				ConnectEvents ();
			}
			App.Current.EventsBroker.SubscribeAsync<AddPlaylistElementEvent> (HandleAddPlaylistElement);
			App.Current.EventsBroker.SubscribeAsync<CreateEvent<PlaylistVM>> (HandleNewPlaylist);
			App.Current.EventsBroker.SubscribeAsync<DeletePlaylistEvent> (HandleDeletePlaylist);
			App.Current.EventsBroker.SubscribeAsync<DeleteEvent<Playlist>> (HandleDeleteSelectedItems);
			App.Current.EventsBroker.SubscribeAsync<EditEvent<PlaylistVM>> (HandleEditSelectedPlaylist);
			App.Current.EventsBroker.Subscribe<RenderPlaylistEvent> (HandleRenderPlaylist);
			App.Current.EventsBroker.Subscribe<InsertVideoInPlaylistEvent> (HandleInsertVideoInPlaylist);
			App.Current.EventsBroker.SubscribeAsync<InsertImageInPlaylistEvent> (HandleInsertImageInPlaylist);
			App.Current.EventsBroker.SubscribeAsync<EditEvent<PlaylistElementVM>> (HandleEditPlaylistElement);
			App.Current.EventsBroker.Subscribe<LoadPlaylistElementEvent> (HandleLoadPlaylistElement);
			App.Current.EventsBroker.Subscribe<LoadEventEvent> (HandleLoadPlayEvent);
			App.Current.EventsBroker.Subscribe<MoveElementsEvent<PlaylistVM, PlaylistElementVM>> (HandleMoveElements);
		}

		public override async Task Stop ()
		{
			await base.Stop ();
			if (ViewModel != null) {
				DisconnectEvents ();
			}
			App.Current.EventsBroker.UnsubscribeAsync<AddPlaylistElementEvent> (HandleAddPlaylistElement);
			App.Current.EventsBroker.UnsubscribeAsync<CreateEvent<PlaylistVM>> (HandleNewPlaylist);
			App.Current.EventsBroker.UnsubscribeAsync<DeletePlaylistEvent> (HandleDeletePlaylist);
			App.Current.EventsBroker.UnsubscribeAsync<DeleteEvent<Playlist>> (HandleDeleteSelectedItems);
			App.Current.EventsBroker.UnsubscribeAsync<EditEvent<PlaylistVM>> (HandleEditSelectedPlaylist);
			App.Current.EventsBroker.Unsubscribe<RenderPlaylistEvent> (HandleRenderPlaylist);
			App.Current.EventsBroker.Unsubscribe<InsertVideoInPlaylistEvent> (HandleInsertVideoInPlaylist);
			App.Current.EventsBroker.UnsubscribeAsync<InsertImageInPlaylistEvent> (HandleInsertImageInPlaylist);
			App.Current.EventsBroker.UnsubscribeAsync<EditEvent<PlaylistElementVM>> (HandleEditPlaylistElement);
			App.Current.EventsBroker.Unsubscribe<LoadPlaylistElementEvent> (HandleLoadPlaylistElement);
			App.Current.EventsBroker.Unsubscribe<LoadEventEvent> (HandleLoadPlayEvent);
			App.Current.EventsBroker.Unsubscribe<MoveElementsEvent<PlaylistVM, PlaylistElementVM>> (HandleMoveElements);
		}

		public override void SetViewModel (IViewModel viewModel)
		{
			if (viewModel == null) {
				return;
			}

			ViewModel = ((IPlaylistCollectionDealer)viewModel).Playlists;


			PlayerVM = ((IVideoPlayerDealer)viewModel).VideoPlayer;
		}

		public override IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

		#endregion

		protected void LoadPlay (TimelineEventVM play, Time seekTime, bool playing)
		{
			if (play?.Model != null && PlayerVM != null && PlayerVM.Player != null) {
				play.Playing = true;
				PlayerVM.Player.LoadEvent (
					play, seekTime, playing);
				if (playing) {
					PlayerVM.Player.Play ();
				}
			}
		}

		protected virtual async Task<PlaylistVM> CreateNewPlaylistVM ()
		{
			string name = Catalog.GetString ("New playlist");
			PlaylistVM playlist = null;
			bool done = false;
			while (name != null && !done) {
				name = await App.Current.Dialogs.QueryMessage (Catalog.GetString ("Playlist name:"), null, name);
				if (name != null) {
					done = true;
					if (ViewModel.ViewModels.Any (p => p.Name == name)) {
						string msg = Catalog.GetString ("A playlist already exists with the same name");
						App.Current.Dialogs.ErrorMessage (msg);
						done = false;
					}
				}
			}
			if (name != null) {
				playlist = new PlaylistVM { Model = new Playlist { Name = name } };
				ViewModel.ViewModels.Add (playlist);
				Save (playlist, true);
				await App.Current.EventsBroker.Publish (new NewPlaylistEvent ());
			}

			return playlist;
		}

		protected virtual async Task HandleAddPlaylistElement (AddPlaylistElementEvent e)
		{
			if (e.Playlist == null) {
				e.Playlist = await CreateNewPlaylistVM ();
				if (e.Playlist == null) {
					return;
				}
			}

			foreach (var item in e.PlaylistElements) {
				if (item is TimelineEventVM) {
					e.Playlist.ViewModels.Add (new PlaylistPlayElementVM () {
						Model = new PlaylistPlayElement ((item as TimelineEventVM).Model),
					});
				} else {
					e.Playlist.ViewModels.Add ((PlaylistElementVM)item);
				}
			}

			Save (e.Playlist, true);
		}

		protected virtual Task HandleDeletePlaylist (DeletePlaylistEvent e)
		{
			PlaylistVM playlist = e.Playlist;

			if (playlist != null && ProjectViewModel == null) {
				App.Current.DatabaseManager.ActiveDB.Delete (e.Playlist.Model);
			}
			ViewModel.Model.Remove (e.Playlist.Model);
			return AsyncHelpers.Return (true);
		}

		protected virtual void HandleLoadPlayEvent (LoadEventEvent e)
		{
			if (e.TimelineEvent?.Duration.MSeconds == 0) {
				// These events don't have duration, we start playing as if it was a seek
				PlayerVM.Player.Switch (null, null, null);
				PlayerVM.Player.UnloadCurrentEvent ();
				PlayerVM.Player.Seek (e.TimelineEvent.EventTime, true);
				PlayerVM.Player.Play ();
			} else {
				if (e.TimelineEvent != null) {
					LoadPlay (e.TimelineEvent, new Time (0), true);
				} else if (PlayerVM != null && PlayerVM.Player != null) {
					PlayerVM.Player.UnloadCurrentEvent ();
				}
			}
		}

		async Task HandleNewPlaylist (CreateEvent<PlaylistVM> e)
		{
			e.Object = await CreateNewPlaylistVM ();
			e.ReturnValue = e.Object != null;
		}

		void Save (PlaylistVM playlist, bool force = false)
		{
			if (playlist != null && ProjectViewModel == null) {
				App.Current.DatabaseManager.ActiveDB.Store (playlist.Model, force);
			}
		}

		void HandleLoadPlaylistElement (LoadPlaylistElementEvent e)
		{
			if (e.Element != null) {
				e.Playlist.SetActive ((PlaylistElementVM)e.Element);
			}
			if (e.Playlist.ChildModels.Count > 0 && PlayerVM != null) {
				PlayerVM.LoadPlaylistEvent (e.Playlist, e.Element, e.Playing);
			}
		}

		void HandleRenderPlaylist (RenderPlaylistEvent e)
		{
			List<EditionJob> jobs = App.Current.GUIToolkit.ConfigureRenderingJob (e.Playlist.Model);
			if (jobs == null)
				return;
			foreach (Job job in jobs) {
				App.Current.JobsManager.Add (job);
			}
		}

		void HandleInsertVideoInPlaylist (InsertVideoInPlaylistEvent e)
		{
			MediaFile file = App.Current.Dialogs.OpenMediaFile ();
			if (file != null) {
				PlaylistVideo video = new PlaylistVideo (file);
				InsertPlaylistElements (video, e.Position);
			}
		}

		async Task HandleInsertImageInPlaylist (InsertImageInPlaylistEvent e)
		{
			Image image = await App.Current.Dialogs.OpenImage ();
			if (image != null) {
				PlaylistImage plimage = new PlaylistImage (image, new Time (5000));
				InsertPlaylistElements (plimage, e.Position);
			}
		}

		async Task HandleEditPlaylistElement (EditEvent<PlaylistElementVM> e)
		{
			dynamic properties = new ExpandoObject ();
			properties.PlaylistElement = e.Object;
			await App.Current.StateController.MoveToModal (EditPlaylistElementState.NAME, properties, true);
			var playlist = ViewModel.ViewModels.FirstOrDefault ((arg) => arg.ViewModels.Contains (e.Object));
			if (playlist != null) {
				Save (playlist);
			}
		}

		void InsertPlaylistElements (IPlaylistElement element, PlaylistPosition position)
		{
			foreach (var playlist in ViewModel.LimitedViewModels) {
				bool save = false;
				foreach (var ple in playlist.Selection) {
					int index = playlist.ViewModels.IndexOf (ple);
					if (position == PlaylistPosition.After) {
						index++;
					}
					playlist.Model.Elements.Insert (index, element.Clone (SerializationType.Json));
					save = true;
				}
				if (save) {
					Save (playlist);
				}
			}
		}

		async Task HandleDeleteSelectedItems (DeleteEvent<Playlist> e)
		{
			bool removed = false;
			bool questioned = false;
			bool shouldRemove = true;

			foreach (var playlist in ViewModel.Selection.ToList ()) {
				if (!questioned) {
					if (!await App.Current.Dialogs.QuestionMessage (confirmDeletePlaylist, null)) {
						break;
					}
					questioned = true;
				}
				if (ProjectViewModel == null) {
					App.Current.DatabaseManager.ActiveDB.Delete (playlist.Model);
				}
				ViewModel.ViewModels.Remove (playlist);
				removed = true;
			}
			if (!removed) {
				foreach (var playlist in ViewModel.ViewModels) {
					if (!shouldRemove) {
						break;
					}
					foreach (var playlistElement in playlist.Selection.ToList ()) {
						if (!questioned) {
							if (!await App.Current.Dialogs.QuestionMessage (confirmDeletePlaylistElements, null)) {
								shouldRemove = false;
								break;
							}
							questioned = true;
						}
						playlist.ViewModels.Remove (playlistElement);
						removed = true;
					}
					if (removed) {
						Save (playlist, false);
						playlist.Selection.Clear ();
						removed = false;
					}
				}
			} else {
				ViewModel.Selection.Clear ();
			}
			e.ReturnValue = true;
		}

		async Task HandleEditSelectedPlaylist (EditEvent<PlaylistVM> e)
		{
			string name = await App.Current.Dialogs.QueryMessage (Catalog.GetString ("Name:"), null,
																  e.Object.Name);
			if (!String.IsNullOrEmpty (name)) {
				e.Object.Name = name;
			}
			Save (e.Object);
		}

		void HandleMoveElements (MoveElementsEvent<PlaylistVM, PlaylistElementVM> e)
		{
			int indexCopy = e.Index;
			for (int i = 0; i < indexCopy; i++) {
				if (e.ElementsToAdd.Value.Contains (e.ElementsToAdd.Key.ViewModels [i])) {
					e.Index--;
				}
			}
			foreach (var playlist in e.ElementsToRemove) {
				playlist.Key.ViewModels.RemoveRange (playlist.Value);
				Save (playlist.Key, true);
			}
			e.Index = e.Index.Clamp (0, e.ElementsToAdd.Key.ViewModels.Count);
			e.ElementsToAdd.Key.ViewModels.InsertRange (e.Index, e.ElementsToAdd.Value);
			Save (e.ElementsToAdd.Key, true);
		}
		#region FIXME
		//FIXME: Copied from ControllerBase<T>, we should change it when migrating to MVVM
		protected override void ConnectEvents ()
		{
			ViewModel.PropertyChanged += HandleViewModelChanged;
		}

		protected override void DisconnectEvents ()
		{
			ViewModel.PropertyChanged -= HandleViewModelChanged;
		}


		protected virtual void HandleViewModelChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Collection_" + nameof (ViewModel.Selection)) {
				ViewModel.DeleteCommand.EmitCanExecuteChanged ();
				ViewModel.PlaylistMenu.UpdateCanExecute ();
				ViewModel.PlaylistElementMenu.UpdateCanExecute ();
			}
		}
		#endregion
	}
}

