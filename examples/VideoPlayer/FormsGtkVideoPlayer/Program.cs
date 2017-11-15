//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using VAS.Core.Addins;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Drawing.Cairo;
using VAS.Multimedia.Utils;
using VAS.Core;
using VAS.UI.Helpers;
using VAS.Video;
using VAS.UI;
using VAS.UI.Dialog;
using VideoPlayer.State;
using VAS.UI.Forms;
using Xamarin.Forms;
using VideoPlayer;
using VAS.Core.MVVMC;

namespace FormsGtkVideoPlayer
{
	class MainClass
	{
		[DllImport("libX11", CallingConvention = CallingConvention.Cdecl)]
		private static extern int XInitThreads();

		static IMultimediaBackend multimediaBackend;

		public static void Main(string[] args)
		{
			Init();
			Shutdown();
		}

		static void Init()
		{
			// Replace the current synchronization context with a GTK synchronization context
			// that continues tasks in the main UI thread instead of a random thread from the pool.
			SynchronizationContext.SetSynchronizationContext(new GtkSynchronizationContext());
			GLib.ExceptionManager.UnhandledException += HandleException;
			TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;
			App.Init();
			InitGtk();
			InitDependencies();
			var splashScreen = new SplashScreen(App.Current.ResourcesLocator.LoadImage(VideoPlayerConstants.SPLASH));
			splashScreen.Show();
			Gtk.Application.Invoke(async (s, e) => await AsyncInit(splashScreen));
			Gtk.Application.Run();
		}

		static async Task AsyncInit(SplashScreen splashScreen)
		{
			IProgressReport progress = splashScreen;

			try
			{
				Task gstInit = Task.Factory.StartNew(() => InitGStreamer(progress));
				App.Current.StartServices();

				// Wait for the GStreamer initialization, but we can also wait for other
				// initialization tasks in the background
				try
				{
					await Task.WhenAll(gstInit);
				}
				catch (AggregateException ae)
				{
					throw ae.Flatten();
				}

				splashScreen.Destroy();
				ConfigureOSXApp();
				Scanner.ScanViews(App.Current.ViewLocator);
				InitFormsApp();
				await App.Current.StateController.SetHomeTransition(FileSelectionState.NAME, null);
			}
			catch (Exception ex)
			{
				ProcessExecutionError(ex);
			}
		}

		static void InitFormsApp()
		{
			Forms.Init();
			var app = new FormsApp();
			var window = new GtkFormsWindow();
			window.LoadApplication(app);
			window.Show();
			App.Current.GUIToolkit = new FormsGtkUIToolkit(window);
		}

		static void Shutdown()
		{
			try
			{
				if (multimediaBackend != null)
				{
					multimediaBackend.Shutdown();
				}
				App.Current.KeyContextManager.Dispose();
				App.Current.StopServices();
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
		}

		static void InitDependencies()
		{
			App.Current.DrawingToolkit = new CairoBackend();
			App.Current.MultimediaToolkit = new MultimediaToolkit();
			App.Current.Navigation = new FormsNavigation();
			App.Current.Dialogs = new FormsGtkDialogs();
		}

		static void InitGStreamer(IProgressReport progress)
		{
			Guid id = Guid.NewGuid();
			progress.Report(0.1f, "Initializing GStreamer", id);
			GStreamer.Init();
			progress.Report(1f, "GStreamer initialized", id);
		}

		static void ConfigureOSXApp()
		{
			if (Utils.OS == OperatingSystemID.OSX)
			{
				GtkOSXApplication app;

				app = new GtkOSXApplication();
				app.NSApplicationBlockTermination += async (o, a) =>
				{
					a.RetVal = await App.Current.GUIToolkit.Quit();
				};

				/*
				MainWindow window = App.Current.GUIToolkit.MainController as MainWindow;
				MenuItem quit;
				quit = window.QuitMenu;
				quit.Visible = false;
				app.SetMenuBar (window.Menu);
				app.InsertAppMenuItem (window.AboutMenu, 0);
				app.InsertAppMenuItem (new SeparatorMenuItem (), 1);
				app.InsertAppMenuItem (window.PreferencesMenu, 2);
				app.InsertAppMenuItem (new SeparatorMenuItem (), 3);
				app.InsertAppMenuItem (window.CheckForUpdatesMenu, 4);
				window.Menu.Visible = false;
				*/
				app.UseQuartzAccelerators = false;
				app.Ready();
			}
		}

		static void InitGtk()
		{
			Environment.SetEnvironmentVariable("GTK_EXE_PREFIX", App.Current.baseDirectory);

			try
			{
				Rc.DefaultFiles = new string[] { Utils.GetDataFilePath(Path.Combine("theme", "gtk-2.0", "gtkrc")) };
			}
			catch (Exception ex)
			{

			}
			App.Current.Style = StyleConf.Load(Utils.GetDataFilePath(Path.Combine("theme", "dark.json")));

			/* We are having some race condition with XCB resulting on an invalid
			 * message and thus an abort of the program, we better activate the
			 * thread sae X11
			 */
			if (Utils.OS == OperatingSystemID.Linux)
				XInitThreads();

			Gtk.Application.Init();

			IconTheme.Default.PrependSearchPath(Utils.GetDataDirPath("icons"));
		}

		static void HandleException(GLib.UnhandledExceptionArgs args)
		{
			ProcessExecutionError((Exception)args.ExceptionObject);
		}

		static void HandleUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			Log.Exception(e.Exception);
		}

		static void ProcessExecutionError(Exception ex)
		{
			Log.Exception(ex);
			MessagesHelpers.ErrorMessage(null,
										  Catalog.GetString("The application has finished with an unexpected error."));
			Gtk.Application.Quit();
			return;
		}
	}
}