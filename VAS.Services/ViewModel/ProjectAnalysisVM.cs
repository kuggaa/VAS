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
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;

namespace VAS.Services.ViewModel
{
	/// <summary>
	/// A ViewModel used in analysis views where there is a video player, a timeline and playlists
	/// </summary>
	public class ProjectAnalysisVM<T> : ViewModelBase<T>, IAnalysisViewModel
		where T : ProjectVM
	{
		public ProjectAnalysisVM ()
		{
		}

		public static implicit operator DashboardVM (ProjectAnalysisVM<T> projectAnalysisVM)
		{
			return projectAnalysisVM?.Project.Dashboard;
		}

		/// <summary>
		/// Cast to PlaylistCollectionVM explicit operator.
		/// </summary>
		/// <returns>PlaylistCollectionVM.</returns>
		/// <param name="vm">The AnalysisVM view model.</param>
		public static implicit operator PlaylistCollectionVM (ProjectAnalysisVM<T> projectAnalysisVM)
		{
			return projectAnalysisVM?.Project.Playlists;
		}

		/// <summary>
		/// Cast to ProjectVM<Project> explicit operator.
		/// </summary>
		/// <returns>ProjectVM<Project>.</returns>
		/// <param name="vm">The AnalysisVM view model.</param>
		public static implicit operator T (ProjectAnalysisVM<T> projectAnalysisVM)
		{
			return projectAnalysisVM?.Project;
		}

		/// <summary>
		/// Cast to RAEventTypeCollectionVM explicit operator.
		/// </summary>
		/// <returns>The explicit.</returns>
		/// <param name="vm">Vm.</param>
		public static implicit operator TimelineVM (ProjectAnalysisVM<T> projectAnalysisVM)
		{
			return projectAnalysisVM?.Project.Timeline;
		}

		/// <summary>
		/// Cast to VideoPlayerVM explicit operator.
		/// </summary>
		/// <returns>Video player ViewModel</returns>
		/// <param name="vm">The AnalysisVM view model.</param>
		public static implicit operator VideoPlayerVM (ProjectAnalysisVM<T> projectAnalysisVM)
		{
			return projectAnalysisVM?.VideoPlayer;
		}

		/// <summary>
		/// Gets or sets the project used in the analysis
		/// </summary>
		/// <value>The project.</value>
		public T Project {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the main video player.
		/// </summary>
		/// <value>The player vm.</value>
		public VideoPlayerVM VideoPlayer {
			get;
			set;
		}

		/// <summary>
		/// Close the current project.
		/// </summary>
		/// <value>The close command.</value>
		public Command CloseCommand {
			get;
			set;
		}

		/// <summary>
		/// Save the current project.
		/// </summary>
		/// <value>The save command.</value>
		public Command SaveCommand {
			get;
			set;
		}

		ProjectVM IAnalysisViewModel.Project {
			get {
				return Project;
			}
		}
	}
}
