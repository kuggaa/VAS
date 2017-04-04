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
using System.Collections.Generic;
using System.Dynamic;
using Moq;
using NUnit.Framework;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Services.State;

namespace VAS.Tests.Services
{
	public class DummyScreenState : ScreenState<IViewModel>
	{
		public event EventHandler CreateControllersCalled;

		Mock<IController> mockController;

		public DummyScreenState (Mock<IController> controller) : base ()
		{
			mockController = controller;
		}

		#region implemented abstract members of ScreenState

		protected override void CreateViewModel (dynamic data)
		{
			ViewModel = data.ViewModel;
		}

		public override string Name {
			get {
				return "Dummy";
			}
		}

		protected override void CreateControllers (dynamic data)
		{
			// We "create" new controllers, setup their properties and add them to the Controller list
			var controller = mockController.Object;
			controller.SetViewModel (Mock.Of<IViewModel> ());
			Controllers.Add (controller);
			if (CreateControllersCalled != null) {
				CreateControllersCalled (this, null);
			}

		}

		#endregion


	}

	[TestFixture ()]
	public class TestScreenState
	{
		DummyScreenState screenState;

		Mock<IViewModel> mockViewModel;
		Mock<IController> mockController;
		Mock<IController> mockRootController;
		Mock<IPanel> mockPanel;

		[TestFixtureSetUp]
		public void FixtureSetup ()
		{
			App.Current.ViewLocator = new ViewLocator ();
			App.Current.ControllerLocator = new ControllerLocator ();
		}

		[SetUp]
		public void SetUp ()
		{
			mockController = new Mock<IController> ();
			screenState = new DummyScreenState (mockController);
			mockPanel = new Mock<IPanel> ();

			mockPanel.Setup (p => p.Title).Returns ("DummyPanel");
			mockPanel.Setup (p => p.GetKeyContext ()).Returns (new KeyContext ());

			screenState.Panel = mockPanel.Object;

			mockRootController = new Mock<IController> ();
			screenState.Controllers.Add (mockRootController.Object);
		}

		[Test ()]
		public void TestLoadState ()
		{
			// Arrange
			dynamic data = new ExpandoObject ();
			mockViewModel = new Mock<IViewModel> ();
			mockViewModel.SetupAllProperties ();
			var viewModel = mockViewModel.Object;
			data.ViewModel = viewModel;

			bool createControllersCalled = false;
			screenState.CreateControllersCalled += (sender, e) => createControllersCalled = true;

			mockViewModel.ResetCalls ();
			mockPanel.ResetCalls ();
			mockController.ResetCalls ();
			mockRootController.ResetCalls ();

			// Act
			screenState.LoadState (data);

			// Assert
			mockPanel.Verify (p => p.SetViewModel (viewModel), Times.Once ());
			mockRootController.Verify (p => p.SetViewModel (viewModel), Times.Once ());
			mockController.Verify (p => p.SetViewModel (viewModel), Times.Once ());
			mockController.Verify (p => p.SetViewModel (It.Is<IViewModel> (vm => vm != viewModel)), Times.Once ());
			Assert.IsTrue (createControllersCalled);
			mockRootController.Verify (p => p.Start (), Times.Never ());
			mockController.Verify (p => p.Start (), Times.Never ());
			Assert.AreEqual (2, screenState.Controllers.Count);
		}

		[Test ()]
		public void TestShowState ()
		{
			// Arrange
			TestLoadState ();

			// Act
			screenState.ShowState ();

			// Assert
			mockRootController.Verify (p => p.Start (), Times.Once ());
			mockController.Verify (p => p.Start (), Times.Once ());
		}

		[Test ()]
		public void TestUnfreezeState ()
		{
			// Arrange
			TestLoadState ();

			// Act
			screenState.UnfreezeState ();

			// Assert
			mockRootController.Verify (p => p.Start (), Times.Once ());
			mockController.Verify (p => p.Start (), Times.Once ());
		}

		[Test ()]
		public void TestHideState ()
		{
			// Arrange
			TestShowState ();
			mockViewModel.ResetCalls ();
			mockPanel.ResetCalls ();
			mockController.ResetCalls ();
			mockRootController.ResetCalls ();

			// Act
			screenState.HideState ();

			// Assert
			mockRootController.Verify (p => p.Stop (), Times.Once ());
			mockController.Verify (p => p.Stop (), Times.Once ());
			mockRootController.Verify (p => p.Dispose (), Times.Never ());
			mockController.Verify (p => p.Dispose (), Times.Never ());
			Assert.AreEqual (2, screenState.Controllers.Count);
		}

		[Test ()]
		public void TestFreezeState ()
		{
			// Arrange
			TestShowState ();
			mockViewModel.ResetCalls ();
			mockPanel.ResetCalls ();
			mockController.ResetCalls ();
			mockRootController.ResetCalls ();

			// Act
			screenState.FreezeState ();

			// Assert
			mockRootController.Verify (p => p.Stop (), Times.Once ());
			mockController.Verify (p => p.Stop (), Times.Once ());
			mockRootController.Verify (p => p.Dispose (), Times.Never ());
			mockController.Verify (p => p.Dispose (), Times.Never ());
			Assert.AreEqual (2, screenState.Controllers.Count);
		}

		[Test ()]
		public void TestUnloadState ()
		{
			// Arrange
			TestHideState ();

			// Act
			screenState.UnloadState ();

			// Assert
			mockRootController.Verify (p => p.Dispose (), Times.Once ());
			mockController.Verify (p => p.Dispose (), Times.Once ());
			Assert.AreEqual (0, screenState.Controllers.Count);
		}
	}
}

