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
using System.Collections.ObjectModel;
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
		bool editing;
		RangeObservableCollection<TModel> model;
		Dictionary<TModel, TViewModel> modelToViewModel;

		public CollectionViewModel ()
		{
			Model = new RangeObservableCollection<TModel> ();
			ViewModels = new RangeObservableCollection<TViewModel> ();
		}

		/// <summary>
		/// Gets or sets the model used in this ViewModel.
		/// </summary>
		/// <value>The model.</value>
		public RangeObservableCollection<TModel> Model {
			set {
				if (ViewModels != null) {
					ViewModels.CollectionChanged -= HandleViewModelsCollectionChanged;
				}
				if (Model != null) {
					Model.CollectionChanged -= HandleModelsCollectionChanged;
				}
				ViewModels = new RangeObservableCollection<TViewModel> ();
				modelToViewModel = new Dictionary<TModel, TViewModel> ();
				model = value;
				AddViewModels (model);
				ViewModels.CollectionChanged += HandleViewModelsCollectionChanged;
				model.CollectionChanged += HandleModelsCollectionChanged;
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

		void AddViewModels (IEnumerable<TModel> models)
		{
			var viewModels = new List<TViewModel> ();
			foreach (TModel model in models) {
				var viewModel = new TViewModel {
					Model = model
				};
				viewModels.Add (viewModel);
				modelToViewModel [model] = viewModel;
			}
			ViewModels.AddRange (viewModels);
		}

		void HandleViewModelsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (editing) {
				return;
			}
			editing = true;
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				model.AddRange (e.NewItems.OfType<TViewModel> ().Select ((arg) => arg.Model));
				break;
			case NotifyCollectionChangedAction.Remove:
				model.RemoveRange (e.OldItems.OfType<TViewModel> ().Select ((arg) => arg.Model));
				break;
			case NotifyCollectionChangedAction.Reset:
				model.Clear ();
				break;
			}
			editing = false;
		}

		void HandleModelsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (editing) {
				return;
			}
			editing = true;
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				AddViewModels (e.NewItems.OfType<TModel> ());
				break;
			case NotifyCollectionChangedAction.Remove:
				ViewModels.RemoveRange (e.OldItems.OfType<TModel> ().Select ((arg) => modelToViewModel [arg]));
				foreach (TModel model in e.OldItems) {
					modelToViewModel.Remove (model);
				}
				break;
			case NotifyCollectionChangedAction.Reset:
				foreach (var vm in ViewModels) {
					modelToViewModel.Remove (vm.Model);
				}
				ViewModels.Clear ();
				break;
			}
			editing = false;
		}
	}
}

