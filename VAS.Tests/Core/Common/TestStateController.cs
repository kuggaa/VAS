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
			screenStateMock.Setup (x => x.PreTransition (It.IsAny<object> ())).Returns (Task.Factory.StartNew (() => true));
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
			var mockGUIToolKit = new Mock<IGUIToolkit> ();
			mockGUIToolKit.Setup (x => x.LoadPanel (It.IsAny<IPanel> ()));
			App.Current.GUIToolkit = mockGUIToolKit.Object;

			sc.Register ("newTransition", getScreenStateDummy);

			// Action
			bool moveTransition = await sc.MoveTo ("newTransition", null);

			// Assert
			Assert.IsTrue (moveTransition);
		}

		[Test ()]
		public void TestPushState ()
		{
			// Action
			sc.PushState ("newTransition");

			// Assert
			Assert.DoesNotThrow (() => sc.PopState ("newTransition"));
		}

		[Test ()]
		public void TestPopState ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => sc.PopState ("newTransition"));
		}

		[Test ()]
		public void TestEmptyStateStack ()
		{
			// Arrange
			sc.PushState ("newTransition");
			sc.PushState ("newTransition2");

			// Action
			sc.EmptyStateStack ();

			// Assert
			Assert.Throws<ArgumentOutOfRangeException> (() => sc.PopState ("newTransition"));
		}
	}
}
