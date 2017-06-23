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
using VAS.Core.MVVMC;
using SkiaSharp;

namespace VAS.Drawing.Skia
{
	public class SkiaSurface : DisposableBase, ISurface
	{
		internal SKBitmap bitmap;
		bool warnOnDispose;

		public SkiaSurface (int width, int height, Image image, bool warnOnDispose = true)
		{
			Init (width, height, image, warnOnDispose);
		}

		public SkiaSurface (string filename)
		{
			var image = new Image (filename);
			Init (image.Width, image.Height, image, true);
		}

		~SkiaSurface ()
		{
			if (!Disposed && warnOnDispose) {
				Log.Error (String.Format ("Surface {0} was not disposed correctly", this));
				Dispose (false);
			}
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			if (bitmap != null) {
				bitmap.Dispose ();
			}
			bitmap = null;
		}

		public object Value {
			get {
				return bitmap;
			}
		}

		public IContext Context {
			get {
				return new SkiaContext (new SKCanvas (bitmap));
			}
		}

		public float DeviceScaleFactor {
			get;
			protected set;
		} = 1;

		public int Width {
			get;
			protected set;
		}

		public int Height {
			get;
			protected set;
		}

		public Image Copy ()
		{
			return null;
		}

		void Init (int width, int height, Image image, bool warnOnDispose)
		{
			this.warnOnDispose = warnOnDispose;
			bitmap = new SKBitmap (width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
			if (image != null) {
				using (var ctx = new SkiaContext (new SKCanvas (bitmap))) {
					var oldCtx = App.Current.DrawingToolkit.Context;
					App.Current.DrawingToolkit.Context = ctx;
					App.Current.DrawingToolkit.DrawImage (image);
					App.Current.DrawingToolkit.Context = oldCtx;
				}
			}
			Width = width;
			Height = height;
		}

	}
}

