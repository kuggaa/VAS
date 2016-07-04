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
using System.IO;
using System.Linq;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Filters;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.Store.Templates;

namespace VAS.Services
{
	public class CoreEventsManager : IService
	{
		string guid = Guid.NewGuid ().ToString ("N");

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

		protected void HandleOpenedProjectChanged (OpenedProjectEvent e)
		{
			Log.Debug ("HandleOpenedProjectChanged on " + guid);

			this.openedProject = e.Project;
			this.projectType = e.ProjectType;
			this.filter = e.Filter;

			if (e.Project == null) {
				if (framesCapturer != null) {
					framesCapturer.Dispose ();
					framesCapturer = null;
				}
				return;
			}

			if (e.ProjectType == ProjectType.FileProject) {
				framesCapturer = App.Current.MultimediaToolkit.GetFramesCapturer ();
				framesCapturer.Open (openedProject.FileSet.First ().FilePath);
			}
			if (e.AnalysisWindow != null) {
				this.analysisWindow = e.AnalysisWindow;
				player = e.AnalysisWindow.Player;
				capturer = e.AnalysisWindow.Capturer;
			}
		}

		void Save (Project project)
		{
			var config = VAS.App.Current.Config;
			if (VAS.App.Current.Config.AutoSave) {
				App.Current.DatabaseManager.ActiveDB.Store (project);
			}
		}

		protected virtual void DeletePlays (List<TimelineEvent> plays, bool update = true)
		{
			Log.Debug (plays.Count + " plays deleted");
			openedProject.RemoveEvents (plays);
			if (projectType == ProjectType.FileProject) {
				Save (openedProject);
			}
			if (loadedPlay != null && plays.Contains (loadedPlay)) {
				App.Current.EventsBroker.Publish<LoadEventEvent> (new LoadEventEvent ());
			}
			filter.Update ();
		}

		protected virtual void HandleShowFullScreenEvent (ShowFullScreenEvent e)
		{
			App.Current.GUIToolkit.FullScreen = e.Active;
		}

		protected virtual void HandlePlayLoaded (EventLoadedEvent e)
		{
			loadedPlay = e.TimelineEvent;
		}

		protected virtual void HandlePlaylistElementLoaded (PlaylistElementLoadedEvent e)
		{
			if (e.Element is PlaylistPlayElement) {
				loadedPlay = (e.Element as PlaylistPlayElement).Play;
			} else {
				loadedPlay = null;
			}
		}

		protected virtual void HandleDetach (DetachEvent e)
		{
			if (analysisWindow != null) {
				analysisWindow.DetachPlayer ();
			}
		}

		protected virtual void HandleTagSubcategoriesChangedEvent (TagSubcategoriesChangedEvent e)
		{
			App.Current.Config.FastTagging = !e.Active;
		}

		//protected virtual void HandleDrawFrame (TimelineEvent play, int drawingIndex, CameraConfig camConfig, bool current)
		protected virtual void HandleDrawFrame (DrawFrameEvent e)
		{
			Image pixbuf;
			FrameDrawing drawing = null;
			Time pos;

			player.Pause (true);
			if (e.Play == null) {
				e.Play = loadedPlay;
			}
			if (e.Play != null) {
				if (e.DrawingIndex == -1) {
					drawing = new FrameDrawing {
						Render = player.CurrentTime,
						CameraConfig = e.CamConfig,
						RegionOfInterest = e.CamConfig.RegionOfInterest.Clone (),
					};
				} else {
					drawing = e.Play.Drawings [e.DrawingIndex];
				}
				pos = drawing.Render;
			} else {
				pos = player.CurrentTime;
			}

			if (framesCapturer != null && !e.Current) {
				if (e.CamConfig.Index > 0) {
					IFramesCapturer auxFramesCapturer;
					auxFramesCapturer = App.Current.MultimediaToolkit.GetFramesCapturer ();
					auxFramesCapturer.Open (openedProject.FileSet [e.CamConfig.Index].FilePath);
					Time offset = openedProject.FileSet [e.CamConfig.Index].Offset;
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
				App.Current.GUIToolkit.ErrorMessage (Catalog.GetString ("Error capturing video frame"));
			} else {
				App.Current.GUIToolkit.DrawingTool (pixbuf, e.Play, drawing, e.CamConfig, openedProject);
			}
		}

		protected void RenderPlay (Project project, TimelineEvent play)
		{
			Playlist playlist;
			EncodingSettings settings;
			EditionJob job;
			string outputDir, outputProjectDir, outputFile;

			if (App.Current.Config.AutoRenderDir == null ||
			    !Directory.Exists (App.Current.Config.AutoRenderDir)) {
				outputDir = App.Current.VideosDir;
			} else {
				outputDir = App.Current.Config.AutoRenderDir;
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
				App.Current.RenderingJobsManger.AddJob (job);
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
			App.Current.EventsBroker.Publish<EventCreatedEvent> (
				new EventCreatedEvent {
					TimelineEvent = play
				}
			);
			
			if (projectType == ProjectType.FileProject) {
				player.Play ();
			}
			Save (openedProject);

			if (projectType == ProjectType.CaptureProject ||
			    projectType == ProjectType.URICaptureProject) {
				if (App.Current.Config.AutoRenderPlaysInLive) {
					RenderPlay (openedProject, play);
				}
			}
		}

		async public virtual void HandleNewDashboardEvent (NewDashboardEvent e)
		{
			if (openedProject == null)
				return;

			if (projectType == ProjectType.CaptureProject ||
			    projectType == ProjectType.URICaptureProject ||
			    projectType == ProjectType.FakeCaptureProject) {
				if (!capturer.Capturing) {
					App.Current.GUIToolkit.WarningMessage (Catalog.GetString ("Video capture is stopped"));
					return;
				}
			}

			if (!openedProject.Dashboard.DisablePopupWindow && e.Edit) {
				if (projectType == ProjectType.FileProject) {
					bool playing = player.Playing;
					player.Pause ();
					await App.Current.GUIToolkit.EditPlay (e.TimelineEvent, openedProject, true, true, true, true);
					if (playing) {
						player.Play ();
					}
				} else {
					await App.Current.GUIToolkit.EditPlay (e.TimelineEvent, openedProject, true, true, true, true);
				}
			}

			Log.Debug (String.Format ("New play created start:{0} stop:{1} category:{2}",
				e.TimelineEvent.Start.ToMSecondsString (), e.TimelineEvent.Stop.ToMSecondsString (),
				e.TimelineEvent.EventType.Name));
			openedProject.AddEvent (e.TimelineEvent);
			AddNewPlay (e.TimelineEvent);
		}

		protected virtual void HandleDeleteEvents (EventsDeletedEvent e)
		{
			DeletePlays (e.TimelineEvents);
		}

		protected virtual void HandleCreateSnaphotSeries (SnapshotSeriesEvent e)
		{
			player.Pause ();
			App.Current.GUIToolkit.ExportFrameSeries (openedProject, e.TimelineEvent, App.Current.SnapshotsDir);
		}

		protected void HandleMoveToEventType (MoveToEventTypeEvent e)
		{
			var newplay = e.TimelineEvent.Clone ();
			DeletePlays (new List<TimelineEvent> { e.TimelineEvent }, false);
			openedProject.AddEvent (newplay);
			App.Current.EventsBroker.Publish<EventCreatedEvent> (
				new EventCreatedEvent {
					TimelineEvent = newplay
				}
			);
			Save (openedProject);
			filter.Update ();
		}

		protected virtual void HandleDashboardEditedEvent (DashboardEditedEvent e)
		{
			openedProject.UpdateEventTypesAndTimers ();
			analysisWindow.ReloadProject ();
		}

		void HandleDuplicateEvents (DuplicateEventsEvent e)
		{
			foreach (var play in e.TimelineEvents) {
				var copy = play.Clone ();
				openedProject.AddEvent (copy);
				App.Current.EventsBroker.Publish<EventCreatedEvent> (
					new EventCreatedEvent {
						TimelineEvent = copy
					}
				);
			}
			filter.Update ();
		}

		public void HandleNewEvent (NewEventEvent e)
		{
			Log.Debug ("HandleNewEvent on " + guid);

			if (openedProject == null) {
				return;
			} else if (projectType == ProjectType.FileProject && player == null) {
				Log.Error ("Player not set, new event will not be created");
				return;
			} else if (projectType == ProjectType.CaptureProject ||
			           projectType == ProjectType.URICaptureProject ||
			           projectType == ProjectType.FakeCaptureProject) {
				if (!capturer.Capturing) {
					App.Current.GUIToolkit.WarningMessage (Catalog.GetString ("Video capture is stopped"));
					return;
				}
			}
			Log.Debug (String.Format ("New play created start:{0} stop:{1} category:{2}",
				e.Start.ToMSecondsString (), e.Stop.ToMSecondsString (),
				e.EventType.Name));
			/* Add the new created play to the project and update the GUI */
			var play = openedProject.AddEvent (e.EventType, e.Start, e.Stop, e.EventTime, null);
			play.Teams = e.Teams;
			if (e.Players != null) {
				play.Players = new ObservableCollection<Player> (e.Players);
			}
			if (e.Tags != null) {
				play.Tags = new ObservableCollection<Tag> (e.Tags);
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
			App.Current.EventsBroker.Subscribe<EventsDeletedEvent> (HandleDeleteEvents);
			App.Current.EventsBroker.Subscribe<EventLoadedEvent> (HandlePlayLoaded);
			App.Current.EventsBroker.Subscribe<PlaylistElementLoadedEvent> (HandlePlaylistElementLoaded);
			App.Current.EventsBroker.Subscribe<OpenedProjectEvent> (HandleOpenedProjectChanged);
			App.Current.EventsBroker.Subscribe<DrawFrameEvent> (HandleDrawFrame);
			App.Current.EventsBroker.Subscribe<SnapshotSeriesEvent> (HandleCreateSnaphotSeries);

			App.Current.EventsBroker.Subscribe<MoveToEventTypeEvent> (HandleMoveToEventType);
			App.Current.EventsBroker.Subscribe<DuplicateEventsEvent> (HandleDuplicateEvents);

			App.Current.EventsBroker.Subscribe<NewDashboardEvent> (HandleNewDashboardEvent);
			App.Current.EventsBroker.Subscribe<NewEventEvent> (HandleNewEvent);

			App.Current.EventsBroker.Subscribe<DashboardEditedEvent> (HandleDashboardEditedEvent);

			App.Current.EventsBroker.Subscribe<TagSubcategoriesChangedEvent> (HandleTagSubcategoriesChangedEvent);
			App.Current.EventsBroker.Subscribe <DetachEvent> (HandleDetach);
			App.Current.EventsBroker.Subscribe<ShowFullScreenEvent> (HandleShowFullScreenEvent);
			return true;
		}

		public bool Stop ()
		{
			App.Current.EventsBroker.Unsubscribe<EventsDeletedEvent> (HandleDeleteEvents);
			App.Current.EventsBroker.Unsubscribe<EventLoadedEvent> (HandlePlayLoaded);
			App.Current.EventsBroker.Unsubscribe<PlaylistElementLoadedEvent> (HandlePlaylistElementLoaded);
			App.Current.EventsBroker.Unsubscribe<OpenedProjectEvent> (HandleOpenedProjectChanged);
			App.Current.EventsBroker.Unsubscribe<DrawFrameEvent> (HandleDrawFrame);
			App.Current.EventsBroker.Unsubscribe<SnapshotSeriesEvent> (HandleCreateSnaphotSeries);

			App.Current.EventsBroker.Unsubscribe<MoveToEventTypeEvent> (HandleMoveToEventType);
			App.Current.EventsBroker.Unsubscribe<DuplicateEventsEvent> (HandleDuplicateEvents);

			App.Current.EventsBroker.Unsubscribe<NewDashboardEvent> (HandleNewDashboardEvent);
			App.Current.EventsBroker.Unsubscribe<NewEventEvent> (HandleNewEvent);

			App.Current.EventsBroker.Unsubscribe<DashboardEditedEvent> (HandleDashboardEditedEvent);

			App.Current.EventsBroker.Unsubscribe<TagSubcategoriesChangedEvent> (HandleTagSubcategoriesChangedEvent);
			App.Current.EventsBroker.Unsubscribe <DetachEvent> (HandleDetach);
			App.Current.EventsBroker.Unsubscribe<ShowFullScreenEvent> (HandleShowFullScreenEvent);
			return true;
		}

		#endregion
	}
}
