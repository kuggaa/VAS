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
using System.Threading.Tasks;
using AppKit;
using Foundation;
using ObjCRuntime;
using Sparkle;
using VAS.Core.Common;
using VAS.Core.Interfaces;

namespace VAS.Services.AppUpdater
{
	public class SparkleOSX : IAppUpdater
	{
		SUUpdater updater;

		public void Start (string companyName, string appName, string version, string castURL, string baseDir)
		{
			try {
				// This internally calls ObjCRuntime.Runtime.EnsureInitialized () to intialize Xamarin.Mac's internals
				NSApplication.Init ();
				string bundlePath = GetBundlePath ();

				string libPath = Path.GetFullPath (Path.Combine (baseDir, "frameworks", "Sparkle.framework", "Sparkle"));
				if (Dlfcn.dlopen (libPath, 0) == IntPtr.Zero) {
					throw new Exception ($"Could not load Sparkle.framework {libPath}");
				}

				var bundle = new NSBundle (bundlePath);
				updater = new SUUpdater (bundle);
				updater.Delegate = new UpdaterDelegate ();
				updater.FeedURL = new NSUrl (castURL);
				updater.AutomaticallyChecksForUpdates = true;
			} catch (Exception ex) {
				Log.Error ($"Updater will not start: {ex.Message}");
			}
		}

		public void Stop ()
		{
		}

		public void CheckForUpdates ()
		{
			updater?.CheckForUpdatesInBackground ();
		}

		string GetBundlePath ()
		{
			string infoPlist = Path.Combine (App.Current.baseDirectory, "..", "Info.plist");
			if (!File.Exists (infoPlist)) {
				throw new Exception ($"{infoPlist} not found");
			}
			string bundlePath = Path.GetFullPath (Path.Combine (infoPlist, "..", ".."));
			Log.Information ($"Found bundle at path {bundlePath}");
			return bundlePath;
		}
	}

	class UpdaterDelegate : SUUpdaterDelegate
	{
		public override bool UpdaterShouldPromptForPermissionToCheckForUpdates (SUUpdater updater)
		{
			return false;
		}

		public override bool UpdaterShouldRelaunchApplication (SUUpdater updater)
		{
			// Let the user a laast change to save its work before closing
			Task<bool> task = App.Current.GUIToolkit.Quit ();
			task.ConfigureAwait (false);
			// Because this is a delegate and we don't have an option to await the result of the task, Wait() it with a
			// continuation in a different thread to avoid blocking.
			task.Wait ();
			return task.Result;
		}
	}
}
