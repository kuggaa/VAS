//
//  Copyright (C) 2015 Fluendo S.A.
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
using System.Collections.ObjectModel;
using System.Timers;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;
using VAS.Services;
using VAS.Services.Controller;

namespace VAS.Tests.Services
{
	[TestFixture ()]
	public class TestVideoPlayerController
	{
		Mock<IVideoPlayer> playerMock;
		Mock<IViewPort> viewPortMock;
		Mock<IMultimediaToolkit> mtkMock;
		MediaFileSet mfs;
		VideoPlayerController player;
		Time currentTime, streamLength;
		TimelineEventVM eventVM1;
		TimelineEventVM eventVM2;
		TimelineEventVM eventVM3;
		PlaylistImageVM plImage;
		PlaylistVM playlistVM;
		PlaylistController plController;
		VideoPlayerVM playerVM;
		Mock<IFileSystemManager> fileManager;
		Mock<ILicenseLimitationsService> mockLimitationService;
		Mock<ITimer> timerMock;

		int elementLoaded;

		[OneTimeSetUp]
		public void FixtureSetup ()
		{
			fileManager = new Mock<IFileSystemManager> ();
			App.Current.FileSystemManager = fileManager.Object;


			var ftk = new Mock<IGUIToolkit> ();
			ftk.Setup (m => m.Invoke (It.IsAny<EventHandler> ())).Callback<EventHandler> (e => e (null, null));
			ftk.SetupGet (o => o.DeviceScaleFactor).Returns (1.0f);
			App.Current.GUIToolkit = ftk.Object;

			App.Current.LowerRate = 1;
			App.Current.UpperRate = 30;
			App.Current.RatePageIncrement = 3;
			App.Current.RateList = new List<double> { 0.04, 0.08, 0.12, 0.16, 0.20, 0.24, 0.28, 0.32, 0.36, 0.40, 0.44,
				0.48, 0.52, 0.56, 0.60, 0.64, 0.68, 0.72, 0.76, 0.80, 0.84, 0.88, 0.92, 0.96, 1, 2, 3, 4, 5
			};
			App.Current.DefaultRate = 25;
		}

		[SetUp ()]
		public void Setup ()
		{
			timerMock = new Mock<ITimer> ();
			playerMock = new Mock<IVideoPlayer> ();
			playerMock.SetupAllProperties ();
			/* Mock properties without setter */
			playerMock.Setup (p => p.CurrentTime).Returns (() => currentTime);
			playerMock.Setup (p => p.StreamLength).Returns (() => streamLength);
			playerMock.Setup (p => p.Play (It.IsAny<bool> ())).Raises (p => p.StateChange += null,
				new PlaybackStateChangedEvent {
					Playing = true
				}
			);
			playerMock.Setup (p => p.Pause (It.IsAny<bool> ())).Raises (p => p.StateChange += null,
				new PlaybackStateChangedEvent {
					Playing = false
				}
			);

			mtkMock = new Mock<IMultimediaToolkit> ();
			mtkMock.Setup (m => m.GetPlayer ()).Returns (playerMock.Object);
			App.Current.MultimediaToolkit = mtkMock.Object;
			mfs = new MediaFileSet ();
			mfs.Add (new MediaFile {
				FilePath = "test1",
				VideoWidth = 320,
				VideoHeight = 240,
				Par = 1,
				Duration = new Time { TotalSeconds = 5000 }
			});
			mfs.Add (new MediaFile {
				FilePath = "test2",
				VideoWidth = 320,
				VideoHeight = 240,
				Par = 1,
				Duration = new Time { TotalSeconds = 5000 }
			});

			mockLimitationService = new Mock<ILicenseLimitationsService> ();
			mockLimitationService.Setup (x => x.CanExecute (It.IsAny<string> ())).
								 Returns (true);
			App.Current.LicenseLimitationsService = mockLimitationService.Object;

			mtkMock.Setup (m => m.GetMultiPlayer ()).Throws (new Exception ());
			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (false);

			eventVM1 = new TimelineEventVM () {
				Model = new TimelineEvent {
					Start = new Time (100),
					Stop = new Time (200),
					CamerasConfig = new RangeObservableCollection<CameraConfig> { new CameraConfig (0) },
					FileSet = mfs
				}
			};
			eventVM2 = new TimelineEventVM () {
				Model = new TimelineEvent {
					Start = new Time (1000),
					Stop = new Time (20000),
					CamerasConfig = new RangeObservableCollection<CameraConfig> { new CameraConfig (0) },
					FileSet = mfs
				}
			};
			eventVM3 = new TimelineEventVM () {
				Model = new TimelineEvent {
					Start = new Time (100),
					Stop = new Time (200),
					CamerasConfig = new RangeObservableCollection<CameraConfig> (),
					FileSet = mfs
				}
			};
			plImage = new PlaylistImageVM { Model = new PlaylistImage (Utils.LoadImageFromFile (), new Time (5000)) };
			Playlist playlist = new Playlist ();
			playlist.Elements.Add (new PlaylistPlayElement (eventVM1.Model));
			playlist.Elements.Add (plImage.Model);
			currentTime = new Time (0);

			player = new VideoPlayerController (new InstantSeeker (), timerMock.Object);
			playerVM = new VideoPlayerVM ();
			player.SetViewModel (playerVM);

			playlistVM = new PlaylistVM { Model = playlist };
			playlistVM.SetActive (new PlaylistPlayElementVM { Model = playlist.Elements [0] as PlaylistPlayElement });

			plController = new PlaylistController ();
			plController.SetViewModel (new DummyPlaylistsManagerVM {
				Player = playerVM
			});
			plController.Start ();

			streamLength = new Time { TotalSeconds = 5000 };

			elementLoaded = 0;
			playerMock.ResetCalls ();
		}

		[TearDown ()]
		public void TearDown ()
		{
			player.Dispose ();
			plController.Dispose ();
		}

		void PreparePlayer (bool readyToSeek = true)
		{
			player.CamerasConfig = new RangeObservableCollection<CameraConfig> {
					new CameraConfig (0),
					new CameraConfig (1)
				};
			viewPortMock = new Mock<IViewPort> ();
			viewPortMock.SetupAllProperties ();
			player.ViewPorts = new List<IViewPort> { viewPortMock.Object, viewPortMock.Object };
			player.Ready (true);
			player.Open (mfs);
			if (readyToSeek) {
				playerMock.Raise (p => p.ReadyToSeek += null, this);
			}
		}

		[Test ()]
		public void TestPropertiesProxy ()
		{
			player.Volume = 10;
			Assert.AreEqual (10, player.Volume);

			currentTime = new Time (20);
			Assert.AreEqual (20, player.CurrentTime.MSeconds);

			streamLength = new Time (40);
			Assert.AreEqual (40, player.StreamLength.MSeconds);
		}

		[Test ()]
		public void TestSetRate ()
		{
			float r = 0;

			player.PlaybackRateChangedEvent += (rate) => r = 10;
			player.Rate = 1;
			/* Event is raised */
			Assert.AreEqual (10, r);
			Assert.AreEqual (1, player.Rate);
		}

		[Test ()]
		public void TestSetRateWithLoadedEvent ()
		{
			float r = 0;
			double expected = 2;

			PreparePlayer ();
			player.LoadEvent (eventVM1, currentTime, true);
			player.PlaybackRateChangedEvent += (rate) => r = 10;

			player.Rate = expected; // Event is raised

			Assert.AreEqual (10, r, "Fails because PlaybackRateChangedEvent is not called");
			Assert.AreEqual (expected, player.Rate, "Fails because player has an incorrect rate");
			Assert.AreEqual (expected, eventVM1.Model.Rate, "Fails because event has an incorrect rate");
		}

		[Test ()]
		public void TestCurrentMiniatureFrame ()
		{
			var img = player.CurrentMiniatureFrame;
			playerMock.Verify (p => p.GetCurrentFrame (Constants.MAX_THUMBNAIL_SIZE,
				Constants.MAX_THUMBNAIL_SIZE));
		}

		[Test ()]
		public void TestCurrentFrame ()
		{
			var img = player.CurrentFrame;
			playerMock.Verify (p => p.GetCurrentFrame (-1, -1));
		}

		[Test ()]
		public void TestOpenFileSet ()
		{
			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (true);

			viewPortMock = new Mock<IViewPort> ();
			viewPortMock.SetupAllProperties ();
			player.ViewPorts = new List<IViewPort> { viewPortMock.Object };
			Assert.IsFalse (player.Opened);

			player.Open (new MediaFileSet { new MediaFile () });

			viewPortMock.VerifySet (v => v.MessageVisible = false, Times.Once ());
			Assert.IsTrue (player.Opened);

			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (false);
		}

		[Test ()]
		public void TestOpenEmptyFileSet ()
		{
			viewPortMock = new Mock<IViewPort> ();
			viewPortMock.SetupAllProperties ();
			player.ViewPorts = new List<IViewPort> { viewPortMock.Object };
			Assert.IsFalse (player.Opened);

			player.Open (new MediaFileSet ());

			playerMock.Verify (p => p.Pause (false), Times.Once ());
			viewPortMock.VerifySet (v => v.Message = "No video loaded", Times.Once ());
			viewPortMock.VerifySet (v => v.MessageVisible = true, Times.Once ());
			Assert.IsTrue (player.Opened);
		}

		[Test]
		public void Open_InvalidFileSet_NoDataLoaded ()
		{
			// Arrange
			viewPortMock = new Mock<IViewPort> ();
			viewPortMock.SetupAllProperties ();
			player.ViewPorts = new List<IViewPort> { viewPortMock.Object };
			Assert.IsFalse (player.Opened);

			var media = new MediaFileSet ();
			var file = new MediaFile { FilePath = "invalid" };
			media.Add (file);

			// Act
			player.Open (media);

			// Assert
			playerMock.Verify (p => p.Pause (false), Times.Once ());
			viewPortMock.VerifySet (v => v.Message = "No video loaded", Times.Once ());
			viewPortMock.VerifySet (v => v.MessageVisible = true, Times.Once ());
			Assert.IsTrue (player.Opened);
			Assert.IsNull (playerVM.AbsoluteDuration);
		}

		[Test ()]
		public void TestOpenNullFileSet ()
		{
			viewPortMock = new Mock<IViewPort> ();
			viewPortMock.SetupAllProperties ();
			player.ViewPorts = new List<IViewPort> { viewPortMock.Object };
			Assert.IsFalse (player.Opened);

			player.Open (null);

			playerMock.Verify (p => p.Pause (false), Times.Once ());
			viewPortMock.VerifySet (v => v.Message = "No video loaded", Times.Once ());
			viewPortMock.VerifySet (v => v.MessageVisible = true, Times.Once ());
			Assert.IsFalse (player.Opened);
		}

		[Test ()]
		public void TestDispose ()
		{
			player.Dispose ();
			playerMock.Verify (p => p.Dispose (), Times.Once ());
			Assert.IsTrue (player.IgnoreTicks);
			Assert.IsNull (player.FileSet);
		}

		[Test ()]
		public void TestOpen ()
		{
			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (true);

			int timeCount = 0;
			bool multimediaError = false;
			Time curTime = null, duration = null;

			player.TimeChangedEvent += (c, d, seekable) => {
				curTime = c;
				duration = d;
				timeCount++;
			};

			/* Open but view is not ready */
			player.Open (mfs);
			Assert.AreEqual (mfs, player.FileSet);
			playerMock.Verify (p => p.Open (mfs [0]), Times.Never ());
			playerMock.Verify (p => p.Play (It.IsAny<bool> ()), Times.Never ());
			playerMock.Verify (p => p.Seek (new Time (0), true, false), Times.Never ());

			/* Open with an invalid camera configuration */
			EventToken et = App.Current.EventsBroker.Subscribe<MultimediaErrorEvent> ((e) => {
				multimediaError = true;
			});

			player.Ready (true);
			player.Open (mfs);
			Assert.IsTrue (multimediaError);
			Assert.IsNull (player.FileSet);
			Assert.IsFalse (player.Opened);

			/* Open with the view ready */
			currentTime = new Time (0);
			PreparePlayer ();
			playerMock.Verify (p => p.Open (mfs [0]), Times.Once ());
			playerMock.Verify (p => p.Play (It.IsAny<bool> ()), Times.Never ());
			playerMock.Verify (p => p.Seek (new Time (0), true, false), Times.Once ());
			Assert.AreEqual (1, timeCount);
			Assert.AreEqual ((float)320 / 240, viewPortMock.Object.Ratio);
			Assert.AreEqual (streamLength, duration);
			Assert.AreEqual (new Time (0), curTime);
			Assert.AreEqual (playerVM.Zoom, 1);

			App.Current.EventsBroker.Unsubscribe<MultimediaErrorEvent> (et);

			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (false);
		}

		[Test ()]
		public void TestPlayPause ()
		{
			bool loadSent = false;
			bool playing = false;
			FrameDrawing drawing = null;


			player.PlaybackStateChangedEvent += (e) => {
				playing = e.Playing;
			};
			player.LoadDrawingsEvent += (f) => {
				loadSent = true;
				drawing = f;
			};

			/* Start playing */
			player.Play ();
			Assert.IsTrue (loadSent);
			Assert.IsNull (drawing);
			playerMock.Verify (p => p.Play (false), Times.Once ());
			Assert.IsTrue (player.Playing);
			Assert.IsTrue (playing);

			/* Go to pause */
			loadSent = false;
			player.Pause ();
			Assert.IsFalse (loadSent);
			Assert.IsNull (drawing);
			playerMock.Verify (p => p.Pause (false), Times.Once ());
			Assert.IsFalse (player.Playing);
			Assert.IsFalse (playing);

			/* Check now with a still image loaded */
			playerMock.ResetCalls ();
			player.Ready (true);

			player.LoadPlaylistEvent (playlistVM, playlistVM.ViewModels [1], true);
			player.Play ();
			playerMock.Verify (p => p.Play (It.IsAny<bool> ()), Times.Never ());
			playerMock.Verify (p => p.Pause (false), Times.Once ());
			Assert.IsTrue (player.Playing);

			/* Go to pause */
			playerMock.ResetCalls ();
			player.Pause ();
			playerMock.Verify (p => p.Play (It.IsAny<bool> ()), Times.Never ());
			playerMock.Verify (p => p.Pause (It.IsAny<bool> ()), Times.Never ());
			Assert.IsFalse (player.Playing);
		}

		[Test ()]
		public void TestTogglePlay ()
		{
			player.TogglePlay ();
			Assert.IsTrue (player.Playing);
			player.TogglePlay ();
			Assert.IsFalse (player.Playing);
		}

		[Test ()]
		public void TestSeek ()
		{
			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (true);

			int drawingsCount = 0;
			int timeChanged = 0;
			Time curTime = new Time (0);
			Time strLenght = new Time (0);

			player.TimeChangedEvent += (c, d, s) => {
				timeChanged++;
				curTime = c;
				strLenght = d;
			};
			player.LoadDrawingsEvent += (f) => drawingsCount++;
			player.Ready (true);
			player.Open (mfs);
			Assert.AreEqual (0, timeChanged);

			/* Not ready, seek queued */
			currentTime = new Time (2000);
			player.Seek (currentTime, false, false, false);
			playerMock.Verify (p => p.Seek (It.IsAny<Time> (), It.IsAny<bool> (), It.IsAny<bool> ()), Times.Never ());
			Assert.AreEqual (1, drawingsCount);
			Assert.AreEqual (0, timeChanged);
			playerMock.ResetCalls ();

			/* Once ready the seek kicks in */
			currentTime = new Time (2000);
			playerMock.Raise (p => p.ReadyToSeek += null, this);
			/* ReadyToSeek emits TimeChanged */
			Assert.AreEqual (1, timeChanged);
			playerMock.Verify (p => p.Seek (currentTime, false, false), Times.Once ());
			Assert.AreEqual (1, drawingsCount);
			Assert.AreEqual (currentTime, curTime);
			Assert.AreEqual (strLenght, streamLength);
			playerMock.ResetCalls ();

			/* Seek when player ready to seek */
			currentTime = new Time (4000);
			player.Seek (currentTime, true, true, false);
			playerMock.Verify (p => p.Seek (currentTime, true, true), Times.Once ());
			Assert.AreEqual (2, drawingsCount);
			Assert.AreEqual (2, timeChanged);
			Assert.AreEqual (currentTime, curTime);
			Assert.AreEqual (strLenght, streamLength);
			playerMock.ResetCalls ();

			currentTime = new Time (5000);
			player.LoadPlaylistEvent (playlistVM, playlistVM.ViewModels [1], true);
			player.Seek (currentTime, true, true, false);
			playerMock.Verify (p => p.Seek (currentTime, It.IsAny<bool> (), It.IsAny<bool> ()), Times.Never ());
			Assert.AreEqual (2, drawingsCount);
			playerMock.ResetCalls ();

			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (false);
		}

		[Test ()]
		public void TestSeekProportional ()
		{
			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (true);
			int seekPos;
			int timeChanged = 0;
			Time curTime = new Time (0);
			Time strLenght = new Time (0);
			var triggerableSeeker = new TriggerableSeeker ();
			player = new VideoPlayerController (triggerableSeeker);
			player.SetViewModel (playerVM);


			player.TimeChangedEvent += (c, d, s) => {
				timeChanged++;
				curTime = c;
				strLenght = d;
			};
			PreparePlayer ();

			/* Seek without any segment loaded */
			seekPos = (int)(streamLength.MSeconds * 0.1);
			currentTime = new Time (seekPos);
			player.Seek (0.1f);
			playerMock.Verify (p => p.Seek (new Time (seekPos), false, false), Times.Once ());
			Assert.IsTrue (timeChanged != 0);
			Assert.AreEqual (seekPos, curTime.MSeconds);
			Assert.AreEqual (strLenght.MSeconds, streamLength.MSeconds);

			/* Seek with a segment loaded */
			timeChanged = 0;
			seekPos = (int)(eventVM1.Duration.MSeconds * 0.5);
			currentTime = new Time (eventVM1.Start.MSeconds + seekPos);
			player.LoadEvent (eventVM1, new Time (0), true);
			playerMock.ResetCalls ();
			player.Seek (0.1f);
			player.Seek (0.5f);
			// this is to avoid the timer + sleep to test the throttled seek. In real code we don't need the trigger.
			triggerableSeeker.TriggerSeek ();

			// Check we got called only once
			playerMock.Verify (p => p.Seek (It.IsAny<Time> (), true, false), Times.Once ());
			// And with the last value
			playerMock.Verify (p => p.Seek (new Time (eventVM1.Start.MSeconds + seekPos), true, false), Times.Once ());
			Assert.IsTrue (timeChanged != 0);
			/* current time is now relative to the loaded segment's duration */
			Assert.AreEqual (eventVM1.Duration * 0.5, curTime);
			Assert.AreEqual (eventVM1.Duration, strLenght);

			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (false);
		}

		[Test ()]
		public void TestStepping ()
		{

			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (true);

			int timeChanged = 0;
			int loadDrawingsChanged = 0;
			Time curTime = new Time (0);
			Time strLenght = new Time (0);

			currentTime = new Time { TotalSeconds = 2000 };
			PreparePlayer ();
			player.TimeChangedEvent += (c, d, s) => {
				timeChanged++;
				curTime = c;
				strLenght = d;
			};
			player.LoadDrawingsEvent += (f) => {
				if (f == null) {
					loadDrawingsChanged++;
				}
			};

			/* Without a segment loaded */

			player.SeekToNextFrame ();
			playerMock.Verify (p => p.SeekToNextFrame (), Times.Once ());
			Assert.AreEqual (1, loadDrawingsChanged);
			Assert.AreEqual (1, timeChanged);
			Assert.AreEqual (currentTime, curTime);
			Assert.AreEqual (streamLength, strLenght);

			loadDrawingsChanged = 0;
			timeChanged = 0;
			player.SeekToPreviousFrame ();
			playerMock.Verify (p => p.SeekToPreviousFrame (), Times.Once ());
			Assert.AreEqual (1, loadDrawingsChanged);
			Assert.AreEqual (1, timeChanged);
			Assert.AreEqual (currentTime, curTime);
			Assert.AreEqual (streamLength, strLenght);

			playerMock.ResetCalls ();
			loadDrawingsChanged = 0;
			timeChanged = 0;
			player.StepForward ();
			Assert.AreEqual (1, loadDrawingsChanged);
			Assert.AreEqual (1, timeChanged);
			playerMock.Verify (p => p.Seek (currentTime + playerVM.Step, true, false), Times.Once ());

			playerMock.ResetCalls ();
			loadDrawingsChanged = 0;
			timeChanged = 0;
			player.StepBackward ();
			Assert.AreEqual (1, loadDrawingsChanged);
			Assert.AreEqual (1, timeChanged);
			playerMock.Verify (p => p.Seek (currentTime - playerVM.Step, true, false), Times.Once ());

			/* Now with an image loaded */
			playerMock.ResetCalls ();
			loadDrawingsChanged = 0;
			timeChanged = 0;
			player.LoadPlaylistEvent (playlistVM, playlistVM.ViewModels [1], true);
			player.SeekToNextFrame ();
			playerMock.Verify (p => p.SeekToNextFrame (), Times.Never ());
			Assert.AreEqual (0, loadDrawingsChanged);
			Assert.AreEqual (0, timeChanged);

			player.SeekToPreviousFrame ();
			playerMock.Verify (p => p.SeekToPreviousFrame (), Times.Never ());
			Assert.AreEqual (0, loadDrawingsChanged);
			Assert.AreEqual (0, timeChanged);

			player.StepForward ();
			Assert.AreEqual (0, loadDrawingsChanged);
			Assert.AreEqual (0, timeChanged);
			playerMock.Verify (p => p.Seek (currentTime + playerVM.Step, true, false), Times.Never ());

			player.StepBackward ();
			Assert.AreEqual (0, loadDrawingsChanged);
			Assert.AreEqual (0, timeChanged);
			playerMock.Verify (p => p.Seek (currentTime - playerVM.Step, true, false), Times.Never ());

			/* Now with an event loaded */
			currentTime = new Time (5000);
			player.UnloadCurrentEvent ();
			player.LoadEvent (eventVM2, new Time (0), true);
			timeChanged = 0;
			playerMock.ResetCalls ();
			player.SeekToNextFrame ();
			playerMock.Verify (p => p.SeekToNextFrame (), Times.Once ());
			Assert.AreEqual (1, timeChanged);
			Assert.AreEqual (currentTime - eventVM2.Start, curTime);
			Assert.AreEqual (eventVM2.Duration, strLenght);

			loadDrawingsChanged = 0;
			timeChanged = 0;
			player.SeekToPreviousFrame ();
			playerMock.Verify (p => p.SeekToPreviousFrame (), Times.Once ());
			Assert.AreEqual (1, loadDrawingsChanged);
			Assert.AreEqual (1, timeChanged);
			Assert.AreEqual (currentTime - eventVM2.Start, curTime);
			Assert.AreEqual (eventVM2.Duration, strLenght);

			playerMock.ResetCalls ();
			loadDrawingsChanged = 0;
			timeChanged = 0;
			player.StepForward ();
			Assert.AreEqual (1, loadDrawingsChanged);
			Assert.AreEqual (1, timeChanged);
			playerMock.Verify (p => p.Seek (currentTime + playerVM.Step, true, false), Times.Once ());

			playerMock.ResetCalls ();
			currentTime = new Time (15000);
			loadDrawingsChanged = 0;
			timeChanged = 0;
			player.StepBackward ();
			Assert.AreEqual (1, loadDrawingsChanged);
			Assert.AreEqual (1, timeChanged);
			playerMock.Verify (p => p.Seek (currentTime - playerVM.Step, true, false), Times.Once ());

			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (false);
		}

		[Test ()]
		public void TestChangeFramerate ()
		{
			float rate = 1;

			playerMock.Object.Rate = 1;
			player.PlaybackRateChangedEvent += (r) => rate = r;

			for (int i = 1; i < 5; i++) {
				player.FramerateUp ();
				playerMock.VerifySet (p => p.Rate = 1 + i);
				Assert.AreEqual (1 + i, rate);
			}
			/* Max is 5 */
			Assert.AreEqual (5, player.Rate);
			player.FramerateUp ();
			playerMock.VerifySet (p => p.Rate = 5);
			Assert.AreEqual (5, rate);

			player.Rate = 1;
			for (int i = 1; i < 25; i++) {
				player.FramerateDown ();
				double _rate = player.Rate;
				playerMock.VerifySet (p => p.Rate = _rate);
				Assert.AreEqual (1 - (double)i / 25, rate, 0.01);
			}

			/* Min is 1 / 30 */
			Assert.AreEqual ((double)1 / 25, player.Rate, 0.01);
			player.FramerateDown ();
			Assert.AreEqual ((double)1 / 25, player.Rate, 0.01);
		}

		[Test ()]
		public void TestSeekMaintainsSpeedRate ()
		{
			PreparePlayer (true);
			player.Rate = 0.16; // 4 / 25
			double expected = player.Rate;
			var timeToSeek = new Time (2000);

			player.Seek (timeToSeek, false, false, true);

			Assert.AreEqual (expected, player.Rate);
		}

		[Test ()]
		public void TestNext ()
		{
			int nextSent = 0;
			PreparePlayer ();
			EventToken et = App.Current.EventsBroker.Subscribe<PlaylistElementLoadedEvent> ((e) => nextSent++);

			player.Next ();
			Assert.AreEqual (0, nextSent);

			player.LoadPlaylistEvent (playlistVM, playlistVM.ViewModels [0], true);
			Assert.AreEqual (0, playlistVM.CurrentIndex);
			Assert.AreEqual (1, nextSent);

			player.Next ();
			Assert.AreEqual (1, playlistVM.CurrentIndex);
			Assert.AreEqual (2, nextSent);

			playlistVM.Next ();
			Assert.IsFalse (playlistVM.HasNext ());
			player.Next ();
			Assert.AreEqual (2, nextSent);

			App.Current.EventsBroker.Unsubscribe<PlaylistElementLoadedEvent> (et);
		}

		[Test ()]
		public void TestNextMantainsPlayingState ()
		{
			bool stateBeforeNext, stateAfterNext;
			PreparePlayer ();
			//Testing state playing
			player.LoadPlaylistEvent (playlistVM, playlistVM.ViewModels [0], true);
			stateBeforeNext = player.Playing;
			Assert.IsTrue (stateBeforeNext);
			player.Next ();
			stateAfterNext = player.Playing;
			Assert.AreEqual (stateBeforeNext, stateAfterNext);
			//Testing State Pause
			player.LoadPlaylistEvent (playlistVM, playlistVM.ViewModels [0], true);
			player.Pause ();
			stateBeforeNext = player.Playing;
			Assert.IsFalse (stateBeforeNext);
			player.Next ();
			stateAfterNext = player.Playing;
			Assert.AreEqual (stateBeforeNext, stateAfterNext);
		}

		[Test ()]
		public void TestPrevious ()
		{
			int prevSent = 0;
			currentTime = new Time (0);
			PreparePlayer ();
			EventToken et = App.Current.EventsBroker.Subscribe<PlaylistElementLoadedEvent> ((e) => prevSent++);

			player.Previous (false);
			playerMock.Verify (p => p.Seek (new Time (0), true, false));
			Assert.AreEqual (0, prevSent);

			player.LoadEvent (eventVM1, new Time (0), false);
			playerMock.ResetCalls ();
			player.Previous (false);
			playerMock.Verify (p => p.Seek (eventVM1.Start, true, false));
			Assert.AreEqual (0, prevSent);

			player.LoadPlaylistEvent (playlistVM, playlistVM.ViewModels [0], true);
			Assert.AreEqual (1, prevSent);
			playerMock.ResetCalls ();
			player.Previous (false);
			Assert.AreEqual (1, prevSent);
			playlistVM.Next ();
			player.Previous (false);
			Assert.AreEqual (2, prevSent);

			App.Current.EventsBroker.Unsubscribe<PlaylistElementLoadedEvent> (et);
		}

		[Test ()]
		public void TestPreviousMantainsPlayingState ()
		{
			bool stateBeforePrevious, stateAfterPrevious;
			PreparePlayer ();
			//Testing state playing
			player.LoadPlaylistEvent (playlistVM, playlistVM.ViewModels [1], true);
			stateBeforePrevious = player.Playing;
			Assert.IsTrue (stateBeforePrevious);
			player.Previous ();
			stateAfterPrevious = player.Playing;
			Assert.AreEqual (stateBeforePrevious, stateAfterPrevious);
			//Testing State Pause
			player.LoadPlaylistEvent (playlistVM, playlistVM.ViewModels [1], true);
			player.Pause ();
			stateBeforePrevious = player.Playing;
			Assert.IsFalse (stateBeforePrevious);
			player.Previous ();
			stateAfterPrevious = player.Playing;
			Assert.AreEqual (stateBeforePrevious, stateAfterPrevious);
		}

		[Test ()]

		public void TestPrev ()
		{
			int playlistElementSelected = 0;
			currentTime = new Time (4000);
			PreparePlayer ();
			App.Current.EventsBroker.Publish<OpenedProjectEvent> (
				new OpenedProjectEvent {
					Project = new Utils.ProjectDummy (),
					ProjectType = ProjectType.FileProject,
					Filter = null,
				}
			);
			EventToken et = App.Current.EventsBroker.Subscribe<LoadPlaylistElementEvent> ((e) => playlistElementSelected++);

			App.Current.EventsBroker.Publish<LoadEventEvent> (
				new LoadEventEvent {
					TimelineEvent = eventVM1
				}
			);
			// loadedPlay != null
			playerMock.ResetCalls ();

			player.Previous (false);

			playerMock.Verify (player => player.Seek (eventVM1.Start, It.IsAny<bool> (), It.IsAny<bool> ()), Times.Once ());
			Assert.AreEqual (0, playlistElementSelected);

			App.Current.EventsBroker.Unsubscribe<LoadPlaylistElementEvent> (et);
		}

		[Test ()]
		public void TestPrev2 ()
		{
			int playlistElementSelected = 0;
			currentTime = new Time (4000);
			PreparePlayer ();
			EventToken et = App.Current.EventsBroker.Subscribe<LoadPlaylistElementEvent> ((e) => playlistElementSelected++);
			playerMock.ResetCalls ();
			// loadedPlay == null

			player.Previous (false);

			playerMock.Verify (player => player.Seek (new Time (0), It.IsAny<bool> (), It.IsAny<bool> ()), Times.Once ());
			Assert.AreEqual (0, playlistElementSelected);

			App.Current.EventsBroker.Unsubscribe<LoadPlaylistElementEvent> (et);
		}

		[Test ()]
		public void TestPrev3 ()
		{
			int playlistElementLoaded = 0;
			currentTime = new Time (4000);
			PreparePlayer ();
			EventToken et = App.Current.EventsBroker.Subscribe<PlaylistElementLoadedEvent> ((e) => playlistElementLoaded++);

			Playlist localPlaylist = new Playlist ();
			PlaylistPlayElement element = new PlaylistPlayElement (new TimelineEvent ());
			element.Play.Start = new Time (0);
			element.Play.Stop = new Time (10000);
			localPlaylist.Elements.Add (element);
			var localPlaylistVM = new PlaylistVM { Model = localPlaylist };
			player.LoadPlaylistEvent (localPlaylistVM, localPlaylistVM.ViewModels [0], false);
			playerMock.ResetCalls ();

			Assert.AreEqual (1, playlistElementLoaded);
			player.Previous (false);

			playerMock.Verify (player => player.Seek (element.Play.Start, true, false), Times.Once ());
			Assert.AreEqual (2, playlistElementLoaded);

			App.Current.EventsBroker.Unsubscribe<PlaylistElementLoadedEvent> (et);
		}

		[Test ()]
		public void TestPrev4 ()
		{
			int playlistElementLoaded = 0;
			currentTime = new Time (499);
			PreparePlayer ();
			EventToken et = App.Current.EventsBroker.Subscribe<LoadPlaylistElementEvent> ((e) => playlistElementLoaded++);

			Playlist localPlaylist = new Playlist ();
			PlaylistPlayElement element0 = new PlaylistPlayElement (new TimelineEvent ());
			PlaylistPlayElement element = new PlaylistPlayElement (new TimelineEvent ());
			element.Play.Start = new Time (0);
			localPlaylist.Elements.Add (element0);
			localPlaylist.Elements.Add (element);
			var localPlaylistVM = new PlaylistVM { Model = localPlaylist };
			player.Switch (null, localPlaylistVM, new PlaylistElementVM { Model = element });
			playerMock.ResetCalls ();

			player.Previous (false);

			Assert.AreEqual (0, playlistElementLoaded);
			Assert.AreSame (element0, localPlaylistVM.Selected.Model);
			App.Current.EventsBroker.Unsubscribe<LoadPlaylistElementEvent> (et);
		}

		[Test ()]
		public void TestPrev5 ()
		{
			int playlistElementLoaded = 0;
			currentTime = new Time (499);
			PreparePlayer ();
			EventToken et = App.Current.EventsBroker.Subscribe<PlaylistElementLoadedEvent> ((e) => playlistElementLoaded++);

			Playlist localPlaylist = new Playlist ();
			IPlaylistElement element = new PlaylistImage (new Image (1, 1), new Time (10));
			localPlaylist.Elements.Add (element);
			var localPlaylistVM = new PlaylistVM { Model = localPlaylist };
			player.Switch (null, localPlaylistVM, new PlaylistElementVM { Model = element });
			playerMock.ResetCalls ();

			player.Previous (false);

			Assert.AreEqual (0, playlistElementLoaded);
			playerMock.Verify (player => player.Seek (It.IsAny<Time> (), It.IsAny<bool> (), It.IsAny<bool> ()), Times.Never ());

			App.Current.EventsBroker.Unsubscribe<PlaylistElementLoadedEvent> (et);
		}

		[Test ()]
		public void TestPrev6 ()
		{
			int playlistElementLoaded = 0;
			currentTime = new Time (4000);
			PreparePlayer ();
			EventToken et = App.Current.EventsBroker.Subscribe<PlaylistElementLoadedEvent> ((e) => playlistElementLoaded++);

			Playlist localPlaylist = new Playlist ();
			PlaylistPlayElement element0 = new PlaylistPlayElement (new TimelineEvent ());
			PlaylistPlayElement element = new PlaylistPlayElement (new TimelineEvent ());
			element.Play.Start = new Time (0);
			localPlaylist.Elements.Add (element0);
			localPlaylist.Elements.Add (element);
			var localPlaylistVM = new PlaylistVM { Model = localPlaylist };
			player.Switch (null, localPlaylistVM, new PlaylistElementVM { Model = element });
			playerMock.ResetCalls ();

			player.Previous (true);

			Assert.AreEqual (0, playlistElementLoaded);
			Assert.AreSame (element0, localPlaylistVM.Selected.Model);

			App.Current.EventsBroker.Unsubscribe<PlaylistElementLoadedEvent> (et);
		}

		[Test ()]
		public void TestEOS ()
		{
			PreparePlayer ();
			playerMock.ResetCalls ();
			playerMock.Raise (p => p.Eos += null, this);
			playerMock.Verify (p => p.Seek (new Time (0), true, false), Times.Once ());
			playerMock.Verify (p => p.Pause (false), Times.Once ());
			playerMock.ResetCalls ();

			TimelineEventVM evtLocalVM = new TimelineEventVM () {
				Model = new TimelineEvent {
					Start = new Time (100),
					Stop = new Time (20000),
					CamerasConfig = new RangeObservableCollection<CameraConfig> { new CameraConfig (0) },
					FileSet = mfs
				}
			};
			player.LoadEvent (evtLocalVM, new Time (0), true);
			playerMock.ResetCalls ();
			playerMock.Raise (p => p.Eos += null, this);
			playerMock.Verify (p => p.Seek (evtLocalVM.Start, true, false), Times.Once ());
			playerMock.Verify (p => p.Pause (false), Times.Once ());
		}

		[Test ()]
		public void TestError ()
		{
			string msg = null;

			App.Current.EventsBroker.Subscribe<MultimediaErrorEvent> ((e) => {
				msg = e.Message;
			});
			playerMock.Raise (p => p.Error += null, this, "error");
			Assert.AreEqual ("error", msg);
		}

		[Test ()]
		public void TestUnloadEvent ()
		{
			int elementLoaded = 0;
			int brokerElementLoaded = 0;
			PreparePlayer ();
			App.Current.EventsBroker.Subscribe<EventLoadedEvent> ((evt) => {
				if (evt.TimelineEvent?.Model == null) {
					brokerElementLoaded--;
				} else {
					brokerElementLoaded++;
				}
			});
			player.ElementLoadedEvent += (element, hasNext) => {
				if (element == null) {
					elementLoaded--;
				} else {
					elementLoaded++;
				}
			};
			// Load
			player.LoadEvent (eventVM1, new Time (0), true);
			Assert.AreEqual (1, elementLoaded);
			Assert.AreEqual (1, brokerElementLoaded);
			Assert.AreEqual (eventVM1.CamerasConfig, player.CamerasConfig);
			// Unload
			player.UnloadCurrentEvent ();
			Assert.AreEqual (0, elementLoaded);
			Assert.AreEqual (0, brokerElementLoaded);
			// Check that cameras have been restored
			Assert.AreEqual (new List<CameraConfig> { new CameraConfig (0), new CameraConfig (1) }, player.CamerasConfig);

			/* Change again the cameras visible */
			player.CamerasConfig = new RangeObservableCollection<CameraConfig> {
					new CameraConfig (1),
					new CameraConfig (0)
				};
			Assert.AreEqual (eventVM1.CamerasConfig, new List<CameraConfig> { new CameraConfig (0) });
			player.LoadEvent (eventVM1, new Time (0), true);
			Assert.AreEqual (1, elementLoaded);
			Assert.AreEqual (1, brokerElementLoaded);
			Assert.AreEqual (eventVM1.CamerasConfig, player.CamerasConfig);
			/* And unload */
			player.UnloadCurrentEvent ();
			Assert.AreEqual (0, elementLoaded);
			Assert.AreEqual (0, brokerElementLoaded);
			// Check that cameras have been restored
			Assert.AreEqual (new List<CameraConfig> { new CameraConfig (1), new CameraConfig (0) }, player.CamerasConfig);
		}

		[Test ()]
		public void TestUnloadEvent_CheckPlayingIsReset ()
		{
			// Arrange
			PreparePlayer ();
			player.LoadEvent (eventVM1, new Time (0), true);

			// Action
			player.UnloadCurrentEvent ();

			// Assert
			Assert.IsFalse (eventVM1.Playing);
		}

		[Test ()]
		public void TestCamerasVisibleValidation ()
		{
			// Create an event referencing unknown MediaFiles in the set.
			TimelineEventVM eventVM2 = new TimelineEventVM () {
				Model = new TimelineEvent {
					Start = new Time (150),
					Stop = new Time (200),
					CamerasConfig = new RangeObservableCollection<CameraConfig> {
						new CameraConfig (0),
						new CameraConfig (1),
						new CameraConfig (4),
						new CameraConfig (6)
					},
					FileSet = mfs
				}
			};

			player.CamerasConfig = new RangeObservableCollection<CameraConfig> {
					new CameraConfig (1),
					new CameraConfig (0)
				};
			viewPortMock = new Mock<IViewPort> ();
			viewPortMock.SetupAllProperties ();
			player.ViewPorts = new List<IViewPort> { viewPortMock.Object, viewPortMock.Object };
			player.Ready (true);
			player.LoadEvent (eventVM2, new Time (0), true);
			// Only valid cameras should be visible although no fileset was opened.
			Assert.AreEqual (2, player.CamerasConfig.Count);
			Assert.AreEqual (0, player.CamerasConfig [0].Index);
			Assert.AreEqual (1, player.CamerasConfig [1].Index);
			// Again now that the fileset is opened
			player.LoadEvent (eventVM2, new Time (0), true);
			// Only valid cameras should be visible
			Assert.AreEqual (2, player.CamerasConfig.Count);
			Assert.AreEqual (0, player.CamerasConfig [0].Index);
			Assert.AreEqual (1, player.CamerasConfig [1].Index);
		}

		[Test ()]
		public void TestLoadEvent ()
		{
			int elementLoaded = 0;
			int brokerElementLoaded = 0;
			int prepareView = 0;

			App.Current.EventsBroker.Subscribe<EventLoadedEvent> ((evt) => {
				if (evt != null) {
					brokerElementLoaded++;
				}
			});
			player.ElementLoadedEvent += (element, hasNext) => {
				if (element != null) {
					elementLoaded++;
				}
			};
			player.PrepareViewEvent += () => prepareView++;

			/* Not ready to seek */
			player.CamerasConfig = new RangeObservableCollection<CameraConfig> {
					new CameraConfig (0),
					new CameraConfig (1)
				};
			viewPortMock = new Mock<IViewPort> ();
			viewPortMock.SetupAllProperties ();
			player.ViewPorts = new List<IViewPort> { viewPortMock.Object, viewPortMock.Object };
			Assert.AreEqual (0, prepareView);

			/* Loading an event with the player not ready should trigger the
			 * PrepareViewEvent and wait until it's ready */
			player.LoadEvent (eventVM1, new Time (0), true);
			Assert.AreEqual (1, prepareView);
			Assert.IsNull (player.FileSet);

			player.Ready (true);
			Assert.AreEqual (1, elementLoaded);
			Assert.AreEqual (1, brokerElementLoaded);
			Assert.AreEqual (mfs, player.FileSet);

			player.LoadEvent (eventVM1, new Time (0), true);
			Assert.AreEqual (mfs, player.FileSet);
			Assert.IsFalse (player.Playing);
			Assert.AreEqual (2, elementLoaded);
			Assert.AreEqual (2, brokerElementLoaded);
			playerMock.Verify (p => p.Seek (It.IsAny<Time> (), It.IsAny<bool> (), It.IsAny<bool> ()), Times.Never ());


			/* Ready to seek */
			currentTime = eventVM1.Start;
			playerMock.Raise (p => p.ReadyToSeek += null, this);
			Assert.IsTrue (player.Playing);
			playerMock.Verify (p => p.Open (mfs [0]));
			playerMock.Verify (p => p.Seek (eventVM1.Start, true, false), Times.Once ());
			playerMock.Verify (p => p.Play (false), Times.Once ());
			playerMock.VerifySet (p => p.Rate = 1);
			Assert.AreEqual (2, elementLoaded);
			Assert.AreEqual (2, brokerElementLoaded);
			elementLoaded = brokerElementLoaded = 0;
			playerMock.ResetCalls ();

			/* Open with a new MediaFileSet and also check seekTime and playing values*/
			MediaFileSet nfs = Cloner.Clone (mfs);
			nfs.ID = Guid.NewGuid ();
			eventVM1.FileSet = nfs;
			player.LoadEvent (eventVM1, eventVM1.Duration, false);
			Assert.AreEqual (1, elementLoaded);
			Assert.AreEqual (1, brokerElementLoaded);
			elementLoaded = brokerElementLoaded = 0;
			Assert.IsTrue (nfs.Equals (player.FileSet));
			Assert.AreEqual (nfs, player.FileSet);
			playerMock.Verify (p => p.Open (nfs [0]));
			playerMock.Verify (p => p.Play (It.IsAny<bool> ()), Times.Never ());
			playerMock.Verify (p => p.Pause (false), Times.Once ());
			playerMock.VerifySet (p => p.Rate = 1);
			playerMock.Raise (p => p.ReadyToSeek += null, this);
			playerMock.Verify (p => p.Seek (eventVM1.Stop, true, false), Times.Once ());
			playerMock.Verify (p => p.Play (It.IsAny<bool> ()), Times.Never ());
			playerMock.ResetCalls ();

			/* Open another event with the same MediaFileSet and already ready to seek
			 * and check the cameras layout and visibility is respected */
			TimelineEventVM eventVM2 = new TimelineEventVM () {
				Model = new TimelineEvent {
					Start = new Time (400),
					Stop = new Time (50000),
					CamerasConfig = new RangeObservableCollection<CameraConfig> {
						new CameraConfig (1),
						new CameraConfig (0)
					},
					CamerasLayout = "test",
					FileSet = nfs
				}
			};
			player.LoadEvent (eventVM2, new Time (0), true);
			Assert.AreEqual (1, elementLoaded);
			Assert.AreEqual (1, brokerElementLoaded);
			elementLoaded = brokerElementLoaded = 0;
			playerMock.Verify (p => p.Open (nfs [0]), Times.Never ());
			playerMock.Verify (p => p.Seek (eventVM2.Start, true, false), Times.Once ());
			playerMock.Verify (p => p.Play (false), Times.Once ());
			playerMock.VerifySet (p => p.Rate = 1);
			Assert.AreEqual (eventVM2.CamerasConfig, player.CamerasConfig);
			Assert.AreEqual (eventVM2.CamerasLayout, player.CamerasLayout);
			playerMock.ResetCalls ();
		}

		[Test ()]
		public void TestLoadPlaylistEvent ()
		{
			int elementLoaded = 0;
			int prepareView = 0;
			MediaFileSet nfs;
			PlaylistPlayElement el1;

			player.ElementLoadedEvent += (element, hasNext) => {
				if (element != null) {
					elementLoaded++;
				}
			};
			player.PrepareViewEvent += () => prepareView++;

			/* Not ready to seek */
			player.CamerasConfig = new RangeObservableCollection<CameraConfig> {
					new CameraConfig (0),
					new CameraConfig (1)
				};
			viewPortMock = new Mock<IViewPort> ();
			viewPortMock.SetupAllProperties ();
			player.ViewPorts = new List<IViewPort> { viewPortMock.Object, viewPortMock.Object };
			Assert.AreEqual (0, prepareView);

			/* Load playlist timeline event element */
			nfs = mfs.Clone ();
			nfs.ID = Guid.NewGuid ();
			el1 = playlistVM.ChildModels [0] as PlaylistPlayElement;
			el1.Play.FileSet = nfs;
			currentTime = el1.Play.Start;
			player.LoadPlaylistEvent (playlistVM, playlistVM.ViewModels [0], true);
			Assert.AreEqual (0, elementLoaded);
			Assert.AreEqual (1, prepareView);

			player.Ready (true);
			Assert.AreEqual (1, elementLoaded);
			elementLoaded = 0;
			Assert.AreEqual (el1.CamerasConfig, player.CamerasConfig);
			Assert.AreEqual (el1.CamerasLayout, player.CamerasLayout);
			playerMock.Verify (p => p.Open (nfs [0]), Times.Once ());
			playerMock.Verify (p => p.Seek (el1.Play.Start, true, false), Times.Never ());
			playerMock.Verify (p => p.Play (It.IsAny<bool> ()), Times.Never ());
			playerMock.VerifySet (p => p.Rate = 1);
			playerMock.Raise (p => p.ReadyToSeek += null, this);
			playerMock.Verify (p => p.Seek (el1.Play.Start, true, false), Times.Once ());
			playerMock.Verify (p => p.Play (false), Times.Once ());

			/* Load still image */
			player.LoadPlaylistEvent (playlistVM, playlistVM.ViewModels [1], true);
			playerMock.ResetCalls ();
			Assert.IsTrue (player.Playing);
			player.Pause ();
			playerMock.Verify (p => p.Pause (It.IsAny<bool> ()), Times.Never ());
			Assert.IsFalse (player.Playing);
			player.Play ();
			playerMock.Verify (p => p.Play (It.IsAny<bool> ()), Times.Never ());
			Assert.IsTrue (player.Playing);

			/* Load drawings */
			PlaylistDrawing dr = new PlaylistDrawing (new FrameDrawing ());
			playlistVM.ChildModels.Add (dr);
			dr.Duration = new Time (5000);
			player.LoadPlaylistEvent (playlistVM, playlistVM.ViewModels [2], true);
			playerMock.ResetCalls ();
			Assert.IsTrue (player.Playing);
			player.Pause ();
			playerMock.Verify (p => p.Pause (It.IsAny<bool> ()), Times.Never ());
			Assert.IsFalse (player.Playing);
			player.Play ();
			playerMock.Verify (p => p.Play (It.IsAny<bool> ()), Times.Never ());
			Assert.IsTrue (player.Playing);

			/* Load video */
			PlaylistVideo vid = new PlaylistVideo (mfs [0]);
			playlistVM.ChildModels.Add (vid);
			player.LoadPlaylistEvent (playlistVM, playlistVM.ViewModels [3], true);
			Assert.AreNotEqual (mfs, player.FileSet);
			Assert.IsTrue (player.Playing);
			Assert.AreEqual (new List<CameraConfig> { new CameraConfig (0) }, player.CamerasConfig);

			/* Load video from another playlist  (playlist is different than LoadedPlayList)*/
			Playlist localPlaylist = new Playlist ();
			var localPlaylistVM = new PlaylistVM { Model = localPlaylist };
			player.Stop (false);
			Assert.IsFalse (player.Playing);
			player.LoadPlaylistEvent (localPlaylistVM, playlistVM.ViewModels [3], true);
			Assert.AreNotEqual (mfs, player.FileSet);
			Assert.IsTrue (player.Playing);
			Assert.AreEqual (new List<CameraConfig> { new CameraConfig (0) }, player.CamerasConfig);
		}

		[Test ()]
		public void TestLoadPlaylistEventNullPlayList ()
		{
			/* Load video */
			player.Ready (true);
			PlaylistVideo vid = new PlaylistVideo (mfs [0]);
			playlistVM.ChildModels.Add (vid);
			player.LoadPlaylistEvent (null, playlistVM.ViewModels [2], true);
			Assert.IsFalse (player.Playing);
			Assert.IsNull (player.LoadedPlaylist);
			Assert.IsNull (player.FileSet);
		}

		[Test ()]
		public void TestStopTimes ()
		{
			plController.Stop ();

			PreparePlayer ();

			/* Check the player is stopped when we pass the event stop time */
			currentTime = new Time (0);
			player.LoadEvent (eventVM1, new Time (0), true);
			Assert.IsTrue (player.Playing);
			currentTime = eventVM1.Duration + new Time (1000);
			player.Seek (currentTime, true, false);
			Assert.IsFalse (player.Playing);

			/* Check the player is stopped when we pass the image stop time */
			currentTime = new Time (0);

			player.LoadPlaylistEvent (playlistVM, playlistVM.ViewModels [1], true);
			playlistVM.SetActive (playlistVM.ViewModels [1]);
			Assert.IsTrue (player.Playing);
			currentTime = plImage.Duration + 1000;
			player.Seek (currentTime, true, false);
			Assert.IsFalse (player.Playing);
		}

		[Test ()]
		public void TestEventDrawings ()
		{
			FrameDrawing dr, drSent = null;

			player.LoadDrawingsEvent += (frameDrawing) => {
				drSent = frameDrawing;
			};

			dr = new FrameDrawing {
				Render = new Time (150),
				CameraConfig = new CameraConfig (0),
			};
			currentTime = new Time (0);
			PreparePlayer ();

			/* Checks drawings are loaded when the clock reaches the render time */
			eventVM1.Drawings.Add (dr);
			player.LoadEvent (eventVM1, new Time (0), true);
			Assert.IsTrue (player.Playing);
			currentTime = dr.Render;
			player.Seek (currentTime - eventVM1.Start, true, false);
			Assert.IsFalse (player.Playing);
			Assert.AreEqual (dr, drSent);
			player.Play ();
			Assert.IsNull (drSent);

			/* Check only drawings for the first camera are loaded */
			dr.CameraConfig = new CameraConfig (1);
			currentTime = eventVM1.Start;
			player.LoadEvent (eventVM1, new Time (0), true);
			Assert.IsTrue (player.Playing);
			currentTime = dr.Render;
			player.Seek (currentTime - eventVM1.Start, true, false);
			Assert.IsTrue (player.Playing);
			Assert.IsNull (drSent);
		}

		[Test ()]
		public void TestMultiplayerCamerasConfig ()
		{
			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (true);
			TimelineEventVM eventVM1;
			RangeObservableCollection<CameraConfig> cams1, cams2;
			Mock<IMultiVideoPlayer> multiplayerMock = new Mock<IMultiVideoPlayer> ();

			mtkMock.Setup (m => m.GetMultiPlayer ()).Returns (multiplayerMock.Object);
			player = new VideoPlayerController ();
			//Should set again the ViewModel
			(player as IController).SetViewModel (playerVM);
			PreparePlayer ();

			/* Only called internally in the openning */
			cams1 = new RangeObservableCollection<CameraConfig> { new CameraConfig (0), new CameraConfig (1) };
			cams2 = new RangeObservableCollection<CameraConfig> { new CameraConfig (1), new CameraConfig (0) };
			multiplayerMock.Verify (p => p.ApplyCamerasConfig (), Times.Never ());
			Assert.AreEqual (cams1, player.CamerasConfig);

			player.CamerasConfig = cams2;
			multiplayerMock.Verify (p => p.ApplyCamerasConfig (), Times.Once ());
			Assert.AreEqual (cams2, player.CamerasConfig);
			multiplayerMock.ResetCalls ();

			/* Now load an event */
			eventVM1 = new TimelineEventVM () {
				Model = new TimelineEvent {
					Start = new Time (100),
					Stop = new Time (200),
					FileSet = mfs,
					CamerasConfig = new RangeObservableCollection<CameraConfig> {
						new CameraConfig (1),
						new CameraConfig (1)
					}
				}
			};
			player.LoadEvent (eventVM1, new Time (0), true);
			multiplayerMock.Verify (p => p.ApplyCamerasConfig (), Times.Once ());
			Assert.AreEqual (eventVM1.CamerasConfig, player.CamerasConfig);
			multiplayerMock.ResetCalls ();

			/* Change event cams config */
			player.CamerasConfig = new RangeObservableCollection<CameraConfig> {
					new CameraConfig (0),
					new CameraConfig (0)
				};
			multiplayerMock.Verify (p => p.ApplyCamerasConfig (), Times.Once ());
			Assert.AreEqual (new List<CameraConfig> { new CameraConfig (0), new CameraConfig (0) }, eventVM1.CamerasConfig);
			Assert.AreEqual (player.CamerasConfig, eventVM1.CamerasConfig);
			multiplayerMock.ResetCalls ();

			/* Unload and check the original cams config is set back*/
			player.UnloadCurrentEvent ();
			multiplayerMock.Verify (p => p.ApplyCamerasConfig (), Times.Once ());
			Assert.AreEqual (cams2, player.CamerasConfig);
			Assert.AreEqual (new List<CameraConfig> { new CameraConfig (0), new CameraConfig (0) }, eventVM1.CamerasConfig);
			multiplayerMock.ResetCalls ();

			/* And changing the config does not affects the unloaded event */
			player.CamerasConfig = cams1;
			multiplayerMock.Verify (p => p.ApplyCamerasConfig (), Times.Once ());
			Assert.AreEqual (new List<CameraConfig> { new CameraConfig (0), new CameraConfig (0) }, eventVM1.CamerasConfig);
			multiplayerMock.ResetCalls ();

			/* Now load a playlist video */
			PlaylistVideo plv = new PlaylistVideo (mfs [0]);
			playlistVM.ChildModels.Add (plv);

			player.LoadPlaylistEvent (playlistVM, new PlaylistVideoVM { Model = plv }, true);
			multiplayerMock.Verify (p => p.ApplyCamerasConfig (), Times.Never ());
			Assert.AreEqual (new List<CameraConfig> { new CameraConfig (0) }, player.CamerasConfig);
			multiplayerMock.ResetCalls ();
			player.UnloadCurrentEvent ();
			/* Called by Open internally () */
			multiplayerMock.Verify (p => p.ApplyCamerasConfig (), Times.Never ());
			Assert.AreEqual (cams2, player.CamerasConfig);
			multiplayerMock.ResetCalls ();

			/* Now load a playlist event and make sure its config is loaded
			 * and not the event's one */
			PlaylistPlayElement ple = new PlaylistPlayElement (eventVM1.Model);
			ple.CamerasConfig = cams2;
			player.LoadPlaylistEvent (playlistVM, new PlaylistPlayElementVM { Model = ple }, true);
			multiplayerMock.Verify (p => p.ApplyCamerasConfig (), Times.Once ());
			Assert.AreEqual (cams2, player.CamerasConfig);
			multiplayerMock.ResetCalls ();

			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (false);
		}

		[Test ()]
		public void TestROICamerasConfig ()
		{
			TimelineEventVM eventVM1;
			RangeObservableCollection<CameraConfig> cams;
			Mock<IMultiVideoPlayer> multiplayerMock = new Mock<IMultiVideoPlayer> ();

			mtkMock.Setup (m => m.GetMultiPlayer ()).Returns (multiplayerMock.Object);
			player = new VideoPlayerController ();
			(player as IController).SetViewModel (playerVM);
			PreparePlayer ();

			/* ROI should be empty */
			Assert.AreEqual (new Area (), player.CamerasConfig [0].RegionOfInterest);

			/* Modify ROI */
			cams = player.CamerasConfig;
			cams [0].RegionOfInterest = new Area (10, 10, 20, 20);
			/* And set */
			player.ApplyROI (cams [0]);

			/* Now create an event with current camera config */
			//Wee need to do a clone here, since now we do not clone in CamerasConfig property getter in videoplayer controller
			eventVM1 = new TimelineEventVM () {
				Model = new TimelineEvent {
					Start = new Time (100),
					Stop = new Time (200),
					FileSet = mfs,
					CamerasConfig = player.CamerasConfig.Clone ()
				}
			};
			/* Check that ROI was copied in event */
			Assert.AreEqual (new Area (10, 10, 20, 20), eventVM1.CamerasConfig [0].RegionOfInterest);

			/* Change ROI again */
			cams [0].RegionOfInterest = new Area (20, 20, 40, 40);
			player.ApplyROI (cams [0]);

			/* Check event was not impacted */
			Assert.AreEqual (new Area (10, 10, 20, 20), eventVM1.CamerasConfig [0].RegionOfInterest);

			/* And load event */
			player.LoadEvent (eventVM1, new Time (0), true);
			Assert.AreEqual (new Area (10, 10, 20, 20), player.CamerasConfig [0].RegionOfInterest);

			/* Unload and check the original cams config is set back*/
			player.UnloadCurrentEvent ();
			Assert.AreEqual (new Area (20, 20, 40, 40), player.CamerasConfig [0].RegionOfInterest);
			/* check the event was not impacted */
			Assert.AreEqual (new Area (10, 10, 20, 20), eventVM1.CamerasConfig [0].RegionOfInterest);
		}

		[Test ()]
		public void TestNonPresentationSeek ()
		{
			PreparePlayer ();

			player.Mode = VideoPlayerOperationMode.Normal;
			player.Seek (new Time (10), true, false, false);
			playerMock.Verify (p => p.Seek (new Time (10), true, false), Times.Once ());
		}

		[Test ()]
		public void TestPresentationSeekToADifferentElement ()
		{
			Playlist localPlaylist = new Playlist ();
			PlaylistPlayElement element0 = new PlaylistPlayElement (eventVM1.Clone ().Model);
			PlaylistPlayElement element = new PlaylistPlayElement (eventVM1.Clone ().Model);
			element0.Play.Start = new Time (10);
			element0.Play.Stop = new Time (20);
			element.Play.Start = new Time (0);
			element.Play.Stop = new Time (10);
			localPlaylist.Elements.Add (element0);
			localPlaylist.Elements.Add (element);
			PreparePlayer ();
			playerMock.ResetCalls ();
			int playlistElementSelected = 0;
			App.Current.EventsBroker.Subscribe<PlaylistElementLoadedEvent> ((e) => playlistElementSelected++);

			var localPlaylistVM = new PlaylistVM { Model = localPlaylist };
			player.LoadPlaylistEvent (localPlaylistVM, new PlaylistPlayElementVM { Model = element0 }, false);
			player.Mode = VideoPlayerOperationMode.Presentation;
			player.Seek (new Time (15), true, false, false);

			// One when the element is loaded, another one when we seek to a time from another element
			Assert.AreEqual (2, playlistElementSelected);
			playerMock.Verify (p => p.Seek (new Time (10), true, false), Times.Once ());
			playerMock.Verify (p => p.Seek (new Time (5), true, false), Times.Once ());
		}

		[Test]
		public void TestPresentationSeekSameElement ()
		{
			Playlist localPlaylist = new Playlist ();
			PlaylistPlayElement element0 = new PlaylistPlayElement (eventVM1.Clone ().Model);
			PlaylistPlayElement element = new PlaylistPlayElement (eventVM1.Clone ().Model);
			element0.Play.Start = new Time (10);
			element0.Play.Stop = new Time (20);
			element.Play.Start = new Time (0);
			element.Play.Stop = new Time (10);
			localPlaylist.Elements.Add (element0);
			localPlaylist.Elements.Add (element);
			PreparePlayer ();
			playerMock.ResetCalls ();
			int playlistElementLoaded = 0;
			App.Current.EventsBroker.Subscribe<LoadPlaylistElementEvent> (
				(e) => playlistElementLoaded++);

			var localPlaylistVM = new PlaylistVM { Model = localPlaylist };
			player.LoadPlaylistEvent (localPlaylistVM, new PlaylistPlayElementVM { Model = element0 }, false);
			player.Mode = VideoPlayerOperationMode.Presentation;
			player.Seek (new Time (5), true, false, false);
			Assert.AreEqual (0, playlistElementLoaded);
			playerMock.Verify (p => p.Seek (new Time (10), true, false), Times.Once ());
			playerMock.Verify (p => p.Seek (new Time (15), true, false), Times.Once ());
		}

		[Test ()]
		public void TestPresentationSeekNoElement ()
		{
			PreparePlayer ();
			Playlist localPlaylist = new Playlist ();
			PlaylistPlayElement element0 = new PlaylistPlayElement (eventVM1.Clone ().Model);
			PlaylistPlayElement element = new PlaylistPlayElement (eventVM1.Clone ().Model);
			element0.Play.Start = new Time (10);
			element0.Play.Stop = new Time (20);
			element.Play.Start = new Time (0);
			element.Play.Stop = new Time (10);
			localPlaylist.Elements.Add (element0);
			localPlaylist.Elements.Add (element);
			var localPlaylistVM = new PlaylistVM { Model = localPlaylist };
			player.Switch (null, localPlaylistVM, new PlaylistPlayElementVM { Model = element0 });
			playerMock.ResetCalls ();

			int playlistElementSelected = 0;
			App.Current.EventsBroker.Subscribe<LoadPlaylistElementEvent> ((e) => playlistElementSelected++);

			player.Mode = VideoPlayerOperationMode.Presentation;
			Assert.IsFalse (player.Seek (new Time (5000), true, false, false));
			Assert.AreEqual (0, playlistElementSelected);
			playerMock.Verify (p => p.Seek (It.IsAny<Time> (), It.IsAny<bool> (), It.IsAny<bool> ()), Times.Never ());
		}

		[Test ()]
		public void TestPresentationSeekLongerThanFileset ()
		{
			Time seekTime = new Time { TotalSeconds = 51000 };
			Assert.Greater (seekTime, mfs.Duration);
			PreparePlayer ();
			Playlist localPlaylist = new Playlist ();
			PlaylistPlayElement element0 = new PlaylistPlayElement (eventVM1.Clone ().Model);
			element0.Play.Start = new Time (0);
			element0.Play.Stop = new Time { TotalSeconds = 6000 };
			localPlaylist.Elements.Add (element0);
			var localPlaylistVM = new PlaylistVM { Model = localPlaylist };
			player.Switch (null, localPlaylistVM, new PlaylistPlayElementVM { Model = element0 });
			playerMock.ResetCalls ();

			int playlistElementSelected = 0;
			App.Current.EventsBroker.Subscribe<LoadPlaylistElementEvent> ((e) => playlistElementSelected++);

			player.Mode = VideoPlayerOperationMode.Presentation;
			Assert.IsFalse (player.Seek (seekTime, true, false, false));
			Assert.AreEqual (0, playlistElementSelected);
			playerMock.Verify (p => p.Seek (It.IsAny<Time> (), It.IsAny<bool> (), It.IsAny<bool> ()), Times.Never ());
		}

		[Test ()]
		public void TestLoadVideoPlaying ()
		{
			player.ElementLoadedEvent += HandleElementLoadedEvent;

			PreparePlayer ();
			playerMock.ResetCalls ();
			Playlist localPlaylist = new Playlist ();
			IPlaylistElement element0 = new PlaylistVideo (mfs [0]);
			localPlaylist.Elements.Add (element0);
			var localPlaylistVM = new PlaylistVM { Model = localPlaylist };
			player.LoadPlaylistEvent (localPlaylistVM, new PlaylistVideoVM { Model = element0 }, true);

			Assert.AreEqual (1, elementLoaded);
			Assert.IsTrue (player.Playing);
			player.ElementLoadedEvent -= HandleElementLoadedEvent;
		}

		[Test ()]
		public void TestLoadVideoNotPlaying ()
		{
			player.ElementLoadedEvent += HandleElementLoadedEvent;

			PreparePlayer ();
			playerMock.ResetCalls ();
			Playlist localPlaylist = new Playlist ();
			IPlaylistElement element0 = new PlaylistVideo (mfs [0]);
			localPlaylist.Elements.Add (element0);
			var localPlaylistVM = new PlaylistVM { Model = localPlaylist };
			player.LoadPlaylistEvent (localPlaylistVM, new PlaylistVideoVM { Model = element0 }, false);

			Assert.AreEqual (1, elementLoaded);
			Assert.IsFalse (player.Playing);
			player.ElementLoadedEvent -= HandleElementLoadedEvent;
		}

		[Test ()]
		public void TestPlayerControllerRefreshMediaFileSetWhenItChanges ()
		{
			PreparePlayer ();
			playerMock.ResetCalls ();
			Playlist localPlaylist = new Playlist ();
			PlaylistPlayElement element0 = new PlaylistPlayElement (eventVM1.Model);
			localPlaylist.Elements.Add (element0);
			var localPlaylistVM = new PlaylistVM { Model = localPlaylist };

			player.LoadPlaylistEvent (localPlaylistVM, new PlaylistPlayElementVM { Model = element0 }, false);

			Assert.AreEqual (eventVM1.FileSet, player.FileSet);

			var fileSet = eventVM1.FileSet.Clone ();
			fileSet [0].FilePath = "test3";
			fileSet.ID = Guid.NewGuid ();
			eventVM1.FileSet = fileSet;

			Assert.IsTrue (player.FileSet.CheckMediaFilesModified (fileSet));
			player.LoadPlaylistEvent (localPlaylistVM, new PlaylistPlayElementVM { Model = element0 }, false);

			playerMock.Verify (prop => prop.Open (eventVM1.FileSet [0]), Times.Once);
			Assert.AreEqual (player.FileSet [0].FilePath, eventVM1.FileSet [0].FilePath);
			Assert.IsFalse (player.FileSet.CheckMediaFilesModified (eventVM1.FileSet));

		}

		[Test ()]
		public void TestLoadEventWithoutCamerasConfig ()
		{
			try {
				PreparePlayer ();
				player.LoadEvent (eventVM3, new Time (0), true);
			} catch {
				Assert.Fail ("PlaylistController raised exception in LoadEvent");
			}
		}

		[Test]
		public void TestSeekStretchMode ()
		{
			PreparePlayer ();
			currentTime = new Time (2100);
			player.FileSet.VisibleRegion.Start = new Time (2000);
			player.FileSet.VisibleRegion.Stop = new Time (5000);

			player.Mode = VideoPlayerOperationMode.Stretched;
			player.Seek (new Time (1000), true);

			// First seek when the mode is changed
			playerMock.Verify (p => p.Seek (new Time (2100), true, false), Times.Once ());
			// Second seek to the new position + VisibleRegion.Start
			playerMock.Verify (p => p.Seek (new Time (3000), true, false), Times.Once ());
		}

		[Test]
		public void TestUnloadEventInStretchMode ()
		{
			PreparePlayer ();
			currentTime = new Time (5000);
			player.FileSet.VisibleRegion.Start = new Time (2000);
			player.FileSet.VisibleRegion.Stop = new Time (15000);

			player.Mode = VideoPlayerOperationMode.Stretched;
			player.LoadEvent (eventVM2, new Time (0), false);
			currentTime = new Time (7000);
			playerMock.ResetCalls ();
			player.UnloadCurrentEvent ();
			player.Seek (new Time (1000), true);

			// Check the first seek to current time
			playerMock.Verify (p => p.Seek (currentTime, true, false), Times.Once ());
			// Check the second seek to 1000 + VisibleRegion.Start seek time
			playerMock.Verify (p => p.Seek (new Time (3000), true, false), Times.Once ());
		}

		[Test]
		public void TestUnloadEventInStretchModeWhenCurrentTimeIsOutsideTheVisibleRegion ()
		{
			PreparePlayer ();
			currentTime = new Time (5000);
			player.FileSet.VisibleRegion.Start = new Time (2000);
			player.FileSet.VisibleRegion.Stop = new Time (5000);

			player.Mode = VideoPlayerOperationMode.Stretched;
			player.LoadEvent (eventVM2, new Time (0), false);
			currentTime = new Time (7000);
			playerMock.ResetCalls ();
			player.UnloadCurrentEvent ();

			// Check the first seek to current time
			playerMock.Verify (p => p.Seek (new Time (5000), true, false), Times.Once ());
			Assert.AreEqual (new Time (3000), playerVM.Duration);
			Assert.AreEqual (new Time (3000), playerVM.AbsoluteDuration);
			Assert.AreEqual (new Time (3000), playerVM.CurrentTime);
		}

		[Test ()]
		public void TestTimeStretchMode ()
		{
			Time curTime = null, relativeTime = null;
			PreparePlayer ();
			currentTime = new Time (7000);
			player.FileSet.VisibleRegion.Start = new Time (2000);
			player.FileSet.VisibleRegion.Stop = new Time (15000);
			App.Current.EventsBroker.Subscribe<PlayerTickEvent> ((obj) => {
				curTime = obj.Time;
				relativeTime = obj.RelativeTime;
			});

			player.Mode = VideoPlayerOperationMode.Stretched;

			Assert.AreEqual (new Time (5000), curTime);
			Assert.AreEqual (new Time (5000), relativeTime);
			Assert.AreEqual (new Time (13000), playerVM.Duration);
			Assert.AreEqual (new Time (13000), playerVM.AbsoluteDuration);
			Assert.AreEqual (new Time (5000), playerVM.CurrentTime);
		}

		[Test]
		public void TestDurationUpdatedWhenPresentationChanged ()
		{
			Playlist localPlaylist = new Playlist ();
			PlaylistPlayElement element1 = new PlaylistPlayElement (eventVM1.Clone ().Model);
			PlaylistPlayElement element2 = new PlaylistPlayElement (eventVM1.Clone ().Model);
			element1.Play.Start = new Time (10);
			element1.Play.Stop = new Time (20);
			element2.Play.Start = new Time (0);
			element2.Play.Stop = new Time (10);
			localPlaylist.Elements.Add (element1);
			localPlaylist.Elements.Add (element2);
			PreparePlayer ();
			playerMock.ResetCalls ();
			var localPlaylistVM = new PlaylistVM { Model = localPlaylist };
			player.LoadPlaylistEvent (localPlaylistVM, new PlaylistPlayElementVM { Model = element1 }, false);
			player.Mode = VideoPlayerOperationMode.Presentation;

			PlaylistPlayElement element3 = new PlaylistPlayElement (eventVM1.Clone ().Model);
			element3.Play.Start = new Time (0);
			element3.Play.Stop = new Time (10);
			localPlaylist.Elements.Add (element3);

			Assert.AreEqual (new Time (30), playerVM.Duration);
			Assert.AreEqual (new Time (30), playerVM.AbsoluteDuration);
		}

		[Test]
		public void TestDurationUpdatedWhenPresentationElementChanged ()
		{
			Playlist localPlaylist = new Playlist ();
			PlaylistPlayElement element1 = new PlaylistPlayElement (eventVM1.Clone ().Model);
			PlaylistPlayElement element2 = new PlaylistPlayElement (eventVM1.Clone ().Model);
			element1.Play.Start = new Time (10);
			element1.Play.Stop = new Time (20);
			element2.Play.Start = new Time (0);
			element2.Play.Stop = new Time (10);
			localPlaylist.Elements.Add (element1);
			localPlaylist.Elements.Add (element2);
			PreparePlayer ();
			playerMock.ResetCalls ();
			var localPlaylistVM = new PlaylistVM { Model = localPlaylist };
			player.LoadPlaylistEvent (localPlaylistVM, new PlaylistPlayElementVM { Model = element1 }, false);
			player.Mode = VideoPlayerOperationMode.Presentation;

			element2.Play.Stop = new Time (20);

			Assert.AreEqual (new Time (30), playerVM.Duration);
			Assert.AreEqual (new Time (30), playerVM.AbsoluteDuration);
		}

		[Test]
		public void SetZoom_InBoundaries_ZoomChanged ()
		{
			PreparePlayer ();
			player.SetZoom (2);

			Assert.AreEqual (2, playerVM.Zoom);
		}

		[Test]
		public void SetZoom_OutLowerBoundary_ZoomNotChanged ()
		{
			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (true);
			PreparePlayer ();
			player.SetZoom (0);

			Assert.AreEqual (1, playerVM.Zoom);
			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (false);
		}

		[Test]
		public void SetZoom_OutHigherBoundary_ZoomNotChanged ()
		{
			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (true);
			PreparePlayer ();
			player.SetZoom (8);
			Assert.AreEqual (1, playerVM.Zoom);
			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (false);
		}

		[Test]
		public void MoveROI_InBoundaries_ROIUpdated ()
		{
			PreparePlayer ();
			var camConfig = player.CamerasConfig [0];
			var oldROI = camConfig.RegionOfInterest = new Area (0, 0, 200, 100);
			player.ApplyROI (camConfig);
			player.MoveROI (new Point (-10, -20));
			var newROI = player.CamerasConfig [0].RegionOfInterest;

			Assert.AreEqual (oldROI.Start.X + 10, newROI.Start.X);
			Assert.AreEqual (oldROI.Start.Y + 20, newROI.Start.Y);
		}

		[Test]
		public void MoveROI_OutBoundaries_ROICliped ()
		{
			PreparePlayer ();
			var camConfig = player.CamerasConfig [0];
			var oldROI = camConfig.RegionOfInterest = new Area (0, 0, 200, 100);
			player.ApplyROI (camConfig);
			player.MoveROI (new Point (10, 20));
			var newROI = player.CamerasConfig [0].RegionOfInterest;

			Assert.AreEqual (oldROI.Start.X, newROI.Start.X);
			Assert.AreEqual (oldROI.Start.Y, newROI.Start.Y);
		}

		[Test]
		public void OpenVideo_ControlsSensitiveUpdated ()
		{
			PreparePlayer ();
			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (true);
			playerVM.ControlsSensitive = false;

			player.Open (mfs);

			Assert.IsTrue (playerVM.ControlsSensitive);
		}

		[Test]
		public void LoadEvent_ControlsSensitiveUpdated ()
		{
			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (true);
			PreparePlayer ();
			playerVM.ControlsSensitive = false;
			var mfsNew = new MediaFileSet ();
			mfsNew.Add (new MediaFile {
				FilePath = "test2",
				VideoWidth = 320,
				VideoHeight = 240,
				Par = 1,
				Duration = new Time { TotalSeconds = 5000 }
			});
			eventVM1.FileSet = mfsNew;

			player.LoadEvent (eventVM1, currentTime, true);

			Assert.IsTrue (playerVM.ControlsSensitive);
		}

		[Test]
		public void LoadPlaylistVideo_ControlsSensitiveUpdated ()
		{
			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (true);
			PreparePlayer ();
			playerVM.ControlsSensitive = false;
			var mfsNew = new MediaFileSet ();
			mfsNew.Add (new MediaFile {
				FilePath = "test2",
				VideoWidth = 320,
				VideoHeight = 240,
				Par = 1,
				Duration = new Time { TotalSeconds = 5000 }
			});
			PlaylistVideo vid = new PlaylistVideo (mfsNew [0]);
			player.LoadPlaylistEvent (playlistVM, new PlaylistVideoVM { Model = vid }, true);

			Assert.IsTrue (playerVM.ControlsSensitive);
		}

		[Test]
		public void OpenVideo_PlayerVMFileSetUpdated ()
		{
			MediaFileSetVM fileset = null;
			int calls = 0;
			PreparePlayer ();
			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (true);
			playerVM.ControlsSensitive = false;
			playerVM.PropertyChanged += (sender, e) => {
				if (e.PropertyName == nameof (playerVM.FileSet)) {
					fileset = playerVM.FileSet;
					calls++;
				}
			};

			player.Open (mfs);

			Assert.AreEqual (1, calls);
			Assert.AreSame (mfs, fileset.Model);
		}

		[Test]
		public void LoadEvent_PlayerVMFileSetUpdated ()
		{
			MediaFileSetVM fileset = null;
			int calls = 0;
			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (true);
			PreparePlayer ();
			playerVM.ControlsSensitive = false;
			var mfsNew = new MediaFileSet ();
			mfsNew.Add (new MediaFile {
				FilePath = "test2",
				VideoWidth = 320,
				VideoHeight = 240,
				Par = 1,
				Duration = new Time { TotalSeconds = 5000 }
			});
			eventVM1.FileSet = mfsNew;
			playerVM.PropertyChanged += (sender, e) => {
				if (e.PropertyName == nameof (playerVM.FileSet)) {
					fileset = playerVM.FileSet;
					calls++;
				}
			};

			player.LoadEvent (eventVM1, currentTime, true);

			Assert.AreEqual (1, calls);
			Assert.AreSame (mfsNew, fileset.Model);
		}

		[Test]
		public void LoadEvent_StartMoved_SeekToNewPosition ()
		{
			var playerEvent = eventVM1;
			PreparePlayer ();
			player.LoadEvent (playerEvent, playerEvent.Start, true);
			playerMock.ResetCalls ();

			playerEvent.Start += 1000;

			playerMock.Verify (p => p.Seek (playerEvent.Start, true, false));
		}

		[Test]
		public void LoadEvent_StopMoved_SeekToNewPosition ()
		{
			PreparePlayer ();
			player.LoadEvent (eventVM1, eventVM1.Start, true);
			playerMock.ResetCalls ();

			eventVM1.Stop += 1000;

			playerMock.Verify (p => p.Seek (eventVM1.Stop, true, false));
		}

		[Test]
		public void LoadPlaylistEvent_IsPlaylistPlayElementAndStartMoved_SeekToNewPosition ()
		{
			PreparePlayer ();
			var playlistElement = playlistVM.ChildModels [0] as PlaylistPlayElement;

			player.LoadPlaylistEvent (playlistVM, playlistVM.ViewModels [0], true);
			playerMock.ResetCalls ();

			playlistElement.Start += 1000;

			playerMock.Verify (p => p.Seek (playlistElement.Start, true, false));
		}

		[Test]
		public void LoadPlaylistEvent_IsPlaylistPlayElementAndStopMoved_SeekToNewPosition ()
		{
			PreparePlayer ();
			var playlistElement = playlistVM.ChildModels [0] as PlaylistPlayElement;

			player.LoadPlaylistEvent (playlistVM, playlistVM.ViewModels [0], true);
			playerMock.ResetCalls ();

			playlistElement.Stop += 1000;

			playerMock.Verify (p => p.Seek (playlistElement.Stop, true, false));
		}

		[Test]
		public void LoadPlaylistVideo_PlayerVMFileSetUpdated ()
		{
			MediaFileSetVM fileset = null;
			int calls = 0;
			fileManager.Setup (f => f.Exists (It.IsAny<string> ())).Returns (true);
			PreparePlayer ();
			playerVM.ControlsSensitive = false;
			var mfsNew = new MediaFileSet ();
			mfsNew.Add (new MediaFile {
				FilePath = "test2",
				VideoWidth = 320,
				VideoHeight = 240,
				Par = 1,
				Duration = new Time { TotalSeconds = 5000 }
			});
			PlaylistVideo vid = new PlaylistVideo (mfsNew [0]);
			playerVM.PropertyChanged += (sender, e) => {
				if (e.PropertyName == nameof (playerVM.FileSet)) {
					fileset = playerVM.FileSet;
					calls++;
				}
			};

			player.LoadPlaylistEvent (playlistVM, new PlaylistVideoVM { Model = vid }, true);

			Assert.AreEqual (1, calls);
			Assert.AreSame (mfsNew [0], fileset.Model [0]);
		}

		[Test ()]
		public void LoadZoomEvent_OpenZoomNotLimited_ApplyZoom ()
		{
			mockLimitationService.Setup (x => x.CanExecute (VASFeature.OpenZoom.ToString ())).
								 Returns (true);
			PreparePlayer ();
			eventVM1.CamerasConfig [0].RegionOfInterest = new Area (0, 0, eventVM1.FileSet [0].VideoWidth / 2, 10);

			player.LoadEvent (eventVM1, new Time (0), true);

			Assert.AreEqual (2.0, playerVM.Zoom);
		}

		[Test ()]
		public void LoadZoomEvent_OpenZoomLimited_DoNotApplyZoom ()
		{
			mockLimitationService.Setup (x => x.CanExecute (VASFeature.OpenZoom.ToString ())).
								 Returns (false);
			PreparePlayer ();
			eventVM1.CamerasConfig [0].RegionOfInterest = new Area (0, 0, 10, 10);

			player.LoadEvent (eventVM1, new Time (0), true);

			Assert.AreEqual (1.0, playerVM.Zoom);
		}

		[Test]
		public void SetEditEventDuration_NoEventLoaded_EditEventDurationEnabledAndEditEventDurationTimeNodeUpdated ()
		{
			PreparePlayer ();
			var start = new Time { TotalSeconds = 60 };
			var stop = new Time { TotalSeconds = 100 };
			var eventVMLocal = new TimelineEventVM () { Model = new TimelineEvent { Start = start, Stop = stop } };
			player.LoadEvent (eventVMLocal, start, true);

			player.SetEditEventDurationMode (true);

			Assert.IsTrue (playerVM.EditEventDurationCommand.CanExecute ());
			Assert.IsTrue (playerVM.EditEventDurationModeEnabled);
			Assert.AreEqual (start - new Time { TotalSeconds = 10 }, playerVM.EditEventDurationTimeNode.Start);
			Assert.AreEqual (stop + new Time { TotalSeconds = 10 }, playerVM.EditEventDurationTimeNode.Stop);
		}

		[Test]
		public void SetEditEventDuration_StartIsLessThan10Seconds_EditEventDurationTimeNodeStartIsClipped ()
		{
			PreparePlayer ();
			var start = new Time { TotalSeconds = 8 };
			var stop = new Time { TotalSeconds = 10 };
			var eventVM = new TimelineEventVM () { Model = new TimelineEvent { Start = start, Stop = stop } };
			player.LoadEvent (eventVM, start, true);

			player.SetEditEventDurationMode (true);

			Assert.IsTrue (playerVM.EditEventDurationCommand.CanExecute ());
			Assert.IsTrue (playerVM.EditEventDurationModeEnabled);
			Assert.AreEqual (new Time (0), playerVM.EditEventDurationTimeNode.Start);
			Assert.AreEqual (stop + new Time { TotalSeconds = 10 }, playerVM.EditEventDurationTimeNode.Stop);
		}

		[Test]
		public void SetEditEventDuration_StopIs5SecondsAwayFromEOF_EditEventDurationTimeNodeStopIsClipped ()
		{
			PreparePlayer ();
			var start = new Time { TotalSeconds = 20 };
			var stop = playerVM.FileSet.Duration - 5;
			var eventVM = new TimelineEventVM () { Model = new TimelineEvent { Start = start, Stop = stop } };
			player.LoadEvent (eventVM, start, true);

			player.SetEditEventDurationMode (true);

			Assert.IsTrue (playerVM.EditEventDurationCommand.CanExecute ());
			Assert.IsTrue (playerVM.EditEventDurationModeEnabled);
			Assert.AreEqual (start - new Time { TotalSeconds = 10 }, playerVM.EditEventDurationTimeNode.Start);
			Assert.AreEqual (playerVM.FileSet.Duration, playerVM.EditEventDurationTimeNode.Stop);
		}

		[Test]
		public void SetEditEventDuration_EventAlreadyLoaded_EditEventDurationModeDisabled ()
		{
			PreparePlayer ();
			var start1 = new Time { TotalSeconds = 60 };
			var stop1 = new Time { TotalSeconds = 100 };
			var start2 = new Time { TotalSeconds = 200 };
			var stop2 = new Time { TotalSeconds = 230 };

			var eventVMLocal1 = new TimelineEventVM () { Model = new TimelineEvent { Start = start1, Stop = stop1 } };
			var eventVMLocal2 = new TimelineEventVM () { Model = new TimelineEvent { Start = start2, Stop = stop2 } };

			player.LoadEvent (eventVMLocal1, start1, true);
			player.SetEditEventDurationMode (true);
			player.LoadEvent (eventVMLocal2, start2, true);

			Assert.IsTrue (playerVM.EditEventDurationCommand.CanExecute ());
			Assert.IsFalse (playerVM.EditEventDurationModeEnabled);
		}

		[Test]
		public void SetEditEventDurationMode_EventLoadedNewEventIsNull_EditEventDurationCommandNotExecutable ()
		{
			PreparePlayer ();
			player.LoadEvent (eventVM1, eventVM1.Start, true);
			player.UnloadCurrentEvent ();

			player.SetEditEventDurationMode (true);

			Assert.IsFalse (playerVM.EditEventDurationCommand.CanExecute ());
			Assert.IsFalse (playerVM.EditEventDurationModeEnabled);
			Assert.IsNull (playerVM.EditEventDurationTimeNode.Model);
		}

		[Test]
		public void SetEditEventDurationMode_DisableModeWithEventLoaded_ModeDisabledNodeUpdated ()
		{
			PreparePlayer ();
			player.LoadEvent (eventVM1, eventVM1.Start, true);
			player.SetEditEventDurationMode (true);

			player.SetEditEventDurationMode (false);

			Assert.IsTrue (playerVM.EditEventDurationCommand.CanExecute ());
			Assert.IsFalse (playerVM.EditEventDurationModeEnabled);
			Assert.IsNull (playerVM.EditEventDurationTimeNode.Model);
		}

		[Test]
		public void SetEditEventDurationMode_EventLoadedAndDurationChanged_EditEventDurationTimeNodeRecalculated ()
		{
			PreparePlayer ();
			var start = new Time { TotalSeconds = 60 };
			var stop = new Time { TotalSeconds = 100 };
			var eventVM = new TimelineEventVM () { Model = new TimelineEvent { Start = start, Stop = stop } };
			player.LoadEvent (eventVM, start, true);
			player.SetEditEventDurationMode (true);

			eventVM.Stop = new Time { TotalSeconds = 120 };
			player.SetEditEventDurationMode (true);

			Assert.IsTrue (playerVM.EditEventDurationCommand.CanExecute ());
			Assert.IsTrue (playerVM.EditEventDurationModeEnabled);
			Assert.AreEqual (start - new Time { TotalSeconds = 10 }, playerVM.EditEventDurationTimeNode.Start);
			Assert.AreEqual (eventVM.Stop + new Time { TotalSeconds = 10 }, playerVM.EditEventDurationTimeNode.Stop);
		}

		[Test]
		public void SetEditEventDurationMode_PlaylistTimelineElementLoaded_EditDurationModeExecutableAndTimeNodeUpdate ()
		{
			PreparePlayer ();
			var start = new Time { TotalSeconds = 60 };
			var stop = new Time { TotalSeconds = 100 };
			var evt = new TimelineEvent { Start = start, Stop = stop };
			var plEvt = new PlaylistPlayElement (evt);
			var playlist = new Playlist ();
			playlist.Elements.Add ((plEvt));
			var playlistVM = new PlaylistVM { Model = playlist };
			player.LoadPlaylistEvent (playlistVM, new PlaylistPlayElementVM { Model = plEvt }, true);

			player.SetEditEventDurationMode (true);

			Assert.IsTrue (playerVM.EditEventDurationCommand.CanExecute ());
			Assert.IsTrue (playerVM.EditEventDurationModeEnabled);
			Assert.AreEqual (start - new Time { TotalSeconds = 10 }, playerVM.EditEventDurationTimeNode.Start);
			Assert.AreEqual (stop + new Time { TotalSeconds = 10 }, playerVM.EditEventDurationTimeNode.Stop);
		}

		[Test]
		public void SetEditEventDurationMode_PlaylistImageLoaded_EditDurationModeNotExecutable ()
		{
			PreparePlayer ();
			var playlist = new Playlist ();
			playlist.Elements.Add (new PlaylistImage (Utils.LoadImageFromFile (), new Time (5000)));
			var playlistVM = new PlaylistVM { Model = playlist };
			player.LoadPlaylistEvent (playlistVM, new PlaylistImageVM { Model = playlist.Elements [0] }, true);

			player.SetEditEventDurationMode (true);

			Assert.IsFalse (playerVM.EditEventDurationCommand.CanExecute ());
			Assert.IsFalse (playerVM.EditEventDurationModeEnabled);
		}

		[Test]
		public void SetEditEventDurationMode_PlaylistPlayElementLoaded_SeekOutsideBoundariesDoesNotCallsNext ()
		{
			PreparePlayer ();

			player.LoadPlaylistEvent (playlistVM, playlistVM.ViewModels [0], true);

			player.SetEditEventDurationMode (true);
			var playlistElement = playlistVM.ChildModels [0] as PlaylistPlayElement;

			currentTime = playlistElement.Stop + new Time { TotalSeconds = 20 };
			// This will trigger an AbsoluteSeek which will trigger a Tick where we want to check that
			// it does not jump to the next element
			playlistElement.Stop += new Time { TotalSeconds = 1 };

			Assert.AreEqual (0, playlistVM.CurrentIndex);
		}

		[Test]
		public void HandlePlaylistEventDrawingsCollectionChanged_VideoPlayerControllerDrawingsCollectionLoaded_SeekInvokedFrameDrawingSetted ()
		{
			//Arrange
			var start = new Time { TotalSeconds = 60 };
			var renderOffsetTime = new Time (80);
			var renderTime = start + renderOffsetTime;
			currentTime = renderTime;
			PreparePlayer ();

			var stop = new Time { TotalSeconds = 100 };
			var eventVMLocal = new TimelineEventVM () { Model = new TimelineEvent { Start = start, Stop = stop } };
			var frameDrawing = new FrameDrawing () {
				Render = renderTime
			};
			player.LoadEvent (eventVMLocal, renderOffsetTime, true);
			playerMock.ResetCalls ();

			//Act
			eventVMLocal.Drawings.Add (frameDrawing);
			currentTime = new Time (120);
			timerMock.Raise (t => t.Elapsed += null, new EventArgs () as ElapsedEventArgs);

			//Assert
			playerMock.Verify (v => v.Pause (false), Times.AtLeastOnce ());
			Assert.AreEqual (frameDrawing, playerVM.FrameDrawing);
			playerMock.Verify (v => v.Seek (frameDrawing.Render, true, true), Times.Once ());
		}

		[Test]
		public void LoadPlaylistPlayElement_ReadyCalledWithoutCamerasConfig_CamerasConfigFilled ()
		{
			player.CamerasConfig = null;
			viewPortMock = new Mock<IViewPort> ();
			viewPortMock.SetupAllProperties ();
			player.ViewPorts = new List<IViewPort> { viewPortMock.Object, viewPortMock.Object };
			playerMock.Raise (p => p.ReadyToSeek += null, this);

			var eventCamerasConfig = new RangeObservableCollection<CameraConfig> { new CameraConfig (0) };
			var eventCamerasLayout = new object ();

			var playlistVM = new PlaylistVM { Model = new Playlist () };


			PlaylistPlayElementVM playlistPlayVM = new PlaylistPlayElementVM {
				Model = new PlaylistPlayElement (new TimelineEvent ()) {
					CamerasConfig = eventCamerasConfig,
					CamerasLayout = eventCamerasLayout
				}
			};

			player.LoadPlaylistEvent (playlistVM, playlistPlayVM, false);

			Assert.AreEqual (eventCamerasConfig, player.CamerasConfig);
			Assert.AreEqual (eventCamerasLayout, player.CamerasLayout);
		}

		[Test]
		public void LoadPlaylistVideoElement_ReadyCalledWithoutCamerasConfig_CamerasConfigFilled ()
		{
			player.CamerasConfig = null;
			viewPortMock = new Mock<IViewPort> ();
			viewPortMock.SetupAllProperties ();
			player.ViewPorts = new List<IViewPort> { viewPortMock.Object, viewPortMock.Object };
			playerMock.Raise (p => p.ReadyToSeek += null, this);

			var eventCamerasConfig = new RangeObservableCollection<CameraConfig> { new CameraConfig (0) };
			var playlistVM = new PlaylistVM { Model = new Playlist () };
			player.LoadPlaylistEvent (playlistVM, new PlaylistVideoVM { Model = new PlaylistVideo (new MediaFile ()) }, false);

			Assert.AreEqual (eventCamerasConfig, player.CamerasConfig);
			Assert.AreEqual (null, player.CamerasLayout);
		}

		[Test]
		public void LoadPlaylistImageElement_ReadyCalledWithoutCamerasConfig_CamerasConfigFilled ()
		{
			player.CamerasConfig = null;
			viewPortMock = new Mock<IViewPort> ();
			viewPortMock.SetupAllProperties ();
			player.ViewPorts = new List<IViewPort> { viewPortMock.Object, viewPortMock.Object };
			playerMock.Raise (p => p.ReadyToSeek += null, this);

			var eventCamerasConfig = new RangeObservableCollection<CameraConfig> { new CameraConfig (0) };
			var playlistVM = new PlaylistVM { Model = new Playlist () };
			player.LoadPlaylistEvent (playlistVM, new PlaylistImageVM {
				Model = new PlaylistImage (App.Current.ResourcesLocator.LoadImage ("name", 1, 1), new Time (10))
			}, false);

			Assert.AreEqual (eventCamerasConfig, player.CamerasConfig);
			Assert.AreEqual (null, player.CamerasLayout);
		}

		[Test]
		public void LoadPlaylistFrameDrawingElement_ReadyCalledWithoutCamerasConfig_CamerasConfigFilled ()
		{
			player.CamerasConfig = null;
			viewPortMock = new Mock<IViewPort> ();
			viewPortMock.SetupAllProperties ();
			player.ViewPorts = new List<IViewPort> { viewPortMock.Object, viewPortMock.Object };
			playerMock.Raise (p => p.ReadyToSeek += null, this);

			var eventCamerasConfig = new RangeObservableCollection<CameraConfig> { new CameraConfig (0) };
			var playlistVM = new PlaylistVM { Model = new Playlist () };
			player.LoadPlaylistEvent (playlistVM, new PlaylistDrawingVM {
				Model = new PlaylistDrawing (new FrameDrawing ())
			}, false);

			Assert.AreEqual (eventCamerasConfig, player.CamerasConfig);
			Assert.AreEqual (null, player.CamerasLayout);
		}

		[Test]
		public void LoadEvent_ReadyCalledWithoutCamerasConfig_CamerasConfigFilled ()
		{
			player.CamerasConfig = null;
			viewPortMock = new Mock<IViewPort> ();
			viewPortMock.SetupAllProperties ();
			player.ViewPorts = new List<IViewPort> { viewPortMock.Object, viewPortMock.Object };
			playerMock.Raise (p => p.ReadyToSeek += null, this);

			var eventCamerasConfig = new RangeObservableCollection<CameraConfig> { new CameraConfig (0) };
			player.LoadEvent (new TimelineEventVM () { Model = new TimelineEvent () }, new Time (0), false);

			Assert.AreEqual (eventCamerasConfig, player.CamerasConfig);
			Assert.AreEqual (null, player.CamerasLayout);
		}

		[Test ()]
		public void LoadPlaylistEvent_LoadedPlaylistAndFrameDrawing_FrameDrawingisNull ()
		{
			///Arrange

			PreparePlayer ();
			IPlaylistElement element = new PlaylistImage (new Image (1, 1), new Time (100));
			PlaylistDrawingVM elementVM = new PlaylistDrawingVM { Model = element };

			eventVM1.Drawings.Add (new FrameDrawing () {
				Pause = new Time (100)
			});

			PlaylistPlayElementVM playlistPlayVM = new PlaylistPlayElementVM {
				Model = new PlaylistPlayElement (eventVM1.Model)
			};

			playlistVM.ViewModels.Clear ();
			playlistVM.ViewModels.Add (new PlaylistImageVM { Model = element });
			playlistVM.ViewModels.Add (playlistPlayVM);
			playerVM.FrameDrawing = new FrameDrawing ();

			///Act

			player.LoadPlaylistEvent (playlistVM, elementVM, false);

			///Assert

			Assert.IsNull (playerVM.FrameDrawing);
		}

		void HandleElementLoadedEvent (object element, bool hasNext)
		{
			elementLoaded++;
		}
	}
}

