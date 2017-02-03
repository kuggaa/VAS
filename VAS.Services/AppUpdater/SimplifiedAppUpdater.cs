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
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAS.Core.Common;
using VAS.Core.Interfaces;

namespace VAS.Services.AppUpdater
{
	/// <summary>
	/// An app updater implementation for platforms not supporting Sparkle. It uses the previous updates notifier and
	/// it will only notify the user about new updates without actually installing them.
	/// </summary>
	public class SimplifiedAppUpdater : IAppUpdater
	{
		CancellationTokenSource tokenSource;

		public void Start (string companyName, string appName, string version, string castURL, string baseDir)
		{
			CheckForUpdates ();
		}

		public void Stop ()
		{
			tokenSource?.Cancel (false);
		}

		public void CheckForUpdates ()
		{
			if (tokenSource != null) {
				tokenSource.Cancel ();
			}
			tokenSource = new CancellationTokenSource ();
			Task.Factory.StartNew (async () => {
				try {
					await CheckForUpdatesInBackground ();
				} catch (Exception ex) {
					Log.Exception (ex);
				}
			}, tokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
		}

		async Task<string> FetchNewVersion (string url)
		{
			string appcastStr = null;
			try {
				var wb = new WebClient ();
				appcastStr = await wb.DownloadStringTaskAsync (url);
			} catch (Exception ex) {
				Log.Warning ($"UpdatesNotifier: Error downloading version file from {url}");
				Log.Exception (ex);
				return appcastStr;
			}
			Log.Information ($"UpdatesNotifier: Downloaded latest version from {url}");
			return appcastStr;
		}

		async Task CheckForUpdatesInBackground ()
		{
			Version latestVersion;
			string downloadURL;
			string changeLog;
			string appcastStr = await FetchNewVersion (App.Current.LatestVersionURL);

			if (!ParseLatestVersion (appcastStr, out latestVersion, out downloadURL, out changeLog))
				return;

			if (App.Current.Version <= latestVersion) {
				Log.Information ($"UpdatesNotifier: Current version is {App.Current.Version} and latest available is " +
								 $"{latestVersion}: Update not needed.");
				return;
			}
			Log.Information ($"UpdatesNotifier: Current version is {App.Current.Version} and latest available is " +
							 $"{latestVersion}: Update needed.");

			if (latestVersion == App.Current.Config.IgnoreUpdaterVersion) {
				Log.Information ($"UpdatesNotifier: Version {latestVersion} has been silenced. Not warning user about update.");
				return;
			}

			App.Current.GUIToolkit.Invoke (async delegate {
				bool ignore = await App.Current.Dialogs.NewVersionAvailable (App.Current.Version, latestVersion,
								  downloadURL, changeLog, null);
				if (ignore) {
					/* User requested to ignore this version */
					Log.Information ($"UpdatesNotifier: Marking version {latestVersion} as silenced.");
					App.Current.Config.IgnoreUpdaterVersion = latestVersion;
				}
			});
		}

		bool ParseLatestVersion (string appcastStr, out Version latestVersion, out string downloadURL, out string changeLog)
		{
			// Load the XML document
			XmlDocument doc = new XmlDocument ();
			doc.Load (appcastStr);

			changeLog = null;
			latestVersion = null;
			downloadURL = null;

			XmlNode latest = doc.GetElementsByTagName ("Item") [0];
			foreach (XmlNode child in latest.ChildNodes) {
				if (child.Name == "sparkle:releaseNotes") {
					changeLog = child.Value;
				}
				if (child.Name == "enclosure") {
					var item = child.Attributes.GetNamedItem ("sparkle:os");
					if (item == null || item.Value != "Linux") {
						continue;
					}
					latestVersion = new Version (child.Attributes ["sparkle:version"].Value);
					downloadURL = child.Attributes ["url"].Value;
					return true;
				}
			}
			return false;
		}
	}
}
