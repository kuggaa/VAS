//
//  Copyright (C) 2015 fluendo
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
using VAS.Core.MVVMC;

namespace VAS.Core.Common
{
	[Serializable]
	public abstract class BaseImage<T> : BindableBase, ISerializable, IDisposable where T : IDisposable
	{

		protected const string BUF_PROPERTY = "pngbuf";

		public BaseImage ()
		{
		}

		public BaseImage (T image)
		{
			Value = image;
		}

		public BaseImage (string filename)
		{
			Value = LoadFromFile (filename);
		}

		public BaseImage (string filename, int width, int height)
		{
			Value = LoadFromFile (filename, width, height);
		}

		public BaseImage (Stream stream)
		{
			Value = LoadFromStream (stream);
		}

		public BaseImage (Stream stream, int width, int height)
		{
			Value = LoadFromStream (stream, width, height);
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			Value.Dispose ();
		}

		public virtual T Value {
			get;
			protected set;
		}

		public int Width {
			get {
				return GetWidth ();
			}
		}

		public int Height {
			get {
				return GetHeight ();
			}
		}

		public void ScaleInplace ()
		{
			ScaleInplace (Constants.MAX_THUMBNAIL_SIZE, Constants.MAX_THUMBNAIL_SIZE);
		}

		public void ScaleFactor (int destWidth, int destHeight, ScaleMode mode,
								 out double scaleX, out double scaleY, out Point offset)
		{
			//ScaleFactor (GetWidth (), GetHeight (), destWidth, destHeight, mode, out scaleX, out scaleY, out offset);
			ScaleFactor (Width, Height, destWidth, destHeight, mode, out scaleX, out scaleY, out offset);
		}

		public static void ScaleFactor (int imgWidth, int imgHeight, int destWidth, int destHeight,
										ScaleMode mode, out double scaleX, out double scaleY, out Point offset)
		{
			int oWidth = 0;
			int oHeight = 0;

			ComputeScale (imgWidth, imgHeight, destWidth, destHeight, mode, out oWidth, out oHeight);
			scaleX = (double)oWidth / imgWidth;
			scaleY = (double)oHeight / imgHeight;
			offset = new Point ((double)(destWidth - oWidth) / 2, (double)(destHeight - oHeight) / 2);
		}

		public static void ComputeScale (int inWidth, int inHeight, int reqOutWidth, int reqOutHeight,
										 ScaleMode mode, out int outWidth, out int outHeight)
		{
			outWidth = reqOutWidth;
			outHeight = reqOutHeight;

			if (mode == ScaleMode.Fill) {
				return;
			}

			double par = (double)inWidth / (double)inHeight;
			double outPar = (double)reqOutWidth / (double)reqOutHeight;

			if (mode == ScaleMode.AspectFill) {
				if (outPar < par) {
					outWidth = (int)(outHeight * par);
				} else {
					outHeight = (int)(outWidth / par);
				}
			} else if (mode == ScaleMode.AspectFit) {
				if (outPar > par) {
					outWidth = (int)(outHeight * par);
				} else {
					outHeight = (int)(outWidth / par);
				}
			}
		}

		public void ScaleInplace (int maxWidth, int maxHeight)
		{
			T scalled;

			scalled = Scale (Value, maxWidth, maxHeight);
			Value.Dispose ();
			Value = scalled;
		}

		// this method is automatically called during serialization
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			try {
				info.AddValue (BUF_PROPERTY, Serialize ());
			} catch {
				info.AddValue (BUF_PROPERTY, null);
			}
		}

		public abstract Image Scale (int maxWidth, int maxHeight);

		public abstract byte [] Serialize ();

		public abstract void Save (string filename);

		public abstract Image Composite (Image image2);

		protected abstract T LoadFromFile (string filename);

		protected abstract T LoadFromFile (string filename, int width, int height);

		protected abstract T LoadFromStream (Stream stream);

		protected abstract T LoadFromStream (Stream stream, int width, int height);

		protected abstract T Scale (T pix, int maxWidth, int maxHeight);

		protected abstract int GetWidth ();

		protected abstract int GetHeight ();

		public abstract IntPtr LockPixels ();

		public abstract void UnlockPixels (IntPtr pixels);
	}
}
