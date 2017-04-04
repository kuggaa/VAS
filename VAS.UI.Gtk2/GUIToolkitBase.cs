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
using VAS.Core.Filters;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;
using VAS.Drawing;
using VAS.Drawing.CanvasObjects.Blackboard;
using VAS.UI.Dialog;
using VAS.UI.Helpers;
using Image = VAS.Core.Common.Image;

namespace VAS.UI
{
	/// <summary>
	/// IGUIToolkit common implementation
	/// </summary>
	public abstract class GUIToolkitBase : IGUIToolkit
	{
		Gtk.Window mainWindow;
		Registry registry;

		protected GUIToolkitBase ()
		{
			registry = new Registry ("GUI backend");
			Scanner.ScanViews (App.Current.ViewLocator);
			RegistryCanvasFromDrawables ();
		}

		protected Gtk.Window MainWindow {
			get {
				return mainWindow;
			}
			set {
				mainWindow = value;
			}
		}

		//FIXME: for compatibility with LongoMatch
		public virtual IMainController MainController { get; }

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

		public virtual void Register<I, C> (int priority)
		{
			registry.Register<I, C> (priority);
		}

		public virtual IVideoPlayerView GetPlayerView ()
		{
			return registry.Retrieve<IVideoPlayerView> ();
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

		public abstract void ExportFrameSeries (Project openedProject, TimelineEvent play, string snapshotsDir);

		public abstract Project ChooseProject (List<Project> projects);

		public abstract void LoadPanel (IPanel panel);

		public abstract void ShowProjectStats (Project project);

		public abstract string RemuxFile (string inputFile, string outputFile, VideoMuxerType muxer);

		public abstract EndCaptureResponse EndCapture (bool isCapturing);

		public abstract bool SelectMediaFiles (MediaFileSet fileSet);

		public virtual async Task<bool> Quit ()
		{
			if (!await App.Current.StateController.MoveToHome ()) {
				return false;
			}
			Log.Information ("Quit application");
			Application.Quit ();
			return true;
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

		public void Invoke (EventHandler handler)
		{
			Application.Invoke (handler);
		}

		public abstract Task<bool> CreateNewTemplate<T> (IList<T> availableTemplates, string defaultName,
														 string countText, string emptyText,
														 CreateEvent<T> evt) where T : ITemplate;

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

		protected void ShowModalWindow (IPanel panel, IPanel parent)
		{
			if (panel is Gtk.Dialog) {
				(panel as Gtk.Dialog).TransientFor = ((Bin)parent).Toplevel as Window;
				(panel as Gtk.Dialog).DeleteEvent += ModalWindowDeleteEvent;
				panel.OnLoad ();
			} else {
				ExternalWindow modalWindow = new ExternalWindow ();
				modalWindow.DefaultWidth = (panel as Gtk.Bin).WidthRequest;
				modalWindow.DefaultHeight = (panel as Gtk.Bin).HeightRequest;
				modalWindow.Title = panel.Title;
				modalWindow.Modal = true;
				modalWindow.TransientFor = ((Bin)parent).Toplevel as Window;
				modalWindow.DeleteEvent += ModalWindowDeleteEvent;
				Widget widget = panel as Gtk.Widget;
				modalWindow.Add (widget);
				modalWindow.SetPosition (WindowPosition.CenterOnParent);
				modalWindow.ShowAll ();
				panel.OnLoad ();
			}
		}

		protected async void ModalWindowDeleteEvent (object o, DeleteEventArgs args)
		{
			if (args.Event.Window != null) {
				args.RetVal = !await App.Current.StateController.MoveBack ();
			}
		}

		protected void RemoveModalPanelAndWindow (IPanel panel)
		{
			panel.OnUnload ();
			((Bin)panel).Toplevel.DeleteEvent -= ModalWindowDeleteEvent;
			((Bin)panel).Toplevel.Destroy ();
			System.GC.Collect ();
		}

		void RegistryCanvasFromDrawables ()
		{
			CanvasFromDrawableObjectRegistry.AddMapping (typeof (Counter), typeof (CounterObject), "VAS.Drawing");
			CanvasFromDrawableObjectRegistry.AddMapping (typeof (Cross), typeof (CrossObject), "VAS.Drawing");
			CanvasFromDrawableObjectRegistry.AddMapping (typeof (Ellipse), typeof (EllipseObject), "VAS.Drawing");
			CanvasFromDrawableObjectRegistry.AddMapping (typeof (Line), typeof (LineObject), "VAS.Drawing");
			CanvasFromDrawableObjectRegistry.AddMapping (typeof (Quadrilateral), typeof (QuadrilateralObject), "VAS.Drawing");
			CanvasFromDrawableObjectRegistry.AddMapping (typeof (Rectangle), typeof (RectangleObject), "VAS.Drawing");
			CanvasFromDrawableObjectRegistry.AddMapping (typeof (Text), typeof (TextObject), "VAS.Drawing");
		}
	}
}

