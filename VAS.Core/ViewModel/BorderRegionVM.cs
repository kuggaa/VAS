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
using VAS.Core.Common;
using VAS.Core.MVVMC;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// View model of the border region
	/// </summary>
	public class BorderRegionVM : ViewModelBase
	{
		/// <summary>
		/// Gets or sets a value indicating whether the top border line is visible.
		/// </summary>
		/// <value><c>true</c> if show top line; otherwise, <c>false</c>.</value>
		public bool ShowTop { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the bottom border line is visible.
		/// </summary>
		/// <value><c>true</c> if show bottom line; otherwise, <c>false</c>.</value>
		public bool ShowBottom { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the left border line is visible.
		/// </summary>
		/// <value><c>true</c> if show left line; otherwise, <c>false</c>.</value>
		public bool ShowLeft { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the right border line is visible.
		/// </summary>
		/// <value><c>true</c> if show right line; otherwise, <c>false</c>.</value>
		public bool ShowRight { get; set; }

		/// <summary>
		/// Gets or sets the padding left value.
		/// </summary>
		/// <value>The padding left value.</value>
		public int PaddingLeft { get; set; }

		/// <summary>
		/// Gets or sets the padding right value.
		/// </summary>
		/// <value>The padding right value.</value>
		public int PaddingRigth { get; set; }

		/// <summary>
		/// Gets or sets the padding top value.
		/// </summary>
		/// <value>The padding top value.</value>
		public int PaddingTop { get; set; }

		/// <summary>
		/// Gets or sets the padding bottom value.
		/// </summary>
		/// <value>The padding bottom value.</value>
		public int PaddingBottom { get; set; }

		/// <summary>
		/// Gets or sets the width of the line.
		/// </summary>
		/// <value>The width of the line.</value>
		public int LineWidth { get; set; }

		/// <summary>
		/// Gets or sets background color.
		/// </summary>
		/// <value>The background color.</value>
		public Color Background { get; set; }

		/// <summary>
		/// Gets or sets the width.
		/// </summary>
		/// <value>The width.</value>
		public int Width { get; set; }

		/// <summary>
		/// Gets or sets the height.
		/// </summary>
		/// <value>The height.</value>
		public int Height { get; set; }
	}
}
