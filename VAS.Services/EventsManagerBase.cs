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

namespace VAS.Services
{
	public abstract class EventsManagerBase : IService
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

		public EventsManagerBase ()
		{
		}

		protected abstract void HandleOpenedProjectChanged (Project project, ProjectType projectType,
		                                                    EventsFilter filter, IAnalysisWindowBase analysisWindow);

		//typed required for database storage
		protected abstract void Save (Project project);

		protected virtual void DeletePlays (List<TimelineEvent> plays, bool update = true)
		{
			Log.Debug (plays.Count + " plays deleted");
			analysisWindow.DeletePlays (plays.Cast<TimelineEvent> ().ToList ());
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

		protected abstract void HandlePlaylistElementLoaded (Playlist playlist, IPlaylistElement element);

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

		protected virtual void HandleShowProjectStatsEvent (Project project)
		{
			Config.GUIToolkit.ShowProjectStats (project);
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

		protected abstract void RenderPlay (Project project, TimelineEvent play);

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

		protected virtual void OnPlaysDeleted (List<TimelineEvent> plays)
		{
			DeletePlays (plays);
		}

		protected abstract void OnDuplicatePlays (List<TimelineEvent> plays);


		protected virtual void OnSnapshotSeries (TimelineEvent play)
		{
			player.Pause ();
			Config.GUIToolkit.ExportFrameSeries (openedProject, play, Config.SnapshotsDir);
		}

		protected abstract void OnPlayCategoryChanged (TimelineEvent play, EventType evType);

		protected virtual void HandleDashboardEditedEvent ()
		{
			openedProject.UpdateEventTypesAndTimers ();
			analysisWindow.ReloadProject ();
		}

		protected abstract void HandleKeyPressed (object sender, HotKey key);

		#region IService

		public virtual int Level {
			get {
				return 60;
			}
		}

		public virtual string Name {
			get {
				return "Events";
			}
		}

		public abstract bool Start ();

		public abstract bool Stop ();

		#endregion
	}
}
