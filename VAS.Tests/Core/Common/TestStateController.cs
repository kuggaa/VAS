//
//  Copyright (C) 2016 FLUENDO S.A.
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
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Interfaces.GUI;

namespace VAS.Tests.Core.Common
{
	[TestFixture ()]
	public class TestStateController
	{
		StateController sc;
		Mock<IScreenState> screenStateMock;

		[SetUp ()]
		public void InitializeStateController ()
		{
			sc = new StateController ();
			screenStateMock = new Mock<IScreenState> ();
			screenStateMock.Setup (x => x.PreTransition (It.IsAny<ExpandoObject> ())).Returns (AsyncHelpers.Return (true));
			screenStateMock.Setup (x => x.PostTransition ()).Returns (AsyncHelpers.Return (true));
			screenStateMock.Setup (x => x.Panel).Returns (new Mock<IPanel> ().Object);
		}

		IScreenState getScreenStateDummy ()
		{
			return screenStateMock.Object;
		}

		[Test ()]
		public void TestRegister ()
		{
			Assert.DoesNotThrow (() => sc.Register ("newTransition", getScreenStateDummy));
		}

		[Test ()]
		public void TestUnRegister ()
		{
			// Arrange
			sc.Register ("newTransition", getScreenStateDummy);

			// Action
			bool obtained = sc.UnRegister ("newTransition");

			// Assert
			Assert.IsTrue (obtained);
		}

		[Test ()]
		public void TestUnRegister_WithOverwrittenTransitions ()
		{
			// Arrange
			sc.Register ("newTransition", getScreenStateDummy);
			sc.Register ("newTransition", getScreenStateDummy);

			// Action & assert
			Assert.DoesNotThrow (() => sc.UnRegister ("newTransition"));
		}

		[Test ()]
		public async void TestMoveTo ()
		{
			// Arrange
			var mockGUIToolKit = new Mock<INavigation> ();
			mockGUIToolKit.Setup (x => x.LoadNavigationPanel (It.IsAny<IPanel> ())).Returns (AsyncHelpers.Return (true));
			App.Current.Navigation = mockGUIToolKit.Object;

			sc.Register ("newTransition", getScreenStateDummy);

			// Action
			bool moveTransition = await sc.MoveTo ("newTransition", null);

			// Assert
			Assert.IsTrue (moveTransition);
		}

		[Test ()]
		public async void TestMoveToModal ()
		{
			// Arrange
			var mockGUIToolKit = new Mock<INavigation> ();
			mockGUIToolKit.Setup (x => x.LoadModalPanel (It.IsAny<IPanel> (), It.IsAny<IPanel> ())).Returns (AsyncHelpers.Return (true));
			App.Current.Navigation = mockGUIToolKit.Object;
			sc.Register ("home", getScreenStateDummy);
			sc.Register ("newModalTransition", getScreenStateDummy);
			await sc.SetHomeTransition ("home", null);
			// Action
			bool moveTransition = await sc.MoveToModal ("newModalTransition", null);

			// Assert
			Assert.IsTrue (moveTransition);
		}

		[Test ()]
		public async void TestMoveBack ()
		{
			bool moveTransition;
			// Arrange
			var mockGUIToolKit = new Mock<INavigation> ();
			mockGUIToolKit.Setup (x => x.LoadNavigationPanel (It.IsAny<IPanel> ())).Returns (AsyncHelpers.Return (true));
			mockGUIToolKit.Setup (x => x.LoadModalPanel (It.IsAny<IPanel> (), It.IsAny<IPanel> ())).Returns (AsyncHelpers.Return (true));
			mockGUIToolKit.Setup (x => x.RemoveModalWindow (It.IsAny<IPanel> ())).Returns (AsyncHelpers.Return (true));
			App.Current.Navigation = mockGUIToolKit.Object;
			sc.Register ("newTransition", getScreenStateDummy);
			sc.Register ("newModalTransition", getScreenStateDummy);
			moveTransition = await sc.MoveTo ("newTransition", null);
			Assert.IsTrue (moveTransition);
			moveTransition = await sc.MoveToModal ("newModalTransition", null);
			Assert.IsTrue (moveTransition);
			moveTransition = await sc.MoveBack ();
			Assert.IsTrue (moveTransition);
			moveTransition = await sc.MoveBack ();
			Assert.IsFalse (moveTransition);
		}

		[Test ()]
		public async void TestMoveBackTo ()
		{
			bool moveTransition;
			// Arrange
			var mockGUIToolKit = new Mock<INavigation> ();
			mockGUIToolKit.Setup (x => x.LoadNavigationPanel (It.IsAny<IPanel> ())).Returns (AsyncHelpers.Return (true));
			mockGUIToolKit.Setup (x => x.LoadModalPanel (It.IsAny<IPanel> (), It.IsAny<IPanel> ())).Returns (AsyncHelpers.Return (true));
			mockGUIToolKit.Setup (x => x.RemoveModalWindow (It.IsAny<IPanel> ())).Returns (AsyncHelpers.Return (true));
			App.Current.Navigation = mockGUIToolKit.Object;
			sc.Register ("Transition1", getScreenStateDummy);
			sc.Register ("Transition2", getScreenStateDummy);
			sc.Register ("newModalTransition", getScreenStateDummy);
			moveTransition = await sc.MoveTo ("Transition1", null);
			Assert.IsTrue (moveTransition);
			moveTransition = await sc.MoveTo ("Transition2", null);
			Assert.IsTrue (moveTransition);
			moveTransition = await sc.MoveToModal ("newModalTransition", null);
			Assert.IsTrue (moveTransition);
			moveTransition = await sc.MoveBackTo ("Transition1");
			Assert.IsTrue (moveTransition);
		}

		[Test ()]
		public async void TestMoveToHome ()
		{
			bool moveTransition;
			// Arrange
			var mockGUIToolKit = new Mock<INavigation> ();
			mockGUIToolKit.Setup (x => x.LoadNavigationPanel (It.IsAny<IPanel> ())).Returns (AsyncHelpers.Return (true));
			mockGUIToolKit.Setup (x => x.LoadModalPanel (It.IsAny<IPanel> (), It.IsAny<IPanel> ())).Returns (AsyncHelpers.Return (true));
			mockGUIToolKit.Setup (x => x.RemoveModalWindow (It.IsAny<IPanel> ())).Returns (AsyncHelpers.Return (true));
			App.Current.Navigation = mockGUIToolKit.Object;
			sc.Register ("Home", getScreenStateDummy);
			sc.Register ("Transition1", getScreenStateDummy);
			sc.Register ("Transition2", getScreenStateDummy);
			sc.Register ("newModalTransition", getScreenStateDummy);
			await sc.SetHomeTransition ("Home", null);
			moveTransition = await sc.MoveTo ("Transition1", null);
			Assert.IsTrue (moveTransition);
			moveTransition = await sc.MoveTo ("Transition2", null);
			Assert.IsTrue (moveTransition);
			moveTransition = await sc.MoveToModal ("newModalTransition", null);
			Assert.IsTrue (moveTransition);
			moveTransition = await sc.MoveToHome ();
			Assert.IsTrue (moveTransition);
		}
	}
}
