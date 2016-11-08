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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Gtk;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using Misc = VAS.UI.Helpers.Misc;

namespace VAS.UI.Common
{
	/// <summary>
	/// Tree view base implementation for MVVM.
	/// </summary>
	public class TreeViewBase<TCollectionViewModel, TModel, TViewModel> : TreeView, IView<TCollectionViewModel>
		where TCollectionViewModel : class, INestedViewModel<TViewModel>
		where TViewModel : class, IViewModel<TModel>, new()
	{
		protected const int COL_DATA = 0;
		protected TreeStore store;
		TCollectionViewModel viewModel;
		protected Dictionary<IViewModel, TreeIter> dictionaryStore;
		protected Dictionary<INotifyCollectionChanged, TreeIter> dictionaryNestedParent;

		protected TreeModelFilter filter;
		protected TreeModelSort sort;

		protected TViewModel activatedViewModel;
		protected Menu menu;

		public TreeViewBase () : this (new TreeStore (typeof (TViewModel)))
		{
		}

		public TreeViewBase (TreeStore treeStore)
		{
			Model = store = treeStore;
			dictionaryStore = new Dictionary<IViewModel, TreeIter> ();
			dictionaryNestedParent = new Dictionary<INotifyCollectionChanged, TreeIter> ();
			Selection.Changed += HandleTreeviewSelectionChanged;
			RowActivated += HandleTreeviewRowActivated;
		}

		#region IView implementation

		public virtual void SetViewModel (object viewModel)
		{
			this.ViewModel = viewModel as TCollectionViewModel;
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
				CreateFilterAndSort ();
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
				ClearSubViewModelListeners (dictionaryStore.Keys.OfType<TViewModel> ());
				store.Clear ();
				dictionaryStore.Clear ();
				dictionaryNestedParent.Clear ();
				break;

			case NotifyCollectionChangedAction.Move:
				break;

			case NotifyCollectionChangedAction.Replace:
				break;
			}

			filter?.Refilter ();
		}

		protected virtual void AddSubViewModel (IViewModel subViewModel, TreeIter parent)
		{
			TreeIter iter;
			(subViewModel as INotifyPropertyChanged).PropertyChanged += PropertyChangedItem;
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
			ClearSubViewModelListeners (viewModel.ViewModels);
			viewModel.ViewModels.CollectionChanged -= ViewModelCollectionChanged;
			store.Clear ();
			dictionaryStore.Clear ();
			dictionaryNestedParent.Clear ();
		}

		void ClearSubViewModelListeners (IEnumerable<TViewModel> collection)
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
			RemoveSubViewModelListener (subViewModel);
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
			store.Remove (ref iter);
			dictionaryStore.Remove (subViewModel);
		}

		protected virtual void RemoveSubViewModel (IViewModel subViewModel)
		{
			RemoveSubViewModelListener (subViewModel);
			TreeIter iter = dictionaryStore [subViewModel];
			if (subViewModel is INestedViewModel) {
				RemoveAllNestedSubViewModels (subViewModel);
			} else {
				store.Remove (ref iter);
				dictionaryStore.Remove (subViewModel);
			}
		}

		void RemoveSubViewModelListener (IViewModel vm)
		{
			vm.PropertyChanged -= PropertyChangedItem;
		}

		void PropertyChangedItem (object sender, PropertyChangedEventArgs e)
		{
			var senderVM = sender as IViewModel;
			if (senderVM == null || Model == null || !dictionaryStore.ContainsKey (senderVM)) {
				return;
			}
			TreeIter iter = dictionaryStore [senderVM];
			store.EmitRowChanged (store.GetPath (iter), iter);
			filter?.Refilter ();
			this.QueueDraw ();
		}

		protected override bool OnButtonPressEvent (Gdk.EventButton evnt)
		{
			bool ret = true;
			TreePath [] paths = Selection.GetSelectedRows ();

			if (Misc.RightButtonClicked (evnt)) {
				// We don't want to unselect the play when several
				// plays are selected and we click the right button
				// For multiedition
				if (paths.Length <= 1) {
					ret = base.OnButtonPressEvent (evnt);
					paths = Selection.GetSelectedRows ();
				}

				ShowMenu ();
			} else {
				ret = base.OnButtonPressEvent (evnt);
			}
			return ret;
		}

		#region Virtual methods

		protected virtual void ShowMenu ()
		{
		}

		protected virtual void CreateFilterAndSort ()
		{
			filter = new TreeModelFilter (store, null);
			filter.VisibleFunc = new TreeModelFilterVisibleFunc (HandleFilter);
			sort = new TreeModelSort (filter);
			sort.SetSortFunc (COL_DATA, HandleSort);
			sort.SetSortColumnId (COL_DATA, SortType.Ascending);
			Model = sort;
		}

		protected virtual void HandleTreeviewRowActivated (object o, RowActivatedArgs args)
		{
			TreeIter iter;
			Model.GetIter (out iter, args.Path);
			activatedViewModel = Model.GetValue (iter, COL_DATA) as TViewModel;
		}

		protected virtual void HandleTreeviewSelectionChanged (object sender, EventArgs e)
		{
			TreeIter iter;
			List<TViewModel> selected = new List<TViewModel> ();

			foreach (var path in Selection.GetSelectedRows ()) {
				Model.GetIterFromString (out iter, path.ToString ());
				TViewModel selectedViewModel = Model.GetValue (iter, COL_DATA) as TViewModel;
				if (selectedViewModel != null) {
					selected.Add (selectedViewModel);
				}
			}
			ViewModel.SelectionReplace (selected);
			filter?.Refilter ();
		}

		protected virtual void HandleViewModelPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "SortType") {
				/* Hack to make it actually resort */
				sort.SetSortFunc (COL_DATA, HandleSort);
			}

			if (e.PropertyName == "FilterText") {
				filter?.Refilter ();
			}

			if (e.PropertyName == "Selection") {
				//Sincronization of the first external selection
				if (ViewModel.Selection.Count == 1 && Selection.CountSelectedRows () == 0) {
					TreeIter externalSelected = dictionaryStore [ViewModel.Selection.FirstOrDefault ()];
					externalSelected = filter.ConvertChildIterToIter (externalSelected);
					externalSelected = sort.ConvertChildIterToIter (externalSelected);
					Selection.SelectIter (externalSelected);
				}
			}
		}

		protected virtual int HandleSort (TreeModel model, TreeIter a, TreeIter b)
		{
			// FIXME: Implement a generic sort for all TViewModels
			return 0;
		}

		protected virtual bool HandleFilter (TreeModel model, TreeIter iter)
		{
			IVisible vm = model.GetValue (iter, COL_DATA) as IVisible;
			return (vm?.Visible ?? true);
		}

		#endregion
	}
}

