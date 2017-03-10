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
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Core.Hotkeys
{
	public class KeyConfig : BindableBase, IEquatable<KeyConfig>
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

		/// <summary>
		/// Determines whether the specified <see cref="VAS.Core.Hotkeys.KeyConfig"/> is equal to the current <see cref="T:VAS.Core.Hotkeys.KeyConfig"/>.
		/// Two KeyConfig are equal if their Name are the same
		/// </summary>
		/// <param name="other">The <see cref="VAS.Core.Hotkeys.KeyConfig"/> to compare with the current <see cref="T:VAS.Core.Hotkeys.KeyConfig"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="VAS.Core.Hotkeys.KeyConfig"/> is equal to the current
		/// <see cref="T:VAS.Core.Hotkeys.KeyConfig"/>; otherwise, <c>false</c>.</returns>
		public bool Equals (KeyConfig other)
		{
			if (other == null) {
				return false;
			}
			return other.Name == Name;
		}

		public override bool Equals (object obj)
		{
			if (obj is KeyConfig) {
				KeyConfig config = obj as KeyConfig;
				return Equals (config);
			} else
				return false;
		}

		public override int GetHashCode ()
		{
			return Name.GetHashCode ();
		}
	}
}
