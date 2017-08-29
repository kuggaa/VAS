//
//  Copyright (C) 2017 ${CopyrightHolder}
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
using VAS.UI.UI.Component;

namespace VAS.UI.UI.Bindings
{
	public class DatePickerBinding : PropertyBinding<DateTime>
	{
		DatePicker datePicker;

		public DatePickerBinding (DatePicker widget, Expression<Func<IViewModel, DateTime>> propertyExpression) : base (propertyExpression)
		{
			datePicker = widget;
		}
		public DatePickerBinding (DatePicker widget, Expression<Func<IViewModel, object>> propertyExpression, TypeConverter converter) : base (propertyExpression, converter)
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
