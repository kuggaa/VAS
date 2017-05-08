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
		VideoPlayerVM playerVM;
		TimelineVM timelineVM;
		ProjectVM projectVM;
		ICapturerBin capturer;

		/// <summary>
		/// Gets or sets the current loaded play.
		/// </summary>
		/// <value>The loaded play, null if no play.</value>
		public TimelineEvent LoadedPlay { get; set; }

		public VideoPlayerVM PlayerVM {
			get {
				return playerVM;
			}
			set {
				if (playerVM != null) {
					playerVM.PropertyChanged -= HandlePlayerVMPropertyChanged;
				}
				playerVM = value;
				if (playerVM != null) {
					playerVM.PropertyChanged += HandlePlayerVMPropertyChanged;
				}
			}
		}

		public virtual TimelineVM Timeline {
			get {
				return timelineVM;
			}
			set {
				if (timelineVM != null) {
					timelineVM.Filters.PropertyChanged -= HandlePropertyChanged;
				}
				timelineVM = value;
				if (timelineVM != null) {
					HandleFiltersChanged ();
					timelineVM.Filters.PropertyChanged += HandlePropertyChanged;
				}
			}
		}

		#region IController implementation

		public override void Start ()
		{
			base.Start ();
			App.Current.EventsBroker.Subscribe<LoadTimelineEventEvent<TimelineEventVM>> (HandleLoadEvent);
			App.Current.EventsBroker.Subscribe<LoadTimelineEventEvent<IEnumerable<TimelineEventVM>>> (HandleLoadEventsList);
			App.Current.EventsBroker.Subscribe<LoadTimelineEventEvent<EventTypeTimelineVM>> (HandleLoadEventType);

			App.Current.EventsBroker.Subscribe<NewEventEvent> (HandleNewEvent);
			App.Current.EventsBroker.Subscribe<EventsDeletedEvent> (HandleDeleteEvents);
			App.Current.EventsBroker.Subscribe<NewDashboardEvent> (HandleNewDashboardEvent);
			App.Current.EventsBroker.Subscribe<MoveToEventTypeEvent> (HandleMoveToEventType);
			App.Current.EventsBroker.Subscribe<DuplicateEventsEvent> (HandleDuplicateEvents);

			App.Current.EventsBroker.Subscribe<EventLoadedEvent> (HandleEventLoadedEvent);
			App.Current.EventsBroker.Subscribe<PlaylistElementLoadedEvent> (HandlePlaylistElementLoaded);
		}

		public override void Stop ()
		{
			base.Stop ();
			App.Current.EventsBroker.Unsubscribe<LoadTimelineEventEvent<TimelineEventVM>> (HandleLoadEvent);
			App.Current.EventsBroker.Unsubscribe<LoadTimelineEventEvent<IEnumerable<TimelineEventVM>>> (HandleLoadEventsList);
			App.Current.EventsBroker.Unsubscribe<LoadTimelineEventEvent<EventTypeTimelineVM>> (HandleLoadEventType);

			App.Current.EventsBroker.Unsubscribe<NewEventEvent> (HandleNewEvent);
			App.Current.EventsBroker.Unsubscribe<EventsDeletedEvent> (HandleDeleteEvents);
			App.Current.EventsBroker.Unsubscribe<NewDashboardEvent> (HandleNewDashboardEvent);
			App.Current.EventsBroker.Unsubscribe<MoveToEventTypeEvent> (HandleMoveToEventType);
			App.Current.EventsBroker.Unsubscribe<DuplicateEventsEvent> (HandleDuplicateEvents);

			App.Current.EventsBroker.Unsubscribe<EventLoadedEvent> (HandleEventLoadedEvent);
			App.Current.EventsBroker.Unsubscribe<PlaylistElementLoadedEvent> (HandlePlaylistElementLoaded);
		}

		public override void SetViewModel (IViewModel viewModel)
		{
			PlayerVM = ((IVideoPlayerDealer)viewModel).VideoPlayer;
			Timeline = ((ITimelineDealer)viewModel).Timeline;
			projectVM = (viewModel as IProjectDealer)?.Project;
			capturer = (viewModel as ICapturerBinDealer)?.Capturer;
		}

		public override IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

		#endregion

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			PlayerVM = null;
			Timeline = null;
		}

		protected virtual void HandlePlayerVMPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
		}

		void HandleNewEvent (NewEventEvent e)
		{
			if (projectVM == null) {
				return;
			}
			if (projectVM.IsLive) {
				if (!capturer.Capturing) {
					App.Current.Dialogs.WarningMessage (Catalog.GetString ("Video capture is stopped"));
					return;
				}
			}

			Log.Debug (String.Format ("New play created start:{0} stop:{1} category:{2}",
				e.Start.ToMSecondsString (), e.Stop.ToMSecondsString (),
				e.EventType.Name));
			/* Add the new created play to the project and update the GUI */
			var play = projectVM.Model.AddEvent (e.EventType, e.Start, e.Stop, e.EventTime, null);
			play.Teams.Replace (e.Teams);
			if (e.Players != null) {
				play.Players.Replace (e.Players);
			}
			if (e.Tags != null) {
				play.Tags.Replace (e.Tags);
			}
			AddNewPlay (play);
		}

		async void HandleNewDashboardEvent (NewDashboardEvent e)
		{
			if (projectVM == null)
				return;

			if (projectVM.IsLive) {
				if (!capturer.Capturing) {
					App.Current.Dialogs.WarningMessage (Catalog.GetString ("Video capture is stopped"));
					return;
				}
			}

			if (!projectVM.Model.Dashboard.DisablePopupWindow && e.Edit) {
				e.TimelineEvent.AddDefaultPositions ();

				PlayEventEditionSettings settings = new PlayEventEditionSettings () {
					EditTags = true, EditNotes = true, EditPlayers = true, EditPositions = true
				};

				if (projectVM.ProjectType == ProjectType.FileProject) {
					bool playing = playerVM.Playing;
					playerVM.Pause ();
					await App.Current.EventsBroker.Publish (new EditEventEvent { TimelineEvent = e.TimelineEvent });
					if (playing) {
						playerVM.Play ();
					}
				} else {
					await App.Current.EventsBroker.Publish (new EditEventEvent { TimelineEvent = e.TimelineEvent });
				}
			}

			Log.Debug (String.Format ("New play created start:{0} stop:{1} category:{2}",
				e.TimelineEvent.Start.ToMSecondsString (), e.TimelineEvent.Stop.ToMSecondsString (),
				e.TimelineEvent.EventType.Name));
			projectVM.Model.AddEvent (e.TimelineEvent);
			AddNewPlay (e.TimelineEvent);
			await App.Current.EventsBroker.Publish (new DashboardEventCreatedEvent { 
				TimelineEvent = e.TimelineEvent, DashboardButton = e.DashboardButton, DashboardButtons = e.DashboardButtons });
		}

		void HandleMoveToEventType (MoveToEventTypeEvent e)
		{
			// Only move the events where the event type changes for real
			var newEvents = e.TimelineEvents.Where (ev => ev.EventType != e.EventType);

			foreach (var evt in newEvents) {
				var newEvent = Cloner.Clone (evt);
				newEvent.ID = Guid.NewGuid ();
				newEvent.EventType = e.EventType;
				// Remove all tags from the previous event type but keep global tags
				newEvent.Tags.RemoveAll (t => (evt.EventType as AnalysisEventType).Tags.Contains (t));
				projectVM.Model.AddEvent (newEvent);
				App.Current.EventsBroker.Publish (new EventCreatedEvent { TimelineEvent = newEvent });
			}
			DeletePlays (newEvents.ToList (), false);
			Save (projectVM);
		}

		void HandleDuplicateEvents (DuplicateEventsEvent e)
		{
			foreach (var play in e.TimelineEvents) {
				var copy = play.Clone ();
				projectVM.Model.AddEvent (copy);
				App.Current.EventsBroker.Publish (new EventCreatedEvent { TimelineEvent = copy });
			}
		}

		void HandleDeleteEvents (EventsDeletedEvent e)
		{
			DeletePlays (e.TimelineEvents);
		}

		void HandleLoadEvent (LoadTimelineEventEvent<TimelineEventVM> e)
		{
			PlayerVM.LoadEvent (e.Object?.Model, e.Playing);
		}

		void HandleLoadEventsList (LoadTimelineEventEvent<IEnumerable<TimelineEventVM>> e)
		{
			PlayerVM.LoadEvents (e.Object.Select (vm => vm.Model), e.Playing);
		}

		void HandleLoadEventType (LoadTimelineEventEvent<EventTypeTimelineVM> e)
		{
			var timelineEvents = e.Object.ViewModels.Where ((arg) => arg.Visible == true)
													.Select ((arg) => arg.Model);
			PlayerVM.LoadEvents (timelineEvents, e.Playing);
		}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == $"Collection_{nameof (timelineVM.Filters.Elements)}" ||
				e.PropertyName == nameof (timelineVM.Filters.Active)) {
				HandleFiltersChanged ();
			}
		}

		void HandleFiltersChanged ()
		{
			foreach (var eventVM in Timeline.EventTypesTimeline.SelectMany (eventTypeVM => eventTypeVM.ViewModels)) {
				eventVM.Visible = Timeline.Filters.Filter (eventVM);
			}
		}

		void AddNewPlay (TimelineEvent play)
		{
			/* Clip play boundaries */
			play.Start.MSeconds = Math.Max (0, play.Start.MSeconds);
			if (projectVM.ProjectType == ProjectType.FileProject) {
				play.Stop.MSeconds = Math.Min (projectVM.FileSet.Duration.MSeconds, play.Stop.MSeconds);
				play.CamerasLayout = playerVM.CamerasLayout;
				play.CamerasConfig = new ObservableCollection<CameraConfig> (playerVM.CamerasConfig);
			} else {
				play.CamerasLayout = null;
				play.CamerasConfig = new ObservableCollection<CameraConfig> { new CameraConfig (0) };
			}

			App.Current.EventsBroker.Publish (new EventCreatedEvent { TimelineEvent = play });

			if (projectVM.ProjectType == ProjectType.FileProject) {
				playerVM.Play ();
			}
			Save (projectVM);

			if (projectVM.ProjectType == ProjectType.CaptureProject ||
				projectVM.ProjectType == ProjectType.URICaptureProject) {
				if (App.Current.Config.AutoRenderPlaysInLive) {
					RenderPlay (projectVM.Model, play);
				}
			}
		}

		void DeletePlays (List<TimelineEvent> plays, bool update = true)
		{
			Log.Debug (plays.Count + " plays deleted");
			projectVM.Timeline.Model.RemoveRange (plays);
			if (projectVM.ProjectType == ProjectType.FileProject) {
				Save (projectVM);
			}
			if (LoadedPlay != null && plays.Contains (LoadedPlay)) {
				App.Current.EventsBroker.Publish (new LoadEventEvent ());
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
			if (e.Element is PlaylistPlayElement) {
				LoadedPlay = (e.Element as PlaylistPlayElement).Play;
			} else {
				LoadedPlay = null;
			}
		}
	}
}

