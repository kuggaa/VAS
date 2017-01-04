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
using NUnit.Framework;

namespace VAS.Tests.Services
{
	[TestFixture ()]
	public class TestTemplatesController
	{
		DummyTemplatesController templatesController;

		[SetUp]
		public void SetUp ()
		{
			templatesController = new DummyTemplatesController ();
		}

		[Test ()]
		public void TestHandleSelectionChanged_LoadedTemplateWithTeamAndPlayer ()
		{
			// Arrange
			Utils.PlayerDummy player = new Utils.PlayerDummy ();
			DummyTeam team = new DummyTeam ();
			team.List.Add (player);
			DummyTeamVM teamVM = new DummyTeamVM () { Model = team };

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
			DummyTeamVM teamVM = new DummyTeamVM () { Model = team };

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
	}
}
