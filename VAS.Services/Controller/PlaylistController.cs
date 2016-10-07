//
//  Copyright (C) 2016 Fluendo S.A.
using System.Collections.Generic;
using System.Linq;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;
using VAS.Services.ViewModel;
using VAS.Core.Store.Playlists;

namespace VAS.Services.Controller
{
	public class PlaylistController : IController
	{
		PlaylistCollectionVM viewModel;

		public IPlayerViewModel PlayerVM {
			get;
			set;
		}

		public PlaylistController (IPlayerViewModel playerVM)
		{
			PlayerVM = playerVM;
		}

		#region IController implementation

		public void Start ()
		{
			App.Current.EventsBroker.Subscribe<AddPlaylistElementEvent> (HandleAddPlaylistElement);
		}

		public void Stop ()
		{
			App.Current.EventsBroker.Unsubscribe<AddPlaylistElementEvent> (HandleAddPlaylistElement);
		}

		public void SetViewModel (IViewModel viewModel)
		{
			this.viewModel = (PlaylistCollectionVM)viewModel;
		}

		public IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{
		}

		#endregion

		protected virtual void HandleAddPlaylistElement (AddPlaylistElementEvent e)
		{
			//FIXME: should use PlaylistVM
			if (e.Playlist == null) {
				e.Playlist = HandleNewPlaylist (
					new NewPlaylistEvent {
					}
				);
				if (e.Playlist == null) {
					return;
				}
			}
			foreach (var item in e.PlaylistElements) {
				e.Playlist.Elements.Add (item);
			}
		}

		protected virtual Playlist HandleNewPlaylist (NewPlaylistEvent e)
		{
			string name = Catalog.GetString ("New playlist");
			Playlist playlist = null;
			bool done = false;
			while (name != null && !done) {
				name = App.Current.Dialogs.QueryMessage (Catalog.GetString ("Playlist name:"), null, name).Result;
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
			}
			return playlist;
		}
	}
}

