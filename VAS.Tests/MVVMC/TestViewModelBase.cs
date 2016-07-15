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
using VAS.Core.MVVMC;

namespace VAS.Tests.MVVMC
{
	[TestFixture]
	public class TestViewModelBase
	{
		[Test]
		public void TextForwardProperty ()
		{
			int eventCount = 0;
			var model = new DummyBindable ();
			var viewModel = new ViewModelBase<BindableBase> ();
			viewModel.Model = model;
			viewModel.PropertyChanged += (sender, e) => eventCount++;
			model.Raise ("test");
			Assert.AreEqual (1, eventCount);
		}

		[Test]
		public void TextChangeModel ()
		{
			int eventCount = 0;
			var model = new DummyBindable ();
			var viewModel = new ViewModelBase<BindableBase> ();
			viewModel.Model = model;

			Assert.DoesNotThrow (() => viewModel.Model = null);

			viewModel.Model = model;
			viewModel.PropertyChanged += (sender, e) => eventCount++;
			model.Raise ("test");
			Assert.AreEqual (1, eventCount);
		}
	}
}

