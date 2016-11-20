//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using VAS.Core.Store.Drawables;

namespace VAS.Drawing.CanvasObjects.Timeline
{
	public class LabelView : CanvasObject, ICanvasSelectableObject
	{
		double width, height, offsetY;
		protected const int DEFAULT_FONT_SIZE = 12;
		protected const FontWeight DEFAULT_FONT_WEIGHT = FontWeight.Bold;

		public LabelView ()
		{
			Color = Color.Red1;
		}

		public virtual string Name {
			get;
			set;
		}

		public virtual Color Color {
			get;
			set;
		}

		public double Width {
			get {
				return width;
			}

			set {
				width = value;
				HandleSizeChanged ();
			}
		}

		public double Height {
			get {
				return height;
			}

			set {
				height = value;
				HandleSizeChanged ();
			}
		}

		public double OffsetY {
			get {
				return offsetY;
			}

			set {
				offsetY = value;
				HandleSizeChanged ();
			}
		}

		public double RequiredWidth {
			get {
				int width, height;
				App.Current.DrawingToolkit.MeasureText (
					Name, out width, out height, App.Current.Style.Font,
					DEFAULT_FONT_SIZE, DEFAULT_FONT_WEIGHT);
				return TextOffset + width + StyleConf.TimelineLabelHSpacing;
			}
		}

		public double Scroll {
			get;
			set;
		}

		public Color BackgroundColor {
			get;
			set;
		}

		protected double RectSize {
			get {
				return Height - StyleConf.TimelineLabelVSpacing * 2;
			}
		}

		protected double TextOffset {
			get {
				return StyleConf.TimelineLabelHSpacing * 2 + RectSize;
			}
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			double hs, vs;
			double y;

			hs = StyleConf.TimelineLabelHSpacing;
			vs = StyleConf.TimelineLabelVSpacing;
			y = OffsetY - Math.Floor (Scroll);
			tk.Begin ();
			tk.FillColor = BackgroundColor;
			tk.StrokeColor = BackgroundColor;
			tk.LineWidth = 0;
			tk.DrawRectangle (new Point (0, y), Width, Height);

			/* Draw a rectangle with the category color */
			tk.FillColor = Color;
			tk.StrokeColor = Color;
			tk.DrawRectangle (new Point (hs, y + vs), RectSize, RectSize);

			/* Draw category name */
			tk.FontSlant = FontSlant.Normal;
			tk.FontWeight = DEFAULT_FONT_WEIGHT;
			tk.FontSize = DEFAULT_FONT_SIZE;
			tk.FillColor = App.Current.Style.PaletteWidgets;
			tk.FontAlignment = FontAlignment.Left;
			tk.StrokeColor = App.Current.Style.PaletteWidgets;
			tk.DrawText (new Point (TextOffset, y), Width - TextOffset, Height, Name);
			tk.End ();
		}

		public Selection GetSelection (Point point, double precision, bool inMotion = false)
		{
			Rectangle r = new Rectangle (new Point (0, OffsetY), Width, Height);
			Selection s = r.GetSelection (point, precision);
			if (s != null) {
				s.Drawable = this;
				s.Position = SelectionPosition.All;
			}
			return s;
		}

		public void Move (Selection s, Point dst, Point start)
		{
		}

		protected virtual void HandleSizeChanged ()
		{
		}
	}
}
