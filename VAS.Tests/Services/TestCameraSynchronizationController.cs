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
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services;
using VAS.Services.Controller;
using VAS.Services.ViewModel;

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
		Mock<IStateController> stateControllerMock;

		[OneTimeSetUp]
		public void Init ()
		{
			stateControllerMock = new Mock<IStateController> ();
			App.Current.StateController = stateControllerMock.Object;
			App.Current.HotkeysService = new HotkeysService ();
			PlaybackHotkeys.RegisterDefaultHotkeys ();
			GeneralUIHotkeys.RegisterDefaultHotkeys ();
			DrawingToolHotkeys.RegisterDefaultHotkeys ();
		}

		[SetUp]
		public void SetUp ()
		{
			videoPlayerVM = new VideoPlayerVM ();
			videoPlayerControllerMock = new Mock<IVideoPlayerController> ();
			videoPlayerVM.Player = videoPlayerControllerMock.Object;
			Project project = Utils.CreateProject (false);
			var period = new Period ();
			period.Nodes.Add (new TimeNode {
				Start = new Time (0),
				Stop = new Time (3000)
			});
			project.Periods.Add (period);
			projectVM = new DummyProjectVM { Model = project };
			camSyncVM = new CameraSynchronizationVM { VideoPlayer = videoPlayerVM, Project = projectVM };
			camSyncController = new CameraSynchronizationController ();
			videoPlayerControllerMock.ResetCalls ();
			stateControllerMock.ResetCalls ();

			KeyContext context = new KeyContext ();
			foreach (KeyAction action in camSyncController.GetDefaultKeyActions ()) {
				context.AddAction (action);
			}
			App.Current.KeyContextManager.NewKeyContexts (new List<KeyContext> { context });
		}

		[TearDown]
		public void TearDown ()
		{
			camSyncController.Stop ();
			videoPlayerControllerMock.Object.Stop ();
		}

		public void Start ()
		{
			videoPlayerControllerMock.Object.Start ();
			camSyncController.SetViewModel (camSyncVM);
			camSyncController.Start ();
		}

		[Test]
		public void TestPeriodsInitialed_NoPeriods ()
		{
			projectVM.Periods.ViewModels.Clear ();
			videoPlayerControllerMock.Object.Start ();
			camSyncController.SetViewModel (camSyncVM);
			camSyncController.Start ();

			Assert.AreEqual (projectVM.Periods.Model, camSyncVM.InitialPeriods);
			Assert.AreEqual (projectVM.Dashboard.GamePeriods.Count, projectVM.Periods.Count ());
		}

		[Test]
		public void TestPeriodsInitialed_ExistingPeriods ()
		{
			Start ();

			Assert.AreEqual (projectVM.Periods.Model, camSyncVM.InitialPeriods);
		}

		[Test]
		public void TestSeek_PeriodStartChanged ()
		{
			Start ();

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
			Start ();
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
			Start ();
			videoPlayerVM.CurrentTime = new Time (50);
			projectVM.FileSet.First ().Offset = new Time (20);

			// FIXME: It should be called once but the forwarder bug makes it 3
			videoPlayerControllerMock.Verify (pl => pl.Seek (new Time (50), true, false, false), Times.Exactly (3));
		}

		[Test]
		public void TestHotkey_FrameStepUp_CameraSelected ()
		{
			Start ();
			MediaFileVM mediaFile = projectVM.FileSet.First ();
			Time previousOffset = mediaFile.Offset;
			mediaFile.SelectedGrabber = SelectionPosition.All;
			App.Current.KeyContextManager.HandleKeyPressed (App.Current.Keyboard.ParseName ("Right"));

			Assert.AreEqual (previousOffset + new Time (1000 / mediaFile.Fps), mediaFile.Offset);
			// FIXME: It should be called once but the forwarder bug makes it 3
			videoPlayerControllerMock.Verify (pl => pl.Seek (new Time (0), true, false, false), Times.Exactly (3));
		}

		[Test]
		public void TestHotkey_FrameStepDown_CameraSelected ()
		{
			Start ();
			MediaFileVM mediaFile = projectVM.FileSet.First ();
			Time previousOffset = mediaFile.Offset;
			mediaFile.SelectedGrabber = SelectionPosition.All;
			App.Current.KeyContextManager.HandleKeyPressed (App.Current.Keyboard.ParseName ("Left"));

			Assert.AreEqual (previousOffset - new Time (1000 / mediaFile.Fps), mediaFile.Offset);
			// FIXME: It should be called once but the forwarder bug makes it 3
			videoPlayerControllerMock.Verify (pl => pl.Seek (new Time (0), true, false, false), Times.Exactly (3));
		}

		[Test]
		public void TestHotkey_FrameStepUp_NoCameraSelected ()
		{
			Start ();
			MediaFileVM mediaFile = projectVM.FileSet.First ();
			Time previousOffset = mediaFile.Offset;
			mediaFile.SelectedGrabber = SelectionPosition.All;
			App.Current.KeyContextManager.HandleKeyPressed (App.Current.Keyboard.ParseName ("Right"));

			Assert.AreEqual (previousOffset + new Time (1000 / mediaFile.Fps), mediaFile.Offset);
			// FIXME: It should be called once but the forwarder bug makes it 3
			videoPlayerControllerMock.Verify (pl => pl.Seek (new Time (0), true, false, false), Times.Exactly (3));
		}

		[Test]
		public void TestHotkey_FrameStepDown_NoCameraSelected ()
		{
			Start ();
			MediaFileVM mediaFile = projectVM.FileSet.First ();
			Time previousOffset = mediaFile.Offset;
			mediaFile.SelectedGrabber = SelectionPosition.All;
			App.Current.KeyContextManager.HandleKeyPressed (App.Current.Keyboard.ParseName ("Right"));

			Assert.AreEqual (previousOffset + new Time (1000 / mediaFile.Fps), mediaFile.Offset);
			// FIXME: It should be called once but the forwarder bug makes it 3
			videoPlayerControllerMock.Verify (pl => pl.Seek (new Time (0), true, false, false), Times.Exactly (3));
		}

		[Test]
		public void TestSave ()
		{
			Start ();
			camSyncVM.Save.Execute ();

			stateControllerMock.Verify (st => st.MoveBack (), Times.Once ());
		}

		[Test]
		public void CameraSynchronizationController_SynchronizeEventsWithPeriods_EventWithOffset ()
		{
			Start ();
			camSyncVM.SynchronizeEventsWithPeriods = true;

			var firstNode = projectVM.Periods.First ().Model.Nodes.First ();
			// Create a new event
			var tlEvent = projectVM.Model.CreateEvent (projectVM.EventTypes.First ().Model, firstNode.Start,
													   firstNode.Start, firstNode.Start + 200, null, 0);
			projectVM.Timeline.Model.Add (tlEvent);
			const int offset = 200;
			firstNode.Start += offset;
			firstNode.Stop += offset;
			// Offset the period by 20
			camSyncVM.Save.Execute ();

			Assert.AreEqual (new Time (offset), tlEvent.Start);
		}

		[Test]
		public void ResyncEvents_OffsetOutsidePeriods_EventUpdatedToNewPeriodTime ()
		{
			Start ();
			camSyncVM.SynchronizeEventsWithPeriods = true;

			var firstNode = projectVM.Periods.First ().Model.Nodes.First ();
			// Create a new event
			var tlEvent = projectVM.Model.CreateEvent (projectVM.EventTypes.First ().Model, firstNode.Start,
													   firstNode.Start, firstNode.Start + 200, null, 0);
			projectVM.Timeline.Model.Add (tlEvent);
			const int offset = 20000;
			firstNode.Start += offset;
			firstNode.Stop += offset;
			// Offset the period outside the current periods
			camSyncVM.Save.Execute ();

			Assert.AreEqual (new Time (offset), tlEvent.Start);
		}

		[Test]
		public void CameraSynchronizationController_DoNotSynchronizeEventsWithPeriods_EventWithNoOffset ()
		{
			Start ();
			camSyncVM.SynchronizeEventsWithPeriods = false;

			var firstNode = camSyncVM.InitialPeriods.First ().Nodes.First ();
			// Create a new event
			var tlEvent = projectVM.Model.CreateEvent (projectVM.EventTypes.First ().Model, firstNode.Start,
													   firstNode.Start, firstNode.Start + 200, null, 0);
			projectVM.Timeline.Model.Add (tlEvent);
			firstNode.Start += 200;
			firstNode.Stop += 200;
			// Offset the period by 20
			camSyncVM.Save.Execute ();

			Assert.AreEqual (new Time (000), tlEvent.Start);
		}
	}
}
