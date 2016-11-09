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
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;
using VAS.Services.Controller;

namespace VAS.Tests.Services
{
	public class TestPlaylistController
	{
		const string name = "name";

		Mock<IGUIToolkit> mockGuiToolkit;
		Mock<IVideoPlayerViewModel> mockPlayerController;
		Mock<IDialogs> mockDiaklogs;
		Mock<IStorageManager> storageManagerMock;
		Mock<IStorage> storageMock;
		PlaylistController controller;

		[TestFixtureSetUp ()]
		public void FixtureSetup ()
		{
			mockPlayerController = new Mock<IVideoPlayerViewModel> ();
			mockGuiToolkit = new Mock<IGUIToolkit> ();

			storageManagerMock = new Mock<IStorageManager> ();
			storageManagerMock.SetupAllProperties ();
			storageMock = new Mock<IStorage> ();
			storageManagerMock.Object.ActiveDB = storageMock.Object;
			App.Current.DatabaseManager = storageManagerMock.Object;
		}

		[SetUp ()]
		public void Setup ()
		{
			mockDiaklogs = new Mock<IDialogs> ();
			App.Current.GUIToolkit = mockGuiToolkit.Object;
			App.Current.Dialogs = mockDiaklogs.Object;
			controller = new PlaylistController (mockPlayerController.Object);
			controller.Start ();
			mockDiaklogs.Setup (m => m.QueryMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (),
													 It.IsAny<object> ())).Returns (AsyncHelpers.Return (name));
		}

		[TearDown ()]
		public void TearDown ()
		{
			controller.Stop ();
		}

		[Test ()]
		public void TestNewPlaylist ()
		{
			PlaylistCollectionVM playlistCollectionVM = new PlaylistCollectionVM ();
			controller.SetViewModel (playlistCollectionVM);

			App.Current.EventsBroker.Publish<AddPlaylistElementEvent> (
				new AddPlaylistElementEvent {
					PlaylistElements = new List<IPlaylistElement> { new PlaylistPlayElement (new TimelineEvent ()) },
					Playlist = null
				}
			);

			mockDiaklogs.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (),
				It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ()), Times.Once ());

			Assert.AreEqual (1, playlistCollectionVM.ViewModels.Count);
			Assert.AreEqual (name, playlistCollectionVM.ViewModels [0].Name);
			Assert.AreEqual (1, playlistCollectionVM.ViewModels.First ().Model.Elements.Count);
		}

		[Test ()]
		public void TestAddSamePlaylist ()
		{
			PlaylistCollectionVM playlistCollectionVM = new PlaylistCollectionVM ();
			controller.SetViewModel (playlistCollectionVM);

			App.Current.EventsBroker.Publish<AddPlaylistElementEvent> (
				new AddPlaylistElementEvent {
					PlaylistElements = new List<IPlaylistElement> { new PlaylistPlayElement (new TimelineEvent ()) },
					Playlist = null
				}
			);

			mockDiaklogs.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (),
				It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ()), Times.Once ());

			Assert.AreEqual (1, playlistCollectionVM.ViewModels.Count);
			Assert.AreEqual (name, playlistCollectionVM.ViewModels [0].Name);
			Assert.AreEqual (1, playlistCollectionVM.ViewModels.First ().Model.Elements.Count);

			App.Current.EventsBroker.Publish<AddPlaylistElementEvent> (
				new AddPlaylistElementEvent {
					PlaylistElements = new List<IPlaylistElement> { new PlaylistPlayElement (new TimelineEvent ()) },
					Playlist = playlistCollectionVM.ViewModels [0].Model
				}
			);

			mockDiaklogs.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (),
				It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ()), Times.Once ());

			Assert.AreEqual (1, playlistCollectionVM.ViewModels.Count);
			Assert.AreEqual (2, playlistCollectionVM.ViewModels.First ().Model.Elements.Count);
		}

		[Test ()]
		public void TestAddTwoPlaylist ()
		{
			PlaylistCollectionVM playlistCollectionVM = new PlaylistCollectionVM ();
			controller.SetViewModel (playlistCollectionVM);

			App.Current.EventsBroker.Publish<AddPlaylistElementEvent> (
				new AddPlaylistElementEvent {
					PlaylistElements = new List<IPlaylistElement> (),
					Playlist = null
				}
			);

			mockDiaklogs.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (),
				It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ()), Times.Once ());

			Assert.AreEqual (1, playlistCollectionVM.ViewModels.Count);
			Assert.AreEqual (name, playlistCollectionVM.ViewModels [0].Name);

			mockDiaklogs.Setup (m => m.QueryMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (),
										 It.IsAny<object> ())).Returns (AsyncHelpers.Return (name + "2"));


			App.Current.EventsBroker.Publish<AddPlaylistElementEvent> (
				new AddPlaylistElementEvent {
					PlaylistElements = new List<IPlaylistElement> (),
					Playlist = null
				}
			);

			mockDiaklogs.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (), It.IsAny<string> (),
																		It.IsAny<string> (), It.IsAny<object> ()), Times.Exactly (2));

			Assert.AreEqual (2, playlistCollectionVM.ViewModels.Count);
			Assert.AreEqual (name + "2", playlistCollectionVM.ViewModels [1].Name);
		}

		[Test ()]
		public void TestDeletePlaylist ()
		{
			PlaylistCollectionVM playlistCollectionVM = new PlaylistCollectionVM ();
			controller.SetViewModel (playlistCollectionVM);

			App.Current.EventsBroker.Publish<AddPlaylistElementEvent> (
				new AddPlaylistElementEvent {
					PlaylistElements = new List<IPlaylistElement> (),
					Playlist = null
				}
			);
			Assert.AreEqual (1, playlistCollectionVM.ViewModels.Count);

			App.Current.EventsBroker.Publish<DeletePlaylistEvent> (
				new DeletePlaylistEvent {
					Playlist = playlistCollectionVM.Model.FirstOrDefault ()
				}
			);

			Assert.AreEqual (0, playlistCollectionVM.ViewModels.Count);
		}
	}
}
