//
//  Copyright (C) 2016 Fluendo S.A.
//
//	This program is free software; you can redistribute it and/or modify
//	it under the terms of the GNU General Public License as published by
//	the Free Software Foundation; either version 2 of the License, or
//	(at your option) any later version.
//
//	This program is distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//	GNU General Public License for more details.
//
//		You should have received a copy of the GNU General Public License
//		along with this program; if not, write to the Free Software
//		Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Gtk;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using System.Threading.Tasks;

namespace VAS.UI.Common
{
	[System.ComponentModel.ToolboxItem (true)]

	/// <summary>
	/// Tree view base implementation for MVVM.
	/// </summary>
	public class TreeViewBase<T, M, VM> : Gtk.TreeView, IView<T> 
		where T : CollectionViewModel<M, VM> 
		where VM: IViewModel<M>, new()
	{
		protected ListStore store;
		T viewModel;
		Dictionary<VM, TreeIter> dictionaryStore;

		public TreeViewBase ()
		{
			Model = store = new Gtk.ListStore (typeof(VM));		
			dictionaryStore = new Dictionary<VM, TreeIter> ();
		}

		#region IView implementation

		public void SetViewModel (object ViewModel)
		{
			this.ViewModel = ViewModel as T;
		}

		public T ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					RemoveAllItemsChangeListener (viewModel.ViewModels);
					viewModel.ViewModels.CollectionChanged -= ViewModelCollectionChanged;
					store.Clear ();
				}
				viewModel = value;
				//AddAllItemsChangeListener (viewModel.ViewModels);
				foreach (VM item in viewModel.ViewModels) {
					AddItemChangeListener (item);
					TreeIter iter = store.AppendValues (item);
					dictionaryStore.Add (item, iter);
				}
				viewModel.ViewModels.CollectionChanged += ViewModelCollectionChanged;
			}
		}

		#endregion

		void ViewModelCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				foreach (VM item in e.NewItems) {
					AddItemChangeListener (item);
					TreeIter iter = store.AppendValues (item);
					dictionaryStore.Add (item, iter);
				}
				break;

			case NotifyCollectionChangedAction.Remove:
				foreach (VM item in e.OldItems) {
					RemoveItemChangeListener (item);
					TreeIter iter = dictionaryStore [item];
					if (store.Remove (ref iter)) {
						dictionaryStore.Remove (item);
					}
				}
				break;

			case NotifyCollectionChangedAction.Reset:
				RemoveAllItemsChangeListener (viewModel.ViewModels);
				store.Clear ();
				dictionaryStore.Clear ();
				break;

			case NotifyCollectionChangedAction.Move:
				break;
				
			case NotifyCollectionChangedAction.Replace:
				break;
			}
		}

		void AddAllItemsChangeListener (ObservableCollection<VM> collection)
		{
			foreach (VM item in collection) {
				AddItemChangeListener (item);
			}
		}

		void RemoveAllItemsChangeListener (ObservableCollection<VM> collection)
		{
			foreach (VM item in collection) {
				RemoveItemChangeListener (item);
			}
		}

		void AddItemChangeListener (VM vm)
		{
			vm.PropertyChanged += PropertyChangedItem;
		}

		void RemoveItemChangeListener (VM vm)
		{
			vm.PropertyChanged -= PropertyChangedItem;
		}

		void PropertyChangedItem (object sender, PropertyChangedEventArgs e)
		{
			if (!(sender is VM)) {
				return;
			}
			TreeIter iter = dictionaryStore [(VM)sender];
			Model.EmitRowChanged (Model.GetPath (iter), iter);
		}
	}
}

