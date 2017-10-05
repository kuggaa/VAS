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
using System;
using System.Linq;
using Gdk;
using Gtk;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.ViewModel;
using VAS.Drawing.Cairo;
using Color = VAS.Core.Common.Color;
using Point = VAS.Core.Common.Point;

namespace VAS.UI.Component
{
	public class EventCellRenderer : CellRenderer, IView<IViewModel>
	{
		protected const int COUNT_RECTANGLE_WIDTH = 35;
		protected const int COUNT_RECTANGLE_HEIGHT = 25;
		protected const int COLOR_RECTANGLE_WIDTH = 5;
		protected const int VERTICAL_OFFSET = 5;
		protected const int SPACING = 5;
		protected const int RIGTH_OFFSET = 5;
		protected const int LEFT_OFFSET = 5;
		protected const double ACTION_IMAGE_WIDTH = 25;
		protected const double ACTION_IMAGE_HEIGHT = 25;
		protected const int LOCATION_IMAGE_WIDTH = 16;
		protected const int LOCATION_IMAGE_HEIGHT = 16;
		protected const int DRAWINGS_IMAGE_WIDTH = 16;
		protected const int DRAWINGS_IMAGE_HEIGHT = 16;
		protected const int EYE_IMAGE_WIDTH = 32;
		protected const int EYE_IMAGE_HEIGHT = 16;
		const double MINIATURE_WIDTH = 32;
		const double MINIATURE_HEIGHT = 32;

		protected static Point cursor;
		protected static double offsetX, offsetY = 0;
		static bool playButtonPrelighted = false;

		static ISurface PlayIcon;
		static ISurface BtnNormalBackground;
		static ISurface BtnNormalBackgroundPrelight;
		static ISurface BtnNormalBackgroundActive;
		static ISurface BtnNormalBackgroundInsensitive;
		static ISurface Eye;
		static ISurface Drawings;
		static ISurface PrelightDrawings;
		static ISurface Location;
		static ISurface PrelightLocation;

		static EventCellRenderer ()
		{
			PlayIcon = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.PlayButton, false);
			BtnNormalBackground = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.NormalButtonNormalTheme, false);
			BtnNormalBackgroundPrelight = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.NormalButtonPrelightTheme, false);
			BtnNormalBackgroundActive = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.NormalButtonActiveTheme, false);
			BtnNormalBackgroundInsensitive = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.NormalButtonInsensititveTheme, false);
			Drawings = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.NormalDrawings, false);
			PrelightDrawings = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.PrelightDrawings, false);
			Location = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.NormalLocation, false);
			PrelightLocation = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.PrelightLocation, false);
			Eye = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.NormalEye, false);
		}

		public EventCellRenderer ()
		{
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
			ViewModel = null;
			base.OnDestroyed ();

			Disposed = true;
		}

		protected bool Disposed { get; private set; } = false;

		public IViewModel ViewModel {
			get;
			set;
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (IViewModel)viewModel;
		}

		/// <summary>
		/// Returns the Area that should redraw based on X, Y positions
		/// </summary>
		/// <returns>The area to be redrawn, or null otherwise</returns>
		/// <param name="cellX">Cell x.</param>
		/// <param name="cellY">Cell y.</param>
		/// <param name="TotalY">Total y.</param>
		/// <param name="width">Width.</param>
		/// <param name="viewModel">View model.</param>
		public static Area ShouldRedraw (double cellX, double cellY, double TotalY, int width, IViewModel viewModel)
		{
			Point drawingImagePoint = null;
			cursor.X = cellX;
			cursor.Y = TotalY;
			double startY = VERTICAL_OFFSET + offsetY;
			double startX = width - offsetX - RIGTH_OFFSET - App.Current.Style.ButtonNormalWidth;
			double margin = cellY - startY;
			//Just to know if its inside PlayButton
			if (cellY > startY && cellY < startY + App.Current.Style.ButtonNormalHeight &&
			    cellX > startX && cellX < startX + App.Current.Style.ButtonNormalWidth) {

				drawingImagePoint = new Point (startX, TotalY - margin);
				playButtonPrelighted = true;
			} else if (playButtonPrelighted) {
				playButtonPrelighted = false;
				drawingImagePoint = new Point (startX, TotalY - margin);
			}
			if (drawingImagePoint == null) {
				return null;
			}
			return new Area (drawingImagePoint, App.Current.Style.ButtonNormalWidth, App.Current.Style.ButtonNormalHeight);
		}

		public static bool ClickedPlayButton (double cellX, double cellY, int width)
		{
			double startY = VERTICAL_OFFSET + offsetY;
			double startX = width - offsetX - RIGTH_OFFSET - App.Current.Style.ButtonNormalWidth;
			if (cellY > startY && cellY < startY + App.Current.Style.ButtonNormalHeight &&
			    cellX > startX && cellX < startX + App.Current.Style.ButtonNormalWidth) {
				return true;
			}
			return false;
		}

		public override void GetSize (Widget widget, ref Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			x_offset = 0;
			y_offset = 0;
			width = 0;
			height = 0;
			if (ViewModel != null) {
				if (ViewModel is PlaylistVM) {
					height = COUNT_RECTANGLE_HEIGHT + 2 * VERTICAL_OFFSET;
				} else if (ViewModel is EventTypeTimelineVM) {
					height = App.Current.Style.ButtonNormalHeight + 2 * VERTICAL_OFFSET;
				}
			}
		}

		protected override void Render (Drawable window, Widget widget, Rectangle backgroundArea, Rectangle cellArea, Rectangle exposeArea, CellRendererState flags)
		{
			CellState state = (CellState)flags;

			using (IContext context = new CairoContext (window)) {
				Area bkg = new Area (new Point (backgroundArea.X, backgroundArea.Y),
							   backgroundArea.Width, backgroundArea.Height);
				Area cell = new Area (new Point (cellArea.X, cellArea.Y),
								cellArea.Width, cellArea.Height);

				//Get the offset to properly calulate if needs tooltip or redraw
				offsetX = bkg.Right - cell.Right;
				offsetY = cell.Top - bkg.Top;

				if (ViewModel is EventTypeTimelineVM) {
					var vm = (EventTypeTimelineVM)ViewModel;
					RenderType (vm.EventTypeVM.Name, vm.VisibleChildrenCount, vm.EventTypeVM.Color, App.Current.DrawingToolkit, context, bkg, cell, state);
					RenderPlayButton (App.Current.DrawingToolkit, cell, vm.VisibleChildrenCount == 0, state);
				} else if (ViewModel is PlaylistVM) {
					var vm = (PlaylistVM)ViewModel;
					RenderType (vm.Name, vm.Count (), App.Current.Style.PaletteText, App.Current.DrawingToolkit, context, bkg, cell, state);
					RenderSelection (App.Current.DrawingToolkit, context, bkg, cell, state, false);
				} else if (ViewModel is PlaylistImageVM || ViewModel is PlaylistVideoVM) {
					RenderPlaylistElement ((PlaylistElementVM)ViewModel, App.Current.DrawingToolkit, context, bkg, cell, state);
				}
			}
		}

		protected void RenderSelection (IDrawingToolkit tk, IContext context,
							  Area backgroundArea, Area cellArea, CellState state, bool isChildElement)
		{
			int selectionLineWidth = 1;
			Point pos = new Point (backgroundArea.Left, backgroundArea.Start.Y + selectionLineWidth);
			if (isChildElement) {
				pos.X += LEFT_OFFSET + COLOR_RECTANGLE_WIDTH;
			}
			double width = backgroundArea.Width - pos.X;
			double height = backgroundArea.Height - selectionLineWidth;

			tk.Context = context;
			tk.Begin ();
			tk.FillColor = Color.Transparent;
			tk.StrokeColor = Color.Transparent;
			if (state.HasFlag (CellState.Selected)) {
				tk.StrokeColor = Color.Orange;
			}
			tk.LineWidth = selectionLineWidth;
			tk.DrawRectangle (pos, width, height);
			tk.StrokeColor = App.Current.Style.PaletteText;
		}

		void RenderType (string name, int childsCount, Color color, IDrawingToolkit tk, IContext context,
							  Area backgroundArea, Area cellArea, CellState state)
		{
			Point textP = new Point (StyleConf.ListTextOffset, backgroundArea.Start.Y);
			tk.Context = context;
			tk.Begin ();
			RenderBackgroundAndTitleText (tk, backgroundArea, textP, cellArea.Width - textP.X, name, App.Current.Style.PaletteBackground, color);
			RenderCount (childsCount, tk, backgroundArea, cellArea);
			RenderSeparationLine (tk, context, backgroundArea);
			tk.End ();
		}

		protected void RenderBackground (IDrawingToolkit tk, Area backgroundArea, Color backgroundColor)
		{
			/* Background */
			tk.LineWidth = 0;
			tk.FillColor = backgroundColor;
			tk.DrawRectangle (backgroundArea.Start, backgroundArea.Width, backgroundArea.Height);
		}

		protected void RenderChildLongText (IDrawingToolkit tk, Area backgroundArea, Point textP, double textW, string text, Color textColor)
		{
			/* Text */
			tk.StrokeColor = textColor;
			tk.FontSize = 11;
			tk.FontWeight = FontWeight.Light;
			tk.FontAlignment = FontAlignment.Left;
			tk.DrawText (textP, textW, backgroundArea.Height, text, false, true);
		}

		protected void RenderChildText (IDrawingToolkit tk, Point p, int width, int height, string text, Color textColor)
		{
			tk.StrokeColor = textColor;
			tk.FontSize = 12;
			tk.FontWeight = FontWeight.Normal;
			tk.FontAlignment = FontAlignment.Left;
			tk.DrawText (p, width, height, text, false, true);
		}

		void RenderBackgroundAndTitleText (IDrawingToolkit tk, Area backgroundArea, Point textP, double textW, string text, Color backgroundColor, Color textColor)
		{
			/* Background */
			RenderBackground (tk, backgroundArea, backgroundColor);

			/* Text */
			tk.StrokeColor = textColor;
			tk.FontSize = StyleConf.ListTextFontSize;
			tk.FontWeight = FontWeight.Bold;
			tk.FontAlignment = FontAlignment.Left;
			tk.DrawText (textP, textW, backgroundArea.Height, text, false, true);
		}

		void RenderCount (int count, IDrawingToolkit tk, Area backgroundArea, Area cellArea)
		{
			double posX, posY;

			posX = cellArea.Start.X + StyleConf.ListRowSeparator;
			posY = cellArea.Start.Y + VERTICAL_OFFSET;

			tk.LineWidth = 0;
			tk.FillColor = App.Current.Style.PaletteBackgroundDark;
			tk.DrawRectangle (new Point (posX, posY), COUNT_RECTANGLE_WIDTH, COUNT_RECTANGLE_HEIGHT);
			tk.StrokeColor = App.Current.Style.PaletteSelected;
			tk.FontAlignment = FontAlignment.Center;
			tk.FontWeight = FontWeight.Bold;
			tk.FontSize = 14;
			posX = posX + COUNT_RECTANGLE_WIDTH / 4;
			tk.DrawText (new Point (posX, posY), StyleConf.ListCountWidth,
				2 * StyleConf.ListCountRadio, count.ToString ());
		}

		void RenderSeparationLine (IDrawingToolkit tk, IContext context, Area backgroundArea)
		{
			double x1, x2, y;

			x1 = backgroundArea.Start.X;
			x2 = x1 + backgroundArea.Width;
			y = backgroundArea.Start.Y + backgroundArea.Height;
			tk.LineWidth = 1;
			tk.StrokeColor = App.Current.Style.PaletteBackgroundLight;
			tk.DrawLine (new Point (x1, y), new Point (x2, y));
		}

		void RenderPlayButton (IDrawingToolkit tk, Area cellArea, bool insensitive, CellState state)
		{
			Point p = new Point (cellArea.Right - App.Current.Style.ButtonNormalWidth - RIGTH_OFFSET,
								cellArea.Top + VERTICAL_OFFSET);
			ISurface background = BtnNormalBackground;
			if (insensitive) {
				background = BtnNormalBackgroundInsensitive;
			} else if (state.HasFlag (CellState.Prelit) && playButtonPrelighted) {
				background = BtnNormalBackgroundPrelight;
			}
			tk.DrawSurface (p, App.Current.Style.ButtonNormalWidth, App.Current.Style.ButtonNormalHeight, background, ScaleMode.AspectFit);
			tk.DrawSurface (p, App.Current.Style.IconLargeHeight, App.Current.Style.IconLargeHeight, PlayIcon, ScaleMode.AspectFit);
		}

		protected void RenderLocationIcon (IDrawingToolkit tk, Area backgroundArea, Area cellArea, bool HasLocation)
		{
			Point p = new Point (backgroundArea.Right - RIGTH_OFFSET - DRAWINGS_IMAGE_WIDTH - LOCATION_IMAGE_WIDTH, cellArea.Start.Y + VERTICAL_OFFSET);
			if (HasLocation) {
				if (cursor.IsInsideArea (p, LOCATION_IMAGE_WIDTH, LOCATION_IMAGE_HEIGHT)) {
					tk.DrawSurface (p, LOCATION_IMAGE_WIDTH, LOCATION_IMAGE_HEIGHT, PrelightLocation, ScaleMode.AspectFit);
				} else {
					tk.DrawSurface (p, LOCATION_IMAGE_WIDTH, LOCATION_IMAGE_HEIGHT, Location, ScaleMode.AspectFit);
				}
			}
		}

		protected void RenderDrawingsIcon (IDrawingToolkit tk, Area backgroundArea, Area cellArea, bool HasDrawings)
		{
			Point p = new Point (backgroundArea.Right - RIGTH_OFFSET - DRAWINGS_IMAGE_WIDTH, cellArea.Start.Y + VERTICAL_OFFSET);
			if (HasDrawings) {
				if (cursor.IsInsideArea (p, DRAWINGS_IMAGE_WIDTH, DRAWINGS_IMAGE_HEIGHT)) {
					tk.DrawSurface (p, DRAWINGS_IMAGE_WIDTH, DRAWINGS_IMAGE_HEIGHT, PrelightDrawings, ScaleMode.AspectFit);
				} else {
					tk.DrawSurface (p, DRAWINGS_IMAGE_WIDTH, DRAWINGS_IMAGE_HEIGHT, Drawings, ScaleMode.AspectFit);
				}
			}
		}

		protected void RenderEye (IDrawingToolkit tk, Area backgroundArea, Area cellArea, bool Playing)
		{
			Point p = new Point (backgroundArea.Right - RIGTH_OFFSET - EYE_IMAGE_WIDTH,
						  cellArea.Start.Y + VERTICAL_OFFSET + LOCATION_IMAGE_HEIGHT);
			if (Playing) {
				tk.DrawSurface (p, EYE_IMAGE_WIDTH, EYE_IMAGE_HEIGHT, Eye, ScaleMode.AspectFit);
			}
		}

		protected void RenderPrelit (bool selected, IDrawingToolkit tk, IContext context,
						   Area backgroundArea, Area cellArea, CellState state)
		{
			if (!state.HasFlag (CellState.Prelit) && !(selected || state.HasFlag (CellState.Selected))) {
				Point pos = new Point (backgroundArea.Start.X + LEFT_OFFSET + COLOR_RECTANGLE_WIDTH, backgroundArea.Start.Y);
				tk.FillColor = Color.BlackTransparent;
				tk.StrokeColor = Color.Transparent;
				tk.DrawRectangle (pos, backgroundArea.Width, backgroundArea.Height);
			}
		}

		protected void RenderImage (IDrawingToolkit tk, Point p, VAS.Core.Common.Image image,
								  double width, double height)
		{
			if (image != null) {
				tk.DrawImage (p, width, height, image, ScaleMode.AspectFit);
			}
		}

		protected void RenderColorStrip (IDrawingToolkit tk, Area backgroundArea, Color color)
		{
			Point p = new Point (backgroundArea.Left + LEFT_OFFSET, backgroundArea.Start.Y);
			//Draw Color strip
			tk.FillColor = color;
			tk.StrokeColor = color;
			tk.DrawRectangle (p, COLOR_RECTANGLE_WIDTH, backgroundArea.Height);
		}

		void RenderPlaylistElement (PlaylistElementVM vm, IDrawingToolkit tk, IContext context, Area backgroundArea, Area cellArea, CellState state)
		{
			tk.Context = context;
			tk.Begin ();
			Point textPoint = new Point (backgroundArea.Left + LEFT_OFFSET + (2 * SPACING) + COLOR_RECTANGLE_WIDTH +
										 MINIATURE_WIDTH + SPACING, cellArea.Start.Y);
			double textWidth = (cellArea.Right - RIGTH_OFFSET - EYE_IMAGE_WIDTH - SPACING) - textPoint.X;
			RenderBackground (tk, backgroundArea, App.Current.Style.PaletteBackgroundDark);
			RenderSelection (tk, context, backgroundArea, cellArea, state, true);
			RenderPrelit (vm.Playing, tk, context, backgroundArea, cellArea, state);
			RenderChildText (tk, textPoint, (int)textWidth, (int)cellArea.Height, vm.Description, App.Current.Style.PaletteText);
			RenderColorStrip (tk, backgroundArea, App.Current.Style.Text_Highlight);
			Point p = new Point (backgroundArea.Left + LEFT_OFFSET + COLOR_RECTANGLE_WIDTH + SPACING, cellArea.Start.Y + VERTICAL_OFFSET);
			RenderImage (tk, p, vm.Miniature, MINIATURE_WIDTH, MINIATURE_HEIGHT);
			RenderEye (tk, backgroundArea, cellArea, vm.Playing);
			tk.End ();
		}
	}
}
