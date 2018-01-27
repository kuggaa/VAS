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
using NUnit.Framework;
using VAS.Core;
using VAS.Core.MVVMC;
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

		[OneTimeSetUp]
		public void OneTimeSetup ()
		{
			SetupClass.SetUp ();
			App.Current.StateController = new StateController ();
			App.Current.StateController.Register (QuickEditorState.NAME, () => new QuickEditorState ());
			App.Current.ControllerLocator = new ControllerLocator ();
			VASServicesInit.ScanController ();
			App.Current.ViewLocator = new ViewLocator ();
			App.Current.ViewLocator.Register (QuickEditorState.NAME, typeof (DummyPanel));
		}

		[Test]
		public async Task When_opened_with_no_file_ItShould_show_the_welcome_message ()
		{
			await Init ();

			Assert.IsTrue (viewModel.WelcomeVisible);
			Assert.IsNotNull (viewModel.WelcomeMessage);
			Assert.IsFalse (viewModel.VideoEditorVisible);
			Assert.IsFalse (viewModel.DrawingToolVisible);
		}

		async Task Init (object parameters = null)
		{
			await App.Current.StateController.MoveTo (QuickEditorState.NAME, parameters);
			state = App.Current.StateController.Current as QuickEditorState;
			viewModel = state.ViewModel;
		}
	}
}
