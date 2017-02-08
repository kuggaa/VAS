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
using System.Threading;
using Moq;
using NUnit.Framework;
using VAS.Core.Events;

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
		public void TearDown ()
		{
			if (controller != null) {
				controller.managedDisposeCalled -= ManagedDisposeCalled;
				controller.unmanagedDisposeCalled -= UnmanagedDisposeCalled;
				if (controller.Started) {
					controller.Stop ();
				}
			}
		}

		[Test]
		public void TestStart ()
		{
			// Act
			controller.Start ();

			// Assert
			Assert.IsTrue (controller.Started);
		}

		[Test]
		public void TestStartTwice ()
		{
			// Arrange
			controller.Start ();

			// Act & Assert
			Assert.Throws<InvalidOperationException> (controller.Start);
		}

		[Test]
		public void TestStop ()
		{
			// Arrange
			controller.Start ();

			// Act
			controller.Stop ();

			// Assert
			Assert.IsFalse (controller.Started);
		}

		[Test]
		public void TestStopTwice ()
		{
			// Arrange
			controller.Start ();
			controller.Stop ();

			// Act & Assert
			Assert.Throws<InvalidOperationException> (controller.Stop);
		}

		[Test]
		public void TestStopWithoutStart ()
		{
			// Arrange & Assume
			Assert.IsFalse (controller.Started);
			// Act & Assert
			Assert.Throws<InvalidOperationException> (controller.Stop);
		}

		[Test]
		public void TestDispose ()
		{
			// Arrange
			controller.Start ();

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
		public void TestPublishBeforeGC ()
		{
			App.Current.EventsBroker = new EventsBroker ();

			// We lose the old controller and garbage collect it.
			controller = null;
			GC.Collect ();
			GC.WaitForPendingFinalizers ();
			unmanagedDisposeCalled = false;
			managedDisposeCalled = false;

			// We create a new controller and add a WeakReference to it
			WeakReference controllerRef = CreateAndLoseController ();
			Assert.IsNotNull (controllerRef.Target);

			// Act
			App.Current.EventsBroker.Publish<NewTagEvent> ();

			// Assert
			Assert.IsFalse (unmanagedDisposeCalled);
			Assert.IsFalse (managedDisposeCalled);
			Assert.IsTrue (answered);
		}

		[Test]
		public void TestPublishAfterGC ()
		{
			App.Current.EventsBroker = new EventsBroker ();

			// We lose the old controller and garbage collect it.
			controller = null;
			GC.Collect ();
			GC.WaitForPendingFinalizers ();
			unmanagedDisposeCalled = false;
			managedDisposeCalled = false;

			// We create a new controller and add a WeakReference to it
			WeakReference controllerRef = CreateAndLoseController ();
			Assert.IsNotNull (controllerRef.Target);

			GC.Collect ();
			GC.WaitForPendingFinalizers ();
			Assert.IsNull (controllerRef.Target);

			// Act
			// Publish an event to the collected controller
			App.Current.EventsBroker.Publish<NewTagEvent> ();

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
		WeakReference CreateAndLoseController ()
		{
			AnswererDummyController controllerLocal = new AnswererDummyController ();
			controllerLocal.answer += (sender, e) => answered = true;
			controllerLocal.managedDisposeCalled += ManagedDisposeCalled;
			controllerLocal.unmanagedDisposeCalled += UnmanagedDisposeCalled;
			controllerLocal.Start ();
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

		public override void Start ()
		{
			base.Start ();
			App.Current.EventsBroker.Subscribe<NewTagEvent> (HandleAction);
		}

		public override void Stop ()
		{
			base.Stop ();
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
