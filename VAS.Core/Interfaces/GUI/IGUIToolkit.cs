// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using System.Threading.Tasks;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Filters;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using Image = VAS.Core.Common.Image;

namespace VAS.Core.Interfaces.GUI
{
	public interface IGUIToolkit
	{
		/* Plugable views */
		void Register <I, C> (int priority);

		IPlayerView GetPlayerView ();

		IMainController MainController { get; }

		IRenderingStateBar RenderingStateBar { get; }

		bool FullScreen { set; }

		void Quit ();


		List<EditionJob> ConfigureRenderingJob (Playlist playlist);

		void ExportFrameSeries (Project openenedProject, TimelineEvent play, string snapshotDir);

		void OpenProject (Project project, ProjectType projectType, 
		                  CaptureSettings props, EventsFilter filter,
		                  out IAnalysisWindowBase analysisWindow);

		void CloseProject ();

		void SelectProject (List<Project> projects);

		Project ChooseProject (List<Project> projects);

		void Welcome ();

		void LoadPanel (IPanel panel);

		void CreateNewProject (Project project = null);

		void ShowProjectStats (Project project);

		void OpenProjectsManager (Project openedProject);

		void OpenCategoriesTemplatesManager ();

		void OpenTeamsTemplatesManager ();

		void OpenDatabasesManager ();

		void OpenPreferencesEditor ();

		void ManageJobs ();

		Task EditPlay (TimelineEvent play, Project project, bool editTags, bool editPositions, bool editPlayers, bool editNotes);

		void DrawingTool (Image pixbuf, TimelineEvent play, FrameDrawing drawing, CameraConfig config, Project project);

		string RemuxFile (string filePath, string outputFile, VideoMuxerType muxer);

		EndCaptureResponse EndCapture (bool isCapturing);

		bool SelectMediaFiles (MediaFileSet fileSet);

		HotKey SelectHotkey (HotKey hotkey, object parent = null);

		void Invoke (EventHandler handler);

		Task<bool> CreateNewTemplate<T> (IList<T> availableTemplates, string defaultName,
		                                 string countText, string emptyText,
		                                 CreateEvent<T> evt) where T: ITemplate;
	}
}

