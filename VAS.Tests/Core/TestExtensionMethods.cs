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
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Store;

namespace VAS.Tests.Core
{
	[TestFixture ()]
	public class TestExtensionMethods
	{
		[Test ()]
		public void TestSortAsc ()
		{
			CustomDummyClassForTest [] array = InitCustomDummyClassForTest ();

			CustomDummyClassForTest [] arrayAsc = array.Sort (c => c.Index, false).ToArray ();

			CollectionAssert.IsOrdered (arrayAsc, new CustomDummyClassForTestComparer (false));
		}

		[Test ()]
		public void TestSortDesc ()
		{
			CustomDummyClassForTest [] array = InitCustomDummyClassForTest ();

			CustomDummyClassForTest [] arrayDesc = array.Sort (c => c.Index, true).ToArray ();

			CollectionAssert.IsOrdered (arrayDesc, new CustomDummyClassForTestComparer (true));
		}

		[Test ()]
		public void TestSortStorableBaseAsc ()
		{
			StorableBase [] array = InitStorableBaseArray ();

			StorableBase [] arrayAsc = array.SortByCreationDate (false).ToArray ();

			CollectionAssert.IsOrdered (arrayAsc, new StorableBaseComparer (false));
		}

		[Test ()]
		public void TestSortStorableBaseDesc ()
		{
			StorableBase [] array = InitStorableBaseArray ();

			StorableBase [] arrayDesc = array.SortByCreationDate (true).ToArray ();

			CollectionAssert.IsOrdered (arrayDesc, new StorableBaseComparer (true));
		}

		[Test]
		public void CompareExpandoObject_DifferentPropertiesCount_NotEquals ()
		{
			dynamic obj1 = new ExpandoObject ();
			obj1.Name = "test";
			dynamic obj2 = new ExpandoObject ();
			Assert.IsFalse ((obj1 as ExpandoObject).Compare ((ExpandoObject)obj2));
		}

		[Test]
		public void CompareExpandoObject_DifferentPropertyKey_NotEquals ()
		{
			dynamic obj1 = new ExpandoObject ();
			obj1.Name = "test";
			dynamic obj2 = new ExpandoObject ();
			obj2.FullName = "test";
			Assert.IsFalse ((obj1 as ExpandoObject).Compare ((ExpandoObject)obj2));
		}

		[Test]
		public void CompareExpandoObject_DifferentPropertyValue_NotEquals ()
		{
			dynamic obj1 = new ExpandoObject ();
			obj1.Name = "test";
			dynamic obj2 = new ExpandoObject ();
			obj2.Name = "nottest";
			Assert.IsFalse ((obj1 as ExpandoObject).Compare ((ExpandoObject)obj2));

		}

		[Test]
		public void CompareExpandoObject_AllPropertiesSimilar_Equals ()
		{
			dynamic obj1 = new ExpandoObject ();
			obj1.Name = "test";
			dynamic obj2 = new ExpandoObject ();
			obj2.Name = "test";
			Assert.IsTrue ((obj1 as ExpandoObject).Compare ((ExpandoObject)obj2));
		}

		[Test]
		public void Move_ExistingItems_ItemsMoved ()
		{
			var object1 = new Object ();
			var object2 = new Object ();
			var object3 = new Object ();
			var list = new List<object> {
				object1,
				object2,
				object3
			};

			list.Move (0, 2);
			Assert.AreSame (object2, list [0]);
			Assert.AreSame (object1, list [2]);
		}

		CustomDummyClassForTest [] InitCustomDummyClassForTest ()
		{
			return new CustomDummyClassForTest [] {
				new CustomDummyClassForTest { Index = 10 },
				new CustomDummyClassForTest { Index = 6 },
				new CustomDummyClassForTest { Index = 4 },
				new CustomDummyClassForTest { Index = 5 }
 			};
		}

		StorableBase [] InitStorableBaseArray ()
		{
			DateTime dt1 = DateTime.Now;
			DateTime dt2 = dt1.AddSeconds (1);
			DateTime dt3 = dt2.AddSeconds (1);
			DateTime dt4 = dt3.AddSeconds (1);
			return new StorableBase [] {
				new StorableBase { CreationDate = dt2, ID = Guid.NewGuid()},
				new StorableBase { CreationDate = dt4, ID = Guid.NewGuid() },
				new StorableBase { CreationDate = dt1, ID = Guid.NewGuid() },
				new StorableBase { CreationDate = dt3, ID = Guid.NewGuid() }
 			};
		}
	}
}
