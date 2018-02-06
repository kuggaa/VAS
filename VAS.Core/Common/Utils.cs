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
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
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

		/// <summary>
		/// Return the corresponding FontSlant enum from the given string
		/// </summary>
		/// <returns>The font slant.</returns>
		/// <param name="fontSlant">Font slant.</param>
		public static FontSlant ParseFontSlant (string fontSlant)
		{
			switch (fontSlant.ToLower ()) {
			case "italic":
			case "italics":
				return FontSlant.Italic;
			case "oblique":
			case "obliques":
				return FontSlant.Oblique;
			default:
				return FontSlant.Normal;
			}
		}

		public static string GetDataFilePath (string filename)
		{
			string fileNameTmp, result = "";
			foreach (string dataDir in App.Current.DataDir) {
				fileNameTmp = Path.Combine (dataDir, filename);
				Log.Verbose ($"Looking for data in {fileNameTmp}");
				if (File.Exists (fileNameTmp)) {
					Log.Verbose ($"Found {fileNameTmp}");
					result = fileNameTmp;
				}
			}
			if (result == "") {
				throw new FileNotFoundException (filename);
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
		/// Starts a process with output redirection.
		/// </summary>
		/// <returns>The started process.</returns>
		/// <param name="filename">Filename without extension.</param>
		/// <param name="arguments">Arguments.</param>
		/// <param name="environmentVariables">Environment variables dictionary.</param>
		public static Process StartProcess (string filename, string arguments = "", StringDictionary environmentVariables = null)
		{
			Process proc = new Process ();
			proc.EnableRaisingEvents = false;
			proc.StartInfo.WorkingDirectory = Path.GetFullPath (Path.Combine (App.Current.baseDirectory, "bin"));

			if (environmentVariables != null) {
				foreach (string variable in environmentVariables.Keys) {
					proc.StartInfo.EnvironmentVariables [variable] = environmentVariables [variable];
				}
			}

			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				proc.StartInfo.FileName = Path.Combine (proc.StartInfo.WorkingDirectory, $"{filename}.exe");
			} else {
				proc.StartInfo.FileName = filename;
			}
			proc.StartInfo.Arguments = arguments;
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.StartInfo.UseShellExecute = false;
			proc.Start ();
			return proc;
		}

		/// <summary>
		/// Opens the URL. if sourcePoint is specified it appends to the URL the necessary parameters for tracking
		/// </summary>
		/// <param name="url">URL.</param>
		/// <param name="sourcePoint">Source point.</param>
		public static void OpenURL (string url, string sourcePoint = null)
		{
			try {
				// FIXME: If there is no ticketId pass the serialId until the web supports retrieving the ticket id
				// in case is not in the configuration
				string ticketIdValue = App.Current.Config.LicenseCode ?? App.Current.LicenseManager.ContainerId;
				if (url.Contains ("?")) {
					url += "&";
				} else {
					url += "?";
				}
				url += $"ticketID={ticketIdValue}";
#if !DEBUG
				if (!string.IsNullOrEmpty (sourcePoint)) {
					url += $"&utm_source={App.Current.SoftwareName}&utm_medium={sourcePoint}&sessionid={App.Current.KPIService.SessionID}&userid={App.Current.Device.ID}";
				}
#endif
				Process.Start (url);
			} catch (Exception ex) {
				Log.Debug ("Failed opening url: " + ex);
			}
		}

		/// <summary>
		/// Checks the network connection.
		/// </summary>
		/// <returns><c>true</c>, if internet connection is OK, <c>false</c> otherwise.</returns>
		public static bool CheckNetworkConnection ()
		{
			try {
				Log.Debug ("Checking Network Connection");
				Ping myPing = new Ping ();
				String host = "8.8.8.8";
				byte [] buffer = new byte [32];
				int timeout = 1000;
				PingOptions pingOptions = new PingOptions ();
				PingReply reply = myPing.Send (host, timeout, buffer, pingOptions);
				bool result = (reply.Status == IPStatus.Success);
				Log.Debug ($"Ping to 8.8.8.8 was {result}");
				return result;
			} catch (Exception) {
				try {
					//Try with NTP server
					Log.Debug ("Error, checking ping connection, trying with NTP Server");
					return GetNetworkTime () > new DateTime (1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
				} catch (Exception ex) {
					Log.Warning ("Error checking network time: " + ex);
					return false;
				}
			}
		}

		/// <summary>
		/// Gets the network time.
		/// </summary>
		/// <returns>The network time in UTC</returns>
		public static DateTime GetNetworkTime ()
		{
			//From: http://stackoverflow.com/a/20157068
			const string ntpServer = "pool.ntp.org";
			var ntpData = new byte [48];
			ntpData [0] = 0x1B; //LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)

			var addresses = Dns.GetHostEntry (ntpServer).AddressList;
			var ipEndPoint = new IPEndPoint (addresses [0], 123);
			var socket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			//Stops code hang if NTP is blocked
			socket.ReceiveTimeout = 1000;
			socket.Connect (ipEndPoint);
			socket.Send (ntpData);
			socket.Receive (ntpData);
			socket.Close ();

			ulong intPart = (ulong)ntpData [40] << 24 | (ulong)ntpData [41] << 16 | (ulong)ntpData [42] << 8 | (ulong)ntpData [43];
			ulong fractPart = (ulong)ntpData [44] << 24 | (ulong)ntpData [45] << 16 | (ulong)ntpData [46] << 8 | (ulong)ntpData [47];

			var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
			var networkDateTime = (new DateTime (1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds ((long)milliseconds);

			return networkDateTime;
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
			} else {
				version = $"{OS} {version}";
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

