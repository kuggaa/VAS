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
using VAS.Core.Services;
using VAS.Core.Services.ViewModel;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;

namespace VAS.Services
{
	public class RenderingJobsController : IRenderingJobsControler, IController
	{
		/* List of pending jobs */
		JobCollectionVM jobs, pendingJobs;
		IVideoEditor videoEditor;
		IFramesCapturer capturer;
		JobVM currentJob;
		ControllerStatus status = ControllerStatus.Stopped;

		public bool Disposed { get; private set; } = false;

		public JobCollectionVM Jobs {
			get {
				return jobs;
			}
		}

		public JobCollectionVM PendingJobs {
			get {
				return pendingJobs;
			}
		}

		public RenderingJobsController ()
		{
			jobs = new JobCollectionVM ();
			pendingJobs = new JobCollectionVM ();
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		public virtual void Dispose (bool disposing)
		{
			if (Disposed)
				return;

			if (disposing) {
			}

			Disposed = true;
		}

		public void AddJob (JobVM job)
		{
			if (job == null)
				return;
			jobs.Model.Add (job.Model);
			pendingJobs.Model.Add (job.Model);
			UpdateJobsStatus ();
			if (pendingJobs.Count () == 1)
				StartNextJob ();
		}

		public void RetryJobs (JobCollectionVM retryJobs)
		{
			foreach (Job job in retryJobs.Model) {
				if (!jobs.Model.Contains (job))
					return;
				if (!pendingJobs.Model.Contains (job)) {
					job.State = JobState.NotStarted;
					jobs.Model.Remove (job);
					jobs.Model.Add (job);
					pendingJobs.Model.Add (job);
					UpdateJobsStatus ();
				}
			}
		}

		public void DeleteJob (JobVM job)
		{
			job.State = JobState.Cancelled;
			CancelJob (job);
		}

		public void ClearDoneJobs ()
		{
			jobs.Model.RemoveAll (j => j.State == JobState.Finished);
		}

		public void CancelJobs (JobCollectionVM cancelJobs)
		{
			foreach (JobVM job in cancelJobs) {
				job.State = JobState.Cancelled;
				pendingJobs.Model.Remove (job.Model);
			}

			if (cancelJobs.Contains (currentJob))
				CancelCurrentJob ();
		}

		public void CancelCurrentJob ()
		{
			CancelJob (currentJob);
		}

		public void CancelJob (JobVM job)
		{
			if (currentJob != job)
				return;

			videoEditor.Progress -= OnProgress;
			videoEditor.Error -= OnError;
			videoEditor.Cancel ();
			job.State = JobState.Cancelled;
			RemoveCurrentFromPending ();
			UpdateJobsStatus ();
			StartNextJob ();
		}

		public void CancelAllJobs ()
		{
			foreach (JobVM job in pendingJobs)
				job.State = JobState.Cancelled;
			pendingJobs.Model.Clear ();
			CancelJob (currentJob);
		}

		protected void ManageJobs ()
		{
			App.Current.GUIToolkit.ManageJobs ();
		}

		private void LoadConversionJob (ConversionJob job)
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

			try {
				videoEditor.Start ();
			} catch (Exception ex) {
				videoEditor.Cancel ();
				job.State = JobState.Error;
				Log.Exception (ex);
				Log.Error ("Error rendering job: ", job.Name);
				App.Current.Dialogs.ErrorMessage (Catalog.GetString ("Error rendering job: ") + ex.Message);
			}
		}

		private void LoadEditionJob (EditionJob job)
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

			try {
				videoEditor.Start ();
			} catch (Exception ex) {
				videoEditor.Cancel ();
				job.State = JobState.Error;
				Log.Exception (ex);
				Log.Error ("Error rendering job: ", job.Name);
				App.Current.Dialogs.ErrorMessage (Catalog.GetString ("Error rendering job: ") + ex.Message);
			}
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
				videoEditor.AddSegment (file.FilePath, lastTS.MSeconds,
					fd.Render.MSeconds - lastTS.MSeconds,
					element.Rate, play.Name, file.HasAudio, roi);
				// Drawings have already been cropped to ROI by the canvas, we pass an empty area
				videoEditor.AddImageSegment (image_path, 0, fd.Pause.MSeconds, play.Name, new Area ());
				lastTS = fd.Render;
			}
			videoEditor.AddSegment (file.FilePath, lastTS.MSeconds,
				play.Stop.MSeconds - lastTS.MSeconds,
				element.Rate, play.Name, file.HasAudio, roi);
			return true;
		}

		private string CreateStillImage (MediaFile file, FrameDrawing drawing)
		{
			Image frame, final_image;
			string path = System.IO.Path.GetTempFileName ().Replace (@"\", @"\\");

			capturer = App.Current.MultimediaToolkit.GetFramesCapturer ();
			capturer.Open (file.FilePath);
			frame = capturer.GetFrame (drawing.Render, true, (int)file.DisplayVideoWidth, (int)file.DisplayVideoHeight);
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

		private void CloseAndNext ()
		{
			RemoveCurrentFromPending ();
			UpdateJobsStatus ();
			StartNextJob ();
		}

		private void ResetGui ()
		{
			App.Current.GUIToolkit.RenderingStateBar.ProgressText = "";
			App.Current.GUIToolkit.RenderingStateBar.JobRunning = false;
		}

		private void StartNextJob ()
		{
			if (pendingJobs.Count () == 0) {
				ResetGui ();
				return;
			}

			currentJob = pendingJobs.ElementAt (0);
			if (currentJob.Model is EditionJob) {
				LoadEditionJob (currentJob.Model as EditionJob);
			} else {
				LoadConversionJob (currentJob.Model as ConversionJob);
			}
		}

		private void UpdateProgress (float progress)
		{
			App.Current.GUIToolkit.RenderingStateBar.Fraction = progress;
			App.Current.GUIToolkit.RenderingStateBar.ProgressText = String.Format ("{0}... {1:0.0}%",
				Catalog.GetString ("Rendering"), progress * 100);
		}

		private void UpdateJobsStatus ()
		{
			App.Current.GUIToolkit.RenderingStateBar.Text = String.Format ("{0} ({1} {2})",
				Catalog.GetString ("Rendering queue"),
			   pendingJobs.Count (), Catalog.GetString ("Pending"));
		}

		private void RemoveCurrentFromPending ()
		{
			try {
				pendingJobs.Model.Remove (currentJob.Model);
			} catch {
			}
		}

		void HandleError ()
		{
			Log.Debug ("Job finished with errors");
			App.Current.Dialogs.ErrorMessage (Catalog.GetString ("An error has occurred in the video editor.")
			+ Catalog.GetString ("Please, try again."));
			currentJob.State = JobState.Error;
			CloseAndNext ();
		}

		private void MainLoopOnProgress (float progress)
		{
			if (progress > (float)EditorState.START && progress <= (float)EditorState.FINISHED
				&& progress > App.Current.GUIToolkit.RenderingStateBar.Fraction) {
				UpdateProgress (progress);
			}

			if (progress == (float)EditorState.CANCELED) {
				Log.Debug ("Job was cancelled");
				currentJob.State = JobState.Cancelled;
				CloseAndNext ();
			} else if (progress == (float)EditorState.START) {
				if (currentJob.State != JobState.Running) {
					Log.Debug ("Job started");
				}
				currentJob.State = JobState.Running;
				App.Current.GUIToolkit.RenderingStateBar.JobRunning = true;
				UpdateProgress (progress);
			} else if (progress == (float)EditorState.FINISHED) {
				Log.Debug ("Job finished successfully");
				videoEditor.Progress -= OnProgress;
				UpdateProgress (progress);
				currentJob.State = JobState.Finished;
				CloseAndNext ();
			} else if (progress == (float)EditorState.ERROR) {
				HandleError ();
			}
		}

		protected void OnError (object sender, string message)
		{
			HandleError ();
		}

		protected void OnProgress (float progress)
		{
			MainLoopOnProgress (progress);
		}

		public void Start ()
		{
			if (status == ControllerStatus.Started) {
				return;
			}
			App.Current.EventsBroker.Subscribe<ClearDoneJobsEvent> (HandleClearDoneJobsEvent);
			App.Current.EventsBroker.Subscribe<RetrySelectedJobsEvent> (HandleRetrySelectedJobsEvent);
			App.Current.EventsBroker.Subscribe<CancelSelectedJobsEvent> (HandleCancelSelectedJobsEvent);
			App.Current.EventsBroker.Subscribe<ConvertVideoFilesEvent> ((e) => {
				ConversionJob job = new ConversionJob (e.Files, e.Settings);
				AddJob (new JobVM { Model = job });
			});
			status = ControllerStatus.Started;
		}

		public void Stop ()
		{
			if (status == ControllerStatus.Stopped) {
				return;
			}
			App.Current.EventsBroker.Unsubscribe<ClearDoneJobsEvent> (HandleClearDoneJobsEvent);
			App.Current.EventsBroker.Unsubscribe<RetrySelectedJobsEvent> (HandleRetrySelectedJobsEvent);
			App.Current.EventsBroker.Unsubscribe<CancelSelectedJobsEvent> (HandleCancelSelectedJobsEvent);
			status = ControllerStatus.Stopped;
		}

		public void SetViewModel (IViewModel viewModel)
		{
			throw new NotImplementedException ();
		}

		public IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

		protected void HandleClearDoneJobsEvent (ClearDoneJobsEvent e)
		{
			ClearDoneJobs ();
		}

		protected void HandleRetrySelectedJobsEvent (RetrySelectedJobsEvent e)
		{
			RetryJobs (e.Jobs);
		}

		protected void HandleCancelSelectedJobsEvent (CancelSelectedJobsEvent e)
		{
			CancelJobs (e.Jobs);
		}
	}
}
