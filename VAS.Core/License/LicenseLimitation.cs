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

namespace VAS.Core.License
{
	/// <summary>
	/// License limitation.
	/// This class represent a generic count limitation.
	/// </summary>
	public class LicenseLimitation : BindableBase
	{
		int maximum;

		/// <summary>
		/// Gets or sets the name of the limitation.
		/// </summary>
		/// <value>The name of the limitation.</value>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the count of licensed items.
		/// </summary>
		/// <value>The count.</value>
		public int Count { get; set; }

		/// <summary>
		/// Gets or sets the maximum number for the limitation.
		/// </summary>
		/// <value>The maximum.</value>
		public int Maximum {
			get {
				// FIXME: Logic in the model?
				int max = int.MaxValue;
				if (Enabled) {
					max = maximum;
				}
				return max;
			}
			set {
				maximum = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.License.LicenseLimitation"/> is enabled.
		/// </summary>
		/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
		public bool Enabled { get; set; }
	}
}
