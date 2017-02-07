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
				new Predicate<string> { Expression = (ev) => true },
				new Predicate<string> { Expression = (ev) => false },
			};

			Assert.IsTrue (filter.Filter (""));
		}

		[Test ()]
		public void TestOrFalse ()
		{
			var filter = new OrPredicate<string> {
				new Predicate<string> { Expression = (ev) => false },
				new Predicate<string> { Expression = (ev) => false },
			};

			Assert.IsFalse (filter.Filter (""));
		}

		[Test ()]
		public void TestAnd ()
		{
			var filter = new AndPredicate<string> {
				new Predicate<string> { Expression = (ev) => true },
				new Predicate<string> { Expression = (ev) => true },
			};

			Assert.IsTrue (filter.Filter (""));
		}

		[Test ()]
		public void TestAndFalse ()
		{
			var filter = new AndPredicate<string> {
				new Predicate<string> { Expression = (ev) => true },
				new Predicate<string> { Expression = (ev) => false },
			};

			Assert.IsFalse (filter.Filter (""));
		}

		[Test ()]
		public void TestAndContainingOr ()
		{
			var filter = new OrPredicate<string> {
				new Predicate<string> { Expression = (ev) => true },
				new Predicate<string> { Expression = (ev) => false },
			};

			var filter2 = new OrPredicate<string> {
				new Predicate<string> { Expression = (ev) => true },
				new Predicate<string> { Expression = (ev) => false },
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
				new Predicate<string> { Expression = (ev) => true },
				new Predicate<string> { Expression = (ev) => false },
			};

			var filter2 = new Predicate<string> { Expression = (ev) => false };

			var container = new AndPredicate<string> ();
			container.Add (filter);
			container.Add (filter2);

			Assert.IsFalse (container.Filter (""));
		}

		[Test ()]
		public void TestOrContainingAnd ()
		{
			var filter = new AndPredicate<string> {
				new Predicate<string> { Expression = (ev) => true },
				new Predicate<string> { Expression = (ev) => false },
			};

			var filter2 = new AndPredicate<string> {
				new Predicate<string> { Expression = (ev) => true },
				new Predicate<string> { Expression = (ev) => true },
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
				new Predicate<string> { Expression = (ev) => true },
				new Predicate<string> { Expression = (ev) => false },
			};

			var filter2 = new Predicate<string> { Expression = (ev) => false };

			var container = new OrPredicate<string> ();
			container.Add (filter);
			container.Add (filter2);

			Assert.IsFalse (container.Filter (""));
		}

		[Test ()]
		public void TestAndContainingEmptyOr ()
		{
			var filter = new OrPredicate<string> {
				new Predicate<string> { Expression = PredicateBuilder.True<string>() },
			};

			// This OR doesn't have any active Predicate, thus it's ignored
			var filter2 = new OrPredicate<string> ();

			var container = new AndPredicate<string> ();
			container.Add (filter);
			container.Add (filter2);

			Assert.IsTrue (container.Filter (""));
		}

		[Test ()]
		public void TestOrContainingEmptyAnd ()
		{
			var filter = new AndPredicate<string> {
				new Predicate<string> { Expression = (ev) => true },
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
			filter.Add (new Predicate<string> { Expression = (ev) => true });

			//Assert
			Assert.AreEqual ("Collection_Elements", property);
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
			filter.Add (new Predicate<string> { Expression = (ev) => true });

			//Assert
			Assert.AreEqual ("Collection_Elements", property);
			Assert.AreEqual (1, count);
		}

		[Test ()]
		public void TestAndContainingOrInactive ()
		{
			var filter = new OrPredicate<string> {
				new Predicate<string> { Expression = (ev) => true, Active = false },
				new Predicate<string> { Expression = (ev) => false },
			};

			var filter2 = new OrPredicate<string> {
				new Predicate<string> { Expression = (ev) => true },
				new Predicate<string> { Expression = (ev) => false },
			};

			var container = new AndPredicate<string> ();
			container.Add (filter);
			container.Add (filter2);

			Assert.IsFalse (filter.Filter (""));
			Assert.IsTrue (filter2.Filter (""));
			Assert.IsFalse (container.Filter (""));
		}

		[Test ()]
		public void TestAndContainingInactiveOr ()
		{
			var filter = new OrPredicate<string> {
				new Predicate<string> { Expression = (ev) => true  },
				new Predicate<string> { Expression = (ev) => false },
			};
			filter.Active = false;

			var filter2 = new OrPredicate<string> {
				new Predicate<string> { Expression = (ev) => true },
				new Predicate<string> { Expression = (ev) => false },
			};

			var container = new AndPredicate<string> ();
			container.Add (filter);
			container.Add (filter2);

			Assert.IsFalse (filter.Elements [0].Active);
			Assert.IsFalse (filter.Elements [1].Active);
			Assert.IsFalse (filter.Filter (""));
			Assert.IsTrue (filter2.Filter (""));
			Assert.IsTrue (container.Filter (""));
		}

		[Test ()]
		public void TestActiveEvents ()
		{
			// Arrange
			string property = "";
			int count = 0;
			var filter = new OrPredicate<string> ();
			var predicate = new Predicate<string> { Expression = (ev) => true };
			filter.Add (predicate);
			filter.PropertyChanged += (sender, e) => {
				property = e.PropertyName;
				count++;
			};

			// Act
			predicate.Active = false;

			//Assert
			Assert.AreEqual ("Active", property);
			Assert.AreEqual (1, count);
		}

		[Test ()]
		public void TestElementsEvents ()
		{
			// Arrange
			string property = "";
			int count = 0;
			int countElements = 0;

			var filter = new OrPredicate<string> {
				new Predicate<string> { Expression = (ev) => true },
				new Predicate<string> { Expression = (ev) => false },
			};

			var filter2 = new OrPredicate<string> {
				new Predicate<string> { Expression = (ev) => true },
				new Predicate<string> { Expression = (ev) => false },
			};

			var container = new AndPredicate<string> ();
			container.Elements.CollectionChanged += (sender, e) => {
				countElements++;
			};
			container.PropertyChanged += (sender, e) => {
				property = e.PropertyName;
				count++;
			};

			// Act
			container.Add (filter);
			container.Add (filter2);

			//Assert
			Assert.AreEqual ("Collection_Elements", property);
			Assert.AreEqual (2, count);
			Assert.AreEqual (count, countElements);
		}

		[Test ()]
		public void TestAndContainingOrSetActive ()
		{
			// Arrange
			var filter = new OrPredicate<string> {
				new Predicate<string> { Expression = (ev) => true },
				new Predicate<string> { Expression = (ev) => true },
			};

			var filter2 = new OrPredicate<string> {
				new Predicate<string> { Expression = (ev) => true },
				new Predicate<string> { Expression = (ev) => true },
			};

			var container = new AndPredicate<string> ();
			container.Add (filter);
			container.Add (filter2);

			// Act
			filter2.Active = false;

			// Assert
			Assert.IsFalse (filter2 [0].Active);
			Assert.IsFalse (filter2 [1].Active);
			Assert.IsTrue (container.Filter (""));
		}

		[Test ()]
		public void TestInactiveAndContainingOr ()
		{
			// Arrange
			var filter = new OrPredicate<string> {
				new Predicate<string> { Expression = (ev) => true },
				new Predicate<string> { Expression = (ev) => true },
			};

			var filter2 = new OrPredicate<string> {
				new Predicate<string> { Expression = (ev) => true },
				new Predicate<string> { Expression = (ev) => true },
			};

			var container = new AndPredicate<string> ();
			container.Add (filter);
			container.Add (filter2);

			// Act
			container.Active = false;

			// Assert
			Assert.IsFalse (container.Active);
			Assert.IsFalse (filter.Active);
			Assert.IsFalse (filter [0].Active);
			Assert.IsFalse (filter [1].Active);
			Assert.IsFalse (filter2.Active);
			Assert.IsFalse (filter2 [0].Active);
			Assert.IsFalse (filter2 [1].Active);
			Assert.IsTrue (container.Filter (""));
		}

		[Test ()]
		public void TestAndOrSetActiveChildren ()
		{
			// Arrange
			var filter = new OrPredicate<string> {
				new Predicate<string> { Expression = (ev) => true },
				new Predicate<string> { Expression = (ev) => true },
			};

			var filter2 = new OrPredicate<string> {
				new Predicate<string> { Expression = (ev) => true },
				new Predicate<string> { Expression = (ev) => true },
			};

			var container = new AndPredicate<string> ();
			container.Add (filter);
			container.Add (filter2);

			// Act
			filter.Active = false;
			filter2.Active = false;

			// Assert
			Assert.IsFalse (container.Active);
			Assert.IsFalse (filter.Active);
			Assert.IsFalse (filter [0].Active);
			Assert.IsFalse (filter [1].Active);
			Assert.IsFalse (filter2.Active);
			Assert.IsFalse (filter2 [0].Active);
			Assert.IsFalse (filter2 [1].Active);
			Assert.IsTrue (container.Filter (""));
		}

		[Test ()]
		public void TestAndOrEmpty ()
		{
			// Arrange
			var filter = new OrPredicate<string> ();
			var filter2 = new OrPredicate<string> ();

			var container = new AndPredicate<string> ();
			container.Add (filter);
			container.Add (filter2);

			// Act

			// Assert
			Assert.IsFalse (filter.Active);
			Assert.IsFalse (filter2.Active);
			Assert.IsFalse (container.Active);
			Assert.IsTrue (container.Filter (""));
		}
	}
}

