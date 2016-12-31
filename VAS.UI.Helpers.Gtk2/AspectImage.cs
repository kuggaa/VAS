//
//  Copyright (C) 2016 Fluendo S.A.
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
using VAS.Core.Common;
using VAS.Drawing.Cairo;
using Image = VAS.Core.Common.Image;
using Point = VAS.Core.Common.Point;

namespace VAS.UI.Helpers.Gtk2
{
	[System.ComponentModel.ToolboxItem (true)]
	public class AspectImage : Gtk.Widget
	{
		public AspectImage ()
		{
			this.SetHasWindow (false);
		}

		public Image Image {
			set;
			get;
		}

		protected override bool OnExposeEvent (EventExpose evnt)
		{
			using (CairoContext c = new CairoContext (GdkWindow)) {
				Cairo.Context cc = c.Value as Cairo.Context;
				Rectangle r = GdkWindow.ClipRegion.Clipbox;
				var area = new Area (new Point (r.X, r.Y), r.Width, r.Height);
				cc.Rectangle (area.Start.X, area.Start.Y, area.Width, area.Height);
				cc.Clip ();
				var tk = App.Current.DrawingToolkit;
				tk.Context = c;
				tk.DrawImage (new Point (Allocation.X, Allocation.Y), Allocation.Width, Allocation.Height, Image, ScaleMode.AspectFit);
			}
			return true;
		}

	}
}
