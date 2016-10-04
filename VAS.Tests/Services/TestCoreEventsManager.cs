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
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Store;
using VAS.Services;

namespace VAS.Tests.Services
{
	[TestFixture]
	public class TestCoreEventsManager
	{
		CoreEventsManager controller;
		Project project;

		[SetUp]
		public void Setup ()
		{
			var capturerMock = new Mock<IFramesCapturer> ();
			capturerMock.SetupAllProperties ();

			var mtkMock = new Mock<IMultimediaToolkit> ();
			mtkMock.Setup (m => m.GetFramesCapturer ()).Returns (capturerMock.Object);
			App.Current.MultimediaToolkit = mtkMock.Object;

			controller = new CoreEventsManager ();
			project = Utils.CreateProject (true);
			controller.Start ();
			App.Current.EventsBroker.Publish (new OpenedProjectEvent {
				Project = project, ProjectType = ProjectType.FileProject, Filter = new Utils.EventsFilterDummy (project),
			});
		}

		[TearDown ()]
		public void TearDown ()
		{
			controller.Stop ();
			project.Dispose ();
		}

		[Test]
		public void TestMoveToEventType ()
		{
			EventType e1 = project.EventTypes [0];
			EventType e2 = project.EventTypes [1];
			var eventsToMove = project.Timeline.Where (e => e.EventType == e1).ToList ();
			Assert.AreEqual (1, eventsToMove.Count);
			Assert.AreEqual (1, project.Timeline.Count (e => e.EventType == e2));

			App.Current.EventsBroker.Publish (
				new MoveToEventTypeEvent {
					TimelineEvents = eventsToMove,
					EventType = e2,
				}
			);

			Assert.AreEqual (0, project.Timeline.Count (e => e.EventType == e1));
			Assert.AreEqual (2, project.Timeline.Count (e => e.EventType == e2));
		}
	}
}
