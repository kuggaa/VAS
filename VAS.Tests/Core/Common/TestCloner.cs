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
using System;
using Newtonsoft.Json;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Serialization;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.Store.Templates;

namespace VAS.Tests.Core.Common
{
	[TestFixture]
	public class TestCloner
	{
		[Test ()]
		public void TestClone_IsBindableBase_ChangeCollectionProperty ()
		{
			// Arrange
			var team = new DummyTeam ();
			var clonedTeam = team.Clone ();

			clonedTeam.IsChanged = false;

			// Action
			clonedTeam.List.Add (new Utils.PlayerDummy ());

			// Assert
			Assert.IsTrue (clonedTeam.IsChanged);
		}

		[Test ()]
		public void TestClone_IsBindableBase_CloneProperty ()
		{
			// Arrange
			Utils.DashboardDummy dashboard = new Utils.DashboardDummy ();
			Utils.ProjectDummy p = new Utils.ProjectDummy ();

			// Action
			p.Dashboard = dashboard.Clone ();

			// Assert
			Assert.IsTrue (p.Dashboard.IsChanged);
		}

		[Test ()]
		public void TestClone_CloneIgnore_BindableBase ()
		{
			var bindableBase = new DummyBindable ();
			bindableBase.IgnoreEvents = true;
			bindableBase.IsChanged = true;
			var bindableBase2 = bindableBase.Clone ();
			Assert.IsTrue (bindableBase2.IgnoreEvents);
			Assert.IsTrue (bindableBase2.IsChanged);
			bindableBase.IgnoreEvents = false;
			bindableBase.IsChanged = false;
			bindableBase2 = bindableBase.Clone ();
			Assert.IsFalse (bindableBase2.IgnoreEvents);
			Assert.IsFalse (bindableBase2.IsChanged);
		}

		[Test ()]
		public void TestClone_CloneIgnore_Dashboard ()
		{
			var mediaFileSet = new MediaFileSet ();
			mediaFileSet.DocumentID = "1234567890";
			mediaFileSet.ParentID = Guid.NewGuid ();
			mediaFileSet.IsChanged = true;
			var mediaFileSet2 = mediaFileSet.Clone ();
			Assert.AreEqual (mediaFileSet.DocumentID, mediaFileSet2.DocumentID);
			Assert.AreEqual (mediaFileSet.ParentID, mediaFileSet2.ParentID);
			Assert.IsTrue (mediaFileSet2.IsChanged);
		}


		[Test ()]
		public void TestClone_CloneIgnore_Player ()
		{
			var player = new DummyPlayer ();
			player.Color = Color.Black;
			player.IsChanged = true;
			var player2 = player.Clone ();
			Assert.AreEqual (player.Color, player2.Color);
			Assert.IsTrue (player2.IsChanged);
		}

		[Test ()]
		public void TestClone_CloneIgnore_PlaylistDrawing ()
		{
			var playlistDrawing = new PlaylistDrawing (new FrameDrawing ());
			playlistDrawing.Selected = true;
			var playlistDrawing2 = playlistDrawing.Clone ();
			Assert.AreEqual (playlistDrawing.Selected, playlistDrawing2.Selected);
			playlistDrawing.Selected = false;
			playlistDrawing2 = playlistDrawing.Clone ();
			Assert.AreEqual (playlistDrawing.Selected, playlistDrawing2.Selected);
		}

		[Test ()]
		public void TestClone_CloneIgnore_PlaylistPlayElement ()
		{
			var playlistPlayElement = new PlaylistPlayElement (new TimelineEvent ());
			playlistPlayElement.Selected = true;
			var playlistPlayElement2 = playlistPlayElement.Clone ();
			Assert.AreEqual (playlistPlayElement.Selected, playlistPlayElement2.Selected);
			playlistPlayElement.Selected = false;
			playlistPlayElement2 = playlistPlayElement.Clone ();
			Assert.AreEqual (playlistPlayElement.Selected, playlistPlayElement2.Selected);
		}

		[Test ()]
		public void TestClone_CloneIgnore_PlaylistVideo ()
		{
			var playlistVideo = new PlaylistVideo (new MediaFile ());
			playlistVideo.Selected = true;
			var playlistVideo2 = playlistVideo.Clone ();
			Assert.AreEqual (playlistVideo.Selected, playlistVideo2.Selected);
			playlistVideo.Selected = false;
			playlistVideo2 = playlistVideo.Clone ();
			Assert.AreEqual (playlistVideo.Selected, playlistVideo2.Selected);
		}

		[Test ()]
		public void TestClone_CloneIgnore_Project ()
		{
			var project = new DummyProject ();
			project.ProjectType = ProjectType.CaptureProject;
			var project2 = project.Clone ();
			Assert.AreEqual (project.ProjectType, project2.ProjectType);
			project.ProjectType = ProjectType.FakeCaptureProject;
			project2 = project.Clone ();
			Assert.AreEqual (project.ProjectType, project2.ProjectType);
		}

		[Test ()]
		public void TestClone_CloneIgnore_StorableBase ()
		{
			var storableBase = new StorableBase ();
			storableBase.DocumentID = Guid.NewGuid ().ToString ("N");
			storableBase.ParentID = Guid.NewGuid ();
			var storableBase2 = storableBase.Clone ();
			Assert.AreEqual (storableBase.DocumentID, storableBase2.DocumentID);
			Assert.AreEqual (storableBase.ParentID, storableBase2.ParentID);
		}

		[Test ()]
		public void TestClone_CloneIgnore_Team ()
		{
			var team = new DummyTeam ();
			team.Static = true;
			team.Color = Color.Black;
			var team2 = team.Clone ();
			Assert.AreEqual (team.Static, team2.Static);
			Assert.AreEqual (team.Color, team2.Color);
		}

		[Test ()]
		public void TestClone_CloneIgnore_TimelineEvent ()
		{
			var timelineEvent = new TimelineEvent ();
			timelineEvent.IsLoaded = false;
			timelineEvent.DocumentID = Guid.NewGuid ().ToString ("N");
			timelineEvent.ParentID = Guid.NewGuid ();
			timelineEvent.Project = new DummyProject ();
			timelineEvent.Playing = true;
			timelineEvent.IsLoaded = true;
			timelineEvent.Start = new Time (1000);

			var timelineEvent2 = timelineEvent.Clone ();
			Assert.AreEqual (timelineEvent.IsLoaded, timelineEvent2.IsLoaded);
			Assert.AreEqual (timelineEvent.DocumentID, timelineEvent2.DocumentID);
			Assert.AreEqual (timelineEvent.ParentID, timelineEvent2.ParentID);
			Assert.AreEqual (timelineEvent.Project, timelineEvent2.Project);
			Assert.AreEqual (timelineEvent.Playing, timelineEvent2.Playing);
		}

		[Test ()]
		public void TestClone_CloneIgnore_JsonCloneTester ()
		{
			var test = new JsonCloneTester ();
			var test2 = test.Clone ();
			Assert.AreEqual (test.theString, test2.theString);
			Assert.AreEqual (test.AnotherString, test2.AnotherString);
			Assert.AreEqual (test.JsonIgnoreString, test2.JsonIgnoreString);
			Assert.AreNotEqual (test.JsonIgnoreAndCloneIgnoreString, test2.JsonIgnoreAndCloneIgnoreString);
			Assert.AreNotEqual (test.OnlyPublicGetterString, test2.OnlyPublicGetterString);
			Assert.AreNotEqual (test.OnlyGetterString, test2.OnlyGetterString);
		}
	}

	class DummyTeam : Team
	{
	}

	class DummyPlayer : Player
	{
	}

	class DummyProject : Project
	{
		public override void AddEvent (TimelineEvent play)
		{
			throw new NotImplementedException ();
		}

		public new TimelineEvent AddEvent (EventType type, Time start, Time stop, Time eventTime, Image miniature)
		{
			throw new NotImplementedException ();
		}

		public override TimelineEvent CreateEvent (EventType type, Time start, Time stop, Time eventTime, Image miniature, int index = 0)
		{
			throw new NotImplementedException ();
		}
	}

	class JsonCloneTester
	{
		public string theString;

		public string AnotherString { get; set; }

		[JsonIgnore]
		public string JsonIgnoreString { get; set; }

		[CloneIgnoreAttribute]
		[JsonIgnore]
		public string JsonIgnoreAndCloneIgnoreString { get; set; }

		public string OnlyPublicGetterString { get; private set; }

		public string OnlyGetterString { get; }

		public JsonCloneTester ()
		{
			theString = GetGuidString ();
			AnotherString = GetGuidString ();
			JsonIgnoreString = GetGuidString ();
			JsonIgnoreAndCloneIgnoreString = GetGuidString ();
			OnlyPublicGetterString = GetGuidString ();
			OnlyGetterString = GetGuidString ();
		}

		public string GetGuidString ()
		{
			return Guid.NewGuid ().ToString ("N");
		}
	}
}
