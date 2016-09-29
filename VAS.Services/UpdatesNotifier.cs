//
//  Copyright (C) 2015 Fluendo S.A.
//
//	This program is free software; you can redistribute it and/or modify
//	it under the terms of the GNU General Public License as published by
//	the Free Software Foundation; either version 2 of the License, or
//	(at your option) any later version.
//
//	This program is distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//	GNU General Public License for more details.
//
//		You should have received a copy of the GNU General Public License
//		along with this program; if not, write to the Free Software
//		Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//

using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAS.Core.Common;
using VAS.Core.Interfaces;

namespace VAS.Services
{
	public class UpdatesNotifier: IService
	{
		static public bool FetchNewVersion (string url, string filename)
		{
			var userAgent = String.Format ("{0}/{1} ({2};{3};{4})",
				                App.Current.SoftwareName,
				                App.Current.Version,
				                Utils.OS,
				                Environment.OSVersion.VersionString,
				                App.Current.BuildVersion);
			try {
				var wb = new WebClient ();
				wb.Headers.Add ("User-Agent", userAgent);
				wb.DownloadFile (url, filename);
			} catch (Exception ex) {
				Log.WarningFormat ("UpdatesNotifier: Error downloading version file from {0} to {1} (User-Agent: {2})",
					url, filename, userAgent);
				Log.Exception (ex);
				return false;
			}
			Log.InformationFormat ("UpdatesNotifier: Downloaded latest version from {0} to {1} (User-Agent: {2})",
				url, filename, userAgent);
			return true;
		}

		static public bool ParseNewVersion (string filename, out Version latestVersion, out string downloadURL, out string changeLog)
		{
			latestVersion = null;
			downloadURL = null;
			changeLog = null;
			try {
				var fileStream = new FileStream (filename, FileMode.Open);
				var sr = new StreamReader (fileStream);
				JObject latestObject = JsonConvert.DeserializeObject<JObject> (sr.ReadToEnd ());
				fileStream.Close ();

				latestVersion = new Version (latestObject ["version"].Value<string> ());
				downloadURL = latestObject ["url"].Value<string> ();
				changeLog = latestObject ["changes"].Value<string> ();
			} catch (Exception ex) {
				Log.WarningFormat ("UpdatesNotifier: Error parsing version file {0}", filename);
				Log.Exception (ex);
				return false;
			}
			Log.InformationFormat ("UpdatesNotifier: Latest version is {0}", latestVersion);
			return true;
		}

		static public bool IsOutDated (Version currentVersion, Version latestVersion)
		{
			return latestVersion > currentVersion;
		}

		static public void CheckForUpdates ()
		{
			string tempFile = Path.Combine (App.Current.HomeDir, "latest.json");
			if (!FetchNewVersion (App.Current.LatestVersionURL, tempFile))
				return;

			Version latestVersion;
			string downloadURL;
			string changeLog;
			if (!ParseNewVersion (tempFile, out latestVersion, out downloadURL, out changeLog))
				return;

			Version currentVersion = Assembly.GetExecutingAssembly ().GetName ().Version;
			if (!IsOutDated (currentVersion, latestVersion)) {
				Log.InformationFormat ("UpdatesNotifier: Current version is {0} and latest available is {1}: Update not needed.",
					currentVersion, latestVersion);
				return;
			}
			Log.InformationFormat ("UpdatesNotifier: Current version is {0} and latest available is {1}: Update needed.",
				currentVersion, latestVersion);

			if (latestVersion == App.Current.Config.IgnoreUpdaterVersion) {
				Log.InformationFormat ("UpdatesNotifier: Version {0} has been silenced. Not warning user about update.",
					latestVersion);
				return;
			}

			App.Current.GUIToolkit.Invoke (async delegate {
				bool ignore = await App.Current.Dialogs.NewVersionAvailable (currentVersion, latestVersion,
					              downloadURL, changeLog, null);
				if (ignore) {
					/* User requested to ignore this version */
					Log.InformationFormat ("UpdatesNotifier: Marking version {0} as silenced.", latestVersion);
					App.Current.Config.IgnoreUpdaterVersion = latestVersion;
				}
			});
		}

		#region IService

		public int Level {
			get {
				return 90;
			}
		}

		public string Name {
			get {
				return "Updates notifier";
			}
		}

		public bool Start ()
		{
			var thread = new Thread (new ThreadStart (CheckForUpdates));
			thread.Start ();

			return true;
		}

		public bool Stop ()
		{
			return true;
		}

		#endregion
	}
}
