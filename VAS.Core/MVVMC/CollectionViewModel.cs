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
using VAS.Core.Interfaces.MVVMC;

namespace VAS.Core.MVVMC
{
	/// <summary>
	/// Base class for ViewModel representing a collection of ViewModel, like for example a list
	/// of <see cref="VAS.Core.Store.Templates.Dashboard"/>.
	/// The base class keeps in sync the ViewModel and the Model collection and provides support
	/// for selecting items within the collection.
	/// </summary>
	public class CollectionViewModel<T, W>:BindableBase, IViewModel<ObservableCollection<T>> where W: IViewModel<T>, new()
	{
		bool editing;
		ObservableCollection<T> model;
		Dictionary<T, W> modelToViewModel;

		public CollectionViewModel ()
		{
			Selection = new ObservableCollection<W> ();
			Selection.CollectionChanged += HandleSelectionChanged;
		}

		/// <summary>
		/// Gets or sets the model used in this ViewModel.
		/// </summary>
		/// <value>The model.</value>
		public ObservableCollection<T> Model {
			set {
				if (ViewModels != null) {
					ViewModels.CollectionChanged -= HandleViewModelsCollectionChanged;
				}
				if (Model != null) {
					Model.CollectionChanged -= HandleModelsCollectionChanged;
				}
				ViewModels = new ObservableCollection<W> ();
				modelToViewModel = new Dictionary<T, W> ();
				model = value;
				foreach (T element in model) {
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
		/// Gets the collection of child ViewModel
		/// </summary>
		/// <value>The ViewModels collection.</value>
		public ObservableCollection<W> ViewModels {
			private set;
			get;
		}

		/// <summary>
		/// Gets the current selection in the collection.
		/// </summary>
		/// <value>The selection.</value>
		public ObservableCollection<W> Selection {
			get;
			private set;
		}

		/// <summary>
		/// Selects the specified item from the list.
		/// </summary>
		/// <param name="item">The item to select.</param>
		public void Select (W viewModel)
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
		public void Select (T item)
		{
			if (item == null) {
				return;
			}
			Select (ViewModels.First (vm => vm.Model.Equals (item)));
		}

		void AddViewModel (T model)
		{
			var viewModel = new W {
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
				foreach (W viewModel in e.NewItems)
					model.Add (viewModel.Model);
				break;
			case NotifyCollectionChangedAction.Remove:
				foreach (W viewModel in e.OldItems)
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
				foreach (T model in e.NewItems) {
					AddViewModel (model); 
				}
				break;
			case NotifyCollectionChangedAction.Remove:
				foreach (T model in e.OldItems) {
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

