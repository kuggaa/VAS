//
//  Copyright (C) 2015 Fluendo S.A.
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace VAS.Core.Common
{
	public class Registry
	{
		ConcurrentDictionary<Type, List<RegistryElement>> elements;
		string name;

		internal class RegistryElement
		{
			public RegistryElement (Type type, int priority)
			{
				Type = type;
				Priority = priority;
			}

			public Type Type {
				get;
				set;
			}

			public int Priority {
				get;
				set;
			}

			public object Instance {
				get;
				set;
			}
		}

		public Registry (string name)
		{
			this.name = name;
			elements = new ConcurrentDictionary<Type, List<RegistryElement>> ();
		}

		/// <summary>
		/// Register a new element type for a given interface with a priority.
		/// </summary>
		/// <param name="priority">Priority.</param>
		/// <typeparam name="TInterface">The interface to register the element.</typeparam>
		/// <typeparam name="TType">The type of the registered element.</typeparam>
		public void Register<TInterface, TType> (int priority = 0)
		{
			Register<TInterface> (typeof (TType), priority);
		}

		/// <summary>
		/// Register a new element type for a given interface with a priority.
		/// </summary>
		/// <param name="type">The type of the registered element.</param>
		/// <param name="priority">Priority.</param>
		/// <typeparam name="TInterface">The interface to register the element.</typeparam>
		public void Register<TInterface> (Type type, int priority = 0)
		{
			Type interfac = typeof (TInterface);
			Register (interfac, type, priority);
		}

		/// <summary>
		/// Register the specified interfac, type and priority in runtime.
		/// </summary>
		/// <param name="interfac">Interfac.</param>
		/// <param name="type">Type.</param>
		/// <param name="priority">Priority.</param>
		public void Register (Type interfac, Type type, int priority = 0)
		{
			if (!elements.ContainsKey (interfac)) {
				elements [interfac] = new List<RegistryElement> ();
			}
			RegistryElement element = new RegistryElement (type, priority);
			AddElement (interfac, element);
			Log.Information (string.Format ("Registered {0} in {1} with priority {2}", type, interfac, priority));
		}

		/// <summary>
		/// register a new element with an instance object for a given interface with a priority
		/// </summary>
		/// <param name="instance">The instance of the registered interface.</param>
		/// <param name="priority">Priority.</param>
		/// <typeparam name="TInterface">The interface to register the element.</typeparam>
		public void Register<TInterface> (object instance, int priority = 0)
		{
			Type interfac = typeof (TInterface);
			Register (interfac, instance, priority);
		}

		/// <summary>
		/// register a new element with an instance object for a given interface with a priority in runtime
		/// </summary>
		/// <param name="interfac">Interfac.</param>
		/// <param name="instance">The instance of the registered interface.</param>
		/// <param name="priority">Priority.</param>
		public void Register (Type interfac, object instance, int priority = 0)
		{
			if (!interfac.IsAssignableFrom (instance.GetType ())) {
				throw new InvalidOperationException ("Registered instance does not implement the proper interface");
			}

			if (!elements.ContainsKey (interfac)) {
				elements [interfac] = new List<RegistryElement> ();
			}

			RegistryElement element = new RegistryElement (instance.GetType (), priority);
			element.Instance = instance;

			AddElement (interfac, element);
			Log.Information (string.Format ("Registered instance of type {0} in {1} with priority {2}", instance.GetType (), interfac, priority));
		}

		/// <summary>
		/// Retrieve an instance of the element registered with the highest pripority.
		/// </summary>
		/// <param name="instanceType">Instance type.</param>
		/// <param name="args">Arguments to create the instance.</param>
		/// <typeparam name="TInterface">The interface to query.</typeparam>
		public virtual TInterface Retrieve<TInterface> (InstanceType instanceType = InstanceType.New, params object [] args)
		{
			CheckInterfaceExists (typeof (TInterface));

			RegistryElement element = elements [typeof (TInterface)].OrderByDescending (e => e.Priority).First ();
			return GetInstance<TInterface> (element, instanceType, args);
		}

		/// <summary>
		/// Retrieves all the elements registered for a given interface.
		/// </summary>
		/// <returns>The elements registered.</returns>
		/// <param name="instanceType">Instance type.</param>
		/// <param name="args">Arguments to create the instances.</param>
		/// <typeparam name="TInterface">The interface to query.</typeparam>
		public List<TInterface> RetrieveAll<TInterface> (InstanceType instanceType = InstanceType.New, params object [] args)
		{
			CheckInterfaceExists (typeof (TInterface));

			return elements [typeof (TInterface)].Select (e => GetInstance<TInterface> (e, instanceType, args)).ToList ();
		}

		void CheckInterfaceExists (Type interfac)
		{
			if (!elements.ContainsKey (interfac)) {
				throw new Exception (String.Format ("No {0} available in the {0} registry",
					interfac, name));
			}
		}

		void AddElement (Type interfac, RegistryElement element)
		{
			int index = elements [interfac].FindIndex (elem => elem.Priority == element.Priority);
			if (index != -1) {
				Log.Error ("Two items registered with the same priority." +
						   $"Replacing {elements [interfac] [index]} with {element}");
				elements [interfac] [index] = element;
			} else {
				elements [interfac].Add (element);
			}
		}

		T GetInstance<T> (RegistryElement element, InstanceType instanceType, params object [] args)
		{
			T instance;

			if (instanceType == InstanceType.New ||
				instanceType == InstanceType.Default && element.Instance == null) {
				instance = (T)Activator.CreateInstance (element.Type, args);
				if (instanceType == InstanceType.Default) {
					element.Instance = instance;
				}
			} else {
				instance = (T)element.Instance;
			}
			return instance;
		}
	}
}
