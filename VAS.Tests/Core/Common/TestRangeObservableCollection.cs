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
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using NUnit.Framework;
using VAS.Core.Common;

namespace VAS.Tests.Core.Common
{
	[TestFixture ()]
	public class TestRangeObservableCollection
	{
		RangeObservableCollection<int> collection;
		int index;
		NotifyCollectionChangedAction actionPerformed;
		int counter;
		bool notified = false;

		[SetUp ()]
		public void SetUp ()
		{
			notified = false;
			counter = 0;
			if (collection != null) {
				collection.CollectionChanged -= CollectionChanged;
			}
			collection = new RangeObservableCollection<int> (new List<int> { 0, 1, 2, 3, 4 });
			index = -2;
			actionPerformed = NotifyCollectionChangedAction.Move;
			collection.CollectionChanged += CollectionChanged;
		}

		[Test ()]
		public void TestAddRange ()
		{
			int indexToVerify;

			indexToVerify = collection.Count;
			List<int> list = new List<int> { 5, 6 };
			collection.AddRange (list.Select ((arg) => IncrementCounter (arg)).Where (arg => arg >= 0));

			Assert.AreEqual (2, counter);
			Assert.AreEqual (indexToVerify, index);
			Assert.AreEqual (7, collection.Count);
			Assert.AreEqual (6, collection.Last ());
			Assert.AreEqual (actionPerformed, NotifyCollectionChangedAction.Add);
		}

		[Test ()]
		public void TestRemoveRange ()
		{
			List<int> list = new List<int> { 1, 3 };
			collection.RemoveRange (list.Select ((arg) => IncrementCounter (arg)).Where (arg => arg >= 0));

			Assert.AreEqual (2, counter);
			Assert.AreEqual (3, collection.Count);
			Assert.AreEqual (2, collection [1]);
			Assert.AreEqual (actionPerformed, NotifyCollectionChangedAction.Remove);
		}

		[Test ()]
		public void TestInsertRange ()
		{
			List<int> list = new List<int> { 5, 6 };
			collection.InsertRange (2, list.Select ((arg) => IncrementCounter (arg)).Where (arg => arg >= 0));

			Assert.AreEqual (2, counter);
			Assert.AreEqual (2, index);
			Assert.AreEqual (5, collection [index]);
			Assert.AreEqual (actionPerformed, NotifyCollectionChangedAction.Add);
		}

		[Test ()]
		public void TestReplace ()
		{
			List<int> collectionToReplace = new List<int> { 5, 6, 7 };
			collection.Replace (collectionToReplace.Select ((arg) => IncrementCounter (arg)).Where (arg => arg >= 0));

			Assert.AreEqual (3, counter);
			Assert.AreEqual (collection, collectionToReplace);
			Assert.AreEqual (actionPerformed, NotifyCollectionChangedAction.Reset);
		}

		[Test ()]
		public void TestReplace_SameList_DoesNotNotify ()
		{
			List<int> collectionToReplace = new List<int> { 0, 1, 2, 3, 4 };
			collection.Replace (collectionToReplace.Select ((arg) => IncrementCounter (arg)).Where (arg => arg >= 0));

			Assert.AreEqual (5, counter);
			Assert.AreEqual (collection, collectionToReplace);
			Assert.IsFalse (notified);
		}

		void CollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add) {
				index = e.NewStartingIndex;
			}
			actionPerformed = e.Action;
			notified = true;
		}

		/// <summary>
		/// Increments the counter.
		/// </summary>
		/// <returns>The counter.</returns>
		/// <param name="value">Value.</param>
		public int IncrementCounter (int value)
		{
			counter++;
			return value;
		}
	}
}
