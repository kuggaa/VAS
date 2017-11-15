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
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;

namespace VAS.Core.Interfaces.GUI
{
	public interface IGUIToolkit
	{
		/// <summary>
		/// Gets the current device scale factor, 1 in regular monitor or > 1 in HiDPI ones.
		/// </summary>
		float DeviceScaleFactor {
			get;
		}

		IMainController MainController { get; }

		bool FullScreen { set; }

		Task<bool> Quit ();

		List<EditionJob> ConfigureRenderingJob (Playlist playlist);

		void ExportFrameSeries (TimelineEvent play, string snapshotDir);

		Project ChooseProject (List<Project> projects);

		bool LoadPanel (IPanel panel);

		void ShowProjectStats (Project project);

		string RemuxFile (string filePath, string outputFile, VideoMuxerType muxer);

		EndCaptureResponse EndCapture (bool isCapturing);

		bool SelectMediaFiles (MediaFileSet fileSet);

		HotKey SelectHotkey (HotKey hotkey, object parent = null);

		/// <summary>
		/// Queues the handler in the main thread. It returns as soon as the handler is queued.
		/// </summary>
		/// <param name="handler">Handler.</param>
		void Invoke (EventHandler handler);

		/// <summary>
		/// Invokes the handler in the main thread asynchronously.
		/// </summary>
		/// <param name="handler">Handler.</param>
		Task<T> Invoke<T> (Func<Task<T>> handler);

		Task<bool> CreateNewTemplate<T> (IList<T> availableTemplates, string defaultName,
										 string countText, string emptyText,
										 CreateEvent<T> evt) where T : ITemplate;

		/// <summary>
		/// Runs a secondary loop until the condition is met (condition returns true)
		/// </summary>
		/// <param name="condition">Condition.</param>
		void RunLoop (Func<bool> condition);
	}
}

