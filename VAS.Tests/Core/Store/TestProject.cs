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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;

namespace VAS.Tests.Core.Store
{
	[TestFixture ()]
	public class TestProject
	{
		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			App.Current.ProjectExtension = ".tmp";
		}

		Utils.ProjectDummy CreateProject (bool fill = true)
		{
			Utils.ProjectDummy p = new Utils.ProjectDummy ();
			p.UpdateEventTypesAndTimers ();
			/*
			 * Requires a dashboard with DefaultTemplate
			 * 
			if (fill) {
				p.AddEvent (p.EventTypes [0], new Time (1000), new Time (2000), null, null);
				p.AddEvent (p.EventTypes [0], new Time (1000), new Time (2000), null, null);
				p.AddEvent (p.EventTypes [1], new Time (1000), new Time (2000), null, null);
				p.AddEvent (p.EventTypes [2], new Time (1000), new Time (2000), null, null);
				p.AddEvent (p.EventTypes [2], new Time (1000), new Time (2000), null, null);
				p.AddEvent (p.EventTypes [2], new Time (1000), new Time (2000), null, null);
				p.AddEvent (p.EventTypes [6], new Time (1000), new Time (2000), null, null);
			}
			*/
			return p;
		}

		[Test ()]
		public void TestSerialization ()
		{
			Utils.ProjectDummy p = new Utils.ProjectDummy ();

			Utils.CheckSerialization (p);

			p = CreateProject ();
			Utils.CheckSerialization (p);
			p.AddEvent (new TimelineEvent ());
			Utils.CheckSerialization (p);

			Utils.ProjectDummy newp = Utils.SerializeDeserialize (p);
			Assert.AreEqual (newp.CompareTo (p), 0);
			Assert.AreEqual (newp.Timeline.Count, p.Timeline.Count);
		}

		[Test ()]
		public void TestProjectSetInTimelineEvents ()
		{
			Utils.ProjectDummy p = CreateProject ();
			TimelineEvent evt = new TimelineEvent ();
			p.AddEvent (evt);
			Utils.ProjectDummy newp = Utils.SerializeDeserialize (p);
			Assert.AreEqual (newp, newp.Timeline [0].Project);
		}

		[Test ()]
		[Ignore ("From LongoMatch - DefaultTemplate not available for VAS")]
		public void TestEventsGroupedByEventType ()
		{
			Utils.ProjectDummy p = CreateProject ();
			var g = p.EventsGroupedByEventType;
			Assert.AreEqual (g.Count (), 4);
			var gr = g.ElementAt (0);
			Assert.AreEqual (p.EventTypes [0], gr.Key);
			Assert.AreEqual (2, gr.Count ());

			gr = g.ElementAt (1);
			Assert.AreEqual (p.EventTypes [1], gr.Key);
			Assert.AreEqual (1, gr.Count ());

			gr = g.ElementAt (2);
			Assert.AreEqual (p.EventTypes [2], gr.Key);
			Assert.AreEqual (3, gr.Count ());

			gr = g.ElementAt (3);
			Assert.AreEqual (p.EventTypes [6], gr.Key);
			Assert.AreEqual (1, gr.Count ());
		}

		[Test ()]
		[Ignore ("Not implemented")]
		public void Clear ()
		{
		}

		[Test ()]
		[Ignore ("From LongoMatch - DefaultTemplate not available for VAS")]
		public void TestAddEvent ()
		{
			Utils.ProjectDummy p = CreateProject (false);
			TimelineEvent evt = p.AddEvent (p.EventTypes [0], new Time (1000), new Time (2000),
				                    null, null, false);
			Assert.AreEqual (p, evt.Project);

			Assert.AreEqual (p.Timeline.Count, 0);
			p.AddEvent (p.EventTypes [0], new Time (1000), new Time (2000), null, null);
			Assert.AreEqual (p.Timeline.Count, 1);
			p.AddEvent (p.EventTypes [0], new Time (1000), new Time (2000), null, null);
			Assert.AreEqual (p.Timeline.Count, 2);

			evt = new TimelineEvent ();
			p.AddEvent (evt);
			Assert.AreEqual (p, evt.Project);
			Assert.AreEqual (p.Timeline.Count, 3);
			p.AddEvent (new TimelineEvent ());
			Assert.AreEqual (p.Timeline.Count, 4);
			/*FIXME: add test for score event updating pd score */
		}

		[Test ()]
		public void TestRemoveEvents ()
		{
			TimelineEvent p1, p2, p3;
			List<TimelineEvent> plays = new List<TimelineEvent> ();
			Utils.ProjectDummy p = CreateProject (false);

			p1 = new TimelineEvent ();
			p2 = new TimelineEvent ();
			p3 = new TimelineEvent ();
			p.AddEvent (p1);
			p.AddEvent (p2);
			p.AddEvent (p3);
			plays.Add (p1);
			plays.Add (p2);
			p.RemoveEvents (plays);
			Assert.AreEqual (p.Timeline.Count, 1);
			Assert.AreEqual (p.Timeline [0], p3);
		}

		[Test ()] 
		[Ignore ("Not implemented")]
		public void TestCleanupTimers ()
		{
		}

		[Test ()] 
		[Ignore ("From LongoMatch - DefaultTemplate not available for VAS")]
		public void TestUpdateEventTypesAndTimers ()
		{
			Utils.ProjectDummy p = new Utils.ProjectDummy ();
			Assert.AreEqual (0, p.Timers.Count);
			Assert.AreEqual (0, p.EventTypes.Count);
			p.UpdateEventTypesAndTimers ();
			Assert.AreEqual (1, p.Timers.Count);
			Assert.AreEqual (10, p.EventTypes.Count);

			// Delete a category button with no events
			p.Dashboard.List.Remove (p.Dashboard.List.OfType<AnalysisEventButton> ().First ());
			p.UpdateEventTypesAndTimers ();
			Assert.AreEqual (1, p.Timers.Count);
			Assert.AreEqual (9, p.EventTypes.Count);

			// Delete a category button with events in the timeline
			AnalysisEventButton button = p.Dashboard.List.OfType<AnalysisEventButton> ().First ();
			p.Timeline.Add (new TimelineEvent { EventType = button.EventType });
			p.UpdateEventTypesAndTimers ();
			Assert.AreEqual (1, p.Timers.Count);
			Assert.AreEqual (9, p.EventTypes.Count);
			p.Dashboard.List.Remove (button);
			p.UpdateEventTypesAndTimers ();
			Assert.AreEqual (1, p.Timers.Count);
			Assert.AreEqual (9, p.EventTypes.Count);

			// Remove the event from the timeline, the event type is no longuer in the dashboard or the timeline
			p.Timeline.Clear ();
			p.UpdateEventTypesAndTimers ();
			Assert.AreEqual (1, p.Timers.Count);
			Assert.AreEqual (8, p.EventTypes.Count);
		}

		[Test ()] 
		[Ignore ("From LongoMatch - DefaultTemplate not available for VAS")]
		public void TestEventsByType ()
		{
			Utils.ProjectDummy p = CreateProject ();
			Assert.AreEqual (2, p.EventsByType (p.EventTypes [0]).Count);
			Assert.AreEqual (1, p.EventsByType (p.EventTypes [1]).Count);
			Assert.AreEqual (3, p.EventsByType (p.EventTypes [2]).Count);
			Assert.AreEqual (0, p.EventsByType (p.EventTypes [3]).Count);
			Assert.AreEqual (1, p.EventsByType (p.EventTypes [6]).Count);
		}

		[Test ()] 
		[Ignore ("Not implemented")]
		public void TestConsolidateDescription ()
		{
		}

		[Test ()] 
		[Ignore ("Not implemented")]
		public void TestEquals ()
		{
			Utils.ProjectDummy p1 = CreateProject ();
			Utils.ProjectDummy p2 = Utils.SerializeDeserialize (p1);
			Utils.ProjectDummy p3 = new Utils.ProjectDummy ();

			Assert.IsTrue (p1.Equals (p2));
			Assert.IsFalse (p1.Equals (p3));
		}

		[Test ()] 
		[Ignore ("Not implemented")]
		public void TestExport ()
		{
		}

		[Test ()] 
		public void TestImport ()
		{
			// Arrange
			string path = Path.GetTempFileName ();
			string videopath = Path.GetTempFileName ();
			string originalPath = Path.Combine ("non-existing-path", Path.GetFileName (videopath));

			Project p = CreateProject ();
			p.FileSet = new MediaFileSet{ new MediaFile{ FilePath = originalPath } };
			Assert.IsFalse (p.FileSet.CheckFiles ());

			Project.Export (p, path);

			try {
				// Act
				Project imported = Project.Import (path);

				// Assert
				Assert.AreEqual (p, imported);
				Assert.AreEqual (videopath, imported.FileSet.First ().FilePath);
				Assert.IsTrue (imported.FileSet.CheckFiles ());
			} finally {
				File.Delete (path);
				File.Delete (videopath);
			}
		}

		[Test ()]
		public void TestResyncEvents ()
		{
			Utils.ProjectDummy p = CreateProject (false);
			int offset1 = 100, offset2 = 120, offset3 = 150;
			Period period;
			List<Period> syncedPeriods;

			period = new Period ();
			period.Nodes.Add (new TimeNode { Start = new Time (0),
				Stop = new Time (3000)
			});
			p.Periods.Add (period);
			period = new Period ();
			period.Nodes.Add (new TimeNode { Start = new Time (3001),
				Stop = new Time (6000)
			});
			p.Periods.Add (period);
			period = new Period ();
			period.Nodes.Add (new TimeNode { Start = new Time (6001),
				Stop = new Time (6500)
			});
			p.Periods.Add (period);

			/* Test with a list of periods that don't match */
			Assert.Throws<IndexOutOfRangeException> (
				delegate {
					p.ResyncEvents (new List<Period> ());
				});

			syncedPeriods = new List<Period> ();
			period = new Period ();
			period.Nodes.Add (new TimeNode {
				Start = new Time (0 + offset1),
				Stop = new Time (3000 + offset1)
			});
			syncedPeriods.Add (period);
			period = new Period ();
			period.Nodes.Add (new TimeNode {
				Start = new Time (3001 + offset2),
				Stop = new Time (6000 + offset2)
			});
			syncedPeriods.Add (period);
			period = new Period ();
			period.Nodes.Add (new TimeNode {
				Start = new Time (6001 + offset3),
				Stop = new Time (6500 + offset3)
			});
			syncedPeriods.Add (period);

			/* 1st Period */
			p.Timeline.Add (new TimelineEvent { EventTime = new Time (0) });
			p.Timeline.Add (new TimelineEvent { EventTime = new Time (1500) });
			p.Timeline.Add (new TimelineEvent { EventTime = new Time (3000) });
			/* 2nd Period */
			p.Timeline.Add (new TimelineEvent { EventTime = new Time (3001) });
			p.Timeline.Add (new TimelineEvent { EventTime = new Time (4500) });
			p.Timeline.Add (new TimelineEvent { EventTime = new Time (6000) });
			/* 3nd Period */
			p.Timeline.Add (new TimelineEvent { EventTime = new Time (6001) });
			p.Timeline.Add (new TimelineEvent { EventTime = new Time (6200) });
			p.Timeline.Add (new TimelineEvent { EventTime = new Time (6500) });

			IList<TimelineEvent> oldTimeline = p.Timeline.Clone ();

			p.ResyncEvents (syncedPeriods);
			Assert.AreEqual (oldTimeline [0].EventTime + offset1, p.Timeline [0].EventTime);
			Assert.AreEqual (oldTimeline [1].EventTime + offset1, p.Timeline [1].EventTime);
			Assert.AreEqual (oldTimeline [2].EventTime + offset1, p.Timeline [2].EventTime);

			Assert.AreEqual (oldTimeline [3].EventTime + offset2, p.Timeline [3].EventTime);
			Assert.AreEqual (oldTimeline [4].EventTime + offset2, p.Timeline [4].EventTime);
			Assert.AreEqual (oldTimeline [5].EventTime + offset2, p.Timeline [5].EventTime);

			Assert.AreEqual (oldTimeline [6].EventTime + offset3, p.Timeline [6].EventTime);
			Assert.AreEqual (oldTimeline [7].EventTime + offset3, p.Timeline [7].EventTime);
			Assert.AreEqual (oldTimeline [8].EventTime + offset3, p.Timeline [8].EventTime);
		}

		[Test ()]
		public void TestIsChanged ()
		{
			Utils.ProjectDummy p = new Utils.ProjectDummy ();
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Dashboard = new Utils.DashboardDummy ();
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Timeline.Add (new TimelineEvent ());
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Timeline = null;
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.EventTypes.Add (new EventType ());
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.EventTypes = null;
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Playlists.Add (new Playlist ());
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Playlists = null;
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Periods.Add (new Period ());
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Periods = null;
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Timers.Add (new Timer ());
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Timers = null;
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
		}


	}
}
