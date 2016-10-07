//
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

using System.Collections.Generic;
using NUnit.Framework;
using VAS.Core.Filters;
using VAS.Core.Interfaces;
using System.Collections.ObjectModel;

namespace VAS.Tests.Core.Filters
{
	[TestFixture ()]
	public class TestFilters
	{
		[Test ()]
		public void TestOr ()
		{
			var filter = new OrPredicate<string> {
				new Predicate<string> { Filter = (ev) => true },
				new Predicate<string> { Filter = (ev) => false },
			};

			Assert.IsTrue (filter.Filter (""));
		}

		[Test ()]
		public void TestOrFalse ()
		{
			var filter = new OrPredicate<string> {
				new Predicate<string> { Filter = (ev) => false },
				new Predicate<string> { Filter = (ev) => false },
			};

			Assert.IsFalse (filter.Filter (""));
		}

		[Test ()]
		public void TestAnd ()
		{
			var filter = new AndPredicate<string> {
				new Predicate<string> { Filter = (ev) => true },
				new Predicate<string> { Filter = (ev) => true },
			};

			Assert.IsTrue (filter.Filter (""));
		}

		[Test ()]
		public void TestAndFalse ()
		{
			var filter = new AndPredicate<string> {
				new Predicate<string> { Filter = (ev) => true },
				new Predicate<string> { Filter = (ev) => false },
			};

			Assert.IsFalse (filter.Filter (""));
		}

		[Test ()]
		public void TestAndContainingOr ()
		{
			var filter = new OrPredicate<string> {
				new Predicate<string> { Filter = (ev) => true },
				new Predicate<string> { Filter = (ev) => false },
			};

			var filter2 = new OrPredicate<string> {
				new Predicate<string> { Filter = (ev) => true },
				new Predicate<string> { Filter = (ev) => false },
			};

			var container = new AndPredicate<string> ();
			container.Add (filter);
			container.Add (filter2);

			Assert.IsTrue (container.Filter (""));
		}

		[Test ()]
		public void TestAndContainingOrFalse ()
		{
			var filter = new OrPredicate<string> {
				new Predicate<string> { Filter = (ev) => true },
				new Predicate<string> { Filter = (ev) => false },
			};

			var filter2 = new Predicate<string> { Filter = (ev) => false };

			var container = new AndPredicate<string> ();
			container.Add (filter);
			container.Add (filter2);

			Assert.IsFalse (container.Filter (""));
		}

		[Test ()]
		public void TestOrContainingAnd ()
		{
			var filter = new AndPredicate<string> {
				new Predicate<string> { Filter = (ev) => true },
				new Predicate<string> { Filter = (ev) => false },
			};

			var filter2 = new AndPredicate<string> {
				new Predicate<string> { Filter = (ev) => true },
				new Predicate<string> { Filter = (ev) => true },
			};

			var container = new OrPredicate<string> ();
			container.Add (filter);
			container.Add (filter2);

			Assert.IsTrue (container.Filter (""));
		}

		[Test ()]
		public void TestOrContainingAndFalse ()
		{
			var filter = new AndPredicate<string> {
				new Predicate<string> { Filter = (ev) => true },
				new Predicate<string> { Filter = (ev) => false },
			};

			var filter2 = new Predicate<string> { Filter = (ev) => false };

			var container = new OrPredicate<string> ();
			container.Add (filter);
			container.Add (filter2);

			Assert.IsFalse (container.Filter (""));
		}

		[Test ()]
		public void TestAndContainingEmptyOr ()
		{
			var filter = new OrPredicate<string> {
				new Predicate<string> { Filter = (ev) => true },
			};

			var filter2 = new OrPredicate<string> ();

			var container = new AndPredicate<string> ();
			container.Add (filter);
			container.Add (filter2);

			Assert.IsFalse (container.Filter (""));
		}

		[Test ()]
		public void TestOrContainingEmptyAnd ()
		{
			var filter = new AndPredicate<string> {
				new Predicate<string> { Filter = (ev) => true },
			};

			var filter2 = new AndPredicate<string> ();

			var container = new OrPredicate<string> ();
			container.Add (filter);
			container.Add (filter2);

			Assert.IsTrue (container.Filter (""));
		}

		[Test ()]
		public void TestAndEvents ()
		{
			// Arrange
			string property = "";
			int count = 0;
			var filter = new AndPredicate<string> ();
			filter.PropertyChanged += (sender, e) => {
				property = e.PropertyName;
				count++;
			};

			// Act
			filter.Add (new Predicate<string> { Filter = (ev) => true });

			//Assert
			Assert.AreEqual ("Collection", property);
			Assert.AreEqual (1, count);
		}

		[Test ()]
		public void TestOrEvents ()
		{
			// Arrange
			string property = "";
			int count = 0;
			var filter = new OrPredicate<string> ();
			filter.PropertyChanged += (sender, e) => {
				property = e.PropertyName;
				count++;
			};

			// Act
			filter.Add (new Predicate<string> { Filter = (ev) => true });

			//Assert
			Assert.AreEqual ("Collection", property);
			Assert.AreEqual (1, count);
		}
	}
}

