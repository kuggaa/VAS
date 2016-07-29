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
using VAS.Core.Common;
using VAS.Core.Filters;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.Store.Templates;


namespace VAS.Core.Events
{
	public class ReturningValueEvent : Event
	{
		public bool ReturnValue { get; set; }
	}

	public class LoadEventEvent : Event
	{
		public TimelineEvent TimelineEvent { get; set; }
	}

	public class EventCreatedEvent : Event
	{
		public TimelineEvent TimelineEvent { get; set; }
	}

	public class EventsDeletedEvent : Event
	{
		public List<TimelineEvent> TimelineEvents { get; set; }
	}

	public class EventLoadedEvent : Event
	{
		public TimelineEvent TimelineEvent { get; set; }
	}

	public class TimeNodeChangedEvent : Event
	{
		public TimeNode TimeNode { get; set; }

		public Time Time { get; set; }
	}

	public class SnapshotSeriesEvent : Event
	{
		public TimelineEvent TimelineEvent { get; set; }
	}

	public class MoveToEventTypeEvent : Event
	{
		public TimelineEvent TimelineEvent { get; set; }

		public EventType EventType { get; set; }
	}

	public class DuplicateEventsEvent : Event
	{
		public List<TimelineEvent> TimelineEvents { get; set; }
	}

	public class TagSubcategoriesChangedEvent : Event
	{
		public bool Active { get; set; }
	}

	public class EventEditedEvent : Event
	{
		public TimelineEvent TimelineEvent { get; set; }
	}

	public class DashboardEditedEvent : Event
	{
	}

	public class TimeNodeStartedEvent : Event
	{
		public TimeNode TimeNode { get; set; }

		public TimerButton TimerButton { get; set; }

		public List<DashboardButton> DashboardButtons {
			get {
				if (dashboardButton == null) {
					dashboardButton = new List<DashboardButton> ();
				}
				return dashboardButton;
			}
			set { 
				dashboardButton = value;
			}
		}

		List<DashboardButton> dashboardButton;
	}

	public class TimeNodeStoppedEvent : Event
	{
		public TimeNode TimeNode { get; set; }

		public TimerButton TimerButton { get; set; }

		public List<DashboardButton> DashboardButtons {
			get {
				if (dashboardButton == null) {
					dashboardButton = new List<DashboardButton> ();
				}
				return dashboardButton;
			}
			set { 
				dashboardButton = value;
			}
		}

		List<DashboardButton> dashboardButton;
	}

	public class DatabaseCreatedEvent : Event
	{
		public string Name { get; set; }
	}

	public class KeyPressedEvent : Event
	{
		public HotKey Key;
	}

	public class RenderPlaylistEvent : Event
	{
		public Playlist Playlist { get; set; }
	}

	public class AddPlaylistElementEvent : Event
	{
		public Playlist Playlist { get; set; }

		public List<IPlaylistElement> PlaylistElements { get; set; }
	}

	public class LoadPlaylistElementEvent : Event
	{
		public Playlist Playlist { get; set; }

		public IPlaylistElement Element  { get; set; }

		public bool Playing  { get; set; }
	}

	public class PlaylistElementLoadedEvent : Event
	{
		public Playlist Playlist { get; set; }

		public IPlaylistElement Element { get; set; }
	}

	public class NewPlaylistEvent : Event
	{
		public Project Project { get; set; }
	}

	public class NextPlaylistElementEvent : Event
	{
		public Playlist Playlist { get; set; }
	}

	public class PreviousPlaylistElementEvent : Event
	{
		public Playlist Playlist { get; set; }
	}

	public class PlayerTickEvent : Event
	{
		public Time Time { get; set; }
	}

	public class CapturerTickEvent : Event
	{
		public Time Time { get; set; }
	}

	public class MultimediaErrorEvent : Event
	{
		//public object Sender { get; set; }

		public string Message { get; set; }
	}

	public class CaptureErrorEvent : Event
	{
		//public object Sender { get; set; }

		public string Message { get; set; }
	}

	public class DrawFrameEvent : Event
	{
		public TimelineEvent Play { get; set; }

		public int DrawingIndex { get; set; }

		public CameraConfig CamConfig { get; set; }

		public bool Current { get; set; }
	}

	public class CaptureFinishedEvent : Event
	{
		public bool Cancel { get; set; }

		public bool Reopen { get; set; }
	}

	public class PlaybackStateChangedEvent : Event
	{
		//public object Sender { get; set; }

		public bool Playing { get; set; }
	}

	public class PlaybackRateChangedEvent : Event
	{
		public float Value { get; set; }
	}

	public class SeekEvent : Event
	{
		public Time Time { get; set; }

		public bool Accurate { get; set; }

		public bool Synchronous { get; set; } = false;

		public bool Throttled { get; set; } = false;
	}

	public class TogglePlayEvent : Event
	{
		public bool Playing { get; set; }
	}

	public class DetachEvent : Event
	{
	}

	public class ConvertVideoFilesEvent : Event
	{
		public List<MediaFile> Files { get; set; }

		public EncodingSettings Settings { get; set; }
	}

	public class QuitApplicationEvent : Event
	{
	}

	public class OpenedPresentationChangedEvent : Event
	{
		public Playlist Presentation { get; set; }

		public IPlayerController Player { get; set; }
	}

	public class CreateProjectEvent : Event
	{
	}

	public class SaveProjectEvent : Event
	{
		public Project Project { get; set; }

		public ProjectType ProjectType { get; set; }
	}

	public class CloseOpenedProjectEvent : ReturningValueEvent
	{
	}

	public class ShowFullScreenEvent : Event
	{
		public bool Active { get; set; }
	}

	public class OpenedProjectEvent : Event
	{
		public Project Project { get; set; }

		public ProjectType ProjectType { get; set; }

		public EventsFilter Filter { get; set; }

		public IAnalysisWindowBase AnalysisWindow { get; set; }
	}

	public class NewDashboardEvent : Event
	{
		public TimelineEvent TimelineEvent { get; set; }

		public DashboardButton DashboardButton { get; set; }

		public bool Edit { get; set; }

		public List<DashboardButton> DashboardButtons { 
			get {
				if (dashboardButtons == null) {
					dashboardButtons = new List<DashboardButton> ();
				}
				return dashboardButtons;
			}
			set {
				dashboardButtons = value;
			}
		}

		List<DashboardButton> dashboardButtons;
	}

	public class NewEventEvent : Event
	{
		public EventType EventType { get; set; }

		public List<Player> Players { get; set; }

		public ObservableCollection<Team> Teams { get; set; }

		public List<Tag> Tags { get; set; }

		public Time Start { get; set; }

		public Time Stop { get; set; }

		public Time EventTime { get; set; }

		public DashboardButton Button { get; set; }
	}
}