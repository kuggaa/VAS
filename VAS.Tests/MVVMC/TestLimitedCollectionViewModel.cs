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
using System.Collections.Specialized;
using System.Linq;
using NUnit.Framework;
using VAS.Core.License;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;

namespace VAS.Tests.MVVMC
{
	[TestFixture]
	public class TestLimitedCollectionViewModel
	{
		LimitedCollectionViewModel<Utils.PlayerDummy, DummyPlayerVM> col;
		List<Utils.PlayerDummy> players;

		[SetUp]
		public void SetUp ()
		{
			col = new LimitedCollectionViewModel<Utils.PlayerDummy, DummyPlayerVM> ();
			col.Limitation = new LicenseLimitationVM ();
			players = CreateDummyPlayers ();
		}

		[Test]
		public void TestWithoutLimitationWithoutSort ()
		{
			col.SortByCreationDateDesc = false;

			col.Model.AddRange (players);

			CheckPlayersInLimitedCollectionBy (new int [] { 1, 2, 0 }, players, col);
		}

		[Test]
		public void TestWithoutLimitationWithSort ()
		{
			col.Model.AddRange (players);

			CheckPlayersInLimitedCollectionBy (new int [] { 0, 2, 1 }, players, col);
		}

		[Test]
		public void TestWithLimitationDisabledWithoutSort ()
		{
			LicenseLimitation ll = new LicenseLimitation { Enabled = false, Maximum = 2 };
			col.SortByCreationDateDesc = false;

			col.Limitation.Model = ll;
			col.Model.AddRange (players);

			CheckPlayersInLimitedCollectionBy (new int [] { 1, 2, 0 }, players, col);
		}

		[Test]
		public void TestWithLimitationDisabledWithSort ()
		{
			LicenseLimitation ll = new LicenseLimitation { Enabled = false, Maximum = 2 };

			col.Limitation.Model = ll;
			col.Model.AddRange (players);

			CheckPlayersInLimitedCollectionBy (new int [] { 0, 2, 1 }, players, col);
		}

		[Test]
		public void TestWithLimitationEnabledWithoutSort ()
		{
			LicenseLimitation ll = new LicenseLimitation { Enabled = true, Maximum = 2 };
			col.SortByCreationDateDesc = false;

			col.Limitation.Model = ll;
			col.Model.AddRange (players);

			CheckPlayersInLimitedCollectionBy (new int [] { 1, 2 }, players, col);
		}

		[Test]
		public void TestWithLimitationEnabledWithSort ()
		{
			LicenseLimitation ll = new LicenseLimitation { Enabled = true, Maximum = 2 };

			col.Limitation.Model = ll;
			col.Model.AddRange (players);

			CheckPlayersInLimitedCollectionBy (new int [] { 0, 2 }, players, col);
		}

		[Test]
		public void TestAddWithLimitationEnabledWithSort ()
		{
			LicenseLimitation ll = new LicenseLimitation { Enabled = true, Maximum = 3 };

			col.Limitation.Model = ll;
			col.Model.AddRange (players);
			List<Utils.PlayerDummy> newPlayers = new List<Utils.PlayerDummy> ();
			newPlayers.Add (new Utils.PlayerDummy { CreationDate = DateTime.Now.AddMinutes (1), Name = "P4" });
			players.AddRange (newPlayers);

			col.Model.AddRange (newPlayers);

			CheckPlayersInLimitedCollectionBy (new int [] { 3, 0, 2 }, players, col);
		}

		[Test]
		public void TestModifyMaxLimitationEnabledWithoutSort ()
		{
			LicenseLimitation ll = new LicenseLimitation { Enabled = true, Maximum = 3 };
			col.SortByCreationDateDesc = false;
			col.Limitation.Model = ll;
			col.Model.AddRange (players);
			List<Utils.PlayerDummy> newPlayers = new List<Utils.PlayerDummy> ();
			newPlayers.Add (new Utils.PlayerDummy { CreationDate = DateTime.Now.AddMinutes (1), Name = "P4" });
			players.AddRange (newPlayers);
			col.Model.AddRange (newPlayers);
			CheckPlayersInLimitedCollectionBy (new int [] { 1, 2, 0 }, players, col);

			ll.Maximum = 2;

			CheckPlayersInLimitedCollectionBy (new int [] { 1, 2 }, players, col);
		}

		[Test]
		public void TestModifyMaxLimitationEnabledWithSort ()
		{
			LicenseLimitation ll = new LicenseLimitation { Enabled = true, Maximum = 3 };

			col.Limitation.Model = ll;
			col.Model.AddRange (players);
			List<Utils.PlayerDummy> newPlayers = new List<Utils.PlayerDummy> ();
			newPlayers.Add (new Utils.PlayerDummy { CreationDate = DateTime.Now.AddMinutes (1), Name = "P4" });
			players.AddRange (newPlayers);
			col.Model.AddRange (newPlayers);
			CheckPlayersInLimitedCollectionBy (new int [] { 3, 0, 2 }, players, col);

			ll.Maximum = 2;

			CheckPlayersInLimitedCollectionBy (new int [] { 3, 0 }, players, col);
		}

		[Test]
		public void TestModifyEnabledLimitationEnabledWithoutSort ()
		{
			LicenseLimitation ll = new LicenseLimitation { Enabled = false, Maximum = 3 };
			col.SortByCreationDateDesc = false;
			col.Limitation.Model = ll;
			col.Model.AddRange (players);
			List<Utils.PlayerDummy> newPlayers = new List<Utils.PlayerDummy> ();
			newPlayers.Add (new Utils.PlayerDummy { CreationDate = DateTime.Now.AddMinutes (1), Name = "P4" });
			players.AddRange (newPlayers);
			col.Model.AddRange (newPlayers);
			CheckPlayersInLimitedCollectionBy (new int [] { 1, 2, 0, 3 }, players, col);

			ll.Enabled = true;

			CheckPlayersInLimitedCollectionBy (new int [] { 1, 2, 0 }, players, col);
		}

		[Test]
		public void TestModifyEnabledLimitationEnabledWithSort ()
		{
			LicenseLimitation ll = new LicenseLimitation { Enabled = false, Maximum = 3 };

			col.Limitation.Model = ll;
			col.Model.AddRange (players);
			List<Utils.PlayerDummy> newPlayers = new List<Utils.PlayerDummy> ();
			newPlayers.Add (new Utils.PlayerDummy { CreationDate = DateTime.Now.AddMinutes (1), Name = "P4" });
			players.AddRange (newPlayers);
			col.Model.AddRange (newPlayers);
			CheckPlayersInLimitedCollectionBy (new int [] { 3, 0, 2, 1 }, players, col);

			ll.Enabled = true;

			CheckPlayersInLimitedCollectionBy (new int [] { 3, 0, 2 }, players, col);
		}

		[Test]
		public void TestModifyEnabledLimitationEnableReversedWithoutSort ()
		{
			LicenseLimitation ll = new LicenseLimitation { Enabled = true, Maximum = 3 };
			col.SortByCreationDateDesc = false;
			col.Limitation.Model = ll;
			col.Model.AddRange (players);
			List<Utils.PlayerDummy> newPlayers = new List<Utils.PlayerDummy> ();
			newPlayers.Add (new Utils.PlayerDummy { CreationDate = DateTime.Now.AddMinutes (1), Name = "P4" });
			players.AddRange (newPlayers);
			col.Model.AddRange (newPlayers);
			CheckPlayersInLimitedCollectionBy (new int [] { 1, 2, 0 }, players, col);

			ll.Enabled = false;

			CheckPlayersInLimitedCollectionBy (new int [] { 1, 2, 0, 3 }, players, col);

		}

		[Test]
		public void TestModifyEnabledLimitationEnabledReverseWithSort ()
		{
			LicenseLimitation ll = new LicenseLimitation { Enabled = true, Maximum = 3 };

			col.Limitation.Model = ll;
			col.Model.AddRange (players);
			List<Utils.PlayerDummy> newPlayers = new List<Utils.PlayerDummy> ();
			newPlayers.Add (new Utils.PlayerDummy { CreationDate = DateTime.Now.AddMinutes (1), Name = "P4" });
			players.AddRange (newPlayers);
			col.Model.AddRange (newPlayers);
			CheckPlayersInLimitedCollectionBy (new int [] { 3, 0, 2 }, players, col);

			ll.Enabled = false;

			CheckPlayersInLimitedCollectionBy (new int [] { 3, 0, 2, 1 }, players, col);
		}

		[Test]
		public void TestNotifyCollection ()
		{
			// Arrange
			NotifyCollectionChangedEventArgs receivedEvent = null;
			bool limitedEqualsViewModel = false;
			bool getNotifyEqualsViewModel = false;
			bool secondViewModelEqualsViewModel = false;

			LicenseLimitation ll = new LicenseLimitation { Enabled = true, Maximum = 3 };
			col.Limitation.Model = ll;

			col.ViewModels.CollectionChanged += (sender, e) => receivedEvent = e;
			col.LimitedViewModels.CollectionChanged += (sender, e) =>
				limitedEqualsViewModel = e == receivedEvent &&
				sender == col.LimitedViewModels;
			col.GetNotifyCollection ().CollectionChanged += (sender, e) =>
				getNotifyEqualsViewModel = e == receivedEvent &&
				sender == col.LimitedViewModels;
			col.ViewModels.CollectionChanged += (sender, e) =>
				secondViewModelEqualsViewModel = e == receivedEvent &&
				sender == col.LimitedViewModels;

			//Act
			col.Model.AddRange (players);

			// Assert
			Assert.AreEqual (players.Count, col.LimitedViewModels.Count);
			Assert.IsNotNull (receivedEvent);

			Assert.IsTrue (limitedEqualsViewModel);
			Assert.IsTrue (getNotifyEqualsViewModel);
			Assert.IsTrue (secondViewModelEqualsViewModel);
		}

		List<Utils.PlayerDummy> CreateDummyPlayers ()
		{
			List<Utils.PlayerDummy> players = new List<Utils.PlayerDummy> ();
			players.Add (new Utils.PlayerDummy { CreationDate = DateTime.Now, Name = "P1" });
			players.Add (new Utils.PlayerDummy { CreationDate = DateTime.Now.AddMinutes (-2), Name = "P2" });
			players.Add (new Utils.PlayerDummy { CreationDate = DateTime.Now.AddMinutes (-1), Name = "P3" });
			return players;
		}

		public void CheckPlayersInLimitedCollectionBy (
			int [] orderPlayers, List<Utils.PlayerDummy> players,
			LimitedCollectionViewModel<Utils.PlayerDummy, DummyPlayerVM> collection)
		{
			Assert.AreEqual (orderPlayers.Length, collection.Count ());
			for (int i = 0; i < orderPlayers.Length; i++) {
				Assert.AreEqual (players [orderPlayers [i]].Name, collection.ElementAt (i).Model.Name);
			}
		}
	}
}
