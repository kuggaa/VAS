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
using System.Dynamic;
using System.IO;
using System.Linq;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;
using VAS.Services.State;

namespace VAS.Services.Controller
{
	// FIXME: Merge with the EventsController
	public class CoreEventsController : ControllerBase
	{
		/* Current play loaded. null if no play is loaded */
		protected TimelineEvent loadedPlay;
		/* current project in use */
		protected ProjectVM project;
		protected VideoPlayerVM videoPlayer;
		protected ICapturerBin capturer;
		protected IFramesCapturer framesCapturer;

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			framesCapturer?.Dispose ();
		}

		public override void Start ()
		{
			base.Start ();
			App.Current.EventsBroker.Subscribe<EventsDeletedEvent> (HandleDeleteEvents);
			App.Current.EventsBroker.Subscribe<EventLoadedEvent> (HandlePlayLoaded);
			App.Current.EventsBroker.Subscribe<PlaylistElementLoadedEvent> (HandlePlaylistElementLoaded);

			App.Current.EventsBroker.Subscribe<DrawFrameEvent> (HandleDrawFrame);
			App.Current.EventsBroker.Subscribe<SnapshotSeriesEvent> (HandleCreateSnaphotSeries);
			App.Current.EventsBroker.Subscribe<MoveToEventTypeEvent> (HandleMoveToEventType);
			App.Current.EventsBroker.Subscribe<DuplicateEventsEvent> (HandleDuplicateEvents);
			App.Current.EventsBroker.Subscribe<NewDashboardEvent> (HandleNewDashboardEvent);
			App.Current.EventsBroker.Subscribe<NewEventEvent> (HandleNewEvent);
			App.Current.EventsBroker.Subscribe<DashboardEditedEvent> (HandleDashboardEditedEvent);
			App.Current.EventsBroker.Subscribe<TagSubcategoriesChangedEvent> (HandleTagSubcategoriesChangedEvent);
			//App.Current.EventsBroker.Subscribe<DetachEvent> (HandleDetach);
			App.Current.EventsBroker.Subscribe<ShowFullScreenEvent> (HandleShowFullScreenEvent);
		}

		public override void Stop ()
		{
			base.Stop ();
			App.Current.EventsBroker.Unsubscribe<EventsDeletedEvent> (HandleDeleteEvents);
			App.Current.EventsBroker.Unsubscribe<EventLoadedEvent> (HandlePlayLoaded);
			App.Current.EventsBroker.Unsubscribe<PlaylistElementLoadedEvent> (HandlePlaylistElementLoaded);

			App.Current.EventsBroker.Unsubscribe<DrawFrameEvent> (HandleDrawFrame);
			App.Current.EventsBroker.Unsubscribe<SnapshotSeriesEvent> (HandleCreateSnaphotSeries);
			App.Current.EventsBroker.Unsubscribe<MoveToEventTypeEvent> (HandleMoveToEventType);
			App.Current.EventsBroker.Unsubscribe<DuplicateEventsEvent> (HandleDuplicateEvents);
			App.Current.EventsBroker.Unsubscribe<NewDashboardEvent> (HandleNewDashboardEvent);
			App.Current.EventsBroker.Unsubscribe<NewEventEvent> (HandleNewEvent);
			App.Current.EventsBroker.Unsubscribe<DashboardEditedEvent> (HandleDashboardEditedEvent);
			App.Current.EventsBroker.Unsubscribe<TagSubcategoriesChangedEvent> (HandleTagSubcategoriesChangedEvent);
			App.Current.EventsBroker.Unsubscribe<ShowFullScreenEvent> (HandleShowFullScreenEvent);
			//App.Current.EventsBroker.Unsubscribe<DetachEvent> (HandleDetach);
		}

		public override void SetViewModel (IViewModel viewModel)
		{
			project = (ProjectVM)(viewModel as dynamic);
			videoPlayer = (VideoPlayerVM)(viewModel as dynamic);
			try {
				capturer = (ICapturerBin)(viewModel as dynamic);
			} catch {
			}

			if (project.ProjectType == ProjectType.FileProject && project.FileSet.Any ()) {
				framesCapturer = App.Current.MultimediaToolkit.GetFramesCapturer ();
				framesCapturer.Open (project.FileSet.First ().FilePath);
			}
		}

		public override IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

		protected virtual void HandleDeleteEvents (EventsDeletedEvent e)
		{
			DeletePlays (e.TimelineEvents);
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

		protected virtual void HandleDrawFrame (DrawFrameEvent e)
		{
			Image pixbuf;
			FrameDrawing drawing = null;
			Time pos;

			videoPlayer.Pause (true);

			if (e.Play == null) {
				e.Play = loadedPlay;
			}

			if (e.Play != null) {
				if (e.DrawingIndex == -1) {
					drawing = new FrameDrawing {
						Render = videoPlayer.CurrentTime,
						CameraConfig = e.CamConfig,
						RegionOfInterest = e.CamConfig.RegionOfInterest.Clone (),
					};
				} else {
					drawing = e.Play.Drawings [e.DrawingIndex];
				}
				pos = drawing.Render;
			} else {
				pos = videoPlayer.CurrentTime;
			}

			if (framesCapturer != null && !e.Current) {
				if (e.CamConfig.Index > 0) {
					IFramesCapturer auxFramesCapturer;
					auxFramesCapturer = App.Current.MultimediaToolkit.GetFramesCapturer ();
					MediaFile file = project.FileSet.Model [e.CamConfig.Index];
					auxFramesCapturer.Open (file.FilePath);
					Time offset = project.FileSet.Model [e.CamConfig.Index].Offset;
					pixbuf = auxFramesCapturer.GetFrame (pos + offset, true,
														 (int)file.DisplayVideoWidth, (int)file.DisplayVideoHeight);
					auxFramesCapturer.Dispose ();
				} else {
					MediaFile file = project.FileSet.First ().Model;
					pixbuf = framesCapturer.GetFrame (pos + file.Offset, true,
													  (int)file.DisplayVideoWidth, (int)file.DisplayVideoHeight);
				}
			} else {
				pixbuf = videoPlayer.CurrentFrame;
			}
			if (pixbuf == null) {
				App.Current.Dialogs.ErrorMessage (Catalog.GetString ("Error capturing video frame"));
			} else {
				dynamic properties = new ExpandoObject ();
				properties.project = project.Model;
				properties.timelineEvent = e.Play;
				properties.frame = pixbuf;
				properties.drawing = drawing;
				properties.cameraconfig = e.CamConfig;
				App.Current.StateController.MoveToModal (DrawingToolState.NAME, properties);
			}
		}

		protected virtual void HandleCreateSnaphotSeries (SnapshotSeriesEvent e)
		{
			videoPlayer.Pause ();
			App.Current.GUIToolkit.ExportFrameSeries (project.Model, e.TimelineEvent, App.Current.SnapshotsDir);
		}

		protected virtual void HandleMoveToEventType (MoveToEventTypeEvent e)
		{
			// Only move the events where the event type changes for real
			var newEvents = e.TimelineEvents.Where (ev => ev.EventType != e.EventType);

			foreach (var evt in newEvents) {
				var newEvent = Cloner.Clone (evt);
				newEvent.ID = Guid.NewGuid ();
				newEvent.EventType = e.EventType;
				// Remove all tags from the previous event type but keep global tags
				newEvent.Tags.RemoveAll (t => (evt.EventType as AnalysisEventType).Tags.Contains (t));
				project.Model.AddEvent (newEvent);
				App.Current.EventsBroker.Publish (new EventCreatedEvent { TimelineEvent = newEvent });
			}
			DeletePlays (newEvents.ToList (), false);
			Save (project);
		}

		protected virtual void HandleDuplicateEvents (DuplicateEventsEvent e)
		{
			foreach (var play in e.TimelineEvents) {
				var copy = play.Clone ();
				project.Model.AddEvent (copy);
				App.Current.EventsBroker.Publish (new EventCreatedEvent { TimelineEvent = copy });
			}
		}

		protected virtual async void HandleNewDashboardEvent (NewDashboardEvent e)
		{
			if (project == null)
				return;

			if (project.IsLive) {
				if (!capturer.Capturing) {
					App.Current.Dialogs.WarningMessage (Catalog.GetString ("Video capture is stopped"));
					return;
				}
			}

			if (!project.Model.Dashboard.DisablePopupWindow && e.Edit) {
				e.TimelineEvent.AddDefaultPositions ();
				if (project.ProjectType == ProjectType.FileProject) {
					bool playing = videoPlayer.Playing;
					videoPlayer.Pause ();
					await App.Current.GUIToolkit.EditPlay (e.TimelineEvent, project.Model, true, true, true, true);
					if (playing) {
						videoPlayer.Play ();
					}
				} else {
					await App.Current.GUIToolkit.EditPlay (e.TimelineEvent, project.Model, true, true, true, true);
				}
			}

			Log.Debug (String.Format ("New play created start:{0} stop:{1} category:{2}",
				e.TimelineEvent.Start.ToMSecondsString (), e.TimelineEvent.Stop.ToMSecondsString (),
				e.TimelineEvent.EventType.Name));
			project.Model.AddEvent (e.TimelineEvent);
			AddNewPlay (e.TimelineEvent);
		}

		protected virtual void HandleNewEvent (NewEventEvent e)
		{
			if (project == null) {
				return;
			}
			if (project.IsLive) {
				if (!capturer.Capturing) {
					App.Current.Dialogs.WarningMessage (Catalog.GetString ("Video capture is stopped"));
					return;
				}
			}

			Log.Debug (String.Format ("New play created start:{0} stop:{1} category:{2}",
				e.Start.ToMSecondsString (), e.Stop.ToMSecondsString (),
				e.EventType.Name));
			/* Add the new created play to the project and update the GUI */
			var play = project.Model.AddEvent (e.EventType, e.Start, e.Stop, e.EventTime, null);
			play.Teams.Replace (e.Teams);
			if (e.Players != null) {
				play.Players.Replace (e.Players);
			}
			if (e.Tags != null) {
				play.Tags.Replace (e.Tags);
			}
			AddNewPlay (play);
		}

		protected virtual void HandleDashboardEditedEvent (DashboardEditedEvent e)
		{
			project.Model.UpdateEventTypesAndTimers ();
		}

		protected virtual void HandleTagSubcategoriesChangedEvent (TagSubcategoriesChangedEvent e)
		{
			App.Current.Config.FastTagging = !e.Active;
		}


		protected virtual void HandleShowFullScreenEvent (ShowFullScreenEvent e)
		{
			App.Current.GUIToolkit.FullScreen = e.Active;
		}

		void Save (ProjectVM project)
		{
			if (App.Current.Config.AutoSave) {
				App.Current.DatabaseManager.ActiveDB.Store (project.Model);
			}
		}

		protected virtual void DeletePlays (List<TimelineEvent> plays, bool update = true)
		{
			Log.Debug (plays.Count + " plays deleted");
			project.Timeline.Model.RemoveRange (plays);
			if (project.ProjectType == ProjectType.FileProject) {
				Save (project);
			}
			if (loadedPlay != null && plays.Contains (loadedPlay)) {
				App.Current.EventsBroker.Publish (new LoadEventEvent ());
			}
		}

		/*protected virtual void HandleDetach (DetachEvent e)
		{
			if (analysisWindow != null) {
				analysisWindow.DetachPlayer ();
			}
		}*/

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
				App.Current.JobsManager.Add (job);
			} catch (Exception ex) {
				Log.Exception (ex);
			}
		}

		protected Image CaptureFrame (Time tagtime)
		{
			Image frame = null;

			/* Get the current frame and get a thumbnail from it */
			if (project.ProjectType == ProjectType.CaptureProject ||
				project.ProjectType == ProjectType.URICaptureProject) {
				frame = capturer.CurrentCaptureFrame;
			} else if (project.ProjectType == ProjectType.FileProject) {
				frame = framesCapturer.GetFrame (tagtime, true, Constants.MAX_THUMBNAIL_SIZE,
					Constants.MAX_THUMBNAIL_SIZE);
			}
			return frame;
		}

		protected virtual void AddNewPlay (TimelineEvent play)
		{
			/* Clip play boundaries */
			play.Start.MSeconds = Math.Max (0, play.Start.MSeconds);
			if (project.ProjectType == ProjectType.FileProject) {
				play.Stop.MSeconds = Math.Min (project.FileSet.Duration.MSeconds, play.Stop.MSeconds);
				play.CamerasLayout = videoPlayer.CamerasLayout;
				play.CamerasConfig = new ObservableCollection<CameraConfig> (videoPlayer.CamerasConfig);
			} else {
				play.CamerasLayout = null;
				play.CamerasConfig = new ObservableCollection<CameraConfig> { new CameraConfig (0) };
			}

			App.Current.EventsBroker.Publish (new EventCreatedEvent { TimelineEvent = play });

			if (project.ProjectType == ProjectType.FileProject) {
				videoPlayer.Play ();
			}
			Save (project);

			if (project.ProjectType == ProjectType.CaptureProject ||
				project.ProjectType == ProjectType.URICaptureProject) {
				if (App.Current.Config.AutoRenderPlaysInLive) {
					RenderPlay (project.Model, play);
				}
			}
		}

	}
}
