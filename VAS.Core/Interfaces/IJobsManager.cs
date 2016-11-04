// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using VAS.Core.Common;

namespace VAS.Core.Interfaces
{
	public interface IJobsManager
	{
		/// <summary>
		/// Add a new job to the queue.
		/// </summary>
		/// <param name="job">Job.</param>
		void Add (Job job);

		/// <summary>
		/// Retry the list of jobs.
		/// </summary>
		/// <param name="jobs">Jobs.</param>
		void Retry (IEnumerable<Job> jobs);

		/// <summary>
		/// Cancel the list of jobs. If <paramref name="jobs"/> is <c>null</c> cancell the current job.
		/// </summary>
		/// <param name="jobs">Cancel jobs.</param>
		void Cancel (IEnumerable<Job> jobs = null);

		/// <summary>
		/// Cancels all jobs.
		/// </summary>
		void CancelAll ();

		/// <summary>
		/// Clears all finished jobs.
		/// </summary>
		void ClearFinished ();
	}
}

