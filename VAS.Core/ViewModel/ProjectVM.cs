//
//  Copyright (C) 2016 Fluendo S.A.
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
using System.Threading.Tasks;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Core.ViewModel
{

	/// <summary>
	/// A ViewModel for <see cref="Project"/> objects.
	/// </summary>
	public class ProjectVM : ViewModelBase<Project>
	{
		Project model;

		public ProjectVM ()
		{
			Timers = new CollectionViewModel<Timer, TimerVM> ();
			Timeline = new TimelineVM ();
			Playlists = new PlaylistCollectionVM ();
			EventTypes = new CollectionViewModel<EventType, EventTypeVM> ();
			FileSet = new MediaFileSetVM ();
			Periods = new CollectionViewModel<Period, PeriodVM> ();
			Dashboard = new DashboardVM ();
		}

		public override Project Model {
			get {
				return model;
			}
			set {
				model = value;
				UpdateModels ();
			}
		}

		/// <summary>
		/// Gets the collection of periods in the project.
		/// </summary>
		/// <value>The timers.</value>
		public CollectionViewModel<Period, PeriodVM> Periods {
			get;
			private set;
		}

		/// <summary>
		/// Gets the collection of timers in the project.
		/// </summary>
		/// <value>The timers.</value>
		public CollectionViewModel<Timer, TimerVM> Timers {
			get;
			private set;
		}

		/// <summary>
		/// Gets the collection of event types in the project.
		/// </summary>
		/// <value>The event types.</value>
		public CollectionViewModel<EventType, EventTypeVM> EventTypes {
			get;
			private set;
		}

		/// <summary>
		/// Gets collection of playlists in the project.
		/// </summary>
		/// <value>The playlists.</value>
		public PlaylistCollectionVM Playlists {
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets the event types timeline vm.
		/// </summary>
		/// <value>The event types timeline vm.</value>
		public TimelineVM Timeline {
			get;
			protected set;
		}

		/// <summary>
		/// Gets or sets the file set.
		/// </summary>
		/// <value>The file set.</value>
		public MediaFileSetVM FileSet {
			get;
			protected set;
		}

		/// <summary>
		/// Gets or sets the file set.
		/// </summary>
		/// <value>The file set.</value>
		public DashboardVM Dashboard {
			get;
			protected set;
		}

		/// <summary>
		/// Gets the short description of the project.
		/// </summary>
		/// <value>The short description.</value>
		public string ShortDescription {
			get {
				return Model.ShortDescription;
			}
		}

		public virtual IEnumerable<PlayerVM> Players {
			get;
			internal set;
		}

		public virtual IEnumerable<TeamVM> Teams {
			get;
			internal set;
		}

		/// <summary>
		/// Gets a value indicating whether the project has been edited.
		/// </summary>
		/// <value><c>true</c> if edited; otherwise, <c>false</c>.</value>
		public bool Edited {
			get {
				return Model?.IsChanged == true;
			}
		}

		/// <summary>
		/// Gets or sets the type of the project.
		/// </summary>
		/// <value>The type of the project.</value>
		public ProjectType ProjectType {
			get {
				return Model.ProjectType;
			}
			set {
				Model.ProjectType = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:VAS.Core.ViewModel.ProjectVM"/> is live.
		/// </summary>
		/// <value><c>true</c> if is live; otherwise, <c>false</c>.</value>
		public bool IsLive {
			get {
				return ProjectType == ProjectType.CaptureProject ||
												 ProjectType == ProjectType.URICaptureProject ||
												 ProjectType == ProjectType.FakeCaptureProject;

			}
		}

		/// <summary>
		/// Updates the model in the child ViewModel. Super classes should override this function to update their
		/// own child ViewModel.
		/// </summary>
		protected virtual void UpdateModels ()
		{
			Playlists.Model = Model.Playlists;
			EventTypes.Model = Model.EventTypes;
			Timers.Model = Model.Timers;
			Periods.Model = Model.Periods;
			Timeline.CreateEventTypeTimelines (EventTypes);
			Timeline.Model = Model.Timeline;
			FileSet.Model = Model.FileSet;
			Dashboard.Model = Model.Dashboard;
		}
	}

	public class ProjectVM<TProject> : ProjectVM, IViewModel<TProject>
		where TProject : Project
	{

		public new TProject Model {
			get {
				return base.Model as TProject;
			}
			set {
				base.Model = value;
			}
		}

		/// <summary>
		/// Command to export a project.
		/// </summary>
		public Task<bool> Export ()
		{
			return App.Current.EventsBroker.PublishWithReturn (new ExportEvent<TProject> { Object = Model });
		}

		/// <summary>
		/// Command to delete a project.
		/// </summary>
		public Task<bool> Delete ()
		{
			return App.Current.EventsBroker.PublishWithReturn (new DeleteEvent<TProject> { Object = Model });
		}

		/// <summary>
		/// Command to save a project.
		/// </summary>
		/// <param name="force">If set to <c>true</c> does not prompt to save.</param>
		public Task<bool> Save (bool force)
		{
			return App.Current.EventsBroker.PublishWithReturn (new UpdateEvent<TProject> { Object = Model, Force = force });
		}
	}
}
