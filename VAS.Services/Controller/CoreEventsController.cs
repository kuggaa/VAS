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
using System.Collections.Generic;
using System.Dynamic;
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
	public class CoreEventsController : ControllerBase
	{
		/* current project in use */
		protected ProjectVM project;
		protected VideoPlayerVM videoPlayer;
		protected ICapturerBin capturer;
		protected IFramesCapturer framesCapturer;

		/// <summary>
		/// Gets or sets the current loaded play.
		/// </summary>
		/// <value>The loaded play, null if no play.</value>
		public TimelineEvent LoadedPlay { get; set; }

		public override void Start ()
		{
			base.Start ();

			App.Current.EventsBroker.Subscribe<DrawFrameEvent> (HandleDrawFrame);
			App.Current.EventsBroker.Subscribe<SnapshotSeriesEvent> (HandleCreateSnaphotSeries);

			App.Current.EventsBroker.Subscribe<DashboardEditedEvent> (HandleDashboardEditedEvent);
			App.Current.EventsBroker.Subscribe<TagSubcategoriesChangedEvent> (HandleTagSubcategoriesChangedEvent);
			App.Current.EventsBroker.Subscribe<ShowFullScreenEvent> (HandleShowFullScreenEvent);

			App.Current.EventsBroker.Subscribe<EventLoadedEvent> (HandleEventLoadedEvent);
			App.Current.EventsBroker.Subscribe<PlaylistElementLoadedEvent> (HandlePlaylistElementLoaded);
		}

		public override void Stop ()
		{
			base.Stop ();

			App.Current.EventsBroker.Unsubscribe<DrawFrameEvent> (HandleDrawFrame);
			App.Current.EventsBroker.Unsubscribe<SnapshotSeriesEvent> (HandleCreateSnaphotSeries);

			App.Current.EventsBroker.Unsubscribe<DashboardEditedEvent> (HandleDashboardEditedEvent);
			App.Current.EventsBroker.Unsubscribe<TagSubcategoriesChangedEvent> (HandleTagSubcategoriesChangedEvent);
			App.Current.EventsBroker.Unsubscribe<ShowFullScreenEvent> (HandleShowFullScreenEvent);

			App.Current.EventsBroker.Subscribe<EventLoadedEvent> (HandleEventLoadedEvent);
			App.Current.EventsBroker.Unsubscribe<PlaylistElementLoadedEvent> (HandlePlaylistElementLoaded);
		}

		public override void SetViewModel (IViewModel viewModel)
		{
			project = (ProjectVM)(viewModel as dynamic);
			videoPlayer = (VideoPlayerVM)(viewModel as dynamic);

			// FIXME: Remove the try catch when the new interface is passed instead of IViewModel
			try {
				capturer = (ICapturerBin)((viewModel as dynamic).Capturer);
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

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			framesCapturer?.Dispose ();
		}

		protected virtual void HandleDrawFrame (DrawFrameEvent e)
		{
			Image pixbuf;
			FrameDrawing drawing = null;
			Time pos;

			videoPlayer.Pause (true);

			if (e.Play == null) {
				e.Play = LoadedPlay;
			}

			if (e.Play != null) {
				if (e.DrawingIndex == -1) {
					drawing = new FrameDrawing {
						Render = videoPlayer.Player.CurrentTime,
						CameraConfig = e.CamConfig,
						RegionOfInterest = e.CamConfig.RegionOfInterest.Clone (),
					};
				} else {
					drawing = e.Play.Drawings [e.DrawingIndex];
				}
				pos = drawing.Render;
			} else {
				pos = videoPlayer.Player.CurrentTime;
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

		void HandleEventLoadedEvent (EventLoadedEvent e)
		{
			LoadedPlay = e.TimelineEvent;
		}

		void HandlePlaylistElementLoaded (PlaylistElementLoadedEvent e)
		{
			if (e.Element is PlaylistPlayElement) {
				LoadedPlay = (e.Element as PlaylistPlayElement).Play;
			} else {
				LoadedPlay = null;
			}
		}
	}
}
