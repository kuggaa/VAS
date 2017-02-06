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
using NUnit.Framework;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Tests.Core.ViewModel
{
	class DummyStorableVM : StorableVM<StorableBase>
	{
		public bool SyncLoadedCalled { get; set; }

		public bool SyncPreloadedCalled { get; set; }

		protected override void SyncLoadedModel ()
		{
			SyncLoadedCalled = true;
		}

		protected override void SyncPreloadedModel ()
		{
			SyncPreloadedCalled = true;
		}
	}

	[TestFixture]
	public class TestStorableVM
	{
		[Test]
		public void TestOnlyPreloadedPropertiesSynced ()
		{
			DummyStorableVM viewModel = new DummyStorableVM ();
			var model = new StorableBase ();
			model.IsLoaded = false;
			viewModel.Model = model;

			Assert.IsTrue (viewModel.SyncPreloadedCalled);
			Assert.IsFalse (viewModel.SyncLoadedCalled);
		}

		[Test]
		public void TestAllPropertiesSyncedWhenStorableLoaded ()
		{
			DummyStorableVM viewModel = new DummyStorableVM ();
			var model = new StorableBase ();
			model.IsLoaded = true;
			viewModel.Model = model;

			Assert.IsTrue (viewModel.SyncPreloadedCalled);
			Assert.IsTrue (viewModel.SyncLoadedCalled);
		}

		[Test]
		public void TestPropertiesSyncedWhenStorableEmitsLoadedEvent ()
		{
			DummyStorableVM viewModel = new DummyStorableVM ();
			var model = new StorableBase ();
			model.IsLoaded = false;
			viewModel.Model = model;
			model.IsLoaded = true;

			Assert.IsTrue (viewModel.SyncPreloadedCalled);
			Assert.IsTrue (viewModel.SyncLoadedCalled);
		}
	}
}
