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

		public PercentCircularChartVM (SeriesVM serie)
		{
			BackgroundColor = App.Current.Style.ChartBase;
			PercentSerie = serie;
			Series.ViewModels.AddRange (new List<SeriesVM> { PercentSerie });
		}

		/// <summary>
		/// Gets or sets the total elements.
		/// </summary>
		/// <value>The total.</value>
		public int TotalElements {
			get => totalElements;
			set {
				totalElements = value;
				UpdatePercentValue ();
			}
		}

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
		/// Gets or sets the color of the background.
		/// </summary>
		/// <value>The color of the background.</value>
		public Color BackgroundColor { get; set; }

		protected override void ForwardPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (NeedsSync (e.PropertyName, nameof (PercentSerie.Elements), sender, PercentSerie)) {
				UpdatePercentValue ();
			}

			base.ForwardPropertyChanged (sender, e);
		}

		void UpdatePercentValue ()
		{
			PercentValue = (TotalElements != 0) ? PercentSerie.Elements * 100.0 / TotalElements : 0;
		}
	}
}
