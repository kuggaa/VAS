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
using System.Threading.Tasks;
using Prism.Events;

namespace VAS.Core.Events
{
	/**
	 * Wrapper of the Prism Event Aggregator
	 * */
	public enum ThreadMethod
	{
		PublisherThread = ThreadOption.PublisherThread,
		UIThread = ThreadOption.UIThread,
		BackgroundThread = ThreadOption.BackgroundThread
	}

	public class EventToken
	{
		internal SubscriptionToken Token { get; set; }
	}

	public class EventsBroker
	{

		IEventAggregator Current {
			get;
			set;
		} = new EventAggregator ();

		/// <summary>
		/// Publish a new empty event.
		/// </summary>
		/// <typeparam name="TEvent">The type of the event.</typeparam>
		public Task Publish<TEvent> ()
		{
			return Publish (default (TEvent));
		}

		/// <summary>
		/// Publish a new event.
		/// </summary>
		/// <param name="event">Event.</param>
		/// <typeparam name="TEvent">The type of the event.</typeparam>
		public Task Publish<TEvent> (TEvent @event)
		{
			return GetEvent<TEvent> ().Publish (@event);
		}

		/// <summary>
		/// Subscribes to an event synchronously.
		/// </summary>
		/// <param name="action">Callback function called when the event is raised.</param>
		/// <param name="threadOption">Thread option.</param>
		/// <param name="keepSubscriberReferenceAlive">If set to <c>true</c> keep subscriber reference alive.</param>
		/// <param name="filter">Filter.</param>
		/// <typeparam name="TEvent">The type of the event.</typeparam>
		public EventToken Subscribe<TEvent> (Action<TEvent> action, ThreadMethod threadOption = ThreadMethod.PublisherThread, bool keepSubscriberReferenceAlive = false, Predicate<TEvent> filter = null)
		{
			return new EventToken {
				Token = GetEvent<TEvent> ().Subscribe (action, (ThreadOption)threadOption, keepSubscriberReferenceAlive, filter)
			};
		}

		/// <summary>
		/// Subscribes to an event asynchronously.
		/// </summary>
		/// <param name="action">Callback function called when the event is raised.</param>
		/// <param name="threadOption">Thread option.</param>
		/// <param name="keepSubscriberReferenceAlive">If set to <c>true</c> keep subscriber reference alive.</param>
		/// <param name="filter">Filter.</param>
		/// <typeparam name="TEvent">The type of the event.</typeparam>
		public EventToken SubscribeAsync<TEvent> (Func<TEvent, Task> action, ThreadMethod threadOption = ThreadMethod.PublisherThread, bool keepSubscriberReferenceAlive = false, Predicate<TEvent> filter = null)
		{
			return new EventToken {
				Token = GetEvent<TEvent> ().Subscribe (action, (ThreadOption)threadOption, keepSubscriberReferenceAlive, filter)
			};
		}

		/// <summary>
		/// Unsubscribe to an event using the specified token.
		/// </summary>
		/// <param name="eventToken">Event token.</param>
		/// <typeparam name="TEvent">The type of the event.</typeparam>
		public void Unsubscribe<TEvent> (EventToken eventToken)
		{
			if (eventToken != null) {
				GetEvent<TEvent> ().Unsubscribe (eventToken.Token);
			}
		}

		/// <summary>
		/// Unsubscribe to an event using the delegate.
		/// </summary>
		/// <param name="subscriber">Subscriber.</param>
		/// <typeparam name="TEvent">The type of the event.</typeparam>
		public void Unsubscribe<TEvent> (Action<TEvent> subscriber)
		{
			if (subscriber != null) {
				GetEvent<TEvent> ().Unsubscribe (subscriber);
			}
		}

		/// <summary>
		/// Unsubscribe to an event using the delegate.
		/// </summary>
		/// <param name="subscriber">Subscriber.</param>
		/// <typeparam name="TEvent">The type of the event.</typeparam>
		public void UnsubscribeAsync<TEvent> (Func<TEvent, Task> subscriber)
		{
			if (subscriber != null) {
				GetEvent<TEvent> ().Unsubscribe (subscriber);
			}
		}

		//this method avoids refactoring return value in all calls
		public bool EmitCloseOpenedProject (object sender)
		{
			CloseOpenedProjectEvent e = new CloseOpenedProjectEvent {
				Sender = sender
			};
			App.Current.EventsBroker.Publish<CloseOpenedProjectEvent> (e);
			return e.ReturnValue;
		}

		//wrapper function to avoid the use of a Prism reference in Tests for eventreset
		protected void ResetEventsBroker ()
		{
			Current = null;
		}

		PubSubEvent<TEvent> GetEvent<TEvent> ()
		{
			return Current.GetEvent<PubSubEvent<TEvent>> ();
		}

	}
}

