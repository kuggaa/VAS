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
using Gtk;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using Image = VAS.Core.Common.Image;

namespace VAS.UI.Helpers.Bindings
{
	public static class BindingsExtensions
	{

		/// <summary>
		/// Gets the binding context for a given widget and creates it if doesn't exist.
		/// </summary>
		/// <returns>The binding context.</returns>
		/// <param name="widget">Widget.</param>
		public static BindingContext GetBindingContext (this Widget widget)
		{
			if (widget.Data.ContainsKey ("Bindings")) {
				return widget.Data ["Bindings"] as BindingContext;
			}
			var context = new BindingContext ();
			widget.Data ["Bindings"] = context;
			return context;
		}

		/// <summary>
		/// Bind the specified label to a property by name.
		/// </summary>
		/// <param name="label">Label.</param>
		public static LabelBinding Bind (this Label label, Expression<Func<IViewModel, string>> propertyExpression)
		{
			return new LabelBinding (label, propertyExpression);
		}

		/// <summary>
		/// Bind the specified label to a property by name using a converter.
		/// </summary>
		/// <returns>The bind.</returns>
		/// <param name="label">Label.</param>
		/// <param name="propertyExpression">Property expression.</param>
		/// <param name="converter">Converter.</param>
		public static LabelBinding Bind (this Label label, Expression<Func<IViewModel, object>> propertyExpression, TypeConverter converter)
		{
			return new LabelBinding (label, propertyExpression, converter);
		}

		/// <summary>
		/// Bind the specified text view to a property by name.
		/// </summary>
		/// <param name="textView">Text view.</param>
		/// <param name="propertyName">Property name.</param>
		public static TextViewBinding Bind (this TextView textView, Expression<Func<IViewModel, string>> propertyExpression)
		{
			return new TextViewBinding (textView, propertyExpression);
		}

		/// <summary>
		/// Bind the specified entry to a property by name.
		/// </summary>
		/// <param name="entry">Entry.</param>
		public static EntryBinding Bind (this Entry entry, Expression<Func<IViewModel, string>> propertyExpression)
		{
			return new EntryBinding (entry, propertyExpression);
		}

		/// <summary>
		/// Bind the specified image to a property by name. The image is scalled to <paramref name="widt"/> and
		/// <paramref name="height"/> if specified.
		/// </summary>
		/// <param name="image">Image.</param>
		/// <param name="propertyExpression">Property expression.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public static ImageBinding Bind (this Gtk.Image image, Expression<Func<IViewModel, Image>> propertyExpression, int width = 0, int height = 0)
		{
			return new ImageBinding (image, propertyExpression, width, height);
		}

		/// <summary>
		/// Bind the specified toggle button to a command by name with the arguments to be passed when the button is
		/// activated and deactivated.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="commandFunc">Command function.</param>
		/// <param name="parameterActive">Parameter when active.</param>
		/// <param name="parameterInactive">Parameter when inactive.</param>
		public static ToggleButtonBinding Bind (this ToggleButton button, Func<IViewModel, Command> commandFunc,
												object parameterActive, object parameterInactive)
		{
			return new ToggleButtonBinding (button, commandFunc, parameterActive, parameterInactive);
		}

		/// <summary>
		/// Bind the specified button to a command by name with the paramater to pass to the command.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="commandFunc">Command function.</param>
		/// <param name="parameter">Parameter.</param>
		public static ButtonBinding Bind (this Button button, Func<IViewModel, Command> commandFunc, object parameter = null)
		{
			return new ButtonBinding (button, commandFunc, parameter);
		}

		/// <summary>
		/// Bind the specified button to a command by name with the paramater to pass to the command using a different
		/// than the one defined in the command.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="image">Image.</param>
		/// <param name="commandFunc">Command name.</param>
		/// <param name="parameter">Parameter.</param>
		public static ButtonBinding BindWithIcon (this Button button, Image image, Func<IViewModel, Command> commandFunc, object parameter = null)
		{
			return new ButtonBinding (button, commandFunc, parameter, image, "");
		}

		/// <summary>
		/// Bind the specified SpinButton to a property by name.
		/// </summary>
		/// <param name="SpinButton">SpinButton.</param>
		public static SpinBinding Bind (this SpinButton spinButton, Expression<Func<IViewModel, object>> propertyExpression,
										TypeConverter converter = null)
		{
			return new SpinBinding (spinButton, propertyExpression, converter);
		}

		/// <summary>
		/// Bind the specified colorButton and propertyExpression.
		/// </summary>
		/// <param name="colorButton">Color button.</param>
		/// <param name="propertyExpression">Property expression.</param>
		public static ColorButtonBinding Bind (this ColorButton colorButton, Expression<Func<IViewModel, Color>> propertyExpression)
		{
			return new ColorButtonBinding (colorButton, propertyExpression);
		}
	}
}
