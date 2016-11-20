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
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store.Drawables;

namespace VAS.Drawing.CanvasObjects.Timeline
{
	public class NeedleView : CanvasObject, ICanvasSelectableObject
	{
		static ISurface needle;
		static bool surfacesInitialized = false;

		public NeedleView ()
		{
			LoadSurfaces ();
			Width = needle.Width;
			X = 0;
			TimelineHeight = 0;
		}

		protected override void Dispose (bool disposing)
		{
			if (Disposed)
				return;

			base.Dispose (disposing);
		}

		public double X {
			get;
			set;
		}

		public double TimelineHeight {
			get;
			set;
		}

		public double Width {
			get;
			set;
		}

		public double Height {
			get {
				return needle.Height;
			}
		}

		public Point TopLeft {
			get {
				return new Point (X - Width / 2, TimelineHeight - needle.Height);
			}
		}

		public double MaxPointX {
			get;
			set;
		}

		Area Area {
			get {
				return new Area (TopLeft, Width, Height);
			}
		}

		static public void LoadSurfaces ()
		{
			if (!surfacesInitialized) {
				needle = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.TimelineNeedleResource, false);
				surfacesInitialized = true;
			}
		}

		public override void Draw (IDrawingToolkit tk, VAS.Core.Common.Area area)
		{
			if (!UpdateDrawArea (tk, area, Area)) {
				return;
			}

			tk.Begin ();
			tk.DrawSurface (TopLeft, StyleConf.TimelineNeedleBigWidth, StyleConf.TimelineNeedleBigHeight, needle, ScaleMode.AspectFit);
			tk.End ();
		}

		public Selection GetSelection (Point point, double precision, bool inMotion = false)
		{
			// Always select the playhead, regardless of where it's clicked.
			return new Selection (this, SelectionPosition.All, 0);
		}

		public void Move (Selection s, Point p, Point start)
		{
			if (s.Position == SelectionPosition.All) {
				X = p.X.Clamp (0, MaxPointX);
			}
		}
	}
}
