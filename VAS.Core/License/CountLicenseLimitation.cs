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

namespace VAS.Core.License
{
	/// <summary>
	/// Count License limitation.
	/// This class represent a generic count limitation.
	/// </summary>
	public class CountLicenseLimitation : LicenseLimitation
	{
		int maximum;

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
	}
}
