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
using System.Reflection;
using VAS.Core.Interfaces.MVVMC;

namespace VAS.Core.MVVMC
{
	public abstract class Binding
	{
		protected readonly string propertyName;
		protected PropertyInfo property;
		IViewModel viewModel;

		public Binding (string propertyName)
		{
		}

		public IViewModel ViewModel {
			get {
				return viewModel;
			}
			set {
				property = viewModel.GetType ().GetProperty (propertyName);

				if (property == null) {
					throw new ArgumentException ("Property " + propertyName + " not found in view model " + viewModel);
				}

				if (property.PropertyType != typeof (string)) {
					throw new ArgumentException ("Binded property must be of type String");
				}
				viewModel = value;
			}
		}

		abstract protected void BindView ();

		abstract protected void UnbindView ();

		abstract protected void HandlePropertyChanged (object s, PropertyChangedEventArgs e);
	}

	public abstract class Binding<T> : Binding
	{
		public Binding (string propertyName) : base (propertyName)
		{
		}

		abstract protected void WriteViewValue (T val);

		protected void WritePropertyValue (T val)
		{
			property.SetValue (ViewModel, val, null);
		}

		T ReadPropertyValue ()
		{
			return (T)property.GetValue (ViewModel);
		}

		override protected void HandlePropertyChanged (object s, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == propertyName) {
				WriteViewValue (ReadPropertyValue ());
			}
		}

		static public void HookInpc (MemberExpression expression, PropertyChangedEventHandler eventHandler)
		{
			if (expression == null)
				return;

			var constantExpression = expression.Expression as ConstantExpression;
			if (constantExpression != null) {
				var _inpc = constantExpression.Value as INotifyPropertyChanged;
				if (_inpc != null) {
					_inpc.PropertyChanged += eventHandler;
				} else {
				}
			}
		}

	}
}
