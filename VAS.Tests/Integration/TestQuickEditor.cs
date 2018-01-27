//
//  Copyright (C) 2018 Fluendo S.A.
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
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services;
using VAS.Services.State;
using VAS.Services.ViewModel;

namespace VAS.Tests.Integration
{
	[TestFixture]
	public class Given_the_quick_editor
	{
		QuickEditorState state;
		QuickEditorVM viewModel;
		Mock<IMultimediaToolkit> mtkMock;
		Mock<IDialogs> dialogsMock;

		[OneTimeSetUp]
		public void OneTimeSetup ()
		{
			SetupClass.SetUp ();
			App.Current.ControllerLocator = new ControllerLocator ();
			VASServicesInit.ScanController ();
			App.Current.ViewLocator = new ViewLocator ();
			App.Current.ViewLocator.Register (QuickEditorState.NAME, typeof (DummyPanel));
			App.Current.HotkeysService = new HotkeysService ();
			App.Current.StateController = new StateController ();
			App.Current.StateController.Register ("HOME", () => Utils.GetScreenStateMocked ("HOME").Object);
			App.Current.StateController.SetHomeTransition ("HOME", null);
			App.Current.StateController.Register (QuickEditorState.NAME, () => new QuickEditorState ());
			GeneralUIHotkeys.RegisterDefaultHotkeys ();
			PlaybackHotkeys.RegisterDefaultHotkeys ();
			DrawingToolHotkeys.RegisterDefaultHotkeys ();

			mtkMock = new Mock<IMultimediaToolkit> ();
			App.Current.MultimediaToolkit = mtkMock.Object;
			mtkMock.Setup (x => x.GetPlayer ()).Returns (Mock.Of<IVideoPlayer> ());

			dialogsMock = new Mock<IDialogs> ();
			App.Current.Dialogs = dialogsMock.Object;
			dialogsMock.Setup (x => x.OpenMediaFile (It.IsAny<object> ())).Returns (Utils.CreateMediaFile ());
		}

		[TearDown]
		public void TearDown ()
		{
			dialogsMock.ResetCalls ();
			App.Current.StateController.MoveToHome (true);
		}

		[Test]
		public async Task When_opened_with_no_file_ItShould_show_the_welcome_message ()
		{
			await Init ();

			CheckWelcomeVisible ();
		}

		[Test]
		public async Task When_opened_with_a_file_ItShould_show_the_video_editor_with_the_file_loaded ()
		{
			await Init (new MediaFileVM { Model = Utils.CreateMediaFile () });

			CheckEditorVisibleAndFileLoaded ();
		}

		[Test]
		public async Task When_open_button_is_clicked_and_file_is_choosen_ItShould_load_the_new_file ()
		{
			await Init ();
			await viewModel.ChooseFileCommand.ExecuteAsync ();

			dialogsMock.Verify (s => s.OpenMediaFile (null), Times.Once ());
			CheckEditorVisibleAndFileLoaded ();
		}

		[Test]
		public async Task When_open_button_is_clicked_and_no_file_is_choosen_ItShould_show_the_welcome_message ()
		{
			dialogsMock.Setup (x => x.OpenMediaFile (It.IsAny<object> ())).Returns<MediaFile> (null);
			await Init ();
			dialogsMock.Verify (s => s.OpenMediaFile (null), Times.Never ());
			await viewModel.ChooseFileCommand.ExecuteAsync ();

			dialogsMock.Verify (s => s.OpenMediaFile (null), Times.Once ());
			CheckWelcomeVisible ();
		}

		async Task Init (object parameters = null)
		{
			await App.Current.StateController.MoveTo (QuickEditorState.NAME, parameters);
			state = App.Current.StateController.Current as QuickEditorState;
			viewModel = state.ViewModel;
		}

		void CheckEditorVisibleAndFileLoaded ()
		{
			Assert.IsFalse (viewModel.WelcomeVisible);
			Assert.IsTrue (viewModel.VideoEditorVisible);
			Assert.IsFalse (viewModel.DrawingToolVisible);
			Assert.IsNotNull (viewModel.LoadedEvent.Model);
			Assert.AreEqual (viewModel.LoadedEvent, viewModel.VideoPlayer.LoadedElement);
			Assert.AreEqual (viewModel.VideoFile.Model, viewModel.LoadedEvent.FileSet [0]);
			Assert.IsFalse (viewModel.VideoPlayer.Playing);
		}

		void CheckWelcomeVisible ()
		{
			Assert.IsTrue (viewModel.WelcomeVisible);
			Assert.IsNotNull (viewModel.WelcomeMessage);
			Assert.IsFalse (viewModel.VideoEditorVisible);
			Assert.IsFalse (viewModel.DrawingToolVisible);
		}
	}
}
