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
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;

namespace VAS.Drawing.CanvasObjects.Blackboard
{
	/// <summary>
	/// Canvas object to draw a single image in the whole drawing area.
	/// </summary>
	public class ImageCanvasObject : CanvasObject
	{
		/// <summary>
		/// Gets or sets the image.
		/// </summary>
		/// <value>The image.</value>
		public Image Image { get; set; }

		/// <summary>
		/// Gets or sets the scale mode.
		/// </summary>
		/// <value>The scale mode.</value>
		public ScaleMode Mode { get; set; }

		/// <summary>
		/// Draws Image in the specified area.
		/// </summary>
		/// <param name="tk">Drawing toolkit.</param>
		/// <param name="area">Area.</param>
		public override void Draw (IDrawingToolkit tk, Area area)
		{
			tk.Begin ();
			tk.DrawImage (area.TopLeft, area.Width, area.Height, Image, Mode);
			tk.End ();
		}
	}
}
