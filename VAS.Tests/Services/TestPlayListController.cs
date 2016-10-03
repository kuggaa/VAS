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
using System;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Services.Controller;
using VAS.Services.ViewModel;

namespace VAS.Tests.Services
{
	public class TestPlayListController
	{
		Mock<IGUIToolkit> mockGuiToolkit;
		Mock<IPlayerController> mockPlayerController;
		Mock<IDialogs> mockDiaklogs;
		PlayListController controller;

		[TestFixtureSetUp ()]
		public void FixtureSetup ()
		{
			mockPlayerController = new Mock<IPlayerController> ();
			mockDiaklogs = new Mock<IDialogs> ();
		}

		[SetUp ()]
		public void Setup ()
		{
			mockGuiToolkit = new Mock<IGUIToolkit> ();
			App.Current.GUIToolkit = mockGuiToolkit.Object;
			mockDiaklogs = new Mock<IDialogs> ();
			App.Current.Dialogs = mockDiaklogs.Object;
			controller = new PlayListController (mockPlayerController.Object);
			controller.Start ();
		}

		[TearDown ()]
		public void TearDown ()
		{
			controller.Stop ();
		}

		[Test ()]
		public void TestNewPlaylist ()
		{
			string name = "name";
			PlaylistCollectionVM playlistCollectionVM = new PlaylistCollectionVM ();
			controller.SetViewModel (playlistCollectionVM);

			mockDiaklogs.Setup (m => m.QueryMessage (It.IsAny<string> (), It.IsAny<string> (),
				It.IsAny<string> (), It.IsAny<object> ())).Returns (AsyncHelpers.Return (name));

			App.Current.EventsBroker.Publish<AddPlaylistElementEvent> (
				new AddPlaylistElementEvent {
					PlaylistElements = new System.Collections.Generic.List<IPlaylistElement> ()
				}
			);

			mockDiaklogs.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (),
				It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ()), Times.Once ());

			Assert.AreEqual (1, playlistCollectionVM.ViewModels.Count);
			Assert.AreEqual (name, playlistCollectionVM.ViewModels [0].Name);
		}

	}
}
