//
//  Copyright (C) 2017 
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
using System.ComponentModel;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.ViewModel.Statistics;

namespace VAS.Drawing.CanvasObjects.Statistics
{
	[System.ComponentModel.ToolboxItem (true)]
	public class PercentCircularChartView : CanvasObject, ICanvasObjectView<PercentCircularChartVM>
	{
		PercentCircularChartVM viewModel;

		public PercentCircularChartVM ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.PropertyChanged -= HandlePropertyChanged;
				}
				viewModel = value;
				if (viewModel != null) {
					viewModel.PropertyChanged += HandlePropertyChanged;
					CallRedraw ();
				}
			}
		}

		/// <summary>
		/// Sets the view model.
		/// </summary>
		/// <param name="viewModel">View model.</param>
		public void SetViewModel (object viewModel)
		{
			ViewModel = (PercentCircularChartVM)viewModel;
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			if (ViewModel == null) {
				return;
			}

			tk.Begin ();

			Point center = new Point (area.Start.X + area.Width / 2.0, area.Start.Y + area.Height / 2.0);
			tk.FillColor = VAS.Core.Common.Color.Transparent;

			int circleStart = -90; // point in the top of the circle
			double radiansConversion = (Math.PI / 180.0);

			// draw main serie
			SeriesVM mainSerie = ViewModel.Series.ViewModels [0];
			double startAngle = circleStart * radiansConversion;
			double finalAngle = (360 * (viewModel.PercentValue / 100.0) + circleStart) * radiansConversion;

			if (viewModel.PercentValue > 0) {
				tk.StrokeColor = mainSerie.Color;
				tk.LineWidth = viewModel.LineWidth;
				tk.DrawArc (center, viewModel.Radius - (viewModel.LineWidth / 2.0), startAngle, finalAngle);
			} else {
				finalAngle = startAngle;
				startAngle = 360 * radiansConversion;
			}

			// draw empty serie
			SeriesVM emptySerie = ViewModel.Series.ViewModels [1];
			tk.StrokeColor = emptySerie.Color;
			tk.LineWidth = viewModel.LineWidth;
			tk.DrawArc (center, viewModel.Radius - (viewModel.LineWidth / 2.0), finalAngle, startAngle);

			// draw the percentage value in the middle of the circle
			tk.FontAlignment = FontAlignment.Center;
			tk.FontSize = 14;
			tk.StrokeColor = App.Current.Style.TextBase;
			tk.DrawText (new Point (0, 0), area.Width, area.Height, viewModel.PercentValueText);

			tk.End ();
		}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			CallRedraw ();
		}

		void CallRedraw ()
		{
			if (App.IsMainThread) {
				ReDraw ();
			} else {
				App.Current.GUIToolkit.Invoke ((sender, e) => ReDraw ());
			}
		}
	}
}
