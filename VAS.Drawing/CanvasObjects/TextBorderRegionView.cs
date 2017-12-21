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
using VAS.Core.Interfaces.Drawing;
using VAS.Core.ViewModel;

namespace VAS.Drawing.CanvasObjects
{
	/// <summary>
	/// View where a text with a border is displayed
	/// </summary>
	public class TextBorderRegionView : BorderRegionView, ICanvasObjectView<TextBorderRegionVM>
	{
		TextBorderRegionVM vm;

		public new TextBorderRegionVM ViewModel { 
			get { return vm; }
			set { 
				vm = value;
				base.ViewModel = vm.BorderVM;
			}
		}

		public new void SetViewModel (object viewModel)
		{
			ViewModel = (TextBorderRegionVM)viewModel;
		}

		/// <summary>
		/// Draws the borders of a region and a text inside the region following the passed viewmodel
		/// </summary>
		/// <param name="tk">Drawing Toolkit.</param>
		/// <param name="area">Area.</param>
		public override void Draw (IDrawingToolkit tk, Area area)
		{
			base.Draw (tk, area);

			tk.Begin ();

			Point p = new Point (area.Start.X, area.Start.Y);

			tk.FontSize = 12;
			tk.StrokeColor = App.Current.Style.TextBase;
			tk.FontAlignment = FontAlignment.Center;
			tk.DrawText (p, area.Width, area.Height, ViewModel.Text);

			tk.End ();
		}
	}
}
