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
using Gtk;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;

namespace VAS.UI.Helpers.Bindings
{
	/// <summary>
	/// Property binding for the spin value changes
	/// </summary>
	public class SpinBinding<TSourceProperty> : PropertyBinding<TSourceProperty, double>
	{
		SpinButton spinButton;

		public SpinBinding (SpinButton spinButton, Expression<Func<IViewModel, TSourceProperty>> propertyExpression,
							TypeConverter converter = null) : base (propertyExpression, converter)
		{
			this.spinButton = spinButton;
		}

		protected override void BindView ()
		{
			spinButton.ValueChanged += HandleValueChanged;
		}

		protected override void UnbindView ()
		{
			spinButton.ValueChanged -= HandleValueChanged;
		}

		protected override void WriteViewValue (double val)
		{
			spinButton.Value = val;
		}

		void HandleValueChanged (object sender, EventArgs args)
		{
			WritePropertyValue (spinButton.Value);
		}
	}
}
