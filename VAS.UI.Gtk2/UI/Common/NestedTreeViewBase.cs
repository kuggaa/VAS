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
using System.Linq;
using Gtk;
using VAS.Core.Interfaces.MVVMC;

namespace VAS.UI.Common
{
	/// <summary>
	/// Tree view base overrided implementation for ViewModels that had another Nested ViewModel.
	/// </summary>
	public class NestedTreeViewBase<TCollectionViewModel, TModel, TViewModel, TChildViewModel> : TreeViewBase<TCollectionViewModel, TModel, TViewModel>
		where TCollectionViewModel : class, INestedViewModel<TViewModel>
		where TChildViewModel : class, IViewModel
		where TViewModel : class, IViewModel<TModel>, INestedViewModel<TChildViewModel>, new()
	{
		protected override void HandleTreeviewSelectionChanged (object sender, EventArgs e)
		{
			if (ViewModel == null) {
				return;
			}
			Dictionary<TViewModel, List<TChildViewModel>> selected =
				new Dictionary<TViewModel, List<TChildViewModel>> ();
			// Iterate selected rows
			foreach (var path in Selection.GetSelectedRows ()) {
				// Try if selected is parent
				TViewModel selectedViewModel = GetValue (path.ToString ()) as TViewModel;
				if (selectedViewModel != null) {
					selected.Add (selectedViewModel, new List<TChildViewModel> ());
				} else {
					// Try if selected is a child
					TChildViewModel selectedViewModelChild = GetValue (path.ToString ()) as TChildViewModel;
					if (selectedViewModelChild != null) {
						// Get parent
						TViewModel vm = GetParentVM (path);
						if (!selected.ContainsKey (vm)) {
							selected.Add (vm, new List<TChildViewModel> { selectedViewModelChild });
						} else {
							selected [vm].Add (selectedViewModelChild);
						}
					}
				}
			}

			// Replace parent selection with our selected list, only if it had changed
			var oldSelections = ViewModel.Selection.Except (selected.Keys);
			if (oldSelections.Any ()) {
				foreach (var parent in oldSelections) {
					if (parent.Selection.Any ()) {
						parent.Selection.Clear ();
					}
				}
				ViewModel.Selection.Replace (selected.Keys);
			}

			foreach (var parentSon in selected) {
				if (parentSon.Value.Any ()) {
					parentSon.Key.Selection.Replace (parentSon.Value);
				}
			}
		}

		object GetValue (string path)
		{
			TreeIter iter;

			Model.GetIterFromString (out iter, path);
			return Model.GetValue (iter, COL_DATA);
		}

		TViewModel GetParentVM (TreePath path)
		{
			TreeIter iterParent, iter;

			store.GetIterFromString (out iter, path.ToString ());
			store.IterParent (out iterParent, iter);
			return store.GetValue (iterParent, COL_DATA) as TViewModel;
		}
	}
}
