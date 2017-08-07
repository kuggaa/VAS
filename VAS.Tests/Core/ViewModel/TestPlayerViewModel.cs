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
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.ViewModel;

namespace VAS.Tests.Core.ViewModel
{
	[TestFixture]
	public class TestPlayerViewModel
	{
		[Test]
		public void TestChangePlaybackRate ()
		{
			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();
			var viewModel = new VideoPlayerVM { Player = playerController.Object };

			viewModel.SetRate (3);

			Assert.AreEqual (3, playerController.Object.Rate);
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

			viewModel.SetVolume (3);

			Assert.AreEqual (3, playerController.Object.Volume);
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
			mockService.Setup (s => s.CanExecuteFeature (VASFeature.Zoom.ToString ())).Returns (true);
			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();
			var viewModel = new VideoPlayerVM { Player = playerController.Object };

			viewModel.SetZoomCommand.Execute (3.0);

			playerController.Verify (p => p.SetZoom (3.0), Times.Once);
		}


		[Test]
		public void SetZoom_Limited_MoveToUpgradeDialog ()
		{
			Mock<ILicenseLimitationsService> mockService = new Mock<ILicenseLimitationsService> ();
			App.Current.LicenseLimitationsService = mockService.Object;
			mockService.Setup (s => s.CanExecuteFeature (VASFeature.Zoom.ToString ())).Returns (false);
			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();
			var viewModel = new VideoPlayerVM { Player = playerController.Object };

			viewModel.SetZoomCommand.Execute (3.0);

			mockService.Verify (s => s.MoveToUpgradeDialog (VASFeature.Zoom.ToString ()), Times.Once);
		}

		[Test]
		public void ShowZoom_NoLimited_ViewModelUpdated ()
		{
			Mock<ILicenseLimitationsService> mockService = new Mock<ILicenseLimitationsService> ();
			App.Current.LicenseLimitationsService = mockService.Object;
			mockService.Setup (s => s.CanExecuteFeature (VASFeature.Zoom.ToString ())).Returns (true);
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
			mockService.Setup (s => s.CanExecuteFeature (VASFeature.Zoom.ToString ())).Returns (false);
			var playerController = new Mock<IVideoPlayerController> ();
			playerController.SetupAllProperties ();
			var viewModel = new VideoPlayerVM { Player = playerController.Object };

			viewModel.ShowZoomCommand.Execute ();

			mockService.Verify (s => s.MoveToUpgradeDialog (VASFeature.Zoom.ToString ()), Times.Once);

		}
	}
}
