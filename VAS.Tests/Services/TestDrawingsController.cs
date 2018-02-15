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
using System;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services.Controller;
using VAS.Services.State;

namespace VAS.Tests.Services
{
	[TestFixture]
	public class TestDrawingsController
	{
		DrawingsService controller;
		VideoPlayerVM videoPlayer;
		Mock<IVideoPlayerController> videoPlayerMock;
		Mock<IMultimediaToolkit> mToolkitMock;
		Mock<IFramesCapturer> framesCapturerMock;
		Mock<IStateController> stateControllerMock;
		dynamic properties;

		[OneTimeSetUp]
		public void OneTimeSetUp ()
		{
			SetupClass.SetUp ();
		}

		[SetUp]
		public async Task Setup ()
		{
			videoPlayerMock = new Mock<IVideoPlayerController> ();
			videoPlayerMock.SetupGet (p => p.CurrentTime).Returns (new Time (100));
			videoPlayerMock.SetupGet (p => p.CurrentFrame).Returns (Utils.LoadImageFromFile ());
			videoPlayer = new VideoPlayerVM { Player = videoPlayerMock.Object };
			mToolkitMock = new Mock<IMultimediaToolkit> ();
			framesCapturerMock = new Mock<IFramesCapturer> ();
			mToolkitMock.Setup (m => m.GetFramesCapturer ()).Returns (framesCapturerMock.Object);
			stateControllerMock = new Mock<IStateController> ();
			stateControllerMock.Setup (sc => sc.MoveToModal (It.IsAny<string> (), It.IsAny<object> (), It.IsAny<bool> ())).Callback (
				(string s, Object e, bool f) => {
					properties = (ExpandoObject)e;
				});
			App.Current.StateController = stateControllerMock.Object;
			App.Current.MultimediaToolkit = mToolkitMock.Object;
			controller = new DrawingsService ();
			await controller.Start ();
		}

		[TearDown]
		public async Task TearDown ()
		{
			await controller.Stop ();
		}

		[Test]
		public void SetViewModel_VideoPlayerOnly_ViewModelSet ()
		{
			var viewModel = new Mock<IViewModel> ();
			var playerDealer = viewModel.As<IVideoPlayerDealer> ();
			playerDealer.SetupGet (vm => vm.VideoPlayer).Returns (videoPlayer);
			Assert.DoesNotThrow (() => controller.SetViewModel (viewModel.Object));
		}

		[Test]
		public void SetViewModel_VideoPlayerAndProject_ViewModelSet ()
		{
			var viewModel = new Mock<IViewModel> ();
			var playerDealer = viewModel.As<IVideoPlayerDealer> ();
			playerDealer.SetupGet (vm => vm.VideoPlayer).Returns (videoPlayer);
			var projectDealer = playerDealer.As<IProjectDealer> ();
			Assert.DoesNotThrow (() => controller.SetViewModel (viewModel.Object));
		}

		[Test]
		public void DrawFrame_WithTimelineEventNewDrawing_DrawingToolOpenedAndDrawingCreated ()
		{
			var viewModel = new Mock<IViewModel> ();
			var playerDealer = viewModel.As<IVideoPlayerDealer> ();
			playerDealer.SetupGet (vm => vm.VideoPlayer).Returns (videoPlayer);
			controller.SetViewModel (viewModel.Object);
			TimelineEventVM evtVM = CreateTimelineEventVM ();
			var frame = Utils.LoadImageFromFile ();
			controller.DrawFrame (videoPlayer, null, evtVM, -1, null, frame);

			mToolkitMock.Verify (m => m.GetFramesCapturer (), Times.Never ());
			stateControllerMock.Verify (s => s.MoveToModal (DrawingToolState.NAME, It.IsAny<ExpandoObject> (), false));
			Assert.AreEqual (evtVM, properties.timelineEvent);
			Assert.AreEqual (evtVM.CamerasConfig [0], properties.cameraconfig);
			Assert.AreEqual (frame, properties.frame);
			Assert.IsNotNull (properties.drawing);
		}

		[Test]
		public void DrawFrame_WithTimelineEventExistingDrawing_DrawingToolOpenedAndEventWithDrawingReused ()
		{
			var viewModel = new Mock<IViewModel> ();
			var playerDealer = viewModel.As<IVideoPlayerDealer> ();
			playerDealer.SetupGet (vm => vm.VideoPlayer).Returns (videoPlayer);
			controller.SetViewModel (viewModel.Object);
			TimelineEventVM evtVM = CreateTimelineEventVM (true);
			var frame = Utils.LoadImageFromFile ();
			controller.DrawFrame (videoPlayer, null, evtVM, 0, null, frame);

			mToolkitMock.Verify (m => m.GetFramesCapturer (), Times.Never ());
			stateControllerMock.Verify (s => s.MoveToModal (DrawingToolState.NAME, It.IsAny<ExpandoObject> (), false));
			Assert.AreEqual (evtVM, properties.timelineEvent);
			Assert.AreEqual (evtVM.CamerasConfig [0], properties.cameraconfig);
			Assert.AreEqual (evtVM.Drawings [0], properties.drawing);
			Assert.AreEqual (frame, properties.frame);
		}

		[Test]
		public void DrawFrame_WithoutTimelineEvent_DrawingToolOpenedWithoutTimelineEvent ()
		{
			var viewModel = new Mock<IViewModel> ();
			var playerDealer = viewModel.As<IVideoPlayerDealer> ();
			playerDealer.SetupGet (vm => vm.VideoPlayer).Returns (videoPlayer);
			controller.SetViewModel (viewModel.Object);
			TimelineEventVM evtVM = CreateTimelineEventVM (true);
			var frame = Utils.LoadImageFromFile ();
			controller.DrawFrame (videoPlayer, null, null, -1, null, frame);

			mToolkitMock.Verify (m => m.GetFramesCapturer (), Times.Never ());
			stateControllerMock.Verify (s => s.MoveToModal (DrawingToolState.NAME, It.IsAny<ExpandoObject> (), false));
			Assert.IsNull (properties.timelineEvent);
			Assert.AreEqual (frame, properties.frame);
		}

		[Test]
		public async Task DrawFrame_AnotherFrameNoDrawing_CapturerCreatedAndUsesVideoPlayerTime ()
		{
			var viewModel = new Mock<IViewModel> ();
			var playerDealer = viewModel.As<IVideoPlayerDealer> ();
			TimelineEventVM evtVM = CreateTimelineEventVM ();
			playerDealer.SetupGet (vm => vm.VideoPlayer).Returns (videoPlayer);
			controller.SetViewModel (viewModel.Object);

			controller.DrawFrame (videoPlayer, null, evtVM, -1, null, null);

			videoPlayerMock.Verify (p => p.Pause (true), Times.Once ());
			mToolkitMock.Verify (m => m.GetFramesCapturer (), Times.Once ());
			framesCapturerMock.Verify (m => m.Open (evtVM.FileSet.MediaFiles [0].FilePath), Times.Once ());
			framesCapturerMock.Verify (m => m.GetFrame (videoPlayerMock.Object.CurrentTime, true,
														(int)evtVM.FileSet.MediaFiles [0].DisplayVideoWidth,
														(int)evtVM.FileSet.MediaFiles [0].DisplayVideoHeight), Times.Once ());
			framesCapturerMock.Verify (m => m.Dispose (), Times.Once ());
		}

		[Test]
		public void DrawFrame_AnotherFrameWitDrawing_CapturerCreatedAndUsesDrawingTime ()
		{
			var viewModel = new Mock<IViewModel> ();
			var playerDealer = viewModel.As<IVideoPlayerDealer> ();
			TimelineEventVM evtVM = CreateTimelineEventVM (true);
			playerDealer.SetupGet (vm => vm.VideoPlayer).Returns (videoPlayer);
			controller.SetViewModel (viewModel.Object);

			controller.DrawFrame (videoPlayer, null, evtVM, 0, null, null);

			mToolkitMock.Verify (m => m.GetFramesCapturer (), Times.Once ());
			framesCapturerMock.Verify (m => m.Open (evtVM.FileSet.MediaFiles [0].FilePath), Times.Once ());
			framesCapturerMock.Verify (m => m.GetFrame (evtVM.Drawings [0].Render, true,
														(int)evtVM.FileSet.MediaFiles [0].DisplayVideoWidth,
														(int)evtVM.FileSet.MediaFiles [0].DisplayVideoHeight), Times.Once ());
			framesCapturerMock.Verify (m => m.Dispose (), Times.Once ());
		}

		[Test]
		public void DrawFrame_CurrentFrame_NoFramesCapturerCreated ()
		{
			var viewModel = new Mock<IViewModel> ();
			var playerDealer = viewModel.As<IVideoPlayerDealer> ();
			playerDealer.SetupGet (vm => vm.VideoPlayer).Returns (videoPlayer);
			controller.SetViewModel (viewModel.Object);
			App.Current.EventsBroker.Publish (
				new DrawFrameEvent {
					Play = null,
					DrawingIndex = -1,
					CamConfig = null,
				});

			mToolkitMock.Verify (m => m.GetFramesCapturer (), Times.Never ());
		}

		TimelineEventVM CreateTimelineEventVM (bool withDrawing = false)
		{
			var evt = new TimelineEvent {
				Start = new Time (100),
				Stop = new Time (200),
			};
			evt.FileSet = new MediaFileSet ();
			evt.FileSet.Add (new MediaFile (Path.GetTempFileName (), 34000, 25, true, true, "mp4", "h264", "aac", 320,
											240, 1.3, null, "Test asset 1"));
			if (withDrawing) {
				evt.Drawings.Add (new FrameDrawing { Render = new Time (2000) });
			}
			return new TimelineEventVM () { Model = evt };
		}

	}
}
