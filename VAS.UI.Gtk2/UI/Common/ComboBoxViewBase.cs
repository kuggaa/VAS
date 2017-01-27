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
	/// Combo box view base implementation for MVVM.
	/// </summary>
	public class ComboBoxViewBase<T, M, VM> : ComboBox, IView<T>
		where T : CollectionViewModel<M, VM>
		where VM : IViewModel<M>, new()
	{
		protected ListStore store;
		T viewModel;
		Dictionary<VM, TreeIter> dictionaryStore;

		public ComboBoxViewBase ()
		{
			Model = store = new Gtk.ListStore (typeof (VM));
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
					ClearSubViewModels ();
				}
				viewModel = value;
				if (viewModel != null) {
					foreach (VM item in viewModel.ViewModels) {
						AddSubViewModel (item);
					}
					viewModel.ViewModels.CollectionChanged += ViewModelCollectionChanged;
				}
			}
		}

		#endregion

		/// <summary>
		/// Gets the selected view model.
		/// </summary>
		/// <returns><c>true</c>, if selected view model was gotten, <c>false</c> otherwise.</returns>
		/// <param name="viewModel">View model.</param>
		public bool GetSelectedViewModel (out VM viewModel)
		{
			TreeIter iter;
			if (!this.GetActiveIter (out iter)) {
				viewModel = new VM ();
				return false;
			}
			viewModel = (VM)Model.GetValue (iter, 0);
			return true;
		}

		void ViewModelCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				foreach (VM item in e.NewItems) {
					AddSubViewModel (item);
				}
				break;

			case NotifyCollectionChangedAction.Remove:
				foreach (VM item in e.OldItems) {
					RemoveSubViewModel (item);
				}
				break;

			case NotifyCollectionChangedAction.Reset:
				ViewModel = ViewModel;
				break;

			case NotifyCollectionChangedAction.Move:
				break;

			case NotifyCollectionChangedAction.Replace:
				break;
			}
		}

		void AddSubViewModel (VM subViewModel)
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

		void ClearSubViewModelListeners (ObservableCollection<VM> collection)
		{
			foreach (VM item in collection) {
				RemoveSubViewModelListener (item);
			}
		}

		void RemoveSubViewModel (VM subViewModel)
		{
			RemoveSubViewModelListener (subViewModel);
			TreeIter iter = dictionaryStore [subViewModel];
			if (store.Remove (ref iter)) {
				dictionaryStore.Remove (subViewModel);
			}
		}

		void RemoveSubViewModelListener (VM vm)
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

