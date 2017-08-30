//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System.Collections.ObjectModel;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Store;
using VAS.Core.Store.Templates;

namespace VAS.Tests.Core.Store.Templates
{
	[TestFixture ()]
	public class TestDashboard
	{
		[Test ()]
		public void TestSerialization ()
		{
			Utils.DashboardDummy cat = new Utils.DashboardDummy ();

			Utils.CheckSerialization (cat);

			cat.Name = "test";
			cat.GamePeriods = new ObservableCollection<string> { "1", "2" };
			cat.List.Add (new AnalysisEventButton { Name = "cat1" });
			cat.List.Add (new AnalysisEventButton { Name = "cat2" });
			cat.List.Add (new AnalysisEventButton { Name = "cat3" });

			Utils.CheckSerialization (cat);

			Dashboard newcat = Utils.SerializeDeserialize (cat);
			Assert.AreEqual (cat.ID, newcat.ID);
			Assert.AreEqual (cat.Name, newcat.Name);
			Assert.AreEqual (cat.GamePeriods.Count, newcat.GamePeriods.Count);
			Assert.AreEqual (cat.GamePeriods [0], newcat.GamePeriods [0]);
			Assert.AreEqual (cat.GamePeriods [1], newcat.GamePeriods [1]);
			Assert.AreEqual (cat.List.Count, newcat.List.Count);
		}

		[Test]
		public void TestVersion ()
		{
			Assert.AreEqual (0, new Utils.DashboardDummy ().Version);
		}

		[Test ()]
		public void TestCircularDepdencies ()
		{
			Utils.DashboardDummy dashboard = new Utils.DashboardDummy ();
			DashboardButton b1 = new DashboardButton ();
			DashboardButton b2 = new DashboardButton ();
			DashboardButton b3 = new DashboardButton ();
			dashboard.List.Add (b1);
			dashboard.List.Add (b2);
			dashboard.List.Add (b3);

			b1.AddActionLink (new ActionLink { DestinationButton = b2 });
			Assert.IsFalse (dashboard.HasCircularDependencies ());
			b2.AddActionLink (new ActionLink { DestinationButton = b3 });
			Assert.IsFalse (dashboard.HasCircularDependencies ());
			b3.AddActionLink (new ActionLink { DestinationButton = b1 });
			Assert.IsTrue (dashboard.HasCircularDependencies ());
		}

		[Test ()]
		public void TestRemoveButton ()
		{
			Utils.DashboardDummy dashboard = new Utils.DashboardDummy ();
			DashboardButton b1 = new DashboardButton ();
			DashboardButton b2 = new DashboardButton ();
			DashboardButton b3 = new DashboardButton ();
			dashboard.List.Add (b1);
			dashboard.List.Add (b2);
			dashboard.List.Add (b3);

			b1.ActionLinks.Add (new ActionLink { SourceButton = b1, DestinationButton = b2 });
			b2.ActionLinks.Add (new ActionLink { SourceButton = b2, DestinationButton = b3 });
			b3.ActionLinks.Add (new ActionLink { SourceButton = b3, DestinationButton = b1 });

			dashboard.RemoveButton (b3);
			Assert.AreEqual (0, b2.ActionLinks.Count);
			dashboard.RemoveButton (b2);
			Assert.AreEqual (0, b1.ActionLinks.Count);
		}

		[Test ()]
		public void RemoveDeadLinks ()
		{
			Utils.DashboardDummy dashboard = new Utils.DashboardDummy ();
			AnalysisEventButton b1 = dashboard.AddDefaultItem (0);
			AnalysisEventButton b2 = dashboard.AddDefaultItem (1);
			dashboard.AddDefaultTags (b1.AnalysisEventType);
			dashboard.AddDefaultTags (b2.AnalysisEventType);

			b1.ActionLinks.Add (new ActionLink { SourceButton = b1, DestinationButton = b2 });
			dashboard.RemoveDeadLinks (b2);
			Assert.AreEqual (1, b1.ActionLinks.Count);

			b1.ActionLinks [0].DestinationTags = new RangeObservableCollection<Tag> { b2.AnalysisEventType.Tags [0] };
			dashboard.RemoveDeadLinks (b2);
			Assert.AreEqual (1, b1.ActionLinks.Count);

			b2.AnalysisEventType.Tags.Remove (b2.AnalysisEventType.Tags [1]);
			dashboard.RemoveDeadLinks (b2);
			Assert.AreEqual (1, b1.ActionLinks.Count);
			b2.AnalysisEventType.Tags.Remove (b2.AnalysisEventType.Tags [0]);
			dashboard.RemoveDeadLinks (b2);
			Assert.AreEqual (0, b1.ActionLinks.Count);
		}
	}
}