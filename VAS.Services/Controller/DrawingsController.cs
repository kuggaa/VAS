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
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services.State;

namespace VAS.Services.Controller
{
	public class DrawingsController : ControllerBase
	{
		VideoPlayerVM videoPlayer;
		ProjectVM project;

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

		public override void SetViewModel (IViewModel viewModel)
		{
			videoPlayer = ((IVideoPlayerDealer)viewModel).VideoPlayer;
			project = (viewModel as IProjectDealer)?.Project;
		}

		public override IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

		protected virtual void HandleDrawFrame (DrawFrameEvent drawEvent)
		{
			FrameDrawing drawing = null;
			Time pos;
			MediaFileSet fileSet;

			videoPlayer.PauseCommand.Execute (true);

			if (drawEvent.Play == null) {
				drawEvent.Play = videoPlayer.LoadedElement as TimelineEvent;
			}

			fileSet = drawEvent.Play?.FileSet;

			if (drawEvent.Play != null) {
				if (drawEvent.DrawingIndex == -1) {
					drawEvent.CamConfig = drawEvent.CamConfig ?? new CameraConfig (0);
					drawing = new FrameDrawing {
						Render = videoPlayer.Player.CurrentTime,
						CameraConfig = drawEvent.CamConfig,
						RegionOfInterest = drawEvent.CamConfig.RegionOfInterest.Clone (),
					};
				} else {
					drawing = drawEvent.Play.Drawings [drawEvent.DrawingIndex];
					drawEvent.CamConfig = drawEvent.CamConfig ?? drawing.CameraConfig;
				}
				pos = drawing.Render;
			} else {
				pos = videoPlayer.Player.CurrentTime;
			}

			if (drawEvent.Frame == null) {
				IFramesCapturer framesCapturer;
				if (fileSet == null) {
					throw new InvalidOperationException ("The event doesn't seems to provide a MediaFileset");
				}
				framesCapturer = App.Current.MultimediaToolkit.GetFramesCapturer ();
				MediaFile file = fileSet [drawEvent.CamConfig.Index];
				framesCapturer.Open (file.FilePath);
				drawEvent.Frame = framesCapturer.GetFrame (pos + file.Offset, true,
												  (int)file.DisplayVideoWidth, (int)file.DisplayVideoHeight);
				framesCapturer.Dispose ();
			}

			if (drawEvent.Frame == null) {
				App.Current.Dialogs.ErrorMessage (Catalog.GetString ("Error capturing video frame"));
				return;
			}

			dynamic properties = new ExpandoObject ();
			properties.project = project?.Model ?? drawEvent.Play?.Project;
			properties.timelineEvent = drawEvent.Play;
			properties.frame = drawEvent.Frame;
			properties.drawing = drawing;
			properties.cameraconfig = drawEvent.CamConfig;
			App.Current.StateController.MoveToModal (DrawingToolState.NAME, properties);
		}

		protected virtual void HandleCreateSnaphotSeries (SnapshotSeriesEvent e)
		{
			videoPlayer.PauseCommand.Execute (false);
			App.Current.GUIToolkit.ExportFrameSeries (e.TimelineEvent, App.Current.SnapshotsDir);
		}
	}
}
