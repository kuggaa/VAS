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
using System.ComponentModel;
using System.Linq.Expressions;
using VAS.Core.Interfaces.MVVMC;

namespace VAS.Core.MVVMC
{
	public static class BindingsExtensions
	{
		/// <summary>
		/// Binds the specified object dest's property expressed on targetExpression to a ViewModel's property expressed on sourceExpression
		/// </summary>
		/// <returns>The bind.</returns>
		/// <param name="dest">Destination.</param>
		/// <param name="sourceExpression">Source expression.</param>
		/// <param name="targetExpression">Target expression.</param>
		public static OneWayPropertyBinding<TSourceProperty, TTargetProperty> Bind<TTarget, TSourceProperty, TTargetProperty> (this TTarget dest,
																				 Expression<Func<TTarget, TTargetProperty>> targetExpression,
																				 Expression<Func<IViewModel, TSourceProperty>> sourceExpression,
																				 TypeConverter typeConverter = null,
																				 TTargetProperty defaultValue = default (TTargetProperty),
																				 Func<TTargetProperty, TTargetProperty> formatterCallback = null)
		{
			return new OneWayPropertyBinding<TSourceProperty, TTarget, TTargetProperty> (dest, sourceExpression, targetExpression, typeConverter, defaultValue, formatterCallback);
		}
	}
}
