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

namespace VAS.Core.Interfaces.Drawing
{
	public interface IContext : IDisposable
	{
		object Value { get; }
	}

	public interface ISurface : IDisposable
	{
		Image Copy ();

		object Value { get; }

		IContext Context { get; }

		int Width { get; }

		int Height { get; }

		float DeviceScaleFactor { get; }
	}

	public interface IDrawingToolkit
	{
		IContext Context { set; get; }

		int LineWidth { set; }

		bool ClearOperation { set; }

		Color StrokeColor { set; }

		Color FillColor { set; }

		string FontFamily { set; }

		FontSlant FontSlant { set; }

		FontWeight FontWeight { set; }

		FontAlignment FontAlignment { set; }

		int FontSize { set; }

		LineStyle LineStyle { set; }

		bool UseAntialias { set; }

		/// <summary>
		/// Creates the surface from an icon name.
		/// </summary>
		/// <returns>The newly created surface.</returns>
		/// <param name="iconName">Icon name.</param>
		/// <param name="warnOnDispose">If set to <c>true</c> warn on dispose.</param>
		/// <param name="useDeviceScaleFactor">If set to <c>true</c> use device scale factor.</param>
		ISurface CreateSurfaceFromIcon (string iconName, bool warnOnDispose = true, bool useDeviceScaleFactor = true);

		ISurface CreateSurfaceFromResource (string resourceName, bool warnOnDispose = true, bool useDeviceScaleFactor = true);

		ISurface CreateSurface (string absolutePath, bool warnOnDispose = true, bool useDeviceScaleFactor = true);

		ISurface CreateSurface (int width, int height, Image image = null, bool warnOnDispose = true, bool useDeviceScaleFactor = true);

		void DrawSurface (ISurface surface, Point p = null, bool masked = false);

		void DrawSurface (Point start, double width, double height, ISurface surface, ScaleMode mode, bool masked = false);

		void Begin ();

		void End ();

		void Clear (Color color);

		void TranslateAndScale (Point translation, Point scale);

		void Clip (Area area);

		void ClipCircle (Point center, double radius);

		void ClipRoundRectangle (Point start, double width, double height, double radius);

		void DrawLine (Point start, Point stop);

		void DrawTriangle (Point corner, double width, double height,
						   SelectionPosition orientation);

		void DrawRectangle (Point start, double width, double height);

		void DrawRoundedRectangle (Point start, double width, double height, double radius);

		void DrawArea (params Point [] vertices);

		void DrawPoint (Point point);

		void DrawCircle (Point center, double radius);

		void DrawArc (Point center, double radius, double angle1, double angle2);

		void DrawEllipse (Point center, double axisX, double axisY);

		void DrawText (Point point, double width, double height, string text, bool escape = false, bool ellipsize = false);

		void DrawImage (Image image, float alpha = 1);

		void DrawImage (Point start, double width, double height, Image image, ScaleMode mode, bool masked = false, float alpha = 1);

		void DrawCircleImage (Point center, double radius, Image image);

		void DrawArrow (Point start, Point stop, int lenght, double degrees, bool closed);

		void Save (ICanvas canvas, Area area, string filename);

		Image Copy (ICanvas canvas, Area area);

		Area UserToDevice (Area area);

		void Invoke (EventHandler handler);

		void MeasureText (string text, out int width, out int height,
						  string fontFamily, int fontSize, FontWeight fontWeight);
	}
}

