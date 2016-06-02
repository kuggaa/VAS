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
using Cairo;
using Gdk;
using VAS.Core.Interfaces.Drawing;
using Pango;
using VAS.Core.Common;
using Color = VAS.Core.Common.Color;
using FontAlignment = VAS.Core.Common.FontAlignment;
using FontSlant = VAS.Core.Common.FontSlant;
using FontWeight = VAS.Core.Common.FontWeight;
using Image = VAS.Core.Common.Image;
using LineStyle = VAS.Core.Common.LineStyle;
using Point = VAS.Core.Common.Point;

namespace VAS.Drawing.Cairo
{
	public class CairoContext: IContext
	{

		Color savedStrokeColor, savedFillColor;
		Style fSlant, savedFSlant;
		Weight fWeight, savedFWeight;
		Alignment fAlignment, savedAlignment;
		int savedLineWidth, savedFontSize;
		bool savedClear;
		LineStyle savedLineStyle;
		string savedFontFamily;

		public CairoContext (Drawable window)
		{
			Value = Gdk.CairoHelper.Create (window);
			Init ();
		}

		public CairoContext (global::Cairo.Surface surface)
		{
			Value = new global::Cairo.Context (surface);
			Init ();
		}

		public CairoContext (global::Cairo.Context context)
		{
			Value = context;
			Init ();
		}

		void Init ()
		{
			PangoLayout = Pango.CairoHelper.CreateLayout (Value as global::Cairo.Context);
			StrokeColor = Color.Black;
			FillColor = Color.Black;
			UseAntialias = true;
			LineWidth = 2;
			FontSize = 12;
			FontFamily = Config.Style.Font;
			FontWeight = FontWeight.Normal;
			FontSlant = FontSlant.Normal;
			LineStyle = LineStyle.Normal;
			FontAlignment = FontAlignment.Center;
			ClearOperation = false;
		}

		public object Value {
			get;
			protected set;
		}

		global::Cairo.Context CContext {
			get {
				return Value as global::Cairo.Context;
			}
		}

		public Layout PangoLayout {
			get;
			protected set;
		}

		public void Dispose ()
		{
			CContext.Dispose ();
		}

		public bool DisableScalling {
			get;
			set;
		}

		public int LineWidth {
			set;
			protected get;
		}

		public Color StrokeColor {
			set;
			protected get;
		}

		public Color FillColor {
			set;
			protected get;
		}

		public string FontFamily {
			set;
			protected get;

		}

		public int FontSize {
			set;
			protected get;
		}

		public FontSlant FontSlant {
			set {
				switch (value) {
				case FontSlant.Italic:
					fSlant = Style.Italic;
					break;
				case FontSlant.Normal:
					fSlant = Style.Normal;
					break;
				case FontSlant.Oblique:
					fSlant = Style.Oblique;
					break;
				}
			}
		}

		public FontWeight FontWeight {
			set {
				fWeight = value.ToPangoWeight ();
			}
		}

		public FontAlignment FontAlignment {
			set {
				switch (value) {
				case FontAlignment.Left:
					fAlignment = Alignment.Left;
					break;
				case FontAlignment.Center:
					fAlignment = Alignment.Center;
					break;
				case FontAlignment.Right:
					fAlignment = Alignment.Right;
					break;
				}
			}
		}

		public LineStyle LineStyle {
			get;
			set;
		}

		public bool ClearOperation {
			get;
			set;
		}

		public bool UseAntialias {
			set;
			protected get;
		}

		public void Clear (Color color)
		{
			SetColor (color);
			CContext.Operator = Operator.Source;
			CContext.Paint ();
			CContext.Operator = Operator.Over;
		}

		public void Begin ()
		{
			savedStrokeColor = StrokeColor;
			savedFillColor = FillColor;
			savedFSlant = fSlant;
			savedFWeight = fWeight;
			savedAlignment = fAlignment;
			savedLineWidth = LineWidth;
			savedFontSize = FontSize;
			savedFontFamily = FontFamily;
			savedLineStyle = LineStyle;
			savedClear = ClearOperation;
			CContext.Save ();
		}

		public void TranslateAndScale (Point translation, Point scale)
		{
			if (!DisableScalling) {
				CContext.Translate (translation.X, translation.Y);
				CContext.Scale (scale.X, scale.Y);
			}
		}

		public void Clip (Area area)
		{
			if (!DisableScalling) {
				CContext.Rectangle (area.Start.X, area.Start.Y,
					area.Width, area.Height);
				CContext.Clip ();
			}
		}

		public Area UserToDevice (Area a)
		{
			double x, y, x2, y2, x3, y3;

			x = a.Start.X;
			y = a.Start.Y;
			CContext.UserToDevice (ref x, ref y);
			Area ua = new Area (new Point (x, y), 0, 0);
			x2 = a.TopRight.X;
			y2 = a.TopRight.Y;
			CContext.UserToDevice (ref x2, ref y2);
			ua.Width = x2 - x;
			x3 = a.BottomLeft.X;
			y3 = a.BottomLeft.Y;
			CContext.UserToDevice (ref x3, ref y3);
			ua.Height = y3 - y;
			return ua;
		}

		public void End ()
		{
			CContext.Restore ();
			ClearOperation = savedClear;
			StrokeColor = savedStrokeColor;
			FillColor = savedFillColor;
			fSlant = savedFSlant;
			fWeight = savedFWeight;
			fAlignment = savedAlignment;
			LineWidth = savedLineWidth;
			FontSize = savedFontSize;
			FontFamily = savedFontFamily;
			LineStyle = savedLineStyle;
		}

		public void DrawLine (Point start, Point stop)
		{
			CContext.LineWidth = LineWidth;
			CContext.MoveTo (start.X, start.Y);
			CContext.LineTo (stop.X, stop.Y);
			StrokeAndFill ();
		}

		public void DrawTriangle (Point corner, double width, double height,
		                          SelectionPosition position)
		{
			double x1, y1, x2, y2, x3, y3;

			x1 = corner.X;
			y1 = corner.Y;

			switch (position) {
			case SelectionPosition.Top:
				x2 = x1 + width / 2;
				y2 = y1 + height;
				x3 = x1 - width / 2;
				y3 = y1 + height;
				break;
			case SelectionPosition.Bottom:
			default:
				x2 = x1 + width / 2;
				y2 = y1 - height;
				x3 = x1 - width / 2;
				y3 = y1 - height;
				break;
			}

			SetColor (StrokeColor);
			CContext.MoveTo (x1, y1);
			CContext.LineTo (x2, y2);
			CContext.LineTo (x3, y3);
			CContext.ClosePath ();
			StrokeAndFill ();
		}

		public void DrawArea (params Point[] vertices)
		{
			double x1, y1;
			Point initial_point = vertices [0];
			CContext.MoveTo (initial_point.X, initial_point.Y);
			for (int i = 1; i < vertices.Length; i++) {
				x1 = vertices [i].X;
				y1 = vertices [i].Y;
				CContext.LineTo (x1, y1);

			}

			CContext.ClosePath ();
			StrokeAndFill ();
		}

		public void DrawRectangle (Point start, double width, double height)
		{
			CContext.Rectangle (new global::Cairo.Rectangle (start.X + LineWidth / 2,
				start.Y + LineWidth / 2,
				width - LineWidth,
				height - LineWidth));
			StrokeAndFill (false);
		}

		static public double ByteToDouble (byte val)
		{
			return (double)(val) / byte.MaxValue;
		}

		public static global::Cairo.Color RGBToCairoColor (Color c)
		{
			return new global::Cairo.Color (ByteToDouble (c.R),
				ByteToDouble (c.G),
				ByteToDouble (c.B));
		}

		public void DrawRoundedRectangle (Point start, double width, double height, double radius)
		{
			DrawRoundedRectangle (start, width, height, radius, true);
		}

		public void DrawRoundedRectangle (Point start, double width, double height, double radius, bool strokeAndFill)
		{
			double x, y;

			x = start.X + LineWidth / 2;
			y = start.Y + LineWidth / 2;
			height -= LineWidth;
			width -= LineWidth;

			if ((radius > height / 2) || (radius > width / 2))
				radius = Math.Min (height / 2, width / 2);

			CContext.MoveTo (x, y + radius);
			CContext.Arc (x + radius, y + radius, radius, Math.PI, -Math.PI / 2);
			CContext.LineTo (x + width - radius, y);
			CContext.Arc (x + width - radius, y + radius, radius, -Math.PI / 2, 0);
			CContext.LineTo (x + width, y + height - radius);
			CContext.Arc (x + width - radius, y + height - radius, radius, 0, Math.PI / 2);
			CContext.LineTo (x + radius, y + height);
			CContext.Arc (x + radius, y + height - radius, radius, Math.PI / 2, Math.PI);
			CContext.ClosePath ();
			if (strokeAndFill) {
				StrokeAndFill ();
			}
		}

		public void DrawCircle (Point center, double radius)
		{
			CContext.Arc (center.X, center.Y, radius, 0, 2 * Math.PI);
			StrokeAndFill ();
		}

		public void DrawPoint (Point point)
		{
			DrawCircle (point, LineWidth);
		}

		public void DrawText (Point point, double width, double height, string text,
		                      bool escape = false, bool ellipsize = false)
		{
			Pango.Rectangle inkRect, logRect;

			if (text == null) {
				return;
			}

			if (escape) {
				text = GLib.Markup.EscapeText (text);
			}

			if (PangoLayout == null) {
				PangoLayout = Pango.CairoHelper.CreateLayout (CContext);
			}

			if (ellipsize) {
				PangoLayout.Ellipsize = EllipsizeMode.End;
			} else {
				PangoLayout.Ellipsize = EllipsizeMode.None;
			}
			PangoLayout.FontDescription = FontDescription.FromString (
				String.Format ("{0} {1}px", FontFamily, FontSize));
			PangoLayout.FontDescription.Weight = fWeight;
			PangoLayout.FontDescription.Style = fSlant;
			PangoLayout.Width = Pango.Units.FromPixels ((int)width);
			PangoLayout.Alignment = fAlignment;
			PangoLayout.SetMarkup (GLib.Markup.EscapeText (text));
			SetColor (StrokeColor);
			Pango.CairoHelper.UpdateLayout (CContext, PangoLayout);
			PangoLayout.GetPixelExtents (out inkRect, out logRect);
			CContext.MoveTo (point.X, point.Y + height / 2 - (double)logRect.Height / 2);
			Pango.CairoHelper.ShowLayout (CContext, PangoLayout);
			CContext.NewPath ();
		}

		public void DrawImage (Image image)
		{
			Gdk.CairoHelper.SetSourcePixbuf (CContext, image.Value, 0, 0);
			CContext.Paint ();
		}

		public void DrawImage (Point start, double width, double height, Image image, ScaleMode mode, bool masked = false)
		{
			double scaleX, scaleY;
			Point offset;

			image.ScaleFactor ((int)width, (int)height, mode, out scaleX, out scaleY, out offset);
			CContext.Save ();
			CContext.Translate (start.X + offset.X, start.Y + offset.Y);
			CContext.Scale (scaleX, scaleY);
			if (masked) {
				CContext.PushGroup ();
				Gdk.CairoHelper.SetSourcePixbuf (CContext, image.Value, 0, 0);
				CContext.Paint ();
				var src = CContext.PopGroup ();
				SetColor (FillColor);
				CContext.Mask (src);
				src.Dispose ();
			} else {
				Gdk.CairoHelper.SetSourcePixbuf (CContext, image.Value, 0, 0);
				CContext.Paint ();
			}
			CContext.Restore ();
		}

		public void DrawCircleImage (Point center, double radius, Image image)
		{
			DrawCircle (center, radius);
			CContext.Save ();
			CContext.Arc (center.X, center.Y, radius, 0, 2 * Math.PI);
			CContext.Clip ();
			DrawImage (new Point (center.X - radius, center.Y - radius), radius * 2, radius * 2, image,
				ScaleMode.AspectFill, false);
			CContext.Restore ();
		}

		public void DrawEllipse (Point center, double axisX, double axisY)
		{
			double max = Math.Max (axisX, axisY);
			CContext.Save ();
			CContext.Translate (center.X, center.Y);
			CContext.Scale (axisX / max, axisY / max);
			CContext.Arc (0, 0, max, 0, 2 * Math.PI);
			StrokeAndFill ();
			CContext.Restore ();
		}

		public void DrawArrow (Point start, Point stop, int lenght, double radians, bool closed)
		{
			double vx1, vy1, vx2, vy2;
			double angle = Math.Atan2 (stop.Y - start.Y, stop.X - start.X) + Math.PI;

			vx1 = stop.X + (lenght + LineWidth) * Math.Cos (angle - radians);
			vy1 = stop.Y + (lenght + LineWidth) * Math.Sin (angle - radians);
			vx2 = stop.X + (lenght + LineWidth) * Math.Cos (angle + radians);
			vy2 = stop.Y + (lenght + LineWidth) * Math.Sin (angle + radians);

			CContext.MoveTo (stop.X, stop.Y);
			CContext.LineTo (vx1, vy1);
			if (!closed) {
				CContext.MoveTo (stop.X, stop.Y);
				CContext.LineTo (vx2, vy2);
			} else {
				CContext.LineTo (vx2, vy2);
				CContext.ClosePath ();
			}
			StrokeAndFill (false);
		}

		public void DrawSurface (ISurface surface, Point p = null)
		{
			ImageSurface image;

			image = surface.Value as ImageSurface;
			if (p == null) {
				CContext.SetSourceSurface (image, 0, 0);
				CContext.Paint ();
			} else {
				CContext.SetSourceSurface (image, (int)p.X, (int)p.Y);
				CContext.Rectangle (p.X, p.Y, image.Width, image.Height);
				CContext.Fill ();
			}
		}

		public void DrawSurface (Point start, double width, double height, ISurface surface, ScaleMode mode)
		{
			double scaleX, scaleY;
			Point offset;

			BaseImage<Pixbuf>.ScaleFactor (surface.Width, surface.Height, (int)width, (int)height, mode, out scaleX, out scaleY, out offset);
			CContext.Save ();
			CContext.Translate (offset.X, offset.Y);
			CContext.Scale (scaleX, scaleY);
			DrawSurface (surface, start);
			CContext.Restore ();
		}

		void SetDash ()
		{
			switch (LineStyle) {
			case LineStyle.Normal:
				CContext.SetDash (new double[] { }, 0);
				break;	
			default:
				CContext.SetDash (new double[] { 10 * LineWidth / 2, 10 * LineWidth / 2 }, 0);
				break;
			}
		}

		void SetAntialias ()
		{
			if (UseAntialias)
				CContext.Antialias = Antialias.Default;
			else
				CContext.Antialias = Antialias.None;
		}

		void StrokeAndFill (bool roundCaps = true)
		{
			SetAntialias ();
			SetDash ();
			if (ClearOperation) {
				CContext.Operator = Operator.Clear;
			} else {
				CContext.Operator = Operator.Over;
			}
			if (roundCaps) {
				CContext.LineCap = LineCap.Round;
				CContext.LineJoin = LineJoin.Round;
			} else {
				CContext.LineCap = LineCap.Butt;
				CContext.LineJoin = LineJoin.Miter;
			}
			CContext.LineWidth = LineWidth;
			SetColor (StrokeColor);
			CContext.StrokePreserve ();
			SetColor (FillColor);
			CContext.Fill ();
		}

		void SetColor (Color color)
		{
			if (color != null) {
				CContext.SetSourceRGBA ((double)color.R / byte.MaxValue,
					(double)color.G / byte.MaxValue,
					(double)color.B / byte.MaxValue,
					(double)color.A / byte.MaxValue);
			} else {
				CContext.SetSourceRGBA (0, 0, 0, 0);
			}
		}
	}
}

