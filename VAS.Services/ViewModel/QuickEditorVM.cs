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
using VAS.Core.Events;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using static VAS.Core.Resources.Icons;

namespace VAS.Services.ViewModel
{
	/// <summary>
	/// View Model for the Quick Editor.
	/// </summary>
	public class QuickEditorVM : ViewModelBase
	{
		public QuickEditorVM ()
		{
			ExportCommand = new AsyncCommand (
				async () => await App.Current.EventsBroker.Publish (new ExportEvent<TimelineEventVM> ())) {
				Text = "Export",
				ToolTipText = "Export to a video file",
				IconName = PlayerControlPlay,
			};
			ChooseFileCommand = new AsyncCommand (
				async () => await App.Current.EventsBroker.Publish (new OpenEvent<MediaFileVM> ())) {
				Text = "Open",
				ToolTipText = "Open a video file for edition",
				IconName = OpenButton,
			};
			VideoPlayer = new VideoPlayerVM ();
			LoadedEvent = new TimelineEventVM ();
			VideoFile = new MediaFileVM ();
			DrawingTool = new DrawingToolVM ();
			WelcomeMessage = "Select a video file for editing";
		}

		/// <summary>
		/// Export the current edition to a file.
		/// </summary>
		public Command ExportCommand { get; set; }

		/// <summary>
		/// Select a file to start editing.
		/// </summary>
		/// <value>The choose file command.</value>
		public Command ChooseFileCommand { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the welcome message is visible or not.
		/// The welcome screen is only visible when starting the tool to let the user drag and drop a file
		/// or select a file with the file chooser.
		/// </summary>
		public bool WelcomeVisible { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the video player is visible or not.
		/// The video player is visible when the user has selected a video file and start editing it.
		/// </summary>
		public bool VideoEditorVisible { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the drawing tool is visible or not.
		/// The drawing tool is visible when the user wants to add a drawing or edit an existing one.
		/// </summary>
		public bool DrawingToolVisible { get; set; }

		/// <summary>
		/// Gets or sets the welcome message to be shown to user when the tool is stated and no video file has been
		/// choosen.
		/// </summary>
		/// <value>The welcome message.</value>
		public string WelcomeMessage { get; set; }

		/// <summary>
		/// The Video Player being used to edit the video file.
		/// </summary>
		public VideoPlayerVM VideoPlayer { get; set; }

		/// <summary>
		/// The Drawing Used being to draw on frames in the video.
		/// </summary>
		public DrawingToolVM DrawingTool { get; set; }

		/// <summary>
		/// The video file loaded.
		/// </summary>
		public MediaFileVM VideoFile { get; set; }

		/// <summary>
		/// The loaded event in the video player that defines the boundaries of the video to export.
		/// </summary>
		/// <value>The loaded event.</value>
		public TimelineEventVM LoadedEvent { get; set; }
	}
}
