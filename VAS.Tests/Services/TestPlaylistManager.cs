//
//  Copyright (C) 2015 Fluendo S.A.
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
using Moq;
using NUnit.Framework;
using VAS;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Services;
using VAS.Tests;
using TestUtils = VAS.Tests.Utils;

namespace VAS.Tests
{
	[TestFixture ()]
	public class TestPlaylistManager
	{
		PlaylistManager plmanager;
		Mock<IGUIToolkit> mockGuiToolkit;
		Mock<IRenderingJobsManager> mockVideoRenderer;
		Mock<IAnalysisWindowBase> mockAnalysisWindow;
		Mock<IPlayerController> mockPlayerController;
		MediaFileSet mfs;

		bool playlistElementLoaded;


		[TestFixtureSetUp ()]
		public void FixtureSetup ()
		{
			mfs = new MediaFileSet ();
			mfs.Add (new MediaFile { FilePath = "test1", VideoWidth = 320, VideoHeight = 240, Par = 1 });
			mfs.Add (new MediaFile { FilePath = "test2", VideoWidth = 320, VideoHeight = 240, Par = 1 });

			App.Current.EventsBroker = new EventsBroker ();
			mockAnalysisWindow = new Mock<IAnalysisWindowBase> ();
			mockPlayerController = new Mock<IPlayerController> ();
			mockPlayerController.SetupAllProperties ();
			mockAnalysisWindow.SetupGet (m => m.Player).Returns (mockPlayerController.Object);
			mockVideoRenderer = new Mock<IRenderingJobsManager> ();

			App.Current.EventsBroker.PlaylistElementLoadedEvent += (playlist, element) => playlistElementLoaded = true;
		}

		[SetUp ()]
		public void Setup ()
		{
			mockGuiToolkit = new Mock<IGUIToolkit> ();
			App.Current.GUIToolkit = mockGuiToolkit.Object;
			App.Current.RenderingJobsManger = mockVideoRenderer.Object; 

			plmanager = new PlaylistManager ();
			plmanager.Start ();
			plmanager.Player = mockPlayerController.Object;

			OpenProject (new TestUtils.ProjectDummy ());
			playlistElementLoaded = false;

		}

		[TearDown ()]
		public void TearDown ()
		{
			plmanager.Stop ();
			mockGuiToolkit.ResetCalls ();
			mockPlayerController.ResetCalls ();
		}

		void OpenProject (Project project = null, ProjectType projectType = ProjectType.FileProject)
		{
			App.Current.EventsBroker.EmitOpenedProjectChanged (project, projectType,
				new TestUtils.EventsFilterDummy (project), mockAnalysisWindow.Object);
		}

		[Test ()]
		public void TestNewPlaylist ()
		{
			string name = "name";
			mockGuiToolkit.Setup (m => m.QueryMessage (It.IsAny<string> (), It.IsAny<string> (),
				It.IsAny<string> (), It.IsAny<object> ())).Returns (Task.Factory.StartNew (() => name));

			TestUtils.ProjectDummy project = new TestUtils.ProjectDummy ();
			App.Current.EventsBroker.EmitNewPlaylist (project);

			mockGuiToolkit.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (),
				It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ()), Times.Once ());

			Assert.AreEqual (1, project.Playlists.Count);
			Assert.AreEqual (name, project.Playlists [0].Name);

		}

		[Test ()]
		public void TestNewPlaylistNull ()
		{
			// We DON'T Setup the QueryMessage, it will return null, and continue without creating the playlist
			TestUtils.ProjectDummy project = new TestUtils.ProjectDummy ();
			App.Current.EventsBroker.EmitNewPlaylist (project);

			mockGuiToolkit.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (),
				It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ()), Times.Once ());

			Assert.AreEqual (0, project.Playlists.Count);
		}

		[Test ()]
		public void TestNewPlaylistRepeatName ()
		{
			bool called = false;
			string name = "name";
			string differentName = "different name";
			mockGuiToolkit.Setup (m => m.QueryMessage (It.IsAny<string> (), It.IsAny<string> (),
				It.IsAny<string> (), It.IsAny<object> ()))
				.Returns (() => Task.Factory.StartNew (() => {
				if (called) {
					return differentName;
				} else {
					called = true;
					return name;
				}
			}));

			TestUtils.ProjectDummy project = new TestUtils.ProjectDummy ();
			App.Current.EventsBroker.EmitNewPlaylist (project);
			called = false;
			App.Current.EventsBroker.EmitNewPlaylist (project);

			mockGuiToolkit.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (),
				It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ()), Times.Exactly (3));

			Assert.AreEqual (2, project.Playlists.Count);
			Assert.AreEqual (name, project.Playlists [0].Name);
			Assert.AreEqual (differentName, project.Playlists [1].Name);

		}

		[Test ()]
		public void TestAddPlaylistElement ()
		{
			var playlist = new Playlist { Name = "name" };
			IPlaylistElement element = new PlaylistPlayElement (new TimelineEvent ());
			var elementList = new List<IPlaylistElement> ();
			elementList.Add (element);

			App.Current.EventsBroker.EmitAddPlaylistElement (playlist, elementList);

			Assert.AreEqual (elementList, playlist.Elements.ToList ());
		}

		[Test ()]
		public void TestAddPlaylistElementNewPlaylist ()
		{
			mockGuiToolkit.Setup (m => m.QueryMessage (It.IsAny<string> (), It.IsAny<string> (),
				It.IsAny<string> (), It.IsAny<object> ())).Returns (Task.Factory.StartNew (() => "name"));

			var elementList = new List<IPlaylistElement> ();
			App.Current.EventsBroker.EmitAddPlaylistElement (null, elementList);
			mockGuiToolkit.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (),
				It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ()), Times.Once ());
		}

		[Test ()]
		public void TestAddPlaylistElementNullPlaylist ()
		{
			// We DON'T Setup the QueryMessage, it will return null, and continue without creating the playlist
			var elementList = new List<IPlaylistElement> ();

			App.Current.EventsBroker.EmitAddPlaylistElement (null, elementList);
			mockGuiToolkit.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (),
				It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ()), Times.Once ());
		}

		[Test ()]
		public void TestLoadPlayEvent ()
		{
			TimelineEvent element = new TimelineEvent { Start = new Time (0), Stop = new Time (5) };
			App.Current.EventsBroker.EmitLoadEvent (element);
			mockPlayerController.Verify (player => player.LoadEvent (element, new Time (0), true),
				Times.Once ());
		}

		[Test ()]
		public void TestLoadPlayEventNull ()
		{
			TimelineEvent element = null;
			App.Current.EventsBroker.EmitLoadEvent (element);
			mockPlayerController.Verify (player => player.UnloadCurrentEvent (), Times.Once ());
		}

		[Test ()]
		public void TestLoadPlayEventWithoutDuration ()
		{
			TimelineEvent element = new TimelineEvent { Start = new Time (0), Stop = new Time (0) };
			App.Current.EventsBroker.EmitLoadEvent (element);
			mockPlayerController.Verify (
				player => player.Seek (element.EventTime, true, false, false), Times.Once ());
			mockPlayerController.Verify (player => player.Play (false), Times.Once ());
		}

		[Test ()]
		public void TestLoadPlayEventFake ()
		{
			var project = new TestUtils.ProjectDummy ();
			OpenProject (project, ProjectType.FakeCaptureProject);
			TimelineEvent element = new TimelineEvent ();
			App.Current.EventsBroker.EmitLoadEvent (element);
			mockPlayerController.Verify (player => player.Seek (It.IsAny<Time> (), It.IsAny<bool> (),
				It.IsAny<bool> (), It.IsAny<bool> ()), Times.Never ());
			mockPlayerController.Verify (player => player.Play (false), Times.Never ());
		}

		[Test ()]
		public void TestPrev ()
		{
			TimelineEvent element = new TimelineEvent ();
			App.Current.EventsBroker.EmitLoadEvent (element);
			// loadedPlay != null
			mockPlayerController.ResetCalls ();

			App.Current.EventsBroker.EmitPreviousPlaylistElement (null);

			mockPlayerController.Verify (player => player.Previous (false), Times.Once ());
		}

		[Test ()]
		public void TestNext ()
		{
			TimelineEvent element = new TimelineEvent ();
			App.Current.EventsBroker.EmitLoadEvent (element);
			// loadedPlay != null
			mockPlayerController.ResetCalls ();

			App.Current.EventsBroker.EmitNextPlaylistElement (null);

			mockPlayerController.Verify (player => player.Next (), Times.Once ());
		}

		[Test ()]
		public void TestOpenPresentation ()
		{
			Playlist presentation = new Playlist ();
			IPlaylistElement element = new PlaylistPlayElement (new TimelineEvent ());
			IPlaylistElement element2 = new PlaylistPlayElement (new TimelineEvent ());
			presentation.Elements.Add (element);
			presentation.Elements.Add (element2);

			IPlayerController playercontroller = mockPlayerController.Object;
			mockPlayerController.ResetCalls ();

			App.Current.EventsBroker.EmitOpenedPresentationChanged (presentation, playercontroller);

			Assert.AreSame (playercontroller, plmanager.Player);

			App.Current.EventsBroker.EmitLoadPlaylistElement (presentation, element, true);
			mockPlayerController.Verify (
				player => player.LoadPlaylistEvent (presentation, element, true), Times.Once ());
		}

		[Test ()]
		public void TestOpenNullPresentation ()
		{
			Playlist presentation = new Playlist ();
			IPlaylistElement element = new PlaylistPlayElement (new TimelineEvent ());
			IPlaylistElement element2 = new PlaylistPlayElement (new TimelineEvent ());
			presentation.Elements.Add (element);
			presentation.Elements.Add (element2);

			IPlayerController playercontroller = mockPlayerController.Object;
			mockPlayerController.ResetCalls ();

			App.Current.EventsBroker.EmitOpenedPresentationChanged (null, playercontroller);

			Assert.AreSame (playercontroller, plmanager.Player);

			App.Current.EventsBroker.EmitLoadPlaylistElement (presentation, element, true);
			mockPlayerController.Verify (player => player.LoadPlaylistEvent (presentation, element, true), Times.Once ());
		}

		[Test ()]
		public void TestOpenPresentationNullPlayer ()
		{
			plmanager.Player = null;

			Playlist presentation = new Playlist ();
			IPlaylistElement element = new PlaylistPlayElement (new TimelineEvent ());
			IPlaylistElement element2 = new PlaylistPlayElement (new TimelineEvent ());
			presentation.Elements.Add (element);
			presentation.Elements.Add (element2);

			IPlayerController playercontroller = mockPlayerController.Object;
			mockPlayerController.ResetCalls ();

			App.Current.EventsBroker.EmitOpenedPresentationChanged (presentation, null);

			App.Current.EventsBroker.EmitLoadPlaylistElement (presentation, element, true);
		}

		[Test ()]
		public void TestOpenNullPresentationNullPlayer ()
		{
			plmanager.Player = null;

			Playlist presentation = new Playlist ();
			IPlaylistElement element = new PlaylistPlayElement (new TimelineEvent ());
			IPlaylistElement element2 = new PlaylistPlayElement (new TimelineEvent ());
			presentation.Elements.Add (element);
			presentation.Elements.Add (element2);

			App.Current.EventsBroker.EmitOpenedPresentationChanged (null, null);
			App.Current.EventsBroker.EmitLoadPlaylistElement (presentation, element, true);
		}

		[Test ()]
		public void TestTimeNodeChanged ()
		{
			TimelineEvent timelineEvent = new TimelineEvent ();
			timelineEvent.Start = new Time (10);
			timelineEvent.Stop = new Time (20);
			App.Current.EventsBroker.EmitTimeNodeChanged (timelineEvent, new Time (5));

			mockPlayerController.Verify (p => p.LoadEvent (timelineEvent, new Time (5), false), Times.Once);
		}
	}
}
