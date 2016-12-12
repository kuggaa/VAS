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
using Gdk;
using Gtk;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using Misc = VAS.UI.Helpers.Misc;
using Point = VAS.Core.Common.Point;

namespace VAS.UI.Common
{
	[System.ComponentModel.ToolboxItem (true)]
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

		//DragDrop variables
		protected TargetList targets;
		Point dragStart;
		bool dragging, dragStarted, enableDragSource;

		public TreeViewBase () : this (new TreeStore (typeof (TViewModel)))
		{
		}

		public TreeViewBase (TreeStore treeStore)
		{
			Model = store = treeStore;
			dictionaryStore = new Dictionary<IViewModel, TreeIter> ();
			dictionaryNestedParent = new Dictionary<INotifyCollectionChanged, TreeIter> ();
			Selection.SelectFunction = SelectFunction;
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
				int i = 0;
				foreach (TViewModel item in viewModel.ViewModels) {
					AddSubViewModel (item, TreeIter.Zero, i);
					i++;
				}
				viewModel.ViewModels.CollectionChanged += ViewModelCollectionChanged;
			}
		}

		#endregion

		protected void CreateFilterAndSort ()
		{
			filter = new TreeModelFilter (store, null);
			filter.VisibleFunc = new TreeModelFilterVisibleFunc (HandleFilter);
			sort = new TreeModelSort (filter);
			sort.SetSortFunc (COL_DATA, HandleSort);
			sort.SetSortColumnId (COL_DATA, SortType.Ascending);
			Model = sort;
		}

		protected void CreateDragSource (TargetEntry [] targetEntries)
		{
			enableDragSource = true;
			targets = new TargetList (targetEntries);
			EnableModelDragSource (ModifierType.None, targetEntries, DragAction.Default);
		}

		protected void CreateDragDest (TargetEntry [] targetEntries)
		{
			EnableModelDragDest (targetEntries, DragAction.Default);
		}

		void ViewModelCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			TreeIter parent = TreeIter.Zero;
			INotifyCollectionChanged sen = (sender as INotifyCollectionChanged);
			if (dictionaryNestedParent.ContainsKey (sen)) {
				parent = dictionaryNestedParent [sen];
			}
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				int i = 0;
				foreach (IViewModel item in e.NewItems) {
					AddSubViewModel (item, parent, e.NewStartingIndex + i);
					i++;
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

		protected virtual void AddSubViewModel (IViewModel subViewModel, TreeIter parent, int index)
		{
			TreeIter iter;
			(subViewModel as INotifyPropertyChanged).PropertyChanged += PropertyChangedItem;
			if (!parent.Equals (TreeIter.Zero)) {
				iter = store.InsertWithValues (parent, index, subViewModel);
				dictionaryStore.Add (subViewModel, iter);
			} else {
				iter = store.InsertWithValues (index, subViewModel);
				dictionaryStore.Add (subViewModel, iter);
			}
			if (subViewModel is IEnumerable) {
				index = 0;
				foreach (var v in (subViewModel as IEnumerable)) {
					AddSubViewModel (v as IViewModel, iter, index++);
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
				if (paths.Length > 0 && enableDragSource) {
					dragging = true;
					dragStarted = false;
					dragStart = new Point (evnt.X, evnt.Y);
				}
			}
			return ret;
		}

		protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
		{
			return false;
		}

		protected override bool OnButtonReleaseEvent (EventButton evnt)
		{
			dragging = dragStarted = false;
			return base.OnButtonReleaseEvent (evnt);
		}

		protected override bool OnMotionNotifyEvent (EventMotion evnt)
		{
			if (dragging && !dragStarted) {
				if (dragStart.Distance (new Point (evnt.X, evnt.Y)) > 5) {
					Gtk.Drag.Begin (this, targets, DragAction.Default, 1, evnt);
					dragStarted = true;
				}
			}
			return base.OnMotionNotifyEvent (evnt);
		}

		protected override void OnDragBegin (DragContext context)
		{
			base.OnDragBegin (context);
			TreePath [] paths = Selection.GetSelectedRows ();
			TreeIter iter;
			Model.GetIter (out iter, paths [0]);
			object firstDraggedElement = Model.GetValue (iter, COL_DATA);
			App.Current.DragContext.SourceDataType = firstDraggedElement.GetType ();
		}

		protected override void OnDragDataGet (DragContext context, SelectionData selectionData, uint info, uint time)
		{
			List<IViewModel> draggedViewModels = new List<IViewModel> ();
			var paths = Selection.GetSelectedRows ();
			foreach (var path in paths) {
				TreeIter iter;
				Model.GetIter (out iter, path);
				var vm = (IViewModel)Model.GetValue (iter, COL_DATA);
				if (vm != null) {
					draggedViewModels.Add (vm);
				}
			}
			App.Current.DragContext.SourceData = draggedViewModels;
		}

		protected override void OnDragDataReceived (DragContext context, int x, int y, SelectionData selectionData, uint info, uint time)
		{
			List<IViewModel> draggedElements = App.Current.DragContext.SourceData as List<IViewModel>;
			bool success = HandleDragReceived (draggedElements, x, y, Gtk.Drag.GetSourceWidget (context) == this);
			Gtk.Drag.Finish (context, success, false, time);
		}

		protected override bool OnDragMotion (DragContext context, int x, int y, uint time)
		{
			TreeIter iter;
			TreePath path;
			TreeViewDropPosition pos;

			if (GetDestRowAtPos (x, y, out path, out pos)) {
				Model.GetIter (out iter, path);
				IViewModel element = Model.GetValue (iter, COL_DATA) as IViewModel;
				if (AllowDrop (element)) {
					DragAction action = DragAction.Copy;
					if (Gtk.Drag.GetSourceWidget (context) == this) {
						action = DragAction.Move;
					}
					DisableDragInto (path, context, time, pos);
					Gdk.Drag.Status (context, action, time);
					return true;
				}
			}
			return false;
		}

		protected override void OnDragEnd (DragContext context)
		{
			App.Current.DragContext.SourceDataType = null;
			App.Current.DragContext.SourceData = null;
			base.OnDragEnd (context);
		}

		protected override bool OnDragFailed (DragContext drag_context, DragResult drag_result)
		{
			App.Current.DragContext.SourceDataType = null;
			App.Current.DragContext.SourceData = null;
			return base.OnDragFailed (drag_context, drag_result);
		}

		#region Virtual methods

		protected virtual void ShowMenu ()
		{
		}

		protected virtual void HandleTreeviewRowActivated (object o, RowActivatedArgs args)
		{
			TreeIter iter;
			Model.GetIter (out iter, args.Path);
			activatedViewModel = Model.GetValue (iter, COL_DATA) as TViewModel;
		}

		protected virtual bool SelectFunction (TreeSelection selection, TreeModel model, TreePath path, bool selected)
		{
			TreePath [] selectedRows;

			selectedRows = selection.GetSelectedRows ();
			if (!selected && selectedRows.Length > 0) {
				object currentSelected;
				object firstSelected;
				TreeIter iter;
				model.GetIter (out iter, selectedRows [0]);
				firstSelected = model.GetValue (iter, COL_DATA);
				model.GetIter (out iter, path);
				currentSelected = model.GetValue (iter, COL_DATA);
				if (currentSelected.GetType ().IsAssignableFrom (firstSelected.GetType ())) {
					return true;
				}
				return false;
			}
			return true;
		}

		protected virtual void HandleTreeviewSelectionChanged (object sender, EventArgs e)
		{
			if (ViewModel == null) {
				return;
			}

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
		}

		protected virtual void HandleViewModelPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "SortType") {
				/* Hack to make it actually resort */
				sort?.SetSortFunc (COL_DATA, HandleSort);
			}

			if (e.PropertyName == "FilterText") {
				filter?.Refilter ();
			}

			if (e.PropertyName == "Selection") {
				//Sincronization of the first external selection
				if (ViewModel.Selection.Count == 1 && Selection.CountSelectedRows () == 0) {
					TreeIter externalSelected = dictionaryStore [ViewModel.Selection.FirstOrDefault ()];
					if (filter != null) {
						externalSelected = filter.ConvertChildIterToIter (externalSelected);
						externalSelected = sort.ConvertChildIterToIter (externalSelected);
					}
					Selection.SelectIter (externalSelected);
				}
			}
		}

		protected virtual int HandleSort (TreeModel model, TreeIter a, TreeIter b)
		{
			TreePath pathA = model.GetPath (a);
			TreePath pathB = model.GetPath (b);
			return pathA.Compare (pathB);
		}

		protected virtual bool HandleFilter (TreeModel model, TreeIter iter)
		{
			IVisible vm = model.GetValue (iter, COL_DATA) as IVisible;
			return (vm?.Visible ?? true);
		}

		protected virtual bool AllowDrop (IViewModel destination)
		{
			return App.Current.DragContext.SourceDataType.IsAssignableFrom (destination.GetType ());
		}

		protected virtual bool HandleDragReceived (List<IViewModel> draggedViewModels, int x, int y, bool internalDrop)
		{
			IViewModel destParentVM, destChildVM;
			TreePath path;
			TreeViewDropPosition pos;
			TreeIter iter;
			var elementsToRemove = new Dictionary<INestedViewModel, List<IViewModel>> ();
			var elementsToAdd = new KeyValuePair<INestedViewModel, List<IViewModel>> ();
			if (GetDestRowAtPos (x, y, out path, out pos)) {
				int destIndex;
				store.GetIter (out iter, path);
				FillParentAndChild (iter, path, pos, out destParentVM, out destChildVM);
				store.GetIter (out iter, path);
				int [] Indices = store.GetPath (iter).Indices;
				if (pos == TreeViewDropPosition.Before ||
							pos == TreeViewDropPosition.IntoOrBefore) {
					destIndex = Indices [Indices.Length - 1];
				} else {
					destIndex = Indices [Indices.Length - 1] + 1;
				}
				if (destChildVM == null) {
					if (internalDrop) {
						elementsToRemove.Add (ViewModel as INestedViewModel, draggedViewModels.ToList ());
					}
					elementsToAdd = new KeyValuePair<INestedViewModel, List<IViewModel>> (ViewModel as INestedViewModel, draggedViewModels);
				} else {
					if (internalDrop) {
						TreeIter parent;
						//Get the parentVM for every drag ViewModel
						foreach (IViewModel vm in draggedViewModels) {
							iter = dictionaryStore [vm];
							if (Model.IterParent (out parent, iter)) {
								INestedViewModel sourceParentVM = Model.GetValue (parent, COL_DATA) as INestedViewModel;
								if (!elementsToRemove.ContainsKey (sourceParentVM)) {
									elementsToRemove.Add (sourceParentVM, new List<IViewModel> ());
								}
								elementsToRemove [sourceParentVM].Add (vm);
							}
						}
					}
					elementsToAdd = new KeyValuePair<INestedViewModel, List<IViewModel>> (destParentVM as INestedViewModel, draggedViewModels);
				}
				return MoveElements (elementsToRemove, elementsToAdd, destIndex);
			}
			return false;
		}

		protected virtual bool MoveElements (Dictionary<INestedViewModel, List<IViewModel>> elementsToRemove,
											 KeyValuePair<INestedViewModel, List<IViewModel>> elementsToAdd, int index)
		{
			return false;
		}

		#endregion

		void FillParentAndChild (TreeIter iter, TreePath path, TreeViewDropPosition pos, out IViewModel parentVM, out IViewModel childVM)

		{
			TreeIter parent;

			IViewModel obj = Model.GetValue (iter, 0) as IViewModel;
			if (Model.IterParent (out parent, iter)) {
				parentVM = Model.GetValue (parent, 0) as IViewModel;
				childVM = obj;
			} else {
				parentVM = obj;
				childVM = null;
			}
		}

		void DisableDragInto (TreePath path, DragContext context, uint time, TreeViewDropPosition pos)
		{
			if (pos == TreeViewDropPosition.IntoOrAfter) {
				pos = TreeViewDropPosition.After;
			} else if (pos == TreeViewDropPosition.IntoOrBefore) {
				pos = TreeViewDropPosition.Before;
			}
			SetDragDestRow (path, pos);
			Gdk.Drag.Status (context, context.SuggestedAction, time);
		}
	}
}

