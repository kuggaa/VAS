//
//  Copyright (C) 2017 Fluendo S.A.
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
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services.Controller;
using VAS.Services.ViewModel;

namespace VAS.Tests.Services
{
	[TestFixture]
	public class TestTaggingController
	{
		TaggingController controller;
		PlayerVM player1;
		PlayerVM player2;
		PlayerVM player3;
		TeamVM team1;
		TeamVM team2;
		List<TeamVM> teams;
		ProjectVM project;
		TimelineEvent sendedTimelineEvent;
		VideoPlayerVM videoPlayer;
		bool hasSendedDashboardEvent;

		[OneTimeSetUp]
		public void SetUpOnce ()
		{
			SetupClass.SetUp ();
		}

		[SetUp]
		public void Setup ()
		{
			player1 = new PlayerVM { Model = new Utils.PlayerDummy () };
			player2 = new PlayerVM { Model = new Utils.PlayerDummy () };
			player3 = new PlayerVM { Model = new Utils.PlayerDummy () };

			team1 = new TeamVM ();
			team1.ViewModels.Add (player1);
			team1.ViewModels.Add (player2);

			team2 = new TeamVM ();
			team2.ViewModels.Add (player3);

			teams = new List<TeamVM> { team1, team2 };
			project = new DummyProjectVM (teams) { Model = new Utils.ProjectDummy () };
			videoPlayer = new VideoPlayerVM {
				CamerasConfig = new ObservableCollection<CameraConfig> ()
			};
			controller = new Utils.DummyTaggingController ();
			controller.SetViewModel (new ProjectAnalysisVM<ProjectVM> {
				Project = project,
				VideoPlayer = videoPlayer
			});

			controller.Start ();
			sendedTimelineEvent = null;
			hasSendedDashboardEvent = true;
			App.Current.EventsBroker.Subscribe<NewDashboardEvent> (HandleNewDashboardEvent);
		}

		[TearDown]
		public void TearDown ()
		{
			App.Current.EventsBroker.Unsubscribe<NewDashboardEvent> (HandleNewDashboardEvent);
			controller.Stop ();
		}

		[Test ()]
		public void TestHandleClickedWhenPlayerIsLocked ()
		{
			// Arrange
			player2.Tagged = false;
			player2.Locked = false;
			var pCardEvent = new ClickedPCardEvent () {
				ClickedPlayer = player2,
				Modifier = ButtonModifier.Control
			};

			// Action
			App.Current.EventsBroker.Publish (pCardEvent);

			// Assert
			Assert.IsTrue (player2.Tagged, "Player 2 should be tagged");
			Assert.IsTrue (player2.Locked, "Player 2 should be locked");
		}

		[Test ()]
		public void TestHandleClickedWhenPlayerIsTagged ()
		{
			// Arrange
			player2.Tagged = false;
			player2.Locked = false;
			var pCardEvent = new ClickedPCardEvent () {
				ClickedPlayer = player2,
				Modifier = ButtonModifier.None
			};

			// Action
			App.Current.EventsBroker.Publish (pCardEvent);

			// Assert
			Assert.IsTrue (player2.Tagged, "Player 2 should be tagged");
			Assert.IsFalse (player2.Locked, "Player 2 should not be locked");
		}

		[Test ()]
		public void TestHandleClickedWhenPlayerIsTaggedAndLocked ()
		{
			// Arrange
			player2.Tagged = true;
			player2.Locked = false;
			var pCardEvent = new ClickedPCardEvent () {
				ClickedPlayer = player2,
				Modifier = ButtonModifier.Control
			};

			// Action
			App.Current.EventsBroker.Publish (pCardEvent);

			// Assert
			Assert.IsTrue (player2.Tagged, "Player 2 should be tagged");
			Assert.IsTrue (player2.Locked, "Player 2 should be locked");
		}

		[Test ()]
		public void TestHandleClickedWhenTwoPlayersAreSelected ()
		{
			// Arrange
			player1.Tagged = false;
			player1.Locked = false;
			player2.Tagged = false;
			player2.Locked = false;

			var pCardEvent = new ClickedPCardEvent () {
				ClickedPlayer = player2,
				Modifier = ButtonModifier.None
			};

			var pCardEvent2 = new ClickedPCardEvent () {
				ClickedPlayer = player1,
				Modifier = ButtonModifier.Shift
			};

			// Action
			App.Current.EventsBroker.Publish (pCardEvent);
			App.Current.EventsBroker.Publish (pCardEvent2);

			// Assert
			Assert.IsTrue (player1.Tagged, "Player 1 should be tagged");
			Assert.IsFalse (player1.Locked, "Player 1 should not be locked");
			Assert.IsTrue (player2.Tagged, "Player 2 should be tagged");
			Assert.IsFalse (player2.Locked, "Player 2 should not be locked");
		}

		[Test ()]
		public void TestHandleClickedWhenTwoPlayersAreLocked ()
		{
			// Arrange
			player1.Tagged = false;
			player1.Locked = false;
			player2.Tagged = false;
			player2.Locked = false;

			var pCardEvent = new ClickedPCardEvent () {
				ClickedPlayer = player2,
				Modifier = ButtonModifier.Control
			};

			var pCardEvent2 = new ClickedPCardEvent () {
				ClickedPlayer = player1,
				Modifier = ButtonModifier.Control
			};

			// Action
			App.Current.EventsBroker.Publish (pCardEvent);
			App.Current.EventsBroker.Publish (pCardEvent2);

			// Assert
			Assert.IsTrue (player1.Tagged, "Player 1 should be tagged");
			Assert.IsTrue (player1.Locked, "Player 1 should be locked");
			Assert.IsTrue (player2.Tagged, "Player 2 should be tagged");
			Assert.IsTrue (player2.Locked, "Player 2 should be locked");
		}

		[Test ()]
		public void TestHandleResetWhenResetAll ()
		{
			// Arrange
			player1.Tagged = true;
			player2.Tagged = true;
			player3.Tagged = true;
			var newTagEvent = new NewTagEvent {
				EventType = new EventType { Name = "test" },
				Start = new Time (0),
				Stop = new Time (10),
				Tags = new List<Tag> (),
				EventTime = new Time (9),
				Button = null
			};

			// Action
			App.Current.EventsBroker.Publish (newTagEvent);

			// Assert
			Assert.IsFalse (player1.Tagged, "Player 1 should not be tagged");
			Assert.IsFalse (player2.Tagged, "Player 2 should not be tagged");
			Assert.IsFalse (player3.Tagged, "Player 3 should not be tagged");
		}

		[Test ()]
		public void TestHandleResetWhenAPlayerIsLocked ()
		{
			// Arrange
			player1.Tagged = true;
			player2.Locked = true;
			player2.Tagged = true;
			player3.Tagged = true;

			var newTagEvent = new NewTagEvent {
				EventType = new EventType { Name = "test" },
				Start = new Time (0),
				Stop = new Time (10),
				Tags = new List<Tag> (),
				EventTime = new Time (9),
				Button = null
			};

			// Action
			App.Current.EventsBroker.Publish (newTagEvent);

			// Assert
			Assert.IsFalse (player1.Tagged, "Player 1 should not be tagged");
			Assert.IsTrue (player2.Locked, "Player 2 should be locked");
			Assert.IsTrue (player2.Tagged, "Player 2 should be tagged");
			Assert.IsFalse (player3.Tagged, "Player 3 should not be tagged");
		}

		[Test]
		public void TestNewTagEvent ()
		{
			var newTagEvent = new NewTagEvent {
				EventType = project.Model.EventTypes [0],
				Start = new Time (0),
				Stop = new Time (10),
				Tags = new List<Tag> (),
				EventTime = new Time (9),
				Button = null
			};

			// Action
			App.Current.EventsBroker.Publish (newTagEvent);

			Assert.AreEqual (new Time (9).MSeconds, sendedTimelineEvent.EventTime.MSeconds);
			Assert.IsTrue (hasSendedDashboardEvent);
		}

		[Test]
		public void TestNewTagEventWithPlayers ()
		{
			player1.Tagged = true;
			player3.Tagged = true;

			var newTagEvent = new NewTagEvent {
				EventType = project.Model.EventTypes [0],
				Start = new Time (0),
				Stop = new Time (10),
				Tags = new List<Tag> (),
				EventTime = new Time (9),
				Button = null
			};

			// Action
			App.Current.EventsBroker.Publish (newTagEvent);

			Assert.AreEqual (new Time (9).MSeconds, sendedTimelineEvent.EventTime.MSeconds);
			Assert.AreEqual (2, sendedTimelineEvent.Players.Count);
		}

		[Test]
		public void TestNewTagEventWithTeam ()
		{
			team1.Tagged = true;

			var newTagEvent = new NewTagEvent {
				EventType = project.Model.EventTypes [0],
				Start = new Time (0),
				Stop = new Time (10),
				Tags = new List<Tag> (),
				EventTime = new Time (9),
				Button = null
			};

			// Action
			App.Current.EventsBroker.Publish (newTagEvent);

			Assert.AreEqual (1, sendedTimelineEvent.Teams.Count);
			Assert.AreSame (team1.Model, sendedTimelineEvent.Teams.First ());

		}

		[Test]
		public void TestCurrenTimeUpdated ()
		{
			Time time = new Time (5000);
			videoPlayer.CurrentTime = time;

			Assert.AreEqual (time, project.Dashboard.CurrentTime);
		}

		[Test]
		public void TestCapturerTimeUpdated ()
		{
			Time time = new Time (3000);

			App.Current.EventsBroker.Publish (new CapturerTickEvent { Time = time });

			Assert.AreEqual (time, project.Dashboard.CurrentTime);
		}

		void HandleNewDashboardEvent (NewDashboardEvent e)
		{
			sendedTimelineEvent = e.TimelineEvent;
			hasSendedDashboardEvent = true;
		}
	}
}
