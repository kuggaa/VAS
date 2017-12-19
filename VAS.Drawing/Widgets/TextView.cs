//
//  Copyright (C) 2017 
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
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;

namespace VAS.Drawing.Widgets
{
	/// <summary>
	/// Text view canvas used to draw custom texts
	/// </summary>
	public class TextView : Canvas
	{
		int fontSize;
		FontWeight fontWeight;
		FontSlant fontSlant;
		string text;
		Color textColor;

		public TextView (IWidget widget) : base(widget)
		{
		}

		/// <summary>
		/// Gets or sets the size of the font size.
		/// </summary>
		/// <value>The size of the font.</value>
		public int FontSize {
			get {
				return fontSize;
			}

			set {
				fontSize = value;
				widget?.ReDraw ();
			}
		}

		/// <summary>
		/// Gets or sets the font weight.
		/// </summary>
		/// <value>The font weight.</value>
		public FontWeight FontWeight {
			get {
				return fontWeight;
			}

			set {
				fontWeight = value;
				widget?.ReDraw ();
			}
		}

		/// <summary>
		/// Gets or sets the font slant.
		/// </summary>
		/// <value>The font slant.</value>
		public FontSlant FontSlant {
			get {
				return fontSlant;
			}

			set {
				fontSlant = value;
				widget?.ReDraw ();
			}
		}

		/// <summary>
		/// Gets or sets the text to be displayed
		/// </summary>
		/// <value>The text.</value>
		public string Text { 
			get {
				return text;
			}

			set {
				text = value;
				widget?.ReDraw ();
			} 
		}

		/// <summary>
		/// Gets or sets the color of the text.
		/// </summary>
		/// <value>The color of the text.</value>
		public Color TextColor { 
			get {
				return textColor;
			}

			set {
				textColor = value;
				widget?.ReDraw ();
			} 
		}

		public override void Draw (IContext context, Area area)
		{
			Begin (context);

			tk.FontSize = FontSize;
			tk.FontWeight = FontWeight;
			tk.FontSlant = FontSlant;
			tk.StrokeColor = TextColor;
			if (Text != null) {
				tk.DrawText (new Point (0, 0), widget.Width, widget.Height, Text);
			}

			End ();
		}
	}
}
