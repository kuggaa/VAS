//
//  Copyright (C) 2016 Fluendo S.A.
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
using System.Linq;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Services.ViewModel;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;

namespace VAS.Services
{
	public class RenderingJobsController : ControllerBase, IService
	{
		IVideoEditor videoEditor;
		IFramesCapturer capturer;
		ControllerStatus status = ControllerStatus.Stopped;

		public RenderingJobsController (JobsManagerVM viewModel)
		{
			ViewModel = viewModel;
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			CancelCurrentJob (false);
		}

		public JobsManagerVM ViewModel {
			get;
			set;
		}

		public int Level {
			get {
				return 0;
			}
		}

		public string Name {
			get {
				return "Jobs Manager";
			}
		}

		public override void SetViewModel (IViewModel viewModel)
		{
			ViewModel = viewModel as JobsManagerVM;
		}

		public override IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

		public override void Start ()
		{
			base.Start ();
			App.Current.EventsBroker.Subscribe<CreateEvent<Job>> (HandleAddJob);
			App.Current.EventsBroker.Subscribe<RetryEvent<IEnumerable<Job>>> (HandleRetryJobs);
			App.Current.EventsBroker.Subscribe<CancelEvent<IEnumerable<Job>>> (HandleCancelJobs);
			App.Current.EventsBroker.Subscribe<ClearEvent<Job>> (HandleClearFinishedJobs);
			status = ControllerStatus.Started;
		}

		public override void Stop ()
		{
			base.Stop ();
			App.Current.EventsBroker.Unsubscribe<CreateEvent<Job>> (HandleAddJob);
			App.Current.EventsBroker.Unsubscribe<RetryEvent<IEnumerable<Job>>> (HandleRetryJobs);
			App.Current.EventsBroker.Unsubscribe<CancelEvent<IEnumerable<Job>>> (HandleCancelJobs);
			App.Current.EventsBroker.Unsubscribe<ClearEvent<Job>> (HandleClearFinishedJobs);
			status = ControllerStatus.Stopped;
		}

		bool IService.Start ()
		{
			Start ();
			return true;
		}

		bool IService.Stop ()
		{
			Stop ();
			return true;
		}

		public void HandleAddJob (CreateEvent<Job> evt)
		{
			Job job = evt.Object;
			if (job == null) {
				return;
			}

			job.State = JobState.Pending;
			ViewModel.Model.Add (job);
			evt.ReturnValue = true;
			if (ViewModel.CurrentJob.Model == null) {
				StartNextJob ();
			}
		}

		public void HandleRetryJobs (RetryEvent<IEnumerable<Job>> evt)
		{
			IEnumerable<Job> jobs = evt.Object;
			evt.ReturnValue = false;

			foreach (Job job in ViewModel.Model.Intersect (jobs)) {
				// Remove jobs from the list and add them back to the queue
				ViewModel.Model.Remove (job);
				job.State = JobState.Pending;
				HandleAddJob (new CreateEvent<Job> { Object = job });
				evt.ReturnValue = true;
			}
		}

		public void HandleCancelJobs (CancelEvent<IEnumerable<Job>> evt)
		{
			IEnumerable<Job> jobs = evt.Object;

			if (jobs == null || !jobs.Any ()) {
				CancelCurrentJob ();
				evt.ReturnValue = true;
			} else {
				foreach (Job job in jobs) {
					if (job == ViewModel.CurrentJob.Model) {
						CancelCurrentJob ();
					} else {
						job.State = JobState.Cancelled;
					}
				}
				evt.ReturnValue = true;
			}
		}

		public void HandleClearFinishedJobs (ClearEvent<Job> evt)
		{
			ViewModel.ViewModels.RemoveAll (j => j.State == JobState.Finished);
			evt.ReturnValue = true;
		}

		void CleanVideoEditor (bool cancel = false)
		{
			if (videoEditor == null) {
				return;
			}

			videoEditor.Progress -= OnProgress;
			videoEditor.Error -= OnError;
			if (cancel) {
				videoEditor.Cancel ();
			}
			videoEditor = null;
		}

		void CancelCurrentJob (bool startNext = true)
		{
			CleanVideoEditor (true);
			ViewModel.CurrentJob.Progress = 0;
			ViewModel.CurrentJob.State = JobState.Cancelled;
			if (startNext) {
				StartNextJob ();
			}
		}

		void LoadConversionJob (ConversionJob job)
		{
			videoEditor = App.Current.MultimediaToolkit.GetVideoEditor ();
			videoEditor.EncodingSettings = job.EncodingSettings;
			videoEditor.Progress += OnProgress;
			videoEditor.Error += OnError;

			foreach (MediaFile file in job.InputFiles) {
				PlaylistVideo video = new PlaylistVideo (file);
				Log.Debug ("Convert video " + video.File.FilePath);
				videoEditor.AddSegment (video.File.FilePath, 0, -1, 1, "", video.File.HasAudio, new Area ());
			}

			videoEditor.Start ();
		}

		void LoadEditionJob (EditionJob job)
		{
			videoEditor = App.Current.MultimediaToolkit.GetVideoEditor ();
			videoEditor.EncodingSettings = job.EncodingSettings;
			videoEditor.Progress += OnProgress;
			videoEditor.Error += OnError;

			foreach (IPlaylistElement segment in job.Playlist.Elements) {
				if (segment is PlaylistPlayElement) {
					ProcessPlay (segment as PlaylistPlayElement);
				} else if (segment is PlaylistVideo) {
					ProcessVideo (segment as PlaylistVideo);
				} else if (segment is PlaylistImage) {
					ProcessImage (segment as PlaylistImage);
				} else if (segment is PlaylistDrawing) {
					ProcessDrawing (segment as PlaylistDrawing);
				}
			}
			videoEditor.Start ();
		}

		void ProcessImage (Image image, Time duration)
		{
			string path = System.IO.Path.GetTempFileName ().Replace (@"\", @"\\");
			image.Save (path);
			videoEditor.AddImageSegment (path, 0, duration.MSeconds, "", new Area ());
		}

		void ProcessImage (PlaylistImage image)
		{
			Log.Debug (String.Format ("Adding still image with duration {0}s",
				image.Duration));
			ProcessImage (image.Image, image.Duration);
		}

		void ProcessVideo (PlaylistVideo video)
		{
			Log.Debug ("Adding external video " + video.File.FilePath);
			videoEditor.AddSegment (video.File.FilePath, 0, video.File.Duration.MSeconds,
				1, "", video.File.HasAudio, new Area ());
		}

		void ProcessDrawing (PlaylistDrawing drawing)
		{
			Image img;

			Log.Debug (String.Format ("Adding still drawing with duration {0}s",
				drawing.Duration));
			img = VAS.Drawing.Utils.RenderFrameDrawing (App.Current.DrawingToolkit, drawing.Width,
				drawing.Height, drawing.Drawing);
			ProcessImage (img, drawing.Duration);
		}

		bool ProcessPlay (PlaylistPlayElement element)
		{
			Time lastTS;
			TimelineEvent play;
			MediaFile file;
			IEnumerable<FrameDrawing> drawings;
			int cameraIndex;
			Area roi;

			play = element.Play;
			Log.Debug (String.Format ("Adding segment {0}", element));

			lastTS = play.Start;
			if (element.CamerasConfig.Count == 0) {
				cameraIndex = 0;
				roi = new Area ();
			} else {
				cameraIndex = element.CamerasConfig [0].Index;
				roi = element.CamerasConfig [0].RegionOfInterest;
			}
			if (cameraIndex >= element.Play.FileSet.Count) {
				Log.Error (string.Format ("Camera index={0} not matching for current fileset count={1}",
					cameraIndex, element.Play.FileSet.Count));
				file = element.Play.FileSet [0];
			} else {
				file = element.Play.FileSet [cameraIndex];
			}
			drawings = play.Drawings.Where (d => d.CameraConfig.Index == cameraIndex).OrderBy (d => d.Render.MSeconds);

			if (file == null || drawings == null) {
				return false;
			}
			if (!file.Exists ()) {
				return false;
			}
			foreach (FrameDrawing fd in drawings) {
				if (fd.Render < play.Start || fd.Render > play.Stop) {
					Log.Warning ("Drawing is not in the segments boundaries " +
					fd.Render.ToMSecondsString ());
					continue;
				}
				string image_path = CreateStillImage (file, fd);
				if (image_path == null) {
					continue;
				}
				videoEditor.AddSegment (file.FilePath, (lastTS + file.Offset).MSeconds,
										fd.Render.MSeconds - lastTS.MSeconds,
										element.Rate, play.Name, file.HasAudio, roi);
				// Drawings have already been cropped to ROI by the canvas, we pass an empty area
				videoEditor.AddImageSegment (image_path, 0, fd.Pause.MSeconds, play.Name, new Area ());
				lastTS = fd.Render;
			}
			videoEditor.AddSegment (file.FilePath, (lastTS + file.Offset).MSeconds,
				play.Stop.MSeconds - lastTS.MSeconds,
				element.Rate, play.Name, file.HasAudio, roi);
			return true;
		}

		string CreateStillImage (MediaFile file, FrameDrawing drawing)
		{
			Image frame, final_image;
			string path = System.IO.Path.GetTempFileName ().Replace (@"\", @"\\");

			capturer = App.Current.MultimediaToolkit.GetFramesCapturer ();
			capturer.Open (file.FilePath);
			frame = capturer.GetFrame (drawing.Render + file.Offset, true, (int)file.DisplayVideoWidth, (int)file.DisplayVideoHeight);
			capturer.Dispose ();
			if (frame == null) {
				Log.Error (String.Format ("Could not get frame for file {0} at pos {1}",
										  file.FilePath, drawing.Render.ToMSecondsString ()));
				return null;
			}
			final_image = VAS.Drawing.Utils.RenderFrameDrawingToImage (App.Current.DrawingToolkit, frame, drawing);
			final_image.Save (path);
			return path;
		}

		void StartNextJob ()
		{
			CleanVideoEditor ();
			JobVM nextJob = ViewModel.PendingJobs.FirstOrDefault ();
			if (nextJob == null) {
				ViewModel.CurrentJob.Model = null;
				return;
			}
			ViewModel.CurrentJob.Model = nextJob.Model;
			ViewModel.CurrentJob.Progress = (float)EditorState.START;
			ViewModel.CurrentJob.State = JobState.Running;

			try {
				if (ViewModel.CurrentJob.Model is EditionJob) {
					LoadEditionJob (ViewModel.CurrentJob.Model as EditionJob);
				} else {
					LoadConversionJob (ViewModel.CurrentJob.Model as ConversionJob);
				}
			} catch (Exception ex) {
				ViewModel.CurrentJob.State = JobState.Error;
				Log.Exception (ex);
				Log.Error ("Error rendering job: ", ViewModel.CurrentJob.Name);
				App.Current.Dialogs.ErrorMessage (Catalog.GetString ("Error rendering job: ") + ex.Message);
				StartNextJob ();
			}
		}

		void OnError (object sender, string message)
		{
			Log.Debug ("Job finished with errors: " + message);
			App.Current.Dialogs.ErrorMessage (Catalog.GetString ("An error has occurred in the video editor.")
			+ Catalog.GetString ("Please, try again."));
			ViewModel.CurrentJob.State = JobState.Error;
			StartNextJob ();
		}

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
		void OnProgress (float progress)
		{
			if (progress < (float)EditorState.START || progress > (float)EditorState.FINISHED) {
				Log.Error ("Progress should have values between 0 and 1: " + progress);
			} else if (progress == (float)EditorState.START) {
				if (ViewModel.CurrentJob.State != JobState.Running) {
					Log.Debug ("Job started");
				}
				ViewModel.CurrentJob.State = JobState.Running;
				ViewModel.CurrentJob.Progress = progress;
			} else if (progress == (float)EditorState.FINISHED) {
				Log.Debug ("Job finished successfully");
				videoEditor.Progress -= OnProgress;
				ViewModel.CurrentJob.Progress = progress;
				ViewModel.CurrentJob.State = JobState.Finished;
				App.Current.EventsBroker.Publish (new JobRenderedEvent ());
				StartNextJob ();
			} else {
				if (progress > ViewModel.CurrentJob.Progress) {
					ViewModel.CurrentJob.Progress = progress;
				}
			}
		}
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
	}
}
