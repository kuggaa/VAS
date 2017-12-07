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
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;
using VAS.Services.Controller;
using VAS.Services.State;
using VAS.Services.ViewModel;

namespace VAS.Tests.Services
{
	[TestFixture]
	public class TestPlaylistController
	{
		const string name = "name";
		//Menu Names Constants
		const string PLAYLIST_EDIT = "Edit Name";
		const string PLAYLIST_RENDER = "Render";
		const string PLAYLIST_DELETE = "Delete";
		const string ELEMENT_EDIT = "Edit Properties";
		const string ELEMENT_INSERT_BEFORE = "Insert before";
		const string ELEMENT_INSERT_AFTER = "Insert after";
		const string ELEMENT_IMAGE = "External Image";
		const string ELEMENT_VIDEO = "External Video";
		const string ELEMENT_DELETE = "Delete";

		Mock<IGUIToolkit> mockGuiToolkit;
		VideoPlayerVM videoPlayerVM;
		Mock<IDialogs> mockDialogs;
		Mock<IStorageManager> storageManagerMock;
		Mock<IStorage> storageMock;
		PlaylistController controller;
		PlaylistCollectionVM playlistCollectionVM;
		ProjectVM projectVM;
		Mock<IStateController> mockStateController;

		[OneTimeSetUp]
		public void FixtureSetup ()
		{
			mockGuiToolkit = new Mock<IGUIToolkit> ();

			storageManagerMock = new Mock<IStorageManager> ();
			storageManagerMock.SetupAllProperties ();
			storageMock = new Mock<IStorage> ();
			storageManagerMock.Object.ActiveDB = storageMock.Object;
			App.Current.DatabaseManager = storageManagerMock.Object;
			mockStateController = new Mock<IStateController> ();
			App.Current.StateController = mockStateController.Object;
		}

		[SetUp]
		public void Setup ()
		{
			mockDialogs = new Mock<IDialogs> ();
			mockGuiToolkit.SetupGet (o => o.DeviceScaleFactor).Returns (1.0f);
			App.Current.GUIToolkit = mockGuiToolkit.Object;
			App.Current.Dialogs = mockDialogs.Object;
			var videoController = new Mock<IVideoPlayerController> ().Object;
			videoPlayerVM = new VideoPlayerVM ();
			videoController.SetViewModel (videoPlayerVM);
			mockDialogs.Setup (m => m.QueryMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (),
													 It.IsAny<object> ())).Returns (AsyncHelpers.Return (name));
			mockDialogs.Setup (m => m.QuestionMessage (It.IsAny<string> (), null, null)).Returns (AsyncHelpers.Return (true));
			mockDialogs.Setup (m => m.OpenMediaFile (null)).Returns (new MediaFile { Duration = new Time (2500) });
			mockDialogs.Setup (m => m.OpenImage (null)).ReturnsAsync (App.Current.ResourcesLocator.LoadImage ("asdf"));
			mockGuiToolkit.Setup (m => m.ConfigureRenderingJob (It.IsAny<Playlist> ())).Returns (
				new List<EditionJob> { new EditionJob (new Playlist (), new EncodingSettings ()) });
			mockStateController.Setup (s => s.MoveToModal (EditPlaylistElementState.NAME, It.IsAny<object> (), true)).ReturnsAsync (true);
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

		void AddSomePlaylistElements ()
		{
			var newPlaylist = new Playlist ();
			newPlaylist.Elements.Add (new PlaylistPlayElement (new TimelineEvent { Start = new Time (0), Stop = new Time (2000) }));
			newPlaylist.Elements.Add (new PlaylistImage (new Image (20, 20), new Time (1000)));
			newPlaylist.Elements.Add (new PlaylistVideo (new MediaFile { Duration = new Time (5000) }));
			playlistCollectionVM.Model.Add (newPlaylist);
		}

		[Test]
		public async Task TestAddEventsToNewPlaylistWithStorage ()
		{
			SetupWithStorage ();

			await App.Current.EventsBroker.Publish (
				new AddPlaylistElementEvent {
					PlaylistElements = new List<IPlayable> {
						new PlaylistPlayElementVM { Model = new PlaylistPlayElement (new TimelineEvent ()) } },
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
					PlaylistElements = new List<IPlayable> {
						new PlaylistPlayElementVM { Model = new PlaylistPlayElement (new TimelineEvent ()) } },
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
					PlaylistElements = new List<IPlayable> {
						new PlaylistPlayElementVM { Model = new PlaylistPlayElement (new TimelineEvent ()) } },
					Playlist = playlistCollectionVM.ViewModels [0]
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
					PlaylistElements = new List<IPlayable> {
						new PlaylistPlayElementVM { Model = new PlaylistPlayElement (new TimelineEvent ()) } },
					Playlist = new PlaylistVM { Model = newPlaylist },
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
					PlaylistElements = new List<IPlayable> (),
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
					PlaylistElements = new List<IPlayable> (),
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
					Playlist = new PlaylistVM { Model = newPlaylist },
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
					Playlist = playlistCollectionVM.FirstOrDefault ()
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

			playlistCollectionVM.Selection.Replace (playlistCollectionVM.ViewModels);
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

			playlistCollectionVM.Selection.Replace (playlistCollectionVM.ViewModels);
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

			playlistCollectionVM.ViewModels [0].Selection.Replace (new List<PlaylistElementVM> {
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
					PlaylistElements = new List<IPlayable> (),
					Playlist = new PlaylistVM { Model = playlist },
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
					PlaylistElements = new List<IPlayable> (),
					Playlist = new PlaylistVM { Model = playlist },
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
			var ev = new CreateEvent<PlaylistVM> ();
			await App.Current.EventsBroker.Publish (ev);

			// Assert
			storageMock.Verify (s => s.Store<Playlist> (ev.Object.Model, true), Times.Once ());
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
					PlaylistElements = new List<IPlayable> {
						new PlaylistPlayElementVM { Model = new PlaylistPlayElement (new TimelineEvent { Name = "event1" })},
						new PlaylistPlayElementVM { Model = new PlaylistPlayElement (new TimelineEvent { Name = "event2" })}
					},
					Playlist = new PlaylistVM { Model = playlist },
				}
			);

			mockDialogs.Setup (m => m.QueryMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (),
										 It.IsAny<object> ())).Returns (AsyncHelpers.Return (name + "2"));

			await App.Current.EventsBroker.Publish (
				new AddPlaylistElementEvent {
					PlaylistElements = new List<IPlayable> (),
					Playlist = new PlaylistVM { Model = playlist2 },
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
					PlaylistElements = new List<IPlayable> {
						new PlaylistPlayElementVM { Model = new PlaylistPlayElement (new TimelineEvent { Name = "event1" })},
						new PlaylistPlayElementVM { Model = new PlaylistPlayElement (new TimelineEvent { Name = "event2" })}
					},
					Playlist = new PlaylistVM { Model = playlist }
				}
			);

			mockDialogs.Setup (m => m.QueryMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (),
										 It.IsAny<object> ())).Returns (AsyncHelpers.Return (name + "2"));

			await App.Current.EventsBroker.Publish (
				new AddPlaylistElementEvent {
					PlaylistElements = new List<IPlayable> (),
					Playlist = new PlaylistVM { Model = playlist2 },
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

		[Test]
		public void DeleteCommand_PlaylistSelection_CanExecute ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();

			Assert.IsFalse (playlistCollectionVM.DeleteCommand.CanExecute ());

			playlistCollectionVM.Select (playlistCollectionVM.ViewModels [0]);

			Assert.IsTrue (playlistCollectionVM.DeleteCommand.CanExecute ());
		}

		[Test]
		public void DeleteCommand_PlaylistElementSelection_CanExecute ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();

			Assert.IsFalse (playlistCollectionVM.DeleteCommand.CanExecute ());

			playlistCollectionVM.ViewModels [0].Select (playlistCollectionVM.ViewModels [0].ViewModels [0]);

			Assert.IsTrue (playlistCollectionVM.DeleteCommand.CanExecute ());
		}

		[Test]
		public void EditCommand_OnePlaylistSelection_CanExecute ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();
			Command editCommand = Utils.GetCommandFromMenu (playlistCollectionVM.PlaylistMenu, PLAYLIST_EDIT);


			Assert.IsFalse (editCommand.CanExecute ());

			playlistCollectionVM.Select (playlistCollectionVM.ViewModels [0]);

			Assert.IsTrue (editCommand.CanExecute ());
		}

		[Test]
		public void EditCommand_TwoPlaylistSelection_CanNotExecute ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();
			playlistCollectionVM.Model.Add (new Playlist ());
			Command editCommand = Utils.GetCommandFromMenu (playlistCollectionVM.PlaylistMenu, PLAYLIST_EDIT);

			Assert.IsFalse (editCommand.CanExecute ());

			playlistCollectionVM.Selection.Replace (playlistCollectionVM.ViewModels);

			Assert.IsFalse (editCommand.CanExecute ());
		}

		[Test]
		public void RenderCommand_OnePlaylistSelection_CanExecute ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();
			Command renderCommand = Utils.GetCommandFromMenu (playlistCollectionVM.PlaylistMenu, PLAYLIST_RENDER);

			Assert.IsFalse (renderCommand.CanExecute ());

			playlistCollectionVM.Select (playlistCollectionVM.ViewModels [0]);

			Assert.IsTrue (renderCommand.CanExecute ());
		}

		[Test]
		public void RenderCommand_TwoPlaylistSelection_CanNotExecute ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();
			playlistCollectionVM.Model.Add (new Playlist ());
			Command renderCommand = Utils.GetCommandFromMenu (playlistCollectionVM.PlaylistMenu, PLAYLIST_RENDER);

			Assert.IsFalse (renderCommand.CanExecute ());

			playlistCollectionVM.Selection.Replace (playlistCollectionVM.ViewModels);

			Assert.IsFalse (renderCommand.CanExecute ());
		}

		[Test]
		public void InsertVideoCommand_PlaylistElementSelection_CanExecute ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();
			Command insertVideoCommand = Utils.GetCommandFromMenu (playlistCollectionVM.PlaylistElementMenu, ELEMENT_VIDEO);

			Assert.IsFalse (insertVideoCommand.CanExecute ());

			playlistCollectionVM.ViewModels [0].Select (playlistCollectionVM.ViewModels [0].ViewModels [0]);

			Assert.IsTrue (insertVideoCommand.CanExecute ());
		}

		[Test]
		public void InsertVideoCommand_ManyPlaylistElementSelection_CanExecute ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();
			Command insertVideoCommand = Utils.GetCommandFromMenu (playlistCollectionVM.PlaylistElementMenu, ELEMENT_VIDEO);

			Assert.IsFalse (insertVideoCommand.CanExecute ());

			playlistCollectionVM.ViewModels [0].Selection.Replace (playlistCollectionVM.ViewModels [0].ViewModels);

			Assert.IsTrue (insertVideoCommand.CanExecute ());
		}

		[Test]
		public void InsertImageCommand_PlaylistElementSelection_CanExecute ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();
			Command insertImageCommand = Utils.GetCommandFromMenu (playlistCollectionVM.PlaylistElementMenu, ELEMENT_IMAGE);

			Assert.IsFalse (insertImageCommand.CanExecute ());

			playlistCollectionVM.ViewModels [0].Select (playlistCollectionVM.ViewModels [0].ViewModels [0]);

			Assert.IsTrue (insertImageCommand.CanExecute ());
		}

		[Test]
		public void InsertAudioCommand_ManyPlaylistElementSelection_CanExecute ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();
			Command insertImageCommand = Utils.GetCommandFromMenu (playlistCollectionVM.PlaylistElementMenu, ELEMENT_IMAGE);

			Assert.IsFalse (insertImageCommand.CanExecute ());

			playlistCollectionVM.ViewModels [0].Selection.Replace (playlistCollectionVM.ViewModels [0].ViewModels);

			Assert.IsTrue (insertImageCommand.CanExecute ());
		}

		[Test]
		public void EditPlaylistElementCommand_PlaylistPlaySelected_CanExecute ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();
			Command editElementCommand = Utils.GetCommandFromMenu (playlistCollectionVM.PlaylistElementMenu, ELEMENT_EDIT);

			Assert.IsFalse (editElementCommand.CanExecute ());

			playlistCollectionVM.ViewModels [0].Select (playlistCollectionVM.ViewModels [0].ViewModels [0]);

			Assert.IsTrue (editElementCommand.CanExecute ());
		}

		[Test]
		public void EditPlaylistElementCommand_PlaylistVideoSelected_CanNotExecute ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();
			Command editElementCommand = Utils.GetCommandFromMenu (playlistCollectionVM.PlaylistElementMenu, ELEMENT_EDIT);

			Assert.IsFalse (editElementCommand.CanExecute ());

			playlistCollectionVM.ViewModels [0].Select (playlistCollectionVM.ViewModels [0].ViewModels [2]);

			Assert.IsFalse (editElementCommand.CanExecute ());
		}

		[Test]
		public void EditPlaylistElementCommand_PlaylistImageSelected_CanExecute ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();
			Command editElementCommand = Utils.GetCommandFromMenu (playlistCollectionVM.PlaylistElementMenu, ELEMENT_EDIT);

			Assert.IsFalse (editElementCommand.CanExecute ());

			playlistCollectionVM.ViewModels [0].Select (playlistCollectionVM.ViewModels [0].ViewModels [1]);

			Assert.IsTrue (editElementCommand.CanExecute ());
		}

		[Test]
		public void EditPlaylistElementCommand_TwoPlaylistPlaySelected_CanNotExecute ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();
			playlistCollectionVM.ViewModels [0].Model.Elements.Add (
				new PlaylistPlayElement (new TimelineEvent { Start = new Time (0), Stop = new Time (2000) }));
			Command editElementCommand = Utils.GetCommandFromMenu (playlistCollectionVM.PlaylistElementMenu, ELEMENT_EDIT);

			Assert.IsFalse (editElementCommand.CanExecute ());

			playlistCollectionVM.ViewModels [0].Selection.Add (playlistCollectionVM.ViewModels [0].ViewModels [0]);
			playlistCollectionVM.ViewModels [0].Selection.Add (playlistCollectionVM.ViewModels [0].ViewModels [3]);

			Assert.IsFalse (editElementCommand.CanExecute ());
		}

		[Test]
		public void EditPlaylistElementCommand_TwoPlaylistImageSelected_CanNotExecute ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();
			playlistCollectionVM.ViewModels [0].Model.Elements.Add (
				new PlaylistImage (new Image (20, 20), new Time (9000)));
			Command editElementCommand = Utils.GetCommandFromMenu (playlistCollectionVM.PlaylistElementMenu, ELEMENT_EDIT);

			Assert.IsFalse (editElementCommand.CanExecute ());

			playlistCollectionVM.ViewModels [0].Selection.Add (playlistCollectionVM.ViewModels [0].ViewModels [1]);
			playlistCollectionVM.ViewModels [0].Selection.Add (playlistCollectionVM.ViewModels [0].ViewModels [3]);

			Assert.IsFalse (editElementCommand.CanExecute ());
		}

		[Test]
		public void DeleteSelectedPlaylists ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();

			Assert.AreEqual (1, playlistCollectionVM.ViewModels.Count);

			playlistCollectionVM.Select (playlistCollectionVM.ViewModels [0]);
			playlistCollectionVM.DeleteCommand.Execute ();

			Assert.AreEqual (0, playlistCollectionVM.ViewModels.Count);
		}

		[Test]
		public void DeleteSelectedPlaylistElements ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();

			Assert.AreEqual (3, playlistCollectionVM.ViewModels [0].ViewModels.Count);

			playlistCollectionVM.ViewModels [0].Select (playlistCollectionVM.ViewModels [0].ViewModels [0]);
			playlistCollectionVM.DeleteCommand.Execute ();

			Assert.AreEqual (2, playlistCollectionVM.ViewModels [0].ViewModels.Count);
		}

		[Test]
		public void EditSelectedPlaylist ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();
			var currentName = playlistCollectionVM.ViewModels [0].Name = "test";
			Command editCommand = Utils.GetCommandFromMenu (playlistCollectionVM.PlaylistMenu, PLAYLIST_EDIT);

			playlistCollectionVM.Select (playlistCollectionVM.ViewModels [0]);
			editCommand.Execute ();

			mockDialogs.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (),
				It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ()), Times.Once ());
			Assert.AreNotEqual (currentName, playlistCollectionVM.ViewModels [0].Name);
			Assert.AreEqual (name, playlistCollectionVM.ViewModels [0].Name);
		}

		[Test]
		public void RenderSelectedPlaylist ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();
			Command renderCommand = Utils.GetCommandFromMenu (playlistCollectionVM.PlaylistMenu, PLAYLIST_RENDER);

			playlistCollectionVM.Select (playlistCollectionVM.ViewModels [0]);
			renderCommand.Execute ();

			mockGuiToolkit.Verify (toolkit => toolkit.ConfigureRenderingJob (It.IsAny<Playlist> ()), Times.Once ());
		}

		[Test]
		public void InsertVideoAfter_OneSelection ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();
			int indexSelected = 0;
			Command insertVideoCommand = Utils.GetCommandFromMenu (playlistCollectionVM.PlaylistElementMenu, ELEMENT_VIDEO);

			Assert.AreEqual (3, playlistCollectionVM.ViewModels [0].ViewModels.Count);

			playlistCollectionVM.ViewModels [0].Select (playlistCollectionVM.ViewModels [0].ViewModels [indexSelected]);
			insertVideoCommand.Execute (PlaylistPosition.After);

			mockDialogs.Verify (guitoolkit => guitoolkit.OpenMediaFile (null), Times.Once ());
			Assert.AreEqual (4, playlistCollectionVM.ViewModels [0].ViewModels.Count);
			Assert.IsInstanceOf (typeof (PlaylistVideoVM), playlistCollectionVM.ViewModels [0].ViewModels [indexSelected + 1]);
		}

		[Test]
		public void InsertVideoBefore_OneSelection ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();
			int indexSelected = 0;
			Command insertVideoCommand = Utils.GetCommandFromMenu (playlistCollectionVM.PlaylistElementMenu, ELEMENT_VIDEO);

			Assert.AreEqual (3, playlistCollectionVM.ViewModels [0].ViewModels.Count);

			playlistCollectionVM.ViewModels [0].Select (playlistCollectionVM.ViewModels [0].ViewModels [indexSelected]);
			insertVideoCommand.Execute (PlaylistPosition.Before);

			mockDialogs.Verify (guitoolkit => guitoolkit.OpenMediaFile (null), Times.Once ());
			Assert.AreEqual (4, playlistCollectionVM.ViewModels [0].ViewModels.Count);
			Assert.IsInstanceOf (typeof (PlaylistVideoVM), playlistCollectionVM.ViewModels [0].ViewModels [indexSelected]);
		}

		[Test]
		public void InsertVideoAfter_TwoSelection ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();
			int indexSelected1 = 0;
			int indexSelected2 = 2;
			Command insertVideoCommand = Utils.GetCommandFromMenu (playlistCollectionVM.PlaylistElementMenu, ELEMENT_VIDEO);

			Assert.AreEqual (3, playlistCollectionVM.ViewModels [0].ViewModels.Count);

			playlistCollectionVM.ViewModels [0].Selection.Add (playlistCollectionVM.ViewModels [0].ViewModels [indexSelected1]);
			playlistCollectionVM.ViewModels [0].Selection.Add (playlistCollectionVM.ViewModels [0].ViewModels [indexSelected2]);
			insertVideoCommand.Execute (PlaylistPosition.After);

			mockDialogs.Verify (guitoolkit => guitoolkit.OpenMediaFile (null), Times.Once ());
			Assert.AreEqual (5, playlistCollectionVM.ViewModels [0].ViewModels.Count);
			Assert.IsInstanceOf (typeof (PlaylistVideoVM), playlistCollectionVM.ViewModels [0].ViewModels [indexSelected1 + 1]);
			Assert.IsInstanceOf (typeof (PlaylistVideoVM), playlistCollectionVM.ViewModels [0].ViewModels [indexSelected2 + 1]);
		}

		[Test]
		public async Task InsertImageAfter_OneSelection ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();
			int indexSelected = 0;
			Command insertImageCommand = Utils.GetCommandFromMenu (playlistCollectionVM.PlaylistElementMenu, ELEMENT_IMAGE);

			Assert.AreEqual (3, playlistCollectionVM.ViewModels [0].ViewModels.Count);

			playlistCollectionVM.ViewModels [0].Selection.Add (playlistCollectionVM.ViewModels [0].ViewModels [indexSelected]);
			await insertImageCommand.ExecuteAsync (PlaylistPosition.After);

			mockDialogs.Verify (guitoolkit => guitoolkit.OpenImage (null), Times.Once ());
			Assert.AreEqual (4, playlistCollectionVM.ViewModels [0].ViewModels.Count);
			Assert.IsInstanceOf (typeof (PlaylistImageVM), playlistCollectionVM.ViewModels [0].ViewModels [indexSelected + 1]);
		}

		[Test]
		public async Task InsertImageBefore_TwoSelection ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();
			int indexSelected1 = 0;
			int indexSelected2 = 2;
			Command insertImageCommand = Utils.GetCommandFromMenu (playlistCollectionVM.PlaylistElementMenu, ELEMENT_IMAGE);

			Assert.AreEqual (3, playlistCollectionVM.ViewModels [0].ViewModels.Count);

			playlistCollectionVM.ViewModels [0].Selection.Add (playlistCollectionVM.ViewModels [0].ViewModels [indexSelected1]);
			playlistCollectionVM.ViewModels [0].Selection.Add (playlistCollectionVM.ViewModels [0].ViewModels [indexSelected2]);
			await insertImageCommand.ExecuteAsync (PlaylistPosition.Before);

			mockDialogs.Verify (guitoolkit => guitoolkit.OpenImage (null), Times.Once ());
			Assert.AreEqual (5, playlistCollectionVM.ViewModels [0].ViewModels.Count);
			Assert.IsInstanceOf (typeof (PlaylistImageVM), playlistCollectionVM.ViewModels [0].ViewModels [indexSelected1]);
			Assert.IsInstanceOf (typeof (PlaylistImageVM), playlistCollectionVM.ViewModels [0].ViewModels [indexSelected2]);
		}

		[Test]
		public void EditPlaylistImage ()
		{
			SetupWithStorage ();
			AddSomePlaylistElements ();
			Command command = Utils.GetCommandFromMenu (playlistCollectionVM.PlaylistElementMenu, ELEMENT_EDIT);

			playlistCollectionVM.ViewModels [0].Selection.Add (playlistCollectionVM.ViewModels [0].ViewModels [1]);
			command.Execute ();

			mockStateController.Verify (s => s.MoveToModal (EditPlaylistElementState.NAME, It.IsAny<object> (), true), Times.Once ());
		}

		class PlaylistControllerWithProject : PlaylistController
		{
			public override void SetViewModel (IViewModel viewModel)
			{
				base.SetViewModel (viewModel);
				ProjectViewModel = ((IProjectDealer)viewModel).Project;
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
