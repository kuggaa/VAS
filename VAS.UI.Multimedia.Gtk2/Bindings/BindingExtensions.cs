//
//  Copyright (C) 2017 Fluendo S.A.
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
using System.Linq.Expressions;
using Gtk;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.UI;

namespace VAS.Bindings
{
	public static class BindingExtensions
	{
		/// <summary>
		/// Binds the specified <paramref name="sliderView"/> to the specified command and sets icon/tooltip from command to <paramref name="showButton"/>,
		/// additionally it binds scale to property expressed on <paramref name="propertyExpression"/> 
		/// and perfoms an UI callback <paramref name="updateViewAction"/> when property binded is changed.
		/// </summary>
		/// <returns>The bind.</returns>
		/// <param name="sliderView">Slider view.</param>
		/// <param name="showButton">Show button.</param>
		/// <param name="commandFunc">Command func.</param>
		/// <param name="propertyExpression">Property expression.</param>
		/// <param name="updateViewAction">Update view action.</param>
		public static Binding Bind (this SliderView sliderView, Button showButton, Func<IViewModel, Command<double>> commandFunc,
											  Expression<Func<IViewModel, double>> propertyExpression = null, Action<double> updateViewAction = null)
		{
			return new SliderViewCommandBinding (sliderView, showButton, commandFunc, propertyExpression, updateViewAction);
		}
	}
}
