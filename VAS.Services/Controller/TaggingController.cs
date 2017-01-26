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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.Store.Templates;
using VAS.Core.ViewModel;

namespace VAS.Services.Controller
{
	public abstract class TaggingController : DisposableBase, IController
	{
		protected ProjectVM project;
		protected VideoPlayerVM videoPlayer;
		protected ICapturerBin capturer;

		/// <summary>
		/// Gets or sets the video player view model
		/// </summary>
		/// <value>The video player.</value>
		protected VideoPlayerVM VideoPlayer {
			get {
				return videoPlayer;
			}

			set {
				if (videoPlayer != null) {
					videoPlayer.PropertyChanged -= HandleVideoPlayerPropertyChanged;
				}
				videoPlayer = value;
				if (videoPlayer != null) {
					videoPlayer.PropertyChanged += HandleVideoPlayerPropertyChanged;
				}
			}
		}

		/// <summary>
		/// Start this instance.
		/// </summary>
		public void Start ()
		{
			App.Current.EventsBroker.Subscribe<ClickedPCardEvent> (HandleClickedPCardEvent);
			App.Current.EventsBroker.Subscribe<NewTagEvent> (HandleNewTagEvent);
		}

		/// <summary>
		/// Stop this instance.
		/// </summary>
		public void Stop ()
		{
			App.Current.EventsBroker.Unsubscribe<ClickedPCardEvent> (HandleClickedPCardEvent);
			App.Current.EventsBroker.Unsubscribe<NewTagEvent> (HandleNewTagEvent);
		}

		/// <summary>
		/// Sets the view model.
		/// </summary>
		/// <param name="viewModel">View model.</param>
		public void SetViewModel (IViewModel viewModel)
		{
			project = (ProjectVM)(viewModel as dynamic);
			VideoPlayer = (VideoPlayerVM)(viewModel as dynamic);
			try {
				capturer = (ICapturerBin)(viewModel as dynamic);
			} catch {
			}
		}

		/// <summary>
		/// Gets the default key actions.
		/// </summary>
		/// <returns>The default key actions.</returns>
		public IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

		/// <summary>
		/// Handles when a Participant Card is clicked.
		/// </summary>
		/// <param name="e">Event.</param>
		protected void HandleClickedPCardEvent (ClickedPCardEvent e)
		{
			if (e.ClickedPlayer != null) {
				if (e.Modifier == ButtonModifier.Control) {
					e.ClickedPlayer.Tagged = !e.ClickedPlayer.Locked;
					e.ClickedPlayer.Locked = !e.ClickedPlayer.Locked;
				} else {
					if (!e.ClickedPlayer.Locked) {
						e.ClickedPlayer.Tagged = !e.ClickedPlayer.Tagged;
					}
				}
			}

			// Without the Shift modifier, unselect the rest of players that are not locked.
			if (e.Modifier != ButtonModifier.Shift) {
				foreach (PlayerVM player in project.Players) {
					if (player != e.ClickedPlayer && !player.Locked) {
						player.Tagged = false;
					}
				}
			}

			// Right now we don't care about selections and moving pcards
		}

		protected async void HandleNewTagEvent (NewTagEvent e)
		{
			//FIXME: This is using the Model of the ViewModel, that method should be moved here
			// Reception of the event Button
			var play = CreateTimelineEvent (e.EventType, e.Start, e.Stop, e.EventTime, null);

			var players = project.Players.Where (p => p.Tagged);
			foreach (var playerVM in players) {
				play.Players.Add (playerVM.Model);
			}

			var teams = project.Teams.Where (team => players.Any (player => team.Contains (player))).Select (vm => vm.Model);
			play.Teams.AddRange (teams);

			// Here we can set the players if necessary, then send to events aggregator
			if (project == null)
				return;

			if (project.IsLive) {
				if (!capturer.Capturing) {
					App.Current.Dialogs.WarningMessage (Catalog.GetString ("Video capture is stopped"));
					return;
				}
			}

			if (!project.Dashboard.DisablePopupWindow && project.Dashboard.EditPlays) {
				play.AddDefaultPositions ();
				if (project.ProjectType == ProjectType.FileProject) {
					bool playing = videoPlayer.Playing;
					videoPlayer.Pause ();
					await App.Current.GUIToolkit.EditPlay (play, project.Model, true, true, true, true);
					if (playing) {
						videoPlayer.Play ();
					}
				} else {
					await App.Current.GUIToolkit.EditPlay (play, project.Model, true, true, true, true);
				}
			}

			Log.Debug (String.Format ("New play created start:{0} stop:{1} category:{2}",
				play.Start.ToMSecondsString (), play.Stop.ToMSecondsString (),
				play.EventType.Name));
			project.Model.Timeline.Add (play);
			AddNewPlay (play);

			Reset ();
		}

		protected abstract TimelineEvent CreateTimelineEvent (EventType type, Time start, Time stop,
															  Time eventTime, Image miniature);

		void AddNewPlay (TimelineEvent play)
		{
			/* Clip play boundaries */
			play.Start.MSeconds = Math.Max (0, play.Start.MSeconds);
			if (project.ProjectType == ProjectType.FileProject) {
				play.Stop.MSeconds = Math.Min (project.FileSet.Duration.MSeconds, play.Stop.MSeconds);
				play.CamerasLayout = videoPlayer.CamerasLayout;
				play.CamerasConfig = new ObservableCollection<CameraConfig> (videoPlayer.CamerasConfig);
			} else {
				play.CamerasLayout = null;
				play.CamerasConfig = new ObservableCollection<CameraConfig> { new CameraConfig (0) };
			}

			App.Current.EventsBroker.Publish (new EventCreatedEvent { TimelineEvent = play });

			if (project.ProjectType == ProjectType.FileProject) {
				videoPlayer.Play ();
			}
			Save (project);

			if (project.ProjectType == ProjectType.CaptureProject ||
				project.ProjectType == ProjectType.URICaptureProject) {
				if (App.Current.Config.AutoRenderPlaysInLive) {
					RenderPlay (project.Model, play);
				}
			}
		}

		void Save (ProjectVM project)
		{
			if (App.Current.Config.AutoSave) {
				App.Current.DatabaseManager.ActiveDB.Store (project.Model);
			}
		}

		protected void RenderPlay (Project project, TimelineEvent play)
		{
			Playlist playlist;
			EncodingSettings settings;
			EditionJob job;
			string outputDir, outputProjectDir, outputFile;

			if (App.Current.Config.AutoRenderDir == null ||
				!Directory.Exists (App.Current.Config.AutoRenderDir)) {
				outputDir = App.Current.VideosDir;
			} else {
				outputDir = App.Current.Config.AutoRenderDir;
			}

			outputProjectDir = Path.Combine (outputDir,
				Utils.SanitizePath (project.ShortDescription));
			outputFile = String.Format ("{0}-{1}.mp4", play.EventType.Name, play.Name);
			outputFile = Utils.SanitizePath (outputFile, ' ');
			outputFile = Path.Combine (outputProjectDir, outputFile);
			try {
				PlaylistPlayElement element;

				Directory.CreateDirectory (outputProjectDir);
				settings = EncodingSettings.DefaultRenderingSettings (outputFile);
				playlist = new Playlist ();
				element = new PlaylistPlayElement (play);
				playlist.Elements.Add (element);
				job = new EditionJob (playlist, settings);
				App.Current.JobsManager.Add (job);
			} catch (Exception ex) {
				Log.Exception (ex);
			}
		}

		/// <summary>
		/// Resets all pCards.
		/// </summary>
		void Reset ()
		{
			foreach (PlayerVM player in project.Players) {
				if (!player.Locked) {
					player.Tagged = false;
				}
			}
		}

		void HandleVideoPlayerPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (sender == videoPlayer && e.PropertyName == "CurrentTime") {
				SetVideoCurrentTimeToTimerButtons ();
			}
		}

		void SetVideoCurrentTimeToTimerButtons ()
		{
			project.Dashboard.CurrentTime = VideoPlayer.CurrentTime;
			foreach (var timerVM in project.Dashboard.ViewModels.OfType<TimerButtonVM> ()) {
				timerVM.CurrentTime = VideoPlayer.CurrentTime;
			}

			foreach (var timedVM in project.Dashboard.ViewModels.OfType<TimedDashboardButtonVM> ()) {
				timedVM.CurrentTime = VideoPlayer.CurrentTime;
			}
		}
	}
}
