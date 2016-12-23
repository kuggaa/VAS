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
	public class TestTimerVM
	{
		[Test]
		public void TestProperties ()
		{
			var model = new Timer {
				Name = "Timer"
			};
			var viewModel = new TimerVM {
				Model = model
			};

			Assert.AreEqual ("Timer", viewModel.Name);
		}

		[Test]
		public void TestForawrdProperties ()
		{
			int count = 0;
			var model = new Timer {
				Name = "Timer"
			};
			var viewModel = new TimerVM {
				Model = model
			};
			viewModel.PropertyChanged += (sender, e) => count++;

			model.Name = "Test";

			Assert.AreEqual (1, count);
		}

		[Test]
		public void TestCollectionSync ()
		{
			int count = 0;
			var model = new Timer {
				Name = "Timer"
			};
			var viewModel = new TimerVM {
				Model = model
			};

			viewModel.PropertyChanged += (sender, e) => count++;
			model.Nodes.Add (new TimeNode ());

			Assert.AreEqual (1, count);
			Assert.AreEqual (1, viewModel.ViewModels.Count);
		}
	}
}
