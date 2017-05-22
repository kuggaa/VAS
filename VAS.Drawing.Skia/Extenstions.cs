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
using SkiaSharp;
using VAS.Core.Common;

namespace VAS.Drawing.Skia
{
	public static class Extenstions
	{
		public static SKRect ToSKRect (this Area area)
		{
			return SKRect.Create ((float)area.TopLeft.X, (float)area.TopLeft.Y, (float)area.Width, (float)area.Height);
		}

		public static Area ToArea (this SKRect rect)
		{
			return new Area (rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public static SKPoint ToSKPoint (this Point point)
		{
			return new SKPoint ((float)point.X, (float)point.Y);
		}

		public static Color ToColor (this SKColor color)
		{
			return new Color (color.Red, color.Green, color.Blue, color.Alpha);
		}

		public static SKColor ToSKColor (this Color color)
		{
			// FIXME: This is very weird, Blue and Red need to be inverted :/
			return new SKColor (color.B, color.G, color.R, color.A);
		}

		public static SKImage ToSKImage (this Image image)
		{
			SKImageInfo info = new SKImageInfo (image.Width, image.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
			return SKImage.FromPixels (info, image.LockPixels ());
		}

		public static SKFontStyleSlant ToSKFontStyleLant (this FontSlant slant)
		{
			switch (slant) {
			case FontSlant.Italic:
				return SKFontStyleSlant.Italic;
			case FontSlant.Oblique:
				return SKFontStyleSlant.Oblique;
			case FontSlant.Normal:
				return SKFontStyleSlant.Upright;
			default:
				throw new InvalidEnumArgumentException ();
			}
		}

		public static SKFontStyleWeight ToSKFontStyleWeight (this FontWeight fontWeight)
		{
			switch (fontWeight) {
			case FontWeight.Light:
				return SKFontStyleWeight.Light;
			case FontWeight.Bold:
				return SKFontStyleWeight.Bold;
			case FontWeight.Normal:
				return SKFontStyleWeight.Normal;
			default:
				throw new InvalidEnumArgumentException ();
			}
		}

		public static SKTextAlign ToSKTextAlign (this FontAlignment alignment)
		{
			switch (alignment) {
			case FontAlignment.Left:
				return SKTextAlign.Left;
			case FontAlignment.Center:
				return SKTextAlign.Center;
			case FontAlignment.Right:
				return SKTextAlign.Right;
			default:
				throw new InvalidEnumArgumentException ();
			}
		}
	}
}
