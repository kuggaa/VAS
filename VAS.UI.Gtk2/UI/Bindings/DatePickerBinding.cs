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
using VAS.Core.MVVMC;
using VAS.UI.Component;

namespace VAS.UI.Bindings
{
	public class DatePickerBinding<TSourceProperty> : PropertyBinding<TSourceProperty, DateTime>
	{
		DatePicker datePicker;

		public DatePickerBinding (DatePicker widget, Expression<Func<IViewModel, TSourceProperty>> sourcePropertyExpression, TypeConverter converter = null) : base (sourcePropertyExpression, converter)
		{
			datePicker = widget;
		}

		protected override void BindView ()
		{
			datePicker.ValueChanged += HandleEntryChanged;
		}

		protected override void UnbindView ()
		{
			datePicker.ValueChanged -= HandleEntryChanged;
		}

		protected override void WriteViewValue (DateTime val)
		{
			datePicker.Date = val;
		}

		void HandleEntryChanged (object sender, EventArgs e)
		{
			WritePropertyValue (datePicker.Date);
		}
	}
}
