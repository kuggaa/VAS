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
	/// <summary>
	/// A view to render a percentual circular chart.
	/// </summary>
	public class PercentCircularChartView : CanvasObject, ICanvasObjectView<PercentCircularChartVM>
	{
		PercentCircularChartVM viewModel;
		const int CIRCLE_START = -90; // point in the top of the circle
		const double RADIANTS_CONVERSION = Math.PI / 180.0;
		const double START_ANGLE = CIRCLE_START * RADIANTS_CONVERSION;
		double percent = 0;

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
					viewModel.Sync ();
				}
			}
		}

		double Percent {
			get => percent;
			set {
				percent = value;
				ReDraw ();
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
			tk.FillColor = Color.Transparent;

			// draw main serie
			SeriesVM mainSerie = ViewModel.PercentSerie;
			double finalAngle = (360 * (Percent / 100.0) + CIRCLE_START) * RADIANTS_CONVERSION;

			// draw empty serie
			tk.StrokeColor = ViewModel.EmptySerie.Color;
			tk.LineWidth = viewModel.LineWidth;
			tk.DrawArc (center, viewModel.Radius - (viewModel.LineWidth / 2.0), 0, 2 * Math.PI);

			if (percent > 0) {
				tk.StrokeColor = mainSerie.Color;
				tk.LineWidth = viewModel.LineWidth;
				tk.DrawArc (center, viewModel.Radius - (viewModel.LineWidth / 2.0), START_ANGLE, finalAngle);
			}

			// draw the percentage value in the middle of the circle
			tk.FontAlignment = FontAlignment.Center;
			tk.FontSize = 14;
			tk.StrokeColor = App.Current.Style.TextBase;
			tk.DrawText (new Point (area.Start.X, area.Start.Y), area.Width, area.Height,
						 string.Format ("{0:0.0}%", Percent));

			tk.End ();
		}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (viewModel.NeedsSync (e, nameof (viewModel.PercentValue))) {
				Percent = viewModel.PercentValue;
			}
		}
	}
}
