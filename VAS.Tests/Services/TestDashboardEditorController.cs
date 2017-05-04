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
using System.Linq;
using NUnit.Framework;
using VAS.Core.ViewModel;
using VAS.Services.Controller;
using VAS.Core.Store;

namespace VAS.Tests.Services
{

	[TestFixture]
	public class TestDashboardEditorController
	{
		DashboardVM dashboardVM;
		DashboardEditorController controller;

		[SetUp]
		public void SetUp ()
		{
			var dashboard = Utils.DashboardDummy.Default ();
			dashboard.InsertTimer ();
			dashboardVM = new DashboardVM { Model = dashboard };
			controller = new DashboardEditorController ();
			controller.SetViewModel (dashboardVM);
			controller.Start ();
		}

		[Test]
		public void TestDuplicate_Button ()
		{
			var last = dashboardVM.ViewModels.Last ();
			dashboardVM.DuplicateButton.Execute (last);

			Assert.AreEqual (last, dashboardVM.ViewModels [6]);
			Assert.AreEqual (8, dashboardVM.ViewModels.Count);
		}

		[Test]
		public void TestDuplicate_EvenTypeButton ()
		{
			var button = dashboardVM.ViewModels.First ();
			dashboardVM.DuplicateButton.Execute (dashboardVM.ViewModels.First ());

			Assert.AreEqual (8, dashboardVM.ViewModels.Count);
			Assert.AreNotEqual ((button.Model as EventButton).EventType.ID,
								(dashboardVM.ViewModels.Last ().Model as EventButton).EventType.ID);
		}
	}
}
