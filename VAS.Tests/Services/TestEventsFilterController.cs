//
//  Copyright (C) 2017 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Filters;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Templates;
using VAS.Core.ViewModel;
using VAS.Services.Controller;
using Predicate = VAS.Core.Filters.Predicate<VAS.Core.ViewModel.TimelineEventVM>;
using Timer = VAS.Core.Store.Timer;

namespace VAS.Tests.Services
{
	[TestFixture]
	public class TestEventsFilterController
	{
		TimelineVM timelineVM;
		EventsFilterController eventsFilterController;

		[OneTimeSetUp]
		public void OneTimeSetUp ()
		{
			SetupClass.SetUp ();
			Mock<IGUIToolkit> mockGuiToolkit = new Mock<IGUIToolkit> ();
			mockGuiToolkit.Setup (m => m.DeviceScaleFactor).Returns (1);
			App.Current.GUIToolkit = mockGuiToolkit.Object;
			eventsFilterController = new Utils.DummyEventsFilterController ();
		}

		[SetUp]
		public async Task SetUp ()
		{
			timelineVM = new TimelineVM ();

			var dealer = new DummyProjectDealer ();
			var projectvm = new DummyProjectVM { Model = EventsFilterUtils.CreateProject () };
			timelineVM.CreateEventTypeTimelines (projectvm.EventTypes);

			dealer.Project = projectvm;
			dealer.Timeline = timelineVM;

			eventsFilterController.SetViewModel (dealer);

			CreateEvents ();

			await eventsFilterController.Start ();
		}

		[TearDown]
		public async Task TearDown ()
		{
			try {
				await eventsFilterController.Stop ();
				eventsFilterController.ViewModel = null;
			} catch (InvalidOperationException) {
				// Ignore the already stopped error
			}
		}

		[Test]
		public async Task SetViewModel_EmptyFilters_EventsFiltersFilled ()
		{
			// Arrange
			try {
				await eventsFilterController.Stop ();
			} catch (InvalidOperationException) {
				// Ignore the already stopped error
			}

			// Act
			await eventsFilterController.Start ();

			// Assert
			// We have the default dashboard, with a subcategory "Position" added to the first event type, with values "Left", "Center" and "Right"
			// The ParentEventsPredicate filter should contain:
			//   - One OrPredicate for Periods, with:
			//     - One Predicate for each period
			//   - One OrPredicate for Timers, with:
			//     - One Predicate for each timer
			//   - One OrPredicate for Common tags, with:
			//     - One OrPredicate for each tag group, with:
			//       - One Predicate for each tag
			//   - One OrPredicate for EventTypes, with:
			//     - One AndPredicate for each EventType with subcategories
			//     - One Predicate for each EventType without subcategories
			Assert.AreEqual (2, timelineVM.Filters.Count);
			Assert.AreEqual (timelineVM.EventsPredicate, timelineVM.Filters.Elements [0]);
			Assert.AreEqual (4, timelineVM.EventsPredicate.Count);

			Assert.AreEqual (3, timelineVM.PeriodsPredicate.Count);
			Assert.IsTrue (timelineVM.PeriodsPredicate.All (p => p is Predicate));

			Assert.AreEqual (3, timelineVM.TimersPredicate.Count);
			Assert.IsTrue (timelineVM.TimersPredicate.All (p => p is Predicate));

			Assert.AreEqual (2, timelineVM.CommonTagsPredicate.Count);
			Assert.IsTrue (timelineVM.CommonTagsPredicate.All (p => p is OrPredicate<TimelineEventVM>));
			Assert.AreEqual (3, ((OrPredicate<TimelineEventVM>)timelineVM.CommonTagsPredicate.First ()).Elements.Count);
			Assert.IsTrue (((OrPredicate<TimelineEventVM>)timelineVM.CommonTagsPredicate.First ()).All (p => p is Predicate));
			Assert.AreEqual (2, ((OrPredicate<TimelineEventVM>)timelineVM.CommonTagsPredicate.ElementAt (1)).Elements.Count);

			Assert.AreEqual (10, timelineVM.EventTypesPredicate.Elements.OfType<OrPredicate<TimelineEventVM>> ().Count ());
			Assert.AreEqual (1, timelineVM.EventTypesPredicate.Elements.OfType<Predicate> ().Count ());
		}

		[Test]
		public void ApplyFilter_EventTypeOne_VisiblePropertyChangedEmmittedOnlyForEventsChangingVisibility ()
		{
			int changedCount = 0;
			var changed = new HashSet<TimelineEventVM> ();

			// Arrange
			timelineVM.Filters.Active = false;
			foreach (var evt in timelineVM.FullTimeline.ViewModels) {
				evt.PropertyChanged += (sender, e) => {
					changedCount++;
					changed.Add (evt);
				};
			}

			// Act
			timelineVM.EventTypesPredicate.Elements [0].Active = true;

			// Assert
			Assert.AreEqual (timelineVM.FullTimeline.Count (e => !e.Visible), changedCount);
		}


		[Test]
		public void ApplyFilter_AllFiltersActive_AllVisible ()
		{
			// Arrange

			// Act
			timelineVM.Filters.Active = true;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (e => e.Visible));
		}

		[Test]
		public void ApplyFilter_NoFiltersActive_AllVisible ()
		{
			// Arrange

			// Act
			timelineVM.Filters.Active = false;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (e => e.Visible));
		}

		[Test]
		public void ApplyFilter_SubcatAllFiltersActive_AllVisible ()
		{
			// Arrange

			// Act
			timelineVM.Filters.Active = true;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (e => e.Visible));
		}

		[Test]
		public void ApplyFilter_SubcatNoFiltersActive_AllVisible ()
		{
			// Arrange

			// Act
			timelineVM.Filters.Active = false;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (e => e.Visible));
		}

		[Test]
		public void ApplyFilter_EventTypeOne_FourVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.EventTypesPredicate.Elements [0].Active = true;

			// Assert
			Assert.AreEqual (4, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (0).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
		}

		[Test]
		public void ApplyFilter_EventTypeOneAndTwo_FiveVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.EventTypesPredicate.Elements [0].Active = true;
			timelineVM.EventTypesPredicate.Elements [1].Active = true;

			// Assert
			Assert.AreEqual (5, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (0).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (4).Visible);
		}

		[Test]
		public void ApplyFilter_EventTypeOneNoTags_OneVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			CompositePredicate<TimelineEventVM> firstEventTypePredicate = ((CompositePredicate<TimelineEventVM>)timelineVM.EventTypesPredicate.Elements [0]);

			// Act
			firstEventTypePredicate.Elements [5].Active = true;

			// Assert
			Assert.AreEqual (1, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (0).Visible);
		}

		[Test]
		public void ApplyFilter_EventTypeOneBad_TwoVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			CompositePredicate<TimelineEventVM> firstEventTypePredicate = ((CompositePredicate<TimelineEventVM>)timelineVM.EventTypesPredicate.Elements [0]);

			// Act
			firstEventTypePredicate.Elements [1].Active = true;

			// Assert
			Assert.AreEqual (2, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
		}

		[Test]
		public void ApplyFilter_EventTypeOneGoodBad_ThreeVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			CompositePredicate<TimelineEventVM> firstEventTypePredicate = ((CompositePredicate<TimelineEventVM>)timelineVM.EventTypesPredicate.Elements [0]);

			// Act
			firstEventTypePredicate.Elements [0].Active = true;
			firstEventTypePredicate.Elements [1].Active = true;

			// Assert
			Assert.AreEqual (3, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
		}

		[Test]
		public void ApplyFilter_EventTypeTwoBad_NoneVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			CompositePredicate<TimelineEventVM> secondEventTypePredicate = ((CompositePredicate<TimelineEventVM>)timelineVM.EventTypesPredicate.Elements [1]);

			// Act
			secondEventTypePredicate.Elements [1].Active = true;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (e => !e.Visible));
		}

		[Test]
		public void ApplyFilter_EventTypeOneGoodBadLeft_ThreeVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			CompositePredicate<TimelineEventVM> firstEventTypePredicate = ((CompositePredicate<TimelineEventVM>)timelineVM.EventTypesPredicate.Elements [0]);

			// Act
			firstEventTypePredicate.Elements [0].Active = true;
			firstEventTypePredicate.Elements [1].Active = true;
			firstEventTypePredicate.Elements [2].Active = true;

			// Assert
			Assert.AreEqual (3, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
		}

		[Test]
		public void ApplyFilter_EventTypeTwoAndThree_TwoVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.EventTypesPredicate.Elements [1].Active = true;
			timelineVM.EventTypesPredicate.Elements [2].Active = true;

			// Assert
			Assert.AreEqual (2, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (4).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (5).Visible);
		}

		[Test]
		public void ApplyFilter_EventTypeThreeOtherTag_OneVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;
			CompositePredicate<TimelineEventVM> fifttEventTypePredicate = ((CompositePredicate<TimelineEventVM>)timelineVM.EventTypesPredicate.Elements [4]);

			// Act
			fifttEventTypePredicate.Elements [2].Active = true;

			// Assert
			Assert.AreEqual (1, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (7).Visible);
		}

		[Test]
		public void ApplyPeriodsFilter_NoPeriods_OneVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.PeriodsPredicate.Elements [0].Active = true;

			// Assert
			Assert.AreEqual (1, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (5).Visible);
		}

		[Test]
		public void ApplyPeriodsFilter_FirstPeriod_FourVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.PeriodsPredicate.Elements [1].Active = true;

			// Assert
			Assert.AreEqual (5, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (0).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (6).Visible);
		}

		[Test]
		public void ApplyPeriodsFilter_SecondPeriod_FourVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.PeriodsPredicate.Elements [2].Active = true;

			// Assert
			Assert.AreEqual (4, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (4).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (6).Visible);
		}

		[Test]
		public void ApplyPeriodsFilter_FirstAndSecondPeriod_SixVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.PeriodsPredicate.Elements [1].Active = true;
			timelineVM.PeriodsPredicate.Elements [2].Active = true;

			// Assert
			Assert.AreEqual (7, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (0).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (4).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (6).Visible);
		}

		[Test]
		public void ApplyPeriodsFilter_AllPeriods_AllVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.PeriodsPredicate.Active = true;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (ev => ev.Visible));
		}

		[Test]
		public void ApplyPeriodsFilter_NoPeriodActive_AllVisible ()
		{
			// Arrange

			// Act
			timelineVM.Filters.Active = false;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (ev => ev.Visible));
		}

		[Test]
		public void ApplyTimersFilter_NoTimers_TwoVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.TimersPredicate.Elements [0].Active = true;

			// Assert
			Assert.AreEqual (2, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (5).Visible);
		}

		[Test]
		public void ApplyTimersFilter_FirstTimer_FourVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.TimersPredicate.Elements [1].Active = true;

			// Assert
			Assert.AreEqual (5, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (0).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (4).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (6).Visible);
		}

		[Test]
		public void ApplyTimersFilter_SecondTimer_FourVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.TimersPredicate.Elements [2].Active = true;

			// Assert
			Assert.AreEqual (5, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (0).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (6).Visible);
		}

		[Test]
		public void ApplyTimersFilter_FirstAndSecondTimer_FiveVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.TimersPredicate.Elements [1].Active = true;
			timelineVM.TimersPredicate.Elements [2].Active = true;

			// Assert
			Assert.AreEqual (6, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (0).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (4).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (6).Visible);
		}

		[Test]
		public void ApplyTimersFilter_AllTimers_AllVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.TimersPredicate.Active = true;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (ev => ev.Visible));
		}

		[Test]
		public void ApplyTimersFilter_NoTimerActive_AllVisible ()
		{
			// Arrange

			// Act
			timelineVM.Filters.Active = false;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (ev => ev.Visible));
		}

		[Test]
		public void ApplyCommonTagsFilter_AllCommonTags_AllVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.CommonTagsPredicate.Active = true;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (ev => ev.Visible));
		}

		[Test]
		public void ApplyCommonTagsFilter_GeneralTags_AllVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.CommonTagsPredicate.Elements [0].Active = true;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (ev => ev.Visible));
		}

		[Test]
		public void ApplyCommonTagsFilter_NoneGeneralTags_FourVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			((CompositePredicate<TimelineEventVM>)timelineVM.CommonTagsPredicate.Elements [0]).Elements [0].Active = true;

			// Assert
			Assert.AreEqual (5, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (0).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (4).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (5).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (6).Visible);
		}

		[Test]
		public void ApplyCommonTagsFilter_AttackTag_TwoVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			((CompositePredicate<TimelineEventVM>)timelineVM.CommonTagsPredicate.Elements [0]).Elements [1].Active = true;

			// Assert
			Assert.AreEqual (2, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
		}

		[Test]
		public void ApplyCommonTagsFilter_DefenseTag_TwoVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			((CompositePredicate<TimelineEventVM>)timelineVM.CommonTagsPredicate.Elements [0]).Elements [2].Active = true;

			// Assert
			Assert.AreEqual (2, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
		}

		[Test]
		public void ApplyCommonTagsFilter_AttackDefenseTag_ThreeVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			((CompositePredicate<TimelineEventVM>)timelineVM.CommonTagsPredicate.Elements [0]).Elements [1].Active = true;
			((CompositePredicate<TimelineEventVM>)timelineVM.CommonTagsPredicate.Elements [0]).Elements [2].Active = true;

			// Assert
			Assert.AreEqual (3, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
		}

		[Test]
		public void ApplyCommonTagsFilter_OtherTag_Thing ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			((CompositePredicate<TimelineEventVM>)timelineVM.CommonTagsPredicate.Elements [1]).Elements [1].Active = true;

			// Assert
			Assert.AreEqual (2, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (4).Visible);
		}

		void CreateEvents ()
		{
			AnalysisEventType firstEventType = ((AnalysisEventType)eventsFilterController.Project.Dashboard.ViewModels.OfType<EventButtonVM> ().First ().EventType.Model);
			AnalysisEventType secondEventType = ((AnalysisEventType)eventsFilterController.Project.Dashboard.ViewModels.OfType<EventButtonVM> ().ElementAt (1).EventType.Model);
			AnalysisEventType thirdEventType = ((AnalysisEventType)eventsFilterController.Project.Dashboard.ViewModels.OfType<EventButtonVM> ().ElementAt (2).EventType.Model);
			AnalysisEventType fourthEventType = ((AnalysisEventType)eventsFilterController.Project.Dashboard.ViewModels.OfType<EventButtonVM> ().ElementAt (3).EventType.Model);
			AnalysisEventType fiftEventType = ((AnalysisEventType)eventsFilterController.Project.Dashboard.ViewModels.OfType<EventButtonVM> ().ElementAt (4).EventType.Model);

			Dictionary<string, List<Tag>> commonTagsByGroup = eventsFilterController.Project.Dashboard.Model.CommonTagsByGroup;

			Tag goodTag = firstEventType.TagsByGroup ["Outcome"].ElementAt (0);
			Tag badTag = firstEventType.TagsByGroup ["Outcome"].ElementAt (1);
			Tag leftTag = new Tag ("Left", "Position");
			Tag centerTag = new Tag ("Center", "Position");
			Tag rightTag = new Tag ("Right", "Position");
			firstEventType.Tags.Add (leftTag);
			firstEventType.Tags.Add (centerTag);
			firstEventType.Tags.Add (rightTag);

			Tag otherTag = new Tag ("tag value", "Other group");
			eventsFilterController.Project.Dashboard.SubViewModel.Model.Add (new TagButton {
				Name = "Other tag",
				Tag = otherTag,
			});

			eventsFilterController.Project.Timers.First ().Model.Nodes.Add (new TimeNode {
				Start = new Time (0),
				Stop = new Time (10),
			});
			eventsFilterController.Project.Timers.First ().Model.Nodes.Add (new TimeNode {
				Start = new Time (31),
				Stop = new Time (60),
			});
			eventsFilterController.Project.Timers.First ().Model.Nodes.Add (new TimeNode {
				Start = new Time (80),
				Stop = new Time (100),
			});

			eventsFilterController.Project.Timers.Model.Add (new VAS.Core.Store.Timer {
				Name = "timer 2",
				Nodes = new RangeObservableCollection<TimeNode> {
					new TimeNode {
						Start = new Time (0),
						Stop = new Time (50),
					},
				}
			});

			timelineVM.FullTimeline.Model.Replace (new List<TimelineEvent> {
				new TimelineEvent {
					Name = "event 1, type 1, without player tags, first period left",
					EventType = firstEventType,
					Start = new Time (0), // it starts outside the period
					Stop = new Time (20), // it ends inside the period
				},
				new TimelineEvent {
					Name = "event 2, type 1 + Bad + Attack, first team tagged, first period",
					EventType = firstEventType,
					Start = new Time (11), // it starts inside the first period, but not the first timer
					Stop = new Time (30), // it ends inside the first period, but not in the second timer
				},
				new TimelineEvent {
					Name = "event 3, type 1 + Good + Defense, second team tagged, between first and second period",
					EventType = firstEventType,
					Start = new Time (40), // it starts inside the 1st period
					Stop = new Time (60), // it ends inside the 2nd period
				},
				new TimelineEvent {
					Name = "event 4, type 1 + Bad + Left + Attack + Defense, first player from first team tagged, second period",
					EventType = firstEventType,
					Start = new Time (60), // it starts inside the 2nd period
					Stop = new Time (80), // it ends inside the 2nd period
				},
				new TimelineEvent {
					Name = "event 5, type 2 + Good + Other Tag, first player from second team tagged, second period right",
					EventType = secondEventType,
					Start = new Time (80), // it starts inside the 2nd period
					Stop = new Time (100), // it ends outside the 2nd period
				},
				new TimelineEvent {
					Name = "event 6, type 3 + Bad, first team and first player from first team tagged, no periods",
					EventType = thirdEventType,
					Start = new Time (900), // It starts outside all periods
					Stop = new Time (1000), // It ends outside all periods, without touching them
				},
				new TimelineEvent {
					Name = "event 7, first team and first player from second team tagged, fill all periods",
					EventType = fourthEventType,
					Start = new Time (0), // It starts outside all periods
					Stop = new Time (100), // It ends outside all periods, touching them all
				},
				new TimelineEvent {
					Name = "event 8, type 3 + Other Tag, without player tags, first period left",
					EventType = fiftEventType,
					Start = new Time (0), // it starts outside the period
					Stop = new Time (20), // it ends inside the period
				}
			});
			// Subcats & Tags
			timelineVM.FullTimeline.ElementAt (1).Model.Tags.Add (badTag);
			timelineVM.FullTimeline.ElementAt (1).Model.Tags.Add (commonTagsByGroup [""].First ());
			timelineVM.FullTimeline.ElementAt (2).Model.Tags.Add (goodTag);
			timelineVM.FullTimeline.ElementAt (2).Model.Tags.Add (commonTagsByGroup [""].ElementAt (1));
			timelineVM.FullTimeline.ElementAt (3).Model.Tags.Add (badTag);
			timelineVM.FullTimeline.ElementAt (3).Model.Tags.Add (leftTag);
			timelineVM.FullTimeline.ElementAt (3).Model.Tags.Add (commonTagsByGroup [""].First ());
			timelineVM.FullTimeline.ElementAt (3).Model.Tags.Add (commonTagsByGroup [""].ElementAt (1));
			timelineVM.FullTimeline.ElementAt (4).Model.Tags.Add (goodTag);
			timelineVM.FullTimeline.ElementAt (4).Model.Tags.Add (otherTag);
			timelineVM.FullTimeline.ElementAt (5).Model.Tags.Add (badTag);
			timelineVM.FullTimeline.ElementAt (7).Model.Tags.Add (otherTag);
		}
	}

	public class DummyProjectDealer : IViewModel, IProjectDealer, ITimelineDealer
	{
		public ProjectVM Project {
			get;
			set;
		}

		public TimelineVM Timeline {
			get;
			set;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void Dispose ()
		{
		}
	}

	public static class EventsFilterUtils
	{
		public static Project CreateProject (bool withEvents = true)
		{
			TimelineEvent pl;
			Project p = new ProjectDummy ();
			p.Dashboard = DashboardDummy.Default ();
			p.FileSet = new MediaFileSet ();
			p.FileSet.Add (new MediaFile (Path.GetTempFileName (), 34000, 25, true, true, "mp4", "h264",
				"aac", 320, 240, 1.3, null, "Test asset 1"));
			p.FileSet.Add (new MediaFile (Path.GetTempFileName (), 34000, 25, true, true, "mp4", "h264",
				"aac", 320, 240, 1.3, null, "Test asset 2"));
			p.Periods.Replace (new RangeObservableCollection<Period> {
				new Period {
					Name = "First Period",
					Nodes = new RangeObservableCollection<TimeNode>{
						new TimeNode {
							Start = new Time (10),
							Stop = new Time (50)
						}
					}
				},
				new Period {
					Name = "Second Period",
					Nodes = new RangeObservableCollection<TimeNode>{
						new TimeNode {
							Start = new Time (50),
							Stop = new Time (90)
						}
					}
				},
			});
			p.UpdateEventTypesAndTimers ();
			p.IsLoaded = true;
			if (withEvents) {
				AnalysisEventButton b = p.Dashboard.List [0] as AnalysisEventButton;

				/* No tags, no players */
				pl = new TimelineEvent {
					EventType = b.EventType,
					Start = new Time (0),
					Stop = new Time (50),
					FileSet = p.FileSet
				};
				p.Timeline.Add (pl);
				/* tags, but no players */
				b = p.Dashboard.List [1] as AnalysisEventButton;
				pl = new TimelineEvent {
					EventType = b.EventType,
					Start = new Time (20),
					Stop = new Time (60),
					FileSet = p.FileSet
				};
				pl.Tags.Add (b.AnalysisEventType.Tags [0]);
				p.Timeline.Add (pl);
				/* tags and players */
				b = p.Dashboard.List [2] as AnalysisEventButton;
				pl = new TimelineEvent {
					EventType = b.EventType,
					Start = new Time (70),
					Stop = new Time (100),
					FileSet = p.FileSet
				};
				pl.Tags.Add (b.AnalysisEventType.Tags [1]);
				p.Timeline.Add (pl);
			}

			return p;
		}

		public class ProjectDummy : Project
		{
			#region implemented abstract members of Project
			public ProjectDummy ()
			{
				FileSet = new MediaFileSet ();
			}

			public override TimelineEvent CreateEvent (EventType type, Time start, Time stop, Time eventTime,
													   Image miniature, int index)
			{
				TimelineEvent evt;
				string count;
				string name;

				count = String.Format ("{0:000}", EventsByType (type).Count + 1);
				name = String.Format ("{0} {1}", type.Name, count);
				evt = new TimelineEvent ();

				evt.Name = name;
				evt.Start = start;
				evt.Stop = stop;
				evt.EventTime = eventTime;
				evt.EventType = type;
				evt.Notes = "";
				evt.Miniature = miniature;
				evt.CamerasConfig = new RangeObservableCollection<CameraConfig> { new CameraConfig (0) };
				evt.FileSet = FileSet;
				evt.Project = this;

				return evt;
			}

			public override void AddEvent (TimelineEvent play)
			{
				play.FileSet = FileSet;
				play.Project = this;
				Timeline.Add (play);

			}

			#endregion
		}

		public class DashboardDummy : Dashboard
		{
			//dummy class for abstract validation. Copied from LongoMatch and adapted to VAS.
			public static DashboardDummy Default ()
			{
				var dashboard = new DashboardDummy ();
				TagButton tagbutton;
				TimerButton timerButton;

				// Create 10 buttons
				dashboard.FillDefaultTemplate (10);
				// And create an extra one without tags
				dashboard.FillDefaultTemplate (1);
				((AnalysisEventButton)dashboard.List.Last ()).AnalysisEventType.Tags.Clear ();
				dashboard.GamePeriods = new RangeObservableCollection<string> { "1", "2" };

				tagbutton = new TagButton {
					Tag = new Tag (Catalog.GetString ("Attack"), ""),
					Position = new Point (10, 10)
				};
				dashboard.List.Add (tagbutton);

				tagbutton = new TagButton {
					Tag = new Tag (Catalog.GetString ("Defense"), ""),
					Position = new Point (10 + (10 + CAT_WIDTH) * 1, 10)
				};
				dashboard.List.Add (tagbutton);

				timerButton = new TimerButton {
					Timer = new Timer { Name = Catalog.GetString ("Ball playing") },
					Position = new Point (10 + (10 + CAT_WIDTH) * 6, 10)
				};
				dashboard.List.Add (timerButton);
				return dashboard;
			}

			public void InsertTimer ()
			{
				var timerButton = new TimerButton {
					Timer = new Timer { Name = "Ball playing" },
					Position = new Point (10 + (10 + CAT_WIDTH) * 6, 10)
				};
				List.Add (timerButton);
			}


			public override AnalysisEventButton CreateDefaultItem (int index)
			{
				AnalysisEventButton button;
				AnalysisEventType evtype;
				Color c = StyleConf.ButtonEventColor;
				HotKey h = new HotKey ();

				evtype = new AnalysisEventType {
					Name = "Event Type " + index,
					SortMethod = SortMethodType.SortByStartTime,
					Color = c
				};
				AddDefaultTags (evtype);

				button = new AnalysisEventButton {
					EventType = evtype,
					Start = new Time { TotalSeconds = 10 },
					Stop = new Time { TotalSeconds = 10 },
					HotKey = h,
					/* Leave the first row for the timers and score */
					Position = new Point (10 + (index % 7) * (CAT_WIDTH + 10),
						10 + (index / 7 + 1) * (CAT_HEIGHT + 10)),
					Width = CAT_WIDTH,
					Height = CAT_HEIGHT,
					ShowIcon = true,
				};
				return button;
			}
		}
	}
}
