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
using System;
using NUnit.Framework;
using VAS.Core.ViewModel;
using VAS.Core.Store;

namespace VAS.Tests.Core.ViewModel
{
	[TestFixture]
	public class TestMediaFileSetVM
	{
		MediaFileSet model;
		MediaFileSetVM viewModel;

		[Test]
		public void Setup ()
		{
			model = new MediaFileSet {
				IsStretched = false,
			};
			model.Add (new MediaFile {
				FilePath = "/videos/test.mp4",
				Duration = new Time (20000),
			});
			viewModel = new MediaFileSetVM {
				Model = model,
			};
		}

		[Test]
		public void TestProperties ()
		{
			model.IsStretched = true;

			Assert.IsTrue (viewModel.IsStretched);
			Assert.AreEqual (1, viewModel.ViewModels.Count);
		}

		[Test]
		public void TestForwardProperties ()
		{
			int count = 0;
			model.IsStretched = true;
			viewModel.PropertyChanged += (sender, e) => count++;
			model.IsStretched = false;

			Assert.AreEqual (1, count);
		}

		[Test]
		public void TestVirtualDurationStretched ()
		{
			model.VisibleRegion.Start = new Time (7000);
			model.VisibleRegion.Stop = new Time (17000);

			model.IsStretched = true;

			Assert.AreEqual (new Time (10000), viewModel.VirtualDuration);
		}

		[Test]
		public void TestVirtualDurationNotStretched ()
		{
			model.IsStretched = false;

			Assert.AreEqual (new Time (20000), viewModel.VirtualDuration);
		}

		[Test]
		public void TestVisibleRegionModelUpdatedInViewModel ()
		{
			model.VisibleRegion = new TimeNode {
				Start = new Time (7000),
				Stop = new Time (17000)
			};

			Assert.AreEqual (new Time (7000), viewModel.VisibleRegion.Start);
			Assert.AreEqual (new Time (17000), viewModel.VisibleRegion.Stop);
		}

	}
}
