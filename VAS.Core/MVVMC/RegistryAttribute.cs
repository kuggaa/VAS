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
namespace VAS.Core.MVVMC
{
	/// <summary>
	/// Attribute used to register Services components.
	/// </summary>
	[AttributeUsage (AttributeTargets.Class, AllowMultiple = false)]
	public class RegistryAttribute : Attribute
	{
		public RegistryAttribute (Type interfaceType, int priority)
		{
			InterfaceType = interfaceType;
			Priority = priority;
		}

		/// <summary>
		/// Gets or sets the type of the interface implemented.
		/// </summary>
		/// <value>The type of the interface implemented.</value>
		public Type InterfaceType { get; set; }

		/// <summary>
		/// Gets or sets the priority.
		/// </summary>
		/// <value>The priority.</value>
		public int Priority { get; set; }
	}
}
