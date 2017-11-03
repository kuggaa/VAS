//
//  Copyright (C) 2017 Fluendo S.A.
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
using System.ComponentModel;
using VAS.Core.Common;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel.Statistics;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// ViewModel for the Bar chart used to display count limitations in the Limitation Widget
	/// </summary>
	public class CountLimitationBarChartVM : ViewModelBase
	{
		BindingContext ctx;
		CountLimitationVM limitation;
		TwoBarChartVM barChart;
		readonly int showOnRemaining;
		string onlyText, noText, leftText, shownText;

		public CountLimitationBarChartVM (int showOnRemaining = -1)
		{
			this.showOnRemaining = showOnRemaining;
			ctx = new BindingContext ();
			Visible = true;
			BackgroundColor = App.Current.Style.ThemeBase;
			onlyText = Catalog.GetString ("Only");
			noText = Catalog.GetString ("No");
			leftText = Catalog.GetString ("left in your plan!");
			shownText = Catalog.GetString ("are being shown (out of");
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			ctx.Dispose ();
			ctx = null;
			BarChart.Dispose ();
			BarChart = null;
			Limitation = null;
		}

		/// <summary>
		/// Gets or sets the limitation.
		/// </summary>
		/// <value>The limitation.</value>
		public CountLimitationVM Limitation {
			get {
				return limitation;
			}

			set {
				if (limitation != null) {
					limitation.PropertyChanged -= HandlePropertyChanged;
				}
				limitation = value;
				Visible = limitation?.Enabled ?? false;
				if (limitation != null) {
					limitation.PropertyChanged += HandlePropertyChanged;
					HandlePropertyChanged (null, null);
				}
			}
		}

		/// <summary>
		/// Gets or sets the bar chart.
		/// </summary>
		/// <value>The bar chart.</value>
		public TwoBarChartVM BarChart {
			get {
				return barChart;
			}

			set {
				if (barChart != null) {
					barChart.PropertyChanged -= HandlePropertyChanged;
				}
				barChart = value;
				if (barChart != null) {
					barChart.PropertyChanged += HandlePropertyChanged;
					HandlePropertyChanged (null, null);
				}
			}
		}

		public string Text {
			get;
			private set;
		}

		public bool Visible {
			get;
			private set;
		}

		public Color BackgroundColor {
			get;
			set;
		}

		/// <summary>
		/// Bind the series in the BarChart to the limitation properties, so that the chart updates automatically.
		/// </summary>
		public void Bind ()
		{
			ctx.Add (BarChart.LeftSerie.Bind (s => s.Elements, vm => ((CountLimitationVM)vm).Remaining));
			ctx.Add (BarChart.RightSerie.Bind (s => s.Elements, vm => ((CountLimitationVM)vm).Count));
			ctx.UpdateViewModel (Limitation);
		}

		void UpdateVisibility ()
		{
			Visible = Limitation.Enabled;
			if (Limitation.Remaining > showOnRemaining && showOnRemaining != -1) {
				Visible = false;
			}
		}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (Limitation == null || BarChart == null) {
				return;
			}
			if (NeedsSync (e?.PropertyName, nameof (Limitation.Count))) {
				UpdateVisibility ();
				BarChart.RightSerie.Color = App.Current.Style.ColorAccentError;
				if (Limitation.Remaining == 0) {
					Text = $"Oops! <b>{noText} {Limitation.DisplayName.ToLower ()}</b> {leftText}";
				} else if (Limitation.Remaining < 0) {
					Text = $"{onlyText} <b>{Limitation.Maximum} {Limitation.DisplayName.ToLower ()}</b> {shownText} " +
						$"{Limitation.Count})";
				} else {
					BarChart.RightSerie.Color = Color.Transparent;
					Text = $"{onlyText} <b>{Limitation.Remaining} {Limitation.DisplayName.ToLower ()}</b> {leftText}";
				}
			}
			if (NeedsSync (e?.PropertyName, nameof (Limitation.Enabled))) {
				UpdateVisibility ();
			}
		}
	}
}
