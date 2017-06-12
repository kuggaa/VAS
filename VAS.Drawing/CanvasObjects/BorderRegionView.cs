//
//  Copyright (C) 2017 FLUENDO S.A.
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
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.ViewModel;

namespace VAS.Drawing.CanvasObjects
{
	/// <summary>
	/// View that displays a border and that is used to decorate other views
	/// </summary>
	public class BorderRegionView : CanvasObject, ICanvasObjectView<BorderRegionVM>
	{
		public BorderRegionVM ViewModel { get; set; }

		/// <summary>
		/// Sets the view model.
		/// </summary>
		/// <param name="viewModel">View model.</param>
		public void SetViewModel (object viewModel)
		{
			ViewModel = (BorderRegionVM)viewModel;
		}

		/// <summary>
		/// Draws the borders of a region following the passed viewmodel
		/// </summary>
		/// <param name="tk">Drawing Toolkit.</param>
		/// <param name="area">Area.</param>
		public override void Draw (IDrawingToolkit tk, Area area)
		{
			tk.Begin ();
			tk.LineWidth = ViewModel.LineWidth;
			tk.StrokeColor = App.Current.Style.BackgroundLevel2;

			int extraLeft = (ViewModel.ShowLeft) ? ViewModel.PaddingLeft : 0;
			double xOrigin = area.Start.X + extraLeft;
			double yOrigin = area.Start.Y + 1;

			double width = area.Width;
			width -= (ViewModel.ShowLeft) ? ViewModel.PaddingLeft : 0;
			width -= (ViewModel.ShowRight) ? ViewModel.PaddingRigth : 0;
			double height = area.Height - (ViewModel.LineWidth * 2) - 5;

			Point start = new Point (xOrigin, yOrigin);
			Point stop = new Point (xOrigin + width, yOrigin);

			if (ViewModel.ShowTop) {
				tk.DrawLine (start, stop);
			}

			if (ViewModel.ShowBottom) {
				start = new Point (xOrigin, yOrigin + height);
				stop = new Point (xOrigin + width, yOrigin + height);
				tk.DrawLine (start, stop);
			}

			if (ViewModel.ShowLeft) {
				start = new Point (xOrigin, yOrigin);
				stop = new Point (xOrigin, yOrigin + height);
				tk.DrawLine (start, stop);
			}

			if (ViewModel.ShowRight) {
				start = new Point (xOrigin + width - ViewModel.LineWidth + 1, yOrigin);
				stop = new Point (xOrigin + width - ViewModel.LineWidth + 1, yOrigin + height);
				tk.DrawLine (start, stop);
			}

			tk.FillColor = ViewModel.Background;
			tk.LineWidth = 0;
			extraLeft = ViewModel.ShowLeft ? ViewModel.LineWidth : 0;
			int extraRight = ViewModel.ShowRight ? ViewModel.LineWidth + 1 : 0;
			start = new Point (xOrigin + extraLeft, yOrigin + ViewModel.LineWidth);
			tk.DrawRectangle (start, width - extraRight, height - ViewModel.LineWidth * 2);

			tk.End ();
		}
	}
}
