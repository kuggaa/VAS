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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;

namespace VAS.Core.MVVMC
{
	/// <summary>
	/// Base class of a nested ViewModel Type. This type of ViewModel contains an observableCollection of Child
	/// ViewModels. It implements INotifyCollectionChanged and IEnumerable interfaces.
	/// </summary>
	public class NestedViewModel<VMChilds> : BindableBase, INestedViewModel<VMChilds>
		where VMChilds : IViewModel
	{
		public NestedViewModel ()
		{
			ViewModels = new ObservableCollection<VMChilds> ();
			Selection = new RangeObservableCollection<VMChilds> ();
			Selection.CollectionChanged += HandleSelectionChanged;
		}

		/// <summary>
		/// Gets the collection of child ViewModel
		/// </summary>
		/// <value>The ViewModels collection.</value>
		public virtual ObservableCollection<VMChilds> ViewModels {
			protected set;
			get;
		}

		/// <summary>
		/// Gets the current selection in the collection.
		/// </summary>
		/// <value>The selection.</value>
		public RangeObservableCollection<VMChilds> Selection {
			get;
			private set;
		}

		/// <summary>
		/// Selects the specified item from the list.
		/// </summary>
		/// <param name="item">The item to select.</param>
		public void Select (VMChilds viewModel)
		{
			if (viewModel == null) {
				return;
			}

			if (Selection.Count == 0) {
				Selection.Add (viewModel);
			} else if (Selection.Count == 1) {
				Selection [0] = viewModel;
			} else {
				Selection.Clear ();
				Selection.Add (viewModel);
			}
		}

		public void SelectionReplace (IEnumerable<VMChilds> selection)
		{
			Selection.Replace (selection);
		}

		/// <summary>
		/// Gets the Interface INotifyCollectionChanged of the Child ViewModels
		/// </summary>
		/// <returns>The Collection as a INotifyCollectionChanged</returns>
		public INotifyCollectionChanged GetNotifyCollection ()
		{
			return ViewModels;
		}

		/// <summary>
		/// Gets the enumerator of the Child View Models Collection
		/// </summary>
		/// <returns>The enumerator.</returns>
		public IEnumerator<VMChilds> GetEnumerator ()
		{
			return ViewModels.GetEnumerator ();
		}

		/// <summary>
		/// Gets the enumerator of the Child View Models Collection
		/// </summary>
		/// <returns>The enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return ViewModels.GetEnumerator ();
		}

		void HandleSelectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			RaisePropertyChanged ("Selection");
		}
	}
}

