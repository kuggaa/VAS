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
using System.ComponentModel;

namespace VAS.Core.Common
{
	/// <summary>
	/// Double-Int32 converter class.
	/// </summary>
	public class VASInt32Converter : Int32Converter
	{
		/// <summary>
		/// Returns true if the convertion can be done
		/// </summary>
		/// <returns><c>true</c>, if convertion can be done, <c>false</c> otherwise.</returns>
		/// <param name="context">Context.</param>
		/// <param name="sourceType">Source type.</param>
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (double)) {
				return true;
			}
			return base.CanConvertFrom (context, sourceType);
		}

		/// <summary>
		/// Converts from the given type to the specified object.
		/// </summary>
		/// <returns>The from.</returns>
		/// <param name="context">Context.</param>
		/// <param name="culture">Culture.</param>
		/// <param name="value">Value.</param>
		public override object ConvertFrom (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if (value is double) {
				return Convert.ToInt32 (value);
			}
			return base.ConvertFrom (context, culture, value);
		}
	}
}
