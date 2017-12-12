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
using NUnit.Framework;
using VAS.Core.Common.TypeConverters;

namespace VAS.Tests.Core.Common
{
	[TestFixture]
	public class TestTypeConverters
	{
		[Test ()]
		public void ConvertTo_NullConvertsToBoolean_ReturnsFalse ()
		{
			///Arrange

			var target = new IsNotNullOrWhiteSpaceStringConverter ();

			///Act

			var result = (bool)target.ConvertTo (null, null, null, typeof (bool));

			///Assert

			Assert.IsFalse (result);
		}

		[Test ()]
		public void ConvertTo_WhiteSpacesConvertsToBoolean_ReturnsFalse ()
		{
			///Arrange

			var target = new IsNotNullOrWhiteSpaceStringConverter ();

			///Act

			var result = (bool)target.ConvertTo (null, null, "   ", typeof (bool));

			///Assert

			Assert.IsFalse (result);
		}

		[Test ()]
		public void ConvertTo_EmptyStringConvertsToBoolean_ReturnsFalse ()
		{
			///Arrange

			var target = new IsNotNullOrWhiteSpaceStringConverter ();

			///Act

			var result = (bool)target.ConvertTo (null, null, string.Empty, typeof (bool));

			///Assert

			Assert.IsFalse (result);
		}



		[Test ()]
		public void ConvertTo_StringConvertsToBoolean_ReturnsTrue ()
		{
			///Arrange

			var target = new IsNotNullOrWhiteSpaceStringConverter ();

			///Act

			var result = (bool)target.ConvertTo (null, null, "Hello!", typeof (bool));

			///Assert

			Assert.IsTrue (result);
		}
	}
}
