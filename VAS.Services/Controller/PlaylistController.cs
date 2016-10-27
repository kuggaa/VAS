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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Filters;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Services.ViewModel;

namespace VAS.Services.Controller
{
	public class PlaylistController : DisposableBase, IController
	{
		PlaylistCollectionVM viewModel;

		public PlaylistController (IVideoPlayerViewModel playerVM)
		{
			PlayerVM = playerVM;
		}

		public IVideoPlayerViewModel PlayerVM {
			get;
			set;
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			Stop ();
		}

		protected EventsFilter Filter { get; set; }

		protected Project OpenedProject {
			get;
			set;
		}

		protected ProjectType OpenedProjectType {
			get;
			set;
		}

		#region IController implementation

		public void Start ()
		{
			App.Current.EventsBroker.SubscribeAsync<AddPlaylistElementEvent> (HandleAddPlaylistElement);
			App.Current.EventsBroker.SubscribeAsync<CreateEvent<Playlist>> (HandleNewPlaylist);
			App.Current.EventsBroker.SubscribeAsync<DeletePlaylistEvent> (HandleDeletePlaylist);
			App.Current.EventsBroker.Subscribe<RenderPlaylistEvent> (HandleRenderPlaylist);
			App.Current.EventsBroker.Subscribe<LoadPlaylistElementEvent> (HandleLoadPlaylistElement);
		}

		public void Stop ()
		{
			App.Current.EventsBroker.UnsubscribeAsync<AddPlaylistElementEvent> (HandleAddPlaylistElement);
			App.Current.EventsBroker.UnsubscribeAsync<CreateEvent<Playlist>> (HandleNewPlaylist);
			App.Current.EventsBroker.UnsubscribeAsync<DeletePlaylistEvent> (HandleDeletePlaylist);
			App.Current.EventsBroker.Unsubscribe<RenderPlaylistEvent> (HandleRenderPlaylist);
			App.Current.EventsBroker.Unsubscribe<LoadPlaylistElementEvent> (HandleLoadPlaylistElement);
		}

		public void SetViewModel (IViewModel viewModel)
		{
			if (viewModel == null) {
				return;
			}
			this.viewModel = (PlaylistCollectionVM)(viewModel as dynamic);
		}

		public IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

		#endregion

		protected virtual async Task<Playlist> CreateNewPlaylist ()
		{
			string name = Catalog.GetString ("New playlist");
			Playlist playlist = null;
			bool done = false;
			while (name != null && !done) {
				name = await App.Current.Dialogs.QueryMessage (Catalog.GetString ("Playlist name:"), null, name);
				if (name != null) {
					done = true;
					if (viewModel.ViewModels.Any (p => p.Name == name)) {
						string msg = Catalog.GetString ("A playlist already exists with the same name");
						App.Current.Dialogs.ErrorMessage (msg);
						done = false;
					}
				}
			}
			if (name != null) {
				playlist = new Playlist { Name = name };
				viewModel.Model.Add (playlist);
				Save (playlist, true);
			}
			return playlist;
		}

		protected virtual async Task HandleAddPlaylistElement (AddPlaylistElementEvent e)
		{
			//FIXME: should use PlaylistVM
			if (e.Playlist == null) {
				e.Playlist = await CreateNewPlaylist ();
				if (e.Playlist == null) {
					return;
				}
			}
			foreach (var item in e.PlaylistElements) {
				e.Playlist.Elements.Add (item);
			}
			Save (e.Playlist, true);
		}

		protected virtual Task HandleDeletePlaylist (DeletePlaylistEvent e)
		{
			App.Current.DatabaseManager.ActiveDB.Delete (e.Playlist);
			viewModel.Model.Remove (e.Playlist);
			return AsyncHelpers.Return (true);
		}

		async Task HandleNewPlaylist (CreateEvent<Playlist> e)
		{
			e.Object = await CreateNewPlaylist ();
			e.ReturnValue = e.Object != null;
		}

		void Save (Playlist playlist, bool force = false)
		{
			if (playlist != null) {
				App.Current.DatabaseManager.ActiveDB.Store (playlist, force);
			}
		}

		void HandleLoadPlaylistElement (LoadPlaylistElementEvent e)
		{
			if (e.Element != null) {
				e.Playlist.SetActive (e.Element);
			}
			if (e.Playlist.Elements.Count > 0 && PlayerVM != null) {
				PlayerVM.LoadPlaylistEvent (e.Playlist, e.Element, e.Playing);
			}
		}

		void HandleRenderPlaylist (RenderPlaylistEvent e)
		{
			List<EditionJob> jobs = App.Current.GUIToolkit.ConfigureRenderingJob (e.Playlist);
			if (jobs == null)
				return;
			foreach (Job job in jobs) {
				App.Current.JobsManager.Add (job);
			}
		}
	}
}

