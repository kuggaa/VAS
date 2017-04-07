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
using System.Collections.ObjectModel;
using System.Linq;
using VAS.Core.Common;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Services.ViewModel
{
	public class CameraSynchronizationVM : ViewModelBase
	{
		ProjectVM projectVM;

		public CameraSynchronizationVM ()
		{
			ApplyChanges = new Command<bool> (HandleApplyChanges);
		}

		public Command<bool> ApplyChanges {
			get;
			set;
		}

		public ProjectVM Project {
			get {
				return projectVM;
			}
			set {
				projectVM = value;
				if (projectVM != null) {
					FixedPeriods = projectVM.Model.IsFakeCapture;
					InitPeriods ();
				}
			}
		}

		public VideoPlayerVM VideoPlayer {
			get;
			set;
		}

		public bool FixedPeriods {
			get;
			set;
		}

		public int SecondsPerPixel {
			get;
			set;
		}

		public ObservableCollection<Period> InitialPeriods {
			get;
			set;
		}

		void InitPeriods ()
		{
			var gamePeriods = Project.Dashboard.GamePeriods;

			MediaFileSetVM fileSet = Project.FileSet;
			var start = new Time (0);
			var file = fileSet.FirstOrDefault ();
			var duration = file.Duration;
			var pDuration = new Time (duration.MSeconds / gamePeriods.Count);

			if (Project.Periods == null || Project.Periods.ViewModels.Count == 0) {
				/* If no periods are provided create the default ones
				 * defined in the dashboard */
				InitialPeriods = new ObservableCollection<Period> ();
				foreach (string s in gamePeriods) {
					Period period = new Period { Name = s };
					period.Start (start);
					period.Stop (start + pDuration);
					InitialPeriods.Add (period);
					start += pDuration;
				}
				Project.Periods.Model.Replace (InitialPeriods);
			} else {
				/* Create a copy of the project periods and keep the
				 * project ones to resynchronize the events in SaveChanges() */
				InitialPeriods = Project.Periods.Model.Clone ();
			}
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
				projectVM.Model.ResyncEvents (InitialPeriods);
			}
		}

	}
}
