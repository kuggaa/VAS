//
//   Copyright (C) 2016 Fluendo S.A.
//
using System;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gtk;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Filters;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using Image = VAS.Core.Common.Image;

namespace VAS.UI
{
	/// <summary>
	/// IGUIToolkit common implementation
	/// </summary>
	public abstract class GUIToolkit : IGUIToolkit
	{
		protected Gtk.Window MainWindow {
			get {
				return mainWindow; 
			}
			set {
				mainWindow = value;
			}
		}

		Gtk.Window mainWindow;
		Registry registry;

		protected GUIToolkit ()
		{
			registry = new Registry ("GUI backend");
			Scanner.ScanViews (App.Current.ViewLocator);
		}

		//FIXME: for compatibility with LongoMatch
		public virtual IMainController MainController {	get; }

		//FIXME: for compatibility with LongoMatch
		public virtual IRenderingStateBar RenderingStateBar { get; }

		public virtual bool FullScreen { 
			protected get;
			set; 
		}

		public virtual void Register <I, C> (int priority)
		{
			registry.Register<I, C> (priority);
		}

		public virtual IPlayerView GetPlayerView ()
		{
			return registry.Retrieve<IPlayerView> ();
		}

		public abstract List<EditionJob> ConfigureRenderingJob (Playlist playlist);

		public abstract void ExportFrameSeries (Project openedProject, TimelineEvent play, string snapshotsDir);

		public abstract Task EditPlay (TimelineEvent play, Project project, bool editTags, bool editPos, bool editPlayers, 
		                               bool editNotes);

		public abstract void DrawingTool (Image image, TimelineEvent play, FrameDrawing drawing,
		                                  CameraConfig camConfig, Project project);

		public abstract Project ChooseProject (List<Project> projects);

		public abstract void SelectProject (List<Project> projects);

		public abstract void OpenCategoriesTemplatesManager ();

		public abstract void OpenTeamsTemplatesManager ();

		public abstract void OpenProjectsManager (Project openedProject);

		public abstract void OpenPreferencesEditor ();

		public abstract void OpenDatabasesManager ();

		public abstract void ManageJobs ();

		public abstract void Welcome ();

		public abstract void LoadPanel (IPanel panel);

		public abstract void CreateNewProject (Project project = null);

		public abstract void ShowProjectStats (Project project);

		public abstract string RemuxFile (string inputFile, string outputFile, VideoMuxerType muxer);

		public abstract void OpenProject (Project project, ProjectType projectType, 
		                                  CaptureSettings props, EventsFilter filter,
		                                  out IAnalysisWindowBase analysisWindow);

		public abstract void CloseProject ();

		public abstract EndCaptureResponse EndCapture (bool isCapturing);

		public abstract bool SelectMediaFiles (MediaFileSet fileSet);

		public virtual void Quit ()
		{
			Log.Information ("Quit application");
			Gtk.Application.Quit ();
		}

		public abstract HotKey SelectHotkey (HotKey hotkey, object parent = null);

		public void Invoke (EventHandler handler)
		{
			Gtk.Application.Invoke (handler);
		}

		public abstract Task<bool> CreateNewTemplate<T> (IList<T> availableTemplates, string defaultName,
		                                                 string countText, string emptyText,
		                                                 CreateEvent<T> evt) where T: ITemplate;

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
	}
}

