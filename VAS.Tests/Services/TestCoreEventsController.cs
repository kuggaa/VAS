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
using System.Collections.ObjectModel;
using System.Linq;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services;
using VAS.Services.Controller;
using VAS.Services.ViewModel;

namespace VAS.Tests.Services
{
	[TestFixture]
	public class TestCoreEventsController
	{
		CoreEventsController controller;
		ProjectVM projectVM;
		VideoPlayerVM videoPlayer;

		[SetUp]
		public void Setup ()
		{
			var capturerMock = new Mock<IFramesCapturer> ();
			capturerMock.SetupAllProperties ();
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

			var mtkMock = new Mock<IMultimediaToolkit> ();
			mtkMock.Setup (m => m.GetPlayer ()).Returns (playerMock.Object);
			mtkMock.Setup (m => m.GetFramesCapturer ()).Returns (capturerMock.Object);
			App.Current.MultimediaToolkit = mtkMock.Object;

			videoPlayer = new VideoPlayerVM {
				CamerasConfig = new ObservableCollection<CameraConfig> ()
			};

			controller = new CoreEventsController ();
			projectVM = new DummyProjectVM { Model = Utils.CreateProject (true) };
			controller.SetViewModel (new ProjectAnalysisVM<ProjectVM> {
				Project = projectVM,
				VideoPlayer = videoPlayer
			});
			controller.Start ();

			PreparePlayer ();
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
			player.CamerasConfig = new ObservableCollection<CameraConfig> {
					new CameraConfig (0),
					new CameraConfig (1)
				};
			Mock<IViewPort> viewPortMock = new Mock<IViewPort> ();
			viewPortMock.SetupAllProperties ();
			player.ViewPorts = new List<IViewPort> { viewPortMock.Object, viewPortMock.Object };
			player.Ready (true);
			player.Open (mfs);
		}

		[TearDown ()]
		public void TearDown ()
		{
			controller.Stop ();
		}

		[Test]
		public void TestMoveToEventType ()
		{
			EventType e1 = projectVM.Model.EventTypes [0];
			EventType e2 = projectVM.Model.EventTypes [1];
			var eventsToMove = projectVM.Model.Timeline.Where (e => e.EventType == e1).ToList ();
			Assert.AreEqual (1, eventsToMove.Count);
			Assert.AreEqual (1, projectVM.Model.Timeline.Count (e => e.EventType == e2));

			App.Current.EventsBroker.Publish (
				new MoveToEventTypeEvent {
					TimelineEvents = eventsToMove,
					EventType = e2,
				}
			);

			Assert.AreEqual (0, projectVM.Timeline.FullTimeline.Count (e => e.Model.EventType == e1));
			Assert.AreEqual (2, projectVM.Timeline.FullTimeline.Count (e => e.Model.EventType == e2));
		}

		[Test]
		public void NewDashboardEventAddsEventToTimeline ()
		{
			int currentCount = projectVM.Timeline.FullTimeline.Count ();
			EventType e1 = projectVM.Model.EventTypes [0];
			TimelineEvent ev = new TimelineEvent {
				EventType = e1,
				Start = new Time (0),
				Stop = new Time (1000),
				EventTime = new Time (500)
			};

			App.Current.EventsBroker.Publish (new NewDashboardEvent {
				TimelineEvent = ev
			});

			Assert.AreEqual (currentCount + 1, projectVM.Timeline.FullTimeline.Count ());
			Assert.AreSame (ev, projectVM.Timeline.FullTimeline.ViewModels [currentCount].Model);
		}
	}
}
