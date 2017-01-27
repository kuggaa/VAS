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
using System;
using System.Diagnostics;
using System.Collections.Generic;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Store.Playlists;

namespace VAS.Services
{
	/// <summary>
	/// This service collects the user usage statistics per session.
	/// </summary>
	public abstract class UserStatisticsService : IService
	{
		string currentState;
		Stopwatch stateTimer;
		Stopwatch generalTimer;

		public UserStatisticsService ()
		{
			ProjectDictionary = new Dictionary<Guid, Tuple<int, int>> ();
			DataDictionary = new Dictionary<string, double> ();
			stateTimer = new Stopwatch ();
			generalTimer = new Stopwatch ();
			TimerList = new List<Tuple<string, long>> ();
		}

		/// <summary>
		/// Gets or sets the states to track.
		/// </summary>
		/// <value>The states to track.</value>
		protected List<string> StatesToTrack {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the total user teams.
		/// </summary>
		/// <value>The total user teams.</value>
		protected int TotalUserTeams {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the total user projects.
		/// </summary>
		/// <value>The total user projects.</value>
		protected int TotalUserProjects {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the total user playlists.
		/// </summary>
		/// <value>The total user playlists.</value>
		protected int TotalUserPlaylists {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the teams amount.
		/// </summary>
		/// <value>The teams amount.</value>
		public int TeamsCount {
			get;
			protected set;
		}

		/// <summary>
		/// Gets the playlists amount.
		/// </summary>
		/// <value>The playlists amount.</value>
		public int PlaylistsCount {
			get;
			private set;
		}

		/// <summary>
		/// Gets the renders amount.
		/// </summary>
		/// <value>The renders amount.</value>
		public int RendersCount {
			get;
			private set;
		}

		/// <summary>
		/// Gets the manual tags amount.
		/// </summary>
		/// <value>The manual tags amount.</value>
		public int ManualEventsCount {
			get;
			private set;
		}

		/// <summary>
		/// Gets the drawings amount.
		/// </summary>
		/// <value>The drawings amount.</value>
		public int DrawingsCount {
			get;
			private set;
		}

		/// <summary>
		/// Gets the created projects.
		/// </summary>
		/// <value>The created projects.</value>
		public int CreatedProjects {
			get;
			private set;
		}

		/// <summary>
		/// Gets the project dictionary.
		/// </summary>
		/// <value>The project dictionary.</value>
		public Dictionary<Guid, Tuple<int, int>> ProjectDictionary {
			get;
			private set;
		}

		/// <sumary>
		/// Gets the data dictionary.
		/// </summary>
		/// <value>The data dictionary.</value>
		public Dictionary<string, double> DataDictionary {
			get;
			protected set;
		}

		/// <summary>
		/// Gets the timer list.
		/// </summary>
		/// <value>The timer list.</value>
		public List<Tuple<string, long>> TimerList {
			get;
			private set;
		}

		#region IService implementation

		/// <summary>
		/// Gets the level of the service. Services are started in ascending level order and stopped in descending level order.
		/// </summary>
		/// <value>The level.</value>
		public int Level {
			get {
				return 21; //Always set a higher level than DatabaseManager
			}
		}

		/// <summary>
		/// Gets the name of the service
		/// </summary>
		/// <value>The name of the service.</value>
		public string Name {
			get {
				return "UserStatistics";
			}
		}

		/// <summary>
		/// Starts the service.
		/// </summary>
		public virtual bool Start ()
		{
			App.Current.EventsBroker.Subscribe<NewPlaylistEvent> (HandleNewPlaylistEvent);
			App.Current.EventsBroker.Subscribe<JobRenderedEvent> (HandleCreateJob);
			App.Current.EventsBroker.Subscribe<NewDashboardEvent> (HandleDashboardEvent);
			App.Current.EventsBroker.Subscribe<DrawingSavedToProjectEvent> (HandleDrawingSavedToProject);
			App.Current.EventsBroker.Subscribe<ProjectCreatedEvent> (HandleNewProject);
			App.Current.EventsBroker.Subscribe<OpenedProjectEvent> (HandleOpenProject);
			App.Current.EventsBroker.Subscribe<NavigationEvent> (HandleNavigationEvent);
			generalTimer.Start ();

			return true;
		}

		/// <summary>
		/// Stops the service.
		/// </summary>
		public virtual bool Stop ()
		{
			SaveTimer ();
			App.Current.EventsBroker.Unsubscribe<NewPlaylistEvent> (HandleNewPlaylistEvent);
			App.Current.EventsBroker.Unsubscribe<JobRenderedEvent> (HandleCreateJob);
			App.Current.EventsBroker.Unsubscribe<NewDashboardEvent> (HandleDashboardEvent);
			App.Current.EventsBroker.Unsubscribe<DrawingSavedToProjectEvent> (HandleDrawingSavedToProject);
			App.Current.EventsBroker.Unsubscribe<ProjectCreatedEvent> (HandleNewProject);
			App.Current.EventsBroker.Unsubscribe<OpenedProjectEvent> (HandleOpenProject);
			App.Current.EventsBroker.Unsubscribe<NavigationEvent> (HandleNavigationEvent);
			generalTimer.Stop ();
			RetrieveUserData ();
			SendData ();

			return true;
		}

		#endregion

		/// <summary>
		/// Retrieves the total user statistics data.
		/// </summary>
		public virtual void RetrieveUserData ()
		{
			TotalUserPlaylists = App.Current.DatabaseManager.ActiveDB.Count<Playlist> ();
		}

		/// <summary>
		/// Loads the current session project values.
		/// </summary>
		/// <param name="projectId">Project identifier.</param>
		void LoadSessionProjectValues (Guid projectId)
		{
			if (!ProjectDictionary.ContainsKey (projectId)) {
				ProjectDictionary.Add (projectId, new Tuple<int, int> (ManualEventsCount, DrawingsCount));
			} else {
				ManualEventsCount = ProjectDictionary [projectId].Item1;
				DrawingsCount = ProjectDictionary [projectId].Item2;
			}
		}

		/// <summary>
		/// Saves the current state time spent on.
		/// </summary>
		void SaveTimer ()
		{
			stateTimer.Stop ();
			TimerList.Add (new Tuple<string, long> (currentState, stateTimer.ElapsedTicks));
			stateTimer.Reset ();
		}

		/// <summary>
		/// Tracks the current session project data to HockeyApp.
		/// </summary>
		void TrackProjects ()
		{
			foreach (var item in ProjectDictionary) {
				TrackProject (item.Key.ToString (), item.Value.Item1, item.Value.Item2);
			}
		}

		/// <summary>
		/// Tracks the project given data.
		/// </summary>
		/// <param name="ProjectId">Project identifier.</param>
		/// <param name="tags">Tags.</param>
		/// <param name="drawings">Drawings.</param>
		void TrackProject (string ProjectId, int events, int drawings)
		{
			App.Current.KPIService.TrackEvent ("Project_usage",
											   new Dictionary<string, string> () {
				{ "Project_id", ProjectId } },
											   new Dictionary<string, double> () {
				{ "Events", events },
				{ "Drawings" , drawings } });
		}

		/// <summary>
		/// Tracks the time spent on each state to HockeyApp.
		/// </summary>
		void TrackTimers ()
		{
			foreach (var item in TimerList) {
				App.Current.KPIService.TrackEvent ("PageView_" + item.Item1, null,
												   new Dictionary<string, double> () { { "Time", item.Item2 } });
			}
		}

		/// <summary>
		/// Tracks the service collected data to HockeyApp.
		/// </summary>
		public virtual void SendData ()
		{
			TrackProjects ();
			TrackTimers ();
			DataDictionary.Add ("Teams", TeamsCount);
			DataDictionary.Add ("Renders", RendersCount);
			DataDictionary.Add ("Playlists", PlaylistsCount);
			DataDictionary.Add ("Projects", CreatedProjects);
			DataDictionary.Add ("Total_playlists", TotalUserPlaylists);
			DataDictionary.Add ("Time", ((int)generalTimer.ElapsedMilliseconds) / 1000);
			App.Current.KPIService.TrackEvent ("Sessions", null, DataDictionary);
			App.Current.KPIService.Flush ();
		}

		#region Handlers

		/// <summary>
		/// Handles when navigation event has been thrown.
		/// </summary>
		/// <param name="evt">Evt.</param>
		void HandleNavigationEvent (NavigationEvent evt)
		{
			string NextEvent = evt.Name;

			if (!string.IsNullOrEmpty (currentState) && (NextEvent != currentState)) {
				SaveTimer ();
			}
			if (StatesToTrack.Contains (NextEvent)) {
				stateTimer.Start ();
				currentState = NextEvent;
			}
		}

		/// <summary>
		/// Handles when a new project has been created.
		/// </summary>
		/// <param name="evt">Evt.</param>
		void HandleNewProject (ProjectCreatedEvent evt)
		{
			CreatedProjects++;
			ManualEventsCount = 0;
			DrawingsCount = 0;
		}

		/// <summary>
		/// Handles when a project has been opened.
		/// </summary>
		/// <param name="evt">Evt.</param>
		void HandleOpenProject (OpenedProjectEvent evt)
		{
			LoadSessionProjectValues (evt.Project.ID);
		}

		/// <summary>
		/// Handles when a dashboard event has been throwed.
		/// </summary>
		/// <param name="evt">Evt.</param>
		void HandleDashboardEvent (NewDashboardEvent evt)
		{
			ManualEventsCount++;
			ProjectDictionary [evt.ProjectId] = new Tuple<int, int> (ManualEventsCount, DrawingsCount);
		}

		/// <summary>
		/// Handles when a drawing has been saved to a project.
		/// </summary>
		/// <param name="evt">Evt.</param>
		void HandleDrawingSavedToProject (DrawingSavedToProjectEvent evt)
		{
			DrawingsCount++;
			ProjectDictionary [evt.ProjectId] = new Tuple<int, int> (ManualEventsCount, DrawingsCount);
		}

		/// <summary>
		/// Handles when a playlist has been created.
		/// </summary>
		/// <param name="evt">Evt.</param>
		void HandleNewPlaylistEvent (NewPlaylistEvent evt)
		{
			PlaylistsCount++;
		}

		/// <summary>
		/// Handles when a job/render has been created.
		/// </summary>
		/// <param name="evt">Evt.</param>
		void HandleCreateJob (JobRenderedEvent evt)
		{
			RendersCount++;
		}

		#endregion
	}
}
