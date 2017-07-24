//
//  Copyright (C) 2017 Fluendo S.A.
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.License;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Core.MVVMC
{
	/// <summary>
	/// CollectionViewModel with a limitation. It contains the number of elements specified by <see cref="CountLicenseLimitation"/>.Maximum
	/// </summary>
	public class LimitedCollectionViewModel<TModel, TViewModel> : CollectionViewModel<TModel, TViewModel>
		where TViewModel : IViewModel<TModel>, new()
		where TModel : StorableBase
	{
		LicenseLimitationVM limitation;
		bool notifying;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:VAS.Core.MVVMC.LimitedCollectionViewModel`3"/> class.
		/// </summary>
		/// <param name="sortByCreationDateDesc">If set to <c>true</c> sort by creation date desc.</param>
		public LimitedCollectionViewModel (bool sortByCreationDateDesc = true)
		{
			this.SortByCreationDateDesc = sortByCreationDateDesc;
			LimitedViewModels = new RangeObservableCollection<TViewModel> ();
			LimitedViewModels.CollectionChanged += HandleViewModelsCollectionChanged;
			base.ViewModels.CollectionChanged += HandleBaseViewModelModelsCollectionChanged;
		}

		protected override void DisposeManagedResources ()
		{
			LimitedViewModels.IgnoreEvents = true;
			LimitedViewModels.CollectionChanged -= HandleViewModelsCollectionChanged;
			base.ViewModels.IgnoreEvents = true;
			base.ViewModels.CollectionChanged -= HandleBaseViewModelModelsCollectionChanged;
			base.DisposeManagedResources ();
			Limitation = null;
			if (LimitedViewModels != null && LimitedViewModels.Any ()) {
				foreach (var viewModel in LimitedViewModels) {
					viewModel.Dispose ();
				}
				LimitedViewModels.Clear ();
			}
			LimitedViewModels = null;
		}

		/// <summary>
		/// Gets or sets the limitation.
		/// </summary>
		/// <value>The limitation.</value>
		public virtual LicenseLimitationVM Limitation {
			get {
				return limitation;
			}
			set {
				if (limitation != null) {
					limitation.PropertyChanged -= LimitationPropertyChanged;
				}
				limitation = value;
				if (limitation != null) {
					limitation.PropertyChanged += LimitationPropertyChanged;
					FillLimitedViewModels (base.ViewModels);
				}
			}
		}

		/// <summary>
		/// Gets or sets the view models.
		/// This property is shadowing CollectionViewModel.ViewModels to "disallow" access
		/// to the real ViewModels collection to all children classes.
		/// </summary>
		/// <value>The view models.</value>
		public new RangeObservableCollection<TViewModel> ViewModels {
			get {
				return LimitedViewModels;
			}
		}

		/// <summary>
		/// Gets the limited collection of viewmodels.
		/// </summary>
		/// <value>The limited view models.</value>
		public RangeObservableCollection<TViewModel> LimitedViewModels {
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.MVVMC.LimitedCollectionViewModel`2"/> sort by
		/// creation date in descending order.
		/// </summary>
		/// <value><c>true</c> if sort by creation date desc; otherwise, <c>false</c>.</value>
		public bool SortByCreationDateDesc {
			get;
			set;
		}

		/// <summary>
		/// Gets the enumerator of the LimitedViewModels. This method is automatically called in a foreach.
		/// </summary>
		/// <returns>The enumerator.</returns>
		public override IEnumerator<TViewModel> GetEnumerator ()
		{
			return LimitedViewModels.GetEnumerator ();
		}

		public override INotifyCollectionChanged GetNotifyCollection ()
		{
			return LimitedViewModels;
		}

		protected override void SetModel (RangeObservableCollection<TModel> model)
		{
			base.SetModel (model);
			if (model != null) {
				FillLimitedViewModels (base.ViewModels);
			}
		}

		protected new void HandleViewModelsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (notifying) {
				return;
			}

			notifying = true;
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				base.ViewModels.InsertRange (e.NewStartingIndex, e.NewItems.OfType<TViewModel> ());
				break;

			case NotifyCollectionChangedAction.Remove:
				base.ViewModels.RemoveRange (e.OldItems.OfType<TViewModel> ());
				break;

			case NotifyCollectionChangedAction.Replace:
			case NotifyCollectionChangedAction.Reset:
				base.ViewModels.Clear ();
				base.ViewModels.InsertRange (0, this.ViewModels);
				break;
			}

			FillLimitedViewModels (base.ViewModels);
			notifying = false;
		}

		protected void HandleBaseViewModelModelsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (notifying) {
				return;
			}
			notifying = true;
			FillLimitedViewModels (base.ViewModels);
			notifying = false;
		}

		void FillLimitedViewModels (IEnumerable<TViewModel> viewmodels)
		{
			viewmodels = viewmodels
				.Sort ((vm) => vm.Model.CreationDate, SortByCreationDateDesc);

			if (Limitation?.Enabled ?? false) {
				viewmodels = viewmodels.Take (Limitation.Maximum);
			}

			LimitedViewModels?.Replace (viewmodels);
		}

		void LimitationPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (Limitation.Enabled) || e.PropertyName == nameof (Limitation.Maximum)) {
				FillLimitedViewModels (base.ViewModels);
			}
		}
	}
}
