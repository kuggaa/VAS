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
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;
using VAS.Services.Controller;
using VAS.Services.ViewModel;

namespace VAS.Tests.Services
{
	[TestFixture]
	public class TestPlaylistController
	{
		const string name = "name";

		Mock<IGUIToolkit> mockGuiToolkit;
		VideoPlayerVM videoPlayerVM;
		Mock<IDialogs> mockDialogs;
		Mock<IStorageManager> storageManagerMock;
		Mock<IStorage> storageMock;
		PlaylistController controller;
		PlaylistCollectionVM playlistCollectionVM;
		ProjectVM projectVM;

		[TestFixtureSetUp]
		public void FixtureSetup ()
		{
			mockGuiToolkit = new Mock<IGUIToolkit> ();

			storageManagerMock = new Mock<IStorageManager> ();
			storageManagerMock.SetupAllProperties ();
			storageMock = new Mock<IStorage> ();
			storageManagerMock.Object.ActiveDB = storageMock.Object;
			App.Current.DatabaseManager = storageManagerMock.Object;
		}

		[SetUp]
		public void Setup ()
		{
			mockDialogs = new Mock<IDialogs> ();
			App.Current.GUIToolkit = mockGuiToolkit.Object;
			App.Current.Dialogs = mockDialogs.Object;
			var videoController = new Mock<IVideoPlayerController> ().Object;
			videoPlayerVM = new VideoPlayerVM ();
			videoController.SetViewModel (videoPlayerVM);
			mockDialogs.Setup (m => m.QueryMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (),
													 It.IsAny<object> ())).Returns (AsyncHelpers.Return (name));
			mockDialogs.Setup (m => m.QuestionMessage (It.IsAny<string> (), null, null)).Returns (AsyncHelpers.Return (true));
		}

		[TearDown]
		public void TearDown ()
		{
			controller.Stop ();
			controller = null;
			storageMock.ResetCalls ();
			storageManagerMock.ResetCalls ();
			mockGuiToolkit.ResetCalls ();
		}

		void SetupWithStorage ()
		{
			controller = new PlaylistController ();
			playlistCollectionVM = new PlaylistCollectionVM ();
			controller.SetViewModel (new DummyPlaylistsManagerVM {
				Playlists = playlistCollectionVM,
				Player = videoPlayerVM
			});
			controller.Start ();
		}

		void SetupWithProject ()
		{
			controller = new PlaylistControllerWithProject ();
			Project project = Utils.CreateProject (true);
			projectVM = new DummyProjectVM { Model = project };
			playlistCollectionVM = projectVM.Playlists;
			var viewModel = new ProjectAnalysisVM<ProjectVM> { Project = projectVM, VideoPlayer = videoPlayerVM };
			controller.SetViewModel (viewModel);
			controller.Start ();
		}

		[Test]
		public async Task TestAddEventsToNewPlaylistWithStorage ()
		{
			SetupWithStorage ();

			await App.Current.EventsBroker.Publish (
				new AddPlaylistElementEvent {
					PlaylistElements = new List<IPlaylistElement> { new PlaylistPlayElement (new TimelineEvent ()) },
					Playlist = null
				}
			);

			mockDialogs.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (),
				It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ()), Times.Once ());

			Assert.AreEqual (1, playlistCollectionVM.ViewModels.Count);
			Assert.AreEqual (name, playlistCollectionVM.ViewModels [0].Name);
			// FIXME: Should be one when we fix the issues with events re-forwarded
			storageMock.Verify (s => s.Store<Playlist> (It.IsAny<Playlist> (), true), Times.AtLeastOnce ());
			Assert.AreEqual (1, playlistCollectionVM.ViewModels.First ().Model.Elements.Count);
		}

		[Test]
		public async Task TestAddEventsToNewPlaylistWithProject ()
		{
			SetupWithProject ();

			await App.Current.EventsBroker.Publish (
				new AddPlaylistElementEvent {
					PlaylistElements = new List<IPlaylistElement> { new PlaylistPlayElement (new TimelineEvent ()) },
					Playlist = null
				}
			);

			mockDialogs.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (), It.IsAny<string> (),
																	   It.IsAny<string> (), It.IsAny<object> ()),
								Times.Once ());

			storageMock.Verify (s => s.Store<Playlist> (It.IsAny<Playlist> (), It.IsAny<bool> ()), Times.Never ());
			Assert.AreEqual (1, playlistCollectionVM.ViewModels.Count);
			Assert.AreEqual (1, projectVM.Playlists.Count ());
			Assert.AreEqual (name, playlistCollectionVM.ViewModels [0].Name);
			Assert.AreEqual (1, playlistCollectionVM.ViewModels.First ().Model.Elements.Count);
		}

		[Test]
		public async Task TestAddEventsToExistingPlaylistWithStorage ()
		{
			SetupWithStorage ();
			var newPlaylist = new Playlist ();
			newPlaylist.Elements.Add (new PlaylistPlayElement (new TimelineEvent ()));
			playlistCollectionVM.Model.Add (newPlaylist);

			await App.Current.EventsBroker.Publish (
				new AddPlaylistElementEvent {
					PlaylistElements = new List<IPlaylistElement> { new PlaylistPlayElement (new TimelineEvent ()) },
					Playlist = playlistCollectionVM.ViewModels [0].Model
				}
			);

			mockDialogs.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (), It.IsAny<string> (),
																	   It.IsAny<string> (), It.IsAny<object> ()), Times.Never ());

			storageMock.Verify (s => s.Store<Playlist> (newPlaylist, true), Times.Once ());
			Assert.AreEqual (1, playlistCollectionVM.ViewModels.Count);
			Assert.AreEqual (2, playlistCollectionVM.ViewModels.First ().ViewModels.Count);
		}

		[Test]
		public async Task TestAddEventsToExistingPlaylistWithProject ()
		{
			SetupWithProject ();
			var newPlaylist = new Playlist ();
			newPlaylist.Elements.Add (new PlaylistPlayElement (new TimelineEvent ()));
			projectVM.Playlists.Model.Add (newPlaylist);

			await App.Current.EventsBroker.Publish (
				new AddPlaylistElementEvent {
					PlaylistElements = new List<IPlaylistElement> { new PlaylistPlayElement (new TimelineEvent ()) },
					Playlist = newPlaylist,
				}
			);

			mockDialogs.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (), It.IsAny<string> (),
																	   It.IsAny<string> (), It.IsAny<object> ()), Times.Never ());

			storageMock.Verify (s => s.Store<Playlist> (It.IsAny<Playlist> (), true), Times.Never ());
			Assert.AreEqual (1, playlistCollectionVM.ViewModels.Count);
			Assert.AreEqual (1, projectVM.Playlists.Model.Count);
			Assert.AreEqual (2, playlistCollectionVM.ViewModels.First ().ViewModels.Count);
			Assert.AreEqual (2, projectVM.Playlists.Model [0].Elements.Count);
		}

		[Test]
		public async Task TestAddNewSecondPlaylistWithStorage ()
		{
			SetupWithStorage ();
			var newPlaylist = new Playlist ();
			newPlaylist.Elements.Add (new PlaylistPlayElement (new TimelineEvent ()));
			playlistCollectionVM.Model.Add (newPlaylist);

			await App.Current.EventsBroker.Publish (
				new AddPlaylistElementEvent {
					PlaylistElements = new List<IPlaylistElement> (),
					Playlist = null
				}
			);

			mockDialogs.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (), It.IsAny<string> (),
																		It.IsAny<string> (), It.IsAny<object> ()), Times.Exactly (1));
			storageMock.Verify (s => s.Store<Playlist> (It.IsAny<Playlist> (), true), Times.AtLeastOnce ());
			Assert.AreEqual (2, playlistCollectionVM.ViewModels.Count);
		}

		[Test]
		public async Task TestAddNewSecondPlaylistWithProject ()
		{
			SetupWithProject ();
			var newPlaylist = new Playlist ();
			newPlaylist.Elements.Add (new PlaylistPlayElement (new TimelineEvent ()));
			projectVM.Playlists.Model.Add (newPlaylist);

			await App.Current.EventsBroker.Publish (
				new AddPlaylistElementEvent {
					PlaylistElements = new List<IPlaylistElement> (),
					Playlist = null
				}
			);

			mockDialogs.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (), It.IsAny<string> (),
																		It.IsAny<string> (), It.IsAny<object> ()), Times.Exactly (1));
			Assert.AreEqual (2, playlistCollectionVM.ViewModels.Count);
			Assert.AreEqual (2, projectVM.Model.Playlists.Count);
		}

		[Test]
		public async Task TestDeletePlaylistWithStorage ()
		{
			SetupWithStorage ();
			var newPlaylist = new Playlist ();
			newPlaylist.Elements.Add (new PlaylistPlayElement (new TimelineEvent ()));
			playlistCollectionVM.Model.Add (newPlaylist);

			await App.Current.EventsBroker.Publish (
				new DeletePlaylistEvent {
					Playlist = newPlaylist,
				}
			);

			storageMock.Verify (s => s.Delete<Playlist> (It.IsAny<Playlist> ()), Times.Once ());
			Assert.AreEqual (0, playlistCollectionVM.ViewModels.Count);
		}


		[Test]
		public async Task TestDeletePlaylistWithProject ()
		{
			SetupWithProject ();
			var newPlaylist = new Playlist ();
			newPlaylist.Elements.Add (new PlaylistPlayElement (new TimelineEvent ()));
			projectVM.Playlists.Model.Add (newPlaylist);

			await App.Current.EventsBroker.Publish (
				new DeletePlaylistEvent {
					Playlist = playlistCollectionVM.Model.FirstOrDefault ()
				}
			);

			storageMock.Verify (s => s.Delete<Playlist> (It.IsAny<Playlist> ()), Times.Never ());
			Assert.AreEqual (0, playlistCollectionVM.ViewModels.Count);
			Assert.AreEqual (0, projectVM.Model.Playlists.Count);
		}

		[Test]
		public async Task TestDeleteSelectedPlaylistWithStorage ()
		{
			SetupWithStorage ();
			var newPlaylist = new Playlist ();
			newPlaylist.Elements.Add (new PlaylistPlayElement (new TimelineEvent ()));
			playlistCollectionVM.Model.Add (newPlaylist);

			playlistCollectionVM.SelectionReplace (playlistCollectionVM.ViewModels);
			await App.Current.EventsBroker.Publish (new DeleteEvent<Playlist> ());

			Assert.AreEqual (0, playlistCollectionVM.ViewModels.Count);
			Assert.AreEqual (0, playlistCollectionVM.Selection.Count);
		}

		[Test]
		public async Task TestDeleteSelectedPlaylistWithProject ()
		{
			SetupWithProject ();
			var newPlaylist = new Playlist ();
			newPlaylist.Elements.Add (new PlaylistPlayElement (new TimelineEvent ()));
			projectVM.Playlists.Model.Add (newPlaylist);

			playlistCollectionVM.SelectionReplace (playlistCollectionVM.ViewModels);
			await App.Current.EventsBroker.Publish (new DeleteEvent<Playlist> ());

			storageMock.Verify (s => s.Delete<Playlist> (It.IsAny<Playlist> ()), Times.Never ());
			Assert.AreEqual (0, playlistCollectionVM.ViewModels.Count);
			Assert.AreEqual (0, projectVM.Model.Playlists.Count);
		}

		[Test]
		public async Task TestDeleteSelectedPlaylistElements ()
		{
			SetupWithStorage ();
			IPlaylistElement element1 = new PlaylistPlayElement (new TimelineEvent { Name = "element1" });
			IPlaylistElement element2 = new PlaylistPlayElement (new TimelineEvent { Name = "element2" });
			IPlaylistElement element3 = new PlaylistPlayElement (new TimelineEvent { Name = "element3" });
			var newPlaylist = new Playlist ();
			newPlaylist.Elements.AddRange (new List<IPlaylistElement> {
				element1, element2, element3 });
			playlistCollectionVM.Model.Add (newPlaylist);

			playlistCollectionVM.ViewModels [0].SelectionReplace (new List<PlaylistElementVM> {
				playlistCollectionVM.ViewModels [0].ViewModels[0],
				playlistCollectionVM.ViewModels [0].ViewModels[2]
			});
			await App.Current.EventsBroker.Publish (new DeleteEvent<Playlist> ());

			Assert.AreEqual (1, playlistCollectionVM.ViewModels [0].ViewModels.Count);
			Assert.AreEqual (0, playlistCollectionVM.ViewModels [0].Selection.Count);

		}

		[Test]
		public async Task TestSavePlaylistWithStorage ()
		{
			// Arrange
			SetupWithStorage ();
			Playlist playlist = new Playlist { Name = "playlist without a project" };

			// Act
			await App.Current.EventsBroker.Publish (
				new AddPlaylistElementEvent {
					PlaylistElements = new List<IPlaylistElement> (),
					Playlist = playlist
				}
			);

			// Assert
			storageMock.Verify (s => s.Store<Playlist> (playlist, true), Times.Once ());
		}

		[Test]
		public async Task TestSavePlaylistWithProject ()
		{
			// Arrange
			SetupWithProject ();
			Playlist playlist = new Playlist { Name = "playlist without a project" };

			// Act
			await App.Current.EventsBroker.Publish (
				new AddPlaylistElementEvent {
					PlaylistElements = new List<IPlaylistElement> (),
					Playlist = playlist
				}
			);

			// Assert
			storageMock.Verify (s => s.Store<Playlist> (playlist, true), Times.Never ());
		}

		[Test]
		public async Task TestSavePlaylistCreateWithStorage ()
		{
			// Arrange
			SetupWithStorage ();

			// Act
			var ev = new CreateEvent<Playlist> ();
			await App.Current.EventsBroker.Publish (ev);

			// Assert
			storageMock.Verify (s => s.Store<Playlist> (ev.Object, true), Times.Once ());
		}

		[Test]
		public async Task TestSavePlaylistCreateWithProject ()
		{
			// Arrange
			SetupWithProject ();

			// Act
			var ev = new CreateEvent<Playlist> ();
			await App.Current.EventsBroker.Publish (ev);

			// Assert
			storageMock.Verify (s => s.Store<Playlist> (ev.Object, true), Times.Never ());
		}

		[Test]
		public async Task TestSavePlaylistMoveElementWithoutProject ()
		{
			// Arrange
			SetupWithStorage ();
			Playlist playlist = new Playlist { Name = "playlist without a project" };
			var playlist2 = new Playlist { Name = "playlist2 without a project" };

			await App.Current.EventsBroker.Publish (
				new AddPlaylistElementEvent {
					PlaylistElements = new List<IPlaylistElement> {
						new PlaylistPlayElement (new TimelineEvent { Name = "event1" }),
						new PlaylistPlayElement (new TimelineEvent { Name = "event2" }),
					},
					Playlist = playlist
				}
			);

			mockDialogs.Setup (m => m.QueryMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (),
										 It.IsAny<object> ())).Returns (AsyncHelpers.Return (name + "2"));

			await App.Current.EventsBroker.Publish (
				new AddPlaylistElementEvent {
					PlaylistElements = new List<IPlaylistElement> (),
					Playlist = playlist2
				}
			);

			Dictionary<PlaylistVM, IEnumerable<PlaylistElementVM>> toRemove = new Dictionary<PlaylistVM, IEnumerable<PlaylistElementVM>> ();
			toRemove.Add (
				new PlaylistVM { Model = playlist },
				new List<PlaylistElementVM> { new PlaylistElementVM { Model = playlist.Elements.First () } }
			);
			KeyValuePair<PlaylistVM, IEnumerable<PlaylistElementVM>> toAdd = new KeyValuePair<PlaylistVM, IEnumerable<PlaylistElementVM>> (
				new PlaylistVM { Model = playlist2 },
				new List<PlaylistElementVM> { new PlaylistElementVM { Model = playlist.Elements.First () } }
			);

			storageMock.ResetCalls ();

			// Act
			var ev = new MoveElementsEvent<PlaylistVM, PlaylistElementVM> {
				ElementsToRemove = toRemove,
				ElementsToAdd = toAdd,
				Index = 0,
			};
			await App.Current.EventsBroker.Publish (ev);

			// Assert
			storageMock.Verify (s => s.Store<Playlist> (playlist, true), Times.Once ());
			storageMock.Verify (s => s.Store<Playlist> (playlist2, true), Times.Once ());
		}

		[Test]
		public async Task TestSavePlaylistMoveElementWithProject ()
		{
			// Arrange
			SetupWithProject ();

			Playlist playlist = new Playlist { Name = "playlist without a project" };
			var playlist2 = new Playlist { Name = "playlist2 without a project" };

			await App.Current.EventsBroker.Publish (
				new AddPlaylistElementEvent {
					PlaylistElements = new List<IPlaylistElement> {
						new PlaylistPlayElement (new TimelineEvent { Name = "event1" }),
						new PlaylistPlayElement (new TimelineEvent { Name = "event2" }),
					},
					Playlist = playlist
				}
			);

			mockDialogs.Setup (m => m.QueryMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (),
										 It.IsAny<object> ())).Returns (AsyncHelpers.Return (name + "2"));

			await App.Current.EventsBroker.Publish (
				new AddPlaylistElementEvent {
					PlaylistElements = new List<IPlaylistElement> (),
					Playlist = playlist2
				}
			);

			Dictionary<PlaylistVM, IEnumerable<PlaylistElementVM>> toRemove = new Dictionary<PlaylistVM, IEnumerable<PlaylistElementVM>> ();
			toRemove.Add (
				new PlaylistVM { Model = playlist },
				new List<PlaylistElementVM> { new PlaylistElementVM { Model = playlist.Elements.First () } }
			);
			KeyValuePair<PlaylistVM, IEnumerable<PlaylistElementVM>> toAdd = new KeyValuePair<PlaylistVM, IEnumerable<PlaylistElementVM>> (
				new PlaylistVM { Model = playlist2 },
				new List<PlaylistElementVM> { new PlaylistElementVM { Model = playlist.Elements.First () } }
			);

			storageMock.ResetCalls ();

			// Act
			var ev = new MoveElementsEvent<PlaylistVM, PlaylistElementVM> {
				ElementsToRemove = toRemove,
				ElementsToAdd = toAdd,
				Index = 0,
			};
			await App.Current.EventsBroker.Publish (ev);

			// Assert
			storageMock.Verify (s => s.Store<Playlist> (playlist, true), Times.Never ());
			storageMock.Verify (s => s.Store<Playlist> (playlist2, true), Times.Never ());
		}

		class PlaylistControllerWithProject : PlaylistController
		{
			public override void SetViewModel (VAS.Core.Interfaces.MVVMC.IViewModel viewModel)
			{
				base.SetViewModel (viewModel);
				ProjectViewModel = (ProjectVM)(viewModel as dynamic);
			}

			// For some reason, without this override, these 3 tests fail:
			// TestAddEventsToExistingPlaylistWithStorage
			// TestAddEventsToNewPlaylistWithProject
			// TestAddEventsToNewPlaylistWithStorage
			protected override Task HandleAddPlaylistElement (AddPlaylistElementEvent e)
			{
				return base.HandleAddPlaylistElement (e);
			}
		}
	}
}
