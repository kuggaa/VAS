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

	/// <summary>
	/// Base class for bindings UI elements with properties, In case the View and the ViewModel poperties have
	/// a different type a converter can be used to convert bwtween them, for example to convert int <--> string
	/// </summary>
	public abstract class PropertyBinding<T> : Binding
	{
		protected readonly Expression<Func<IViewModel, T>> propertyExpression;
		Action<IViewModel, T> propertySet;
		Func<IViewModel, T> propertyGet;
		string propertyName;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:VAS.Core.MVVMC.PropertyBinding`1"/> class to bind
		/// a ViewModel Property.
		/// </summary>
		/// <param name="propertyExpression">Property expression.</param>
		public PropertyBinding (Expression<Func<IViewModel, T>> propertyExpression)
		{
			// The expression has the following form
			// vm => Convert(vm).PropertyFoo;
			var member = (MemberExpression)propertyExpression.Body;
			propertyName = member.Member.Name;
			propertySet = CreateSetter (propertyExpression, member);

			// Create getter
			propertyGet = propertyExpression.Compile ();
		}

		protected Action<IViewModel, T> CreateSetter (Expression<Func<IViewModel, T>> propertyExpression, MemberExpression member)
		{
			Action<IViewModel, T> setter;
			if (((PropertyInfo)member.Member).CanWrite) {
				// Create the setter
				var param = Expression.Parameter (typeof (T), "value");
				// vm => vm.Property  ---> ((IViewModel) vm, (T)t) => vm.Property = t; 
				var set = Expression.Lambda<Action<IViewModel, T>> (
					Expression.Assign (member, param), propertyExpression.Parameters [0], param);
				setter = set.Compile ();
			} else {
				setter = (vm, o) => { };
			}
			return setter;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:VAS.Core.MVVMC.PropertyBinding`1"/> class to bind
		/// a ViewModel Property using a type converter, for example when binding a UI Label widget expecting a
		/// <see cref="string"/> with a Property using an <see cref="int"/>.
		/// </summary>
		/// <param name="propertyExpression">Property expression.</param>
		/// <param name="converter">Converter.</param>
		public PropertyBinding (Expression<Func<IViewModel, object>> propertyExpression, TypeConverter converter)
		{
			// The expression contains an conversion to object
			// vm => Convert(Convert(vm).PropertyFoo);
			UnaryExpression operand = (UnaryExpression)(propertyExpression.Body);
			var member = (MemberExpression)(operand.Operand);
			propertyName = member.Member.Name;

			if (((PropertyInfo)member.Member).CanWrite) {
				// Create the setter function that converts between functions using the converter.
				Type propertyType = ((PropertyInfo)member.Member).PropertyType;

				if (!converter.CanConvertFrom (typeof (T))) {
					throw new InvalidCastException (
						$"The specified converter cannot convert from {typeof (T)} to {propertyType} for the destination property {propertyName}");
				}
				if (!converter.CanConvertTo (propertyType)) {
					throw new InvalidCastException (
						$"The specified converter cannot convert to {propertyType} for the destination property {propertyName}");
				}

				Expression<Func<T, object>> converterExp = (T t) => (converter.ConvertFrom (t));
				var param = Expression.Parameter (typeof (T), "value");
				// (vm, t) => vm.Property = (PropertyType)(converter.ConvertFrom (t))
				var convert = Expression.Lambda<Action<IViewModel, T>> (
					// vm.Property = (PropertyType)(converter.ConvertFrom (t))
					Expression.Assign (member,
									   // (PropertyType)(converter.ConvertFrom (t))
									   Expression.Convert (
										   // converter.ConvertFrom (t)
										   Expression.Invoke (converterExp, param), propertyType)),
					propertyExpression.Parameters [0], param);

				propertySet = convert.Compile ();
			} else {
				propertySet = (vm, o) => { };
			}

			// Create getter function that converts between values
			var getter = propertyExpression.Compile ();
			propertyGet = (IViewModel arg) => (T)(converter.ConvertTo (getter (arg), typeof (T)));
		}

		protected override void BindViewModel ()
		{
			if (ViewModel != null) {
				ViewModel.PropertyChanged += HandlePropertyChanged;
			}
		}

		protected override void UnbindViewModel ()
		{
			if (ViewModel != null) {
				ViewModel.PropertyChanged -= HandlePropertyChanged;
			}
		}

		/// <summary>
		/// Writes the new value to the property when the View changes.
		/// </summary>
		/// <param name="val">Value.</param>
		protected void WritePropertyValue (T val)
		{
			if (ViewModel != null) {
				propertySet (ViewModel, val);
			}
		}

		/// <summary>
		/// Subclasses must implement this method to write the new value to the View, when the ViewModel changes.
		/// </summary>
		/// <param name="val">Value.</param>
		protected abstract void WriteViewValue (T val);

		void HandlePropertyChanged (object s, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == propertyName || e.PropertyName == null) {
				WriteViewValue (propertyGet (ViewModel));
			}
		}
	}
}
