using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.License;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.MVVMC;
using VAS.KPI;

namespace VAS
{
	public abstract class App
	{
		static int mainThreadId;

		/* State */
		public IGUIToolkit GUIToolkit;
		public INavigation Navigation;
		public IMultimediaToolkit MultimediaToolkit;
		public IDrawingToolkit DrawingToolkit;
		public IDialogs Dialogs;
		public IKeyboard Keyboard;
		public IDevice Device;
		public EventsBroker EventsBroker;
		public IStateController StateController;
		public Registry DependencyRegistry;
		public IStorageManager DatabaseManager;
		public IKpiService KPIService;
		public IJobsManager JobsManager;
		public ViewLocator ViewLocator;
		public ControllerLocator ControllerLocator;
		public DragContext DragContext;
		public ILicenseManager LicenseManager;
		public IHotkeysService HotkeysService;
		public IResourcesLocator ResourcesLocator;

		public string homeDirectory = ".";
		public string baseDirectory = ".";
		public string configDirectory = ".";

		public KeyContextManager KeyContextManager {
			get {
				return KeyContextManager.Instance;
			}
		}

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

		public static bool IsMainThread {
			get {
				return Thread.CurrentThread.ManagedThreadId == mainThreadId;
			}
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

			mainThreadId = Thread.CurrentThread.ManagedThreadId;

			Current = appInit;

			string home = null;

			App.Current.Uninstalled = Environment.GetEnvironmentVariable (evUninstalled) != null;

			if (App.Current.Uninstalled) {
				App.Current.baseDirectory = GetPrefixPath ();
				App.Current.DataDir.Add (Path.Combine (Path.GetFullPath ("."), "../VAS/data"));
				App.Current.DataDir.Add (Path.Combine (Path.GetFullPath ("."), "../data"));
				ConfigureEnvVariables ();
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

			InitVersion ();
			InitTranslations (softwareName);
			InitDependencies ();
		}

		internal static void InitDependencies ()
		{
			App.Current.Keyboard = new Keyboard ();
			App.Current.ViewLocator = new ViewLocator ();
			App.Current.ControllerLocator = new ControllerLocator ();
			App.Current.StateController = new StateController ();
			App.Current.DependencyRegistry = new Registry ("App Registry");
			App.Current.EventsBroker = new EventsBroker ();
			App.Current.Device = new Core.Device ();
			App.Current.KPIService = new KpiService ();
			App.Current.DragContext = new DragContext ();
			App.Current.ResourcesLocator = new ResourcesLocator ();
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

		static void InitVersion ()
		{
			Assembly assembly = Assembly.GetEntryAssembly () ?? Assembly.GetExecutingAssembly ();
			Current.Version = assembly.GetName ().Version;
			var attribute = assembly.
									GetCustomAttributes (typeof (AssemblyInformationalVersionAttribute), false).
									FirstOrDefault ();
			if (attribute != null) {
				Current.BuildVersion = (attribute as AssemblyInformationalVersionAttribute).InformationalVersion;
			} else {
				Current.BuildVersion = Current.Version.ToString ();
			}
		}

		static void ConfigureEnvVariables ()
		{
			Environment.SetEnvironmentVariable ("GDK_PIXBUF_MODULEDIR",
				App.Current.RelativeToPrefix ("lib/gdk-pixbuf-2.0/2.10.0/loaders"));
			Environment.SetEnvironmentVariable ("GDK_PIXBUF_MODULE_FILE",
				App.Current.RelativeToPrefix ("lib/gdk-pixbuf-2.0/2.10.0/loaders.cache"));
		}

		static string GetPrefixPath ()
		{
			// The runtime prefix is always defined in the PATH environment variable,
			// either because we choosed a custom .NET Runtime in Xamarin Studio, we are running
			// in cerbero's shell or we are executing the app as an application bundle with
			// the PATH configured to have our prefix. Here we iterate over all PATH entries
			// until we find it, which is normally
			// libdir and we use it to infer the prefix path, unless we are in a cerbero shell. We iterate
			// over all the directories to find the prefix.
			foreach (var pathEntry in ((string)Environment.GetEnvironmentVariables () ["PATH"]).Split (':')) {
				var prefix = Path.Combine (pathEntry, "../");
				if (Directory.Exists (Path.Combine (prefix, "lib", "gdk-pixbuf-2.0"))) {
					return prefix;
				}
			}
			throw new Exception ($"No potential prefix was found in $PATH." +
								 "Make sure your Run Configuration is using the correct .Net Runtime," +
								 "or the environment is configured correctly.");
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

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:VAS.App"/> is running uninstalled.
		/// </summary>
		/// <value><c>true</c> if uninstalled; otherwise, <c>false</c>.</value>
		public bool Uninstalled {
			get;
			private set;
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
				return App.Current.ResourcesLocator.LoadImage (Constants.FIELD_BACKGROUND);
			}
		}

		public Image HalfFieldBackground {
			get {
				return App.Current.ResourcesLocator.LoadImage (Constants.HALF_FIELD_BACKGROUND);
			}
		}

		public Image HHalfFieldBackground {
			get {
				return App.Current.ResourcesLocator.LoadImage (Constants.HHALF_FIELD_BACKGROUND);
			}
		}

		public Image GoalBackground {
			get {
				return App.Current.ResourcesLocator.LoadImage (Constants.GOAL_BACKGROUND);
			}
		}

		public Image WatermarkImage {
			get;
			set;
		}

		public string ProjectExtension {
			get;
			set;
		}

		public string DefaultDBName {
			get;
			set;
		}

		public double UpperRate {
			get;
			set;
		}

		public double LowerRate {
			get;
			set;
		}

		public double RatePageIncrement {
			get;
			set;
		}

		public List<double> RateList {
			get;
			set;
		}

		public double DefaultRate {
			get;
			set;
		}

		public ILicenseLimitationsService LicenseLimitationsService {
			get;
			set;
		}

		#endregion

	}
}

