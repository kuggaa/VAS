//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using VAS.Core.Filters;
using VAS.Core.Handlers;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.Store.Templates;

namespace VAS.Core.Common
{
	public class EventsBroker
	{
		public event LoadEventHandler LoadEventEvent;
		public event EventCreatedHandler EventCreatedEvent;
		public event EventLoadedHandler EventLoadedEvent;
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event DeleteEventsHandler EventsDeletedEvent;
		public event SnapshotSeriesHandler SnapshotSeries;
		public event MoveEventHandler MoveToEventTypeEvent;
		public event DuplicateEventsHandler DuplicateEventsEvent;
		public event TagSubcategoriesChangedHandler TagSubcategoriesChangedEvent;
		public event EventEditedHandler EventEditedEvent;

		public event NewEventHandler NewEventEvent;
		public event NewDashboardEventHandler NewDashboardEventEvent;

		public event DashboardEditedHandler DashboardEditedEvent;

		public event TimeNodeStartedHandler TimeNodeStartedEvent;
		public event TimeNodeStoppedHandler TimeNodeStoppedEvent;
		public event DatabaseCreatedHandler DatabaseCreatedEvent;

		public event KeyHandler KeyPressed;

		/* Playlist */
		public event RenderPlaylistHandler RenderPlaylist;
		public event AddPlaylistElementHandler AddPlaylistElementEvent;
		public event LoadPlaylistElementHandler LoadPlaylistElementEvent;
		public event PlaylistElementLoadedHandler PlaylistElementLoadedEvent;
		public event NewPlaylistHandler NewPlaylistEvent;
		public event NextPlaylistElementHandler NextPlaylistElementEvent;
		public event PreviousPlaylistElementHandler PreviousPlaylistElementEvent;

		/* Player and Capturer */
		public event TickHandler PlayerTick;
		public event ErrorHandler MultimediaError;
		public event DrawFrameHandler DrawFrame;
		public event StateChangeHandler PlaybackStateChangedEvent;
		public event PlaybackRateChangedHandler PlaybackRateChanged;
		public event SeekEventHandler SeekEvent;
		public event TogglePlayEventHandler TogglePlayEvent;
		public event DetachPlayerHandler Detach;

		/* IMainController */
		public event ConvertVideoFilesHandler ConvertVideoFilesEvent;
		public event QuitApplicationHandler QuitApplicationEvent;

		public event OpenedProjectChangedHandler OpenedProjectChanged;
		public event OpenedPresentationChangedHandler OpenedPresentationChanged;

		/* Project options */
		public event SaveProjectHandler SaveProjectEvent;
		public event CloseOpenendProjectHandler CloseOpenedProjectEvent;
		public event ShowFullScreenHandler ShowFullScreenEvent;


		public void EmitNewEvent (EventType eventType, List<Player> players = null, ObservableCollection<Team> teams = null,
		                          List<Tag> tags = null, Time start = null, Time stop = null, Time eventTime = null)
		{
			if (NewEventEvent != null)
				NewEventEvent (eventType, players, teams, tags, start, stop, eventTime, null);
		}

		public void EmitEventCreated (TimelineEvent evt)
		{
			if (EventCreatedEvent != null) {
				EventCreatedEvent (evt);
			}
		}

		public void EmitEventLoaded (TimelineEvent play)
		{
			if (EventLoadedEvent != null)
				EventLoadedEvent (play);
		}

		public void EmitTimeNodeChanged (TimeNode tn, Time time)
		{
			if (TimeNodeChanged != null)
				TimeNodeChanged (tn, time);
		}

		public void EmitEventsDeleted (List<TimelineEvent> events)
		{
			if (EventsDeletedEvent != null)
				EventsDeletedEvent (events);
		}

		public void EmitNewDashboardEvent (TimelineEvent evt, DashboardButton btn, bool edit, List<DashboardButton> from)
		{
			if (NewDashboardEventEvent != null) {
				if (from == null)
					from = new List<DashboardButton> ();
				NewDashboardEventEvent (evt, btn, edit, from);
			}
		}

		public virtual void EmitMoveToEventType (TimelineEvent evnt, EventType eventType)
		{
			if (MoveToEventTypeEvent != null)
				MoveToEventTypeEvent (evnt, eventType);
		}

		public void EmitDuplicateEvent (List<TimelineEvent> events)
		{
			if (DuplicateEventsEvent != null)
				DuplicateEventsEvent (events);
		}

		public void EmitMultimediaError (object sender, string message)
		{
			if (MultimediaError != null) {
				MultimediaError (sender, message);
			}
		}

		public void EmitPlaybackStateChanged (object sender, bool playing)
		{
			if (PlaybackStateChangedEvent != null) {
				PlaybackStateChangedEvent (sender, playing);
			}
		}

		public virtual void EmitDrawFrame (TimelineEvent play, int drawingIndex, CameraConfig camConfig, bool current)
		{
			if (DrawFrame != null) {
				DrawFrame (play, drawingIndex, camConfig, current);
			}
		}

		public void EmitPlaybackRateChanged (float val)
		{
			if (PlaybackRateChanged != null) {
				PlaybackRateChanged (val);
			}
		}

		public virtual void EmitLoadPlaylistElement (Playlist playlist, IPlaylistElement element, bool playing)
		{
			if (LoadPlaylistElementEvent != null)
				LoadPlaylistElementEvent (playlist, element, playing);
		}

		public virtual void EmitPlaylistElementLoaded (Playlist playlist, IPlaylistElement element)
		{
			if (PlaylistElementLoadedEvent != null)
				PlaylistElementLoadedEvent (playlist, element);
		}

		public void EmitRenderPlaylist (Playlist playlist)
		{
			if (RenderPlaylist != null)
				RenderPlaylist (playlist);
		}

		public void EmitNewPlaylist (Project project)
		{
			if (NewPlaylistEvent != null) {
				NewPlaylistEvent (project);
			}
		}

		public void EmitAddPlaylistElement (Playlist playlist, List<IPlaylistElement> plays)
		{
			if (AddPlaylistElementEvent != null)
				AddPlaylistElementEvent (playlist, plays);
		}

		public void EmitNextPlaylistElement (Playlist playlist)
		{
			if (NextPlaylistElementEvent != null) {
				NextPlaylistElementEvent (playlist);
			}
		}

		public void EmitPreviousPlaylistElement (Playlist playlist)
		{
			if (PreviousPlaylistElementEvent != null) {
				PreviousPlaylistElementEvent (playlist);
			}
		}

		public virtual void EmitPlayerTick (Time currentTime)
		{
			if (PlayerTick != null) {
				PlayerTick (currentTime);
			}
		}

		public virtual void EmitTimeNodeStartedEvent (TimeNode node, TimerButton btn, List<DashboardButton> from)
		{
			if (TimeNodeStartedEvent != null) {
				if (from == null)
					from = new List<DashboardButton> ();
				TimeNodeStartedEvent (node, btn, from);
			}
		}

		public virtual void EmitTimeNodeStoppedEvent (TimeNode node, TimerButton btn, List<DashboardButton> from)
		{
			if (TimeNodeStoppedEvent != null) {
				if (from == null)
					from = new List<DashboardButton> ();
				TimeNodeStoppedEvent (node, btn, from);
			}
		}

		public virtual void EmitDatabaseCreated (string name)
		{
			if (DatabaseCreatedEvent != null) {
				DatabaseCreatedEvent (name);
			}
		}

		public virtual void EmitKeyPressed (object sender, HotKey key)
		{
			if (KeyPressed != null)
				KeyPressed (sender, key);
		}

		public void EmitLoadEvent (TimelineEvent evt)
		{
			if (LoadEventEvent != null)
				LoadEventEvent (evt);
		}

		public  void EmitOpenedProjectChanged (Project project, ProjectType projectType,
		                                       EventsFilter filter, IAnalysisWindowBase analysisWindow)
		{
			if (OpenedProjectChanged != null) {
				OpenedProjectChanged (project, projectType, filter, analysisWindow);
			}
		}

		public  void EmitOpenedPresentationChanged (Playlist presentation, IPlayerController player)
		{
			if (OpenedPresentationChanged != null) {
				OpenedPresentationChanged (presentation, player);
			}
		}

		public void EmitConvertVideoFiles (List<MediaFile> files, EncodingSettings settings)
		{
			if (ConvertVideoFilesEvent != null)
				ConvertVideoFilesEvent (files, settings);
		}

		public void EmitSeekEvent (Time time, bool accurate, bool synchronous = false, bool throttled = false)
		{
			if (SeekEvent != null) {
				SeekEvent (time, accurate, synchronous, throttled);
			}
		}

		public void EmitTogglePlayEvent (bool playing)
		{
			if (TogglePlayEvent != null) {
				TogglePlayEvent (playing);
			}
		}

		public void EmitSnapshotSeries (TimelineEvent play)
		{
			if (SnapshotSeries != null)
				SnapshotSeries (play);
		}

		public void EmitDashboardEdited ()
		{
			if (DashboardEditedEvent != null) {
				DashboardEditedEvent ();
			}
		}

		public void EmitSaveProject (Project project, ProjectType projectType)
		{
			if (SaveProjectEvent != null)
				SaveProjectEvent (project, projectType);
		}

		public bool EmitCloseOpenedProject ()
		{
			if (CloseOpenedProjectEvent != null)
				return CloseOpenedProjectEvent ();
			return false;
		}

		public void EmitTagSubcategories (bool active)
		{
			if (TagSubcategoriesChangedEvent != null)
				TagSubcategoriesChangedEvent (active);
		}

		public void EmitEventEdited (TimelineEvent play)
		{
			if (EventEditedEvent != null) {
				EventEditedEvent (play);
			}
		}

		public void EmitShowFullScreen (bool active)
		{
			if (ShowFullScreenEvent != null) {
				ShowFullScreenEvent (active);
			}
		}

		public void EmitQuitApplication ()
		{
			if (QuitApplicationEvent != null) {
				QuitApplicationEvent ();
			}
		}

		public void EmitDetach ()
		{
			if (Detach != null) {
				Detach ();
			}
		}
	}
}
