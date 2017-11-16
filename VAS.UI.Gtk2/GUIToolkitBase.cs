//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Gtk;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.UI.Dialog;
using VAS.UI.Helpers;
using VAS.UI.Multimedia;

namespace VAS.UI
{
	/// <summary>
	/// IGUIToolkit common implementation
	/// </summary>
	public abstract class GUIToolkitBase : IGUIToolkit, INavigation
	{
		Gtk.Window mainWindow;
		Registry registry;

		protected GUIToolkitBase ()
		{
			registry = new Registry ("GUI backend");
			Scanner.ScanAll ();
			DrawingInit.Init ();
			VASUIMultimediaInit.Init ();
			RegistryCanvasFromDrawables ();
		}

		protected Gtk.Window MainWindow {
			get {
				return mainWindow;
			}
			private set {
				mainWindow = value;
			}
		}

		public virtual IMainController MainController {
			get {
				return (IMainController)MainWindow;
			}
		}

		public bool FullScreen {
			set {
				if (MainWindow != null) {
					if (value)
						MainWindow.GdkWindow.Fullscreen ();
					else
						MainWindow.GdkWindow.Unfullscreen ();
				}
			}
		}

		public float DeviceScaleFactor {
			get {
				// On Windows, the device can return non-round values, 1.2, 1.5, but we always round up.
				double scaleFactor = 1;
				Widget widget = MainWindow as Widget;
				if (widget != null) {
					scaleFactor = widget.GetScaleFactor ();
				} else {
					//  screen
					Gdk.Screen screen = Gdk.Display.Default.DefaultScreen;
					scaleFactor = screen.GetScaleFactor (0);
				}
				return (float)(Math.Ceiling (scaleFactor));
			}
		}

		public void Init (Window mainWindow = null)
		{
			if (mainWindow == null) {
				mainWindow = new MainWindow ();
			}
			mainWindow.Hide ();
			MainWindow = mainWindow;
		}

		public virtual List<EditionJob> ConfigureRenderingJob (Playlist playlist)
		{
			VideoEditionProperties vep;
			List<EditionJob> jobs = new List<EditionJob> ();
			int response;

			Log.Information ("Configure rendering job");
			if (playlist.Elements.Count == 0) {
				App.Current.Dialogs.WarningMessage (Catalog.GetString ("The playlist you want to render is empty."));
				return null;
			}

			vep = new VideoEditionProperties (MainWindow as Gtk.Window);
			vep.Playlist = playlist;
			response = vep.Run ();
			while (response == (int)ResponseType.Ok) {
				if (!vep.SplitFiles && vep.EncodingSettings.OutputFile == "") {
					App.Current.Dialogs.WarningMessage (Catalog.GetString ("Please, select a video file."));
					response = vep.Run ();
				} else if (vep.SplitFiles && vep.OutputDir == null) {
					App.Current.Dialogs.WarningMessage (Catalog.GetString ("Please, select an output directory."));
					response = vep.Run ();
				} else {
					break;
				}
			}
			if (response == (int)ResponseType.Ok) {
				if (!vep.SplitFiles) {
					jobs.Add (new EditionJob (playlist, vep.EncodingSettings));
				} else {
					int i = 0;
					foreach (IPlaylistElement play in playlist.Elements) {
						EncodingSettings settings;
						Playlist pl;
						string name, ext, filename;

						settings = vep.EncodingSettings;
						pl = new Playlist ();
						if (play is PlaylistPlayElement) {
							name = (play as PlaylistPlayElement).Play.Name;
							ext = settings.EncodingProfile.Extension;
						} else {
							name = "image";
							ext = "png";
						}
						filename = String.Format ("{0}-{1}.{2}", i.ToString ("d4"), name, ext);

						pl.Elements.Add (play);
						settings.OutputFile = Path.Combine (vep.OutputDir, filename);
						jobs.Add (new EditionJob (pl, settings));
						i++;
					}
				}
			}
			vep.Destroy ();
			return jobs;
		}

		public abstract void ExportFrameSeries (TimelineEvent play, string snapshotsDir);

		public abstract Project ChooseProject (List<Project> projects);

		public abstract void ShowProjectStats (Project project);

		public abstract string RemuxFile (string inputFile, string outputFile, VideoMuxerType muxer);

		public abstract EndCaptureResponse EndCapture (bool isCapturing);

		public abstract bool SelectMediaFiles (MediaFileSet fileSet);

		public virtual async Task<bool> Quit ()
		{
			if (!await App.Current.StateController.MoveToHome (true)) {
				return false;
			}
			Log.Information ("Quit application");
			Application.Quit ();
			return true;
		}

		/// <summary>
		/// Pushes the specified panel to show it. This task does not finish until the panel is shown.
		/// </summary>
		/// <returns>The view.</returns>
		/// <param name="panel">Panel.</param>
		public virtual bool LoadPanel (IPanel panel)
		{
			return MainController.SetPanel (panel);
		}

		public HotKey SelectHotkey (HotKey hotkey, object parent = null)
		{
			HotKeySelectorDialog dialog;
			Window w;

			w = parent != null ? (parent as Widget).Toplevel as Window : MainWindow;
			dialog = new HotKeySelectorDialog (w);
			if (dialog.Run () == (int)ResponseType.Ok) {
				hotkey = dialog.HotKey;
			} else {
				hotkey = null;
			}
			dialog.Destroy ();
			return hotkey;
		}

		/// <summary>
		/// Queues the handler in the main thread. It returns as soon as the handler is queued.
		/// </summary>
		/// <param name="handler">Handler.</param>
		public void Invoke (EventHandler handler)
		{
			if (App.Current.IsMainThread) {
				Log.Verbose ("Invoke called from the main thread");
			}
			Application.Invoke (handler);
		}

		/// <summary>
		/// Invokes the handler in the main thread asynchronously.
		/// </summary>
		/// <param name="handler">Handler.</param>
		public async Task<T> Invoke<T> (Func<Task<T>> handler)
		{
			if (App.Current.IsMainThread) {
				Log.Verbose ("Invoke called from the main thread");
				return await handler ();
			}
			TaskCompletionSource<T> tcs = new TaskCompletionSource<T> ();
			Application.Invoke (async (sender, e) => {
				try {
					T res = await handler ();
					tcs.SetResult (res);
				} catch (Exception ex) {
					Log.Exception (ex);
					tcs.SetException (ex);
				}
			});
			return await tcs.Task;
		}


		public abstract Task<bool> CreateNewTemplate<T> (IList<T> availableTemplates, string defaultName,
														 string countText, string emptyText,
														 CreateEvent<T> evt) where T : ITemplate;

		public void RunLoop (Func<bool> condition)
		{
			while (!condition ()) {
				if (Application.EventsPending ()) {
					Application.RunIteration ();
				}
			}
		}

		public Widget GetParentWidget (object parent)
		{
			if (parent is Widget) {
				return parent as Widget;
			}
			if (MainWindow != null && MainWindow.Visible) {
				return MainWindow;
			}
			return null;
		}

		public Task<bool> Push (IPanel panel)
		{
			var tcs = new TaskCompletionSource<bool> ();
			bool taskResult = LoadPanel (panel);
			Application.Invoke ((sender, e) => SetTaskCompletionResult (tcs, taskResult));
			return tcs.Task;
		}

		public Task<bool> Pop (IPanel panel)
		{
			// In Gtk+ poping a panel is equivalent to replacing the current panel with the previous panel
			// in the stack
			return Push (panel);
		}

		public Task PushModal (IPanel panel, IPanel parent)
		{
			var tcs = new TaskCompletionSource<bool> ();
			bool taskResult = ShowModalWindow (panel, parent);
			Application.Invoke ((sender, e) => SetTaskCompletionResult (tcs, taskResult));
			return tcs.Task;
		}

		public Task PopModal (IPanel panel)
		{
			RemoveModalPanelAndWindow (panel);
			return AsyncHelpers.Return ();
		}

		protected bool ShowModalWindow (IPanel panel, IPanel parent)
		{
			var dialog = panel as Gtk.Dialog;
			if (dialog != null) {
				dialog.TransientFor = ((Bin)parent).Toplevel as Window;
				dialog.Response += HandleModalWindowResponse;
				dialog.Center ();
				panel.OnLoad ();
			} else {
				ExternalWindow modalWindow = new ExternalWindow ();
				modalWindow.Resizable = false;
				modalWindow.DefaultWidth = (panel as Gtk.Bin).WidthRequest;
				modalWindow.DefaultHeight = (panel as Gtk.Bin).HeightRequest;
				modalWindow.Title = panel.Title;
				modalWindow.Modal = true;
				modalWindow.TransientFor = ((Bin)parent).Toplevel as Window;
				modalWindow.DeleteEvent += HandleModalWindowDeleteEvent;
				Widget widget = panel as Gtk.Widget;
				modalWindow.Add (widget);
				modalWindow.SetPosition (WindowPosition.CenterOnParent);
				modalWindow.ShowAll ();
				panel.OnLoad ();
			}
			return true;
		}

		private async void HandleModalWindowResponse (object o, ResponseArgs args)
		{
			args.RetVal = !await App.Current.StateController.MoveBack ();
		}

		private async void HandleModalWindowDeleteEvent (object o, DeleteEventArgs args)
		{
			args.RetVal = !await App.Current.StateController.MoveBack ();
		}

		protected void RemoveModalPanelAndWindow (IPanel panel)
		{
			panel.OnUnload ();
			if (panel is Gtk.Dialog dialog) {
				dialog.Response -= HandleModalWindowResponse;
				dialog.Destroy ();
			} else if (panel is Bin bin) {
				bin.Toplevel.DeleteEvent -= HandleModalWindowDeleteEvent;
				bin.Toplevel.Destroy ();
			}
			System.GC.Collect ();
		}

		/// <summary>
		/// Sets the task completion result as true when the main thread has finished showing the panel.
		/// </summary>
		/// <returns><c>true</c>, if task completion result was set, <c>false</c> otherwise.</returns>
		void SetTaskCompletionResult (TaskCompletionSource<bool> tcs, bool taskResult)
		{
			tcs.SetResult (taskResult);
		}
	}
}

