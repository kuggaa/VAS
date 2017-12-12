//
//  Copyright (C) 2017 
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
	/// Collection that updates the internal dictionary recreating it when a property of one of the 
	/// view mdoels changes
	/// </summary>
	/// <remarks>Use it only when the key uses mutable properties and this cannot be changed</remarks>
	public class KeyUpdaterCollectionViewModel<TModel, TViewModel> : CollectionViewModel<TModel, TViewModel>
		where TModel : class
		where TViewModel : IViewModel<TModel>, new()
	{
		protected override void DisposeUnmanagedResources ()
		{
			foreach (var vm in modelToViewModel.Values) {
				vm.PropertyChanged -= HandlePropertyChanged;
			}

			base.DisposeUnmanagedResources ();
		}

		protected override TViewModel CreateInstance (TModel model)
		{
			var vm = base.CreateInstance (model);
			vm.PropertyChanged += HandlePropertyChanged;
			return vm;
		}

		void HandlePropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			var vm = (TViewModel)sender;
			if (!modelToViewModel.ContainsKey (vm.Model))
			{
				this.IgnoreEvents = true;

				// update the dictionary with the new mutable object as the key
				var key = modelToViewModel.Keys.Where (x => x == vm.Model).FirstOrDefault ();
				modelToViewModel.Remove (key);
				modelToViewModel.Add (vm.Model, vm);

				this.IgnoreEvents = false;
			}
		}

		protected override void HandleViewModelsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
				case NotifyCollectionChangedAction.Add:
					foreach (var vm in e.NewItems.OfType<TViewModel> ()) {
						// check if the view model subscription was already managed by the create instance to avoid
						// a double subscription
						if (!modelToViewModel.ContainsKey (vm.Model)) {
							vm.PropertyChanged += HandlePropertyChanged;
						}
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach (var vm in e.OldItems.OfType<TViewModel> ()) {
						vm.PropertyChanged -= HandlePropertyChanged;
					}
					break;
				case NotifyCollectionChangedAction.Replace:
					foreach (var vm in e.OldItems.OfType<TViewModel> ()) {
						vm.PropertyChanged -= HandlePropertyChanged;

					}
					foreach (var vm in e.NewItems.OfType<TViewModel> ()) {
						vm.PropertyChanged += HandlePropertyChanged;

					}
					break;
				case NotifyCollectionChangedAction.Reset:
					foreach (var vm in modelToViewModel.Values)
					{
						vm.PropertyChanged -= HandlePropertyChanged;
					}
					foreach (var vm in ViewModels) {
						vm.PropertyChanged += HandlePropertyChanged;
					}
					break;
			}

			base.HandleViewModelsCollectionChanged (sender, e);
		}
	}
}
