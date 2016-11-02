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
//

using System;
using System.IO;
using Microsoft.HockeyApp.Extensibility;

namespace VAS.KPI
{
	internal class HockeyConstants
	{
		internal const string CrashFilePrefix = "crashinfo_";
		internal const string USER_AGENT_STRING = "Hockey/Mono";
		internal const string SDKNAME = "HockeySDKMono";
		internal const string NAME_OF_SYSTEM_SEMAPHORE = "HOCKEYAPPSDK_SEMAPHORE";

		internal static string SDKVERSION {
			get {
				return SdkVersionPropertyContextInitializer.GetAssemblyVersion ();
			}
		}

		public static string GetPathToHockeyCrashes ()
		{
			string path = Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData);
			if (!path.EndsWith ("\\", StringComparison.OrdinalIgnoreCase)) { path += "\\"; }
			path += "HockeyCrashes\\";
			if (!Directory.Exists (path)) { Directory.CreateDirectory (path); }
			return path;
		}


	}
}
