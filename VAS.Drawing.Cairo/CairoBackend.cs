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
using Gdk;
using Pango;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using FontWeight = VAS.Core.Common.FontWeight;
using Image = VAS.Core.Common.Image;

namespace VAS.Drawing.Cairo
{
	public class CairoBackend: IDrawingToolkit
	{
		Layout layout;

		public CairoBackend ()
		{
			if (layout == null) {
				layout = new Layout (PangoHelper.ContextGet ());
			}
		}

		public void Invoke (EventHandler handler)
		{
			Gtk.Application.Invoke (handler);
		}

		public void MeasureText (string text, out int width, out int height,
		                         string fontFamily, int fontSize, FontWeight fontWeight)
		{
			FontDescription desc = new FontDescription ();
			desc.Family = fontFamily;
			desc.Size = Units.FromPixels (fontSize);
			desc.Weight = fontWeight.ToPangoWeight ();
			layout.FontDescription = desc;
			layout.SetMarkup (GLib.Markup.EscapeText (text));
			layout.GetPixelSize (out width, out height);
		}

		public Image Copy (ICanvas canvas, Area area)
		{
			Image img;
			Pixmap pm;
			global::Cairo.Context ctx;

			pm = new Pixmap (null, (int)area.Width, (int)area.Height, 24);
			ctx = Gdk.CairoHelper.Create (pm);
			using (CairoContext c = new CairoContext (ctx)) {
				c.DisableScalling = true;
				ctx.Translate (-area.Start.X, -area.Start.Y);
				canvas.Draw (c, null);
			}
			img = new Image (Pixbuf.FromDrawable (pm, Colormap.System, 0, 0, 0, 0,
				(int)area.Width, (int)area.Height));
			return img;
		}

		public void Save (ICanvas canvas, Area area, string filename)
		{
			Image img = Copy (canvas, area);
			img.Save (filename);
			img.Dispose ();
		}

		public ISurface CreateSurface (string filename, bool warnOnDispose = true)
		{
			Image img = new Image (filename);
			return CreateSurface (img.Width, img.Height, img, warnOnDispose);
		}

		public ISurface CreateSurface (int width, int height, Image image = null, bool warnOnDispose = true)
		{
			return new Surface (width, height, image, warnOnDispose);
		}

	}

	public static class CairoExtensions
	{
		public static Weight ToPangoWeight (this FontWeight value)
		{
			Weight weight = Weight.Normal;

			switch (value) {
			case FontWeight.Light:
				weight = Weight.Light;
				break;
			case FontWeight.Bold:
				weight = Weight.Semibold;
				break;
			case FontWeight.Normal:
				weight = Weight.Normal;
				break;
			}
			return weight;
		}
	}
}
