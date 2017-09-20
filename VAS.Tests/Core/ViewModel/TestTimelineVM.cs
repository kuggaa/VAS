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
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Core.Store.Templates;
using VAS.Core.MVVMC;
using VAS.Core.Common;

namespace VAS.Tests.Core.ViewModel
{
	[TestFixture]
	public class TestTimelineVM
	{
		[Test]
		public void TestCreateEventTypesTimeline ()
		{
			Project project = Utils.CreateProject (true);
			ProjectVM projectVM = new DummyProjectVM { Model = project };
			TimelineVM viewModel = new TimelineVM ();

			viewModel.CreateEventTypeTimelines (projectVM.EventTypes);

			Assert.AreEqual (5, viewModel.EventTypesTimeline.ViewModels.Count);
		}

		[Test]
		public void TestSetModel ()
		{
			Project project = Utils.CreateProject (true);
			ProjectVM projectVM = new DummyProjectVM { Model = project };
			TimelineVM viewModel = new TimelineVM ();

			viewModel.CreateEventTypeTimelines (projectVM.EventTypes);
			viewModel.Model = project.Timeline;

			Assert.AreEqual (5, viewModel.EventTypesTimeline.ViewModels.Count);
			Assert.AreEqual (3, viewModel.FullTimeline.ViewModels.Count);
			Assert.AreEqual (1, viewModel.EventTypesTimeline.ViewModels [0].Count ());
			Assert.AreEqual (1, viewModel.EventTypesTimeline.ViewModels [1].Count ());
			Assert.AreEqual (1, viewModel.EventTypesTimeline.ViewModels [2].Count ());
		}

		[Test]
		public void TestTimelineEvent ()
		{
			Project project = Utils.CreateProject (true);
			ProjectVM projectVM = new DummyProjectVM { Model = project };
			TimelineVM viewModel = new TimelineVM ();
			viewModel.CreateEventTypeTimelines (projectVM.EventTypes);
			viewModel.Model = project.Timeline;
			EventType eventType = project.EventTypes [0];
			Assert.AreEqual (3, viewModel.FullTimeline.ViewModels.Count);
			Assert.AreEqual (1, viewModel.EventTypesTimeline.ViewModels.First (e => e.EventTypeVM.Model == eventType).Count ());

			project.AddEvent (eventType, new Time (0), new Time (10), new Time (5), null);

			Assert.AreEqual (4, viewModel.FullTimeline.ViewModels.Count);
			Assert.AreEqual (2, viewModel.EventTypesTimeline.ViewModels.First (e => e.EventTypeVM.Model == eventType).Count ());
		}

		[Test]
		public void TestAddFirstTimelineEventInEventType ()
		{
			Project project = Utils.CreateProject (true);
			ProjectVM projectVM = new DummyProjectVM { Model = project };
			TimelineVM viewModel = new TimelineVM ();
			viewModel.CreateEventTypeTimelines (projectVM.EventTypes);
			viewModel.Model = project.Timeline;
			EventType eventType = project.EventTypes [4];
			Assert.AreEqual (3, viewModel.FullTimeline.ViewModels.Count);
			Assert.AreEqual (0, viewModel.EventTypesTimeline.ViewModels.First (e => e.EventTypeVM.Model == eventType).Count ());

			project.AddEvent (eventType, new Time (0), new Time (10), new Time (5), null);

			Assert.AreEqual (4, viewModel.FullTimeline.ViewModels.Count);
			Assert.AreEqual (1, viewModel.EventTypesTimeline.ViewModels.First (e => e.EventTypeVM.Model == eventType).Count ());
		}

		[Test]
		public void TestAddTimelineEventWithEmptyEventTypes ()
		{
			Project project = Utils.CreateProject (true);
			TimelineVM viewModel = new TimelineVM ();

			viewModel.FullTimeline.ViewModels.Add (new TimelineEventVM { Model = project.Timeline [0] });

			Assert.AreEqual (project.Timeline [0], viewModel.FullTimeline.ViewModels [0].Model);
			Assert.IsTrue (viewModel.EventTypesTimeline.ViewModels.Count == 1);
		}

		[Test]
		public void TestClearTimeline ()
		{
			Project project = Utils.CreateProject (true);
			ProjectVM projectVM = new DummyProjectVM { Model = project };
			TimelineVM viewModel = new TimelineVM ();
			viewModel.CreateEventTypeTimelines (projectVM.EventTypes);
			viewModel.Model = project.Timeline;

			viewModel.Clear ();

			Assert.AreEqual (0, viewModel.EventTypesTimeline.ViewModels.Count);
			Assert.AreEqual (0, viewModel.FullTimeline.ViewModels.Count);
		}

		[Test]
		public void TestRenameEventType ()
		{
			Project project = Utils.CreateProject (true);
			ProjectVM projectVM = new DummyProjectVM { Model = project };
			TimelineVM viewModel = new TimelineVM ();
			viewModel.CreateEventTypeTimelines (projectVM.EventTypes);
			viewModel.Model = project.Timeline;
			string expected = "New Name";

			EventType et = viewModel.Model.FirstOrDefault ().EventType;
			et.Name = expected;
			viewModel.Model.Add (new TimelineEvent { EventType = et });

			Assert.AreEqual (5, viewModel.EventTypesTimeline.ViewModels.Count);
			Assert.AreEqual (expected, viewModel.EventTypesTimeline.ViewModels.FirstOrDefault ().Model.Name);
		}

		[Test]
		public void TestAddNewEvent_WithPlayerFromOneTeam ()
		{
			TeamVM homeTeamVM, awayTeamVM;
			TimelineVM timeline;

			CreateTimelineWithTeams (out timeline, out homeTeamVM, out awayTeamVM);
			PlayerVM player = homeTeamVM.ViewModels [0];
			AddTimelineEventWithPlayers (timeline, player);

			CheckPlayerEvents (timeline, player, 1);
		}

		[Test]
		public void TestAddNewEvent_WithPlayerFromBothTeams ()
		{
			TeamVM homeTeamVM, awayTeamVM;
			TimelineVM timeline;

			CreateTimelineWithTeams (out timeline, out homeTeamVM, out awayTeamVM);
			PlayerVM player1 = homeTeamVM.ViewModels [0];
			PlayerVM player2 = awayTeamVM.ViewModels [0];
			AddTimelineEventWithPlayers (timeline, player1, player2);

			CheckPlayerEvents (timeline, player1, 1);
			CheckPlayerEvents (timeline, player2, 1);
		}

		[Test]
		public void TestAddNewEvent_WithPlayersFromOneTeam ()
		{
			TeamVM homeTeamVM, awayTeamVM;
			TimelineVM timeline;

			CreateTimelineWithTeams (out timeline, out homeTeamVM, out awayTeamVM);

			PlayerVM player1 = homeTeamVM.ViewModels [0];
			PlayerVM player2 = homeTeamVM.ViewModels [1];
			AddTimelineEventWithPlayers (timeline, player1, player2);

			CheckPlayerEvents (timeline, player1, 1);
			CheckPlayerEvents (timeline, player2, 1);
		}

		[Test]
		public void TestRemoveEvent_WithPlayerFromOneTeam ()
		{
			TeamVM homeTeamVM, awayTeamVM;
			TimelineVM timeline;

			CreateTimelineWithTeams (out timeline, out homeTeamVM, out awayTeamVM);
			PlayerVM player1 = homeTeamVM.ViewModels [0];
			var timelineEvnt = AddTimelineEventWithPlayers (timeline, player1);

			timeline.Model.Remove (timelineEvnt);
			CheckPlayerEvents (timeline, player1, 0);
		}

		[Test]
		public void TestRemoveEvent_WithPlayerFromBothTeam ()
		{
			TeamVM homeTeamVM, awayTeamVM;
			TimelineVM timeline;

			CreateTimelineWithTeams (out timeline, out homeTeamVM, out awayTeamVM);
			PlayerVM player1 = homeTeamVM.ViewModels [0];
			PlayerVM player2 = awayTeamVM.ViewModels [0];
			var timelineEvnt = AddTimelineEventWithPlayers (timeline, player1, player2);

			timeline.Model.Remove (timelineEvnt);
			CheckPlayerEvents (timeline, player1, 0);
			CheckPlayerEvents (timeline, player2, 0);

		}

		[Test]
		public void TestRemovePlayerTag ()
		{
			TeamVM homeTeamVM, awayTeamVM;
			TimelineVM timeline;

			CreateTimelineWithTeams (out timeline, out homeTeamVM, out awayTeamVM);
			PlayerVM player1 = homeTeamVM.ViewModels [0];
			PlayerVM player2 = awayTeamVM.ViewModels [0];
			var timelineEvnt = AddTimelineEventWithPlayers (timeline, player1, player2);
			timelineEvnt.Players.Clear ();

			CheckPlayerEvents (timeline, player1, 0);
			CheckPlayerEvents (timeline, player2, 0);
		}

		[Test]
		public void TestAddPlayerTag ()
		{
			TeamVM homeTeamVM, awayTeamVM;
			TimelineVM timeline;

			CreateTimelineWithTeams (out timeline, out homeTeamVM, out awayTeamVM);
			PlayerVM player1 = homeTeamVM.ViewModels [0];
			PlayerVM player2 = awayTeamVM.ViewModels [0];
			EventType evt = new EventType { Name = "Tests" };
			TimelineEvent timelineEvnt = new TimelineEvent ();
			timelineEvnt.EventType = evt;
			timeline.Model.Add (timelineEvnt);
			timelineEvnt.Players.Add (player1.Model);
			timelineEvnt.Players.Add (player2.Model);

			CheckPlayerEvents (timeline, player1, 1);
			CheckPlayerEvents (timeline, player2, 1);
		}

		[Test]
		public void Timeline_SetLimitation_SetsInFullTimeline()
		{
			TimelineVM viewModel = new TimelineVM ();
			viewModel.Model = new RangeObservableCollection<TimelineEvent> ();
			var countLimitation = new CountLimitationVM ();
			var countLimitationChart = new CountLimitationBarChartVM {
				Limitation = countLimitation
			};

			viewModel.LimitationChart = countLimitationChart;

			Assert.AreSame (countLimitation, viewModel.FullTimeline.Limitation);
		}

		void CheckPlayerEvents (TimelineVM timeline, PlayerVM player, int count)
		{
			Assert.AreEqual (count, timeline.TeamsTimeline.ViewModels.SelectMany (p => p.ViewModels).
							 Where (p => p.Player == player).SelectMany (p => p.ViewModels).Count ());
		}

		TimelineEvent AddTimelineEventWithPlayers (TimelineVM timeline, params PlayerVM [] players)
		{
			EventType evt = new EventType { Name = "Tests" };
			TimelineEvent timelineEvnt = new TimelineEvent ();
			timelineEvnt.EventType = evt;
			foreach (PlayerVM p in players) {
				timelineEvnt.Players.Add (p.Model);
			}
			timeline.Model.Add (timelineEvnt);
			return timelineEvnt;
		}

		void CreateTimelineWithTeams (out TimelineVM timeline, out TeamVM homeTeamVM, out TeamVM awayTeamVM)
		{
			homeTeamVM = new TeamVM ();
			awayTeamVM = new TeamVM ();
			homeTeamVM.Model = new DummyTeam ();
			awayTeamVM.Model = new DummyTeam ();
			for (int i = 0; i < 5; i++) {
				homeTeamVM.Model.List.Add (new Utils.PlayerDummy { Name = $"Player{i}" });
				awayTeamVM.Model.List.Add (new Utils.PlayerDummy { Name = $"Player{i}" });
			}
			timeline = new TimelineVM ();
			timeline.Model = new RangeObservableCollection<TimelineEvent> ();
			timeline.CreateTeamsTimelines (new List<TeamVM> { homeTeamVM, awayTeamVM });
		}
	}
}
