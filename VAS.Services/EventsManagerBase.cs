// EventsManagerBase.cs
//
//  Copyright (C2007-2009 Andoni Morales Alastruey
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
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Linq;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.Filters;
using VAS.Core.Store.Templates;
using System.IO;

namespace VAS.Services
{
	public class CoreEventsManager : IService
	{
		/* Current play loaded. null if no play is loaded */
		protected TimelineEvent loadedPlay;
		/* current project in use */
		protected Project openedProject;
		protected ProjectType projectType;
		protected EventsFilter filter;
		protected IAnalysisWindowBase analysisWindow;
		protected IPlayerController player;
		protected ICapturerBin capturer;
		protected IFramesCapturer framesCapturer;

		protected void HandleOpenedProjectChanged (Project project, ProjectType projectType,
		                                           VAS.Core.Filters.EventsFilter filter, IAnalysisWindowBase analysisWindow)
		{
			this.openedProject = project;
			this.projectType = projectType;
			this.filter = filter;

			if (project == null) {
				if (framesCapturer != null) {
					framesCapturer.Dispose ();
					framesCapturer = null;
				}
				return;
			}

			if (projectType == ProjectType.FileProject) {
				framesCapturer = Config.MultimediaToolkit.GetFramesCapturer ();
				framesCapturer.Open (openedProject.FileSet.First ().FilePath);
			}
			this.analysisWindow = analysisWindow;
			player = analysisWindow.Player;
			capturer = analysisWindow.Capturer;
		}

		void Save (Project project)
		{
			if (Config.AutoSave) {
				Config.DatabaseManager.ActiveDB.Store (project);
			}
		}

		protected virtual void DeletePlays (List<TimelineEvent> plays, bool update = true)
		{
			Log.Debug (plays.Count + " plays deleted");
			analysisWindow.DeletePlays (plays.ToList ());
			openedProject.RemoveEvents (plays);
			if (projectType == ProjectType.FileProject) {
				Save (openedProject);
			}
			if (loadedPlay != null && plays.Contains (loadedPlay)) {
				Config.EventsBroker.EmitLoadEvent (null);
			}
			filter.Update ();
		}

		protected virtual void HandleShowFullScreenEvent (bool fullscreen)
		{
			Config.GUIToolkit.FullScreen = fullscreen;
		}

		protected virtual void HandlePlayLoaded (TimelineEvent play)
		{
			loadedPlay = play;
		}

		protected virtual void HandlePlaylistElementLoaded (Playlist playlist, IPlaylistElement element)
		{
			if (element is PlaylistPlayElement) {
				loadedPlay = (element as PlaylistPlayElement).Play;
			} else {
				loadedPlay = null;
			}
		}

		protected virtual void HandleDetach ()
		{
			if (analysisWindow != null) {
				analysisWindow.DetachPlayer ();
			}
		}

		protected virtual void HandleTagSubcategoriesChangedEvent (bool tagsubcategories)
		{
			Config.FastTagging = !tagsubcategories;
		}

		protected virtual void HandleDrawFrame (TimelineEvent play, int drawingIndex, CameraConfig camConfig, bool current)
		{
			Image pixbuf;
			FrameDrawing drawing = null;
			Time pos;

			player.Pause (true);
			if (play == null) {
				play = loadedPlay;
			}
			if (play != null) {
				if (drawingIndex == -1) {
					drawing = new FrameDrawing {
						Render = player.CurrentTime,
						CameraConfig = camConfig,
						RegionOfInterest = camConfig.RegionOfInterest.Clone (),
					};
				} else {
					drawing = play.Drawings [drawingIndex];
				}
				pos = drawing.Render;
			} else {
				pos = player.CurrentTime;
			}

			if (framesCapturer != null && !current) {
				if (camConfig.Index > 0) {
					IFramesCapturer auxFramesCapturer;
					auxFramesCapturer = Config.MultimediaToolkit.GetFramesCapturer ();
					auxFramesCapturer.Open (openedProject.FileSet [camConfig.Index].FilePath);
					Time offset = openedProject.FileSet [camConfig.Index].Offset;
					pixbuf = auxFramesCapturer.GetFrame (pos + offset, true, -1, -1);
					auxFramesCapturer.Dispose ();
				} else {
					Time offset = openedProject.FileSet.First ().Offset;
					pixbuf = framesCapturer.GetFrame (pos + offset, true, -1, -1);
				}
			} else {
				pixbuf = player.CurrentFrame;
			}
			if (pixbuf == null) {
				Config.GUIToolkit.ErrorMessage (Catalog.GetString ("Error capturing video frame"));
			} else {
				Config.GUIToolkit.DrawingTool (pixbuf, play, drawing, camConfig, openedProject);
			}
		}

		protected void RenderPlay (Project project, TimelineEvent play)
		{
			Playlist playlist;
			EncodingSettings settings;
			EditionJob job;
			string outputDir, outputProjectDir, outputFile;

			if (Config.AutoRenderDir == null ||
			    !Directory.Exists (Config.AutoRenderDir)) {
				outputDir = Config.VideosDir;
			} else {
				outputDir = Config.AutoRenderDir;
			}

			outputProjectDir = Path.Combine (outputDir,
				Utils.SanitizePath (project.ShortDescription));
			outputFile = String.Format ("{0}-{1}.mp4", play.EventType.Name, play.Name);
			outputFile = Utils.SanitizePath (outputFile, ' ');
			outputFile = Path.Combine (outputProjectDir, outputFile);
			try {
				PlaylistPlayElement element;

				Directory.CreateDirectory (outputProjectDir);
				settings = EncodingSettings.DefaultRenderingSettings (outputFile);
				playlist = new Playlist ();
				element = new PlaylistPlayElement (play);
				playlist.Elements.Add (element);
				job = new EditionJob (playlist, settings);
				Config.RenderingJobsManger.AddJob (job);
			} catch (Exception ex) {
				Log.Exception (ex);
			}
		}

		protected Image CaptureFrame (Time tagtime)
		{
			Image frame = null;

			/* Get the current frame and get a thumbnail from it */
			if (projectType == ProjectType.CaptureProject ||
			    projectType == ProjectType.URICaptureProject) {
				frame = capturer.CurrentCaptureFrame;
			} else if (projectType == ProjectType.FileProject) {
				frame = framesCapturer.GetFrame (tagtime, true, Constants.MAX_THUMBNAIL_SIZE,
					Constants.MAX_THUMBNAIL_SIZE);
			}
			return frame;
		}

		protected virtual void AddNewPlay (TimelineEvent play)
		{
			/* Clip play boundaries */
			play.Start.MSeconds = Math.Max (0, play.Start.MSeconds);
			if (projectType == ProjectType.FileProject) {
				play.Stop.MSeconds = Math.Min (player.StreamLength.MSeconds, play.Stop.MSeconds);
				play.CamerasLayout = player.CamerasLayout;
				play.CamerasConfig = new ObservableCollection<CameraConfig> (player.CamerasConfig);
			} else {
				play.CamerasLayout = null;
				play.CamerasConfig = new ObservableCollection<CameraConfig> { new CameraConfig (0) };
			}

			filter.Update ();
			analysisWindow.AddPlay (play);
			Config.EventsBroker.EmitEventCreated (play);
			if (projectType == ProjectType.FileProject) {
				player.Play ();
			}
			Save (openedProject);

			if (projectType == ProjectType.CaptureProject ||
			    projectType == ProjectType.URICaptureProject) {
				if (Config.AutoRenderPlaysInLive) {
					RenderPlay (openedProject, play);
				}
			}
		}

		async public virtual void HandleNewDashboardEvent (TimelineEvent play, DashboardButton btn, bool edit, List<DashboardButton> from)
		{
			if (openedProject == null)
				return;

			if (projectType == ProjectType.CaptureProject ||
			    projectType == ProjectType.URICaptureProject ||
			    projectType == ProjectType.FakeCaptureProject) {
				if (!capturer.Capturing) {
					Config.GUIToolkit.WarningMessage (Catalog.GetString ("Video capture is stopped"));
					return;
				}
			}

			if (!openedProject.Dashboard.DisablePopupWindow && edit) {
				if (projectType == ProjectType.FileProject) {
					bool playing = player.Playing;
					player.Pause ();
					await Config.GUIToolkit.EditPlay (play, openedProject, true, true, true, true);
					if (playing) {
						player.Play ();
					}
				} else {
					await Config.GUIToolkit.EditPlay (play, openedProject, true, true, true, true);
				}
			}

			Log.Debug (String.Format ("New play created start:{0} stop:{1} category:{2}",
				play.Start.ToMSecondsString (), play.Stop.ToMSecondsString (),
				play.EventType.Name));
			openedProject.AddEvent (play);
			AddNewPlay (play);
		}

		protected virtual void HandleDeleteEvents (List<TimelineEvent> plays)
		{
			DeletePlays (plays);
		}

		protected virtual void HandleCreateSnaphotSeries (TimelineEvent play)
		{
			player.Pause ();
			Config.GUIToolkit.ExportFrameSeries (openedProject, play, Config.SnapshotsDir);
		}

		protected void HandleMoveToEventType (TimelineEvent play, EventType evType)
		{
			var newplay = play.Clone ();
			DeletePlays (new List<TimelineEvent> { play }, false);
			openedProject.AddEvent (newplay);
			analysisWindow.AddPlay (newplay);
			Save (openedProject);
			filter.Update ();
		}

		protected virtual void HandleDashboardEditedEvent ()
		{
			openedProject.UpdateEventTypesAndTimers ();
			analysisWindow.ReloadProject ();
		}

		void HandleDuplicateEvents (List<TimelineEvent> plays)
		{
			foreach (var play in plays) {
				var copy = play.Clone ();
				openedProject.AddEvent (copy);
				analysisWindow.AddPlay (copy);
			}
			filter.Update ();
		}

		public void HandleNewEvent (EventType evType, List<Player> players, ObservableCollection<Team> teams, List<Tag> tags,
		                            Time start, Time stop, Time eventTime, DashboardButton btn)
		{
			if (openedProject == null) {
				return;
			} else if (projectType == ProjectType.FileProject && player == null) {
				Log.Error ("Player not set, new event will not be created");
				return;
			} else if (projectType == ProjectType.CaptureProject ||
			           projectType == ProjectType.URICaptureProject ||
			           projectType == ProjectType.FakeCaptureProject) {
				if (!capturer.Capturing) {
					Config.GUIToolkit.WarningMessage (Catalog.GetString ("Video capture is stopped"));
					return;
				}
			}
			Log.Debug (String.Format ("New play created start:{0} stop:{1} category:{2}",
				start.ToMSecondsString (), stop.ToMSecondsString (),
				evType.Name));
			/* Add the new created play to the project and update the GUI */
			var play = openedProject.AddEvent (evType, start, stop, eventTime, null);
			play.Teams = teams;
			if (players != null) {
				play.Players = new ObservableCollection<Player> (players);
			}
			if (tags != null) {
				play.Tags = new ObservableCollection<Tag> (tags);
			}
			AddNewPlay (play);
		}


		#region IService

		public virtual int Level {
			get {
				return 60;
			}
		}

		public virtual string Name {
			get {
				return "Core Events";
			}
		}

		public bool Start ()
		{
			Config.EventsBroker.EventsDeletedEvent += HandleDeleteEvents;
			Config.EventsBroker.EventLoadedEvent += HandlePlayLoaded;
			Config.EventsBroker.PlaylistElementLoadedEvent += HandlePlaylistElementLoaded;
			Config.EventsBroker.OpenedProjectChanged += HandleOpenedProjectChanged;
			Config.EventsBroker.DrawFrame += HandleDrawFrame;
			Config.EventsBroker.SnapshotSeries += HandleCreateSnaphotSeries;

			Config.EventsBroker.MoveToEventTypeEvent += HandleMoveToEventType;
			Config.EventsBroker.DuplicateEventsEvent += HandleDuplicateEvents;

			Config.EventsBroker.NewDashboardEventEvent += HandleNewDashboardEvent;
			Config.EventsBroker.NewEventEvent += HandleNewEvent;

			Config.EventsBroker.DashboardEditedEvent += HandleDashboardEditedEvent;

			Config.EventsBroker.TagSubcategoriesChangedEvent += HandleTagSubcategoriesChangedEvent;
			Config.EventsBroker.Detach += HandleDetach;
			Config.EventsBroker.ShowFullScreenEvent += HandleShowFullScreenEvent;
			return true;
		}

		public bool Stop ()
		{
			Config.EventsBroker.EventsDeletedEvent -= HandleDeleteEvents;
			Config.EventsBroker.EventLoadedEvent -= HandlePlayLoaded;
			Config.EventsBroker.PlaylistElementLoadedEvent -= HandlePlaylistElementLoaded;
			Config.EventsBroker.OpenedProjectChanged -= HandleOpenedProjectChanged;
			Config.EventsBroker.DrawFrame -= HandleDrawFrame;
			Config.EventsBroker.SnapshotSeries -= HandleCreateSnaphotSeries;

			Config.EventsBroker.MoveToEventTypeEvent -= HandleMoveToEventType;
			Config.EventsBroker.DuplicateEventsEvent -= HandleDuplicateEvents;

			Config.EventsBroker.NewDashboardEventEvent -= HandleNewDashboardEvent;
			Config.EventsBroker.NewEventEvent -= HandleNewEvent;

			Config.EventsBroker.DashboardEditedEvent -= HandleDashboardEditedEvent;

			Config.EventsBroker.TagSubcategoriesChangedEvent -= HandleTagSubcategoriesChangedEvent;
			Config.EventsBroker.Detach -= HandleDetach;
			Config.EventsBroker.ShowFullScreenEvent -= HandleShowFullScreenEvent;
			return true;
		}

		#endregion
	}
}
