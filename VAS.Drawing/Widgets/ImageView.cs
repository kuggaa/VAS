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
using System;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;

namespace VAS.Drawing.Widgets
{
	public class ImageView : Canvas
	{
		public Image Image { get; set; }
		public bool IsSensitive { get; set; }
		public Color MaskColor { get; set; }

		public ImageView ()
		{

		}

		public ImageView (IWidget widget) : base (widget) { }

		public override void Draw (IContext context, Area area)
		{
			App.Current.DrawingToolkit.Context = context;
			var alpha = IsSensitive ? 1f : 0.4f;
			App.Current.DrawingToolkit.FillColor = MaskColor;
			App.Current.DrawingToolkit.DrawImage (new Point (0, 0), widget.Width, widget.Height, Image,
												  ScaleMode.AspectFit, MaskColor != null, alpha);
		}
	}
}
