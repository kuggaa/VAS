//
//  Copyright (C) 2017 
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
using System.Globalization;
using VAS.Core.Store;

namespace VAS.Core.Common.TypeConverters
{
	public class TimeToStringConverter : TypeConverter
	{
		readonly bool includeHour;
		readonly bool withDetail;

		public TimeToStringConverter (bool includeHour = false, bool withDetail = false)
		{
			this.includeHour = includeHour;
			this.withDetail = withDetail;
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			return (destinationType == typeof (Time));
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof (string);
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (withDetail) {
				return ((Time)value)?.ToMSecondsString (includeHour);
			} else {
				return ((Time)value)?.ToSecondsString (includeHour);
			}

		}
	}
}
