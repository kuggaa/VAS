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
namespace VAS.Core.ViewModel.Statistics
{
	/// <summary>
	/// Viewmodel for the circular series graphic
	/// </summary>
	public class CircularChartVM : ChartVM
	{
		/// <summary>
		/// Gets or sets the radius of the cicle
		/// </summary>
		/// <value>The circle radius.</value>
		public int Radius { get; set; }

		/// <summary>
		/// Gets or sets the thickness of the arc line
		/// </summary>
		/// <value>The thickness.</value>
		public int LineWidth { get; set; }
	}
}
