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
using VAS.Core.Common;
using VAS.Core.Interfaces;

namespace VAS.Services.AppUpdater
{
	public class AppUpdater : IService
	{
		string companyName, feedURL;
		IAppUpdater updater;

		public AppUpdater (string companyName, string feedURL)
		{
			this.companyName = companyName;
			this.feedURL = feedURL;
		}

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
			switch (Utils.OS) {
			case OperatingSystemID.Windows:
				updater = new SparkleWin ();
				break;
			case OperatingSystemID.OSX:
#if OSTYPE_OS_X
				updater = new SparkleOSX ();
				break;
#else
				Log.Error ("Sparkle backend for OS X not enabled in this build, define OSTYPE_OS_X");
				return false;
#endif
			case OperatingSystemID.Linux:
				updater = new SimplifiedAppUpdater ();
				break;
			}
			try {
				updater?.Start (companyName, App.Current.SoftwareName, App.Current.Version.ToString (),
								feedURL, App.Current.baseDirectory);
			} catch (Exception ex) {
				Log.Exception (ex);
				return false;
			}
			return true;
		}

		public bool Stop ()
		{
			try {
				updater?.Stop ();
			} catch (Exception ex) {
				Log.Exception (ex);
				return false;
			}
			return true;
		}
	}
}
