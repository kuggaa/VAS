//
//  Copyright (C) 2016 Fluendo S.A.
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
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;
using VAS.Core.Resources.Styles;
using VAS.Core.ViewModel;

namespace VAS.Drawing.CanvasObjects.Timeline
{
	/// <summary>
	/// A label for the cameras timeline row.
	/// </summary>
	[View ("CameraLabelView")]
	public class CameraLabelView : LabelView, ICanvasObjectView<MediaFileVM>
	{
		MediaFileVM viewModel;
		string name;

		public MediaFileVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
			}
		}

		public override string Name {
			get {
				return viewModel?.Name;
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (MediaFileVM)viewModel;
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			// Draw background
			tk.Begin ();
			tk.FillColor = BackgroundColor;
			tk.StrokeColor = App.Current.Style.TextBase;
			tk.LineWidth = 1;
			tk.DrawRectangle (new Point (0, scrolledY), Width, Height);

			/* Draw category name */
			tk.FontSlant = FontSlant.Normal;
			tk.FontWeight = FontWeight.Bold;
			tk.FontSize = Sizes.TimelineCameraFontSize;
			tk.FillColor = App.Current.Style.TextBase;
			tk.FontAlignment = FontAlignment.Right;
			tk.StrokeColor = App.Current.Style.TextBase;
			tk.DrawText (new Point (0, scrolledY), Width - Sizes.TimelineLabelHSpacing, Height, Name);
			tk.End ();
		}
	}

}
