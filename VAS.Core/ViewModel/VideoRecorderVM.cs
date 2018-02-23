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
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Resources;

namespace VAS.Core.ViewModel
{
	public class VideoRecorderVM : ViewModelBase
	{
		public VideoRecorderVM (CaptureSettings settings, MediaFileVM outputFile, ICapturer capturer)
		{
			StartRecordingCommand = new Command<bool> () {
				ToolTipText = Strings.StartRecording,
				IconName = "vas-control-record"
			};
			StopRecordingCommand = new Command () {
				ToolTipText = Strings.StopRecording,
				IconName = "vas-stop"
			};
			PauseClockCommand = new Command () {
				ToolTipText = Strings.PauseClock,
				IconName = "vas-pause-clock"
			};
			ResumeClockCommand = new Command () {
				ToolTipText = Strings.ResumeClock,
				IconName = "vas-pause-clock"
			};
			SaveCommand = new AsyncCommand () {
				ToolTipText = Strings.SaveProject,
				IconName = "vas-save"
			};
			CancelCommand = new AsyncCommand () {
				ToolTipText = Strings.CancelProject,
				IconName = "vas-cancel-rec"
			};
			DeleteLastEventCommand = new Command () {
				ToolTipText = Strings.DeleteEvent,
				IconName = "vas-delete"
			};
			PlayLastEventCommand = new Command () {
				ToolTipText = Strings.ReplayEvent,
				IconName = "vas-control-play"
			};
		}

		/// <summary>
		/// Starts the recording and create a new period.
		/// The first parameter is a boolean definning if a new period should be started.
		/// </summary>
		public Command<bool> StartRecordingCommand { get; }

		/// <summary>
		/// Stops the recording and create a new new Period
		/// </summary>
		public Command StopRecordingCommand { get; }

		/// <summary>
		/// Pauses the clock, the recording is not stopped.
		/// </summary>
		public Command PauseClockCommand { get; }

		/// <summary>
		/// Resume the clock.
		/// </summary>
		/// <value>The resume clock command.</value>
		public Command ResumeClockCommand { get; }

		/// <summary>
		/// Save the recording session.
		/// </summary>
		/// <value>The save command.</value>
		public Command SaveCommand { get; }

		/// <summary>
		/// Cancell the recording session.
		/// </summary>
		/// <value>The cancel command.</value>
		public Command CancelCommand { get; }

		/// <summary>
		/// Replay the last event.
		/// </summary>
		public Command PlayLastEventCommand { get; }

		/// <summary>
		/// Delete the last event 
		/// </summary>
		public Command DeleteLastEventCommand { get; }

		/// <summary>
		/// Gets or sets the video window to preview the recording.
		/// </summary>
		/// <value>The video window.</value>
		public IViewPort VideoWindow { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.VideoRecorderVM"/> recorder
		/// is ready to start recoirding.
		/// </summary>
		/// <value><c>true</c> if recorder is ready; otherwise, <c>false</c>.</value>
		public bool RecorderIsReady { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.VideoRecorderVM"/> is recording.
		/// </summary>
		/// <value><c>true</c> if recording; otherwise, <c>false</c>.</value>
		public bool Recording { get; set; }

		/// <summary>
		/// Gets or sets the current time, which the accumulated recording time.
		/// </summary>
		/// <value>The current time.</value>
		public Time CurrentTime { get; set; }

		/// <summary>
		/// Gets or sets the elapsed recording time from the beginning of the last period
		/// </summary>
		/// <value>The elapsed time.</value>
		public Time ElapsedTime { get; set; }

		/// <summary>
		/// Gets or sets the last created event in the timeline.
		/// </summary>
		/// <value>The last created event.</value>
		public TimelineEventVM LastCreatedEvent { get; set; }

		/// <summary>
		/// Gets or sets the capturer implementation.
		/// </summary>
		/// <value>The capturer.</value>
		public ICapturer Capturer { get; set; }

		/// <summary>
		/// Gets or sets the recording settings.
		/// </summary>
		/// <value>The settings.</value>
		public CaptureSettings Settings { get; set; }

		/// <summary>
		/// Gets or sets the output file where the recording is saved.
		/// </summary>
		/// <value>The output file.</value>
		public MediaFileVM OutputFile { get; set; }

		/// <summary>
		/// Gets or sets the name of the current period name.
		/// </summary>
		/// <value>The name of the period.</value>
		public string PeriodName { get; set; }

		/// <summary>
		/// Gets or sets the periods names.
		/// </summary>
		/// <value>The periods names.</value>
		public List<string> PeriodsNames { get; set; }

		/// <summary>
		/// Gets or sets the created periods.
		/// </summary>
		/// <value>The periods.</value>
		public List<Period> Periods { get; set; }

		/// <summary>
		/// Gets or sets the recorder mode.
		/// </summary>
		/// <value>The recorder mode.</value>
		public CapturerType RecorderMode { get; set; }
	}
}
