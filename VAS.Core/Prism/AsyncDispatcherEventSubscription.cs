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

namespace Prism.Events
{
	///<summary>
	/// Extends <see cref="EventSubscription"/> to invoke the <see cref="EventSubscription.Action"/> delegate
	/// in a specific <see cref="SynchronizationContext"/>.
	///</summary>
	internal class AsyncDispatcherEventSubscription : AsyncEventSubscription
	{
		private readonly SynchronizationContext syncContext;

		///<summary>
		/// Creates a new instance of <see cref="BackgroundEventSubscription"/>.
		///</summary>
		///<param name="actionReference">A reference to a delegate of type <see cref="System.Action{TPayload}"/>.</param>
		///<param name="context">The synchronization context to use for UI thread dispatching.</param>
		///<exception cref="ArgumentNullException">When <paramref name="actionReference"/> or <see paramref="filterReference"/> are <see langword="null" />.</exception>
		///<exception cref="ArgumentException">When the target of <paramref name="actionReference"/> is not of type <see cref="System.Action{TPayload}"/>.</exception>
		public AsyncDispatcherEventSubscription (IDelegateReference actionReference, SynchronizationContext context)
			: base (actionReference)
		{
			syncContext = context;
		}

		/// <summary>
		/// Invokes the specified <see cref="Func{Task}"/> asynchronously in the specified <see cref="SynchronizationContext"/>.
		/// </summary>
		/// <param name="action">The action to execute.</param>
		public override Task InvokeAction (Func<Task> action)
		{
			var tcs = new TaskCompletionSource<bool> ();
			syncContext.Post (async (o) => {
				await action ();
				tcs.SetResult (true);
			}, null);
			return tcs.Task;
		}
	}

	///<summary>
	/// Extends <see cref="EventSubscription{TPayload}"/> to invoke the <see cref="EventSubscription{TPayload}.Action"/> delegate
	/// in a specific <see cref="SynchronizationContext"/>.
	///</summary>
	/// <typeparam name="TPayload">The type to use for the generic <see cref="System.Action{TPayload}"/> and <see cref="Predicate{TPayload}"/> types.</typeparam>
	internal class AsyncDispatcherEventSubscription<TPayload> : AsyncEventSubscription<TPayload>
	{
		private readonly SynchronizationContext syncContext;

		///<summary>
		/// Creates a new instance of <see cref="BackgroundEventSubscription{TPayload}"/>.
		///</summary>
		///<param name="actionReference">A reference to a delegate of type <see cref="System.Action{TPayload}"/>.</param>
		///<param name="filterReference">A reference to a delegate of type <see cref="Predicate{TPayload}"/>.</param>
		///<param name="context">The synchronization context to use for UI thread dispatching.</param>
		///<exception cref="ArgumentNullException">When <paramref name="actionReference"/> or <see paramref="filterReference"/> are <see langword="null" />.</exception>
		///<exception cref="ArgumentException">When the target of <paramref name="actionReference"/> is not of type <see cref="System.Action{TPayload}"/>,
		///or the target of <paramref name="filterReference"/> is not of type <see cref="Predicate{TPayload}"/>.</exception>
		public AsyncDispatcherEventSubscription (IDelegateReference actionReference, IDelegateReference filterReference, SynchronizationContext context)
			: base (actionReference, filterReference)
		{
			syncContext = context;
		}

		/// <summary>
		/// Invokes the specified <see cref="Func{TPayload, Task}"/> asynchronously in the specified <see cref="SynchronizationContext"/>.
		/// </summary>
		/// <param name="action">The action to execute.</param>
		/// <param name="argument">The payload to pass <paramref name="action"/> while invoking it.</param>
		public override Task InvokeAction (Func<TPayload, Task> action, TPayload argument)
		{
			var tcs = new TaskCompletionSource<bool> ();
			syncContext.Post (async (o) => {
				await action ((TPayload)o);
				tcs.SetResult (true);
			}, null);
			return tcs.Task;
		}
	}
}