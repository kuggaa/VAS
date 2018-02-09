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

namespace VAS.Core.Interfaces.MVVMC
{
	/// <summary>
	/// Interface to retrieve classes inheriting from the same class.
	/// All classes should inherit from the TResult specified.
	/// </summary>
	public interface ILocator<TReturn>
	{
		/// <summary>
		/// Register the specified name and class.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="klass">Type to register.</param>
		void Register (string name, Type klass, int priority = 0);

		/// <summary>
		/// Retrieve an instance of the class associated with the specified name.
		/// </summary>
		/// <returns>Instance of the class.</returns>
		/// <param name="name">Name.</param>
		TReturn Retrieve (string name);

		/// <summary>
		/// Retrieve all instances of the class associated with the specified name.
		/// </summary>
		/// <returns>List containing instances of all the classes registered.</returns>
		/// <param name="name">Name.</param>
		IEnumerable<TReturn> RetrieveAll (string name);
	}
}