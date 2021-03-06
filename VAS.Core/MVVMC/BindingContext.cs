﻿//
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
using VAS.Core.Interfaces.MVVMC;

namespace VAS.Core.MVVMC
{

	/// <summary>
	/// A binding context that must be used in a view to register bindings and update them when the ViewModel changes.
	/// </summary>
	public class BindingContext : DisposableBase
	{
		public BindingContext ()
		{
			Bindings = new List<Binding> ();
		}

		protected override void DisposeManagedResources ()
		{
			foreach (Binding binding in Bindings) {
				binding.Dispose ();
			}
			Bindings.Clear ();
			base.DisposeManagedResources ();
		}

		List<Binding> Bindings {
			get;
			set;
		}

		/// <summary>
		/// Adds a new binding to the context.
		/// </summary>
		/// <param name="binding">Binding.</param>
		public void Add (Binding binding)
		{
			Bindings.Add (binding);
		}

		/// <summary>
		/// Updates the view model in all bindings.
		/// </summary>
		/// <param name="viewModel">View model.</param>
		public void UpdateViewModel (IViewModel viewModel)
		{
			foreach (var binding in Bindings) {
				binding.ViewModel = viewModel;
			}
		}
	}
}
