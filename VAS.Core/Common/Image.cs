// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using System.IO;
using System.Runtime.Serialization;
using Gdk;
using SkiaSharp;

namespace VAS.Core.Common
{
	[Serializable]
	public class Image : BaseImage<SKBitmap>
	{
		const string FILE_EXTENSION = "png";

		public Image (int width, int height)
		{
			Value = new SKBitmap (width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
		}

		public Image (SKBitmap image) : base (image)
		{
		}

		public Image (string filepath) : base (filepath)
		{
		}

		public Image (string filepath, int width, int height) : base (filepath, width, height)
		{
		}

		public Image (byte [] data, int width, int height, int stride) : base (data, width, height, stride)
		{
		}

		public Image (Stream stream) : base (stream)
		{
		}

		public Image (Stream stream, int width, int height) : base (stream, width, height)
		{
		}

		// this constructor is automatically called during deserialization
		public Image (SerializationInfo info, StreamingContext context)
		{
			try {
				Value = Deserialize ((byte [])info.GetValue (BUF_PROPERTY, typeof (byte []))).Value;
			} catch {
				Value = null;
			}
		}

		protected override SKBitmap LoadFromFile (string filepath)
		{
			using (var fileStream = File.OpenRead (filepath)) {
				using (var stream = new SKManagedStream (fileStream)) {
					return SKBitmap.Decode (stream);
				}
			}
		}

		protected override SKBitmap LoadFromFile (string filepath, int width, int height)
		{
			int idx = filepath.LastIndexOf ('.');
			var path = filepath.Substring (0, idx) + "@2x" + filepath.Substring (idx);
			if (File.Exists (path)) {
				DeviceScaleFactor = 2;
				return new Pixbuf (path);
			}
			return new Pixbuf (filepath);
			SKBitmap bitmap = LoadFromFile (filepath);
			// FIXME: Scale
			return bitmap;
		}

		protected override SKBitmap LoadFromStream (Stream fileStream)
		{
			using (var stream = new SKManagedStream (fileStream)) {
				return SKBitmap.Decode (stream);
			}
		}

		protected override SKBitmap LoadFromData (byte [] data, int width, int height, int stride)
		{
			SKBitmap bitmap = new SKBitmap (width, height, SKColorType.Rgba8888, SKAlphaType.Opaque);
			throw new NotImplementedException ();
		}

		protected override Pixbuf LoadFromStream (Stream stream, int width, int height)
		{
			return new Pixbuf (stream, width, height);
		}

		public override byte [] Serialize ()
		{
			if (Value == null)
				return null;

			using (var image = SKImage.FromBitmap (Value)) {
				using (var data = image.Encode (SKEncodedImageFormat.Png, 100)) {
					return data.ToArray ();
				}
			}
		}

		public static Image Deserialize (byte [] ser)
		{
			return new Image (SKBitmap.Decode (ser));
		}

		public override Image Scale (int maxWidth, int maxHeight)
		{
			return new Image (Scale (Value, maxWidth, maxHeight));
		}

		public override IntPtr LockPixels ()
		{
			return Value.GetPixels ();
		}

		public override void UnlockPixels (IntPtr pixels)
		{
		}

		protected override SKBitmap Scale (SKBitmap pix, int maxWidth, int maxHeight)
		{
			int width, height;
			ComputeScale (pix.Width, pix.Height, maxWidth, maxHeight, ScaleMode.AspectFit, out width, out height);
			return pix.Resize (new SKImageInfo (width, height), SKBitmapResizeMethod.Lanczos3);
		}

		/// <summary>
		/// Save the image in the specified path.
		/// </summary>
		/// <param name="filename">Filename.</param>
		public override void Save (string filename)
		{
			using (var image = SKImage.FromBitmap (Value)) {
				using (var data = image.Encode (SKEncodedImageFormat.Png, 100)) {
					using (var stream = File.OpenWrite (filename)) {
						data.SaveTo (stream);
					}
				}
			}
		}

		protected override int GetWidth ()
		{
			return Value.Width;
		}

		protected override int GetHeight ()
		{
			return Value.Height;
		}

		public override Image Composite (Image image)
		{
			return image;
		}
	}
}
