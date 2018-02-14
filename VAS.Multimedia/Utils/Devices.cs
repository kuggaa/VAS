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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using VAS.Core.Common;
using VAS.Core.Resources;
using Device = VAS.Core.Common.Device;

namespace VAS.Multimedia.Utils
{
	public class Devices
	{
		[DllImport ("libvas.dll")]
		static extern IntPtr lgm_device_enum_video_devices (string source);

		[DllImport ("libvas.dll")]
		static extern IntPtr lgm_device_get_formats (IntPtr raw);

		[DllImport ("libvas.dll")]
		static extern IntPtr lgm_device_get_device_name (IntPtr raw);

		[DllImport ("libvas.dll")]
		static extern void lgm_device_free (IntPtr raw);

		[DllImport ("libvas.dll")]
		static extern IntPtr lgm_device_video_format_get_info (IntPtr raw, out int width, out int height,
															   out int fps_n, out int fps_d);

		static readonly string AVFVIDEOSRC = "avfvideosrc";
		static readonly string OSXSCREENCAPSRC = "osxscreencapsrc";
		static readonly string DECKLINKVIDEOSRC = "decklinkvideosrc";
		static readonly string KSVIDEOSRC = "ksvideosrc";
		static readonly string DSHOWVIDEOSRC = "dshowvideosrc";
		static readonly string GDISCREENCAPSRC = "gdiscreencapsrc";
		static readonly string DX9SCREENCAPSRC = "dx9screencapsrc";
		static readonly string V4L2SRC = "v4l2src";
		static readonly string DV1394SRC = "dv1394src";
		static readonly string [] devices_osx = { AVFVIDEOSRC, OSXSCREENCAPSRC, DECKLINKVIDEOSRC };
		static readonly string [] devices_win = { KSVIDEOSRC, DSHOWVIDEOSRC, GDISCREENCAPSRC, DX9SCREENCAPSRC };
		static readonly string [] devices_lin = { V4L2SRC, DV1394SRC };

		static public List<Device> ListVideoDevices ()
		{
			string [] devices;
			if (VAS.Core.Common.Utils.OS == OperatingSystemID.OSX)
				devices = devices_osx;
			else if (VAS.Core.Common.Utils.OS == OperatingSystemID.Windows)
				devices = devices_win;
			else
				devices = devices_lin;

			List<Device> devicesList = new List<Device> ();

			foreach (string source in devices) {
				GLib.List devices_raw = new GLib.List (lgm_device_enum_video_devices (source),
											typeof (IntPtr), true, false);

				foreach (IntPtr device_raw in devices_raw) {
					string deviceName = GLib.Marshaller.PtrToStringGFree (lgm_device_get_device_name (device_raw));
					/* The Direct Show GStreamer element seems to have problems with the
					 * BlackMagic DeckLink cards, so filter them out. They are also
					 * available through the ksvideosrc element. */
					if (source == DSHOWVIDEOSRC &&
						Regex.Match (deviceName, ".*blackmagic.*|.*decklink.*", RegexOptions.IgnoreCase).Success) {
						continue;
					}

					Device device = new Device ();
					device.DeviceType = CaptureSourceType.System;
					device.SourceElement = source;
					device.ID = deviceName;
					if (source == GDISCREENCAPSRC || source == DX9SCREENCAPSRC) {
						device.Prefix = Strings.Monitor;
					}

					GLib.List formats_raw = new GLib.List (lgm_device_get_formats (device_raw),
												typeof (IntPtr), false, false);
					foreach (IntPtr format_raw in formats_raw) {
						DeviceVideoFormat format = new DeviceVideoFormat ();
						lgm_device_video_format_get_info (format_raw, out format.width, out format.height,
							out format.fps_n, out format.fps_d);
						device.Formats.Add (format);
					}
					/* Make sure the device has formats
					 * before adding it to the device list */
					if (device.Formats.Count > 0) {
						devicesList.Add (device);
					}
					lgm_device_free (device_raw);
				}
			}
			return devicesList;
		}
	}
}

