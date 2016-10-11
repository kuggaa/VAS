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
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace VAS.Core.Common
{
	public static class DeviceUtils
	{
		const int RTLD_NOW = 2;
		const string CORE_FOUNDATION = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
		const string IO_KIT = "/System/Library/Frameworks/IOKit.framework/IOKit";
		const string SYSTEM = "/usr/lib/libSystem.dylib";
		const string K_IO_MASTERPORT_DEFAULT = "kIOMasterPortDefault";
		const string IO_SERVICE = "IOService:/";
		static IntPtr platformUUIDCF = GetCFString ("IOPlatformUUID");
		static Guid deviceID = Guid.Empty;

		[DllImport (SYSTEM)]
		static extern IntPtr dlopen (string filename, int flags);

		[DllImport (SYSTEM)]
		static extern IntPtr dlclose (IntPtr handle);

		[DllImport (SYSTEM)]
		static extern IntPtr dlsym (IntPtr lib, string symbol);

		[DllImport (IO_KIT, CharSet = CharSet.Unicode)]
		static extern IntPtr IORegistryEntryCreateCFProperty (IntPtr registry, IntPtr key, IntPtr allocator, uint options);

		[DllImport (IO_KIT)]
		static extern IntPtr IORegistryEntryFromPath (IntPtr port, string path);

		[DllImport (IO_KIT, CharSet = CharSet.Unicode)]
		static extern void IOObjectRelease (IntPtr self);

		[DllImport (CORE_FOUNDATION, CharSet = CharSet.Unicode)]
		static extern IntPtr CFStringCreateWithCharacters (IntPtr allocator, string str, int count);

		[DllImport (CORE_FOUNDATION, CharSet = CharSet.Unicode)]
		static extern void CFRelease (IntPtr obj);

		[DllImport (CORE_FOUNDATION, CharSet = CharSet.Unicode)]
		static extern int CFStringGetLength (IntPtr handle);

		[DllImport (CORE_FOUNDATION, CharSet = CharSet.Unicode)]
		static extern IntPtr CFStringGetCharactersPtr (IntPtr handle);

		[DllImport (CORE_FOUNDATION, CharSet = CharSet.Unicode)]
		static extern IntPtr CFStringGetCharacters (IntPtr handle, CFRange range, IntPtr buffer);


		[StructLayout (LayoutKind.Sequential)]
		struct CFRange
		{
			long loc; // defined as 'long' in native code
			long len; // defined as 'long' in native code

			public CFRange (long l, long len)
			{
				this.loc = l;
				this.len = len;
			}
		}

		public static Guid DeviceID {
			get {
				if (deviceID == Guid.Empty) {
					switch (Utils.OS) {
					case OperatingSystemID.Windows: {
							deviceID = WindowsDeviceID ();
							break;
						}
					case OperatingSystemID.Linux: {
							deviceID = LinuxDeviceID ();
							break;
						}
					case OperatingSystemID.OSX: {
							deviceID = OSXDeviceID ();
							break;
						}
					}
				}
				return deviceID;
			}
		}

		static string GetString (IntPtr handle)
		{
			string str;

			if (handle == IntPtr.Zero)
				return null;

			int l = CFStringGetLength (handle);
			IntPtr u = CFStringGetCharactersPtr (handle);
			IntPtr buffer = IntPtr.Zero;
			if (u == IntPtr.Zero) {
				CFRange r = new CFRange (0, l);
				buffer = Marshal.AllocCoTaskMem (l * 2);
				CFStringGetCharacters (handle, r, buffer);
				u = buffer;
			}
			unsafe
			{
				str = new string ((char*)u, 0, l);
			}
			return str;
		}

		static IntPtr GetCFString (string str)
		{
			return CFStringCreateWithCharacters (IntPtr.Zero, str, str.Length);
		}

		static Guid OSXDeviceID ()
		{
			IntPtr ioKit = dlopen (IO_KIT, RTLD_NOW);
			if (ioKit != IntPtr.Zero) {
				IntPtr port = dlsym (ioKit, K_IO_MASTERPORT_DEFAULT);
				IntPtr ioRegistryRoot = IORegistryEntryFromPath (Marshal.ReadIntPtr (port), IO_SERVICE);
				if (ioRegistryRoot != IntPtr.Zero) {
					IntPtr uuidCf = IORegistryEntryCreateCFProperty (ioRegistryRoot, platformUUIDCF, IntPtr.Zero, 0);
					if (uuidCf != IntPtr.Zero) {
						deviceID = Guid.Parse (GetString (uuidCf));
					}
					IOObjectRelease (ioRegistryRoot);
				}
				dlclose (ioKit);
			}
			return deviceID;
		}

		static Guid WindowsDeviceID ()
		{
			RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey ("Software\\Microsoft\\Cryptography");
			try {
				return Guid.Parse (key.GetValue ("MachineGuid") as string);
			} catch (Exception ex) {
				Log.Exception (ex);
				return Guid.Empty;
			}
		}

		static Guid LinuxDeviceID ()
		{
			var proc = new Process {
				StartInfo = new ProcessStartInfo {
					FileName = "vas-system-uuid",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				}
			};

			proc.Start ();
			string line = "";
			while (!proc.StandardOutput.EndOfStream) {
				line = proc.StandardOutput.ReadLine ();
				break;
			}

			try {
				return Guid.Parse (line);
			} catch (Exception ex) {
				Log.Exception (ex);
				return Guid.Empty;
			}
		}
	}
}
