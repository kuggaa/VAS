//
//  Copyright (C) 2016 Fluendo S.A.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace VAS.Core.Common
{
	public class RangeObservableCollection<T> : ObservableCollection<T>
	{
		public void AddRange (IEnumerable<T> items)
		{
			IList<T> newItems = new List<T> ();
			foreach (var item in items) {
				if (!Items.Contains (item)) {
					Items.Add (item);
					newItems.Add (item);
				}
			}
			if (newItems.Any ()) {
				NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, newItems);
				OnCollectionChanged (e);
			}
		}

		public void RemoveRange (IEnumerable<T> items)
		{
			IList<T> oldItems = new List<T> ();
			foreach (var item in items) {
				if (Items.Contains (item)) {
					Items.Remove (item);
					oldItems.Add (item);
				}
			}
			if (oldItems.Any ()) {
				NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, oldItems);
				OnCollectionChanged (e);
			}
		}

		public void Replace (IEnumerable<T> items)
		{
			IList<T> newItems = new List<T> ();
			foreach (var item in items) {
				if (!Items.Contains (item)) {
					Items.Add (item);
					newItems.Add (item);
				}
			}
			IList<T> oldItems = new List<T> ();
			foreach (var item in Items.ToList()) {
				if (!items.Contains (item)) {
					Items.Remove (item);
					oldItems.Add (item);
				}
			}
			if (newItems.Any () || oldItems.Any ()) {
				NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, newItems, oldItems);
				OnCollectionChanged (e);
			}
		}
	}
}

