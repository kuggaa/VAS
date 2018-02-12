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
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Tests.Core.ViewModel
{
	[TestFixture]
	public class TestVideoPlayerVM
	{
		[OneTimeSetUp]
		public void OneTimeSetUp ()
		{
			SetupClass.SetUp ();
		}

		[Test]
		public void TestChangePlaybackRate ()
		{
			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();
			var viewModel = new VideoPlayerVM { Player = playerController.Object };

			App.Current.RateList = new List<double> () { 3D };
			viewModel.ChangeRateCommand.Execute (0D);

			Assert.AreEqual (3D, playerController.Object.Rate);
		}

		[Test]
		public void TestUpdatePlaybackRate ()
		{
			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();
			var viewModel = new VideoPlayerVM { Player = playerController.Object };

			// Rate changes the VM property but not the actual playback rate
			viewModel.Rate = 3;

			Assert.AreEqual (0, playerController.Object.Rate);
		}

		[Test]
		public void TestChangeVolume ()
		{
			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();
			var viewModel = new VideoPlayerVM { Player = playerController.Object };

			viewModel.ChangeVolumeCommand.Execute (3D);

			Assert.AreEqual (3D / 100, playerController.Object.Volume);
		}

		[Test]
		public void TestUpdateVolume ()
		{
			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();
			var viewModel = new VideoPlayerVM { Player = playerController.Object };

			// Volume changes the VM property but not the actual volume
			viewModel.Volume = 5;

			Assert.AreEqual (0, playerController.Object.Volume);
		}

		[Test]
		public void MoveROI_ROIMoved ()
		{
			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();
			var viewModel = new VideoPlayerVM { Player = playerController.Object };

			viewModel.MoveROI (new Point (1, 3));

			playerController.Verify (p => p.MoveROI (new Point (1, 3)));
		}

		[Test]
		public void SetZoom_NoLimited_ViewModelUpdated ()
		{
			Mock<ILicenseLimitationsService> mockService = new Mock<ILicenseLimitationsService> ();
			App.Current.LicenseLimitationsService = mockService.Object;
			mockService.Setup (s => s.CanExecute (VASFeature.Zoom.ToString ())).Returns (true);
			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();
			var viewModel = new VideoPlayerVM { Player = playerController.Object };

			viewModel.SetZoomCommand.Execute (3.0f);

			playerController.Verify (p => p.SetZoom (3.0f), Times.Once);
		}


		[Test]
		public void SetZoom_Limited_MoveToUpgradeDialog ()
		{
			Mock<ILicenseLimitationsService> mockService = new Mock<ILicenseLimitationsService> ();
			App.Current.LicenseLimitationsService = mockService.Object;
			mockService.Setup (s => s.CanExecute (VASFeature.Zoom.ToString ())).Returns (false);
			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();
			var viewModel = new VideoPlayerVM { Player = playerController.Object };

			viewModel.SetZoomCommand.Execute (3.0f);

			mockService.Verify (s => s.MoveToUpgradeDialog (VASFeature.Zoom.ToString ()), Times.Once);
		}

		[Test]
		public void ShowZoom_NoLimited_ViewModelUpdated ()
		{
			Mock<ILicenseLimitationsService> mockService = new Mock<ILicenseLimitationsService> ();
			App.Current.LicenseLimitationsService = mockService.Object;
			mockService.Setup (s => s.CanExecute (VASFeature.Zoom.ToString ())).Returns (true);
			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();
			var viewModel = new VideoPlayerVM { Player = playerController.Object };

			Assert.IsFalse (viewModel.ShowZoom);

			viewModel.ShowZoomCommand.Execute ();

			Assert.IsTrue (viewModel.ShowZoom);
		}

		[Test]
		public void ShowZoom_Limited_MoveToUpgradeDialog ()
		{
			Mock<ILicenseLimitationsService> mockService = new Mock<ILicenseLimitationsService> ();
			App.Current.LicenseLimitationsService = mockService.Object;
			mockService.Setup (s => s.CanExecute (VASFeature.Zoom.ToString ())).Returns (false);
			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();
			var viewModel = new VideoPlayerVM { Player = playerController.Object };

			viewModel.ShowZoomCommand.Execute ();

			mockService.Verify (s => s.MoveToUpgradeDialog (VASFeature.Zoom.ToString ()), Times.Once);
		}

		[Test]
		public void EditEventDurationCommand_Initialized_CommandCallsController ()
		{
			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();
			var viewModel = new VideoPlayerVM { Player = playerController.Object };
			viewModel.EditEventDurationCommand.Executable = true;

			viewModel.EditEventDurationCommand.Execute (true);

			playerController.Verify (p => p.SetEditEventDurationMode (true));
		}

		[Test ()]
		public void ChangeVolumeCommand_NewVideoPlayerVM_PlayerVolumeSettedTo0dot5 ()
		{
			///Arrange
			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();
			var target = new VideoPlayerVM { Player = playerController.Object };

			///Act

			target.ChangeVolumeCommand.Execute (50.0D);

			///Assert
			playerController.VerifySet (p => p.Volume, Times.Once ());
			Assert.AreEqual (0.5D, target.Player.Volume);
		}

		[Test ()]
		public void ChangeStepCommand_NewVideoPlayerVM_PlayerSetStepInvokedWith5secondsParameter ()
		{
			///Arrange
			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();

			App.Current.StepList = new List<int> { 5 };
			var target = new VideoPlayerVM { Player = playerController.Object };

			///Act

			target.ChangeStepCommand.Execute (0.0D);

			///Assert
			playerController.Verify (p => p.SetStep (new Time { TotalSeconds = 5 }), Times.Once ());
		}

		[Test ()]
		public void ChangeRateCommand_NewVideoPlayerVM_PlayerRateSettedTo12dot0 ()
		{
			///Arrange
			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();

			App.Current.RateList = new List<double> { 12.0D };
			var target = new VideoPlayerVM { Player = playerController.Object };

			///Act

			target.ChangeRateCommand.Execute (0.0D);

			///Assert
			playerController.VerifySet (p => p.Rate, Times.Once ());
			Assert.AreEqual (12.0D, playerController.Object.Rate);
		}

		[Test ()]
		public void CloseCommand_NewVideoPlayerVM_PlayerUnloadCurrentEventInvoked ()
		{
			///Arrange

			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();

			var target = new VideoPlayerVM { Player = playerController.Object };

			///Act

			target.CloseCommand.Execute ();

			///Assert
			playerController.Verify (p => p.UnloadCurrentEvent (), Times.Once ());
		}

		[Test ()]
		public void PreviousCommand_NewVideoPlayerVM_PlayerPreviousInvoked ()
		{
			///Arrange

			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();

			var target = new VideoPlayerVM { Player = playerController.Object };

			///Act

			target.PreviousCommand.Execute ();

			///Assert
			playerController.Verify (p => p.Previous (false), Times.Once ());
		}

		[Test ()]
		public void NextCommand_NewVideoPlayerVM_PlayerNextInvoked ()
		{
			///Arrange

			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();

			var target = new VideoPlayerVM { Player = playerController.Object };

			///Act

			target.NextCommand.Execute ();

			///Assert
			playerController.Verify (p => p.Next (), Times.Once ());
		}

		[Test ()]
		public void PlayCommand_NewVideoPlayerVM_PlayerPlayInvoked ()
		{
			///Arrange

			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();

			var target = new VideoPlayerVM { Player = playerController.Object };

			///Act

			target.PlayCommand.Execute ();

			///Assert
			playerController.Verify (p => p.Play (false), Times.Once ());
		}

		[Test ()]
		public void PauseCommand_NewVideoPlayerVM_PlayerPauseInvoked ()
		{
			///Arrange

			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();

			var target = new VideoPlayerVM { Player = playerController.Object };

			///Act

			target.PauseCommand.Execute (false);

			///Assert
			playerController.Verify (p => p.Pause (false), Times.Once ());
		}

		[Test ()]
		public void DrawCommand_NewVideoPlayerVM_PlayerDrawFrameInvoked ()
		{
			///Arrange

			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();

			var target = new VideoPlayerVM { Player = playerController.Object };

			///Act

			target.DrawCommand.Execute ();

			///Assert
			playerController.Verify (p => p.DrawFrame (), Times.Once ());
		}

		[Test ()]
		public void DetachCommand_NewVideoPlayerVM_PlayerDetachCommandInvoked ()
		{
			///Arrange

			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();

			var target = new VideoPlayerVM { Player = playerController.Object };
			var subscribeCalled = false;
			App.Current.EventsBroker.Subscribe<DetachEvent> (detachedEvent => subscribeCalled = true);

			///Act

			target.DetachCommand.Execute ();

			///Assert
			Assert.IsTrue (subscribeCalled);

		}

		[Test ()]
		public void ViewPortsSwitchToggleCommand_NewVideoPlayerVM_PlayerDetachCommandInvoked ()
		{
			///Arrange

			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();

			var target = new VideoPlayerVM { Player = playerController.Object };

			///Act

			target.ViewPortsSwitchToggleCommand.Execute ();

			///Assert

			Assert.IsFalse (target.ViewPortsSwitchActive);

		}

		[Test ()]
		public void SeekCommand_NewVideoPlayerVM_PlayerSeekInvoked ()
		{
			///Arrange

			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();
			var time = new Time (1234);

			var target = new VideoPlayerVM { Player = playerController.Object };

			///Act

			target.SeekCommand.Execute (new VideoPlayerSeekOptions (time));

			///Assert
			playerController.Verify (p => p.Seek (time, false, false, false), Times.Once ());
		}

		[Test ()]
		public void ExposeCommand_NewVideoPlayerVM_PlayerExposeInvoked ()
		{
			///Arrange

			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();

			var target = new VideoPlayerVM { Player = playerController.Object };

			///Act

			target.ExposeCommand.Execute ();

			///Assert
			playerController.Verify (p => p.Expose (), Times.Once ());
		}

		[Test ()]
		public void ReadyCommand_NewVideoPlayerVM_PlayerReadyInvoked ()
		{
			///Arrange

			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();

			var target = new VideoPlayerVM { Player = playerController.Object };

			///Act

			target.ReadyCommand.Execute (true);

			///Assert
			playerController.Verify (p => p.Ready (true), Times.Once ());
		}

		[Test ()]
		public void StopCommand_NewVideoPlayerVM_PlayerStopInvoked ()
		{
			///Arrange

			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();

			var target = new VideoPlayerVM { Player = playerController.Object };

			///Act

			target.StopCommand.Execute ();

			///Assert
			playerController.Verify (p => p.Stop (), Times.Once ());
		}

		[Test ()]
		public void TogglePlayCommand_NewVideoPlayerVM_PlayerTogglePlayInvoked ()
		{
			///Arrange

			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();

			var target = new VideoPlayerVM { Player = playerController.Object };

			///Act

			target.TogglePlayCommand.Execute ();

			///Assert
			playerController.Verify (p => p.TogglePlay (), Times.Once ());
		}

		[Test ()]
		public void SeekToPreviousFrameCommand_NewVideoPlayerVM_PlayerSeekToPreviousFrameInvoked ()
		{
			///Arrange

			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();

			var target = new VideoPlayerVM { Player = playerController.Object };

			///Act

			target.SeekToPreviousFrameCommand.Execute ();

			///Assert
			playerController.Verify (p => p.SeekToPreviousFrame (), Times.Once ());
		}

		[Test ()]
		public void SeekToNextFrameCommand_NewVideoPlayerVM_PlayerSeekToNextFrameInvoked ()
		{
			///Arrange

			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();

			var target = new VideoPlayerVM { Player = playerController.Object };

			///Act

			target.SeekToNextFrameCommand.Execute ();

			///Assert
			playerController.Verify (p => p.SeekToNextFrame (), Times.Once ());
		}

		[Test ()]
		public void StepBackwardCommand_NewVideoPlayerVM_PlayerStepBackwardInvoked ()
		{
			///Arrange

			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();

			var target = new VideoPlayerVM { Player = playerController.Object };

			///Act

			target.StepBackwardCommand.Execute ();

			///Assert
			playerController.Verify (p => p.StepBackward (), Times.Once ());
		}

		[Test ()]
		public void StepForwardCommand_NewVideoPlayerVM_StepForwardInvoked ()
		{
			///Arrange

			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();

			var target = new VideoPlayerVM { Player = playerController.Object };

			///Act

			target.StepForwardCommand.Execute ();

			///Assert
			playerController.Verify (p => p.StepForward (), Times.Once ());
		}

		[Test]
		public void LoadEvents_ReusesTimelineEventVMInstance ()
		{
			var evVM1 = new TimelineEventVM {
				Model = new TimelineEvent {
					Name = "evVM1"
				}
			};
			var evVM2 = new TimelineEventVM {
				Model = new TimelineEvent {
					Name = "evVM2"
				}
			};
			PlaylistVM playlist = null;
			IEnumerable<TimelineEventVM> eventList = new List<TimelineEventVM> { evVM1, evVM2 };
			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();
			var viewModel = new VideoPlayerVM { Player = playerController.Object };
			playerController.Setup (pl => pl.LoadPlaylistEvent
									(It.IsAny<PlaylistVM> (), It.IsAny<IPlayable> (), true))
			                .Callback<PlaylistVM, IPlayable, bool> ((a, b, c) => { playlist = a; b.Playing = c; });

			viewModel.LoadEvents (eventList, true);

			Assert.IsTrue (evVM1.Playing);
			Assert.IsFalse (evVM2.Playing);
			Assert.AreSame (evVM1, (playlist.ViewModels[0] as PlaylistPlayElementVM).Play);
			Assert.AreSame (evVM2, (playlist.ViewModels [1] as PlaylistPlayElementVM).Play);

			playlist.ViewModels [0].Playing = false;
			playlist.ViewModels [1].Playing = true;

			Assert.IsFalse (evVM1.Playing);
			Assert.IsTrue (evVM2.Playing);
		}
	}
}
