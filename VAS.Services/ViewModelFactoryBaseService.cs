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
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;

namespace VAS.Services
{
	/// <summary>
	/// View model factory service, creates correctly typed ViewModel instances based on it's model.
	/// This is useful to work with base classes and create child viewmodels without knowing the derived type
	/// </summary>
	public class ViewModelFactoryBaseService : IViewModelFactoryService
	{
		public ViewModelFactoryBaseService ()
		{
			TypeMappings = new Dictionary<Type, Type> ();
			TypeMappings.Add (typeof (PlaylistPlayElement), typeof (PlaylistPlayElementVM));
			TypeMappings.Add (typeof (PlaylistVideo), typeof (PlaylistVideoVM));
			TypeMappings.Add (typeof (PlaylistImage), typeof (PlaylistImageVM));
			TypeMappings.Add (typeof (PlaylistDrawing), typeof (PlaylistDrawingVM));
			TypeMappings.Add (typeof (AnalysisEventButton), typeof (AnalysisEventButtonVM));
			TypeMappings.Add (typeof (TagButton), typeof (TagButtonVM));
			TypeMappings.Add (typeof (TimerButton), typeof (TimerButtonVM));
		}

		/// <summary>
		/// Gets the TypeMappings Dictionary, where Key is the Model Type and Value is the ViewModel Type
		/// </summary>
		/// <value>The Type Mappings</value>
		protected Dictionary<Type, Type> TypeMappings {
			get;
		}

		/// <summary>
		/// Adds a new TypeMapping
		/// </summary>
		/// <param name="modelType">Model type.</param>
		/// <param name="viewModelType">View model type.</param>
		public void AddTypeMapping (Type modelType, Type viewModelType)
		{
			TypeMappings.Add (modelType, viewModelType);
		}

		/// <summary>
		/// Creates a ViewModel based on a Model
		/// </summary>
		/// <returns>The derived instance of the ViewModel</returns>
		/// <param name="model">the model</param>
		public TViewModel CreateViewModel<TViewModel, TModel> (TModel model)
			where TViewModel : IViewModel<TModel>, new()
		{
			Type viewModelType;
			Type modelType = model.GetType ();
			TViewModel viewModel = default (TViewModel);

			// If there's a typeMapping defined for the specific type
			if (TypeMappings.TryGetValue (modelType, out viewModelType)) {
				Log.Verbose ($"TypeMapping found {modelType} => {viewModelType}");
				viewModel = (TViewModel)Activator.CreateInstance (viewModelType);
			} else {
				// If there isn't, get the first mapping that matches a parent class
				foreach (var type in TypeMappings.Keys) {
					if (type.IsAssignableFrom (modelType)) {
						if (TypeMappings.TryGetValue (type, out viewModelType)) {
							Log.Verbose ($"TypeMapping found {modelType} => {viewModelType}");
							viewModel = (TViewModel)Activator.CreateInstance (viewModelType);
							break;
						}
					}
				}
			}

			if (viewModel == null) {
				Log.Verbose ($"TypeMapping not found for {modelType}. Using the base ViewModel {typeof (TViewModel).Name}");
				viewModel = new TViewModel ();
			}
			viewModel.Model = model;
			return viewModel;
		}
	}
}
