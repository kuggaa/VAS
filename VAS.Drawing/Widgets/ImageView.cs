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
using VAS.Core.Resources.Styles;

namespace VAS.Drawing.Widgets
{
	/// <summary>
	/// A view that draws an Image.
	/// </summary>
	public class ImageView : Canvas
	{
		Image image;

		public ImageView () { }

		public ImageView (IWidget widget) : base (widget) { }

		/// <summary>
		/// Gets or sets the image.
		/// </summary>
		/// <value>The image.</value>
		public Image Image {
			get {
				return image;
			}
			set {
				image = value;
				widget.ReDraw ();
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Drawing.Widgets.ImageView"/> is sensitive.
		/// </summary>
		/// <value><c>true</c> if is sensitive; otherwise, <c>false</c>.</value>
		public bool IsSensitive { get; set; } = true;

		/// <summary>
		/// Gets or sets the color of the mask, image will be filled with this color.
		/// </summary>
		/// <value>The color of the mask.</value>
		public Color MaskColor { get; set; }

		/// <summary>
		/// Gets or sets the scale mode used to draw the image.
		/// </summary>
		/// <value>The scale mode.</value>
		public ScaleMode ScaleMode { get; set; } = ScaleMode.AspectFit;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.UI.Helpers.ImageView"/> is rendered in a circular shape.
		/// </summary>
		/// <value><c>true</c> if circular; otherwise, <c>false</c>.</value>
		public bool Circular { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.UI.Helpers.ImageView"/> has shadow.
		/// </summary>
		/// <value><c>true</c> if has shadow; otherwise, <c>false</c>.</value>
		public bool HasShadow { get; set; }

		/// <summary>
		/// Gets or sets the color of the shadow.
		/// </summary>
		/// <value>The color of the shadow.</value>
		public Color ShadowColor { get; set; }

		protected Point Center { get; set; }

		protected override void HandleSizeChangedEvent ()
		{
			base.HandleSizeChangedEvent ();
			Center = new Point (widget.Width / 2, widget.Height / 2);
		}

		public override void Draw (IContext context, Area area)
		{
			if (Image == null) {
				return;
			}
			var alpha = IsSensitive ? Colors.AlphaImageSensitive : Colors.AlphaImageNoSensitive;

			Begin (context);
			tk.FillColor = MaskColor;

			int imageWidth, imageHeight;
			Image.ComputeScale (Image.Width, Image.Height, (int)widget.Width, (int)widget.Height, ScaleMode,
								out imageWidth, out imageHeight);

			if (Circular) {
				double radius = (Math.Min (imageWidth, imageHeight) / 2) - 1;
				// draw avatar
				if (HasShadow) {
					Color shadowColor = App.Current.Style.ColorPrimary;
					shadowColor.SetAlpha (0.8f);
					App.Current.DrawingToolkit.FillColor = shadowColor;
					App.Current.DrawingToolkit.LineWidth = 0;
					App.Current.DrawingToolkit.DrawCircle (new Point (Center.X + 1, Center.Y + 1), radius);
				}
				App.Current.DrawingToolkit.ClipCircle (Center, radius);
			}
			tk.DrawImage (new Point (0, 0), widget.Width, widget.Height, Image, ScaleMode, MaskColor != null, alpha);
			End ();
		}
	}
}
