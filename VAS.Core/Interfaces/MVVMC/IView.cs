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

namespace VAS.Core.Interfaces.MVVMC
{
	/// <summary>
	/// Interface the for the View components in the MVVMC framework.
	/// </summary>
	public interface IView
	{
		/// <summary>
		/// Sets the ViewModel associated to this View.
		/// </summary>
		/// <param name="viewModel"> The ViewModel.</param>
		void SetViewModel (object ViewModel);
	}

	public interface IView<T>: IView where T:IViewModel
	{
		/// <summary>
		/// Gets or sets the ViewModel binded to the IView.
		/// </summary>
		/// <value>The ViewModel.</value>
		T ViewModel { get; set; }
	}
}

