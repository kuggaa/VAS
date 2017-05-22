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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gdk;
using Newtonsoft.Json;
using SkiaSharp;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using Color = VAS.Core.Common.Color;
using FontAlignment = VAS.Core.Common.FontAlignment;
using FontSlant = VAS.Core.Common.FontSlant;
using FontWeight = VAS.Core.Common.FontWeight;
using Image = VAS.Core.Common.Image;
using LineStyle = VAS.Core.Common.LineStyle;
using Point = VAS.Core.Common.Point;

namespace VAS.Drawing.Skia
{
	public class SkiaBackend : IDrawingToolkit
	{
		IContext context;
		bool disableScalling;
		Stack<ContextStatus> contextStatusStack;
		ContextStatus currentContextStatus;

		public SkiaBackend ()
		{
			contextStatusStack = new Stack<ContextStatus> ();
			currentContextStatus = new ContextStatus ();
			contextStatusStack.Push (currentContextStatus);
		}

		public IContext Context {
			set {
				context = value;
			}
			get {
				return context;
			}
		}

		public int LineWidth {
			set {
				currentContextStatus.LineWidth = value;
			}
		}

		public Color StrokeColor {
			set {
				currentContextStatus.StrokeColor = value;
			}
		}

		public Color FillColor {
			set {
				currentContextStatus.FillColor = value;
			}
		}

		public string FontFamily {
			set {
				currentContextStatus.FontFamily = value;
			}
		}

		public int FontSize {
			set {
				currentContextStatus.FontSize = value;
			}
		}

		public FontSlant FontSlant {
			set {
				currentContextStatus.FontSlant = value;
			}
		}

		public FontWeight FontWeight {
			set {
				currentContextStatus.FontWeight = value;
			}
		}

		public FontAlignment FontAlignment {
			set {
				currentContextStatus.FontAlignment = value;
			}
		}

		public LineStyle LineStyle {
			set {
				currentContextStatus.LineStyle = value;
			}
		}

		public bool UseAntialias {
			set {
				currentContextStatus.UseAntialias = value;
			}
		}

		public bool ClearOperation {
			set {
				currentContextStatus.Clear = value;
			}
		}

		SKPaint Paint {
			get {
				return currentContextStatus.Paint;
			}
		}

		SKCanvas Canvas {
			get {
				return context.Value as SKCanvas;
			}
		}

		public ISurface CreateSurfaceFromResource (string resourceName, bool warnOnDispose = true)
		{
			Image img = Resources.LoadImage (resourceName);
			return CreateSurface (img.Width, img.Height, img, warnOnDispose);
		}

		public ISurface CreateSurface (string absolutePath, bool warnOnDispose = true)
		{
			Image img = new Image (absolutePath);
			return CreateSurface (img.Width, img.Height, img, warnOnDispose);
		}

		public ISurface CreateSurface (int width, int height, Image image = null, bool warnOnDispose = true)
		{
			return new SkiaSurface (width, height, image, warnOnDispose);
		}

		public void Clear (Color color)
		{
			Canvas.DrawColor (color.ToSKColor (), SKBlendMode.Src);
		}

		public void Begin ()
		{
			contextStatusStack.Push (currentContextStatus.Clone ());
			Canvas.Save ();
		}

		public void End ()
		{
			Canvas.Restore ();
			currentContextStatus = contextStatusStack.Pop ();
		}

		public void TranslateAndScale (Point translation, Point scale)
		{
			Canvas.Translate ((float)translation.X, (float)translation.Y);
			Canvas.Scale ((float)scale.X, (float)scale.Y);
		}

		public void Clip (Area area)
		{
			Canvas.ClipRect (area.ToSKRect ());
		}

		public Area UserToDevice (Area a)
		{
			SKMatrix inverse;
			bool success = Canvas.TotalMatrix.TryInvert (out inverse);
			if (success) {
				SKRect userRect = a.ToSKRect ();
				SKRect deviceRect = inverse.MapRect (userRect);
				return deviceRect.ToArea ();
			} else {
				throw new InvalidOperationException ();
			}
		}

		void Setup ()
		{
			Paint.Color = currentContextStatus.StrokeColor.ToSKColor ();
			Paint.BlendMode = SKBlendMode.SrcOver;
			Paint.IsAntialias = currentContextStatus.UseAntialias;
			Paint.StrokeCap = SKStrokeCap.Round;
			Paint.StrokeJoin = SKStrokeJoin.Round;
			Paint.StrokeWidth = currentContextStatus.LineWidth;
		}

		void Draw (Action draw)
		{
			Setup ();
			Paint.Color = currentContextStatus.StrokeColor.ToSKColor ();
			Paint.IsStroke = true;
			draw ();
			Paint.Color = currentContextStatus.FillColor.ToSKColor ();
			Paint.IsStroke = false;
			draw ();
		}

		public void DrawLine (Point start, Point stop)
		{
			Draw (() => {
				Canvas.DrawLine ((float)start.X, (float)start.Y, (float)stop.X, (float)stop.Y, Paint);
			});
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

			using (var path = new SKPath ()) {
				path.MoveTo ((float)x1, (float)y1);
				path.LineTo ((float)x2, (float)y2);
				path.LineTo ((float)x3, (float)y3);
				path.Close ();
				Draw (() => {
					Canvas.DrawPath (path, Paint);
				});
			}
		}

		public void DrawArea (params Point [] vertices)
		{
			using (var path = new SKPath ()) {
				path.MoveTo ((float)vertices [0].X, (float)vertices [0].Y);
				for (int i = 1; i < vertices.Length; i++) {
					path.LineTo ((float)vertices [i].X, (float)vertices [i].Y);
				}
				path.Close ();
				Draw (() => {
					Canvas.DrawPath (path, Paint);
				});
			}
		}

		public void DrawRectangle (Point start, double width, double height)
		{
			Draw (() => {
				Canvas.DrawRect (CreateRectangle (start, width, height), Paint);
			});
		}

		public void DrawRoundedRectangle (Point start, double width, double height, double radius)
		{
			Draw (() => {
				Canvas.DrawRoundRect (CreateRectangle (start, width, height), (float)radius, (float)radius, Paint);
			});
		}

		public void DrawCircle (Point center, double radius)
		{
			Draw (() => {
				Canvas.DrawCircle ((float)center.X, (float)center.Y, (float)radius, Paint);
			});
		}

		public void DrawPoint (Point point)
		{
			Draw (() => {
				Canvas.DrawPoint ((float)point.X, (float)point.Y, Paint);
			});
		}

		public void DrawText (Point point, double width, double height, string text,
							  bool escape = false, bool ellipsize = false)
		{

			Paint.Color = currentContextStatus.StrokeColor.ToSKColor ();
			Paint.TextSize = currentContextStatus.FontSize;
			Paint.TextAlign = currentContextStatus.FontAlignment.ToSKTextAlign ();
			Paint.LcdRenderText = true;
			Paint.IsAntialias = true;
			var typeface = SKTypeface.FromFamilyName (currentContextStatus.FontFamily,
													  currentContextStatus.FontWeight.ToSKFontStyleWeight (),
													  SKFontStyleWidth.Normal,
													  currentContextStatus.FontSlant.ToSKFontStyleLant ());
			Paint.Typeface = typeface;
			SkiaTextBox.Draw (text, point.X, point.Y, width, height, Canvas, Paint);
			Paint.Typeface = null;
		}

		public void DrawImage (Image image)
		{
			Canvas.DrawImage (image.ToSKImage (), 0, 0, Paint);
		}

		public void DrawImage (Point start, double width, double height, Image image, ScaleMode mode, bool masked = false)
		{
			double scaleX, scaleY;
			Point offset;

			Setup ();
			image.ScaleFactor ((int)width, (int)height, mode, out scaleX, out scaleY, out offset);
			using (var paint = Paint.Clone ()) {
				if (masked) {
					paint.ColorFilter = SKColorFilter.CreateBlendMode (currentContextStatus.FillColor.ToSKColor (), SKBlendMode.SrcIn);
					paint.FilterQuality = SKFilterQuality.Medium;
				}
				Canvas.Save ();
				Canvas.Translate ((float)(offset.X + start.X), (float)(offset.Y + start.Y));
				Canvas.Scale ((float)scaleX, (float)scaleY);
				Canvas.DrawImage (image.ToSKImage (), 0, 0, paint);
				Canvas.Restore ();
			}
		}

		public void DrawCircleImage (Point center, double radius, Image image)
		{
			Setup ();
			DrawCircle (center, radius);
			Canvas.Save ();
			var path = new SKPath ();
			path.AddCircle ((float)center.X, (float)center.Y, (float)radius);
			Canvas.ClipPath (path);
			DrawImage (new Point (center.X - radius, center.Y - radius), radius * 2, radius * 2, image,
				ScaleMode.AspectFill, false);
			Canvas.Restore ();
		}

		public void DrawEllipse (Point center, double axisX, double axisY)
		{
			Draw (() => {
				Canvas.DrawOval ((float)center.X, (float)center.Y, (float)axisX, (float)axisY, Paint);
			});
		}

		public void DrawArrow (Point start, Point stop, int lenght, double radians, bool closed)
		{
			double vx1, vy1, vx2, vy2;
			double angle = Math.Atan2 (stop.Y - start.Y, stop.X - start.X) + Math.PI;
			int lineWidth = currentContextStatus.LineWidth;

			vx1 = stop.X + (lenght + lineWidth) * Math.Cos (angle - radians);
			vy1 = stop.Y + (lenght + lineWidth) * Math.Sin (angle - radians);
			vx2 = stop.X + (lenght + lineWidth) * Math.Cos (angle + radians);
			vy2 = stop.Y + (lenght + lineWidth) * Math.Sin (angle + radians);

			using (var path = new SKPath ()) {
				path.MoveTo ((float)stop.X, (float)stop.Y);
				path.LineTo ((float)vx1, (float)vy1);
				if (!closed) {
					path.MoveTo ((float)stop.X, (float)stop.Y);
					path.LineTo ((float)vx2, (float)vy2);
				} else {
					path.LineTo ((float)vx2, (float)vy2);
					path.Close ();
				}
				Draw (() => {
					Canvas.DrawPath (path, Paint);
				});
			}
		}

		public void DrawSurface (ISurface surface, Point p = null)
		{
			if (p == null) {
				p = new Point (0, 0);
			}
			Canvas.DrawBitmap ((SKBitmap)surface.Value, (float)p.X, (float)p.Y, Paint);
		}

		public void DrawSurface (Point start, double width, double height, ISurface surface, ScaleMode mode)
		{
			double scaleX, scaleY;
			Point offset;

			BaseImage<IDisposable>.ScaleFactor (surface.Width, surface.Height, (int)width, (int)height, mode,
												out scaleX, out scaleY, out offset);

			Canvas.Save ();
			Canvas.Translate ((float)(offset.X + start.X), (float)(offset.Y + start.Y));
			Canvas.Scale ((float)scaleX, (float)scaleY);
			Canvas.DrawBitmap (surface.Value as SKBitmap, 0, 0, Paint);
			Canvas.Restore ();
		}

		public Image Copy (ICanvas canvas, Area area)
		{
			IntPtr len;
			var bitmap = new SKBitmap ((int)area.Width, (int)area.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
			var surface = SKSurface.Create (bitmap.Info.Width, bitmap.Info.Height, SKColorType.Rgba8888,
										 SKAlphaType.Premul, bitmap.GetPixels (out len), bitmap.Info.RowBytes);
			using (SkiaContext c = new SkiaContext (surface)) {
				canvas.Draw (c, null);
			}

			using (var image = SKImage.FromBitmap (bitmap)) {
				using (var data = image.Encode (SKEncodedImageFormat.Png, 100)) {
					return new Image (data.AsStream ());
				}
			}
		}

		public void Save (ICanvas canvas, Area area, string filename)
		{
			Image img = Copy (canvas, area);
			img.Save (filename);
			img.Dispose ();
		}

		SKRect CreateRectangle (Point start, double width, double height)
		{
			return SKRect.Create ((float)start.X, (float)start.Y, (float)width, (float)height);
		}

		void SetDash ()
		{
			switch (currentContextStatus.LineStyle) {
			case LineStyle.Normal:
				//Canvas.SetDash (new double [] { }, 0);
				break;
			default:
				//Canvas.SetDash (new double [] { 10 * LineWidth / 2, 10 * LineWidth / 2 }, 0);
				break;
			}
		}

		public void MeasureText (string text, out int width, out int height,
								 string fontFamily, int fontSize, FontWeight fontWeight)
		{
			using (SKPaint textPaint = new SKPaint ()) {
				textPaint.TextAlign = SKTextAlign.Left;
				textPaint.TextSize = fontSize;
				textPaint.Typeface = SKTypeface.FromFamilyName (fontFamily, fontWeight.ToSKFontStyleWeight (),
																SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
				var bounds = new SKRect ();
				textPaint.MeasureText (text, ref bounds);
				width = (int)bounds.Width;
				height = (int)bounds.Height;
			}
		}

	}

	/// <summary>
	/// Context Status class to save/retrieve intrernal properties
	/// </summary>
	class ContextStatus
	{
		public ContextStatus ()
		{
			StrokeColor = Color.Black;
			FillColor = Color.Black;
			UseAntialias = true;
			LineWidth = 2;
			FontSize = 12;
			FontFamily = App.Current.Style.Font;
			FontWeight = FontWeight.Normal;
			FontSlant = FontSlant.Normal;
			LineStyle = LineStyle.Normal;
			FontAlignment = FontAlignment.Center;
			Paint = new SKPaint ();
			Paint.IsAntialias = true;
		}

		[JsonIgnore]
		public SKPaint Paint {
			get;
			set;
		}

		public Color StrokeColor {
			get;
			set;
		}

		public Color FillColor {
			get;
			set;
		}

		public int LineWidth {
			get;
			set;
		}

		public int FontSize {
			get;
			set;
		}

		public FontSlant FontSlant {
			get;
			set;
		}

		public FontWeight FontWeight {
			get;
			set;
		}

		public FontAlignment FontAlignment {
			get;
			set;
		}

		public string FontFamily {
			get;
			set;
		}

		public bool Clear {
			get;
			set;
		}

		public LineStyle LineStyle {
			get;
			set;
		}

		public bool UseAntialias {
			get;
			set;
		}

		public ContextStatus Clone ()
		{
			var newContextStatus = Cloner.Clone (this);
			newContextStatus.Paint = Paint.Clone ();
			return newContextStatus;
		}
	}
}

