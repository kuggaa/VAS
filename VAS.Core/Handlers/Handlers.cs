// Handlers.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VAS.Core.Common;
using VAS.Core.Filters;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.Store.Templates;

namespace VAS.Core.Handlers
{
	/* An event was loaded */
	public delegate void EventLoadedHandler (TimelineEvent evt);

	/* An events needs to be loaded */
	public delegate void LoadEventHandler (TimelineEvent evt);

	/* An event has been created */
	public delegate void EventCreatedHandler (TimelineEvent evt);

	/* A list of plays needs to be deleted */
	public delegate void DeleteEventsHandler (List<TimelineEvent> events);

	/* Create snapshots for a play */
	public delegate void SnapshotSeriesHandler (TimelineEvent evt);

	/* Move the event to a different event category */
	public delegate void MoveEventHandler (TimelineEvent play,EventType eventType);

	/* An event was edited */
	public delegate void EventEditedHandler (TimelineEvent play);

	/* Duplicate play */
	public delegate void DuplicateEventsHandler (List<TimelineEvent> events);

	/* Add a new event to the current project from the dashboard */
	public delegate void NewDashboardEventHandler (TimelineEvent evt,DashboardButton btn,bool edit,List<DashboardButton> from);

	/* A new play needs to be created for a specific category at the current play time */
	public delegate void NewEventHandler (EventType eventType,List<Player> players,ObservableCollection<Team> team,
		List<Tag> tags,Time start,Time stop,Time EventTime,DashboardButton btn);

	/* Edit the event subcategories */
	public delegate void TagSubcategoriesChangedHandler (bool tagsubcategories);

	public delegate void TimeNodeStartedHandler (TimeNode tn,TimerButton btn,List<DashboardButton> from);
	public delegate void TimeNodeStoppedHandler (TimeNode tn,TimerButton btn,List<DashboardButton> from);

	/* A new database has been created */
	public delegate void DatabaseCreatedHandler (string name);

	/* Project Events */
	public delegate void OpenedProjectChangedHandler (Project project,ProjectType projectType,EventsFilter filter,
		IAnalysisWindowBase analysisWindow);
	public delegate void OpenedPresentationChangedHandler (Playlist presentation,IPlayerController player);

	/* An event was edited */
	public delegate void TimeNodeChangedHandler (TimeNode tNode,Time time);
	/* Edit EventType properties */
	public delegate void EditEventTypeHandler (EventType cat);

	/* Emited when the dashboard is edited and might have new EventTypes */
	public delegate void DashboardEditedHandler ();

	/* Dashboard buttons selected */
	public delegate void ButtonsSelectedHandler (List<DashboardButton> taggerbuttons);
	public delegate void ButtonSelectedHandler (DashboardButton taggerbutton);

	/* Dashboard link selected */
	public delegate void ActionLinksSelectedHandler (List<ActionLink> actionLink);

	/* Dashboard link crated */
	public delegate void ActionLinkCreatedHandler (ActionLink actionLink);

	/* Show dashboard menu */
	public delegate void ShowDashboardMenuHandler (List<DashboardButton> selectedButtons,List<ActionLink> selectedLinks);

	public delegate void QuitApplicationHandler ();

	/*Playlist Events*/
	/* Create a new playlist */
	public delegate Playlist NewPlaylistHandler (Project project);
	/* Add a new rendering job */
	public delegate void RenderPlaylistHandler (Playlist playlist);
	/* Add a play to a playlist */
	public delegate void AddPlaylistElementHandler (Playlist playlist,List<IPlaylistElement> element);
	/* Play next playlist element */
	public delegate void NextPlaylistElementHandler (Playlist playlist);
	/* Play previous playlist element */
	public delegate void PreviousPlaylistElementHandler (Playlist playlist);
	/* Request a play list element to be selected */
	public delegate void LoadPlaylistElementHandler (Playlist playlist,IPlaylistElement element,bool playing);
	/* A play list element is selected */
	public delegate void PlaylistElementLoadedHandler (Playlist playlist,IPlaylistElement element);

	/* GUI */
	public delegate void ManageJobsHandler ();
	public delegate void ManageTeamsHandler ();
	public delegate void ManageDashboardsHandler ();
	public delegate void ManageProjects ();
	public delegate void ManageDatabases ();
	public delegate void EditPreferences ();
	public delegate void MigrateDBHandler ();

	/* Convert a video file */
	public delegate void ConvertVideoFilesHandler (List<MediaFile> inputFiles,EncodingSettings encSettings);
	/* A date was selected */
	public delegate void DateSelectedHandler (DateTime selectedDate);
	/* A new version of the software exists */
	public delegate void NewVersionHandler (Version version,string URL);

	public delegate void KeyHandler (object sender,HotKey key);
	/* The plays filter was updated */
	public delegate void FilterUpdatedHandler ();
	public delegate void DetachPlayerHandler ();

	public delegate void ShowFullScreenHandler (bool fullscreen);

	public delegate void ShowDrawToolMenuHandler (IBlackboardObject drawable);
	public delegate void ConfigureDrawingObjectHandler (IBlackboardObject drawable,DrawTool tool);
	public delegate void DrawableChangedHandler (IBlackboardObject drawable);
	public delegate void BackEventHandle ();
	/* Camera dragging */
	public delegate void CameraDraggedHandler (MediaFile file,TimeNode timenode);
	public delegate void ShowTimersMenuHandler (List<TimeNode> timenodes);
	public delegate void ShowTimerMenuHandler (Timer timer,Time time);

	/* Project Events */
	public delegate void SaveProjectHandler (Project project,ProjectType projectType);
	public delegate bool CloseOpenendProjectHandler ();
	public delegate void ShowProjectStats (Project project);

}
