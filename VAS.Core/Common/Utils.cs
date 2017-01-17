//
//  Copyright (C) 2015 Andoni Morales Alastruey
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
using System.Reflection;

namespace VAS.Core.Common
{
	public class Utils
	{
		static Random randomGen;
		static OperatingSystemID operatingSystem = OperatingSystemID.None;
		static string osVersion = null;

		public static string SanitizePath (string path, params char [] replaceChars)
		{
			path = path.Trim ();
			foreach (char c in Path.GetInvalidFileNameChars ()) {
				path = path.Replace (c, '_');
			}
			foreach (char c in replaceChars) {
				path = path.Replace (c, '_');
			}
			return path;
		}

		static public string SysInfo {
			get {
				return string.Format (
					"Running {0} {1} build:\"{2}\" OS:\"{3}\" OS Version:\"{4}\" Device ID:\"{5}\"",
					App.Current.SoftwareName,
					App.Current.Version,
					App.Current.BuildVersion,
					Utils.OS,
					OSVersion,
					App.Current.Device.ID
				);
			}
		}

		public static OperatingSystemID OS {
			get {
				if (operatingSystem == OperatingSystemID.None) {
					#if OSTYPE_ANDROID
					operatingSystem = OperatingSystemID.Android;
					#elif OSTYPE_IOS
					operatingSystem = OperatingSystemID.iOS;
					#else
					switch (Environment.OSVersion.Platform) {
					case PlatformID.MacOSX:
						operatingSystem = OperatingSystemID.OSX;
						break;
					case PlatformID.Unix:
						// OS X is detetected as a Unix system and needs an extra check using the filesystem layout
						if (Directory.Exists ("/Applications")
						    & Directory.Exists ("/System")
						    & Directory.Exists ("/Users")
						    & Directory.Exists ("/Volumes")) {
							operatingSystem = OperatingSystemID.OSX;
						} else {
							operatingSystem = OperatingSystemID.Linux;
						}
						break;
					case PlatformID.Win32NT:
						operatingSystem = OperatingSystemID.Windows;
						break;
					default:
						throw new NotSupportedException ();
					}
					#endif
				}
				return operatingSystem;
			}
		}

		/// <summary>
		///     Returns the OS version number for this computer.
		/// </summary>
		public static string OSVersion {
			get {
				if (osVersion == null) {
					if (OS == OperatingSystemID.Windows) {
						osVersion = GetWindowsVersion ();
					} else {
						osVersion = GetOSVersionFallback ();
					}
				}
				return osVersion;
			}
		}

		public static Color RandomColor ()
		{
			if (randomGen == null)
				randomGen = new Random ();
			return new Color (Convert.ToByte (randomGen.Next (0, 255)), Convert.ToByte (randomGen.Next (0, 255)),
				Convert.ToByte (randomGen.Next (0, 255)));
		}

		public static string GetDataFilePath (string filename)
		{
			string fileNameTmp, result = "";
			foreach (string dataDir in App.Current.DataDir) {
				fileNameTmp = Path.Combine (dataDir, filename);
				if (File.Exists (fileNameTmp)) {
					result = fileNameTmp;
				}
			}
			if (result == "") {
				throw new FileNotFoundException ();
			}
			return result;
		}

		public static string GetDataDirPath (string dirname)
		{
			string dirNameTmp, result = "";
			foreach (string dataDir in App.Current.DataDir) {
				dirNameTmp = Path.Combine (dataDir, dirname);
				if (Directory.Exists (dirNameTmp)) {
					result = dirNameTmp;
				}
			}
			if (result == "") {
				throw new DirectoryNotFoundException ();
			}
			return result;
		}

		public static Stream GetEmbeddedResourceFileStream (string resourceId)
		{
			var assembly = Assembly.GetCallingAssembly ();
			return assembly.GetManifestResourceStream (resourceId);
		}

		/// <summary>
		///     Returns the Windows version number for this computer.
		///     Adapted from http://stackoverflow.com/a/37716269/1324984
		/// </summary>
		static string GetWindowsVersion ()
		{
			Log.Debug ("Getting windows version");
			string version = null;
			int majorVersion, minorVersion;
			// The 'CurrentMajorVersionNumber' and 'CurrentMinorVersionNumber' string values in the CurrentVersion key is new for Windows 10, 
			// and will most likely (hopefully) be there for some time before MS decides to change this - again...
			string versionRegistryPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
			string majorRegistryKey = "CurrentMajorVersionNumber";
			string minorRegistryKey = "CurrentMinorVersionNumber";

			try {
				var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey (versionRegistryPath);
				if (key != null) {
					object val = key.GetValue (majorRegistryKey);
					if (val != null) {
						majorVersion = (int)val;
						Log.Debug ("major version = " + majorVersion);
						object valMinor = key.GetValue (minorRegistryKey);
						minorVersion = (int)valMinor;
						Log.Debug ("minor version = " + valMinor);
						version = majorVersion + "." + minorVersion;
					} else {
						// When the 'CurrentMajorVersionNumber' value is not present we fallback to reading the previous key used for this: 'CurrentVersion'
						version = key.GetValue ("CurrentVersion") as string;
						Log.Debug ("CurrentVersion = " + version);
					}
				}
			} catch (Exception e) {
				Log.Exception (e);
			}

			// When we can't access the version correctly, we fallback to the old (bad) way of getting the OS.
			// In windows systems greater than Windows 8 without a manifest, this will always return Windows 8.
			if (version == null) {
				Log.Warning ("Failed getting OS version. Falling back to Environment");
				version = GetOSVersionFallback ();
			}
			Log.Debug ("version = " + version);

			return version;
		}

		static string GetOSVersionFallback ()
		{
			return $"{OS} {Environment.OSVersion.Version} {Environment.OSVersion.ServicePack}";
		}
	}
}

