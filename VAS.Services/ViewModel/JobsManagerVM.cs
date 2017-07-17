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
using System.Linq;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;

namespace VAS.Services.ViewModel
{
	public class JobsManagerVM : CollectionViewModel<Job, JobVM>, IJobsManager
	{
		public JobsManagerVM ()
		{
			CancelCommand = new Command<IEnumerable<Job>> (Cancel);
			CancelCommand.Icon = App.Current.ResourcesLocator.LoadIcon ("vas-close", App.Current.Style.IconSmallWidth);
			ClearFinishedCommand = new Command (ClearFinished);
			ClearFinishedCommand.Icon = App.Current.ResourcesLocator.LoadIcon ("vas-delete", App.Current.Style.IconSmallWidth);
			ClearFinishedCommand.Text = Catalog.GetString ("Clear finished jobs");
			CancelSelectedCommand = new Command (CancelSelected);
			CancelSelectedCommand.Icon = App.Current.ResourcesLocator.LoadIcon ("vas-close", App.Current.Style.IconSmallWidth);
			CancelSelectedCommand.Text = Catalog.GetString ("Cancel job");
			RetryCommand = new Command (RetrySelected);
			RetryCommand.Icon = App.Current.ResourcesLocator.LoadIcon (StyleConf.RetryIcon, App.Current.Style.IconSmallWidth);
			RetryCommand.Text = Catalog.GetString ("Retry job");


		}
		/// <summary>
		/// Gets or sets the cancel command.
		/// </summary>
		/// <value>The cancel command.</value>
		public Command CancelCommand {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the clear finished command.
		/// </summary>
		/// <value>The clear finished command.</value>
		public Command ClearFinishedCommand {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the cancel selected command.
		/// </summary>
		/// <value>The cancel selected command.</value>
		public Command CancelSelectedCommand {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the retry command.
		/// </summary>
		/// <value>The retry command.</value>
		public Command RetryCommand {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the current job.
		/// </summary>
		/// <value>The current job.</value>
		public JobVM CurrentJob {
			get;
			protected set;
		} = new JobVM ();

		/// <summary>
		/// Gets the list of pending jobs in the queue.
		/// </summary>
		/// <value>The pending jobs.</value>
		public List<JobVM> PendingJobs {
			get {
				return ViewModels.Where (j => j.State == JobState.Pending).ToList ();
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:VAS.Services.ViewModel.JobsManagerVM"/> is busy with
		/// a job running.
		/// </summary>
		/// <value><c>true</c> if is busy; otherwise, <c>false</c>.</value>
		public bool IsBusy {
			get {
				return CurrentJob.Model != null;
			}
		}

		#region IJobsManager

		public void Add (Job job)
		{
			App.Current.EventsBroker.Publish (new CreateEvent<Job> { Object = job });
		}

		public void Retry (IEnumerable<Job> jobs)
		{
			App.Current.EventsBroker.Publish (new RetryEvent<IEnumerable<Job>> { Object = jobs });
		}

		public void Cancel (IEnumerable<Job> jobs = null)
		{
			App.Current.EventsBroker.Publish (new CancelEvent<IEnumerable<Job>> { Object = jobs });
		}

		public void CancelAll ()
		{
			Cancel (Model.Where (j => j.State == JobState.Pending || j.State == JobState.Running).ToList ());
		}

		public void ClearFinished ()
		{
			App.Current.EventsBroker.Publish (new ClearEvent<Job> ());
		}

		void RetrySelected ()
		{
			App.Current.EventsBroker.Publish (new RetryEvent<IEnumerable<Job>> {
				Object = Selection.Select (jvm => jvm.Model).ToList ()
			});
		}

		void CancelSelected ()
		{
			App.Current.EventsBroker.Publish (new CancelEvent<IEnumerable<Job>> {
				Object = Selection.Select (jvm => jvm.Model).ToList ()
			});
		}
		#endregion
	}
}
