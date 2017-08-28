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
using System.Collections.ObjectModel;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Store;
using DummyPlayer = VAS.Tests.Utils.PlayerDummy;

namespace VAS.Tests.MVVMC
{

	[TestFixture]
	public class TestBindableBase
	{
		[Test]
		public void TestRaiseProperty ()
		{
			int eventCount = 0;
			var bindable = new DummyBindable ();
			Assert.IsFalse (bindable.IsChanged);
			bindable.PropertyChanged += (sender, e) => eventCount++;
			bindable.Raise ("test");
			Assert.IsTrue (bindable.IsChanged);
			Assert.AreEqual (1, eventCount);
		}

		[Test]
		public void TestRaiseChildrenProperty ()
		{
			int evtCount = 0;
			EventType evt = new EventType ();
			evt.Color = Color.Red.Copy ();
			evt.PropertyChanged += (sender, e) => evtCount++;

			evt.Color.R = 3;

			Assert.AreEqual (1, evtCount);
		}

		[Test]
		public void TestChangeChildAndRaiseChildrenProperty ()
		{
			int evtCount = 0;
			EventType evt = new EventType ();
			evt.Color = Color.Red.Copy ();
			evt.Color = Color.Red.Copy ();
			evt.PropertyChanged += (sender, e) => evtCount++;

			evt.Color.R = 3;

			Assert.AreEqual (1, evtCount);
		}

		[Test]
		public void TestRaiseCollectionChanged ()
		{
			int evtCount = 0;
			string name = null;
			AnalysisEventType evt = new AnalysisEventType ();
			evt.Tags = new RangeObservableCollection<Tag> ();
			evt.PropertyChanged += (sender, e) => {
				evtCount++;
				name = e.PropertyName;
			};

			evt.Tags.Add (new Tag ("test"));

			Assert.AreEqual (1, evtCount);
			Assert.AreEqual ("Collection_Tags", name);
		}

		[Test]
		public void TestRaiseCollectionPropertyChanged ()
		{
			int evtCount = 0;
			AnalysisEventType evt = new AnalysisEventType ();
			evt.PropertyChanged += (sender, e) => evtCount++;

			evt.Tags = new RangeObservableCollection<Tag> ();

			Assert.AreEqual (1, evtCount);
		}

		[Test]
		public void TestRaiseInCollectionChildren ()
		{
			int evtCount = 0;
			AnalysisEventType evt = new AnalysisEventType ();
			evt.Tags = new RangeObservableCollection<Tag> ();

			evt.Tags.Add (new Tag ("test"));
			evt.PropertyChanged += (sender, e) => evtCount++;
			evt.Tags [0].Value = "TT";

			Assert.AreEqual (1, evtCount);
		}

		[Test]
		public void TestDoNotRaiseAfterRemovingCollectionChildren ()
		{
			int evtCount = 0;
			Tag tag = new Tag ("test");
			AnalysisEventType evt = new AnalysisEventType ();
			evt.Tags = new RangeObservableCollection<Tag> ();

			evt.Tags.Add (tag);
			evt.Tags.Remove (tag);
			evt.PropertyChanged += (sender, e) => evtCount++;

			tag.Value = "TT";

			Assert.AreEqual (0, evtCount);
		}

		[Test]
		public void TestIgnoreEvents ()
		{
			// Arrange
			int eventCount = 0;
			var bindable = new DummyBindable ();
			Assert.IsFalse (bindable.IsChanged);
			bindable.PropertyChanged += (sender, e) => eventCount++;

			// Act
			bindable.IgnoreEvents = true;
			bindable.Raise ("test");
			bindable.IgnoreEvents = false;

			// Assert
			Assert.IsTrue (bindable.IsChanged);
			Assert.AreEqual (0, eventCount);
		}

		[Test]
		public void TestIsChangedSetBeforeRaising ()
		{
			bool isChanged = false;
			AnalysisEventType evt = new AnalysisEventType ();
			evt.IsChanged = false;
			evt.PropertyChanged += (sender, e) => {
				isChanged = evt.IsChanged;
			};

			evt.Name = "RRR";

			Assert.IsTrue (evt.IsChanged);
			Assert.IsTrue (isChanged);
		}

		[Test]
		public void TestEventForwardingInCollectionReplacements ()
		{
			// Arrange
			List<object> senderObjects = new List<object> ();
			DummyPlayer p1 = new DummyPlayer {
				Name = "Player 1"
			};
			DummyPlayer p2 = new DummyPlayer {
				Name = "Player 2"
			};
			DummyTeam team = new DummyTeam ();
			team.List.Add (p1);
			team.List.Add (p2);

			team.List.Swap (p1, p2);

			team.IsChanged = false;
			p1.IsChanged = false;
			p2.IsChanged = false;

			team.PropertyChanged += (sender, e) => {
				senderObjects.Add (sender);
			};

			// Action
			p2.Nationality = "FR";
			p1.Nationality = "DE";

			//Assert
			Assert.AreEqual (p2.Nationality, team.List [0].Nationality);
			Assert.AreEqual (p1.Nationality, team.List [1].Nationality);
			Assert.IsTrue (team.IsChanged);
			Assert.IsTrue (p1.IsChanged);
			Assert.IsTrue (p2.IsChanged);
			Assert.AreEqual (2, senderObjects.Count);
			Assert.AreEqual (p2, senderObjects [0]);
			Assert.AreEqual (p1, senderObjects [1]);
		}
	}
}

