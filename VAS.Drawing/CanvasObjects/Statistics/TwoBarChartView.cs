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
using System;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.ViewModel.Statistics;

namespace VAS.Drawing.CanvasObjects.Statistics
{
	/// <summary>
	/// View of a chart with two horizontal and consecutive series
	/// </summary>
	public class TwoBarChartView : BarChartView, ICanvasObjectView<TwoBarChartVM>
	{
		TwoBarChartVM viewModel;

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			ViewModel.Dispose ();
			ViewModel = null;
		}

		/// <summary>
		/// Gets or sets the view model.
		/// </summary>
		/// <value>The view model.</value>
		public TwoBarChartVM ViewModel {
			get {
				return viewModel;
			}

			set {
				base.ViewModel = value;
				viewModel = value;
				if (App.IsMainThread) {
					ReDraw ();
				} else {
					App.Current.GUIToolkit.Invoke ((sender, e) => ReDraw ());
				}
			}
		}

		/// <summary>
		/// Sets the view model.
		/// </summary>
		/// <param name="viewmodel">Viewmodel.</param>
		public new void SetViewModel (object viewmodel)
		{
			ViewModel = (TwoBarChartVM)viewmodel;
		}

		/// <summary>
		/// Draws a chart with two horizontal and consecutive series
		/// </summary>
		/// <param name="tk">Drawing Toolkit.</param>
		/// <param name="area">Area.</param>
		public override void Draw (IDrawingToolkit tk, Area area)
		{
			if (ViewModel == null || Math.Abs (ViewModel.TotalNumber) < 0.1f) {
				return;
			}

			base.Draw (tk, area);

			tk.Begin ();
			tk.FontSize = 12;

			double posX = area.Start.X + viewModel.LeftPadding;
			double posY = area.Start.Y + ViewModel.TopPadding;
			double width = area.Width - ViewModel.LeftPadding - ViewModel.RightPadding;
			double height = area.Height - ViewModel.TopPadding - ViewModel.BottomPadding;

			// text serie left
			tk.StrokeColor = App.Current.Style.TextColor;
			tk.FontAlignment = FontAlignment.Left;
			tk.DrawText (new Point (posX, posY + 5), width, height, ViewModel.LeftDisplayText);

			// text serie right
			if (ViewModel.RightTextStyle != TwoBarTextStyle.Hidden) {
				tk.StrokeColor = App.Current.Style.TextColor;
				tk.FontAlignment = FontAlignment.Right;
				tk.DrawText (new Point (posX, posY + 5), width, height, ViewModel.RightSerie.Elements.ToString ());
			}

			tk.End ();
		}
	}
}
