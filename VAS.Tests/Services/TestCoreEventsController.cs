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
using VAS.Core.ViewModel;
using VAS.Services.Controller;
using VAS.Services.ViewModel;

namespace VAS.Tests.Services
{
	[TestFixture]
	public class TestCoreEventsController
	{
		CoreEventsController controller;
		ProjectVM projectVM;

		[SetUp]
		public void Setup ()
		{
			var capturerMock = new Mock<IFramesCapturer> ();
			capturerMock.SetupAllProperties ();

			var mtkMock = new Mock<IMultimediaToolkit> ();
			mtkMock.Setup (m => m.GetFramesCapturer ()).Returns (capturerMock.Object);
			App.Current.MultimediaToolkit = mtkMock.Object;

			controller = new CoreEventsController ();
			projectVM = new ProjectVM { Model = Utils.CreateProject (true) };
			controller.SetViewModel (new ProjectAnalysisVM<ProjectVM> { Project = projectVM });
			controller.Start ();
		}

		[TearDown ()]
		public void TearDown ()
		{
			controller.Stop ();
		}

		[Test]
		public void TestMoveToEventType ()
		{
			EventType e1 = projectVM.Model.EventTypes [0];
			EventType e2 = projectVM.Model.EventTypes [1];
			var eventsToMove = projectVM.Model.Timeline.Where (e => e.EventType == e1).ToList ();
			Assert.AreEqual (1, eventsToMove.Count);
			Assert.AreEqual (1, projectVM.Model.Timeline.Count (e => e.EventType == e2));

			App.Current.EventsBroker.Publish (
				new MoveToEventTypeEvent {
					TimelineEvents = eventsToMove,
					EventType = e2,
				}
			);

			Assert.AreEqual (0, projectVM.Timeline.FullTimeline.Count (e => e.Model.EventType == e1));
			Assert.AreEqual (2, projectVM.Timeline.FullTimeline.Count (e => e.Model.EventType == e2));
		}
	}
}
