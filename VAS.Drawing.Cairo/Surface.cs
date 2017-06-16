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
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;

namespace VAS.Drawing.Cairo
{
	public class Surface : DisposableBase, ISurface
	{
		ImageSurface surface;
		bool warnOnDispose;

		public Surface (int width, int height, Image image, bool warnOnDispose = true)
		{
			this.warnOnDispose = warnOnDispose;
			if (image != null) {
				DeviceScaleFactor = image.DeviceScaleFactor;
			} else {
				DeviceScaleFactor = App.Current.GUIToolkit.DeviceScaleFactor;
			}
			surface = new ImageSurface (Format.ARGB32, (int)(width * DeviceScaleFactor), (int)(height * DeviceScaleFactor));
			if (image != null) {
				using (CairoContext ccontext = new CairoContext (surface)) {
					var oldContext = App.Current.DrawingToolkit.Context;
					App.Current.DrawingToolkit.Context = ccontext;
					// The image must be drawn using it's real size, since the backend ImageSurface's size is also scalled
					App.Current.DrawingToolkit.DrawImage (new Core.Common.Point (0, 0), image.Width * image.DeviceScaleFactor,
														  image.Height * image.DeviceScaleFactor, image, ScaleMode.AspectFit);
					App.Current.DrawingToolkit.Context = oldContext;
				}
			}
		}

		~Surface ()
		{
			if (!Disposed && warnOnDispose) {
				Log.Error (String.Format ("Surface {0} was not disposed correctly", this));
				Dispose (false);
			}
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			if (surface != null) {
				surface.Dispose ();
			}
			surface = null;
		}

		public object Value {
			get {
				return surface;
			}
		}

		public IContext Context {
			get {
				var ctx = new CairoContext (surface);
				(ctx.Value as global::Cairo.Context).Scale (DeviceScaleFactor, DeviceScaleFactor);
				return ctx;
			}
		}

		public int Width {
			get {
				return (int)(surface.Width / DeviceScaleFactor);
			}
		}

		public int Height {
			get {
				return (int)(surface.Height / DeviceScaleFactor);
			}
		}

		public float DeviceScaleFactor {
			get;
			protected set;
		}

		public Image Copy ()
		{
			Gdk.Colormap colormap = Gdk.Colormap.System;

			/* FIXME: copying a surface to a pixbuf through a pixmap does not require writting / reading to a file,
			 * but it does not handle transparencies correctly and draws transparent pixels in a black color, so
			 * for now we force the first method */
			colormap = null;

			/* In unit tests running without a display, the default Screen is null and we can't get a valid colormap.
			 * In this scenario we use a fallback that writes the surface to a temporary file */
			//if (colormap == null) {
			string tempFile = System.IO.Path.GetTempFileName ();
			surface.WriteToPng (tempFile);
			return new Image (tempFile);
			/*} else {
				Gdk.Pixmap pixmap = new Gdk.Pixmap (null, Width, Height, 24);
				using (Context cr = Gdk.CairoHelper.Create (pixmap)) {
					cr.Operator = Operator.Source;
					cr.SetSource (surface);
					cr.Paint ();
				}
				return new Image (Gdk.Pixbuf.FromDrawable (pixmap, Gdk.Colormap.System, 0, 0, 0, 0, Width, Height));*/
		}
	}
}

