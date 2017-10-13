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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services.State;
using VAS.Services.ViewModel;

namespace VAS.Services.Controller
{
	[Controller (CameraSynchronizationEditorState.NAME)]
	public class CameraSynchronizationController : ControllerBase
	{
		protected CameraSynchronizationVM cameraSynchronizationVM;
		protected ProjectVM projectVM;
		VideoPlayerVM videoPlayerVM;


		public override async Task Start ()
		{
			await base.Start ();
			App.Current.EventsBroker.Subscribe<SaveEvent<ProjectVM>> (HandleSave);
			projectVM.FileSet.PropertyChanged += HandlePropertyChanged;
			projectVM.Periods.PropertyChanged += HandlePropertyChanged;
			videoPlayerVM.OpenFileSet (projectVM.FileSet);
		}

		public override async Task Stop ()
		{
			await base.Stop ();
			App.Current.EventsBroker.Unsubscribe<SaveEvent<ProjectVM>> (HandleSave);
			projectVM.Periods.PropertyChanged -= HandlePropertyChanged;
			projectVM.FileSet.PropertyChanged -= HandlePropertyChanged;
		}

		public override void SetViewModel (IViewModel viewModel)
		{
			cameraSynchronizationVM = (CameraSynchronizationVM)viewModel;
			if (viewModel != null) {
				videoPlayerVM = cameraSynchronizationVM.VideoPlayer;
				projectVM = cameraSynchronizationVM.Project;
				InitPeriods ();
			}
		}

		public override IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return new KeyAction [] {
				// These key actions override the video player ones, so we want to have them highest priority.
				new KeyAction (
					App.Current.HotkeysService.GetByName("PLAYER_SEEK_LEFT_SHORT"),
					() => PerformStep (false), 1
				),
				new KeyAction (
					App.Current.HotkeysService.GetByName("PLAYER_SEEK_RIGHT_SHORT"),
					() => PerformStep (true), 1
				),
			};
		}

		protected void SaveChanges (SaveEvent<ProjectVM> saveEvent)
		{
			if (projectVM != saveEvent.Object) {
				return;
			}

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

			if (cameraSynchronizationVM.SynchronizeEventsWithPeriods) {
				projectVM.Model.ResyncEvents (cameraSynchronizationVM.InitialPeriods);
			}
		}

		protected virtual void HandleSave (SaveEvent<ProjectVM> saveEvent)
		{
			SaveChanges (saveEvent);
			App.Current.StateController.MoveBack ();
		}

		void InitPeriods ()
		{
			var gamePeriods = projectVM.Dashboard.GamePeriods;

			MediaFileSetVM fileSet = projectVM.FileSet;
			var start = new Time (0);
			var file = fileSet.FirstOrDefault ();
			var duration = file.Duration;
			var pDuration = new Time (duration.MSeconds / gamePeriods.Count);

			if (projectVM.Periods == null || projectVM.Periods.ViewModels.Count == 0) {
				// If no periods are provided create the default ones
				// defined in the dashboard
				cameraSynchronizationVM.InitialPeriods = new ObservableCollection<Period> ();
				foreach (string s in gamePeriods) {
					Period period = new Period { Name = s };
					period.Start (start);
					period.Stop (start + pDuration);
					cameraSynchronizationVM.InitialPeriods.Add (period);
					start += pDuration;
				}
				projectVM.Periods.Model.Reset (cameraSynchronizationVM.InitialPeriods);
			} else {
				// Create a copy of the project periods and keep the
				// project ones to resynchronize the events in SaveChanges()
				cameraSynchronizationVM.InitialPeriods = projectVM.Periods.Model.Clone ();
			}
		}

		void PerformStep (bool isForward)
		{
			MediaFileVM file = projectVM.FileSet.FirstOrDefault (f => f.SelectedGrabber == SelectionPosition.All);
			if (file != null) {
				var oneFrame = new Time (1000 / file.Fps);
				if (isForward) {
					file.Offset = file.Offset + oneFrame;
				} else {
					file.Offset = file.Offset - oneFrame;
				}
			} else {
				if (isForward) {
					videoPlayerVM.SeekToNextFrameCommand.Execute ();
				} else {
					videoPlayerVM.SeekToPreviousFrameCommand.Execute ();
				}
			}
		}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (sender is TimeNodeVM) {
				TimeNodeVM timenode = sender as TimeNodeVM;
				Time seekTime = null;

				if (e.PropertyName == nameof (TimeNode.Start)) {
					seekTime = timenode.Start;
				} else if (e.PropertyName == nameof (TimeNode.Stop)) {
					seekTime = timenode.Stop;
				}
				if (seekTime != null) {
					videoPlayerVM.PauseCommand.Execute (false);
					videoPlayerVM.SeekCommand.Execute (new VideoPlayerSeekOptions (seekTime));
				}
			}
			if (sender is MediaFileVM) {
				if (e.PropertyName == nameof (MediaFile.Offset)) {
					videoPlayerVM.PauseCommand.Execute (false);
					videoPlayerVM.SeekCommand.Execute (new VideoPlayerSeekOptions (videoPlayerVM.CurrentTime, true));
				}
			}
		}
	}
}
