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
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;
using VAS.Services;
using VAS.Services.Controller;
using VAS.Services.ViewModel;
using VASCountLimitedObjects = VAS.Core.Common.VASCountLimitedObjects;
using Timer = VAS.Core.Store.Timer;

namespace VAS.Tests.Services
{
	[TestFixture]
	public class TestEventsController
	{
		EventsController controller;
		Mock<IVideoPlayerController> playerController;
		TimelineEvent ev1, ev2, ev11;
		TimelineEventVM evVM1, evVM2, evVM11;
		ProjectVM projectVM;
		VideoPlayerVM videoPlayer;
		Mock<ILicenseLimitationsService> mockLimitationService;
		Mock<IDialogs> mockDialogs;

		[SetUp]
		public async Task Setup ()
		{
			controller = new EventsController ();
			playerController = new Mock<IVideoPlayerController> ();
			videoPlayer = new VideoPlayerVM {
				Player = playerController.Object,
				CamerasConfig = new RangeObservableCollection<CameraConfig> ()
			};

			Mock<IVideoPlayer> playerMock = new Mock<IVideoPlayer> ();
			playerMock.SetupAllProperties ();
			/* Mock properties without setter */
			playerMock.Setup (p => p.CurrentTime).Returns (() => new Time (0));
			playerMock.Setup (p => p.StreamLength).Returns (() => new Time (0));
			playerMock.Setup (p => p.Play (It.IsAny<bool> ())).Raises (p => p.StateChange += null,
				new PlaybackStateChangedEvent {
					Playing = true
				}
			);
			playerMock.Setup (p => p.Pause (It.IsAny<bool> ())).Raises (p => p.StateChange += null,
				new PlaybackStateChangedEvent {
					Playing = false
				}
			);

			mockLimitationService = new Mock<ILicenseLimitationsService> ();
			mockLimitationService.Setup (ls => ls.CanExecute (It.IsAny<string> ())).Returns (true);
			App.Current.LicenseLimitationsService = mockLimitationService.Object;

			projectVM = new DummyProjectVM { Model = Utils.CreateProject (true) };
			controller.SetViewModel (new ProjectAnalysisVM<ProjectVM> {
				Project = projectVM,
				VideoPlayer = videoPlayer
			});

			var mtkMock = new Mock<IMultimediaToolkit> ();
			mtkMock.Setup (m => m.GetPlayer ()).Returns (playerMock.Object);
			App.Current.MultimediaToolkit = mtkMock.Object;

			await controller.Start ();

			var eventType1 = new EventType {
				Name = "Test"
			};
			var eventType2 = new EventType {
				Name = "Test2"
			};

			ev1 = new TimelineEvent ();
			ev1.EventType = eventType1;
			ev1.Start = new Time (0);
			ev1.Stop = new Time (1000);
			evVM1 = new TimelineEventVM { Model = ev1, Visible = true };
			ev2 = new TimelineEvent ();
			ev2.EventType = eventType1;
			ev2.Start = new Time (2000);
			ev2.Stop = new Time (3000);
			evVM2 = new TimelineEventVM { Model = ev2, Visible = true };
			ev11 = new TimelineEvent ();
			ev11.EventType = eventType2;
			ev11.Start = new Time (6000);
			ev11.Stop = new Time (8000);
			evVM11 = new TimelineEventVM { Model = ev11, Visible = true };

			mockDialogs = new Mock<IDialogs> ();
			App.Current.Dialogs = mockDialogs.Object;
			mockDialogs.Setup (m => m.QuestionMessage (It.IsAny<string> (), null, It.IsAny<object> ())).Returns (AsyncHelpers.Return (true));
		}

		[TearDown]
		public async Task TearDown ()
		{
			await controller.Stop ();
		}

		[Test]
		public void TestLoadTimelineEvent ()
		{
			App.Current.EventsBroker.Publish (new LoadTimelineEventEvent<TimelineEventVM> {
				Object = evVM1
			});

			playerController.Verify (p => p.LoadEvent (It.Is<TimelineEventVM> (vm => vm == evVM1),
													   It.IsAny<Time> (), It.IsAny<bool> ()), Times.Once);
		}

		[Test]
		public void TestLoadNullTimelineEvent ()
		{
			App.Current.EventsBroker.Publish (new LoadTimelineEventEvent<TimelineEventVM> {
				Object = null
			});

			playerController.Verify (p => p.LoadEvent (It.IsAny<TimelineEventVM> (),
													   It.IsAny<Time> (), It.IsAny<bool> ()), Times.Never);
			playerController.Verify (p => p.UnloadCurrentEvent (), Times.Once);
		}

		[Test]
		public void TestLoadTimelineEvents ()
		{
			IEnumerable<TimelineEventVM> eventList = new List<TimelineEventVM> { evVM1, evVM2 };

			App.Current.EventsBroker.Publish (new LoadTimelineEventEvent<IEnumerable<TimelineEventVM>> {
				Object = eventList
			});

			playerController.Verify (p => p.LoadPlaylistEvent (It.Is<PlaylistVM> (pl => pl.ChildModels.Count == 2),
															   It.Is<IPlayable> (pe => ComparePlaylistElement (pe, ev1)),
															   It.IsAny<bool> ()), Times.Once);
		}

		[Test]
		public void LoadTimelineEvents_DifferentEventType_NotOrderByStartTime ()
		{
			IEnumerable<TimelineEventVM> originalEventList = new List<TimelineEventVM> { evVM11, evVM1 };
			List<TimelineEventVM> playedEventList = originalEventList.ToList ();

			App.Current.EventsBroker.Publish (new LoadTimelineEventEvent<IEnumerable<TimelineEventVM>> {
				Object = originalEventList
			});

			playerController.Verify (p => p.LoadPlaylistEvent (It.Is<PlaylistVM> (pl => ComparePlaylist (pl, playedEventList)),
															   It.Is<IPlayable> (pe => ComparePlaylistElement (pe, ev11)),
																				It.IsAny<bool> ()), Times.Once);
		}

		[Test]
		public void LoadTimelineEvents_SameEventType_OrderByStartTime ()
		{
			IEnumerable<TimelineEventVM> originalEventList = new List<TimelineEventVM> { evVM2, evVM1 };
			List<TimelineEventVM> playedEventList = new List<TimelineEventVM> { evVM1, evVM2 };

			App.Current.EventsBroker.Publish (new LoadTimelineEventEvent<IEnumerable<TimelineEventVM>> {
				Object = originalEventList
			});

			playerController.Verify (
				p => p.LoadPlaylistEvent (It.Is<PlaylistVM> (pl => ComparePlaylist (pl, playedEventList)),
										  It.Is<IPlayable> (pe => ComparePlaylistElement (pe, ev1)),
					It.IsAny<bool> ()), Times.Once);
		}

		[Test]
		public void TestLoadEventTypeTimelineVM ()
		{
			EventTypeTimelineVM eTypeVM = new EventTypeTimelineVM ();
			eTypeVM.ViewModels.Add (evVM1);
			eTypeVM.ViewModels.Add (evVM2);

			eTypeVM.LoadEventType ();

			playerController.Verify (p => p.LoadPlaylistEvent (It.Is<PlaylistVM> (pl => pl.ChildModels.Count == 2),
															   It.Is<IPlayable>
															   (pe => ComparePlaylistElement (pe, ev1)),
															   It.IsAny<bool> ()), Times.Once);
		}

		[Test]
		public async Task EventsController_LoadEventTypeTimelineVM_OrderedByStartTime ()
		{
			EventTypeTimelineVM eTypeVM = new EventTypeTimelineVM ();
			eTypeVM.ViewModels.Add (evVM2);
			eTypeVM.ViewModels.Add (evVM1);
			List<TimelineEventVM> playedEventList = new List<TimelineEventVM> { evVM1, evVM2 };

			await eTypeVM.LoadEventType ();

			playerController.Verify (p => p.LoadPlaylistEvent (It.Is<PlaylistVM> (pl => ComparePlaylist (pl, playedEventList)),
															   It.Is<IPlayable>
															   (pe => ComparePlaylistElement (pe, ev1)),
															   It.IsAny<bool> ()), Times.Once);
		}

		[Test]
		public void TestLoadEventTypeTimelineVMWithOnlyOneVisible ()
		{
			EventTypeTimelineVM eTypeVM = new EventTypeTimelineVM ();
			eTypeVM.ViewModels.Add (evVM1);
			eTypeVM.ViewModels.Add (evVM2);
			evVM1.Visible = false;

			eTypeVM.LoadEventType ();

			playerController.Verify (p => p.LoadPlaylistEvent (It.Is<PlaylistVM> (pl => pl.ChildModels.Count == 1),
															   It.Is<IPlayable>
															   (pe => ComparePlaylistElement (pe, ev2)),
															   It.IsAny<bool> ()), Times.Once);
		}

		[Test]
		public void TestMoveToEventType ()
		{
			PreparePlayer ();

			EventType e1 = projectVM.Model.EventTypes [0];
			EventType e2 = projectVM.Model.EventTypes [1];

			var eventsToMove = projectVM.Model.Timeline.Where (e => e.EventType == e1).ToList ();

			Assert.AreEqual (1, eventsToMove.Count);
			Assert.AreEqual (1, projectVM.Model.Timeline.Count (e => e.EventType == e2));

			App.Current.EventsBroker.Publish (
				new MoveToEventTypeEvent {
					TimelineEvents = eventsToMove.Select (evt => new TimelineEventVM () { Model = evt }).ToList (),
					EventType = e2,
				}
			);

			Assert.AreEqual (0, projectVM.Timeline.FullTimeline.Count (e => e.Model.EventType == e1));
			Assert.AreEqual (2, projectVM.Timeline.FullTimeline.Count (e => e.Model.EventType == e2));
		}

		[Test]
		public void PeriodChange_StartChanged_SeekDone ()
		{
			Period period = new Period {
				Nodes = {
					new TimeNode { Start = new Time (0), Stop = new Time (10000) }
				}
			};
			projectVM.Periods.Model.Add (period);
			Time start = new Time (5000);

			period.Nodes [0].Start = start;

			playerController.Verify (p => p.Pause (false));
			playerController.Verify (p => p.Seek (start, true, false, true));
		}

		[Test]
		public void PeriodChange_StopChanged_SeekDone ()
		{
			Period period = new Period {
				Nodes = {
					new TimeNode { Start = new Time (0), Stop = new Time (10000) }
				}
			};
			projectVM.Periods.Model.Add (period);
			Time stop = new Time (5000);

			period.Nodes [0].Stop = stop;

			playerController.Verify (p => p.Pause (false));
			playerController.Verify (p => p.Seek (stop, true, false, true));
		}

		[Test]
		public void TimerChange_StartChanged_SeekDone ()
		{
			Timer timer = new Timer {
				Nodes = {
					new TimeNode { Start = new Time (0), Stop = new Time (10000) }
				}
			};
			projectVM.Timers.Model.Add (timer);
			Time start = new Time (5000);

			timer.Nodes [0].Start = start;

			playerController.Verify (p => p.Pause (false));
			playerController.Verify (p => p.Seek (start, true, false, true));
		}

		[Test]
		public void TimerChange_StopChanged_SeekDone ()
		{
			Timer timer = new Timer {
				Nodes = {
					new TimeNode { Start = new Time (0), Stop = new Time (10000) }
				}
			};
			projectVM.Timers.Model.Add (timer);
			Time stop = new Time (5000);

			timer.Nodes [0].Stop = stop;

			playerController.Verify (p => p.Pause (false));
			playerController.Verify (p => p.Seek (stop, true, false, true));
		}

		[Test]
		public void NewDashboardEvent_LimitationNotEnabled_AddsEvent ()
		{
			PreparePlayer ();

			int currentCount = projectVM.Timeline.FullTimeline.Count ();
			EventType e1 = projectVM.Model.EventTypes [0];
			TimelineEvent ev = new TimelineEvent {
				EventType = e1,
				Start = new Time (0),
				Stop = new Time (1000),
				EventTime = new Time (500)
			};

			App.Current.EventsBroker.Publish (new NewDashboardEvent {
				TimelineEvent = new TimelineEventVM () { Model = ev }
			});

			Assert.AreEqual (currentCount + 1, projectVM.Timeline.FullTimeline.Count ());
			Assert.AreSame (ev, projectVM.Timeline.FullTimeline.Model [currentCount]);
			mockLimitationService.Verify (ls => ls.MoveToUpgradeDialog (VASCountLimitedObjects.TimelineEvents.ToString ()),
										  Times.Never);
		}

		[Test]
		public void NewDashboardEvent_LimitationEnabled_MoveToUpgradeDialog ()
		{
			mockLimitationService.Setup (ls => ls.CanExecute (VASCountLimitedObjects.TimelineEvents.ToString ()))
								 .Returns (false);
			PreparePlayer ();

			int currentCount = projectVM.Timeline.FullTimeline.Count ();
			EventType e1 = projectVM.Model.EventTypes [0];
			TimelineEvent ev = new TimelineEvent {
				EventType = e1,
				Start = new Time (0),
				Stop = new Time (1000),
				EventTime = new Time (500)
			};

			App.Current.EventsBroker.Publish (new NewDashboardEvent {
				TimelineEvent = new TimelineEventVM { Model = ev }
			});

			Assert.AreEqual (currentCount, projectVM.Timeline.FullTimeline.Count ());
			mockLimitationService.Verify (ls => ls.MoveToUpgradeDialog (VASCountLimitedObjects.TimelineEvents.ToString ()),
										  Times.Once);
		}

		[Test]
		public void NewEventEvent_LimitationNotEnabled_AddsEvent ()
		{
			PreparePlayer ();

			int currentCount = projectVM.Timeline.FullTimeline.Count ();
			EventType e1 = projectVM.Model.EventTypes [0];
			TimelineEvent ev = new TimelineEvent {
				EventType = e1,
				Start = new Time (0),
				Stop = new Time (1000),
				EventTime = new Time (500)
			};

			App.Current.EventsBroker.Publish (new NewEventEvent {
				EventTime = ev.EventTime,
				EventType = ev.EventType,
				Start = ev.Start,
				Stop = ev.Stop
			});

			Assert.AreEqual (currentCount + 1, projectVM.Timeline.FullTimeline.Count ());
			Assert.AreSame (ev.EventType, projectVM.Timeline.FullTimeline.Model [currentCount].EventType);
			mockLimitationService.Verify (ls => ls.MoveToUpgradeDialog (VASCountLimitedObjects.TimelineEvents.ToString ()),
										  Times.Never);
		}

		[Test]
		public void NewEventEvent_LimitationEnabled_MoveToUpgradeDialog ()
		{
			mockLimitationService.Setup (ls => ls.CanExecute (VASCountLimitedObjects.TimelineEvents.ToString ()))
								 .Returns (false);
			PreparePlayer ();

			int currentCount = projectVM.Timeline.FullTimeline.Count ();
			EventType e1 = projectVM.Model.EventTypes [0];
			TimelineEvent ev = new TimelineEvent {
				EventType = e1,
				Start = new Time (0),
				Stop = new Time (1000),
				EventTime = new Time (500)
			};

			App.Current.EventsBroker.Publish (new NewEventEvent {
				EventTime = ev.EventTime,
				EventType = ev.EventType,
				Start = ev.Start,
				Stop = ev.Stop
			});

			Assert.AreEqual (currentCount, projectVM.Timeline.FullTimeline.Count ());
			mockLimitationService.Verify (ls => ls.MoveToUpgradeDialog (VASCountLimitedObjects.TimelineEvents.ToString ()),
										  Times.Once);
		}

		[Test]
		public void DuplicateEventsEvent_LimitationNotEnabled_AddsEvents ()
		{
			PreparePlayer ();

			int currentCount = projectVM.Timeline.FullTimeline.Count ();

			var plays = projectVM.Timeline.FullTimeline.Model.Select (m => new TimelineEventVM () { Model = m }).ToList ();

			App.Current.EventsBroker.Publish (new DuplicateEventsEvent {
				TimelineEvents = plays
			});

			Assert.AreEqual (2 * currentCount, projectVM.Timeline.FullTimeline.Count ());
			mockLimitationService.Verify (ls => ls.MoveToUpgradeDialog (VASCountLimitedObjects.TimelineEvents.ToString ()),
										  Times.Never);
		}

		[Test]
		public void DuplicateEventsEvent_LimitationEnabled_MoveToUpgradeDialog ()
		{
			mockLimitationService.Setup (ls => ls.CanExecute (VASCountLimitedObjects.TimelineEvents.ToString ()))
								 .Returns (false);
			PreparePlayer ();

			int currentCount = projectVM.Timeline.FullTimeline.Count ();

			var plays = projectVM.Timeline.FullTimeline.Model.Select (m => new TimelineEventVM () { Model = m }).ToList ();

			App.Current.EventsBroker.Publish (new DuplicateEventsEvent {
				TimelineEvents = plays
			});

			Assert.AreEqual (currentCount, projectVM.Timeline.FullTimeline.Count ());
			mockLimitationService.Verify (ls => ls.MoveToUpgradeDialog (VASCountLimitedObjects.TimelineEvents.ToString ()),
										  Times.Once);
		}

		[Test]
		public void EventsDeletedEvent_NonDeletableEvents_NotDeleted ()
		{
			EventType e1 = projectVM.Model.EventTypes [0];
			var evNotDeletable = new StaticTimelineEvent ();
			evNotDeletable.EventType = e1;
			evNotDeletable.Start = new Time (6000);
			evNotDeletable.Stop = new Time (8000);
			projectVM.Timeline.Model.Add (evNotDeletable);

			int countBeforeDelete = projectVM.Timeline.Model.Count;

			App.Current.EventsBroker.Publish (new EventsDeletedEvent {
				TimelineEvents = new List<TimelineEventVM> { new TimelineEventVM () { Model = evNotDeletable } }
			});

			int countAfterDelete = projectVM.Timeline.Model.Count;

			Assert.AreEqual (countAfterDelete, countBeforeDelete);
			Assert.Contains (evNotDeletable, projectVM.Timeline.Model);
		}

		[Test]
		public void EventsDeletedEvent_DeleteAllEvents_ExceptNotDeletable ()
		{
			EventType e1 = projectVM.Model.EventTypes [0];
			var evNotDeletable = new StaticTimelineEvent ();
			evNotDeletable.EventType = e1;
			evNotDeletable.Start = new Time (6000);
			evNotDeletable.Stop = new Time (8000);
			projectVM.Timeline.Model.Add (evNotDeletable);

			var plays = projectVM.Timeline.Model.Select (m => new TimelineEventVM () { Model = m });

			App.Current.EventsBroker.Publish (new EventsDeletedEvent {
				TimelineEvents = plays
			});

			Assert.AreEqual (1, projectVM.Timeline.Model.Count);
			Assert.AreSame (evNotDeletable, projectVM.Timeline.Model.FirstOrDefault ());
		}

		[Test]
		public void EventsDeletedEvent_NotConfirm_EventsNotDeleted ()
		{
			mockDialogs.Setup (m => m.QuestionMessage (It.IsAny<string> (), null, It.IsAny<object> ())).
			           Returns (AsyncHelpers.Return (false));
			EventType e1 = projectVM.Model.EventTypes [0];
			var evNotDeletable = new StaticTimelineEvent ();
			evNotDeletable.EventType = e1;
			evNotDeletable.Start = new Time (6000);
			evNotDeletable.Stop = new Time (8000);
			projectVM.Timeline.Model.Add (evNotDeletable);

			var countBeforeDelete = projectVM.Timeline.Model.Count;

			App.Current.EventsBroker.Publish (new EventsDeletedEvent {
				TimelineEvents = projectVM.Timeline.FullTimeline
			});

			Assert.AreEqual (countBeforeDelete, projectVM.Timeline.Model.Count);
		}

		bool ComparePlaylist (PlaylistVM playlist, List<TimelineEventVM> eventList)
		{
			if (playlist.ViewModels.Count () != eventList.Count ()) {
				return false;
			}

			int count = playlist.ViewModels.Count ();

			for (int i = 0; i < count; i++) {
				if (!ComparePlaylistElement (playlist.ViewModels [i],
											 eventList [i].Model)) {
					return false;
				}
			}
			return true;
		}

		bool ComparePlaylistElement (IPlayable element, TimelineEvent ev)
		{
			var el = element as PlaylistPlayElementVM;
			if (el != null) {
				return el.TypedModel.Play == ev;
			}
			return false;
		}

		void PreparePlayer (bool readyToSeek = true)
		{
			var player = new VideoPlayerController ();
			player.SetViewModel (videoPlayer);
			var mfs = new MediaFileSet ();
			mfs.Add (new MediaFile {
				FilePath = "test1",
				VideoWidth = 320,
				VideoHeight = 240,
				Par = 1,
				Duration = new Time { TotalSeconds = 5000 }
			});
			player.CamerasConfig = new RangeObservableCollection<CameraConfig> {
					new CameraConfig (0),
					new CameraConfig (1)
				};
			Mock<IViewPort> viewPortMock = new Mock<IViewPort> ();
			viewPortMock.SetupAllProperties ();
			player.ViewPorts = new List<IViewPort> { viewPortMock.Object, viewPortMock.Object };
			player.Ready (true);
			player.Open (mfs);
		}
	}

	/// <summary>
	/// Static timeline event, this event can not deleted (Deletable=false) just for test purposes.
	/// </summary>
	class StaticTimelineEvent : TimelineEvent
	{
		public StaticTimelineEvent ()
		{
			Deletable = false;
		}
	}
}
