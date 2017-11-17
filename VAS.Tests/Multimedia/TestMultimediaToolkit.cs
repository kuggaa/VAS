//
//  Copyright (C) 2017 FLUENDO S.A.
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
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Handlers;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.License;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Video;

namespace VAS.Tests.Multimedia
{
	public class TestMultimediaToolkit
	{
		Mock<ILicenseLimitationsService> mockLimitationService;
		ILicenseLimitationsService currentService;

		[OneTimeSetUp]
		public void OneTimeSetUp ()
		{
			currentService = App.Current.LicenseLimitationsService;
		}

		[OneTimeTearDown]
		public void OneTimeTearDown ()
		{
			App.Current.LicenseLimitationsService = currentService;
		}

		[SetUp]
		public void SetUp ()
		{
			mockLimitationService = new Mock<ILicenseLimitationsService> ();
			App.Current.LicenseLimitationsService = mockLimitationService.Object;
		}

		[Test]
		public void GetMultiPlayer_MultiCameraLimitationActive_ThrowException ()
		{
			// Arrange
			IMultimediaToolkit sut = new MultimediaToolkit ();
			sut.Register<IMultiVideoPlayer, DummyMultiPlayer> (0);

			mockLimitationService.Setup (s => s.Get<FeatureLimitationVM> (VASFeature.OpenMultiCamera.ToString ()))
								 .Returns (new FeatureLimitationVM { Model = new FeatureLicenseLimitation { Enabled = true } });

			// Act
			Assert.Throws<InvalidOperationException> (() => sut.GetMultiPlayer ());
		}

		[Test]
		public void CreatePlayer_MultiCameraNoLimitation_ReturnMultiPlayer ()
		{
			// Arrange
			bool exceptionThrow = false;
			IMultimediaToolkit sut = new MultimediaToolkit ();
			sut.Register<IMultiVideoPlayer, DummyMultiPlayer> (0);

			mockLimitationService.Setup (s => s.Get<FeatureLimitationVM> (VASFeature.OpenMultiCamera.ToString ()))
								 .Returns (new FeatureLimitationVM { Model = new FeatureLicenseLimitation { Enabled = false } });

			// Act
			try {
				sut.GetMultiPlayer ();
			} catch (InvalidOperationException) {
				exceptionThrow = true;
			}

			// Assert
			Assert.IsFalse (exceptionThrow);
		}
	}

	class DummyMultiPlayer : IMultiVideoPlayer
	{
		public RangeObservableCollection<CameraConfig> CamerasConfig {
			set {
				throw new NotImplementedException ();
			}
		}

		public Time CurrentTime {
			get {
				throw new NotImplementedException ();
			}
		}

		public bool Playing {
			get {
				throw new NotImplementedException ();
			}
		}

		public double Rate {
			get {
				throw new NotImplementedException ();
			}

			set {
				throw new NotImplementedException ();
			}
		}

		public Time StreamLength {
			get {
				throw new NotImplementedException ();
			}
		}

		public double Volume {
			get {
				throw new NotImplementedException ();
			}

			set {
				throw new NotImplementedException ();
			}
		}

		public object WindowHandle {
			set {
				throw new NotImplementedException ();
			}
		}

		public List<object> WindowHandles {
			set {
				throw new NotImplementedException ();
			}
		}

		public event EosHandler Eos;
		public event ErrorHandler Error;
		public event ReadyToSeekHandler ReadyToSeek;
		public event ScopeStateChangedHandler ScopeChangedEvent;
		public event StateChangeHandler StateChange;

		public void ApplyCamerasConfig ()
		{
			throw new NotImplementedException ();
		}

		public void ApplyROI (CameraConfig camConfig)
		{
			throw new NotImplementedException ();
		}

		public void Close ()
		{
			throw new NotImplementedException ();
		}

		public void Dispose ()
		{
			throw new NotImplementedException ();
		}

		public void Expose ()
		{
			throw new NotImplementedException ();
		}

		public Image GetCurrentFrame (int width = -1, int height = -1)
		{
			throw new NotImplementedException ();
		}

		public bool Open (MediaFile mf)
		{
			throw new NotImplementedException ();
		}

		public bool Open (string mrl)
		{
			throw new NotImplementedException ();
		}

		public bool Open (MediaFileSet mfs)
		{
			throw new NotImplementedException ();
		}

		public void Pause (bool synchronous = false)
		{
			throw new NotImplementedException ();
		}

		public void Play (bool synchronous = false)
		{
			throw new NotImplementedException ();
		}

		public bool Seek (Time time, bool accurate = false, bool synchronous = false)
		{
			throw new NotImplementedException ();
		}

		public bool SeekToNextFrame ()
		{
			throw new NotImplementedException ();
		}

		public bool SeekToPreviousFrame ()
		{
			throw new NotImplementedException ();
		}

		public void Stop (bool synchronous = false)
		{
			throw new NotImplementedException ();
		}
	}
}
