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
using Prism.Events;

namespace VAS.Core.Events
{
	/**
	 * Wrapper of the Prism Event Aggregator
	 * */
	public enum ThreadMethod
	{
		PublisherThread = Prism.Events.ThreadOption.PublisherThread,
		UIThread = Prism.Events.ThreadOption.UIThread,
		BackgroundThread = Prism.Events.ThreadOption.BackgroundThread
	}

	public class EventToken
	{
		internal SubscriptionToken Token { get; set; }
	}

	public class EventsBroker
	{
		private static IEventAggregator _current;

		public IEventAggregator Current {
			get {
				return _current ?? (_current = new EventAggregator ());
			}
			private set { 
				_current = value;
			}
		}

		private PubSubEvent<TEvent> GetEvent<TEvent> ()
		{
			return Current.GetEvent<PubSubEvent<TEvent>> ();
		}

		public void Publish<TEvent> ()
		{
			Publish<TEvent> (default(TEvent));
		}

		public void Publish<TEvent> (TEvent @event)
		{
			GetEvent<TEvent> ().Publish (@event);
		}

		public EventToken Subscribe<TEvent> (Action action, ThreadMethod threadOption = ThreadMethod.PublisherThread, bool keepSubscriberReferenceAlive = false)
		{
			return Subscribe<TEvent> (e => action (), threadOption, keepSubscriberReferenceAlive);
		}

		public EventToken Subscribe<TEvent> (Action<TEvent> action, ThreadMethod threadOption = ThreadMethod.PublisherThread, bool keepSubscriberReferenceAlive = false, Predicate<TEvent> filter = null)
		{
			return new EventToken {
				Token = GetEvent<TEvent> ().Subscribe (action, (ThreadOption)threadOption, keepSubscriberReferenceAlive, filter)
			};
		}

		public void Unsubscribe<TEvent> (EventToken eventToken)
		{
			if (eventToken != null) {
				GetEvent<TEvent> ().Unsubscribe (eventToken.Token);
			}
		}

		public void Unsubscribe<TEvent> (Action<TEvent> subscriber)
		{
			if (subscriber != null) {
				GetEvent<TEvent> ().Unsubscribe (subscriber);
			}
		}

		//wrapper function to avoid the use of a Prism reference in Tests for eventreset
		protected void ResetEventsBroker ()
		{
			Current = null;
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
	}
}

