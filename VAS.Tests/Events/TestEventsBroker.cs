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
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Events;

namespace VAS.Tests.Events
{

	[TestFixture]
	public class TestEventsBroker
	{
		EventsBroker eventsBroker;

		[SetUp]
		public void SetUp ()
		{
			eventsBroker = new EventsBroker ();
		}

		[TearDown]
		public void TearDown ()
		{
			SetupClass.Initialize ();
		}

		[Test]
		public void TestPublishAndSubscribe ()
		{
			int count = 0;
			eventsBroker.Subscribe<ReturningValueEvent> ((a) => count++);
			eventsBroker.Publish (new ReturningValueEvent ());
			Assert.AreEqual (1, count);
		}

		[Test]
		public async Task TestPublishAndSubscribeAsync ()
		{
			int count = 0;
			eventsBroker.SubscribeAsync<ReturningValueEvent> ((arg) => {
				return Task.Run (() => {
					Thread.Sleep (20);
					count++;
				});
			});
			await eventsBroker.Publish (new ReturningValueEvent ()).ContinueWith ((arg) => count++);
			Assert.AreEqual (2, count);
		}

		[Test]
		public void TestUnsubscribe ()
		{
			// Arrange
			int count = 0;
			Action<ReturningValueEvent> act = (a) => count++;
			eventsBroker.Subscribe (act);

			// Act
			eventsBroker.Unsubscribe (act);
			eventsBroker.Publish (new ReturningValueEvent ());

			// Assert
			Assert.AreEqual (0, count);
		}

		[Test]
		public async Task TestUnsubscribeAsync ()
		{
			// Arrange
			int count = 0;
			Func<ReturningValueEvent, Task> act = (a) => {
				count++;
				return AsyncHelpers.Return ();
			};
			eventsBroker.SubscribeAsync (act);

			// Act
			eventsBroker.UnsubscribeAsync (act);
			await eventsBroker.Publish (new ReturningValueEvent ());

			// Assert
			Assert.AreEqual (0, count);
		}


		[Test]
		public async Task TestPublishAndSubscribeMixed ()
		{
			// Arrange
			int count = 0;
			Func<ReturningValueEvent, Task> act1 = (a) => {
				count++;
				return AsyncHelpers.Return ();
			};
			Action<ReturningValueEvent> act2 = (a) => {
				count++;
			};

			// Act
			eventsBroker.SubscribeAsync (act1);
			eventsBroker.Subscribe (act2);
			await eventsBroker.Publish (new ReturningValueEvent ());

			// Assert
			Assert.AreEqual (2, count);
		}

		[Test]
		public async Task TestUnsubscribeMixed ()
		{
			// Arrange
			int count = 0;
			Func<ReturningValueEvent, Task> act1 = (a) => {
				count++;
				return AsyncHelpers.Return ();
			};
			Action<ReturningValueEvent> act2 = (a) => {
				count++;
			};
			eventsBroker.SubscribeAsync (act1);
			eventsBroker.Subscribe (act2);

			// Act
			eventsBroker.UnsubscribeAsync (act1);
			eventsBroker.Unsubscribe (act2);
			await eventsBroker.Publish (new ReturningValueEvent ());

			// Assert
			Assert.AreEqual (0, count);
		}

		[Test]
		public async Task Unsuscribe_VirtualMethod_Ok ()
		{
			// Arrange
			BDummyClass classSubscriber = new BDummyClass();
			classSubscriber.Start (eventsBroker);
			classSubscriber.Stop (eventsBroker);

			// Act
			await eventsBroker.Publish (new ReturningValueEvent ());

			// Assert
			Assert.AreEqual (0, classSubscriber.Count);
		}
	}

	class ADummyClass
	{
		public int Count { get; set; }

		protected virtual Task DoSomething (ReturningValueEvent e)
		{
			Count++;
			return AsyncHelpers.Return ();
		}

		public void Start (EventsBroker eventsBtoker)
		{
			eventsBtoker.SubscribeAsync<ReturningValueEvent> (DoSomething);
		}

		public void Stop (EventsBroker eventsBtoker)
		{
			eventsBtoker.UnsubscribeAsync<ReturningValueEvent> (DoSomething);
		}
	}

	class BDummyClass : ADummyClass
	{
	}
}
