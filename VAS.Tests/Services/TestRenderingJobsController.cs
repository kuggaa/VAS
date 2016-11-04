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
using System.Collections.ObjectModel;
using System.IO;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Drawing.Cairo;
using VAS.Services;
using VAS.Services.ViewModel;

namespace VAS.Tests.Services
{
	[TestFixture]
	public class TestRenderingJobsController
	{
		JobsManagerVM manager;
		RenderingJobsController controller;
		Mock<IVideoEditor> editorMock;
		string file1, file2;

		[TestFixtureSetUp]
		public void SetUpOnce ()
		{
			file1 = Path.GetTempFileName ();
			file2 = Path.GetTempFileName ();
		}

		[TestFixtureTearDown]
		public void TearDownOnce ()
		{
			try {
				File.Delete (file1);
				File.Delete (file2);
			} catch {
			}
			SetupClass.Initialize ();
		}

		[SetUp]
		public void Setup ()
		{
			editorMock = new Mock<IVideoEditor> ();

			var capturerMock = new Mock<IFramesCapturer> ();
			capturerMock.Setup (c =>
				c.GetFrame (It.IsAny<Time> (), It.IsAny<bool> (), It.IsAny<int> (), It.IsAny<int> ())).
			Returns (Utils.LoadImageFromFile ());

			// Mock IMultimediaToolkit
			var mtk = new Mock<IMultimediaToolkit> ();
			mtk.Setup (m => m.GetVideoEditor ()).Returns (editorMock.Object);
			mtk.Setup (m => m.GetFramesCapturer ()).Returns (capturerMock.Object);

			// and guitoolkit
			var gtk = Mock.Of<IGUIToolkit> ();

			// And eventbroker
			App.Current.GUIToolkit = gtk;
			App.Current.MultimediaToolkit = mtk.Object;
			App.Current.DrawingToolkit = new CairoBackend ();
			App.Current.EventsBroker = new EventsBroker ();

			manager = new JobsManagerVM { Model = new ObservableCollection<Job> () };
			controller = new RenderingJobsController (manager);
			controller.Start ();
		}

		[TearDown]
		public void TearDown ()
		{
			controller.Stop ();
		}

		[Test ()]
		public void TestRenderedCamera ()
		{
			Project p = Utils.CreateProject ();

			try {
				EditionJob job;

				job = PrepareEditon ();

				TimelineEvent evt = p.Timeline [0] as TimelineEvent;
				evt.CamerasConfig = new ObservableCollection<CameraConfig> { new CameraConfig (0) };
				PlaylistPlayElement element = new PlaylistPlayElement (evt);
				job.Playlist.Elements.Add (element);

				manager.Add (job);

				// Check that AddSegment is called with the right video file.
				editorMock.Verify (m => m.AddSegment (p.FileSet [0].FilePath,
					evt.Start.MSeconds, evt.Stop.MSeconds, evt.Rate, evt.Name, true, new Area ()), Times.Once ());

				/* Test with a camera index bigger than the total cameras */
				manager.CancelAll ();
				editorMock.ResetCalls ();
				evt = p.Timeline [1] as TimelineEvent;
				evt.CamerasConfig = new ObservableCollection<CameraConfig> { new CameraConfig (1) };
				element = new PlaylistPlayElement (evt);
				job.Playlist.Elements [0] = element;
				job.State = JobState.Pending;
				manager.Add (job);
				editorMock.Verify (m => m.AddSegment (p.FileSet [1].FilePath,
					evt.Start.MSeconds, evt.Stop.MSeconds, evt.Rate, evt.Name, true, new Area ()), Times.Once ());

				/* Test with the secondary camera */
				manager.CancelAll ();
				editorMock.ResetCalls ();
				evt = p.Timeline [1] as TimelineEvent;
				evt.CamerasConfig = new ObservableCollection<CameraConfig> { new CameraConfig (2) };
				element = new PlaylistPlayElement (evt);
				job.Playlist.Elements [0] = element;
				job.State = JobState.Pending;
				manager.Add (job);
				editorMock.Verify (m => m.AddSegment (p.FileSet [0].FilePath,
					evt.Start.MSeconds, evt.Stop.MSeconds, evt.Rate, evt.Name, true, new Area ()), Times.Once ());
			} finally {
				Utils.DeleteProject (p);
			}
		}

		[Test ()]
		public void TestLoadEditionJob ()
		{
			EditionJob job;

			job = PrepareEditon ();

			AddTimelineEvent (job.Playlist, 30000, 31000, file1);
			AddTimelineEvent (job.Playlist, 10000, 15000, file2);
			AddTimelineEvent (job.Playlist, 43000, 46000, file2);
			AddTimelineEvent (job.Playlist, 0, 0, "Does not exists");
			AddTimelineEvent (job.Playlist, 86000, 90000, file1);
			AddTimelineEvent (job.Playlist, 0, 0, "Does not exists");

			manager.Add (job);

			editorMock.Verify (m => m.AddSegment (file1, 30000, 1000, 1, null, false, new Area ()));
			editorMock.Verify (m => m.AddSegment (file2, 10000, 5000, 1, null, false, new Area ()));
			editorMock.Verify (m => m.AddSegment (file2, 43000, 3000, 1, null, false, new Area ()));
			editorMock.Verify (m => m.AddSegment (file1, 86000, 4000, 1, null, false, new Area ()));
		}

		[Test ()]
		public void TestLoadEditionJobWithDrawingsInEvents ()
		{
			EditionJob job;
			TimelineEvent evt;

			job = PrepareEditon ();
			AddTimelineEvent (job.Playlist, 0, 10000, file1);

			evt = (job.Playlist.Elements [0] as PlaylistPlayElement).Play;
			evt.Drawings.Add (new FrameDrawing { Render = new Time (5000) });
			evt.Drawings.Add (new FrameDrawing { Render = new Time (2000) });
			evt.Drawings.Add (new FrameDrawing { Render = new Time (8000) });

			manager.Add (job);

			editorMock.Verify (m => m.AddSegment (file1, 0, 2000, 1, null, false, new Area ()));
			editorMock.Verify (m => m.AddImageSegment (It.IsAny<string> (), 0, 5000, null, new Area ()));
			editorMock.Verify (m => m.AddSegment (file1, 2000, 3000, 1, null, false, new Area ()));
			editorMock.Verify (m => m.AddImageSegment (It.IsAny<string> (), 0, 5000, null, new Area ()));
			editorMock.Verify (m => m.AddSegment (file1, 5000, 3000, 1, null, false, new Area ()));
			editorMock.Verify (m => m.AddImageSegment (It.IsAny<string> (), 0, 5000, null, new Area ()));
			editorMock.Verify (m => m.AddSegment (file1, 8000, 2000, 1, null, false, new Area ()));
		}

		EditionJob PrepareEditon ()
		{
			var playlist = new Playlist ();

			const string outputFile = "path";
			EncodingSettings settings = new EncodingSettings (VideoStandards.P720, EncodingProfiles.MP4,
											EncodingQualities.Medium, 25, 1, outputFile, true, true, 20);

			return new EditionJob (playlist, settings);
		}

		void AddTimelineEvent (Playlist playlist, int start, int stop, string file)
		{
			MediaFileSet fileset = new MediaFileSet ();
			fileset.Add (new MediaFile { FilePath = file });
			TimelineEvent evt = new TimelineEvent {
				Start = new Time (start),
				Stop = new Time (stop),
				FileSet = fileset,
			};
			playlist.Elements.Add (new PlaylistPlayElement (evt));
		}
	}
}
