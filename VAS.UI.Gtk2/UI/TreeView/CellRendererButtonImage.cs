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
using Gtk;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Handlers;
using VAS.Core.Interfaces.Drawing;
using VAS.Drawing.Cairo;
using Image = VAS.Core.Common.Image;

namespace VAS.UI.Component
{
	public class CellRendererButtonImage : CellRendererToggle
	{
		int BUTTON_WIDTH = App.Current.Style.ButtonNormalWidth;
		int BUTTON_HEIGHT = App.Current.Style.ButtonNormalHeight;
		const int SPACING = 5;
		const int LINE_WIDTH = 1;
		static double offsetX, offsetY = 0;
		static int cellWidth, cellHeight = 0;
		static Point cursor;
		public event ClickedHandler Clicked;
		bool buttonPrelighted = false;

		public CellRendererButtonImage (Image icon)
		{
			Icon = icon;
			cursor = new Point (0, 0);
		}

		public override void Dispose ()
		{
			Dispose (true);
			base.Dispose ();
		}

		protected virtual void Dispose (bool disposing)
		{
			if (Disposed) {
				return;
			}
			if (disposing) {
				Destroy ();
			}
			Disposed = true;
		}

		protected override void OnDestroyed ()
		{
			Log.Verbose ($"Destroying {GetType ()}");
			Icon.Dispose ();
			base.OnDestroyed ();

			Disposed = true;
		}

		protected bool Disposed { get; private set; } = false;

		/// <summary>
		/// Gets or sets the icon.
		/// </summary>
		/// <value>The icon.</value>
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

		public override void GetSize (Widget widget, ref Gdk.Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			x_offset = 0;
			y_offset = 0;

			width = BUTTON_WIDTH;
			height = BUTTON_HEIGHT;
		}

		/// <summary>
		/// Returns the Area that should redraw based on X, Y positions
		/// </summary>
		/// <returns>The area to be redrawn, or null otherwise</returns>
		/// <param name="cellX">Cell x.</param>
		/// <param name="cellY">Cell y.</param>
		/// <param name="TotalY">Total y.</param>
		public Area ShouldRedraw (double cellX, double cellY, double TotalY, double column0_width)
		{
			Point drawingImagePoint = null;

			cursor.X = column0_width + cellX;
			cursor.Y = TotalY;
			double startX = offsetX;
			int tx = cellWidth - StyleConf.FilterTreeViewOnlyRightOffset;
			double startY = offsetY;
			double margin = cellY - startY;
			if (startX < cellX && offsetX + tx > cellX &&
				startY < cellY && startY + cellHeight > cellY) {
				buttonPrelighted = true;
				drawingImagePoint = new Point (startX + column0_width, TotalY - margin);
			} else if (buttonPrelighted) {
				buttonPrelighted = false;
				drawingImagePoint = new Point (startX + column0_width, TotalY - margin);
			}

			return drawingImagePoint == null ? null : new Area (drawingImagePoint, cellWidth, cellHeight * 2);
		}

		protected override void Render (Gdk.Drawable window, Widget widget, Gdk.Rectangle backgroundArea,
										Gdk.Rectangle cellArea, Gdk.Rectangle exposeArea, CellRendererState flags)
		{
			IDrawingToolkit tk = App.Current.DrawingToolkit;

			using (IContext context = new CairoContext (window)) {
				cellWidth = cellArea.Width;
				cellHeight = cellArea.Height;

				Point pos = new Point (cellArea.X + backgroundArea.Width / 2 - BUTTON_WIDTH / 2,
									   cellArea.Y + StyleConf.FilterTreeViewOnlyTopOffset);

				Point imagePos = new Point (pos.X + SPACING, pos.Y + StyleConf.FilterTreeViewOnlyTopOffset);

				//Get the offset to properly calculate if needs tooltip or redraw
				offsetX = pos.X - backgroundArea.X;
				offsetY = pos.Y - backgroundArea.Y;

				tk.Context = context;
				tk.Begin ();
				tk.StrokeColor = Color.Black;
				tk.FillColor = Color.Transparent;
				if (flags.HasFlag (CellRendererState.Prelit) && cursor.IsInsideArea (pos, BUTTON_WIDTH, BUTTON_HEIGHT)) {
					tk.StrokeColor = Color.Orange;
					tk.LineWidth = LINE_WIDTH;
				}

				tk.DrawRectangle (pos, BUTTON_WIDTH, BUTTON_HEIGHT);
				tk.DrawImage (imagePos, BUTTON_WIDTH - (SPACING * 2), BUTTON_HEIGHT - (StyleConf.FilterTreeViewOnlyTopOffset * 2), Icon, ScaleMode.AspectFit);
				tk.End ();
				tk.Context = null;
			}
		}
	}
}

