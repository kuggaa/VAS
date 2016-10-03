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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Gtk;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using System.Linq;

namespace VAS.UI.Common
{
	[System.ComponentModel.ToolboxItem (true)]

	/// <summary>
	/// Tree view base implementation for MVVM.
	/// </summary>
	public class TreeViewBase<TCollectionViewModel, TModel, TViewModel> : Gtk.TreeView, IView<TCollectionViewModel>
		where TCollectionViewModel : NestedViewModel<TViewModel>
		where TViewModel : IViewModel<TModel>, new()
	{
		protected TreeStore store;
		TCollectionViewModel viewModel;
		protected Dictionary<IViewModel, TreeIter> dictionaryStore;
		protected Dictionary<INotifyCollectionChanged, TreeIter> dictionaryNestedParent;

		public TreeViewBase () : this (new Gtk.TreeStore (typeof(TViewModel)))
		{
		}

		public TreeViewBase (Gtk.TreeStore listStore)
		{
			Model = store = listStore;
			dictionaryStore = new Dictionary<IViewModel, TreeIter> ();
			dictionaryNestedParent = new Dictionary<INotifyCollectionChanged, TreeIter> ();
		}

		#region IView implementation

		public virtual void SetViewModel (object ViewModel)
		{
			this.ViewModel = ViewModel as TCollectionViewModel;
		}

		public TCollectionViewModel ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					ClearSubViewModels ();
				}
				viewModel = value;
				foreach (TViewModel item in viewModel.ViewModels) {
					AddSubViewModel (item, TreeIter.Zero);
				}
				viewModel.ViewModels.CollectionChanged += ViewModelCollectionChanged;
			}
		}

		#endregion

		void ViewModelCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			TreeIter parent = TreeIter.Zero;
			INotifyCollectionChanged sen = (sender as INotifyCollectionChanged);
			if (dictionaryNestedParent.ContainsKey (sen)) {
				parent = dictionaryNestedParent [sen];
			}
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				foreach (IViewModel item in e.NewItems) {
					AddSubViewModel (item, parent);
				}
				break;

			case NotifyCollectionChangedAction.Remove:
				foreach (IViewModel item in e.OldItems) {
					RemoveSubViewModel (item);
				}
				break;

			case NotifyCollectionChangedAction.Reset:
				ClearSubViewModelListeners (dictionaryStore.Keys.Where (ev => ev is TViewModel));
				store.Clear ();
				dictionaryStore.Clear ();
				dictionaryNestedParent.Clear ();
				break;

			case NotifyCollectionChangedAction.Move:
				break;
				
			case NotifyCollectionChangedAction.Replace:
				break;
			}
		}

		protected virtual void AddSubViewModel (IViewModel subViewModel, TreeIter parent)
		{
			TreeIter iter;
			subViewModel.PropertyChanged += PropertyChangedItem;
			if (!parent.Equals (TreeIter.Zero)) {
				iter = store.AppendValues (parent, subViewModel);
				dictionaryStore.Add (subViewModel, iter);
			} else {
				iter = store.AppendValues (subViewModel);
				dictionaryStore.Add (subViewModel, iter);
			}
			if (subViewModel is IEnumerable) {
				foreach (var v in (subViewModel as IEnumerable)) {
					AddSubViewModel (v as IViewModel, iter);
				}
				INotifyCollectionChanged notif = (subViewModel as INestedViewModel).GetNotifyCollection ();
				if (!dictionaryNestedParent.ContainsKey (notif)) {
					dictionaryNestedParent.Add (notif, iter);
					notif.CollectionChanged += ViewModelCollectionChanged;
				}
			}
		}

		void ClearSubViewModels ()
		{
			ClearSubViewModelListeners (viewModel.ViewModels as IEnumerable<IViewModel>);
			viewModel.ViewModels.CollectionChanged -= ViewModelCollectionChanged;
			store.Clear ();
			dictionaryStore.Clear ();
			dictionaryNestedParent.Clear ();
		}

		void ClearSubViewModelListeners (IEnumerable<IViewModel> collection)
		{
			foreach (IViewModel item in collection) {
				RemoveSubViewModelListener (item);
				if (item is INestedViewModel) {
					ClearAllNestedViewModelsListeners (item);
				}
			}
		}

		void ClearAllNestedViewModelsListeners (IViewModel subViewModel)
		{
			if (subViewModel is IEnumerable) {
				foreach (var v in (subViewModel as IEnumerable)) {
					ClearAllNestedViewModelsListeners (v as IViewModel);
				}
				(subViewModel as INestedViewModel).GetNotifyCollection ().CollectionChanged -= ViewModelCollectionChanged;
			}
		}

		void RemoveAllNestedSubViewModels (IViewModel subViewModel)
		{
			if (subViewModel is IEnumerable) {
				foreach (var v in (subViewModel as IEnumerable)) {
					RemoveAllNestedSubViewModels (v as IViewModel);
				}
				(subViewModel as INestedViewModel).GetNotifyCollection ().CollectionChanged -= ViewModelCollectionChanged;
				dictionaryNestedParent.Remove ((subViewModel as INestedViewModel).GetNotifyCollection ());
			}
			subViewModel.PropertyChanged -= PropertyChangedItem;
			TreeIter iter = dictionaryStore [subViewModel];
			if (store.Remove (ref iter)) {
				dictionaryStore.Remove (subViewModel);
			}
		}

		protected virtual void RemoveSubViewModel (IViewModel subViewModel)
		{
			RemoveSubViewModelListener (subViewModel);
			TreeIter iter = dictionaryStore [subViewModel];
			if (store.Remove (ref iter)) {
				dictionaryStore.Remove (subViewModel);
			}
			if (subViewModel is INestedViewModel) {
				RemoveAllNestedSubViewModels (subViewModel);
			}
		}

		void RemoveSubViewModelListener (IViewModel vm)
		{
			vm.PropertyChanged -= PropertyChangedItem;
		}

		void PropertyChangedItem (object sender, PropertyChangedEventArgs e)
		{
			if (!(sender is IViewModel)) {
				return;
			}
			TreeIter iter = dictionaryStore [(IViewModel)sender];
			Model.EmitRowChanged (store.GetPath (iter), iter);
			this.QueueDraw ();
		}
	}
}

