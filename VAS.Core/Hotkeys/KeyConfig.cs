//
//  Copyright (C) 2017 Fluendo S.A.
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
using VAS.Core.Store;

namespace VAS.Core.Hotkeys
{
	public class KeyConfig
	{
		/// <summary>
		/// Gets or sets the identifier name of the HotKey
		/// </summary>
		/// <value>The name of the HotKey.</value>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the key that performs the action
		/// </summary>
		/// <value>The key.</value>
		public HotKey Key { get; set; }

		/// <summary>
		/// Gets or sets the category.
		/// </summary>
		/// <value>The category.</value>
		public string Category { get; set; }

		/// <summary>
		/// Gets or sets the description of the HotKey
		/// </summary>
		/// <value>The description.</value>
		public string Description { get; set; }
	}
}
