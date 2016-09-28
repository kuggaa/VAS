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
using System.ComponentModel;
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
		where TViewModel: IViewModel<TModel>, new()
	{
		bool editing;
		ObservableCollection<TModel> model;
		Dictionary<TModel, TViewModel> modelToViewModel;

		public CollectionViewModel ()
		{
			Model = new ObservableCollection<TModel> ();
			ViewModels = new ObservableCollection<TViewModel> ();
			Selection = new RangeObservableCollection<TViewModel> ();
			Selection.CollectionChanged += HandleSelectionChanged;
		}

		/// <summary>
		/// Gets or sets the model used in this ViewModel.
		/// </summary>
		/// <value>The model.</value>
		public ObservableCollection<TModel> Model {
			set {
				if (ViewModels != null) {
					ViewModels.CollectionChanged -= HandleViewModelsCollectionChanged;
				}
				if (Model != null) {
					Model.CollectionChanged -= HandleModelsCollectionChanged;
				}
				ViewModels = new ObservableCollection<TViewModel> ();
				modelToViewModel = new Dictionary<TModel, TViewModel> ();
				model = value;
				foreach (TModel element in model) {
					AddViewModel (element);
				}
				ViewModels.CollectionChanged += HandleViewModelsCollectionChanged;
				model.CollectionChanged += HandleModelsCollectionChanged;
			}
			get {
				return model;
			}
		}

		/// <summary>
		/// Gets the current selection in the collection.
		/// </summary>
		/// <value>The selection.</value>
		public RangeObservableCollection<TViewModel> Selection {
			get;
			private set;
		}

		/// <summary>
		/// Selects the specified item from the list.
		/// </summary>
		/// <param name="item">The item to select.</param>
		public void Select (TViewModel viewModel)
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

		void AddViewModel (TModel model)
		{
			var viewModel = new TViewModel {
				Model = model
			};
			ViewModels.Add (viewModel);
			modelToViewModel [model] = viewModel;
		}

		void HandleViewModelsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (editing) {
				return;
			}
			editing = true;
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				foreach (TViewModel viewModel in e.NewItems)
					model.Add (viewModel.Model);
				break;
			case NotifyCollectionChangedAction.Remove:
				foreach (TViewModel viewModel in e.OldItems)
					model.Remove (viewModel.Model);
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
				foreach (TModel model in e.NewItems) {
					AddViewModel (model); 
				}
				break;
			case NotifyCollectionChangedAction.Remove:
				foreach (TModel model in e.OldItems) {
					ViewModels.Remove (modelToViewModel [model]);
					modelToViewModel.Remove (model);
				}
				break;
			case NotifyCollectionChangedAction.Reset:
				ViewModels.Clear ();
				break;
			}
			editing = false;
		}

		void HandleSelectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			RaisePropertyChanged ("Selection");
		}
	}
}

