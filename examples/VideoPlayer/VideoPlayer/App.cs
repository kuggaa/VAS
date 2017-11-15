using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.MVVMC;
using VAS.Services;
using VideoPlayer.State;

namespace VideoPlayer
{
	public class App : VAS.App
	{
		Config config;

		new public static App Current {
			get {
				return (App)VAS.App.Current;
			}
			set {
				VAS.App.Current = value;
			}
		}

		public static void Init ()
		{
			App app = new App ();

			Init (app, "LGM_UNINSTALLED", VideoPlayerConstants.SOFTWARE_NAME, VideoPlayerConstants.SOFTWARE_ICON_NAME, "", "VIDEO_PLAYER_HOME");
			App.Current.DataDir.Add (Path.Combine (Path.GetFullPath ("."), "../data"));
			App.Current.InitConstants ();
			Current.ResourcesLocator.Register (Assembly.GetExecutingAssembly ());

			/* Redirects logs to a file */
			Log.SetLogFile (App.Current.LogFile);
			Log.Information ("Starting " + VideoPlayerConstants.SOFTWARE_NAME);
			Log.Information (Utils.SysInfo);

			// Scan controllers
			Scanner.ScanAll();
			VASServicesInit.Init();

			// Initialize Hotkeys
			App.Current.HotkeysService = new HotkeysService ();
			App.Current.RegisterService (App.Current.HotkeysService);
			GeneralUIHotkeys.RegisterDefaultHotkeys ();
			PlaybackHotkeys.RegisterDefaultHotkeys ();
			DrawingToolHotkeys.RegisterDefaultHotkeys ();

			App.Current.RegisterStates ();

			App.Current.LicenseLimitationsService = new VideoPlayerLicenseLimitationsService ();
		}

		public new Config Config {
			get {
				return config;
			}
			set {
				config = value;
				base.Config = config;
			}
		}

		protected override VAS.Config CreateConfig ()
		{
			return new Config ();
		}

		void InitConstants ()
		{
			Current.Copyright = VideoPlayerConstants.COPYRIGHT;
			Current.License = VideoPlayerConstants.LICENSE;
			Current.LowerRate = 1;
			Current.UpperRate = 30;
			Current.RatePageIncrement = 3;
			Current.RateList = new List<double> { 0.1, 0.25, 0.50, 0.75, 1, 2, 3, 4, 5 };
			Current.StepList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60 };
			Current.DefaultRate = 25;
			/* Not needed for this app but might be needed for others
			Current.LatestVersionURL = Constants.LATEST_VERSION_URL;
			Current.DefaultDBName = Constants.DEFAULT_DB_NAME;
			Current.ProjectExtension = Constants.PROJECT_EXT;
			Current.Website = Constants.WEBSITE;
			Current.Translators = Constants.TRANSLATORS;
			*/
		}

		void RegisterStates ()
		{
			App.Current.StateController.Register (FileSelectionState.NAME, () => new FileSelectionState ());
			App.Current.StateController.Register (VideoPlayerState.NAME, () => new VideoPlayerState ());
		}
	}
}
