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
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;

namespace VAS.Core.MVVMC
{
	/// <summary>
	/// Base class for bindings UI elements with properties, In case the View and the ViewModel properties have
	/// a different type a converter can be used to convert between them, for example to convert int <--> string
	/// </summary>
	public abstract class PropertyBinding<TSourceProperty, TTargetProperty> : Binding
	{
		protected readonly Expression<Func<IViewModel, TSourceProperty>> propertyExpression;
		protected Action<IViewModel, TTargetProperty> sourcePropertySet;
		protected Func<IViewModel, TTargetProperty> sourcePropertyGet;
		protected string sourcePropertyName;

		protected PropertyBinding ()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:VAS.Core.MVVMC.PropertyBinding`1"/> class to bind
		/// a ViewModel Property.
		/// </summary>
		/// <param name="sourcePropertyExpresison">Property expression.</param>
		public PropertyBinding (Expression<Func<IViewModel, TSourceProperty>> sourcePropertyExpresison, TypeConverter typeConverter)
		{
			// The expression has the following form
			// vm => Convert(vm).PropertyFoo;
			if (typeConverter == null) typeConverter = new DefaultTypeConverter<TSourceProperty, TTargetProperty> ();

			sourcePropertySet = CreateSetter<IViewModel, TSourceProperty, TTargetProperty> (sourcePropertyExpresison, out string propertyMemberName, typeConverter);
			sourcePropertyName = propertyMemberName;

			// Create getter
			sourcePropertyGet =
				(IViewModel arg) => (TTargetProperty)(typeConverter.ConvertTo (sourcePropertyExpresison.Compile () (arg), typeof (TTargetProperty)));
		}

		public PropertyBinding (Expression<Func<IViewModel, TSourceProperty>> propertyExpression)
		{
			// The expression has the following form
			// vm => Convert(vm).PropertyFoo;
			if (!typeof (TTargetProperty).IsAssignableFrom (typeof (TSourceProperty))) {
				throw new ArgumentException ($"The specified type {typeof (TSourceProperty)} is not equal to {typeof (TTargetProperty)}");
			}

			sourcePropertySet = CreateSetter<IViewModel, TSourceProperty, TTargetProperty> (propertyExpression, out string propertyMemberName);
			sourcePropertyName = propertyMemberName;

			// Create getter
			sourcePropertyGet = (IViewModel arg) => (TTargetProperty)(object)propertyExpression.Compile ().Invoke (arg);
		}

		protected Action<TSource, TFromProperty> CreateSetter<TSource, TToProperty, TFromProperty> (Expression<Func<TSource, TToProperty>> propertyExpression, out string propertyMemberName, TypeConverter typeConverter = null)
		{
			var member = (MemberExpression)propertyExpression.Body;
			propertyMemberName = member.Member.Name;
			Action<TSource, TFromProperty> setter;
			if (((PropertyInfo)member.Member).CanWrite) {

				// Create the setter
				var setParameter = Expression.Parameter (typeof (TFromProperty), "value");

				if (typeConverter != null) {
					Type sourcePropertyType = ((PropertyInfo)member.Member).PropertyType;

					Expression<Func<TFromProperty, object>> converterExp = null;
					Expression<Action<TSource, TFromProperty>> convert = null;
					if (typeConverter.CanConvertFrom (typeof (TFromProperty))) {
						converterExp = (TFromProperty t) => (typeConverter.ConvertFrom (t));
					} else if (typeConverter.CanConvertTo (sourcePropertyType)) {
						converterExp = (TFromProperty t) => (typeConverter.ConvertTo (t, sourcePropertyType));
					}

					if (converterExp == null) throw new InvalidCastException (
						$"The specified converter cannot convert from {typeof (TFromProperty)} to {sourcePropertyType} for the destination property {sourcePropertyName}" +
						 $" and The specified converter cannot convert to {sourcePropertyType} for the destination property {sourcePropertyName}"
					 );

					// converter.ConvertFrom (t)
					var converterFunction = Expression.Invoke (converterExp, setParameter);
					// (PropertyType)(converter.ConvertFrom (t))
					var expressionConvert = Expression.Convert (converterFunction, sourcePropertyType);
					// vm.Property = (PropertyType)(converter.ConvertFrom (t))
					var expressionAssign = Expression.Assign (member, expressionConvert);

					convert = Expression.Lambda<Action<TSource, TFromProperty>> (expressionAssign, propertyExpression.Parameters [0], setParameter);


					setter = convert.Compile ();
				} else {
					// vm => vm.Property  ---> ((IViewModel) vm, (T)t) => vm.Property = t; 
					var set = Expression.Lambda<Action<TSource, TFromProperty>> (
						Expression.Assign (member, setParameter), propertyExpression.Parameters [0], setParameter);
					setter = set.Compile ();
				}
			} else {
				setter = (vm, o) => { };
			}
			return setter;
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
		protected void WritePropertyValue (TTargetProperty val)
		{
			if (ViewModel != null) {
				sourcePropertySet (ViewModel, val);
			}
		}

		protected override void BindViewModel ()
		{
			if (ViewModel != null) {
				ViewModel.PropertyChanged += HandlePropertyChanged;
				HandlePropertyChanged (null, new PropertyChangedEventArgs (sourcePropertyName));
			}
		}

		/// <summary>
		/// Subclasses must implement this method to write the new value to the View, when the ViewModel changes.
		/// </summary>
		/// <param name="val">Value.</param>
		protected abstract void WriteViewValue (TTargetProperty val);

		void HandlePropertyChanged (object s, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == sourcePropertyName || e.PropertyName == null) {
				try {
					WriteViewValue (sourcePropertyGet (ViewModel));
				} catch (Exception ex) {
					Log.Exception (ex);
				}
			}
		}
	}

	/// <summary>
	/// Base class for bindings UI elements with properties of the same type
	/// </summary>
	public abstract class PropertyBinding<TSourceProperty> : PropertyBinding<TSourceProperty, TSourceProperty>
	{
		public PropertyBinding (Expression<Func<IViewModel, TSourceProperty>> propertyExpression)
		{
			sourcePropertySet = CreateSetter<IViewModel, TSourceProperty, TSourceProperty> (propertyExpression, out string propertyMemberName);
			sourcePropertyName = propertyMemberName;
			sourcePropertyGet = propertyExpression.Compile ();
		}
	}

	/// <summary>
	/// This class provides a default type converter for equal types.
	/// </summary>
	class DefaultTypeConverter<TSourceProperty, TTargetProperty> : TypeConverter
	{
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			return typeof (TTargetProperty) == typeof (TSourceProperty);
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			return typeof (TTargetProperty) == typeof (TSourceProperty);
		}

		public override object ConvertTo (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			return value;
		}

		public override object ConvertFrom (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			return value;
		}
	}
}
