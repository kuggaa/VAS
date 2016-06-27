﻿using System;
using System.Collections.Generic;
using System.IO;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;

namespace VAS
{
	public abstract class App
	{
		/* State */
		public IGUIToolkit GUIToolkit;
		public IMultimediaToolkit MultimediaToolkit;
		public IDrawingToolkit DrawingToolkit;
		public EventsBroker EventsBroker;

		public IStorageManager DatabaseManager;
		public IRenderingJobsManager RenderingJobsManger;


		public string homeDirectory = ".";
		public string baseDirectory = ".";
		public string configDirectory = ".";

		/* State */
		public EventsAggregator EventsAggregator;

		public List<string> DataDir {
			get {
				if (dataDir == null) {
					dataDir = new List<string> ();
				}
				return dataDir;
			}
			set {
				dataDir = value;
			}
		}

		protected StyleConf style;

		public static App Current {
			get;
			set;
		}

		List<string> dataDir;

		public static void Init (App appInit, string evUninstalled, string softwareName, string portableFile, string evHome)
		{
			/* NOTE
			*  All derived Configs should set the following:
			*  
			*  Config.baseDirectory
			*  Config.configDirectory
			*  Config.dataDir
			*  Config.homeDirectory
			*/

			Current = appInit;
			InitTranslations (softwareName);

			string home = null;

			if (Environment.GetEnvironmentVariable (evUninstalled) != null) {
				App.Current.baseDirectory = Path.GetFullPath (".");
				App.Current.DataDir.Add (App.Current.RelativeToPrefix ("../data"));
			} else {
				if (Utils.OS == OperatingSystemID.Android) {
					App.Current.baseDirectory = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
				} else if (Utils.OS == OperatingSystemID.iOS) {
					App.Current.baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
				} else {
					App.Current.baseDirectory = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "../");
					if (!Directory.Exists (Path.Combine (App.Current.baseDirectory, "share", softwareName))) {
						App.Current.baseDirectory = Path.Combine (App.Current.baseDirectory, "../");
					}
				}
				if (!Directory.Exists (Path.Combine (App.Current.baseDirectory, "share", softwareName)))
					Log.Warning ("Prefix directory not found");
				App.Current.DataDir.Add (App.Current.RelativeToPrefix (Path.Combine ("share", softwareName.ToLower ())));
			}

			if (Utils.OS == OperatingSystemID.Android) {
				home = App.Current.baseDirectory;
			} else if (Utils.OS == OperatingSystemID.iOS) {
				home = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), "..", "Library");
			} else {
				/* Check for the magic file PORTABLE to check if it's a portable version
					* and the config goes in the same folder as the binaries */
				if (File.Exists (Path.Combine (App.Current.baseDirectory, portableFile))) {
					home = App.Current.baseDirectory;
				} else {
					home = Environment.GetEnvironmentVariable (evHome);
					if (home != null && !Directory.Exists (home)) {
						try {
							Directory.CreateDirectory (home);
						} catch (Exception ex) {
							Log.Exception (ex);
							Log.Warning (String.Format (evHome + " {0} not found", home));
							home = null;
						}
					}
					if (home == null) {
						home = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
					}
				}
			}

			App.Current.homeDirectory = Path.Combine (home, softwareName);
			App.Current.configDirectory = App.Current.homeDirectory;

			// Migrate old config directory the home directory so that OS X users can easilly find
			// log files and config files without having to access hidden folders
			if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
				string oldHome = Path.Combine (home, "." + softwareName.ToLower ()); 
				string configFilename = softwareName.ToLower () + "-1.0.config";
				string configFilepath = Path.Combine (oldHome, configFilename);
				if (File.Exists (configFilepath) && !File.Exists (App.Current.ConfigFile)) {
					try {
						File.Move (configFilepath, App.Current.ConfigFile);
					} catch (Exception ex) {
						Log.Exception (ex);
					}
				}
			}
		}

		// copied from OneplayLongomMatch::CoreServices
		static void InitTranslations (string softwareName)
		{
			string localesDir = App.Current.RelativeToPrefix ("share/locale");

			if (!Directory.Exists (localesDir)) {
				var cerbero_prefix = Environment.GetEnvironmentVariable ("CERBERO_PREFIX");
				if (cerbero_prefix != null) {
					localesDir = Path.Combine (cerbero_prefix, "share", "locale");
				} else {
					Log.ErrorFormat ("'{0}' does not exist. This looks like an uninstalled execution." +
					"Define CERBERO_PREFIX.", localesDir);
				}
			}
			/* Init internationalization support */
			Catalog.SetDomain (softwareName.ToLower (), localesDir);
		}

		public Config Config {
			get;
			set;
		}

		public StyleConf Style {
			get {
				if (style == null) {
					style = new StyleConf ();
				}
				return style;
			}
			set {
				style = value;
			}
		}

		public string ConfigFile {
			get {
				string filename = App.Current.SoftwareName.ToLower () + "-1.0.config";
				return Path.Combine (App.Current.ConfigDir, filename);
			}
		}

		public string HomeDir {
			get {
				return homeDirectory;
			}
		}

		public string BaseDir {
			set {
				baseDirectory = value;
			}
		}

		public string ConfigDir {
			set {
				configDirectory = value;
			}
			get {
				return configDirectory;
			}
		}

		public string TemplatesDir {
			get {
				return Path.Combine (DBDir, "templates");
			}
		}

		public string PlayListDir {
			get {
				return Path.Combine (homeDirectory, "playlists");
			}
		}

		public string SnapshotsDir {
			get {
				return Path.Combine (homeDirectory, "snapshots");
			}
		}

		public string VideosDir {
			get {
				return Path.Combine (homeDirectory, "videos");
			}
		}

		public string TempVideosDir {
			get {
				return Path.Combine (configDirectory, "temp");
			}
		}

		public string DBDir {
			get {
				return Path.Combine (homeDirectory, "db");
			}
		}

		public string RelativeToPrefix (string relativePath)
		{
			return Path.Combine (baseDirectory, relativePath);
		}

		#region Properties

		public Image Background {
			get;
			set;
		}

		public string Copyright {
			get;
			set;
		}

		public string License {
			get;
			set;
		}

		public string SoftwareName {
			get;
			set;
		}

		public string SoftwareIconName {
			get;
			set;
		}

		public bool SupportsMultiCamera {
			get;
			set;
		}

		public bool SupportsFullHD {
			get;
			set;
		}

		public bool SupportsActionLinks {
			get;
			set;
		}

		public bool SupportsZoom {
			get;
			set;
		}

		public string LatestVersionURL {
			get;
			set;
		}

		public bool UseGameUnits {
			get;
			set;
		}

		public Version Version {
			get;
			set;
		}

		public string BuildVersion {
			get;
			set;
		}

		public Image FieldBackground {
			get {
				return Resources.LoadImage (Constants.FIELD_BACKGROUND);
			}
		}

		public Image HalfFieldBackground {
			get {
				return Resources.LoadImage (Constants.HALF_FIELD_BACKGROUND);
			}
		}

		public Image HHalfFieldBackground {
			get {
				return Resources.LoadImage (Constants.HHALF_FIELD_BACKGROUND);
			}
		}

		public Image GoalBackground {
			get {
				return Resources.LoadImage (Constants.GOAL_BACKGROUND);
			}
		}

		public string ProjectExtension {
			get;
			set;
		}

		public string DefaultDBName {
			get;
			set;
		}

		#endregion

	}
}
