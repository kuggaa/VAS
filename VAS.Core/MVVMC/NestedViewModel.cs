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
using System.Collections.Specialized;
using System.Linq;
using VAS.Core.Common;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;

namespace VAS.Core.MVVMC
{
	/// <summary>
	/// Base class implementation of <see cref="INestedViewModel"/> with support for child selection.
	/// This base class should be used by any ViewModel with a collection of typeparam name="VMChilds".
	/// </summary>
	public class NestedViewModel<VMChilds> : ViewModelBase, INestedViewModel<VMChilds>
		where VMChilds : IViewModel
	{
		RangeObservableCollection<VMChilds> viewModels;
		RangeObservableCollection<VMChilds> selection;

		public NestedViewModel ()
		{
			ViewModels = new RangeObservableCollection<VMChilds> ();
			Selection = new RangeObservableCollection<VMChilds> ();
		}

		protected override void DisposeManagedResources ()
		{
			ViewModels.IgnoreEvents = true;
			Selection.IgnoreEvents = true;
			base.DisposeManagedResources ();
			if (ViewModels != null && ViewModels.Any ()) {
				foreach (var viewModel in ViewModels) {
					viewModel.Dispose ();
				}
				ViewModels.Clear ();
			}
			ViewModels = null;
			Selection.Clear ();
		}

		/// <summary>
		/// Gets the collection of child ViewModel
		/// </summary>
		/// <value>The ViewModels collection.</value>
		public virtual RangeObservableCollection<VMChilds> ViewModels {
			get {
				return viewModels;
			}
			private set {
				viewModels = value;
			}
		}

		public RangeObservableCollection<VMChilds> FilteredViewModels { get; set; }

		/// <summary>
		/// Gets the current selection in the collection.
		/// </summary>
		/// <value>The selection.</value>
		public RangeObservableCollection<VMChilds> Selection {
			get {
				return selection;
			}
			private set {
				selection = value;
			}
		}

		/// <summary>
		/// Gets the count of visible children
		/// </summary>
		public int VisibleChildrenCount {
			get {
				return ViewModels.OfType<IVisible> ().Count (vm => vm.Visible);
			}
		}


		/// <summary>
		/// Selects the specified child viewModel.
		/// </summary>
		/// <param name="viewModel">The item to select.</param>
		public void Select (VMChilds viewModel)
		{
			if (viewModel == null) {
				Selection.Clear ();
				return;
			}

			if (Selection.Count == 0) {
				Selection.Add (viewModel);
			} else if (Selection.Count == 1) {
				Selection [0] = viewModel;
			} else {
				Selection.Reset (viewModel.ToEnumerable ());
			}
		}

		/// <summary>
		/// Gets the Interface INotifyCollectionChanged of the Child ViewModels
		/// </summary>
		/// <returns>The Collection as a INotifyCollectionChanged</returns>
		public virtual INotifyCollectionChanged GetNotifyCollection ()
		{
			return ViewModels;
		}

		/// <summary>
		/// Gets the Interface INotifyCollectionChanged of the Selection collection
		/// </summary>
		/// <returns>The Collection as a INotifyCollectionChanged</returns>
		public virtual INotifyCollectionChanged GetSelectionNotifyCollection ()
		{
			return Selection;
		}

		/// <summary>
		/// Gets the enumerator of the Child View Models Collection
		/// </summary>
		/// <returns>The enumerator.</returns>
		public virtual IEnumerator<VMChilds> GetEnumerator ()
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
	}
}

