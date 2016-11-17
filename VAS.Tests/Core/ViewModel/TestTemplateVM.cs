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
using VAS.Core.Store.Templates;
using VAS.Core.ViewModel;
using System.Reflection;

namespace VAS.Tests.Core.ViewModel
{
	[TestFixture]
	public class TestTemplateVM
	{
		[Test]
		public void TestProperties ()
		{
			var model = new Utils.DashboardDummy {
				Name = "dash",
				Static = true,
			};
			var viewModel = new DummyDashboardViewModel {
				Model = model
			};
			model.IsChanged = false;

			Assert.AreEqual (false, viewModel.Edited);
			Assert.AreEqual ("dash", viewModel.Name);
			Assert.AreEqual (null, viewModel.Icon);
		}

		[Test]
		public void TestEdited ()
		{
			var model = new Utils.DashboardDummy {
				Name = "dash",
				Static = true,
			};
			var viewModel = new DummyDashboardViewModel {
				Model = model
			};
			model.IsChanged = false;
			Assert.AreEqual (false, viewModel.Edited);

			model.Name = "Test";
			Assert.AreEqual (true, viewModel.Edited);
		}
	}
}
