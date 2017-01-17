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
using Moq;
using NUnit.Framework;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Drawing.CanvasObjects.Timeline;

namespace VAS.Tests.Drawing.CanvasObjects.Timeline
{
	[TestFixture]
	public class TestEventTypeTimelineView
	{
		Mock<IWidget> widgetMock;
		Project project;
		EventTypeTimelineView timeline;


		[TestFixtureSetUp]
		public void Init ()
		{
			App.Current.ViewLocator = new ViewLocator ();
			App.Current.ViewLocator.Register ("TimelineEventView", typeof (DummyTimelineEventView));
			App.Current.DrawingToolkit = new Mock<IDrawingToolkit> ().Object;
		}

		[SetUp]
		public void SetUp ()
		{
			project = Utils.CreateProject (true);
			var projectVM = new ProjectVM { Model = project };
			widgetMock = new Mock<IWidget> ();
			widgetMock.SetupAllProperties ();
			timeline = new EventTypeTimelineView ();
			timeline.ViewModel = projectVM.Timeline.EventTypesTimeline.ViewModels.First ();
		}

		[Test]
		public void TestFillTimeFromViewModel ()
		{
			Assert.AreEqual (1, timeline.nodes.OfType<DummyTimelineEventView> ().Count ());
		}

		[Test]
		public void TestClearTimelineWithNewViewModel ()
		{
			timeline.ViewModel = new EventTypeTimelineVM (new EventTypeVM { Model = new EventType () });

			Assert.AreEqual (0, timeline.nodes.OfType<DummyTimelineEventView> ().Count ());
		}

		[Test]
		public void TestAddEvent ()
		{
			project.AddEvent (new TimelineEvent { EventType = project.EventTypes [0] });

			Assert.AreEqual (2, timeline.nodes.OfType<DummyTimelineEventView> ().Count ());
		}

		[Test]
		public void TestRemoveEvent ()
		{
			project.Timeline.Remove (project.Timeline [0]);

			Assert.AreEqual (0, timeline.nodes.OfType<DummyTimelineEventView> ().Count ());
		}
	}
}
