﻿//
//  Copyright (C) 2016 Fluendo S.A.
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
using VAS.Core.MVVMC;

namespace VAS.Tests.MVVMC
{
	[TestFixture]
	public class TestViewLocator
	{
		[Test]
		public void TestRegisterAndRetrieve ()
		{
			var locator = new ViewLocator ();
			locator.Register ("test", typeof(DummyView));
			Assert.AreEqual (typeof(DummyView), locator.Retrieve ("test").GetType ());
		}

		[Test]
		public void TestRegisterWrongType ()
		{
			var locator = new ViewLocator ();
			Assert.Throws<InvalidCastException> (() => locator.Register ("test", typeof(object)));
		}

		[Test]
		public void TestRetrieveNonExistant ()
		{
			var locator = new ViewLocator ();
			Assert.IsNull (locator.Retrieve ("test"));
		}
	}
}
