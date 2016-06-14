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
using System.Linq;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Filters;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using Timer = System.Threading.Timer;

namespace VAS.Services
{
	public class PlaylistManager: IService
	{
		EventsFilter filter;

		public IPlayerController Player {
			get;
			set;
		}

		public Project OpenedProject {
			get;
			set;
		}

		public ProjectType OpenedProjectType {
			get;
			set;
		}

		protected virtual void LoadPlay (TimelineEvent play, Time seekTime, bool playing)
		{
			if (play != null && Player != null) {
				play.Selected = true;
				Player.LoadEvent (
					play, seekTime, playing);
				if (playing) {
					Player.Play ();
				}
			}
		}

		protected virtual void HandleOpenedProjectChanged (Project project, ProjectType projectType,
		                                                   EventsFilter filter, IAnalysisWindowBase analysisWindow)
		{
			var player = analysisWindow?.Player;
			if (player == null && Player == null) {
				return;
			} else if (player != null) {
				Player = player;
			}

			OpenedProject = project;
			OpenedProjectType = projectType;
			this.filter = filter;
			Player.LoadedPlaylist = null;
		}

		/// <summary>
		/// Set the playlistManager with a presentation (<see cref="Playlist"/> not attached to a <see cref="Project"/>)
		/// If player is null, the last one will be used (if there is one).
		/// </summary>
		/// <param name="presentation">Presentation.</param>
		/// <param name="player">Player.</param>
		protected virtual void HandleOpenedPresentationChanged (Playlist presentation, IPlayerController player)
		{
			if (player == null && Player == null) {
				return;
			} else if (player != null) {
				Player = player;
			}

			OpenedProject = null;
			Player.Switch (null, presentation, null);

			OpenedProjectType = ProjectType.None;
			filter = null;
		}

		protected virtual void HandleLoadPlaylistElement (Playlist playlist, IPlaylistElement element, bool playing = false)
		{
			if (element != null) {
				playlist.SetActive (element);
			}
			if (playlist.Elements.Count > 0 && Player != null)
				Player.LoadPlaylistEvent (playlist, element, playing);
		}

		protected virtual void HandlePlayChanged (TimeNode tNode, Time time)
		{
			if (tNode is TimelineEvent) {
				LoadPlay (tNode as TimelineEvent, time, false);
				if (filter != null) {
					filter.Update ();
				}
			}
		}

		protected virtual void HandleLoadPlayEvent (TimelineEvent play)
		{
			if (OpenedProject == null || OpenedProjectType == ProjectType.FakeCaptureProject) {
				return;
			}

			if (play?.Duration.MSeconds == 0) {
				// These events don't have duration, we start playing as if it was a seek
				Player.Switch (null, null, null);
				Player.UnloadCurrentEvent ();
				Player.Seek (play.EventTime, true);
				Player.Play ();
			} else {
				if (play != null) {
					LoadPlay (play, new Time (0), true);
				} else if (Player != null) {
					Player.UnloadCurrentEvent ();
				}
			}
		}

		protected virtual void HandleNext (Playlist playlist)
		{
			Player.Next ();
		}

		protected virtual void HandlePrev (Playlist playlist)
		{
			Player.Previous ();
		}

		protected virtual void HandlePlaybackRateChanged (float rate)
		{
		}

		protected virtual void HandleAddPlaylistElement (Playlist playlist, List<IPlaylistElement> element)
		{
			if (playlist == null) {
				playlist = HandleNewPlaylist (OpenedProject);
				if (playlist == null) {
					return;
				}
			}

			foreach (var item in element) {
				playlist.Elements.Add (item);
			}
		}

		protected virtual Playlist HandleNewPlaylist (Project project)
		{
			string name = Catalog.GetString ("New playlist");
			Playlist playlist = null;
			bool done = false;
			if (project != null) {
				while (name != null && !done) {
					name = App.Current.GUIToolkit.QueryMessage (Catalog.GetString ("Playlist name:"), null, name).Result;
					if (name != null) {
						done = true;
						if (project.Playlists.Any (p => p.Name == name)) {
							string msg = Catalog.GetString ("A playlist already exists with the same name");
							App.Current.GUIToolkit.ErrorMessage (msg);
							done = false;
						}
					}
				}
				if (name != null) {
					playlist = new Playlist { Name = name };
					project.Playlists.Add (playlist);
				}
			}
			return playlist;
		}

		protected virtual void HandleRenderPlaylist (Playlist playlist)
		{
			List<EditionJob> jobs = App.Current.GUIToolkit.ConfigureRenderingJob (playlist);
			if (jobs == null)
				return;
			foreach (Job job in jobs)
				App.Current.RenderingJobsManger.AddJob (job);
		}

		protected virtual void HandleTogglePlayEvent (bool playing)
		{
			if (Player != null) {
				if (playing) {
					Player.Play ();
				} else {
					Player.Pause ();
				}
			}
		}

		#region IService

		public virtual int Level {
			get {
				return 80;
			}
		}

		public virtual string Name {
			get {
				return "Playlists";
			}
		}

		public virtual bool Start ()
		{
			App.Current.EventsBroker.NewPlaylistEvent += HandleNewPlaylist;
			App.Current.EventsBroker.AddPlaylistElementEvent += HandleAddPlaylistElement;
			App.Current.EventsBroker.RenderPlaylist += HandleRenderPlaylist;
			App.Current.EventsBroker.OpenedProjectChanged += HandleOpenedProjectChanged;
			App.Current.EventsBroker.OpenedPresentationChanged += HandleOpenedPresentationChanged;
			App.Current.EventsBroker.PreviousPlaylistElementEvent += HandlePrev;
			App.Current.EventsBroker.NextPlaylistElementEvent += HandleNext;
			App.Current.EventsBroker.LoadEventEvent += HandleLoadPlayEvent;
			App.Current.EventsBroker.LoadPlaylistElementEvent += HandleLoadPlaylistElement;
			App.Current.EventsBroker.PlaybackRateChanged += HandlePlaybackRateChanged;
			App.Current.EventsBroker.TimeNodeChanged += HandlePlayChanged;
			App.Current.EventsBroker.TogglePlayEvent += HandleTogglePlayEvent;

			return true;
		}

		public virtual bool Stop ()
		{
			App.Current.EventsBroker.NewPlaylistEvent -= HandleNewPlaylist;
			App.Current.EventsBroker.AddPlaylistElementEvent -= HandleAddPlaylistElement;
			App.Current.EventsBroker.RenderPlaylist -= HandleRenderPlaylist;
			App.Current.EventsBroker.OpenedProjectChanged -= HandleOpenedProjectChanged;
			App.Current.EventsBroker.OpenedPresentationChanged -= HandleOpenedPresentationChanged;
			App.Current.EventsBroker.PreviousPlaylistElementEvent -= HandlePrev;
			App.Current.EventsBroker.NextPlaylistElementEvent -= HandleNext;
			App.Current.EventsBroker.LoadEventEvent -= HandleLoadPlayEvent;
			App.Current.EventsBroker.LoadPlaylistElementEvent -= HandleLoadPlaylistElement;
			App.Current.EventsBroker.PlaybackRateChanged -= HandlePlaybackRateChanged;
			App.Current.EventsBroker.TimeNodeChanged -= HandlePlayChanged;
			App.Current.EventsBroker.TogglePlayEvent -= HandleTogglePlayEvent;

			return true;
		}

		#endregion
	}
}
