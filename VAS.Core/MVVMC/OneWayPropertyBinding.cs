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
using System.ComponentModel;
using System.Linq.Expressions;
using VAS.Core.Interfaces.MVVMC;

namespace VAS.Core.MVVMC
{
	/// <summary>
	/// This class provides one way property binding.
	/// <see cref="IViewModel" /> property is passed to write action callback performed on the class that setted the binding.
	/// </summary>
	public class OneWayPropertyBinding<TSourceProperty> : PropertyBinding<TSourceProperty>
	{
		protected Action<TSourceProperty> writeAction;

		public OneWayPropertyBinding (Expression<Func<IViewModel, TSourceProperty>> propertyExpression, Action<TSourceProperty> writeAction) : base (propertyExpression)
		{
			this.writeAction = writeAction;
		}

		public OneWayPropertyBinding (object dest, Expression<Func<object, TSourceProperty>> targetExpression,
									  Expression<Func<IViewModel, TSourceProperty>> sourceExpression) : base (sourceExpression)
		{
			var setter = CreateSetter (targetExpression, out string memberName);
			Action<TSourceProperty> setterAction = (TSourceProperty t) => setter.Invoke (dest, t);
			this.writeAction = setterAction;
		}

		protected override void BindView ()
		{
		}

		protected override void UnbindView ()
		{
		}

		protected override void WriteViewValue (TSourceProperty val)
		{
			this.writeAction (val);
		}
	}

	public class OneWayPropertyBinding<TSourceProperty, TTarget> : OneWayPropertyBinding<TSourceProperty>
	{
		public OneWayPropertyBinding (TTarget dest, Expression<Func<TTarget, TSourceProperty>> targetExpression,
									  Expression<Func<IViewModel, TSourceProperty>> sourceExpression) : base (sourceExpression, null)
		{
			var setter = CreateSetter (targetExpression, out string memberName);
			Action<TSourceProperty> setterAction = (TSourceProperty t) => setter.Invoke (dest, t);
			this.writeAction = setterAction;
		}
	}
}
