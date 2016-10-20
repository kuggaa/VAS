//
//  Copyright (C) 2016 Fluendo S.A.
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
using VAS.Core.Interfaces;
using VAS.Core.Common;
using System.Threading.Tasks;

namespace VAS.Core
{
	public class Device : IDevice
	{
		static Guid deviceID = Guid.Empty;

		public Guid ID {
			get {
				if (deviceID == Guid.Empty) {
					switch (Utils.OS) {
					case OperatingSystemID.Windows: {
							deviceID = DeviceID.WindowsDeviceID ();
							break;
						}
					case OperatingSystemID.Linux: {
							deviceID = DeviceID.LinuxDeviceID ();
							break;
						}
					case OperatingSystemID.OSX: {
							deviceID = DeviceID.OSXDeviceID ();
							break;
						}
					}
				}
				return deviceID;
			}
		}

		public Task<bool> CheckCapturePermissions ()
		{
			return AsyncHelpers.Return (true);
		}

		public Task<bool> CheckExternalStoragePermission ()
		{
			return AsyncHelpers.Return (true);
		}

		public void ShareFiles (string [] filePaths, bool emailEnabled)
		{
			return;
		}
	}
}
