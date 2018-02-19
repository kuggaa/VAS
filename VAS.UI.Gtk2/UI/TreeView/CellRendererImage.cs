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
using Gdk;
using Gtk;
using VAS.Core.Interfaces.Drawing;
using VAS.Drawing.Cairo;
using VAS.Drawing.Widgets;
using Color = VAS.Core.Common.Color;
using Image = VAS.Core.Common.Image;
using Point = VAS.Core.Common.Point;

namespace VAS.UI.Component
{
	public class CellRendererImage : CellRenderer
	{
		ImageView imageView;

		public CellRendererImage ()
		{
			imageView = new ImageView ();
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

		[GLib.Property ("MaskColor")]
		public Color MaskColor { get; set; }

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

			imageView.Image = Image;
			imageView.MaskColor = MaskColor;
			imageView.SetWidget (new NoWindowWidget { Width = cellArea.Width, Height = cellArea.Height });

			using (IContext context = new CairoContext (window)) {
				tk.Context = context;
				tk.TranslateAndScale (new Point (cellArea.X, cellArea.Y), new Point (1, 1));
				imageView.Draw (context, null);
			}
		}
	}
}
