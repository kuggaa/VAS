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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace VAS.Core.Common
{
	/// <summary>
	/// Range observable collection.
	/// </summary>
	[Serializable]
	public class RangeObservableCollection<T> : ObservableCollection<T>
	{
		public RangeObservableCollection () : base ()
		{
		}

		public RangeObservableCollection (List<T> list) : base (list)
		{
		}

		public RangeObservableCollection (IEnumerable<T> collection) : base (collection)
		{
		}

		/// <summary>
		/// Adds multiple items
		/// </summary>
		/// <param name="items">Items to add</param>
		public void AddRange (IEnumerable<T> items)
		{
			int index = Items.Count;
			var itemsList = items.ToList ();
			if (itemsList.Any ()) {
				foreach (T item in itemsList) {
					Items.Add (item);
				}
				NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, itemsList, index);
				OnCollectionChanged (e);
			}
		}

		/// <summary>
		/// Remove multiple items
		/// </summary>
		/// <param name="items">Items to remove</param>
		public void RemoveRange (IEnumerable<T> items)
		{
			var itemsList = items.ToList ();
			if (itemsList.Any ()) {
				foreach (var item in itemsList) {
					Items.Remove (item);
				}
				NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, itemsList);
				OnCollectionChanged (e);
			}
		}

		/// <summary>
		/// Inserts multiple items at a specified position
		/// </summary>
		/// <param name="index">Position</param>
		/// <param name="items">Items to add</param>
		public void InsertRange (int index, IEnumerable<T> items)
		{
			var itemsList = items.ToList ();
			if (itemsList.Any ()) {
				int indexCopy = index;
				foreach (var item in itemsList) {
					Items.Insert (index++, item);
				}
				NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, itemsList, indexCopy);
				OnCollectionChanged (e);
			}
		}

		/// <summary>
		/// Replace all collection with new elements
		/// </summary>
		/// <param name="items">Items to replace</param>
		public void Replace (IEnumerable<T> items)
		{
			Items.Clear ();
			if (items != null) {
				foreach (var item in items) {
					Items.Add (item);
				}
			}
			NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset);
			OnCollectionChanged (e);
		}
	}
}

