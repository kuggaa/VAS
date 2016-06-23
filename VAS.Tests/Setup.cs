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
using System.Threading;
using ICSharpCode.SharpZipLib;
using NUnit.Framework;
using VAS;
using VAS.DB;

namespace VAS.Tests
{
	[SetUpFixture]
	public class SetupClass
	{
		[SetUp]
		public void Setup ()
		{
			// Initialize VAS.Core by using a type, this will call the module initialization
			VFS.SetCurrent (new FileSystem ());
			App.Current = new AppDummy ();
			App.Current.Config = new ConfigDummy ();
			SynchronizationContext.SetSynchronizationContext (new MockSynchronizationContext ());
			App.Current.EventsBroker = new VAS.Core.Events.EventsBroker ();
		}
	}

	public class AppDummy : App
	{
		//Dummy class for VAS.App
	}

	public class ConfigDummy : Config
	{
		//Dummy class for VAS.Config
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