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
using Timer = VAS.Core.Store.Timer;

namespace VAS.Tests.Core.Store
{
	[TestFixture ()]
	public class TestProject
	{
		[OneTimeSetUp]
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
		public void EventsGroupedByEventType_ProjectWithEventTypes_GroupedEventsCountOk ()
		{
			///Arrange

			var targetProject = Utils.CreateProject ();

			///Act

			var eventsGrouped = targetProject.EventsGroupedByEventType;

			///Assert

			Assert.AreEqual (3, eventsGrouped.Count ());
			Assert.AreEqual (targetProject.EventTypes [0], eventsGrouped.ElementAt (0).Key);
			Assert.AreEqual (targetProject.EventTypes [1], eventsGrouped.ElementAt (1).Key);
			Assert.AreEqual (targetProject.EventTypes [2], eventsGrouped.ElementAt (2).Key);
		}

		[Test ()]
		public void Dispose_FullProject_ProjectElementsCleared ()
		{
			//Arrange
			var projectTarget = Utils.CreateProject () as Utils.ProjectDummy;
			projectTarget.Periods.Add (new Period () {
				Name = "Test"
			});
			projectTarget.Playlists.Add (new Playlist () {
				Name = "Test"
			});


			//Act
			projectTarget.Dispose ();

			//Assert
			Assert.NotNull (projectTarget);
			//FIXME: This collection doesnt clear its children --> Assert.IsTrue (!projectTarget.FileSet.Any ());
			//FIXME: This collection doesnt clear its children --> Assert.IsTrue (!projectTarget.Timeline.Any ());
			Assert.IsTrue (!projectTarget.Timers.Any ());
			Assert.IsTrue (!projectTarget.Periods.Any ());
			Assert.IsTrue (!projectTarget.Playlists.Any ());
			Assert.IsTrue (!projectTarget.EventTypes.Any ());

		}

		[Test ()]

		public void AddEvent_NewProjectWithoutEvent_ProjectWithEvent ()
		{
			//Arrange

			var targetProject = Utils.CreateProject (withEvents: false) as Utils.ProjectDummy;
			TimelineEvent timeLineEvent = targetProject.CreateEvent (targetProject.EventTypes.FirstOrDefault (), new Time (1000), new Time (2000),
																	 null, null, 0);

			//Act

			targetProject.AddEvent (timeLineEvent);

			//Assert

			Assert.AreEqual (targetProject, timeLineEvent.Project);
			Assert.AreEqual (targetProject.Timeline.Count, 1);

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
		public void CleanupTimers_ProjectWithValidAndInvalidTimers_ProjectWithoutInvalidTimers ()
		{
			///Arrange

			var target = Utils.CreateProject ();
			target.Timers.Clear ();

			#region Valid Instances

			var validTimeNode = new TimeNode () {
				Start = new Time (1000),
				Stop = new Time (2000)
			};

			var validTimeNode2 = new TimeNode () {
				Start = new Time (),
				Stop = new Time (1000)
			};

			var validTimeNode3 = new TimeNode () {
				Start = new Time (1000),
				Stop = new Time ()
			};

			var validTimer = new Timer () {
				Nodes = new RangeObservableCollection<TimeNode> ()
				{
					validTimeNode,
					validTimeNode2,
					validTimeNode3
				}
			};
			#endregion

			#region Invalid Instances
			var invalidTimeNode = new TimeNode () {
				Start = null,
				Stop = new Time (2000)
			};

			var invalidTimeNode2 = new TimeNode () {
				Start = new Time (1000),
				Stop = null
			};

			var invalidTimeNode3 = new TimeNode () {
				Start = null,
				Stop = null
			};

			var invalidTimer = new Timer () {
				Nodes = new RangeObservableCollection<TimeNode> ()
				{
					invalidTimeNode
				}
			};

			var invalidTimer2 = new Timer () {
				Nodes = new RangeObservableCollection<TimeNode> ()
				{
					validTimeNode,
					invalidTimeNode2,
					validTimeNode3
				}
			};

			var invalidTimer3 = new Timer () {
				Nodes = new RangeObservableCollection<TimeNode> ()
				{
					invalidTimeNode,
					invalidTimeNode2,
					invalidTimeNode3
				}
			};

			#endregion


			target.Timers.Add (validTimer);
			target.Timers.Add (invalidTimer);
			target.Timers.Add (invalidTimer2);
			target.Timers.Add (invalidTimer3);

			///Act

			target.CleanupTimers ();

			///Assert

			Assert.IsNotNull (target);
			Assert.AreEqual (4, target.Timers.Count ());
			Assert.AreEqual (3, target.Timers [0].Nodes.Count ());
			Assert.AreEqual (0, target.Timers [1].Nodes.Count ());
			Assert.AreEqual (2, target.Timers [2].Nodes.Count ());
			Assert.AreEqual (0, target.Timers [3].Nodes.Count ());
		}

		[Test ()]
		public void UpdateEventTypesAndTimers_ProjectWithEventTypesAndTimersUpdated_ProjectWithoutCategoryButtonUpdated ()
		{
			///Arrange

			var targetProject = new Utils.ProjectDummy ();
			targetProject.UpdateEventTypesAndTimers ();

			///Act

			targetProject.Dashboard.List.Remove (targetProject.Dashboard.List.OfType<AnalysisEventButton> ().First ());
			targetProject.UpdateEventTypesAndTimers ();

			///Assert

			Assert.IsNotNull (targetProject);
			Assert.AreEqual (1, targetProject.Timers.Count);
			Assert.AreEqual (4, targetProject.EventTypes.Count);
		}

		[Test ()]
		public void UpdateEventTypesAndTimers_ProjectWithEventTypesAndTimersUpdated_ProjectWithoutCategoryButtonWithEventsInTimeLine ()
		{
			///Arrange

			var targetProject = new Utils.ProjectDummy ();
			targetProject.Timers.Clear ();
			targetProject.EventTypes.Clear ();
			targetProject.UpdateEventTypesAndTimers ();

			///Act

			AnalysisEventButton button = targetProject.Dashboard.List.OfType<AnalysisEventButton> ().First ();
			targetProject.Timeline.Add (new TimelineEvent { EventType = button.EventType });
			targetProject.Dashboard.List.Remove (button);
			targetProject.UpdateEventTypesAndTimers ();

			///Assert

			Assert.IsNotNull (targetProject);
			Assert.AreEqual (1, targetProject.Timers.Count);
			Assert.AreEqual (5, targetProject.EventTypes.Count);

		}

		[Test ()]
		public void UpdateEventTypesAndTimers_NewProjectWithEventTypesAndTimers_ProjectWithoutEventInDashboardAndTimeline ()
		{
			///Arrange

			var targetProject = new Utils.ProjectDummy ();
			targetProject.Timers.Clear ();
			targetProject.EventTypes.Clear ();
			AnalysisEventButton button = targetProject.Dashboard.List.OfType<AnalysisEventButton> ().First ();
			targetProject.Timeline.Add (new TimelineEvent { EventType = button.EventType });
			targetProject.UpdateEventTypesAndTimers ();

			///Act

			targetProject.Dashboard.List.Remove (button);
			targetProject.Timeline.Clear ();
			targetProject.UpdateEventTypesAndTimers ();

			///Assert

			Assert.IsNotNull (targetProject);
			Assert.AreEqual (1, targetProject.Timers.Count);
			Assert.AreEqual (4, targetProject.EventTypes.Count);
		}

		[Test ()]
		public void UpdateEventTypesAndTimers_NewProjectWithoutEvenTypesAndTimers_ProjectWithEventTypesAndTimersUpdated ()
		{
			///Arrange

			var targetProject = new Utils.ProjectDummy ();
			targetProject.Timers.Clear ();
			targetProject.EventTypes.Clear ();

			///Act

			targetProject.UpdateEventTypesAndTimers ();

			///Assert

			Assert.AreEqual (1, targetProject.Timers.Count);
			Assert.AreEqual (5, targetProject.EventTypes.Count);
		}


		[Test ()]
		public void EventsByType_NewProjectWithTypedEvents_EventTypesCountOk ()
		{
			///Arrange

			var targetProject = Utils.CreateProject ();

			///Act

			var eventsByTypeFirst = targetProject.EventsByType (targetProject.EventTypes [0]);
			var eventsByTypeSecond = targetProject.EventsByType (targetProject.EventTypes [1]);
			var eventsByTypeThird = targetProject.EventsByType (targetProject.EventTypes [2]);
			var eventsByTypeFourth = targetProject.EventsByType (targetProject.EventTypes [3]);
			var eventsByTypeFifth = targetProject.EventsByType (targetProject.EventTypes [4]);

			///Assert

			Assert.AreEqual (1, eventsByTypeFirst.Count ());
			Assert.AreEqual (1, eventsByTypeSecond.Count ());
			Assert.AreEqual (1, eventsByTypeThird.Count ());
			Assert.AreEqual (0, eventsByTypeFourth.Count ());
			Assert.AreEqual (0, eventsByTypeFifth.Count ());
		}

		[Test ()]
		[Ignore ("Not implemented")]
		public void TestConsolidateDescription ()
		{
		}

		[Test ()]
		public void Equals_NewProject_SerializedAndDeserializedProjectAreEqual ()
		{
			///Arrange
			/// 
			var targetProject = Utils.CreateProject () as Utils.ProjectDummy; ;
			var anotherProject = new Utils.ProjectDummy ();

			///Act

			var serializedAndDeserializedProject = Utils.SerializeDeserialize (targetProject);

			///Assert

			Assert.IsTrue (targetProject.Equals (serializedAndDeserializedProject));
			Assert.IsFalse (targetProject.Equals (anotherProject));
			Assert.IsFalse (serializedAndDeserializedProject.Equals (anotherProject));
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
			p.FileSet = new MediaFileSet { new MediaFile { FilePath = originalPath } };
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
			period.Nodes.Add (new TimeNode {
				Start = new Time (0),
				Stop = new Time (3000)
			});
			p.Periods.Add (period);
			period = new Period ();
			period.Nodes.Add (new TimeNode {
				Start = new Time (3001),
				Stop = new Time (6000)
			});
			p.Periods.Add (period);
			period = new Period ();
			period.Nodes.Add (new TimeNode {
				Start = new Time (6001),
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
			p.EventTypes.Add (new EventType ());
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.EventTypes = null;
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Playlists.Add (new Playlist ());
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Periods.Add (new Period ());
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Timers.Add (new Timer ());
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
		}

		[Test ()]
		public void TestWhenMediaFileIsChangedThenEmitsChanged ()
		{
			// Arrange
			int eventCount = 0;

			MediaFile originalMediaFileSet = new MediaFile { FilePath = Path.GetRandomFileName () };
			MediaFile newMediaFileSet = new MediaFile { FilePath = Path.GetRandomFileName () };

			Project p = CreateProject ();
			p.FileSet = new MediaFileSet { originalMediaFileSet };
			p.IsChanged = false;
			p.PropertyChanged += (sender, e) => eventCount++;

			// Act
			p.FileSet.Replace (originalMediaFileSet, newMediaFileSet);

			// Assert
			Assert.AreEqual (1, eventCount);
			Assert.IsTrue (p.IsChanged);
		}
	}
}
