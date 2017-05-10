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
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Services.State;

namespace VAS.Tests.Core.Common
{
	[TestFixture]
	public class TestStateController
	{
		StateController sc;
		NavigationEvent lastNavigationEvent;
		bool moveBackAfterNavigation;
		bool moveToAfterNavigation;
		Action navigation;

		[SetUp]
		public void InitializeStateController ()
		{
			sc = new StateController ();
			App.Current.EventsBroker.Subscribe<NavigationEvent> (HandleTransitionEvent);
			lastNavigationEvent = null;
		}

		[TearDown]
		public void Deinitializer ()
		{
			App.Current.EventsBroker.Unsubscribe<NavigationEvent> (HandleTransitionEvent);
		}

		void HandleTransitionEvent (NavigationEvent evt)
		{
			lastNavigationEvent = evt;

			if (moveBackAfterNavigation) {
				moveBackAfterNavigation = false;
				Task.Factory.StartNew (() => sc.MoveBack ());
			} else if (moveToAfterNavigation) {
				moveToAfterNavigation = false;
				Task.Factory.StartNew (() => navigation ());
			}
		}

		IScreenState GetScreenStateDummy (string transitionName)
		{
			var screenStateMock = new Mock<IScreenState> ();
			screenStateMock.Setup (x => x.LoadState (It.IsAny<ExpandoObject> ())).Returns (AsyncHelpers.Return (true));
			screenStateMock.Setup (x => x.ShowState ()).Returns (AsyncHelpers.Return (true));
			screenStateMock.Setup (x => x.UnloadState ()).Returns (AsyncHelpers.Return (true));
			screenStateMock.Setup (x => x.HideState ()).Returns (AsyncHelpers.Return (true));
			screenStateMock.Setup (x => x.FreezeState ()).Returns (AsyncHelpers.Return (true));
			screenStateMock.Setup (x => x.UnfreezeState ()).Returns (AsyncHelpers.Return (true));
			screenStateMock.Setup (x => x.Panel).Returns (new Mock<IPanel> ().Object);
			screenStateMock.Setup (x => x.Name).Returns (transitionName);
			return screenStateMock.Object;
		}

		[Test]
		public void TestRegister ()
		{
			Assert.DoesNotThrow (() => sc.Register ("newTransition", () => GetScreenStateDummy ("newTransition")));
		}

		[Test]
		public void TestUnRegister ()
		{
			// Arrange
			sc.Register ("newTransition", () => GetScreenStateDummy ("newTransition"));

			// Action
			bool obtained = sc.UnRegister ("newTransition");

			// Assert
			Assert.IsTrue (obtained);
		}

		[Test]
		public async void TestUnRegister_CheckDispose ()
		{
			// Arrange
			string transitionName = "newTransition";
			sc.Register (transitionName, () => GetScreenStateDummy ("newTransition"));
			await sc.MoveTo (transitionName, null);

			// Action
			sc.UnRegister (transitionName);

			// Assert
			bool moveTransition = await sc.MoveBack ();
			Assert.IsFalse (moveTransition);
		}

		[Test]
		public void TestUnRegister_WithOverwrittenTransitions ()
		{
			// Arrange
			sc.Register ("newTransition", () => GetScreenStateDummy ("newTransition"));
			sc.Register ("newTransition", () => GetScreenStateDummy ("newTransition"));

			// Action & assert
			Assert.DoesNotThrow (() => sc.UnRegister ("newTransition"));
		}

		[Test]
		public async void TestMoveTo ()
		{
			// Arrange
			sc.Register ("newTransition", () => GetScreenStateDummy ("newTransition"));

			// Action
			bool moveTransition = await sc.MoveTo ("newTransition", null);

			// Assert
			Assert.IsTrue (moveTransition);
			Assert.AreEqual ("newTransition", lastNavigationEvent.Name);
			Assert.IsFalse (lastNavigationEvent.IsModal);
		}


		[Test]
		public async void TestMoveTo_TrueEmptyStack ()
		{
			// Arrange
			sc.Register ("newTransition", () => GetScreenStateDummy ("newTransition"));
			sc.Register ("newTransition2", () => GetScreenStateDummy ("newTransition2"));

			await sc.MoveTo ("newTransition", null);
			await sc.MoveTo ("newTransition2", null);

			// Action
			bool moveTransition = await sc.MoveTo ("newTransition", null, true);

			// Assert
			Assert.IsTrue (moveTransition);
			Assert.AreEqual ("newTransition", lastNavigationEvent.Name);
			Assert.IsFalse (lastNavigationEvent.IsModal);
			bool moveBack = await sc.MoveBack ();
			Assert.IsFalse (moveBack);
		}

		[Test]
		public async void TestSetHomeTransition ()
		{
			// Arrange
			sc.Register ("newTransition", () => GetScreenStateDummy ("newTransition"));

			// Action
			bool moveTransition = await sc.SetHomeTransition ("newTransition", null);

			// Assert
			Assert.IsTrue (moveTransition);
			Assert.AreEqual ("newTransition", lastNavigationEvent.Name);
			Assert.IsFalse (lastNavigationEvent.IsModal);
		}

		[Test]
		public async void TestMoveToModal ()
		{
			// Arrange
			sc.Register ("home", () => GetScreenStateDummy ("home"));
			sc.Register ("newModalTransition", () => GetScreenStateDummy ("newModalTransition"));
			await sc.SetHomeTransition ("home", null);
			// Action
			bool moveTransition = await sc.MoveToModal ("newModalTransition", null);

			// Assert
			Assert.IsTrue (moveTransition);
			Assert.AreEqual ("newModalTransition", lastNavigationEvent.Name);
			Assert.IsTrue (lastNavigationEvent.IsModal);
		}

		[Test]
		public async void TestMoveBack ()
		{
			bool moveTransition;
			// Arrange
			sc.Register ("newTransition", () => GetScreenStateDummy ("newTransition"));
			sc.Register ("newModalTransition", () => GetScreenStateDummy ("newModalTransition"));
			moveTransition = await sc.MoveTo ("newTransition", null);
			Assert.IsTrue (moveTransition);
			Assert.AreEqual ("newTransition", lastNavigationEvent.Name);
			Assert.IsFalse (lastNavigationEvent.IsModal);
			moveTransition = await sc.MoveToModal ("newModalTransition", null);
			Assert.IsTrue (moveTransition);
			Assert.AreEqual ("newModalTransition", lastNavigationEvent.Name);
			Assert.IsTrue (lastNavigationEvent.IsModal);
			moveTransition = await sc.MoveBack ();
			Assert.IsTrue (moveTransition);
			Assert.AreEqual ("newTransition", lastNavigationEvent.Name);
			Assert.IsFalse (lastNavigationEvent.IsModal);
			moveTransition = await sc.MoveBack ();
			Assert.IsFalse (moveTransition);
		}

		[Test]
		public async void TestMoveBackTo ()
		{
			bool moveTransition;
			// Arrange
			sc.Register ("Transition1", () => GetScreenStateDummy ("Transition1"));
			sc.Register ("Transition2", () => GetScreenStateDummy ("Transition2"));
			sc.Register ("newModalTransition", () => GetScreenStateDummy ("newModalTransition"));
			moveTransition = await sc.MoveTo ("Transition1", null);
			Assert.IsTrue (moveTransition);
			Assert.AreEqual ("Transition1", lastNavigationEvent.Name);
			Assert.IsFalse (lastNavigationEvent.IsModal);
			moveTransition = await sc.MoveTo ("Transition2", null);
			Assert.IsTrue (moveTransition);
			Assert.AreEqual ("Transition2", lastNavigationEvent.Name);
			Assert.IsFalse (lastNavigationEvent.IsModal);
			moveTransition = await sc.MoveToModal ("newModalTransition", null);
			Assert.IsTrue (moveTransition);
			Assert.AreEqual ("newModalTransition", lastNavigationEvent.Name);
			Assert.IsTrue (lastNavigationEvent.IsModal);
			moveTransition = await sc.MoveBackTo ("Transition1");
			Assert.IsTrue (moveTransition);
			Assert.AreEqual ("Transition1", lastNavigationEvent.Name);
			Assert.IsFalse (lastNavigationEvent.IsModal);
		}

		[Test]
		public async void TestMoveToHome ()
		{
			bool moveTransition;
			// Arrange
			sc.Register ("Home", () => GetScreenStateDummy ("Home"));
			sc.Register ("Transition1", () => GetScreenStateDummy ("Transition1"));
			sc.Register ("Transition2", () => GetScreenStateDummy ("Transition2"));
			sc.Register ("newModalTransition", () => GetScreenStateDummy ("newModalTransition"));
			await sc.SetHomeTransition ("Home", null);
			moveTransition = await sc.MoveTo ("Transition1", null);
			Assert.IsTrue (moveTransition);
			Assert.AreEqual ("Transition1", lastNavigationEvent.Name);
			Assert.IsFalse (lastNavigationEvent.IsModal);
			moveTransition = await sc.MoveTo ("Transition2", null);
			Assert.IsTrue (moveTransition);
			Assert.AreEqual ("Transition2", lastNavigationEvent.Name);
			Assert.IsFalse (lastNavigationEvent.IsModal);
			moveTransition = await sc.MoveToModal ("newModalTransition", null);
			Assert.IsTrue (moveTransition);
			Assert.AreEqual ("newModalTransition", lastNavigationEvent.Name);
			Assert.IsTrue (lastNavigationEvent.IsModal);
			moveTransition = await sc.MoveToHome ();
			Assert.IsTrue (moveTransition);
			Assert.AreEqual ("Home", lastNavigationEvent.Name);
			Assert.IsFalse (lastNavigationEvent.IsModal);
		}

		[Test]
		public async void TestMoveToAndEmptyStack ()
		{
			// Arrange
			sc.Register ("Home", () => GetScreenStateDummy ("Home"));
			sc.Register ("Transition1", () => GetScreenStateDummy ("Transition1"));
			sc.Register ("Transition2", () => GetScreenStateDummy ("Transition2"));
			sc.Register ("Transition3", () => GetScreenStateDummy ("Transition2"));
			await sc.SetHomeTransition ("Home", null);
			await sc.MoveTo ("Transition1", null);
			await sc.MoveTo ("Transition2", null);

			// Action
			await sc.MoveTo ("Transition3", null, true);
			await sc.MoveBack ();

			// Assert
			Assert.AreEqual (sc.Current, "Home");
		}

		[Test]
		public async void MoveToModal_WaitingForCompletion_FinalStateOriginalOne ()
		{
			// Arrange
			sc.Register ("Home", () => GetScreenStateDummy ("Home"));
			sc.Register ("Wait1", () => GetScreenStateDummy ("Wait1"));
			await sc.SetHomeTransition ("Home", null);

			moveBackAfterNavigation = true; // forces unload after navigation

			// Act
			bool result = await sc.MoveToModal ("Wait1", null, true);

			// Assert
			Assert.IsTrue (result);
			Assert.AreEqual (sc.Current, "Home");
		}

		[Test]
		public async void MoveToModal_NotWaitingForCompletion_FinalStateNewState ()
		{
			// Arrange
			sc.Register ("Home", () => GetScreenStateDummy ("Home"));
			sc.Register ("Wait1", () => GetScreenStateDummy ("Wait1"));
			await sc.SetHomeTransition ("Home", null);

			// Act
			bool result = await sc.MoveToModal ("Wait1", null, false);

			// Assert
			Assert.IsTrue (result);
			Assert.AreEqual (sc.Current, "Wait1");
		}

		[Test]
		public async void MoveToModal_ShowDialogAndMoveBack_StateUnfreezedOk ()
		{
			// Arrange
			var backgroundStateMock = Utils.GetScreenStateMocked ("Home");
			sc.Register ("Home", () => backgroundStateMock.Object);
			sc.Register ("Dialog", () => GetScreenStateDummy ("Dialog"));
			await sc.SetHomeTransition ("Home", null);

			moveBackAfterNavigation = true; // forces unload dialog after navigation

			// Act
			bool result = await sc.MoveToModal ("Dialog", null, true);

			// Assert
			Assert.IsTrue (result);
			Assert.AreEqual (sc.Current, "Home");
			backgroundStateMock.Verify (s => s.FreezeState (), Times.Once ());
			backgroundStateMock.Verify (s => s.HideState (), Times.Never ());
			backgroundStateMock.Verify (s => s.UnloadState (), Times.Never ());
			backgroundStateMock.Verify (s => s.UnfreezeState (), Times.Once ());
			backgroundStateMock.Verify (s => s.ShowState (), Times.Once ());
		}

		[Test]
		public async void MoveToModal_ShowDialogAndMoveBack_WaitForControllerToStart ()
		{
			// Arrange
			DummyController fakeController = new DummyController ();
			var backgroundState = new DummyState ();
			backgroundState.Controllers.Add (fakeController);
			sc.Register ("Home", () => GetScreenStateDummy ("Home"));
			sc.Register ("First", () => backgroundState);
			sc.Register ("Dialog", () => GetScreenStateDummy ("Dialog"));
			await sc.SetHomeTransition ("Home", null);

			await sc.MoveTo ("First", null);
			moveBackAfterNavigation = true; // forces unload dialog after navigation

			// Act
			bool result = await sc.MoveToModal ("Dialog", null, true);

			// Assert
			Assert.IsTrue (fakeController.Started);
			Assert.IsTrue (result);
			Assert.AreEqual (sc.Current, "First");
		}

		[Test]
		public async void MoveToModal_ShowDialogAndMoveToNewStateCleaningStack_Ok ()
		{
			// Arrange
			var backgroundStateMock = Utils.GetScreenStateMocked ("Initial");
			sc.Register ("Initial", () => backgroundStateMock.Object);
			sc.Register ("Dialog", () => GetScreenStateDummy ("Dialog"));
			sc.Register ("New", () => GetScreenStateDummy ("New"));
			await sc.MoveTo ("Initial", null);

			// Act
			await sc.MoveToModal ("Dialog", null);
			bool result = await sc.MoveTo ("New", null, true);

			// Assert
			Assert.IsTrue (result);
			Assert.AreEqual (sc.Current, "New");
			backgroundStateMock.Verify (s => s.FreezeState (), Times.Once ());
			backgroundStateMock.Verify (s => s.HideState (), Times.Once ());
			backgroundStateMock.Verify (s => s.UnloadState (), Times.Once ());
			backgroundStateMock.Verify (s => s.UnfreezeState (), Times.Never ());
			backgroundStateMock.Verify (s => s.ShowState (), Times.Once ());
		}

		[Test]
		public async void MoveToModal_ShowDialogAndMoveToNewState_Ok ()
		{
			// Arrange
			var backgroundStateMock = Utils.GetScreenStateMocked ("Initial");
			sc.Register ("Initial", () => backgroundStateMock.Object);
			sc.Register ("Dialog", () => GetScreenStateDummy ("Dialog"));
			sc.Register ("New", () => GetScreenStateDummy ("New"));
			await sc.MoveTo ("Initial", null);

			// Act
			await sc.MoveToModal ("Dialog", null);
			bool result = await sc.MoveTo ("New", null);

			// Assert
			Assert.IsTrue (result);
			Assert.AreEqual (sc.Current, "New");
			backgroundStateMock.Verify (s => s.FreezeState (), Times.Once ());
			backgroundStateMock.Verify (s => s.HideState (), Times.Never ());
			backgroundStateMock.Verify (s => s.UnloadState (), Times.Never ());
			backgroundStateMock.Verify (s => s.UnfreezeState (), Times.Never ());
			backgroundStateMock.Verify (s => s.ShowState (), Times.Once ());
		}

		[Test]
		public async void MoveToModal_EmptyStackWhenStateFreezed_CompletesDialogTaskOk ()
		{
			// Arrange
			var backgroundStateMock = Utils.GetScreenStateMocked ("First");
			sc.Register ("Home", () => GetScreenStateDummy ("Home"));
			sc.Register ("First", () => backgroundStateMock.Object);
			sc.Register ("Dialog", () => GetScreenStateDummy ("Dialog"));
			sc.Register ("Second", () => GetScreenStateDummy ("Second"));
			await sc.SetHomeTransition ("Home", null);
			await sc.MoveTo ("First", null);


			moveToAfterNavigation = true; // forces unload dialog after navigation
			navigation = (async () => await sc.MoveTo ("Second", null, true));

			// Act
			bool result = await sc.MoveToModal ("Dialog", null, true);

			// Assert
			Assert.IsTrue (result);
			backgroundStateMock.Verify (s => s.FreezeState (), Times.Once ());
			backgroundStateMock.Verify (s => s.HideState (), Times.Once ());
			backgroundStateMock.Verify (s => s.UnloadState (), Times.Once ());
			backgroundStateMock.Verify (s => s.UnfreezeState (), Times.Never ());
			backgroundStateMock.Verify (s => s.ShowState (), Times.Once ());
		}

		[Test]
		public async void MoveTo_HideStateDoesTransition_Ok ()
		{
			// Arrange
			var transition1StateMock = Utils.GetScreenStateMocked ("Transition1");
			sc.Register ("Home", () => GetScreenStateDummy ("Home"));
			sc.Register ("Transition1", () => transition1StateMock.Object);
			sc.Register ("Transition2", () => GetScreenStateDummy ("Transition2"));
			sc.Register ("Transition3", () => GetScreenStateDummy ("Transition3"));
			await sc.SetHomeTransition ("Home", null);

			bool hidden = false;
			transition1StateMock.Setup (x => x.HideState ()).Callback (() => {
				if (!hidden) {
					hidden = true;
					sc.MoveTo ("Transition3", null);
				}
			}).Returns (AsyncHelpers.Return (true));

			// Act
			await sc.MoveTo ("Transition1", null);
			await sc.MoveTo ("Transition2", null);

			// Assert
			Assert.AreEqual ("Transition2", sc.Current);
		}
	}

	class DummyController : IController
	{
		public bool Started { get; set; }

		public void Dispose ()
		{
		}

		public IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return null;
		}

		public void SetViewModel (IViewModel viewModel)
		{
		}

		public void Start ()
		{
			Started = true;
		}

		public void Stop ()
		{
			Started = false;
		}
	}

	class DummyState : VAS.Services.State.ScreenState<IViewModel>
	{
		public override string Name {
			get {
				return "First";
			}
		}

		protected override void CreateViewModel (dynamic data)
		{
		}

		public override Task<bool> LoadState (dynamic data)
		{
			return AsyncHelpers.Return (true);
		}
	}

}
