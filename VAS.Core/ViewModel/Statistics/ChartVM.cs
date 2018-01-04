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
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;

namespace VAS.Core.ViewModel.Statistics
{
	/// <summary>
	/// Viewmodel of the common properties of a chart
	/// </summary>
	public class ChartVM : ViewModelBase
	{
		/// <summary>
		/// Gets or sets the left padding.
		/// </summary>
		/// <value>The left padding.</value>
		public int LeftPadding { get; set; }

		/// <summary>
		/// Gets or sets the right padding.
		/// </summary>
		/// <value>The right padding.</value>
		public int RightPadding { get; set; }

		/// <summary>
		/// Gets or sets the top padding.
		/// </summary>
		/// <value>The top padding.</value>
		public int TopPadding { get; set; }

		/// <summary>
		/// Gets or sets the bottom padding.
		/// </summary>
		/// <value>The bottom padding.</value>
		public int BottomPadding { get; set; }

		/// <summary>
		/// Series collection
		/// </summary>
		/// <value>The series collection.</value>
		public SeriesCollectionVM Series {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the background image.
		/// This image will be rendered under the series.
		/// </summary>
		/// <value>The background image.</value>
		public ICanvasObject Background { get; set; }
	}
}
