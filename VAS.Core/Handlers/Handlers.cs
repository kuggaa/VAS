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
using VAS.Core.Common;
using VAS.Core.Filters;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;

namespace VAS.Core.Handlers
{
	/* An event was loaded */
	public delegate void EventLoadedHandler (TimelineEvent evt);
	/* An events needs to be loaded */
	public delegate void LoadEventHandler (TimelineEvent evt);
	/* An event has been created */
	public delegate void EventCreatedHandler (TimelineEvent evt);

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
	/* A play list element is selected */
	public delegate void PlaylistElementSelectedHandler (Playlist playlist,IPlaylistElement element,bool playing);

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

}
