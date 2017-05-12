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
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// Generic base class for <see cref="ITemplate"/> ViewModel.
	/// </summary>
	public abstract class TemplateViewModel<T> : StorableVM<T> where T : class, ITemplate
	{
		/// <summary>
		/// Gets the name of the template.
		/// </summary>
		/// <value>The name.</value>
		public string Name {
			get {
				return Model?.Name;
			}
			set {
				Model.Name = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the template is editable.
		/// </summary>
		/// <value><c>true</c> if editable; otherwise, <c>false</c>.</value>
		public bool Editable {
			get {
				return Model?.Static == false;
			}
		}

		/// <summary>
		/// Gets or sets the icon used for the template.
		/// </summary>
		/// <value>The icon.</value>
		public abstract Image Icon {
			get;
			set;
		}

		/// <summary>
		/// Gets a value indicating whether the template has been edited.
		/// </summary>
		/// <value><c>true</c> if edited; otherwise, <c>false</c>.</value>
		public bool Edited {
			get {
				return Model?.IsChanged == true;
			}
		}
	}

	public abstract class TemplateViewModel<TModel, TChildModel, TChildViewModel> : TemplateViewModel<TModel>, INestedViewModel<TChildViewModel>
		where TModel : class, ITemplate<TChildModel>
		where TChildViewModel : IViewModel<TChildModel>, new()
		where TChildModel : BindableBase
	{
		public TemplateViewModel ()
		{
			Selection = new RangeObservableCollection<TChildViewModel> ();
			SubViewModel = CreateSubViewModel ();
			GetNotifyCollection ().CollectionChanged += HandleViewModelsChanged;
		}

		public CollectionViewModel<TChildModel, TChildViewModel> SubViewModel {
			get;
			set;
		}

		#region Implement INestedViewModel

		/// <summary>
		/// Gets the selection.
		/// </summary>
		/// <value>The selection.</value>
		public RangeObservableCollection<TChildViewModel> Selection {
			get;
			protected set;
		}

		/// <summary>
		/// Gets the enumerator of the Child View Models Collection
		/// </summary>
		/// <returns>The enumerator.</returns>
		public IEnumerator<TChildViewModel> GetEnumerator ()
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

		/// Gets the Interface INotifyCollectionChanged of the Child ViewModels
		/// </summary>
		/// <returns>The Collection as a INotifyCollectionChanged</returns>
		public INotifyCollectionChanged GetNotifyCollection ()
		{
			return ViewModels;
		}

		/// <summary>
		/// Gets the Interface INotifyCollectionChanged of the Selection collection
		/// </summary>
		/// <returns>The Collection as a INotifyCollectionChanged</returns>
		public INotifyCollectionChanged GetSelectionNotifyCollection ()
		{
			return Selection;
		}

		/// <summary>
		/// Gets the collection of child ViewModel
		/// </summary>
		/// <value>The ViewModels collection.</value>
		public RangeObservableCollection<TChildViewModel> ViewModels {
			get {
				return SubViewModel.ViewModels;
			}
		}

		/// <summary>
		/// Replaces the Selection with the provided one
		/// </summary>
		/// <param name="selection">Selection.</param>
		public void SelectionReplace (IEnumerable<TChildViewModel> selection)
		{
			Selection.Replace (selection);
		}

		/// <summary>
		/// Selects the current object.
		/// </summary>
		/// <param name="selected">Selected element.</param>
		/// <param name="clearSelection">If <c>true</c>, clears the previous selection</param>
		public void Select (TChildViewModel selected, bool clearSelection = true)
		{
			if (clearSelection) {
				Selection.Replace (new List<TChildViewModel> { selected });
			} else {
				Selection.Add (selected);
			}
		}

		/// <summary>
		/// Creates the sub view model.
		/// </summary>
		/// <returns>The sub view model.</returns>
		public virtual CollectionViewModel<TChildModel, TChildViewModel> CreateSubViewModel ()
		{
			return new CollectionViewModel<TChildModel, TChildViewModel> ();
		}

		#endregion

		protected override void SyncLoadedModel ()
		{
			base.SyncLoadedModel ();
			SubViewModel.Model = Model?.List;
		}

		void HandleViewModelsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Remove:
				Selection.RemoveRange (Selection.Intersect (e.OldItems.OfType<TChildViewModel> ()));
				break;
			case NotifyCollectionChangedAction.Replace:
				Selection.RemoveRange (Selection.Intersect (e.OldItems.OfType<TChildViewModel> ()));
				break;
			case NotifyCollectionChangedAction.Reset:
				Selection.Clear ();
				break;
			}
		}
	}
}
