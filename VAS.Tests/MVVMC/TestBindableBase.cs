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
using System.Collections.ObjectModel;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Store;

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
			AnalysisEventType evt = new AnalysisEventType ();
			evt.Tags = new ObservableCollection<Tag> ();
			evt.PropertyChanged += (sender, e) => evtCount++;

			evt.Tags.Add (new Tag ("test"));

			Assert.AreEqual (1, evtCount);
		}

		[Test]
		public void TestRaiseCollectionPropertyChanged ()
		{
			int evtCount = 0;
			AnalysisEventType evt = new AnalysisEventType ();
			evt.PropertyChanged += (sender, e) => evtCount++;

			evt.Tags = new ObservableCollection<Tag> ();

			Assert.AreEqual (1, evtCount);
		}

		[Test]
		public void TestRaiseInCollectionChildren ()
		{
			int evtCount = 0;
			AnalysisEventType evt = new AnalysisEventType ();
			evt.Tags = new ObservableCollection<Tag> ();

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
			evt.Tags = new ObservableCollection<Tag> ();

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
			Assert.IsFalse (bindable.IsChanged);
			Assert.AreEqual (0, eventCount);
		}
	}
}

