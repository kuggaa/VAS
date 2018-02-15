//
//  Copyright (C) 2017 Fluendo S.A.
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
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Interfaces.Services;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services.State;

namespace VAS.Services.Controller
{
	public class DrawingsService : ControllerBase, IDrawingsService
	{
		// FIXME: Remove subscriptions and ViewModel settings when EventsMenu and SportPlaysMenu are migrated to MVVM
		VideoPlayerVM videoPlayer;
		ProjectVM project;

		public override void SetViewModel (IViewModel viewModel)
		{
			videoPlayer = ((IVideoPlayerDealer)viewModel).VideoPlayer;
			project = (viewModel as IProjectDealer)?.Project;
		}

		public override async Task Start ()
		{
			await base.Start ();

			App.Current.EventsBroker.Subscribe<DrawFrameEvent> (HandleDrawFrame);
			App.Current.EventsBroker.Subscribe<SnapshotSeriesEvent> (HandleCreateSnaphotSeries);
		}

		public override async Task Stop ()
		{
			await base.Stop ();

			App.Current.EventsBroker.Unsubscribe<DrawFrameEvent> (HandleDrawFrame);
			App.Current.EventsBroker.Unsubscribe<SnapshotSeriesEvent> (HandleCreateSnaphotSeries);
		}

		void HandleDrawFrame (DrawFrameEvent evt)
		{
			DrawFrame (videoPlayer, project, evt.Play, evt.DrawingIndex, evt.CamConfig, evt.Frame);
		}

		void HandleCreateSnaphotSeries (SnapshotSeriesEvent obj)
		{
			CreateSnapshotSeries (videoPlayer, obj.TimelineEvent);
		}

		public void DrawFrame (VideoPlayerVM videoPlayer, ProjectVM project, TimelineEventVM play, int drawingIndex, CameraConfig cameraConfig, Image frame)
		{
			FrameDrawing drawing = null;
			Time pos;
			MediaFileSet fileSet;

			videoPlayer.PauseCommand.Execute (true);

			if (play != null &&
				play.Model == null) {
				play = videoPlayer.LoadedElement as TimelineEventVM;
			}

			fileSet = play?.Model?.FileSet;

			if (play?.Model != null) {
				if (drawingIndex == -1) {
					cameraConfig = cameraConfig ?? new CameraConfig (0);
					drawing = new FrameDrawing {
						Render = videoPlayer.Player.CurrentTime,
						CameraConfig = cameraConfig,
						RegionOfInterest = cameraConfig.RegionOfInterest.Clone (),
					};
				} else {
					drawing = play.Model.Drawings [drawingIndex];
					cameraConfig = cameraConfig ?? drawing.CameraConfig;
				}
				pos = drawing.Render;
			} else {
				pos = videoPlayer.Player.CurrentTime;
			}

			if (frame == null) {
				IFramesCapturer framesCapturer;
				if (fileSet == null) {
					throw new InvalidOperationException ("The event doesn't seems to provide a MediaFileset");
				}
				framesCapturer = App.Current.MultimediaToolkit.GetFramesCapturer ();
				MediaFile file = fileSet [cameraConfig.Index];
				framesCapturer.Open (file.FilePath);
				frame = framesCapturer.GetFrame (pos + file.Offset, true,
												  (int)file.DisplayVideoWidth, (int)file.DisplayVideoHeight);
				framesCapturer.Dispose ();
			}

			if (frame == null) {
				App.Current.Dialogs.ErrorMessage (Catalog.GetString ("Error capturing video frame"));
				return;
			}

			dynamic properties = new ExpandoObject ();
			properties.project = project?.Model ?? play?.Model?.Project;
			properties.timelineEvent = play;
			properties.frame = frame;
			properties.drawing = drawing;
			properties.cameraconfig = cameraConfig;
			App.Current.StateController.MoveToModal (DrawingToolState.NAME, properties);
		}

		public void CreateSnapshotSeries (VideoPlayerVM videoPlayer, TimelineEventVM timelineEvent)
		{
			videoPlayer.PauseCommand.Execute (false);
			// FIXME: Use DrawingsService.CreateSnapshot when this menu is migrated to MVVM
			App.Current.GUIToolkit.ExportFrameSeries (timelineEvent.Model, App.Current.SnapshotsDir);
		}
	}
}
