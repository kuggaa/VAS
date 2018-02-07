//
//  Copyright (C) 2018 Fluendo S.A.
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
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;
using VAS.Services.State;
using VAS.Services.ViewModel;

namespace VAS.Services.Controller
{
	/// <summary>
	/// Controller for the Quick Video Editor.
	/// </summary>
	[Controller (QuickEditorState.NAME)]
	public class QuickEditorController : ControllerBase<QuickEditorVM>
	{
		enum Tool
		{
			Welcome,
			VideoEditor,
			DrawingTool,
		}

		public override async Task Start ()
		{
			await base.Start ();
			if (ViewModel.VideoFile.Model == null) {
				LoadTool (Tool.Welcome);
			} else {
				LoadFile ();
			}
		}

		protected override void ConnectEvents ()
		{
			base.ConnectEvents ();
			App.Current.EventsBroker.Subscribe<OpenEvent<MediaFileVM>> (HandleChooseMediaFile);
			App.Current.EventsBroker.Subscribe<ExportEvent<TimelineEventVM>> (HandleExport);
		}

		protected override void DisconnectEvents ()
		{
			base.DisconnectEvents ();
			App.Current.EventsBroker.Unsubscribe<OpenEvent<MediaFileVM>> (HandleChooseMediaFile);
			App.Current.EventsBroker.Unsubscribe<ExportEvent<TimelineEventVM>> (HandleExport);
		}

		public override void SetViewModel (IViewModel viewModel)
		{
			ViewModel = (QuickEditorVM)viewModel;
		}

		void LoadFile ()
		{
			if (ViewModel.LoadedEvent.Model == null) {
				ViewModel.LoadedEvent.Model = new TimelineEvent {
					Start = new Time (0),
					Stop = new Time (ViewModel.VideoFile.Duration.MSeconds),
				};
			}
			var fileset = new MediaFileSet ();
			fileset.Add (ViewModel.VideoFile.Model);
			ViewModel.LoadedEvent.FileSet = fileset;
			LoadTool (Tool.VideoEditor);
			ViewModel.VideoPlayer.LoadEvent (ViewModel.LoadedEvent, false);
			ViewModel.VideoPlayer.EditEventDurationCommand.Execute (true);
		}

		void LoadTool (Tool tool)
		{
			ViewModel.WelcomeVisible = tool == Tool.Welcome;
			ViewModel.VideoEditorVisible = tool == Tool.VideoEditor;
		}

		void HandleChooseMediaFile (OpenEvent<MediaFileVM> arg)
		{
			MediaFile file = App.Current.Dialogs.OpenMediaFile ();
			if (file != null) {
				ViewModel.VideoFile.Model = file;
				LoadFile ();
			}
		}

		void HandleExport (ExportEvent<TimelineEventVM> arg)
		{
			Playlist pl = new Playlist ();
			pl.Elements.Add (new PlaylistPlayElement (ViewModel.LoadedEvent.Model));
			List<EditionJob> jobs = App.Current.GUIToolkit.ConfigureRenderingJob (pl);
			if (jobs == null)
				return;
			foreach (Job job in jobs) {
				App.Current.JobsManager.Add (job);
			}
		}
	}
}
