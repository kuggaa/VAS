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
using System.Linq.Expressions;
//using Gdk;
using Gtk;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;

namespace VAS.UI.Helpers.Bindings
{
	/// <summary>
	/// Property binding for the colorbutton colors.
	/// </summary>
	public class ColorButtonBinding : PropertyBinding<Color>
	{
		ColorButton colorButton;
		bool inChange;

		public ColorButtonBinding (ColorButton colorButton, Expression<Func<IViewModel, Color>> propertyExpression) : base (propertyExpression)
		{
			this.colorButton = colorButton;
		}

		protected override void BindView ()
		{
			colorButton.ColorSet += HandleColorChanged;
		}

		protected override void UnbindView ()
		{
			colorButton.ColorSet -= HandleColorChanged;
		}

		protected override void WriteViewValue (Color val)
		{
			inChange = true;
			colorButton.Color = Misc.ToGdkColor (val);
			inChange = false;
		}

		void HandleColorChanged (object sender, EventArgs e)
		{
			if (!inChange) {
				Color color = Misc.ToLgmColor (colorButton.Color);
				WritePropertyValue (color);
			}
		}
	}
}
