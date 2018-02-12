//
//  Copyright (C) 2018 Fluendo S.A.
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
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Core.Interfaces
{
	/// <summary>
	/// An interface for creating correctly typed ViewModel instances based on it's model, this is useful to work with
	/// base classes and create child viewmodels without knowing the reference on it.
	/// </summary>
	public interface IViewModelFactoryService
	{
		/// <summary>
		/// Adds a new TypeMapping
		/// </summary>
		/// <param name="modelType">the model type</param>
		/// <param name="viewModelType">the viewmodel type</param>
		void AddTypeMapping (Type modelType, Type viewModelType);

		/// <summary>
		/// Creates a ViewModel based on a Model
		/// </summary>
		/// <returns>The derived instance of the ViewModel</returns>
		/// <param name="model">the model</param>
		TViewModel CreateViewModel<TViewModel, TModel> (TModel model)
			where TViewModel : IViewModel<TModel>, new();
	}
}
