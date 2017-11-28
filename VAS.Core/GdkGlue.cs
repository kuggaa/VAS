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
using System.Runtime.InteropServices;
using Gdk;

namespace VAS
{
	/// <summary>
	/// Gdk extension methods.
	/// </summary>
	public static class GdkGlue
	{
		[DllImport ("libgdk_pixbuf-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool gdk_pixbuf_save_utf8 (IntPtr raw, string filename, string type, out IntPtr error, IntPtr dummy);

		[DllImport ("libgdk_pixbuf-2.0-0.dll")]
		static extern IntPtr gdk_pixbuf_new_from_file_utf8 (IntPtr filename, out IntPtr error);

		[DllImport ("libgdk_pixbuf-2.0-0.dll")]
		static extern IntPtr gdk_pixbuf_new_from_file_at_size_utf8 (IntPtr filename, int width, int height, out IntPtr error);

		/// <summary>
		/// Save the image in the specified path.
		/// </summary>
		/// <param name="image">Image.</param>
		/// <param name="filePath">File path.</param>
		public static void SaveUtf (this Pixbuf image, string filePath, string extension)
		{
			IntPtr error = IntPtr.Zero;
			gdk_pixbuf_save_utf8 (image.Handle, filePath, extension, out error, IntPtr.Zero);
			if (error != IntPtr.Zero) {
				throw new GLib.GException (error);
			}
		}

		/// <summary>
		/// Creates the pixbuf for win32.
		/// </summary>
		/// <returns>The pixbuf.</returns>
		/// <param name="filename">Filename.</param>
		public static Pixbuf CreatePixbufWin32 (string filename)
		{
			IntPtr native_filename = GLib.Marshaller.StringToPtrGStrdup (filename);
			IntPtr error = IntPtr.Zero;
			IntPtr raw = gdk_pixbuf_new_from_file_utf8 (native_filename, out error);
			GLib.Marshaller.Free (native_filename);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return new Pixbuf (raw);
		}

		/// <summary>
		/// Creates the pixbuf at size for win32.
		/// </summary>
		/// <returns>The pixbuf.</returns>
		/// <param name="filename">Filename.</param>
		public static Pixbuf CreatePixbufWin32 (string filename, int width, int height)
		{
			IntPtr native_filename = GLib.Marshaller.StringToPtrGStrdup (filename);
			IntPtr error = IntPtr.Zero;
			IntPtr raw = gdk_pixbuf_new_from_file_at_size_utf8 (native_filename, width, height, out error);
			GLib.Marshaller.Free (native_filename);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return new Pixbuf (raw);
		}
	}
}
