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
using System.Collections.Generic;
using VAS.Core.Interfaces.MVVMC;

namespace VAS.Core.MVVMC
{
	/// <summary>
	/// A location service for IView components.
	/// </summary>
	public class ViewLocator : IViewLocator
	{
		readonly Dictionary<string, Type> elements;
		readonly string locatorName;

		public ViewLocator ()
		{
			locatorName = typeof (IView).Name;
			elements = new Dictionary<string, Type> ();
		}

		/// <summary>
		/// Register a View with a name.
		/// </summary>
		/// <param name="name">View name.</param>
		/// <param name="klass">Type of the View.</param>
		public void Register (string name, Type klass)
		{
			if (!typeof (IView).IsAssignableFrom (klass)) {
				throw new InvalidCastException (klass + " is not of type " + typeof (IView));
			}
			elements [name] = klass;
		}

		/// <summary>
		/// Gets a View with the specified name.
		/// </summary>
		/// <param name="name">The View name.</param>
		public IView Retrieve (string name)
		{
			if (!elements.ContainsKey (name)) {
				return null;
			}
			return (IView)Activator.CreateInstance (elements [name]);
		}

		/// <summary>
		/// Gets a View with the specified name. With constructor Params
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="args">View Constructor Param Arguments.</param>
		public IView Retrieve (string name, params object [] args)
		{
			if (!elements.ContainsKey (name)) {
				return null;
			}
			return (IView)Activator.CreateInstance (elements [name], args);
		}
	}
}