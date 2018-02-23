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
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Services.Service
{
	public class VideoRecoderService : DisposableBase, IVideoRecorderService
	{
		TimeNode currentTimeNode;
		Time accumTime;
		DateTime currentPeriodStart;
		ITimer timer;
		Period currentPeriod;
		Action delayedRun;
		bool delayStart;
		VideoRecorderVM viewModel;

		public VideoRecorderVM ViewModel {
			get {
				return viewModel;
			}

			set {
				if (viewModel != null) {
					viewModel.VideoWindow.ReadyEvent -= HandleVideoWindowReady;
					ViewModel.Capturer.Error -= HandleError;
					ViewModel.Capturer.MediaInfo -= HandleMediaInfo;
					ViewModel.Capturer.DeviceChange -= HandleDeviceChange;
					ViewModel.Capturer.ReadyToCapture -= HandleReadyToCapture;
					App.Current.EventsBroker.Unsubscribe<EventCreatedEvent> (HandleEventCreated);
				}
				viewModel = value;
				if (viewModel != null) {
					App.Current.EventsBroker.Subscribe<EventCreatedEvent> (HandleEventCreated);
					viewModel.VideoWindow.ReadyEvent += HandleVideoWindowReady;
					ViewModel.Capturer.Error += HandleError;
					ViewModel.Capturer.MediaInfo += HandleMediaInfo;
					ViewModel.Capturer.DeviceChange += HandleDeviceChange;
					ViewModel.Capturer.ReadyToCapture += HandleReadyToCapture;
				}
			}
		}

		public Image CurrentFrame {
			get {
				if (ViewModel.Capturer == null)
					return null;

				Image image = ViewModel.Capturer.CurrentFrame;

				if (image.Value == null)
					return null;
				image.ScaleInplace (Constants.MAX_THUMBNAIL_SIZE,
					Constants.MAX_THUMBNAIL_SIZE);
				return image;
			}
		}

		Time CurrentCaptureTime {
			get {
				int timeDiff;

				timeDiff = (int)(DateTime.UtcNow - currentPeriodStart).TotalMilliseconds;
				return new Time (accumTime.MSeconds + timeDiff);
			}
		}

		Time ElapsedTime {
			get {
				if (currentPeriod != null) {
					return currentPeriod.TotalTime;
				}
				return new Time (0);
			}
		}

		public void Run ()
		{
			if (!ViewModel.VideoWindow.Ready) {
				delayedRun = () => InternalRun ();
				return;
			}
			InternalRun ();
		}

		public void StartRecording (bool newPeriod)
		{
			if (!ViewModel.RecorderIsReady) {
				return;
			}

			if (currentPeriod != null) {
				string msg = Catalog.GetString ("Period recording already started");
				App.Current.Dialogs.WarningMessage (msg, this);
				return;
			}
			ViewModel.StartRecordingCommand.Executable = false;
			ViewModel.PauseClockCommand.Executable = ViewModel.SaveCommand.Executable = ViewModel.StopRecordingCommand.Executable = true;

			if (ViewModel.PeriodsNames != null && ViewModel.PeriodsNames.Count > ViewModel.Periods.Count) {
				ViewModel.PeriodName = ViewModel.PeriodsNames [ViewModel.Periods.Count];
			} else {
				ViewModel.PeriodName = (ViewModel.Periods.Count + 1).ToString ();
			}
			currentPeriod = new Period { Name = ViewModel.PeriodName };

			currentTimeNode = currentPeriod.Start (accumTime, ViewModel.PeriodName);
			currentTimeNode.Stop = currentTimeNode.Start;
			currentPeriodStart = DateTime.UtcNow;
			timer.Start ();

			if (ViewModel.Capturer != null) {
				if (ViewModel.Periods.Count == 0) {
					ViewModel.Capturer.Start ();
				} else {
					ViewModel.Capturer.TogglePause ();
				}
			}
			ViewModel.Recording = true;
			ViewModel.Periods.Add (currentPeriod);
			Log.Debug ("Start new period start=", currentTimeNode.Start.ToMSecondsString ());
		}

		public void StopRecording ()
		{
			if (currentPeriod == null) {
				string msg = Catalog.GetString ("Period recording not started");
				App.Current.Dialogs.WarningMessage (msg, this);
				return;
			}

			timer.Stop ();
			currentPeriod.Stop (CurrentCaptureTime);
			accumTime = CurrentCaptureTime;
			Log.Debug ("Stop period stop=", accumTime.ToMSecondsString ());
			currentTimeNode = null;
			currentPeriod = null;
			ViewModel.StartRecordingCommand.Executable = true;
			ViewModel.PauseClockCommand.Executable = ViewModel.SaveCommand.Executable = ViewModel.StopRecordingCommand.Executable = false;
			if (ViewModel.Capturer != null && ViewModel.Recording) {
				ViewModel.Capturer.TogglePause ();
			}
			ViewModel.Recording = false;
		}

		public void PauseClock ()
		{
			if (currentPeriod == null) {
				string msg = Catalog.GetString ("Period recording not started");
				App.Current.Dialogs.WarningMessage (msg, this);
				return;
			}
			Log.Debug ("Pause period at currentTime=", CurrentCaptureTime.ToMSecondsString ());
			currentPeriod.Stop (CurrentCaptureTime);
			currentTimeNode = null;
			ViewModel.PauseClockCommand.Executable = false;
			ViewModel.ResumeClockCommand.Executable = true;
			ViewModel.Recording = false;
		}

		public void ResumeClock ()
		{
			if (currentPeriod == null) {
				string msg = Catalog.GetString ("Period recording not started");
				App.Current.Dialogs.WarningMessage (msg, this);
				return;
			}
			Log.Debug ("Resume period at currentTime=", CurrentCaptureTime.ToMSecondsString ());
			currentTimeNode = currentPeriod.Start (CurrentCaptureTime);
			ViewModel.PauseClockCommand.Executable = true;
			ViewModel.ResumeClockCommand.Executable = false;
			ViewModel.Recording = true;
		}

		public async Task Save ()
		{
			string msg = Catalog.GetString ("Do you want to finish the current capture?");
			if (await App.Current.Dialogs.QuestionMessage (msg, null)) {
				await App.Current.EventsBroker.Publish (
					new CaptureFinishedEvent {
						Cancel = false,
						Reopen = true
					}
				);
			}
		}

		public async Task Cancel ()
		{
			string msg = Catalog.GetString ("Do you want to close and cancel the current capture?");
			if (await App.Current.Dialogs.QuestionMessage (msg, null)) {
				await App.Current.EventsBroker.Publish (
					new CaptureFinishedEvent {
						Cancel = true,
						Reopen = false
					}
				);
			}
		}

		bool UpdateTime ()
		{
			if (currentTimeNode != null) {
				currentTimeNode.Stop = CurrentCaptureTime;
			}

			ViewModel.ElapsedTime = ElapsedTime;
			ViewModel.CurrentTime = CurrentCaptureTime;

			App.Current.EventsBroker.Publish (
				new CapturerTickEvent {
					Time = CurrentCaptureTime
				}
			);
			return true;
		}

		void InternalRun ()
		{
			if (ViewModel.RecorderMode == CapturerType.Live) {
				ViewModel.RecorderIsReady = false;
				ViewModel.VideoWindow.Message = Catalog.GetString ("Loading");
				ViewModel.VideoWindow.MessageVisible = true;
				ViewModel.VideoWindow.Ratio = (float)ViewModel.OutputFile.VideoWidth / ViewModel.OutputFile.VideoHeight;
				ViewModel.Periods = new List<Period> ();
				if (ViewModel.VideoWindow.Ready) {
					Configure ();
				} else {
					delayStart = true;
				}
			} else {
				ViewModel.RecorderIsReady = true;
			}
		}

		public void Close ()
		{
			if (currentPeriod != null) {
				StopRecording ();
			}
			/* stopping and closing capturer */
			if (ViewModel.Capturer != null) {
				try {
					ViewModel.Capturer.Close ();
					ViewModel.Capturer.Error -= HandleError;
					ViewModel.Capturer.DeviceChange -= HandleDeviceChange;
					ViewModel.Capturer.Dispose ();
				} catch (Exception ex) {
					Log.Exception (ex);
				}
			}
			ViewModel.Capturer = null;
		}

		public void PlayLastEvent ()
		{
			if (ViewModel.LastCreatedEvent == null) {
				return;
			}
			App.Current.EventsBroker.Publish (
				new LoadTimelineEventEvent<TimelineEventVM> {
					Object = ViewModel.LastCreatedEvent,
					Playing = true,
				}
			);
		}

		public void DeleteLastEvent ()
		{
			if (ViewModel.LastCreatedEvent == null) {
				return;
			}
			App.Current.EventsBroker.Publish (
				new EventsDeletedEvent {
					TimelineEvents = new List<TimelineEventVM> { ViewModel.LastCreatedEvent }
				}
			);
			ViewModel.LastCreatedEvent = null;
		}

		void Configure ()
		{
			VideoMuxerType muxer;

			if (ViewModel.Capturer == null) {
				ViewModel.VideoWindow.MessageVisible = false;
				return;
			}

			/* We need to use Matroska for live replay and remux when the capture is done */
			muxer = ViewModel.Settings.EncodingSettings.EncodingProfile.Muxer;
			if (muxer == VideoMuxerType.Avi || muxer == VideoMuxerType.Mp4) {
				ViewModel.Settings.EncodingSettings.EncodingProfile.Muxer = VideoMuxerType.Matroska;
			}
			ViewModel.Capturer.Configure (ViewModel.Settings, ViewModel.VideoWindow.WindowHandle);
			ViewModel.Settings.EncodingSettings.EncodingProfile.Muxer = muxer;
			delayStart = false;
			ViewModel.Capturer.Run ();
		}

		void HandleReady (object sender, EventArgs e)
		{
			if (delayStart) {
				Configure ();
			}
		}

		void HandleExposeEvent ()
		{
			ViewModel.Capturer?.Expose ();
		}

		void HandleEventCreated (EventCreatedEvent e)
		{
			ViewModel.LastCreatedEvent = e.TimelineEvent;
		}

		void HandleError (object sender, string message)
		{
			App.Current.GUIToolkit.Invoke (delegate {
				App.Current.EventsBroker.Publish (
					new CaptureErrorEvent {
						Sender = sender,
						Message = message
					}
				);
			});
		}

		void HandleDeviceChange (int deviceID)
		{
			App.Current.GUIToolkit.Invoke (async delegate {
				string msg;
				/* device disconnected, pause capture */
				if (deviceID == -1) {
					PauseClock ();
					msg = Catalog.GetString ("Device disconnected. " + "The capture will be paused");
					App.Current.Dialogs.WarningMessage (msg);
				} else {
					msg = Catalog.GetString ("Device reconnected. " + "Do you want to restart the capture?");
					if (await App.Current.Dialogs.QuestionMessage (msg, null)) {
						ResumeClock ();
					}
				}
			});
		}

		void HandleMediaInfo (int width, int height, int parN, int parD)
		{
			App.Current.GUIToolkit.Invoke (delegate {
				ViewModel.VideoWindow.Ratio = (float)width / height * parN / parD;
				ViewModel.OutputFile.Model.VideoWidth = (uint)width;
				ViewModel.OutputFile.Model.VideoHeight = (uint)height;
				ViewModel.OutputFile.Model.Par = (float)parN / parD;
			});
		}

		void HandleReadyToCapture (object sender)
		{
			App.Current.GUIToolkit.Invoke (delegate {
				ViewModel.RecorderIsReady = true;
			});
		}

		void HandleVideoWindowReady (object sender, EventArgs args)
		{
			if (delayedRun != null) {
				delayedRun ();
				delayedRun = null;
			}
		}

	}
}
