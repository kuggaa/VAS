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
using System.Collections.ObjectModel;
using VAS.Core.Events;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Services.ViewModel
{
	/// <summary>
	/// A ViewModel to synchronize different cameras in a project.
	/// </summary>
	public class CameraSynchronizationVM : ViewModelBase, IVideoPlayerDealer
	{
		ProjectVM projectVM;

		public CameraSynchronizationVM ()
		{
			Save = new Command (() => {
				App.Current.EventsBroker.Publish (new SaveEvent<ProjectVM> { Object = Project });
			});
		}

		/// <summary>
		/// Gets or sets the save command to apply changes.
		/// </summary>
		/// <value>The save command.</value>
		public Command Save {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the project to edit.
		/// </summary>
		/// <value>The project.</value>
		public ProjectVM Project {
			get {
				return projectVM;
			}
			set {
				projectVM = value;
			}
		}

		/// <summary>
		/// Gets or sets the video player.
		/// </summary>
		/// <value>The video player.</value>
		public VideoPlayerVM VideoPlayer {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Services.ViewModel.CameraSynchronizationVM"/>
		/// has fixed periods. When periods are fixed the duration can't be edited, they can only be moved.
		/// </summary>
		/// <value><c>true</c> if fixed periods; otherwise, <c>false</c>.</value>
		public bool FixedPeriods {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Services.ViewModel.CameraSynchronizationVM"/>
		/// should resynchronize events when applying changes. This is used after synchronization the project periods
		/// in fake live projects, where events needs to be resyncrhonize to the new periods boundaries.
		/// </summary>
		/// <value><c>true</c> if resynchronize events; otherwise, <c>false</c>.</value>
		public bool SynchronizeEventsWithPeriods {
			get;
			set;
		}

		/// <summary>
		/// A copy of the initial periods.
		/// </summary>
		/// <value>The initial periods.</value>
		public ObservableCollection<Period> InitialPeriods {
			get;
			set;
		}
	}
}
