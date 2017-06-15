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
	/// Property binding for text entries.
	/// </summary>
	public class ComboBoxBinding : PropertyBinding<string>
	{
		ComboBox comboBox;

		public ComboBoxBinding (ComboBox comboBox, Expression<Func<IViewModel, string>> propertyExpression) : base (propertyExpression)
		{
			this.comboBox = comboBox;
		}

		public ComboBoxBinding (ComboBox comboBox, Expression<Func<IViewModel, object>> propertyExpression, TypeConverter converter) : base (propertyExpression, converter)
		{
			this.comboBox = comboBox;
		}

		protected override void BindView ()
		{
			comboBox.Changed += HandleSelectionChanged;
		}

		protected override void UnbindView ()
		{
			comboBox.Changed -= HandleSelectionChanged;
		}

		protected override void WriteViewValue (string val)
		{
			TreeIter iter;
			comboBox.Model.GetIterFirst (out iter);
			do {
				GLib.Value thisRow = new GLib.Value ();
				comboBox.Model.GetValue (iter, 0, ref thisRow);
				if ((thisRow.Val as string).Equals (val)) {
					comboBox.SetActiveIter (iter);
					break;
				}
			} while (comboBox.Model.IterNext (ref iter));
		}

		void HandleSelectionChanged (object sender, EventArgs e)
		{
			WritePropertyValue (comboBox.ActiveText);
		}
	}
}