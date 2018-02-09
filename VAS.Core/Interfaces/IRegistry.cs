//
//  Copyright (C) 2018 Fluendo S.A.
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
using VAS.Core.Common;

namespace VAS.Core.Interfaces
{
	/// <summary>
	/// Interface to Register types or instances by interface, and to retrieve new instances or default instances given
	/// an interface
	/// </summary>
	public interface IRegistry
	{
		/// <summary>
		/// Register a new element type for a given interface with a priority.
		/// </summary>
		/// <param name="priority">Priority.</param>
		/// <typeparam name="TInterface">The interface to register the element.</typeparam>
		/// <typeparam name="TType">The type of the registered element.</typeparam>
		void Register<TInterface, TType> (int priority = 0);

		/// <summary>
		/// Register a new element type for a given interface with a priority.
		/// </summary>
		/// <param name="type">The type of the registered element.</param>
		/// <param name="priority">Priority.</param>
		/// <typeparam name="TInterface">The interface to register the element.</typeparam>
		void Register<TInterface> (Type type, int priority = 0);

		/// <summary>
		/// Register the specified interfac, type and priority in runtime.
		/// </summary>
		/// <param name="interfac">Interfac.</param>
		/// <param name="type">Type.</param>
		/// <param name="priority">Priority.</param>
		void Register (Type interfac, Type type, int priority = 0);

		/// <summary>
		/// register a new element with an instance object for a given interface with a priority
		/// </summary>
		/// <param name="instance">The instance of the registered interface.</param>
		/// <param name="priority">Priority.</param>
		/// <typeparam name="TInterface">The interface to register the element.</typeparam>
		void Register<TInterface> (object instance, int priority = 0);

		/// <summary>
		/// register a new element with an instance object for a given interface with a priority in runtime
		/// </summary>
		/// <param name="interfac">Interfac.</param>
		/// <param name="instance">The instance of the registered interface.</param>
		/// <param name="priority">Priority.</param>
		void Register (Type interfac, object instance, int priority = 0);

		/// <summary>
		/// Retrieve an instance of the element registered with the highest pripority.
		/// </summary>
		/// <param name="instanceType">Instance type.</param>
		/// <param name="args">Arguments to create the instance.</param>
		/// <typeparam name="TInterface">The interface to query.</typeparam>
		TInterface Retrieve<TInterface> (InstanceType instanceType = InstanceType.New, params object [] args);

		/// <summary>
		/// Retrieves all the elements registered for a given interface.
		/// </summary>
		/// <returns>The elements registered.</returns>
		/// <param name="instanceType">Instance type.</param>
		/// <param name="args">Arguments to create the instances.</param>
		/// <typeparam name="TInterface">The interface to query.</typeparam>
		List<TInterface> RetrieveAll<TInterface> (InstanceType instanceType = InstanceType.New, params object [] args);
	}
}
