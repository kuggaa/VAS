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

namespace VAS.Core.Interfaces.MVVMC
{
	public interface IViewLocator
	{
		/// <summary>
		/// Register the specified name and class.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="klass">Type to register.</param>
		void Register (string name, Type klass);

		/// <summary>
		/// Retrieve an instance of the class associated with the specified name.
		/// </summary>
		/// <returns>Instance of the class.</returns>
		/// <param name="name">Name.</param>
		IView Retrieve (string name);

		/// <summary>
		/// Retrieve an instance of the class associated with the specified name.
		/// </summary>
		/// <returns>Instance of the class.</returns>
		/// <param name="name">Name.</param>
		/// <param name="args">Constructor arguments.</param>
		IView Retrieve (string name, params object [] args);
	}
}