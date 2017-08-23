//
//  Copyright (C) 2016 FLUENDO S.A.
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
using System.Collections.Generic;
using System.Threading;
using ICSharpCode.SharpZipLib;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.License;
using VAS.DB;
using VAS.Services;
using Timer = VAS.Core.Common.Timer;

namespace VAS.Tests
{
	[SetUpFixture]
	public class SetupClass
	{
		[OneTimeSetUp]
		public static void Setup ()
		{
			// Initialize VAS.Core by using a type, this will call the module initialization
			VFS.SetCurrent (new FileSystem ());

			App.Current = new AppDummy ();
			App.InitDependencies ();
			App.Current.Config = new ConfigDummy ();
			SynchronizationContext.SetSynchronizationContext (new MockSynchronizationContext ());
			App.Current.DependencyRegistry.Register<IStorageManager, CouchbaseManager> (1);
			App.Current.DependencyRegistry.Register<IStopwatch, Stopwatch> (1);
			App.Current.DependencyRegistry.Register<ITimer, Timer> (1);
			App.Current.Dialogs = new Mock<IDialogs> ().Object;
			var navigation = new Mock<INavigation> ();
			navigation.Setup (x => x.Push (It.IsAny<IPanel> ())).Returns (AsyncHelpers.Return (true));
			navigation.Setup (x => x.PushModal (It.IsAny<IPanel> (), It.IsAny<IPanel> ())).Returns (AsyncHelpers.Return (true));
			navigation.Setup (x => x.PopModal (It.IsAny<IPanel> ())).Returns (AsyncHelpers.Return (true));
			navigation.Setup (x => x.Pop (It.IsAny<IPanel> ())).Returns (AsyncHelpers.Return (true));
			App.Current.Navigation = navigation.Object;
			var mockLicenseManager = new Mock<ILicenseManager> ();
			var mockLicenseStatus = new Mock<ILicenseStatus> ();
			mockLicenseManager.SetupGet (obj => obj.LicenseStatus).Returns (mockLicenseStatus.Object);
			mockLicenseStatus.SetupGet (obj => obj.Limited).Returns (true);
			App.Current.LicenseManager = mockLicenseManager.Object;
			App.Current.ResourcesLocator = new DummyResourcesLocator ();
			var mockToolkit = new Mock<IGUIToolkit> ();
			App.Current.GUIToolkit = mockToolkit.Object;
			App.Current.FileSystemManager = new FileSystemManager ();
			mockToolkit.SetupGet (o => o.DeviceScaleFactor).Returns (1.0f);
			App.Current.LicenseLimitationsService = new DummyLicenseLimitationsService ();
		}
	}

	public class AppDummy : App
	{
		//Dummy class for VAS.App
	}

	public class ConfigDummy : Config
	{
		//Dummy class for VAS.Config
		public ConfigDummy ()
		{
			KeyConfigs = new List<KeyConfig> ();
		}
	}

	/// <summary>
	/// Prism's UI thread option works by invoking Post on the current synchronization context.
	/// When we do that, base.Post actually looses SynchronizationContext.Current
	/// because the work has been delegated to ThreadPool.QueueUserWorkItem.
	/// This implementation makes our async-intended call behave synchronously,
	/// so we can preserve and verify sync contexts for callbacks during our unit tests.
	/// </summary>
	internal class MockSynchronizationContext : SynchronizationContext
	{
		public override void Post (SendOrPostCallback d, object state)
		{
			d (state);
		}

		public override void Send (SendOrPostCallback d, object state)
		{
			d (state);
		}
	}
}
