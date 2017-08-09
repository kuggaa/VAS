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
using System.Reflection;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel.Statistics;

namespace VAS.Core.MVVMC
{
	/// <summary>
	/// Bind the setter of one property of an object (here called View) 
	/// to the getter of a different property of an object (here called ViewModel) 
	/// </summary>
	public class PropertyToPropertyBinding<TProperty> : PropertyBinding<TProperty>
	{
		IViewModel destination;
		Action<IViewModel, TProperty> setter;

		public PropertyToPropertyBinding (IViewModel dest, Expression<Func<IViewModel, TProperty>> setterExpression, Expression<Func<IViewModel, TProperty>> getterExpression) : base (getterExpression)
		{
			destination = dest;
			var member = (MemberExpression)setterExpression.Body;
			if (!((PropertyInfo)member.Member).CanWrite) {
				throw new InvalidOperationException ("Cannot bind to a non-writable property");
			}
			setter = CreateSetter (setterExpression, member);
		}

		protected override void BindView ()
		{
		}

		protected override void UnbindView ()
		{
		}

		protected override void WriteViewValue (TProperty val)
		{
			setter.Invoke (destination, val);
		}
	}
}
