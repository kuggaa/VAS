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
using VAS.Core.Common;

namespace VAS.Core.ViewModel.Statistics
{
	/// <summary>
	/// View model for specific charts where a percentage is displayed in 
	/// the middle of a circle
	/// </summary>
	public sealed class PercentCircularChartVM : CircularChartVM
	{
		public PercentCircularChartVM (SeriesVM serie, int total, Color emptyChartColor = null)
		{
			Color emptyColor = emptyChartColor ?? App.Current.Style.ChartBase;
			SeriesVM emptySerie = new SeriesVM ("empty", total - serie.Elements, emptyColor);
			Series = new SeriesCollectionVM {
				ViewModels = { serie, emptySerie }
			};

			PercentValue = (total != 0) ? serie.Elements * 100.0 / total : 0;
			PercentValueText = string.Format ("{0:0.0}%", PercentValue);
		}

		/// <summary>
		/// Gets the percent text value of the serie.
		/// </summary>
		/// <value>The percent text value.</value>
		public string PercentValueText { get; private set; }

		/// <summary>
		/// Gets the percent value.
		/// </summary>
		/// <value>The percent value.</value>
		public double PercentValue { get; private set; }
	}
}
