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
using System.Collections.ObjectModel;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;

namespace VAS.Core.MVVMC
{
	/// <summary>
	/// Base class for ViewModel's having a Model with child Model's, for example like the <see cref="PlaylistVM"/>
	/// using a <see cref="Playlist"/> model containing <see cref="IPlaylistElement"/> children.
	/// </summary>
	public abstract class NestedSubViewModel<TModel, TViewModel, TModelChild, TVMChild> : NestedViewModel<TVMChild>, IViewModel<TModel>
		where TModel : BindableBase
		where TViewModel : IViewModel<TModel>
		where TVMChild : IViewModel<TModelChild>, new()
	{
		TModel model;

		/// <summary>
		/// Gets or sets the model.
		/// </summary>
		/// <value>The model.</value>
		[PropertyChanged.DoNotCheckEquality]
		public TModel Model {
			get {
				return model;
			}

			set {
				model = value;
				SubViewModel.Model = ChildModels;
			}
		}

		/// <summary>
		/// Gets the list of child ViewModel's from the collection ViewModel.
		/// </summary>
		/// <value>The view models.</value>
		public override RangeObservableCollection<TVMChild> ViewModels {
			get {
				return SubViewModel?.ViewModels;
			}
		}

		/// <summary>
		/// Gets the list of child models.
		/// </summary>
		/// <value>The child models.</value>
		public abstract RangeObservableCollection<TModelChild> ChildModels {
			get;
		}

		/// <summary>
		/// Gets or sets the collection containing the children ViewModel.
		/// </summary>
		protected CollectionViewModel<TModelChild, TVMChild> SubViewModel {
			get;
			set;
		} = new CollectionViewModel<TModelChild, TVMChild> ();
	}
}
