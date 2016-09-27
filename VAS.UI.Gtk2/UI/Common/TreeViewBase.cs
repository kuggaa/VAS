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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Gtk;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;

namespace VAS.UI.Common
{
	[System.ComponentModel.ToolboxItem (true)]

	/// <summary>
	/// Tree view base implementation for MVVM.
	/// </summary>
	public class TreeViewBase<TCollectionViewModel, TModel, TViewModel> : Gtk.TreeView, IView<TCollectionViewModel>
		where TCollectionViewModel : CollectionViewModel<TModel, TViewModel>
		where TViewModel: IViewModel<TModel>, new()
	{
		protected ListStore store;
		TCollectionViewModel viewModel;
		protected Dictionary<TViewModel, TreeIter> dictionaryStore;

		public TreeViewBase () : this (new Gtk.ListStore (typeof(TViewModel)))
		{
		}

		public TreeViewBase (Gtk.ListStore listStore)
		{
			Model = store = listStore;
			dictionaryStore = new Dictionary<TViewModel, TreeIter> ();
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
					AddSubViewModel (item);
				}
				viewModel.ViewModels.CollectionChanged += ViewModelCollectionChanged;
			}
		}

		#endregion

		void ViewModelCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				foreach (TViewModel item in e.NewItems) {
					AddSubViewModel (item);
				}
				break;

			case NotifyCollectionChangedAction.Remove:
				foreach (TViewModel item in e.OldItems) {
					RemoveSubViewModel (item);
				}
				break;

			case NotifyCollectionChangedAction.Reset:
				ClearSubViewModelListeners (dictionaryStore.Keys);
				store.Clear ();
				dictionaryStore.Clear ();
				break;

			case NotifyCollectionChangedAction.Move:
				break;
				
			case NotifyCollectionChangedAction.Replace:
				break;
			}
		}

		protected virtual void AddSubViewModel (TViewModel subViewModel)
		{
			subViewModel.PropertyChanged += PropertyChangedItem;
			TreeIter iter = store.AppendValues (subViewModel);
			dictionaryStore.Add (subViewModel, iter);
		}

		void ClearSubViewModels ()
		{
			ClearSubViewModelListeners (viewModel.ViewModels);
			viewModel.ViewModels.CollectionChanged -= ViewModelCollectionChanged;
			store.Clear ();
			dictionaryStore.Clear ();
		}

		void ClearSubViewModelListeners (IEnumerable<TViewModel> collection)
		{
			foreach (TViewModel item in collection) {
				RemoveSubViewModelListener (item);
			}
		}

		protected virtual void RemoveSubViewModel (TViewModel subViewModel)
		{
			RemoveSubViewModelListener (subViewModel);
			TreeIter iter = dictionaryStore [subViewModel];
			if (store.Remove (ref iter)) {
				dictionaryStore.Remove (subViewModel);
			}
		}

		void RemoveSubViewModelListener (TViewModel vm)
		{
			vm.PropertyChanged -= PropertyChangedItem;
		}

		void PropertyChangedItem (object sender, PropertyChangedEventArgs e)
		{
			if (!(sender is TViewModel)) {
				return;
			}
			TreeIter iter = dictionaryStore [(TViewModel)sender];
			Model.EmitRowChanged (store.GetPath (iter), iter);
		}
	}
}

