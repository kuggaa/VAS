//
//  Copyright (C) 2018 
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
using NUnit.Framework;
using VAS.Core.Common.TypeConverters;
using VAS.Core.Store;

namespace VAS.Tests.Core.Common
{
	[TestFixture]
	public class TestTimeToStringConverter
	{
		[Test]
		public void ConvertTo_DefaultConverter_StringWithoutHourOrMs ()
		{
			// Arrange
			var converter = new TimeToStringConverter (false, false);

			// Act
			string convertedTime = (string)converter.ConvertTo (new Time (100002), typeof (string));

			// Assert
			Assert.AreEqual ("1:40", convertedTime);
		}
		[Test]
		public void ConvertTo_NoIncludeHourButHasHour_StringWithHour ()
		{
			// Arrange
			var converter = new TimeToStringConverter (false, false);

			// Act
			string convertedTime = (string)converter.ConvertTo (new Time (10000002), typeof (string));

			// Assert
			Assert.AreEqual ("2:46:40", convertedTime);
		}

		[Test]
		public void ConvertTo_IncludeHour_StringWithHour ()
		{
			// Arrange
			var converter = new TimeToStringConverter (true, false);

			// Act
			string convertedTime = (string)converter.ConvertTo (new Time (100002), typeof (string));

			// Assert
			Assert.AreEqual ("0:01:40", convertedTime);
		}

		[Test]
		public void ConvertTo_IncludeHourWithDetail_StringWithHourAndMs ()
		{
			// Arrange
			var converter = new TimeToStringConverter (true, true);

			// Act
			string convertedTime = (string)converter.ConvertTo (new Time (100002), typeof (string));

			// Assert
			Assert.AreEqual ("0:01:40,002", convertedTime);
		}

		[Test]
		public void ConvertTo_WithDetail_StringWithoutHourAndWithMs ()
		{
			// Arrange
			var converter = new TimeToStringConverter (false, true);

			// Act
			string convertedTime = (string)converter.ConvertTo (new Time (100002), typeof (string));

			// Assert
			Assert.AreEqual ("1:40,002", convertedTime);
		}

		[Test]
		public void ConvertTo_NullTime_NullString ()
		{
			// Arrange
			var converter = new TimeToStringConverter (false, true);
			string convertedTime = "";

			// Act & Assert
			Assert.DoesNotThrow (() =>
								 convertedTime = (string)converter.ConvertTo (null, typeof (string)));
			Assert.IsNull (convertedTime);
		}
	}
}
