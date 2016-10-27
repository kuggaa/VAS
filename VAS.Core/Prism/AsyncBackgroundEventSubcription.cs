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

namespace Prism.Events
{
	/// <summary>
	/// Extends <see cref="EventSubscription"/> to invoke the <see cref="EventSubscription.Action"/> delegate in a background thread.
	/// </summary>
	internal class AsyncBackgroundEventSubscription : AsyncEventSubscription
	{
		/// <summary>
		/// Creates a new instance of <see cref="BackgroundEventSubscription"/>.
		/// </summary>
		/// <param name="actionReference">A reference to a delegate of type <see cref="System.Action"/>.</param>
		/// <exception cref="ArgumentNullException">When <paramref name="actionReference"/> or <see paramref="filterReference"/> are <see langword="null" />.</exception>
		/// <exception cref="ArgumentException">When the target of <paramref name="actionReference"/> is not of type <see cref="System.Action"/>.</exception>
		public AsyncBackgroundEventSubscription (IDelegateReference actionReference)
			: base (actionReference)
		{
		}

		/// <summary>
		/// Invokes the specified <see cref="Task"/> in an asynchronous thread by using a <see cref="Task"/>.
		/// </summary>
		/// <param name="action">The action to execute.</param>
		public override Task InvokeAction (Func<Task> action)
		{
			return Task.Run (action);
		}
	}

	/// <summary>
	/// Extends <see cref="EventSubscription{TPayload}"/> to invoke the <see cref="EventSubscription{TPayload}.Action"/> delegate in a background thread.
	/// </summary>
	/// <typeparam name="TPayload">The type to use for the generic <see cref="System.Action{TPayload}"/> and <see cref="Predicate{TPayload}"/> types.</typeparam>
	internal class AsyncBackgroundEventSubscription<TPayload> : AsyncEventSubscription<TPayload>
	{
		/// <summary>
		/// Creates a new instance of <see cref="BackgroundEventSubscription{TPayload}"/>.
		/// </summary>
		/// <param name="actionReference">A reference to a delegate of type <see cref="System.Action{TPayload}"/>.</param>
		/// <param name="filterReference">A reference to a delegate of type <see cref="Predicate{TPayload}"/>.</param>
		/// <exception cref="ArgumentNullException">When <paramref name="actionReference"/> or <see paramref="filterReference"/> are <see langword="null" />.</exception>
		/// <exception cref="ArgumentException">When the target of <paramref name="actionReference"/> is not of type <see cref="System.Action{TPayload}"/>,
		/// or the target of <paramref name="filterReference"/> is not of type <see cref="Predicate{TPayload}"/>.</exception>
		public AsyncBackgroundEventSubscription (IDelegateReference actionReference, IDelegateReference filterReference)
			: base (actionReference, filterReference)
		{
		}

		/// <summary>
		/// Invokes the specified <see cref="Task"/> in an asynchronous thread by using a <see cref="Task"/>.
		/// </summary>
		/// <param name="action">The action to execute.</param>
		/// <param name="argument">The arguments for the action to execute.</param>
		public override Task InvokeAction (Func<TPayload, Task> action, TPayload argument)
		{
			return Task.Run (() => action (argument));
		}
	}
}