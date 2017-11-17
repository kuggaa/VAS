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
using NUnit.Framework;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;

namespace VAS.Tests.MVVMC
{
	[TestFixture]
	public class TestViewModelBase
	{
		[Test]
		public void TestForwardProperty ()
		{
			// Arrange
			int eventCount = 0;
			bool senderIsTimeNodeVM = false;
			TimeNode timeNode = new TimeNode ();
			TimeNodeVM viewModel = new TimeNodeVM ();
			viewModel.Model = timeNode;
			viewModel.PropertyChanged += (sender, e) => {
				eventCount++;
				senderIsTimeNodeVM = sender is TimeNodeVM;
			};

			// Action
			timeNode.EventTime = new Time (0);

			// Assert
			Assert.AreEqual (1, eventCount, $"PropertyChanged was called {eventCount} instead of once");
			Assert.IsTrue (senderIsTimeNodeVM, "Sender is not a ViewModel");
		}

		[Test]
		public void TestForwardModelChangesToVM ()
		{
			object senderObject = null;
			TimerVM viewModel = new TimerVM ();
			Timer timer = new Timer ();
			timer.Nodes.Add (new TimeNode ());
			viewModel.Model = timer;
			viewModel.PropertyChanged += (sender, e) => {
				senderObject = sender;
			};

			viewModel.ViewModels [0].Model.Name = "test";

			Assert.AreEqual (viewModel.ViewModels [0], senderObject);
		}

		[Test]
		public void TestForwardModelChangesToVMWithPlaylists ()
		{
			object senderObject = null;
			var viewModel = new PlaylistVM {
				Model = new Playlist (),
			};
			TimelineEvent timelineEvent = new TimelineEvent ();

			viewModel.Model.Elements.Add (new PlaylistPlayElement (timelineEvent));

			viewModel.PropertyChanged += (sender, e) => {
				senderObject = sender;
			};

			timelineEvent.Name = "test";
			Assert.AreEqual (viewModel.ViewModels [0], senderObject);
		}

		[Test]
		public void TestForwardNotChangeSenderIfVM ()
		{
			object senderObject = null;
			TimerVM viewModel = new TimerVM ();
			Timer timer = new Timer ();
			TimeNode timeNode = new TimeNode ();
			timer.Nodes.Add (timeNode);
			viewModel.Model = timer;
			viewModel.PropertyChanged += (sender, e) => {
				senderObject = sender;
			};

			viewModel.ViewModels [0].Name = "test";

			Assert.AreEqual (viewModel.ViewModels [0], senderObject);
		}

		[Test]
		public void TestChangeModel ()
		{
			int eventCount = 0;
			TimeNode timeNode = new TimeNode ();
			TimeNodeVM viewModel = new TimeNodeVM ();
			viewModel.Model = null;
			viewModel.Model = timeNode;
			viewModel.PropertyChanged += (sender, e) => eventCount++;

			timeNode.EventTime = new Time (0);

			Assert.AreEqual (1, eventCount);
		}

		[Test]
		public void TestNeedsSyncWithNullPropertyName ()
		{
			var viewModel = new ViewModelBase<StorableBase> ();
			Assert.IsTrue (viewModel.NeedsSync (propertyNameChanged: null, propertyNameToCheck: null));

		}

		[Test]
		public void TestNeedsSyncSameProperties ()
		{
			var viewModel = new ViewModelBase<StorableBase> ();
			Assert.IsTrue (viewModel.NeedsSync ("Prop", "Prop"));
		}

		[Test]
		public void TestNeedsSyncDifferentProperties ()
		{
			var viewModel = new ViewModelBase<StorableBase> ();
			Assert.IsFalse (viewModel.NeedsSync ("Prop", "Prop1"));
		}

		[Test]
		public void TestNeedsSyncSamePropertiesDifferentSender ()
		{
			var viewModel = new ViewModelBase<StorableBase> ();
			Assert.IsFalse (viewModel.NeedsSync ("Prop1", "Prop1", new object (), new object ()));
		}

		[Test]
		public void TestNeedsSyncSamePropertiesSameSender ()
		{
			var viewModel = new ViewModelBase<StorableBase> ();
			Assert.IsTrue (viewModel.NeedsSync ("Prop1", "Prop1", this, this));
		}

		[Test]
		public void TestNeedsSyncSamePropertiesNullSender ()
		{
			var viewModel = new ViewModelBase<StorableBase> ();
			Assert.IsTrue (viewModel.NeedsSync ("Prop1", "Prop1", null, null));
		}

		[Test]
		public void NeedsSync_SenderNotNullSenderToCheckNull_False ()
		{
			var viewModel = new ViewModelBase<StorableBase> ();
			Assert.IsFalse (viewModel.NeedsSync ("Prop1", "Prop1", new object (), null));
		}

		[Test]
		public void TestSync ()
		{
			string propertyName = "foo";
			var viewModel = new ViewModelBase<StorableBase> ();
			viewModel.PropertyChanged += (sender, e) => propertyName = e.PropertyName;

			viewModel.Sync ();

			Assert.IsNull (propertyName);
		}

		[Test]
		public void Model_Set_SyncCalled ()
		{
			bool called = false;
			var viewModel = new ViewModelBase<StorableBase> ();
			viewModel.PropertyChanged += (sender, e) => {
				called |= e.PropertyName == null;
			};

			viewModel.Model = new StorableBase ();

			Assert.IsTrue (called);
		}
	}
}

