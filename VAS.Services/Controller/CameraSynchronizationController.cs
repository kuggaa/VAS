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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services.ViewModel;

namespace VAS.Services.Controller
{
	public class CameraSynchronizationController : ControllerBase
	{
		CameraSynchronizationVM cameraSynchronizationVM;
		ProjectVM projectVM;
		VideoPlayerVM videoPlayerVM;

		public override void SetViewModel (IViewModel viewModel)
		{
			if (projectVM != null) {
				projectVM.Periods.PropertyChanged -= HandlePropertyChanged;
				projectVM.FileSet.PropertyChanged -= HandlePropertyChanged;
			}
			cameraSynchronizationVM = (CameraSynchronizationVM)viewModel;
			if (viewModel != null) {
				videoPlayerVM = cameraSynchronizationVM.VideoPlayer;
				projectVM = cameraSynchronizationVM.Project;
				projectVM.FileSet.PropertyChanged += HandlePropertyChanged;
				projectVM.Periods.PropertyChanged += HandlePropertyChanged;
			}
		}

		public override IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return new KeyAction [] {
				new KeyAction (
					App.Current.HotkeysService.GetByName("PLAYER_SEEK_LEFT_SHORT"),
					() => PerformStep (false)
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName("PLAYER_SEEK_LEFT_LONG"),
					() => PerformStep (true)
				),
			};
		}

		public override void Start ()
		{
			base.Start ();
			videoPlayerVM.OpenFileSet (projectVM.FileSet);
		}

		public override void Stop ()
		{
			base.Stop ();
			videoPlayerVM.Pause ();
		}

		void HandleApplyChanges (bool resyncEvents)
		{
			/* If a new camera has been added or a camera has been removed,
			 * make sure events have a correct camera configuration */
			foreach (TimelineEventVM evt in projectVM.Timeline.FullTimeline) {
				int cc = evt.Model.CamerasConfig.Count;
				int fc = projectVM.FileSet.ViewModels.Count;

				if (cc < fc) {
					for (int i = cc; i < fc; i++) {
						evt.Model.CamerasConfig.Add (new CameraConfig (i));
					}
				}
			}

			if (resyncEvents) {
				projectVM.Model.ResyncEvents (cameraSynchronizationVM.InitialPeriods);
			}
		}

		void PerformStep (bool isForward)
		{
			MediaFileVM file = projectVM.FileSet.FirstOrDefault (f => f.Selected);
			if (file != null) {
				videoPlayerVM.Pause ();
				var oneFrame = new Time (1 / file.Fps);
				if (isForward) {
					file.Offset += oneFrame;
				} else {
					file.Offset -= oneFrame;
				}
			}
		}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (sender is TimeNode) {
				Time seekTime = null;

				if (e.PropertyName == nameof (TimeNode.Start)) {
					seekTime = (sender as TimeNode).Start;
				} else if (e.PropertyName == nameof (TimeNode.Stop)) {
					seekTime = (sender as TimeNode).Stop;
				}
				if (seekTime != null) {
					videoPlayerVM.Seek (seekTime, false, false, false);
				}
			}
			if (sender is MediaFileVM) {
				if (e.PropertyName == nameof (MediaFile.Offset)) {
					videoPlayerVM.Pause ();
					videoPlayerVM.Seek (videoPlayerVM.CurrentTime, false, false, false);
				}
			}
		}
	}
}
