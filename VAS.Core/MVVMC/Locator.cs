//
//  Copyright (C) 2018 
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
using System.Collections.Generic;
using VAS.Core.Interfaces.MVVMC;

namespace VAS.Core.MVVMC
{
	/// <summary>
	/// A location service for <see cref="TReturn"/> components
	/// </summary>
	public class Locator<TReturn> : ILocator<TReturn>
	{

		List<RegisteredElement> elements;

		internal struct RegisteredElement
		{
			public Type Type;
			public int Priority;
			public string Name;

			public RegisteredElement (string name, Type type, int priority)
			{
				Type = type;
				Priority = priority;
				Name = name;
			}
		}

		public Locator ()
		{
			elements = new List<RegisteredElement> ();
		}

		/// <summary>
		/// Register a <see cref="TReturn"/> by name usign its <see cref="Type"/>.
		/// </summary>
		/// <param name="name">Name to map to this element.</param>
		/// <param name="type">Type to register.</param>
		/// <param name="priority">Priority.</param>
		public void Register (string name, Type type, int priority = 0)
		{
			if (!typeof (TReturn).IsAssignableFrom (type)) {
				throw new InvalidCastException (type + " is not of type " + typeof (TReturn));
			}
			elements.Add (new RegisteredElement (name, type, priority));
		}

		/// <summary>
		/// Retrieves all the elements registered for a given name.
		/// </summary>
		/// <returns>The list of elements.</returns>
		/// <param name="name">The name to filter by.</param>
		public IEnumerable<TReturn> RetrieveAll (string name)
		{
			return elements.Where (e => e.Name == name).
				OrderBy (e => e.Priority).
				Select (e => (TReturn)Activator.CreateInstance (e.Type)).ToList ();
		}

		/// <summary>
		/// Retrieve the element with the highest priority for a given name.
		/// </summary>
		/// <param name="name">The name.</param>
		public TReturn Retrieve (string name)
		{
			var element = elements.Where (e => e.Name == name).
				OrderByDescending (e => e.Priority).
				FirstOrDefault ();
			if (element.Type == null) {
				return default (TReturn);
			}
			return (TReturn)Activator.CreateInstance (element.Type);
		}
	}
}
