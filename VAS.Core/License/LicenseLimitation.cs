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
	/// Base class for the limitations
	/// </summary>
	public class LicenseLimitation : BindableBase
	{
		string displayName;

		/// <summary>
		/// Gets or sets the Registered name of the limitation.
		/// </summary>
		/// <value>The registered name of the limitation.</value>
		public string RegisterName { get; set; }

		/// <summary>
		/// Gets or sets the display name of the limitation.
		/// This is the one that will be displayed in popups, etc.
		/// If this is not set, it returns the RegisterName.
		/// </summary>
		/// <value>The display name of the limitation.</value>
		public string DisplayName {
			get {
				return displayName ?? RegisterName;
			}
			set {
				displayName = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.License.LicenseLimitation"/> is enabled.
		/// </summary>
		/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
		public bool Enabled { get; set; }
	}
}
