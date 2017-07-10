//
//  Copyright (C) 2017 Fluendo S.A.
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
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Drawing.Widgets;

namespace VAS.Tests.Drawing.Widgets
{
	[TestFixture]
	public class TestTimerule
	{
		Timerule timerule;
		VideoPlayerVM videoPlayer;
		Mock<IWidget> widgetMock;
		Mock<IVideoPlayerController> videoPlayerMock;

		[SetUp]
		public void Setup ()
		{
			var drawingToolkitMock = new Mock<IDrawingToolkit> ();
			drawingToolkitMock.Setup (d => d.CreateSurfaceFromResource (It.IsAny<string> (), It.IsAny<bool> (), It.IsAny<bool> ())).
							  Returns (Mock.Of<ISurface> ());
			App.Current.DrawingToolkit = drawingToolkitMock.Object;
			widgetMock = new Mock<IWidget> ();
			timerule = new Timerule (widgetMock.Object);
			videoPlayer = new VideoPlayerVM ();
			timerule.SetViewModel (videoPlayer);
			videoPlayerMock = new Mock<IVideoPlayerController> ();
			videoPlayer.Player = videoPlayerMock.Object;
		}

		[Test]
		public void TestAutoUpdateEnabled ()
		{
			timerule.AutoUpdate = true;
			videoPlayer.CurrentTime = new Time (5000);

			Assert.AreEqual (new Time (5000), timerule.CurrentTime);
		}

		[Test]
		public void TestAutoUpdateDisabled ()
		{
			timerule.AutoUpdate = false;
			videoPlayer.CurrentTime = new Time (5000);

			Assert.AreEqual (new Time (0), timerule.CurrentTime);
		}

		[Test]
		public void StartSeek_UseAbsoluteDuration_EventUnloaded ()
		{
			timerule.UseAbsoluteDuration = true;

			widgetMock.Raise ((obj) => obj.ButtonPressEvent += null,
							  new Point (0, 0), (uint)0, ButtonType.Left, ButtonModifier.None,
							  ButtonRepetition.Single);

			videoPlayerMock.Verify (p => p.UnloadCurrentEvent (), Times.Once ());
		}

		[Test]
		public void StartSeek_NotUseAbsoluteDuration_EventNotUnloaded ()
		{
			timerule.UseAbsoluteDuration = false;

			widgetMock.Raise ((obj) => obj.ButtonPressEvent += null,
							  new Point (0, 0), (uint)0, ButtonType.Left, ButtonModifier.None,
							  ButtonRepetition.Single);

			videoPlayerMock.Verify (p => p.Pause (false), Times.Once ());
			videoPlayerMock.Verify (p => p.UnloadCurrentEvent (), Times.Never ());
		}
	}
}
