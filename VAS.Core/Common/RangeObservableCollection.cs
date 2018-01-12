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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;

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

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool IgnoreEvents {
			get;
			set;
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
				bool somethingToNotify = false;
				foreach (var item in itemsList) {
					somethingToNotify |= Items.Remove (item);
				}

				if (somethingToNotify) {
					NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, itemsList);
					OnCollectionChanged (e);
				}
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
		/// Replace collection with new elements, first notifies removed items and after the added ones
		/// </summary>
		/// <param name="items">Items to replace with</param>
		public void Replace (IEnumerable<T> items)
		{
			RemoveRange (Items.Except (items));
			AddRange (items.Except (Items));
		}

		/// <summary>
		/// Replace all collection with new elements
		/// </summary>
		/// <param name="items">Items to replace</param>
		public void Reset (IEnumerable<T> items)
		{
			var list = items?.ToList ();
			if (Items.SequenceEqualSafe (list)) {
				return;
			}

			Items.Clear ();
			if (list != null) {
				foreach (var item in list) {
					Items.Add (item);
				}
			}
			NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset);
			OnCollectionChanged (e);
		}

		protected override void OnCollectionChanged (NotifyCollectionChangedEventArgs e)
		{
			if (!IgnoreEvents) {
				base.OnCollectionChanged (e);
				OnPropertyChanged (new PropertyChangedEventArgs (nameof (Count)));
			}
		}
	}
}

