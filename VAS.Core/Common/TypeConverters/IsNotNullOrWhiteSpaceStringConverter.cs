//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using System.ComponentModel;
using System.Globalization;

namespace VAS.Core.Common.TypeConverters
{
	public class IsNotNullOrWhiteSpaceStringConverter : TypeConverter
	{
		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			return (destinationType == typeof (string));
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			return !string.IsNullOrWhiteSpace ((string)value);
		}
	}
}

