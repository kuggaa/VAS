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
using System.Linq;
using System.Collections.Generic;
using VAS.Core.Interfaces.MVVMC;

namespace VAS.Core.MVVMC
{
	/// <summary>
	/// A location service for <see cref="IController"/> components
	/// </summary>
	public class ControllerLocator
	{
		List<ControllerElement> elements;

		internal struct ControllerElement
		{
			public Type Type;
			public int Priority;
			public string Name;

			public ControllerElement (string name, Type type, int priority)
			{
				Type = type;
				Priority = priority;
				Name = name;
			}
		}

		public ControllerLocator ()
		{
			elements = new List<ControllerElement> ();
		}

		/// <summary>
		/// Register a <see cref="IController"/> by name usign its <see cref="Type"/>.
		/// </summary>
		/// <param name="name">Name of the View where this controller is used.</param>
		/// <param name="type">Type of the Controller.</param>
		/// <param name="priority">Priority.</param>
		public void Register (string name, Type type, int priority = 0)
		{
			if (!typeof(IController).IsAssignableFrom (type)) {
				throw new InvalidCastException (type + " is not of type " + typeof(IController));
			}
			elements.Add (new ControllerElement (name, type, priority));
		}

		/// <summary>
		/// Retrieves all the Controllers for a given name.
		/// </summary>
		/// <returns>The list of Controller.</returns>
		/// <param name="name">The name to filter by.</param>
		public List<IController> RetrieveAll (string name)
		{
			return elements.Where (e => e.Name == name).
				OrderBy (e => e.Priority).
				Select (e => (IController)Activator.CreateInstance (e.Type)).ToList ();
		}

		/// <summary>
		/// Retrieve the Controller with the highest priority for a given name.
		/// </summary>
		/// <param name="name">The name.</param>
		public IController Retrieve (string name)
		{
			var controllerElement = elements.Where (e => e.Name == name).
				OrderByDescending (e => e.Priority).
				FirstOrDefault ();
			return (IController)Activator.CreateInstance (controllerElement.Type);
		}
	}
}

