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
using System.Collections.Generic;
using System.ComponentModel;
using VAS.Core.Common;

namespace VAS.Core.ViewModel.Statistics
{
	/// <summary>
	/// View model for specific charts where a percentage is displayed in 
	/// the middle of a circle
	/// </summary>
	public sealed class PercentCircularChartVM : CircularChartVM
	{
		int totalElements;

		public PercentCircularChartVM (SeriesVM serie, Color emptyChartColor = null)
		{
			Color emptyColor = emptyChartColor ?? App.Current.Style.ChartBase;
			EmptySerie = new SeriesVM ("empty", emptyColor);
			PercentSerie = serie;
			Series.ViewModels.AddRange (new List<SeriesVM> { PercentSerie, EmptySerie });
		}

		/// <summary>
		/// Gets or sets the total elements.
		/// </summary>
		/// <value>The total.</value>
		public int TotalElements {
			get => totalElements;
			set {
				totalElements = value;
				UpdateEmptySerie ();
			}
		}

		/// <summary>
		/// Gets the percent text value of the serie.
		/// </summary>
		/// <value>The percent text value.</value>
		// public string PercentValueText { get => String.Format ("{0:0.0}%", PercentValue); }

		/// <summary>
		/// Gets the percent value.
		/// </summary>
		/// <value>The percent value.</value>
		public double PercentValue { get; set; }

		/// <summary>
		/// Gets the percent serie .
		/// </summary>
		/// <value>The serie.</value>
		public SeriesVM PercentSerie { get; private set; }

		/// <summary>
		/// Gets the empty serie.
		/// </summary>
		/// <value>The empty serie.</value>
		public SeriesVM EmptySerie { get; private set; }

		void UpdateEmptySerie () {
			EmptySerie.Elements = TotalElements - PercentSerie.Elements;
		}

		protected override void ForwardPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Elements" && sender == PercentSerie) {
				PercentValue = (TotalElements != 0) ? PercentSerie.Elements * 100.0 / TotalElements : 0;
				UpdateEmptySerie ();
			}

			base.ForwardPropertyChanged (sender, e);
		}
	}
}
