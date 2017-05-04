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
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.Store.Templates;
using VAS.Core.ViewModel;

namespace VAS.Core.Events
{
	public class ReturningValueEvent<T> : Event
	{
		public T ReturnValue { get; set; }
	}

	public class ReturningValueEvent : ReturningValueEvent<bool>
	{
	}

	// FIXME: this event should be replaced with the one using the ViewModel
	public class LoadEventEvent : Event
	{
		/// <summary>
		/// Gets or sets the timeline event to load.
		/// </summary>
		/// <value>The timeline event.</value>
		public TimelineEvent TimelineEvent { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.Events.LoadEventEvent"/> will
		/// start playing when it's loaded.
		/// </summary>
		/// <value><c>true</c> if playing; otherwise, <c>false</c>.</value>
		public bool Playing { get; set; }
	}

	public class LoadTimelineEventEvent<T> : Event
	{
		/// <summary>
		/// Gets or sets the timeline event to load.
		/// </summary>
		/// <value>The timeline event.</value>
		public T Object { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.Events.LoadEventEvent"/> will
		/// start playing when it's loaded.
		/// </summary>
		/// <value><c>true</c> if playing; otherwise, <c>false</c>.</value>
		public bool Playing { get; set; }
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
		public IList<TimelineEvent> TimelineEvents { get; set; }

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

	public class EditEventEvent : Event
	{
		public TimelineEvent TimelineEvent { get; set; }
	}

	public class EventEditedEvent : Event
	{
		public TimelineEvent TimelineEvent { get; set; }
	}

	public class DashboardEditedEvent : Event
	{
	}

	#region Participant card events

	public class ClickedPCardEvent : Event
	{
		public PlayerVM ClickedPlayer { get; set; }

		public ButtonModifier Modifier { get; set; }
	}

	#endregion

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

	public class JobRenderedEvent : Event
	{
	}

	public class RenderPlaylistEvent : Event
	{
		public Playlist Playlist { get; set; }
	}

	public class DeletePlaylistEvent : Event
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

		public IPlaylistElement Element { get; set; }

		public bool Playing { get; set; }
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

		public Time RelativeTime { get; set; }
	}

	public class CapturerTickEvent : Event
	{
		public Time Time { get; set; }
	}

	public class MultimediaErrorEvent : Event
	{
		public string Message { get; set; }
	}

	public class CaptureErrorEvent : Event
	{
		public string Message { get; set; }
	}

	public class DrawFrameEvent : Event
	{
		public TimelineEvent Play { get; set; }

		public int DrawingIndex { get; set; }

		public CameraConfig CamConfig { get; set; }

		public bool Current { get; set; }
	}

	public class DrawingSavedToProjectEvent : Event
	{
		public Guid ProjectId { get; set; }
	}

	public class CaptureFinishedEvent : Event
	{
		public bool Cancel { get; set; }

		public bool Reopen { get; set; }
	}

	public class PlaybackStateChangedEvent : Event
	{
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

	public class QuitApplicationEvent : Event
	{
	}

	public class OpenedPresentationChangedEvent : Event
	{
		public Playlist Presentation { get; set; }

		public IVideoPlayerController Player { get; set; }
	}

	public class ProjectCreatedEvent : Event
	{
		public Guid ProjectId { get; set; }
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

		public Guid ProjectId { get; set; }
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

	/// <summary>
	/// Event sent to notify about navigation in the application.
	/// </summary>
	public class NavigationEvent : Event
	{
		/// <summary>
		/// Gets or sets the name of the transition.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets a value indicating if we are navigating to a modal state.
		/// </summary>
		/// <value><c>true</c> if it is a modal state; otherwise, <c>false</c>.</value>
		public bool IsModal { get; set; }

	}

	/// <summary>
	/// Event to move <typeparam name="TChild"> elements of different parents:<typeparam name="TParent"> to another one at a specified index
	/// </summary>
	public class MoveElementsEvent<TParent, TChild> : Event
	{
		public Dictionary<TParent, IEnumerable<TChild>> ElementsToRemove { get; set; }
		public KeyValuePair<TParent, IEnumerable<TChild>> ElementsToAdd { get; set; }
		public int Index { get; set; }
	}

	public class LicenseChangeEvent : Event
	{
	}
}
