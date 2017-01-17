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
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;
using VAS.Services.Controller;

namespace VAS.Tests.Services
{
	[TestFixture]
	public class TestEventsController
	{
		EventsController controller;
		Mock<IVideoPlayerController> playerController;
		TimelineEvent ev1, ev2;
		TimelineEventVM evVM1, evVM2;

		[SetUp]
		public void Setup ()
		{
			controller = new EventsController ();
			playerController = new Mock<IVideoPlayerController> ();
			controller.PlayerVM = new VideoPlayerVM { Player = playerController.Object };
			controller.Start ();
			ev1 = new TimelineEvent ();
			ev1.Start = new Time (0);
			ev1.Stop = new Time (1000);
			evVM1 = new TimelineEventVM { Model = ev1, Visible = true };
			ev2 = new TimelineEvent ();
			ev2.Start = new Time (2000);
			ev2.Stop = new Time (3000);
			evVM2 = new TimelineEventVM { Model = ev2, Visible = true };
		}

		[TearDown]
		public void TearDown ()
		{
			controller.Stop ();
		}

		[Test]
		public void TestLoadTimelineEvent ()
		{
			App.Current.EventsBroker.Publish (new LoadTimelineEventEvent<TimelineEventVM> {
				Object = evVM1
			});

			playerController.Verify (p => p.LoadEvent (It.Is<TimelineEvent> (tle => tle == ev1),
													   It.IsAny<Time> (), It.IsAny<bool> ()), Times.Once);
		}

		[Test]
		public void TestLoadTimelineEvents ()
		{
			IEnumerable<TimelineEventVM> eventList = new List<TimelineEventVM> { evVM1, evVM2 };

			App.Current.EventsBroker.Publish (new LoadTimelineEventEvent<IEnumerable<TimelineEventVM>> {
				Object = eventList
			});

			playerController.Verify (p => p.LoadPlaylistEvent (It.Is<Playlist> (pl => pl.Elements.Count == 2),
															   It.Is<IPlaylistElement> (pe => ComparePlaylistElement (pe, ev1)),
															   It.IsAny<bool> ()), Times.Once);
		}

		[Test]
		public void TestLoadEventTypeTimelineVM ()
		{
			EventTypeTimelineVM eTypeVM = new EventTypeTimelineVM ();
			eTypeVM.ViewModels.Add (evVM1);
			eTypeVM.ViewModels.Add (evVM2);

			eTypeVM.LoadEventType ();

			playerController.Verify (p => p.LoadPlaylistEvent (It.Is<Playlist> (pl => pl.Elements.Count == 2),
															   It.Is<IPlaylistElement> (pe => ComparePlaylistElement (pe, ev1)),
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

			playerController.Verify (p => p.LoadPlaylistEvent (It.Is<Playlist> (pl => pl.Elements.Count == 1),
															   It.Is<IPlaylistElement> (pe => ComparePlaylistElement (pe, ev2)),
															   It.IsAny<bool> ()), Times.Once);
		}

		bool ComparePlaylistElement (IPlaylistElement element, TimelineEvent ev)
		{
			var el = element as PlaylistPlayElement;
			if (el != null) {
				return el.Play == ev;
			}
			return false;
		}
	}
}
