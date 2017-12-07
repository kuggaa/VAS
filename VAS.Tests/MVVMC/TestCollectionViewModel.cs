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
using System.Linq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Tests.MVVMC
{
	[TestFixture]
	public class TestCollectionViewModel
	{
		CollectionViewModel<BindableBase, DummyViewModel<BindableBase>> viewModel;
		RangeObservableCollection<BindableBase> model;

		[SetUp]
		public void SetUp ()
		{
			viewModel = new CollectionViewModel<BindableBase, DummyViewModel<BindableBase>> ();
			model = new RangeObservableCollection<BindableBase> { new BindableBase (), new BindableBase () };
			viewModel.Model = model;
		}

		[Test]
		public void TestSetModel ()
		{
			Assert.AreEqual (2, viewModel.ViewModels.Count);
			Assert.AreEqual (0, viewModel.Selection.Count);
		}

		[Test]
		public void TestSyncViewModel ()
		{
			model.RemoveAt (0);
			Assert.AreEqual (1, viewModel.ViewModels.Count);
			model.RemoveAt (0);
			Assert.AreEqual (0, viewModel.ViewModels.Count);
			model.Add (new BindableBase ());
			Assert.AreEqual (1, viewModel.ViewModels.Count);
			model [0] = new BindableBase ();
			Assert.AreEqual (1, viewModel.ViewModels.Count);
		}

		[Test]
		public void TestSyncModel ()
		{
			viewModel.ViewModels.RemoveAt (0);
			Assert.AreEqual (1, viewModel.Model.Count);
			viewModel.ViewModels.RemoveAt (0);
			Assert.AreEqual (0, viewModel.Model.Count);
			viewModel.ViewModels.Add (new DummyViewModel<BindableBase> { Model = new BindableBase () });
			Assert.AreEqual (1, viewModel.Model.Count);
			viewModel.ViewModels [0] = new DummyViewModel<BindableBase> { Model = new BindableBase () };
			Assert.AreEqual (1, viewModel.Model.Count);
		}

		[Test]
		public void TestSelect ()
		{
			int eventCount = 0;
			viewModel.PropertyChanged += (sender, e) => eventCount++;

			viewModel.Select (model [0]);
			Assert.AreEqual (1, eventCount);
			Assert.AreEqual (1, viewModel.Selection.Count);
			Assert.AreSame (model [0], viewModel.Selection [0].Model);

			viewModel.Select (viewModel.ViewModels [1]);
			Assert.AreEqual (2, eventCount);
			Assert.AreEqual (1, viewModel.Selection.Count);
			Assert.AreSame (viewModel.ViewModels [1], viewModel.Selection [0]);
		}

		[Test]
		public void TestChangeModel ()
		{
			var sel = viewModel.Selection;
			int eventCount = 0;

			Assert.AreEqual (2, viewModel.ViewModels.Count);
			Assert.AreSame (sel, viewModel.Selection);

			model = new RangeObservableCollection<BindableBase> { new BindableBase () };
			viewModel.Model = model;
			Assert.AreEqual (1, viewModel.ViewModels.Count);
			Assert.AreSame (sel, viewModel.Selection);

			viewModel.PropertyChanged += (sender, e) => eventCount++;
			viewModel.Select (model [0]);
			Assert.AreEqual (1, eventCount);
		}

		[Test]
		public void TestReplaceWithNull ()
		{
			model.Reset (null);

			Assert.AreEqual (0, model.Count);
			Assert.AreEqual (0, viewModel.Count ());
		}

		[Test]
		public void TestReplace ()
		{
			model.Reset (new BindableBase ().ToEnumerable ());

			Assert.AreEqual (1, model.Count);
			Assert.AreEqual (1, viewModel.Count ());
		}

		[Test]
		public void Add_ViewModel_AddModelBeforeNotify ()
		{
			// Arrange
			Utils.PlayerDummy m = new Utils.PlayerDummy ();
			Utils.PlayerDummy modelNotified = null;
			CollectionViewModel<Utils.PlayerDummy, DummyPlayerVM> collection = new CollectionViewModel<Utils.PlayerDummy, DummyPlayerVM> ();
			collection.GetNotifyCollection ().CollectionChanged += (sender, e) => modelNotified = ((DummyPlayerVM)e.NewItems [0]).Model;

			// Act
			collection.ViewModels.Add (new DummyPlayerVM { Model = m });

			// Assert
			Assert.AreEqual (m, modelNotified);
		}

		// FIXME: Fix the collection view model notification
		// forwarding is done before updating the model collection
		/*[Test]
		public void AddViewModel_AttachParentPropertychanged_AddModelBeforeNotify ()
		{
			// Arrange
			MediaFile m = new MediaFile { Name = "test" };
			MediaFile modelNotified = null;
			MediaFileSetVM vm = new MediaFileSetVM ();
			vm.PropertyChanged += (sender, e) => {
				if (e.PropertyName == "Collection_ViewModels") {
					modelNotified = vm.Model? [0];
				}
			};

			// Act
			vm.ViewModels.Add (new MediaFileVM { Model = m });

			// Assert
			Assert.AreEqual (m, modelNotified);
		}*/
	}
}

