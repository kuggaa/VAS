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
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Tests.Core.ViewModel
{
	[TestFixture]
	public class TestTimeNodeVM
	{
		[Test]
		public void TestProperties ()
		{
			var model = new TimeNode {
				Start = new Time (0),
				Stop = new Time (10), EventTime = new Time (5),
				Name = "Event"
			};
			var viewModel = new TimeNodeVM {
				Model = model,
			};

			Assert.AreEqual ("Event", viewModel.Name);
			Assert.AreEqual (new Time (0), viewModel.Start);
			Assert.AreEqual (new Time (10), viewModel.Stop);
			Assert.AreEqual (new Time (5), viewModel.EventTime);
		}

		[Test]
		public void TestForwardProperties ()
		{
			int count = 0;
			var model = new TimeNode {
				Name = "Event"
			};
			var viewModel = new TimeNodeVM {
				Model = model,
			};

			viewModel.PropertyChanged += (sender, e) => count++;
			viewModel.Name = "Event2";

			Assert.AreEqual (2, count);
		}
	}
}
