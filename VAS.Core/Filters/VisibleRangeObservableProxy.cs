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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using VAS.Core.Common;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;

namespace VAS.Core.Filters
{
	/// <summary>
	/// RangeObservableCollection which only shows TVisibleViewModels that property Visible is equals 'true'.
	/// It also tracks all property changes and collection changes of rootCollection passed in ctor().
	/// </summary>
	public class VisibleRangeObservableProxy<TVisibleViewModel> : RangeObservableCollection<TVisibleViewModel> where TVisibleViewModel : ViewModelBase, IVisible
	{
		readonly List<TVisibleViewModel> propertyChangeList = new List<TVisibleViewModel> ();
		readonly IEnumerable<TVisibleViewModel> rootCollection;

		public VisibleRangeObservableProxy (IEnumerable<TVisibleViewModel> rootCollection)
		{
			this.rootCollection = rootCollection;
			if (rootCollection is INotifyCollectionChanged observableCollection) {
				observableCollection.CollectionChanged += HandleCollectionChanged;
			}
			foreach (var item in rootCollection) {
				if (item is INotifyPropertyChanged bindableObject) {
					bindableObject.PropertyChanged += HandlePropertyChanged;
				}
			}

			foreach (var item in rootCollection) {
				AddItem (item);
			}
			propertyChangeList.Clear ();
		}

		public VisibleRangeObservableProxy ()
		{
		}

		void AddItem (TVisibleViewModel viewModel)
		{
			if (viewModel.Visible)
			{
				Add (viewModel);
			}
		}

		void RemoveItem (TVisibleViewModel viewModel)
		{
			Remove (viewModel);
		}

		void HandleCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add) {
				var newItems = e.NewItems.OfType<TVisibleViewModel> ().Where (tVisibleViewModel => tVisibleViewModel.Visible);
				foreach (var item in newItems) {
					item.PropertyChanged += HandlePropertyChanged;
					AddItem (item);
				}
			} else if (e.Action == NotifyCollectionChangedAction.Remove) {
				var oldItems = e.OldItems.OfType<TVisibleViewModel> ();
				foreach (var item in oldItems) {
					item.PropertyChanged -= HandlePropertyChanged;
					RemoveItem (item);
				}
			} else if (e.Action == NotifyCollectionChangedAction.Reset) {
				foreach (var item in this) {
					item.PropertyChanged -= HandlePropertyChanged;
				}
				Clear ();

				foreach (var item in rootCollection) {
					item.PropertyChanged += HandlePropertyChanged;
					AddItem (item);
				}
			}

		}
		/// <summary>
		/// Applies the property changes for all items that changed its Visible property.
		/// </summary>
		public void ApplyPropertyChanges ()
		{
			AddRange (propertyChangeList.Where (v => v.Visible));
			RemoveRange (propertyChangeList.Where (v => !v.Visible));
			propertyChangeList.Clear ();
		}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (IVisible.Visible))
			{
				if (propertyChangeList.Contains (sender as TVisibleViewModel))
				{
					propertyChangeList.Remove (sender as TVisibleViewModel);
				}
				propertyChangeList.Add (sender as TVisibleViewModel);
			}
		}
	}
}
