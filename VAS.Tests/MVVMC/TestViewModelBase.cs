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
		public void TestForwardPropertyChangeSenderIfNotViewModel ()
		{
			object senderObject = null;
			TimerVM timerVM = new TimerVM ();
			Timer timer = new Timer ();
			TimeNode timeNode = new TimeNode ();
			timer.Nodes.Add (timeNode);
			timerVM.Model = timer;
			TimeNodeVM shouldBeSender = timerVM.ViewModels [0];
			timerVM.PropertyChanged += (sender, e) => {
				senderObject = sender;
			};

			timeNode.Name = "test";

			Assert.AreEqual (shouldBeSender, senderObject);
		}

		[Test]
		public void TestForwardPropertyNotChangeSenderIfViewModel ()
		{

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
		public void TestSync ()
		{
			string propertyName = "foo";
			var viewModel = new ViewModelBase<StorableBase> ();
			viewModel.PropertyChanged += (sender, e) => propertyName = e.PropertyName;

			viewModel.Sync ();

			Assert.IsNull (propertyName);
		}
	}
}

