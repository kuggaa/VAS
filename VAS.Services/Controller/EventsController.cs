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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;

namespace VAS.Services.Controller
{
	/// <summary>
	/// Events controller, base class of the Events Controller.
	/// </summary>
	public class EventsController : ControllerBase
	{
		/// <summary>
		/// Gets or sets the current loaded play.
		/// </summary>
		/// <value>The loaded play, null if no play.</value>
		protected TimelineEventVM LoadedPlay { get; set; }

		protected virtual VideoPlayerVM VideoPlayer { get; set; }

		protected virtual ProjectVM Project { get; set; }

		protected virtual ICapturerBin Capturer { get; set; }

		#region IController implementation

		public override async Task Start ()
		{
			await base.Start ();
			App.Current.EventsBroker.Subscribe<LoadTimelineEventEvent<TimelineEventVM>> (HandleLoadEvent);
			App.Current.EventsBroker.Subscribe<LoadTimelineEventEvent<IEnumerable<TimelineEventVM>>> (HandleLoadEventsList);
			App.Current.EventsBroker.Subscribe<LoadTimelineEventEvent<EventTypeTimelineVM>> (HandleLoadEventType);

			App.Current.EventsBroker.Subscribe<NewEventEvent> (HandleNewEvent);
			App.Current.EventsBroker.SubscribeAsync<EventsDeletedEvent> (HandleDeleteEvents);
			App.Current.EventsBroker.SubscribeAsync<NewDashboardEvent> (HandleNewDashboardEvent);
			App.Current.EventsBroker.SubscribeAsync<MoveToEventTypeEvent> (HandleMoveToEventType);
			App.Current.EventsBroker.Subscribe<DuplicateEventsEvent> (HandleDuplicateEvents);

			App.Current.EventsBroker.Subscribe<EventLoadedEvent> (HandleEventLoadedEvent);
			App.Current.EventsBroker.Subscribe<PlaylistElementLoadedEvent> (HandlePlaylistElementLoaded);
			if (VideoPlayer != null) {
				VideoPlayer.PropertyChanged += HandlePlayerVMPropertyChanged;
			}
			if (Project?.Periods != null) {
				Project.Periods.PropertyChanged += HandlePropertyChanged;
			}
			if (Project?.Timers != null) {
				Project.Timers.PropertyChanged += HandlePropertyChanged;
			}
		}

		public override async Task Stop ()
		{
			await base.Stop ();
			App.Current.EventsBroker.Unsubscribe<LoadTimelineEventEvent<TimelineEventVM>> (HandleLoadEvent);
			App.Current.EventsBroker.Unsubscribe<LoadTimelineEventEvent<IEnumerable<TimelineEventVM>>> (HandleLoadEventsList);
			App.Current.EventsBroker.Unsubscribe<LoadTimelineEventEvent<EventTypeTimelineVM>> (HandleLoadEventType);

			App.Current.EventsBroker.Unsubscribe<NewEventEvent> (HandleNewEvent);
			App.Current.EventsBroker.UnsubscribeAsync<EventsDeletedEvent> (HandleDeleteEvents);
			App.Current.EventsBroker.UnsubscribeAsync<NewDashboardEvent> (HandleNewDashboardEvent);
			App.Current.EventsBroker.UnsubscribeAsync<MoveToEventTypeEvent> (HandleMoveToEventType);
			App.Current.EventsBroker.Unsubscribe<DuplicateEventsEvent> (HandleDuplicateEvents);

			App.Current.EventsBroker.Unsubscribe<EventLoadedEvent> (HandleEventLoadedEvent);
			App.Current.EventsBroker.Unsubscribe<PlaylistElementLoadedEvent> (HandlePlaylistElementLoaded);
			if (VideoPlayer != null) {
				VideoPlayer.PropertyChanged -= HandlePlayerVMPropertyChanged;
			}
			if (Project?.Periods != null) {
				Project.Periods.PropertyChanged -= HandlePropertyChanged;
			}
			if (Project?.Timers != null) {
				Project.Timers.PropertyChanged -= HandlePropertyChanged;
			}
		}

		public override void SetViewModel (IViewModel viewModel)
		{
			VideoPlayer = ((IVideoPlayerDealer)viewModel).VideoPlayer;
			Project = (viewModel as IProjectDealer)?.Project;
			Capturer = (viewModel as ICapturerBinDealer)?.Capturer;
		}

		#endregion

		protected virtual void HandlePlayerVMPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
		}

		protected bool CheckTimelineEventsLimitation ()
		{
			bool limitation = false;
			var eventLimitation = VASCountLimitedObjects.TimelineEvents.ToString ();
			if (!App.Current.LicenseLimitationsService.CanExecute (eventLimitation)) {
				App.Current.LicenseLimitationsService.MoveToUpgradeDialog (eventLimitation);
				limitation = true;
			}
			return limitation;
		}

		void HandleNewEvent (NewEventEvent e)
		{
			if (CheckTimelineEventsLimitation ()) {
				return;
			}
			if (Project == null) {
				return;
			}
			if (Project.IsLive) {
				if (!Capturer.Capturing) {
					App.Current.Dialogs.WarningMessage (Catalog.GetString ("Video capture is stopped"));
					return;
				}
			}

			Log.Debug (String.Format ("New play created start:{0} stop:{1} category:{2}",
				e.Start.ToMSecondsString (), e.Stop.ToMSecondsString (),
				e.EventType.Name));
			/* Add the new created play to the project and update the GUI */
			var play = Project.Model.AddEvent (e.EventType, e.Start, e.Stop, e.EventTime, null);
			play.Teams.Reset (e.Teams);
			if (e.Players != null) {
				play.Players.Reset (e.Players);
			}
			if (e.Tags != null) {
				play.Tags.Reset (e.Tags);
			}
			AddNewPlay (play);
		}

		async Task HandleNewDashboardEvent (NewDashboardEvent e)
		{
			if (CheckTimelineEventsLimitation ()) {
				return;
			}
			if (Project == null)
				return;

			if (Project.IsLive) {
				if (!Capturer.Capturing) {
					App.Current.Dialogs.WarningMessage (Catalog.GetString ("Video capture is stopped"));
					return;
				}
			}

			if (!Project.Model.Dashboard.DisablePopupWindow && e.Edit) {
				e.TimelineEvent.Model.AddDefaultPositions ();

				PlayEventEditionSettings settings = new PlayEventEditionSettings () {
					EditTags = true,
					EditNotes = true,
					EditPlayers = true,
					EditPositions = true
				};

				if (Project.ProjectType == ProjectType.FileProject) {
					bool playing = VideoPlayer.Playing;
					VideoPlayer.PauseCommand.Execute (false);
					await App.Current.EventsBroker.Publish (new EditEventEvent { TimelineEvent = e.TimelineEvent });
					if (playing) {
						VideoPlayer.PlayCommand.Execute ();
					}
				} else {
					await App.Current.EventsBroker.Publish (new EditEventEvent { TimelineEvent = e.TimelineEvent });
				}
			}

			Log.Debug (String.Format ("New play created start:{0} stop:{1} category:{2}",
				e.TimelineEvent.Start.ToMSecondsString (), e.TimelineEvent.Stop.ToMSecondsString (),
				e.TimelineEvent.Model.EventType.Name));
			Project.Model.AddEvent (e.TimelineEvent.Model);
			AddNewPlay (e.TimelineEvent.Model);
			await App.Current.EventsBroker.Publish (new DashboardEventCreatedEvent {
				TimelineEvent = e.TimelineEvent,
				DashboardButton = e.DashboardButton,
				DashboardButtons = e.DashboardButtons
			});
		}

		async Task HandleMoveToEventType (MoveToEventTypeEvent e)
		{
			// Only move the events where the event type changes for real
			var newEventVMs = e.TimelineEvents.Where (vm => vm.Model.EventType != e.EventType);

			foreach (var eventVM in newEventVMs) {
				var newEventVM = Cloner.Clone (eventVM);
				newEventVM.Model.ID = Guid.NewGuid ();
				newEventVM.Model.EventType = e.EventType;
				// Remove all tags from the previous event type but keep global tags
				newEventVM.Model.Tags.RemoveAll (t => (eventVM.Model.EventType as AnalysisEventType).Tags.Contains (t));
				Project.Model.AddEvent (newEventVM.Model);
			}
			await DeletePlays (newEventVMs.ToList (), false);
			Save (Project);
		}

		void HandleDuplicateEvents (DuplicateEventsEvent e)
		{
			foreach (var play in e.TimelineEvents) {
				var copy = play.Model.Clone ();

				if (CheckTimelineEventsLimitation ()) {
					return;
				}

				Project.Model.AddEvent (copy);

				App.Current.EventsBroker.Publish (new EventCreatedEvent {
					TimelineEvent = new TimelineEventVM { Model = copy }
				});
			}
		}

		async Task HandleDeleteEvents (EventsDeletedEvent e)
		{
			await DeletePlays (e.TimelineEvents);
		}

		void HandleLoadEvent (LoadTimelineEventEvent<TimelineEventVM> e)
		{
			VideoPlayer.LoadEvent (e.Object, e.Playing);
		}

		void HandleLoadEventsList (LoadTimelineEventEvent<IEnumerable<TimelineEventVM>> e)
		{
			var eventVMs = e.Object;
			//Only order them if they have the same EventType
			var firstEventVM = eventVMs.FirstOrDefault ();

			if (eventVMs.All (eventVM => eventVM.Model.EventType == firstEventVM.Model.EventType)) {
				eventVMs = eventVMs.OrderBy (evt => evt.Start);
			}

			VideoPlayer.LoadEvents (eventVMs, e.Playing);
		}

		void HandleLoadEventType (LoadTimelineEventEvent<EventTypeTimelineVM> e)
		{
			var timelineEvents = e.Object.ViewModels.Where ((arg) => arg.Visible == true)
								  .OrderBy (evt => evt.Start);

			VideoPlayer.LoadEvents (timelineEvents, e.Playing);
		}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (sender is TimeNodeVM) {
				if (e.PropertyName == nameof (TimeNodeVM.Start)) {
					VideoPlayer.PauseCommand.Execute (false);
					VideoPlayer.SeekCommand.Execute (new VideoPlayerSeekOptions ((sender as TimeNodeVM).Start, true, false, true));
				} else if (e.PropertyName == nameof (TimeNodeVM.Stop)) {
					VideoPlayer.PauseCommand.Execute (false);
					VideoPlayer.SeekCommand.Execute (new VideoPlayerSeekOptions ((sender as TimeNodeVM).Stop, true, false, true));
				}
			}
		}

		void AddNewPlay (TimelineEvent play)
		{
			/* Clip play boundaries */
			play.Start.MSeconds = Math.Max (0, play.Start.MSeconds);
			if (Project.ProjectType == ProjectType.FileProject) {
				play.Stop.MSeconds = Math.Min (Project.FileSet.Duration.MSeconds, play.Stop.MSeconds);
				play.CamerasLayout = VideoPlayer.CamerasLayout;
				play.CamerasConfig = VideoPlayer.CamerasConfig.Clone ();
			} else {
				play.CamerasLayout = null;
				play.CamerasConfig = new ObservableCollection<CameraConfig> { new CameraConfig (0) };
			}

			var evt = Project.Timeline.FullTimeline.Where (vm => vm.Model == play).FirstOrDefault ();

			App.Current.EventsBroker.Publish (new EventCreatedEvent {
				TimelineEvent = evt
			});

			if (Project.ProjectType == ProjectType.FileProject) {
				VideoPlayer.PlayCommand.Execute ();
			}
			Save (Project);

			if (Project.ProjectType == ProjectType.CaptureProject ||
				Project.ProjectType == ProjectType.URICaptureProject) {
				if (App.Current.Config.AutoRenderPlaysInLive) {
					RenderPlay (Project.Model, play);
				}
			}
		}

		async Task DeletePlays (IEnumerable<TimelineEventVM> plays, bool askConfirmation = true)
		{
			plays = plays.Where (p => p.Model.Deletable);
			Log.Debug (plays.Count () + " plays deleted");
			if (askConfirmation) {
				var delete = await App.Current.Dialogs.QuestionMessage (
					Catalog.GetString (String.Format ("Do you want to delete {0} event(s)?", plays.Count ())),
												null);
				if (!delete) {
					return;
				}
			}
			Project.Timeline.Model.RemoveRange (plays.Select (vm => vm.Model));
			if (Project.ProjectType == ProjectType.FileProject) {
				Save (Project);
			}
			if (LoadedPlay != null && plays.Contains (LoadedPlay)) {
				await App.Current.EventsBroker.Publish (new LoadEventEvent ());
			}
		}

		void Save (ProjectVM project)
		{
			if (App.Current.Config.AutoSave) {
				App.Current.DatabaseManager.ActiveDB.Store (project.Model);
			}
		}

		void RenderPlay (Project project, TimelineEvent play)
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

		void HandleEventLoadedEvent (EventLoadedEvent e)
		{
			LoadedPlay = e.TimelineEvent;
		}

		void HandlePlaylistElementLoaded (PlaylistElementLoadedEvent e)
		{
			if (e.Element is PlaylistPlayElementVM) {
				LoadedPlay = (e.Element as PlaylistPlayElementVM).Play;
			} else {
				LoadedPlay = null;
			}
		}
	}
}

