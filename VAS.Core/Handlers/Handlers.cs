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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Templates;
using VAS.Core.ViewModel;
using Timer = VAS.Core.Store.Timer;

namespace VAS.Core.Handlers
{
	/* A new play needs to be created for a specific category at the current play time */
	public delegate void NewEventHandler (EventType eventType, List<Player> players, ObservableCollection<Team> team,
		List<TagVM> tags, Time start, Time stop, Time EventTime, DashboardButton btn);

	/* An event was edited */
	public delegate void TimeNodeChangedHandler (TimeNode tNode, Time time);
	/* Edit EventType properties */
	public delegate void EditEventTypeHandler (EventType cat);

	/* Dashboard buttons selected */
	public delegate void ButtonsSelectedHandler (List<DashboardButton> taggerbuttons);
	public delegate void ButtonSelectedHandler (DashboardButton taggerbutton);

	/* Dashboard link selected */
	public delegate void ActionLinksSelectedHandler (List<ActionLinkVM> actionLink);

	/* Dashboard link crated */
	public delegate void ActionLinkCreatedHandler (ActionLinkVM actionLink);

	/* Show dashboard menu */
	public delegate void ShowDashboardMenuHandler (List<DashboardButtonVM> selectedButtons, List<ActionLinkVM> selectedLinks);

	/* The plays filter was updated */
	public delegate void FilterUpdatedHandler ();

	public delegate void ShowDrawToolMenuHandler (IBlackboardObject drawable);
	public delegate void ConfigureDrawingObjectHandler (IBlackboardObject drawable, DrawTool tool);
	public delegate void DrawableChangedHandler (IEnumerable<IBlackboardObject> drawables);
	public delegate void BackEventHandle ();
	/* Camera dragging */
	public delegate void CameraDraggedHandler (MediaFile file, TimeNode timenode);

	/* Show project stats */
	public delegate void ShowTimersMenuHandler (List<TimeNode> timenodes);
	public delegate void ShowTimerMenuHandler (Timer timer, Time time);
	public delegate void ShowPeriodsMenuHandler (Timer timer, Time time);
	public delegate void ShowTimelineMenuHandler (List<TimelineEvent> plays, EventType cat, Time time);

	/* Button clicked */
	public delegate void ClickedHandler (object sender, ClickedArgs args);

	/* Passing ViewModel */
	public delegate void ViewModelHandler (object sender, IViewModel viewModel);
}
