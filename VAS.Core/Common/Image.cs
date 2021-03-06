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
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Gdk;

namespace VAS.Core.Common
{
	[Serializable]
	public class Image : BaseImage<Pixbuf>
	{
		const string FILE_EXTENSION = "png";

		public Image (int width, int height)
		{
			Value = new Pixbuf (Colorspace.Rgb, true, 32, width, height);
		}

		public Image (Pixbuf image) : base (image)
		{
		}

		public Image (string filepath) : base (filepath)
		{
		}

		public Image (string filepath, int width, int height) : base (filepath, width, height)
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

		protected override Pixbuf LoadFromFile (string filepath)
		{
			int idx = filepath.LastIndexOf ('.');
			var path = filepath.Substring (0, idx) + "@2x" + filepath.Substring (idx);
			if (App.Current.FileSystemManager.FileExists (path)) {
				//FIXME: this seems a HACK, on non-retina displays it will load @2x and that's why should force a
				//device scale factor of 2 on non-retina displays.
				deviceScaleFactor = 2;
				return CreatePixbuf (path);
			}
			return CreatePixbuf (filepath);
		}

		protected override Pixbuf LoadFromFile (string filepath, int width, int height)
		{
			return CreatePixbuf (filepath, width, height);
		}

		protected override Pixbuf LoadFromStream (Stream stream)
		{
			return new Pixbuf (stream);
		}

		protected override Pixbuf LoadFromStream (Stream stream, int width, int height)
		{
			return new Pixbuf (stream, width, height);
		}

		public override byte [] Serialize ()
		{
			if (Value == null)
				return null;
			return Value.SaveToBuffer ("png");
		}

		public static Image Deserialize (byte [] ser)
		{
			return new Image (new Pixbuf (ser));
		}

		public override Image Scale (int maxWidth, int maxHeight)
		{
			return new Image (Scale (Value, maxWidth, maxHeight));
		}

		public override IntPtr LockPixels ()
		{
			return Value.Pixels;
		}

		public override void UnlockPixels (IntPtr pixels)
		{
		}

		protected override Pixbuf Scale (Pixbuf pix, int maxWidth, int maxHeight)
		{
			int width, height;

			ComputeScale (pix.Width, pix.Height, maxWidth, maxHeight, ScaleMode.AspectFit, out width, out height);
			return pix.ScaleSimple (width, height, Gdk.InterpType.Bilinear);
		}

		/// <summary>
		/// Save the image in the specified path.
		/// </summary>
		/// <param name="filename">Filename.</param>
		public override void Save (string filename)
		{
			//HACK: Force gdk_pixbuf_save_utf8 call if windows OS. Otherwhise call gdk_pixbuf_save
			if (Utils.OS == OperatingSystemID.Windows) {
				Value.SaveUtf (filename, FILE_EXTENSION);
			} else {
				Value.Save (filename, FILE_EXTENSION);
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
			Pixbuf dest = new Pixbuf (Value.Colorspace, true, Value.BitsPerSample, Width, Height);
			Value.Composite (dest, 0, 0, Width, Height, 0, 0, 1, 1,
				Gdk.InterpType.Bilinear, 255);
			image.Value.Composite (dest, 0, 0, image.Width, image.Height, 0, 0, 1, 1,
				Gdk.InterpType.Bilinear, 255);
			return new Image (dest);
		}

		Pixbuf CreatePixbuf (string filename)
		{
			if (Utils.OS == OperatingSystemID.Windows) {
				return GdkGlue.CreatePixbufWin32 (filename);
			}
			return new Pixbuf (filename);
		}

		Pixbuf CreatePixbuf (string filename, int width, int height)
		{
			if (Utils.OS == OperatingSystemID.Windows) {
				return GdkGlue.CreatePixbufWin32 (filename, width, height);
			}
			return new Pixbuf (filename, width, height);
		}
	}
}
