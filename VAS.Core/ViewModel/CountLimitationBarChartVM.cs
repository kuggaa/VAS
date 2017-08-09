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

		public CountLimitationBarChartVM ()
		{
			ctx = new BindingContext ();
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
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the bar chart.
		/// </summary>
		/// <value>The bar chart.</value>
		public TwoBarChartVM BarChart {
			get;
			set;
		}

		/// <summary>
		/// Bind the series in the BarChart to the limitation properties, so that the chart updates automatically.
		/// </summary>
		public void Bind ()
		{
			ctx.Add (BarChart.LeftSerie.Bind ((vm) => ((SeriesVM)vm).Elements, (vm) => ((CountLimitationVM)vm).Remaining));
			ctx.Add (BarChart.RightSerie.Bind ((vm) => ((SeriesVM)vm).Elements, (vm) => ((CountLimitationVM)vm).Count));
			ctx.UpdateViewModel (Limitation);
		}
	}
}
