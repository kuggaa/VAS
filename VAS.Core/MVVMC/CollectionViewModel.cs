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
using System.Collections.Specialized;
using System.Linq;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;

namespace VAS.Core.MVVMC
{
	/// <summary>
	/// Base class for ViewModel representing a collection of ViewModel, like for example a list
	/// of <see cref="VAS.Core.Store.Templates.Dashboard"/>.
	/// The base class keeps in sync the ViewModel and the Model collection and provides support
	/// for selecting items within the collection.
	/// </summary>
	public class CollectionViewModel<TModel, TViewModel> : NestedViewModel<TViewModel>
		where TViewModel : IViewModel<TModel>, new()
	{
		protected bool editing;
		protected Dictionary<TModel, TViewModel> modelToViewModel;
		protected RangeObservableCollection<TModel> model;

		public CollectionViewModel ()
		{
			Model = new RangeObservableCollection<TModel> ();
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			foreach (var viewModel in ViewModels) {
				viewModel.Dispose ();
			}
		}

		/// <summary>
		/// Gets or sets the model used in this ViewModel.
		/// </summary>
		/// <value>The model.</value>
		public RangeObservableCollection<TModel> Model {
			set {
				SetModel (value);
			}
			get {
				return model;
			}
		}

		/// <summary>
		/// Selects the specified item from the list.
		/// </summary>
		/// <param name="item">The item to select.</param>
		public void Select (TModel item)
		{
			if (item == null) {
				return;
			}
			Select (ViewModels.First (vm => vm.Model.Equals (item)));
		}

		protected virtual TViewModel CreateInstance (TModel model)
		{
			return new TViewModel { Model = model };
		}

		protected virtual void SetModel (RangeObservableCollection<TModel> model)
		{
			if (ViewModels != null) {
				ViewModels.CollectionChanged -= HandleViewModelsCollectionChanged;
			}
			if (this.model != null) {
				this.model.CollectionChanged -= HandleModelsCollectionChanged;
			}
			ViewModels.Clear ();
			modelToViewModel = new Dictionary<TModel, TViewModel> ();
			this.model = model;
			AddViewModels (0, this.model);
			if (ViewModels != null) {
				ViewModels.CollectionChanged += HandleViewModelsCollectionChanged;
			}
			if (this.model != null) {
				this.model.CollectionChanged += HandleModelsCollectionChanged;
			}
		}

		protected virtual void AddViewModels (int index, IEnumerable<TModel> models)
		{
			if (models == null) {
				return;
			}

			var viewModels = new List<TViewModel> ();
			foreach (TModel tModel in models) {
				var viewModel = CreateInstance (tModel);
				viewModels.Add (viewModel);
				modelToViewModel [tModel] = viewModel;
			}
			ViewModels.InsertRange (index, viewModels);
		}

		protected virtual void HandleViewModelsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (editing) {
				return;
			}
			editing = true;
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				model.InsertRange (e.NewStartingIndex, e.NewItems.OfType<TViewModel> ().Select ((arg) => arg.Model));
				break;
			case NotifyCollectionChangedAction.Remove:
				model.RemoveRange (e.OldItems.OfType<TViewModel> ().Select ((arg) => arg.Model));
				break;
			case NotifyCollectionChangedAction.Reset:
				model.Replace (ViewModels.Select (vm => vm.Model));
				break;
			}
			editing = false;
		}

		protected virtual void HandleModelsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (editing) {
				return;
			}
			editing = true;
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				AddViewModels (e.NewStartingIndex, e.NewItems.OfType<TModel> ());
				break;
			case NotifyCollectionChangedAction.Remove:
				ViewModels.RemoveRange (e.OldItems.OfType<TModel> ().Select ((arg) => modelToViewModel [arg]));
				foreach (TModel model in e.OldItems) {
					modelToViewModel.Remove (model);
				}
				break;
			case NotifyCollectionChangedAction.Reset:
				ViewModels.Clear ();
				AddViewModels (0, Model);
				break;
			}
			editing = false;
		}
	}
}
