//
//  Copyright (C) 2017 Fluendo S.A.
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
using Gtk;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Drawing.Cairo;
using Image = VAS.Core.Common.Image;
using Point = VAS.Core.Common.Point;

namespace VAS.UI.Component
{
	public class CellRendererImage : CellRenderer
	{
		public CellRendererImage ()
		{
		}

		public CellRendererImage (Image image)
		{
			Image = image;
		}

		[GLib.Property ("Image")]
		public Image Image {
			get;
			set;
		}

		public override void GetSize (Widget widget, ref Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			x_offset = 0;
			y_offset = 0;
			width = Image.Width;
			height = Image.Width;
		}

		protected override void Render (Drawable window, Widget widget, Rectangle backgroundArea,
										Rectangle cellArea, Rectangle exposeArea, CellRendererState flags)
		{
			IDrawingToolkit tk = App.Current.DrawingToolkit;

			using (IContext context = new CairoContext (window)) {
				Point pos = new Point (cellArea.X, cellArea.Y);
				tk.Context = context;
				tk.Begin ();
				tk.DrawImage (pos, cellArea.Width, cellArea.Height, Image, ScaleMode.AspectFit);
				tk.End ();
				tk.Context = null;
			}
		}
	}


}
