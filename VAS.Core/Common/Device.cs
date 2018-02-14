//
//  Copyright (C) 2010 Andoni Morales Alastruey
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

namespace VAS.Core.Common
{
	public class Device
	{
		public Device ()
		{
			Formats = new List<DeviceVideoFormat> ();

		}

		/// <summary>
		/// Capture source type
		/// </summary>
		public CaptureSourceType DeviceType {
			get;
			set;
		}

		/// <summary>
		/// Device id, can be a human friendly name (for DirectShow devices),
		/// the de device name (/dev/video0) or the GUID (dv1394src)
		/// </summary>
		public string ID {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the name of the Multimedia source element.
		/// </summary>
		/// <value>The source element.</value>
		public string SourceElement {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a prefix to use in the device description.
		/// </summary>
		/// <value>The prefix.</value>
		public string Prefix { get; set; }

		public List<DeviceVideoFormat> Formats {
			get;
			set;
		}

		/// <summary>
		/// Gets the description of the device.
		/// </summary>
		/// <returns>The description.</returns>
		/// <param name="includeSourceName">If set to <c>true</c> include source name.</param>
		public string GetDescription (bool includeSourceName)
		{
			var deviceName = "";
			if (!String.IsNullOrEmpty (Prefix)) {
				deviceName += $"{Prefix} ";
			}

			if (String.IsNullOrEmpty (ID)) {
				deviceName += Catalog.GetString ("Unknown");
			} else {
				deviceName += ID;
			}
			if (includeSourceName) {
				deviceName += $" ({SourceElement})";
			}
			return deviceName;
		}
	}

	public struct DeviceVideoFormat
	{
		public int width;
		public int height;
		public int fps_n;
		public int fps_d;

		public override string ToString ()
		{
			if (width == 0 && height == 0 && fps_n == 0 && fps_d == 0) {
				return Catalog.GetString ("Default");
			}
			if (fps_n == 0 && fps_d == 0) {
				return string.Format ("{0}x{1}", width, height);
			}
			return string.Format ("{0}x{1}@{2}fps", width, height,
				((double)fps_n / fps_d).ToString ("#.##"));
		}
	}
}
