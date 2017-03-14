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
using System.Linq;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Store.Templates;
using VAS.Core.ViewModel;

namespace VAS.Tests.Services
{
	[TestFixture ()]
	public class TestTemplatesController
	{
		DummyTemplatesController templatesController;
		Mock<IDialogs> mockDialogs;

		[SetUp]
		public void SetUp ()
		{
			templatesController = new DummyTemplatesController ();
			templatesController.Start ();
		}

		[TestFixtureSetUp]
		public void SetUpFixture ()
		{
			mockDialogs = new Mock<IDialogs> ();
			App.Current.Dialogs = mockDialogs.Object;
			mockDialogs.Setup (m => m.QuestionMessage (It.IsAny<string> (), null, It.IsAny<DummyTemplatesController> ())).Returns (AsyncHelpers.Return (true));
			mockDialogs.Setup (m => m.QueryMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (), It.IsAny<DummyTemplatesController> ())).Returns (AsyncHelpers.Return ("Test Team #2_copy"));
		}

		[TearDown]
		public void TearDownOnce ()
		{
			templatesController.Stop ();
		}

		[Test ()]
		public void TestHandleSelectionChanged_LoadedTemplateWithTeamAndPlayer ()
		{
			// Arrange
			Utils.PlayerDummy player = new Utils.PlayerDummy ();
			DummyTeam team = new DummyTeam ();
			team.List.Add (player);
			TeamVM teamVM = new TeamVM { Model = team };

			templatesController.ViewModel.ViewModels.Add (teamVM);

			// Action
			templatesController.ViewModel.SelectionReplace (templatesController.ViewModel.ViewModels);

			// Assert
			Assert.IsNotNull (templatesController.ViewModel.LoadedTemplate,
						   "Loaded template model");
			Assert.IsNotEmpty (templatesController.ViewModel.LoadedTemplate.SubViewModel,
						   "Loaded template subVM");
		}

		[Test ()]
		public void TestHandleSelectionChanged_ClearSelectionWithTeamAndPlayer ()
		{
			// Arrange
			Utils.PlayerDummy player = new Utils.PlayerDummy ();
			DummyTeam team = new DummyTeam ();
			team.List.Add (player);
			TeamVM teamVM = new TeamVM { Model = team };

			templatesController.ViewModel.ViewModels.Add (teamVM);
			templatesController.ViewModel.SelectionReplace (templatesController.ViewModel.ViewModels);

			// Action
			templatesController.ViewModel.Selection.Clear ();

			// Assert
			Assert.IsNull (templatesController.ViewModel.LoadedTemplate.Model,
						   "Loaded template model");
			Assert.IsEmpty (templatesController.ViewModel.LoadedTemplate.SubViewModel,
						   "Loaded template subVM");
		}

		[Test ()]
		public void TestHandleSave_ForcedNameChanged ()
		{
			// Arrange
			Utils.PlayerDummy player = new Utils.PlayerDummy ();
			DummyTeam team = new DummyTeam ();
			team.Name = "Test Team #1";
			team.List.Add (player);
			TeamVM teamVM = new TeamVM { Model = team };

			templatesController.ViewModel.ViewModels.Add (teamVM);

			DummyTeam team2 = team.Clone ();
			string expectedName = "Test Team #2";
			team2.Name = expectedName;

			// Action
			App.Current.EventsBroker.Publish (new UpdateEvent<Team> () {
				Object = team2,
				Force = true
			});

			// Assert
			Assert.AreEqual (templatesController.ViewModel.ViewModels.First (x => x.ID == team.ID).Name,
						   expectedName);
			Assert.IsFalse (templatesController.ViewModel.SaveSensitive);
		}

		[Test ()]
		public void TestHandleSave_NonForcedNameChanged ()
		{
			// Arrange
			Utils.PlayerDummy player = new Utils.PlayerDummy ();
			DummyTeam team = new DummyTeam ();
			team.Name = "Test Team #1";
			team.List.Add (player);
			TeamVM teamVM = new TeamVM { Model = team };

			templatesController.ViewModel.ViewModels.Add (teamVM);

			DummyTeam team2 = team.Clone ();
			string expectedName = "Test Team #2";
			team2.Name = expectedName;

			// Action
			App.Current.EventsBroker.Publish (new UpdateEvent<Team> () {
				Object = team2,
				Force = false
			});

			// Assert
			mockDialogs.Verify (m => m.QuestionMessage (It.IsAny<string> (), null, It.IsAny<DummyTemplatesController> ()), Times.Once ());
			Assert.AreEqual (templatesController.ViewModel.ViewModels.First (x => x.ID == team.ID).Name,
						   expectedName);
			Assert.IsFalse (templatesController.ViewModel.SaveSensitive);
		}

		[Test ()]
		public void TestHandleSave_SaveStatic ()
		{
			// Arrange
			Utils.PlayerDummy player = new Utils.PlayerDummy ();
			DummyTeam team = new DummyTeam ();
			team.Name = "Test Team #1";
			team.List.Add (player);
			TeamVM teamVM = new TeamVM { Model = team };

			templatesController.ViewModel.ViewModels.Add (teamVM);

			DummyTeam team2 = team.Clone ();
			string expectedName = "Test Team #2";
			team2.Name = expectedName;
			team2.Static = true;
			var evt = new UpdateEvent<Team> () {
				Object = team2,
				Force = true
			};

			// Action
			App.Current.EventsBroker.Publish (evt);

			// Assert
			Assert.IsTrue (evt.ReturnValue);
			mockDialogs.Verify (m => m.QueryMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (), It.IsAny<DummyTemplatesController> ()), Times.Once ());
		}
	}
}
