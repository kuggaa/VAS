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
using System.Linq;
using NUnit.Framework;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Tests.Core.ViewModel
{
	[TestFixture]
	public class TestTimelineVM
	{
		[Test]
		public void TestCreateEventTypesTimeline ()
		{
			Project project = Utils.CreateProject (true);
			ProjectVM projectVM = new ProjectVM { Model = project };
			TimelineVM viewModel = new TimelineVM ();

			viewModel.CreateEventTypeTimelines (projectVM.EventTypes);

			Assert.AreEqual (5, viewModel.ViewModels.Count);
		}

		[Test]
		public void TestSetModel ()
		{
			Project project = Utils.CreateProject (true);
			ProjectVM projectVM = new ProjectVM { Model = project };
			TimelineVM viewModel = new TimelineVM ();

			viewModel.CreateEventTypeTimelines (projectVM.EventTypes);
			viewModel.Model = project.Timeline;

			Assert.AreEqual (5, viewModel.ViewModels.Count);
			Assert.AreEqual (3, viewModel.FullTimeline.ViewModels.Count);
			Assert.AreEqual (1, viewModel.ViewModels [0].Count ());
			Assert.AreEqual (1, viewModel.ViewModels [1].Count ());
			Assert.AreEqual (1, viewModel.ViewModels [2].Count ());
		}


		[Test]
		public void TestTimelineEvent ()
		{
			Project project = Utils.CreateProject (true);
			ProjectVM projectVM = new ProjectVM { Model = project };
			TimelineVM viewModel = new TimelineVM ();
			viewModel.CreateEventTypeTimelines (projectVM.EventTypes);
			viewModel.Model = project.Timeline;
			EventType eventType = project.EventTypes [0];
			Assert.AreEqual (3, viewModel.FullTimeline.ViewModels.Count);
			Assert.AreEqual (1, viewModel.ViewModels.First (e => e.EventTypeVM.Model == eventType).Count ());

			project.AddEvent (eventType, new Time (0), new Time (10), new Time (5), null);

			Assert.AreEqual (4, viewModel.FullTimeline.ViewModels.Count);
			Assert.AreEqual (2, viewModel.ViewModels.First (e => e.EventTypeVM.Model == eventType).Count ());
		}

		[Test]
		public void TestAddFirstTimelineEventInEventType ()
		{
			Project project = Utils.CreateProject (true);
			ProjectVM projectVM = new ProjectVM { Model = project };
			TimelineVM viewModel = new TimelineVM ();
			viewModel.CreateEventTypeTimelines (projectVM.EventTypes);
			viewModel.Model = project.Timeline;
			EventType eventType = project.EventTypes [4];
			Assert.AreEqual (3, viewModel.FullTimeline.ViewModels.Count);
			Assert.AreEqual (0, viewModel.ViewModels.First (e => e.EventTypeVM.Model == eventType).Count ());

			project.AddEvent (eventType, new Time (0), new Time (10), new Time (5), null);

			Assert.AreEqual (4, viewModel.FullTimeline.ViewModels.Count);
			Assert.AreEqual (1, viewModel.ViewModels.First (e => e.EventTypeVM.Model == eventType).Count ());
		}
	}
}
