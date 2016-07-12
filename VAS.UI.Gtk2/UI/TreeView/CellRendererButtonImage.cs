//  Copyright (C) 2016 Fluendo S.A.
using System;
using Gdk;
using GLib;
using Gtk;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Handlers;
using VAS.Core.Interfaces.Drawing;
using VAS.Drawing.Cairo;
using Image = VAS.Core.Common.Image;
using Point = VAS.Core.Common.Point;

namespace VAS.UI.Component
{
	public class CellRendererButtonImage : CellRendererToggle
	{
		const int BUTTON_HEIGHT = 30;
		const int BUTTON_WIDTH = 60;

		public event ClickedHandler Clicked;

		public CellRendererButtonImage (Image icon)
		{
			Icon = icon;
		}

		public Image Icon {
			get;
			set;
		}

		protected override void OnToggled (string path)
		{
			if (Clicked != null) {
				Clicked (this, new ClickedArgs (path));
			}
		}

		public override void GetSize (Widget widget, ref Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			x_offset = 0;
			y_offset = 0;

			width = BUTTON_WIDTH;
			height = BUTTON_HEIGHT;
		}

		protected override void Render (Drawable window, Widget widget, Rectangle backgroundArea,
		                                Rectangle cellArea, Rectangle exposeArea, CellRendererState flags)
		{
			IDrawingToolkit tk = App.Current.DrawingToolkit;

			using (IContext context = new CairoContext (window)) {
				int width = cellArea.Width - StyleConf.FilterTreeViewOnlyRightOffset;
				int height = cellArea.Height - StyleConf.FilterTreeViewOnlyTopOffset * 2;
				Point pos = new Point (cellArea.X + backgroundArea.Width - cellArea.Width,
					            cellArea.Y + StyleConf.FilterTreeViewOnlyTopOffset);
				tk.Context = context;
				tk.Begin ();
				tk.FillColor = VAS.Core.Common.Color.Green1;
				if (flags.HasFlag (CellRendererState.Prelit)) {
					tk.FillColor = VAS.Core.Common.Color.Green;
				}

				tk.LineWidth = 1;
				tk.StrokeColor = VAS.Core.Common.Color.Green;
				tk.DrawRoundedRectangle (pos, width, height, 3);
				tk.StrokeColor = App.Current.Style.PaletteText;
				tk.DrawImage (pos, width, height, Icon, ScaleMode.AspectFit);
				tk.End ();
				tk.Context = null;
			}
		}
	}
}

