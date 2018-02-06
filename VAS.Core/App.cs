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
using VAS.Core.Interfaces.MVVMC;
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
		public ILocator<IView> ViewLocator;
		public ILocator<IController> ControllerLocator;
		public DragContext DragContext;
		public ILicenseManager LicenseManager;
		public IHotkeysService HotkeysService;
		public IResourcesLocator ResourcesLocator;
		public IFileSystemManager FileSystemManager;
		public INetworkManager NetworkManager;
		public IApplicationMenu ApplicationMenu;

		public string homeDirectory = ".";
		public string baseDirectory = ".";
		public string configDirectory = ".";

		public App ()
		{
			ZoomLevels = new List<float> { 1.0f, 1.25f, 1.50f, 1.75f, 2.0f, 2.25f, 2.50f, 2.75f, 3.0f, 3.25f, 3.50f, 3.75f, 4.0f };
		}

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
			Current.SoftwareName = softwareName;

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
					Log.Debug ($"Searching for data path in {App.Current.baseDirectory}");
					if (!Directory.Exists (Path.Combine (App.Current.baseDirectory, "share", softwareName))) {
						App.Current.baseDirectory = Path.Combine (App.Current.baseDirectory, "../");
						Log.Debug ($"Not found, searching for data path in {App.Current.baseDirectory}");
					}
				}
				string dataDir = App.Current.RelativeToPrefix (Path.Combine ("share", softwareName.ToLower ()));
				if (!Directory.Exists (dataDir))
					Log.Warning ($"Prefix directory not found in {dataDir}");
				App.Current.DataDir.Add (dataDir);
			}
			Log.Debug ($"DataDir = [{string.Join (", ", App.Current.DataDir)}]");

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
			if (!Directory.Exists (App.Current.homeDirectory)) {
				Directory.CreateDirectory (App.Current.homeDirectory);
			}

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

			InitTranslations (softwareName);
			InitDependencies ();
			InitVersion ();
		}

		internal static void InitDependencies ()
		{
			App.Current.Keyboard = new Keyboard ();
			App.Current.ViewLocator = new Locator<IView> ();
			App.Current.ControllerLocator = new Locator<IController> ();
			App.Current.StateController = new StateController ();
			App.Current.DependencyRegistry = new Registry ("App Registry");
			App.Current.EventsBroker = new EventsBroker ();
			App.Current.Device = new Core.Device ();
			App.Current.KPIService = new KpiService ();
			App.Current.DragContext = new DragContext ();
			App.Current.ResourcesLocator = new ResourcesLocator ();
			App.Current.FileSystemManager = new FileSystemManager ();
			App.Current.NetworkManager = new NetworkManager ();
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
			Catalog.Init (softwareName.ToLower (), localesDir);
		}

		static void InitVersion ()
		{
			Current.Version = Current.Device.Version;
			Current.BuildVersion = Current.Device.BuildVersion;
		}

		static void ConfigureEnvVariables ()
		{
			Environment.SetEnvironmentVariable ("GDK_PIXBUF_MODULEDIR",
				App.Current.RelativeToPrefix ("lib/gdk-pixbuf-2.0/2.10.0/loaders"));
			Environment.SetEnvironmentVariable ("GDK_PIXBUF_MODULE_FILE",
				App.Current.RelativeToPrefix ("lib/gdk-pixbuf-2.0/2.10.0/loaders.cache"));
			Environment.SetEnvironmentVariable ("FONTCONFIG_PATH",
				App.Current.RelativeToPrefix ("etc/fonts"));
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

		public string Website {
			get;
			set;
		}

		public string Translators {
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

		/// <summary>
		/// Gets or sets the step value list for the videoplayer.
		/// </summary>
		/// <value>The step list.</value>
		public List<int> StepList {
			get;
			set;
		}

		public double DefaultRate {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a list with the possible zoom levels for the video player.
		/// </summary>
		/// <value>The zoom levels.</value>
		public List<float> ZoomLevels {
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

