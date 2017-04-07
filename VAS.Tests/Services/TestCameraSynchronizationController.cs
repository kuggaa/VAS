//
//  Copyright (C) 2017 Fluendo S.A.
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
using Moq;
using NUnit.Framework;
using VAS.Core.Interfaces;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services.Controller;
using VAS.Services.ViewModel;
using System.Linq;

namespace VAS.Tests.Services
{
	[TestFixture]
	public class TestCameraSynchronizationController
	{
		CameraSynchronizationVM camSyncVM;
		CameraSynchronizationController camSyncController;
		VideoPlayerVM videoPlayerVM;
		ProjectVM projectVM;
		Mock<IVideoPlayerController> videoPlayerControllerMock;

		[SetUp]
		public void SetUp ()
		{
			videoPlayerVM = new VideoPlayerVM ();
			videoPlayerControllerMock = new Mock<IVideoPlayerController> ();
			videoPlayerVM.Player = videoPlayerControllerMock.Object;
			Project project = Utils.CreateProject (false);
			projectVM = new DummyProjectVM { Model = project };
			camSyncVM = new CameraSynchronizationVM { VideoPlayer = videoPlayerVM, Project = projectVM };
			camSyncController = new CameraSynchronizationController ();
			camSyncController.SetViewModel (camSyncVM);
			videoPlayerControllerMock.ResetCalls ();
		}

		[Test]
		public void TestSart ()
		{
			videoPlayerControllerMock.Object.Start ();
			camSyncController.Start ();
		}

		[Test]
		public void TestSeek_PeriodStartChanged ()
		{
			Period p = new Period { Name = "Period" };
			var timeNode = new TimeNode {
				Name = "Period1",
				Start = new Time { TotalSeconds = 0 },
				Stop = new Time { TotalSeconds = projectVM.FileSet.Duration.Seconds }
			};
			p.Nodes.Add (timeNode);
			projectVM.Periods.Model.Add (p);
			timeNode.Start = new Time (10);

			videoPlayerControllerMock.Verify (pl => pl.Seek (new Time (10), false, false, false), Times.Once ());
		}

		[Test]
		public void TestSeek_PeriodStopChanged ()
		{
			Period p = new Period { Name = "Period" };
			var timeNode = new TimeNode {
				Name = "Period1",
				Start = new Time { TotalSeconds = 0 },
				Stop = new Time { TotalSeconds = projectVM.FileSet.Duration.Seconds }
			};
			p.Nodes.Add (timeNode);
			projectVM.Periods.Model.Add (p);
			timeNode.Stop = new Time (50);

			videoPlayerControllerMock.Verify (pl => pl.Seek (new Time (50), false, false, false), Times.Once ());
		}

		[Test]
		public void TestSeek_CameraChanged ()
		{
			videoPlayerControllerMock.Setup (vp => vp.CurrentTime).Returns (new Time (50));
			projectVM.FileSet.First ().Offset = new Time (20);

			videoPlayerControllerMock.Verify (pl => pl.Seek (new Time (50), true, false, false), Times.Once ());
		}

		[Test]
		public void TestSeek_FrameStepUp ()
		{
		}

		[Test]
		public void TestSeek_FrameStepDown ()
		{
		}

		[Test]
		public void TestResyncEvents ()
		{
		}
	}
}
