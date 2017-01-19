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
using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.ViewModel;
using VAS.Services.Controller;

namespace VAS.Tests.Services
{
	[TestFixture]
	public class TestTaggingController
	{
		TaggingController<DummyTeam, DummyTeamVM> controller;
		PlayerVM player1;
		PlayerVM player2;
		PlayerVM player3;
		RangeObservableCollection<PlayerVM> players;
		ProjectVM project;

		[TestFixtureSetUp]
		public void SetUpOnce ()
		{
			SetupClass.Initialize ();
			controller = new TaggingController<DummyTeam, DummyTeamVM> ();
			controller.Start ();
		}

		[TestFixtureTearDown]
		public void TearDownOnce ()
		{
			controller.Stop ();
		}

		[SetUp]
		public void Setup ()
		{
			player1 = new PlayerVM ();
			player2 = new PlayerVM ();
			player3 = new PlayerVM ();
			players = new RangeObservableCollection<PlayerVM> () { player1, player2, player3 };
			project = new ProjectVM { Players = players };

			controller.SetViewModel (project);
		}

		[Test ()]
		public void Test_HandleClicked_PlayerIsLocked ()
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
		public void Test_HandleClicked_PlayerIsTagged ()
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
		public void Test_HandleClicked_TwoPlayersAreSelected ()
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
		public void Test_HandleClicked_TwoPlayersAreLocked ()
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
		public void Test_HandleReset_ResetAll ()
		{
			// Arrange
			player1.Tagged = true;
			player2.Tagged = true;
			player3.Tagged = true;
			var pCardEvent = new ResetPCardEvent ();

			// Action
			App.Current.EventsBroker.Publish (pCardEvent);

			// Assert
			Assert.IsFalse (player1.Tagged, "Player 1 should not be tagged");
			Assert.IsFalse (player2.Tagged, "Player 2 should not be tagged");
			Assert.IsFalse (player3.Tagged, "Player 3 should not be tagged");
		}

		[Test ()]
		public void Test_HandleReset_WithALockedPlayer ()
		{
			// Arrange
			player1.Tagged = true;
			player2.Locked = true;
			player2.Tagged = true;
			player3.Tagged = true;
			var pCardEvent = new ResetPCardEvent ();

			// Action
			App.Current.EventsBroker.Publish (pCardEvent);

			// Assert
			Assert.IsFalse (player1.Tagged, "Player 1 should not be tagged");
			Assert.IsTrue (player2.Locked, "Player 2 should be locked");
			Assert.IsTrue (player2.Tagged, "Player 2 should be tagged");
			Assert.IsFalse (player3.Tagged, "Player 3 should not be tagged");
		}
	}
}
