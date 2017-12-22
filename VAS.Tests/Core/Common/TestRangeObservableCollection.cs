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
using System.ComponentModel;
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
		List<NotifyCollectionChangedAction> actionsPerformed;
		int counter;
		bool notified = false;
		int notifications, countNotifications;

		[SetUp ()]
		public void SetUp ()
		{
			notified = false;
			notifications = 0;
			countNotifications = 0;
			counter = 0;
			collection = new RangeObservableCollection<int> (new List<int> { 0, 1, 2, 3, 4 });
			index = -2;
			actionsPerformed = new List<NotifyCollectionChangedAction> ();
			collection.CollectionChanged += CollectionChanged;
			(collection as INotifyPropertyChanged).PropertyChanged += HandlePropertyChanged;
		}

		[TearDown]
		public void TearDown ()
		{
			if (collection != null) {
				collection.CollectionChanged -= CollectionChanged;
				(collection as INotifyPropertyChanged).PropertyChanged -= HandlePropertyChanged;
			}
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
			Assert.AreEqual (actionsPerformed [0], NotifyCollectionChangedAction.Add);
			Assert.AreEqual (1, notifications);
			Assert.AreEqual (1, countNotifications);
		}

		[Test ()]
		public void TestRemoveRange ()
		{
			List<int> list = new List<int> { 1, 3 };
			collection.RemoveRange (list.Select ((arg) => IncrementCounter (arg)).Where (arg => arg >= 0));

			Assert.AreEqual (2, counter);
			Assert.AreEqual (3, collection.Count);
			Assert.AreEqual (2, collection [1]);
			Assert.AreEqual (actionsPerformed [0], NotifyCollectionChangedAction.Remove);
			Assert.AreEqual (notifications, 1);
			Assert.AreEqual (countNotifications, 1);
		}

		[Test ()]
		public void RemoveRange_ElementsNotContained_DoNotNotify ()
		{
			// Arrange
			List<int> list = new List<int> { 100, 300 };

			// Act
			collection.RemoveRange (list);

			// Assert
			Assert.AreEqual (0, notifications);
			Assert.AreEqual (0, countNotifications);
			Assert.AreEqual (5, collection.Count);
			Assert.AreEqual (1, collection [1]);
		}

		[Test ()]
		public void Replace_SameElementsInCollection_DoNotNotify ()
		{
			// Arrange
			List<int> list = new List<int> { 0, 1, 2, 3, 4 };

			// Act
			collection.Replace (list);

			// Assert
			Assert.AreEqual (0, notifications);
			Assert.AreEqual (0, countNotifications);
			Assert.AreEqual (5, collection.Count);
			Assert.AreEqual (1, collection [1]);
		}

		[Test ()]
		public void Replace_DifferentElementsInCollection_Notify ()
		{
			// Arrange
			List<int> list = new List<int> { 0, 1, 5 };

			// Act
			collection.Replace (list);

			// Assert
			Assert.AreEqual (2, notifications);
			Assert.AreEqual (2, countNotifications);
			Assert.AreEqual (actionsPerformed [0], NotifyCollectionChangedAction.Remove);
			Assert.AreEqual (actionsPerformed [1], NotifyCollectionChangedAction.Add);
			Assert.AreEqual (3, collection.Count);
			Assert.AreEqual (5, collection [2]);
		}

		[Test ()]
		public void TestInsertRange ()
		{
			List<int> list = new List<int> { 5, 6 };
			collection.InsertRange (2, list.Select ((arg) => IncrementCounter (arg)).Where (arg => arg >= 0));

			Assert.AreEqual (2, counter);
			Assert.AreEqual (2, index);
			Assert.AreEqual (5, collection [index]);
			Assert.AreEqual (actionsPerformed [0], NotifyCollectionChangedAction.Add);
			Assert.AreEqual (1, notifications);
			Assert.AreEqual (1, countNotifications);
		}

		[Test ()]
		public void TestReset ()
		{
			List<int> collectionToReplace = new List<int> { 5, 6, 7 };
			collection.Reset (collectionToReplace.Select ((arg) => IncrementCounter (arg)).Where (arg => arg >= 0));

			Assert.AreEqual (3, counter);
			Assert.AreEqual (collection, collectionToReplace);
			Assert.AreEqual (actionsPerformed [0], NotifyCollectionChangedAction.Reset);
			Assert.AreEqual (1, notifications);
			Assert.AreEqual (1, countNotifications);
		}

		[Test ()]
		public void TestReset_SameList_DoesNotNotify ()
		{
			List<int> collectionToReplace = new List<int> { 0, 1, 2, 3, 4 };
			collection.Reset (collectionToReplace.Select ((arg) => IncrementCounter (arg)).Where (arg => arg >= 0));

			Assert.AreEqual (5, counter);
			Assert.AreEqual (collection, collectionToReplace);
			Assert.IsFalse (notified);
		}

		void CollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add) {
				index = e.NewStartingIndex;
			}
			actionsPerformed.Add (e.Action);
			notifications++;
			notified = true;
		}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (collection.Count)) {
				countNotifications++;
			}
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
