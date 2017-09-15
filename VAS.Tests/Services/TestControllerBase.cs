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
using System.Threading.Tasks;
using NUnit.Framework;
using VAS.Core.Events;
using VAS.Core.MVVMC;

namespace VAS.Tests.Services
{
	[TestFixture]
	public class TestControllerBase
	{
		DummyController controller;
		bool managedDisposeCalled;
		bool unmanagedDisposeCalled;
		bool answered;

		[SetUp]
		public void Setup ()
		{
			controller = new DummyController ();
			managedDisposeCalled = false;
			unmanagedDisposeCalled = false;
			answered = false;

			controller.managedDisposeCalled += ManagedDisposeCalled;
			controller.unmanagedDisposeCalled += UnmanagedDisposeCalled;
		}

		[TearDown]
		public async Task TearDown ()
		{
			if (controller != null) {
				controller.managedDisposeCalled -= ManagedDisposeCalled;
				controller.unmanagedDisposeCalled -= UnmanagedDisposeCalled;
				if (controller.Started) {
					await controller.Stop ();
				}
			}
		}

		[Test]
		public async Task TestStart ()
		{
			// Act
			await controller.Start ();

			// Assert
			Assert.IsTrue (controller.Started);
		}

		[Test]
		public async Task TestStartTwice ()
		{
			// Arrange
			await controller.Start ();

			// Act & Assert
			Assert.ThrowsAsync<InvalidOperationException> (async () => await controller.Start ());
		}

		[Test]
		public async Task TestStop ()
		{
			// Arrange
			await controller.Start ();

			// Act
			await controller.Stop ();

			// Assert
			Assert.IsFalse (controller.Started);
		}

		[Test]
		public async Task TestStopTwice ()
		{
			// Arrange
			await controller.Start ();
			await controller.Stop ();

			// Act & Assert
			Assert.ThrowsAsync<InvalidOperationException> (controller.Stop);
		}

		[Test]
		public void TestStopWithoutStart ()
		{
			// Arrange & Assume
			Assert.IsFalse (controller.Started);
			// Act & Assert
			Assert.ThrowsAsync <InvalidOperationException> (async () => await controller.Stop());
		}

		[Test]
		public async Task TestDispose ()
		{
			// Arrange
			await controller.Start ();

			// Act
			controller.Dispose ();

			// Assert
			Assert.IsTrue (managedDisposeCalled);
			Assert.IsTrue (unmanagedDisposeCalled);
			Assert.IsFalse (controller.Started);
		}

		[Test]
		public void TestDisposeStopped ()
		{
			// Arrange & Assume
			Assert.IsFalse (controller.Started);

			// Act & Assert
			// It doesn't try to call Stop if it's already stopped
			Assert.DoesNotThrow (controller.Dispose);
			Assert.IsTrue (managedDisposeCalled);
			Assert.IsTrue (unmanagedDisposeCalled);
		}

		[Test]
		public async Task SetViewModel_ControllerStarted_ExceptionThrown ()
		{
			var controller = new ControllerBase<ViewModelBase> ();
			controller.SetViewModel (new ViewModelBase ());
			await controller.Start ();

			Assert.Throws<InvalidOperationException> (() => { controller.ViewModel = new ViewModelBase (); });
		}

		[Test]
		public async Task SetViewModel2_ControllerStarted_ExceptionThrown ()
		{
			var controller = new ControllerBase<ViewModelBase> ();
			controller.SetViewModel (new ViewModelBase ());
			await controller.Start ();

			Assert.Throws<InvalidOperationException> (() => controller.SetViewModel (new ViewModelBase ()));
		}

		[Test]
		public void Start_NoViewModel_ExceptionThrown ()
		{
			var controller = new ControllerBase<ViewModelBase> ();
			Assert.ThrowsAsync<InvalidOperationException> (async () => await controller.Start ());
		}

		[Test]
		public void Stop_NoViewModel_ExceptionThrown ()
		{
			var controller = new ControllerBase<ViewModelBase> ();
			Assert.ThrowsAsync<InvalidOperationException> (async () => await controller.Stop ());
		}

		[Test]
		public void TestDisposeGC ()
		{
			// Arrange
			GC.Collect ();
			GC.WaitForPendingFinalizers ();
			managedDisposeCalled = false;
			unmanagedDisposeCalled = false;

			// Act
			controller = null;
			GC.Collect ();
			GC.WaitForPendingFinalizers ();

			// Assert
			Assert.IsTrue (unmanagedDisposeCalled);
			Assert.IsFalse (managedDisposeCalled);
		}

		[Test]
		[Ignore ("This test deadlock in the CI server")]
		public async Task TestPublishBeforeGC ()
		{
			App.Current.EventsBroker = new EventsBroker ();

			// We lose the old controller and garbage collect it.
			controller = null;
			GC.Collect ();
			GC.WaitForPendingFinalizers ();
			unmanagedDisposeCalled = false;
			managedDisposeCalled = false;

			// We create a new controller and add a WeakReference to it
			WeakReference controllerRef = await CreateAndLoseController ();
			Assert.IsNotNull (controllerRef.Target);

			// Act
			await App.Current.EventsBroker.Publish<NewTagEvent> ();

			// Assert
			Assert.IsFalse (unmanagedDisposeCalled);
			Assert.IsFalse (managedDisposeCalled);
			Assert.IsTrue (answered);
		}

		[Test]
		[Ignore ("This test deadlock in the CI server")]
		public async Task TestPublishAfterGC ()
		{
			App.Current.EventsBroker = new EventsBroker ();

			// We lose the old controller and garbage collect it.
			controller = null;
			GC.Collect ();
			GC.WaitForPendingFinalizers ();
			unmanagedDisposeCalled = false;
			managedDisposeCalled = false;

			// We create a new controller and add a WeakReference to it
			WeakReference controllerRef = await CreateAndLoseController ();
			Assert.IsNotNull (controllerRef.Target);

			do {
				GC.Collect ();
				GC.WaitForPendingFinalizers ();
			} while (controllerRef.Target != null);

			// Act
			// Publish an event to the collected controller
			await App.Current.EventsBroker.Publish<NewTagEvent> ();

			// Assert
			Assert.IsTrue (unmanagedDisposeCalled);
			Assert.IsFalse (managedDisposeCalled);
			Assert.IsFalse (answered);
		}

		/// <summary>
		/// This helper method is used to avoid the JIT optimization that extend the lifetime of an object
		/// until the end of its scope
		/// </summary>
		/// <returns>A weak reference to the lost controller.</returns>
		async Task<WeakReference> CreateAndLoseController ()
		{
			AnswererDummyController controllerLocal = new AnswererDummyController ();
			controllerLocal.answer += (sender, e) => answered = true;
			controllerLocal.managedDisposeCalled += ManagedDisposeCalled;
			controllerLocal.unmanagedDisposeCalled += UnmanagedDisposeCalled;
			await controllerLocal.Start ();
			WeakReference controllerRef = new WeakReference (controllerLocal);

			controllerLocal = null;
			return controllerRef;
		}

		void ManagedDisposeCalled (object sender, EventArgs e)
		{
			managedDisposeCalled = true;
		}

		void UnmanagedDisposeCalled (object sender, EventArgs e)
		{
			unmanagedDisposeCalled = true;
		}
	}

	/// <summary>
	/// Dummy controller that is subscribed to an event and answers with a different event.
	/// </summary>
	public class AnswererDummyController : DummyController
	{
		public event EventHandler answer;

		public override async Task Start ()
		{
			await base.Start ();
			App.Current.EventsBroker.Subscribe<NewTagEvent> (HandleAction);
		}

		public override async Task Stop ()
		{
			await base.Stop ();
			App.Current.EventsBroker.Unsubscribe<NewTagEvent> (HandleAction);
		}

		void HandleAction (NewTagEvent obj)
		{
			if (answer != null) {
				answer (this, new EventArgs ());
			}
		}
	}
}
