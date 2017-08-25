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
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Hotkeys;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services;
using VAS.Services.Controller;

namespace VAS.Tests.Services
{

	[TestFixture]
	public class TestDashboardEditorController
	{
		DashboardVM dashboardVM;
		DashboardEditorController controller;
		Mock<IDialogs> mockDialogs;

		[OneTimeSetUp]
		public void Init ()
		{
			App.Current.HotkeysService = new HotkeysService ();
			GeneralUIHotkeys.RegisterDefaultHotkeys ();
			mockDialogs = new Mock<IDialogs> ();
			App.Current.Dialogs = mockDialogs.Object;
			mockDialogs.Setup (m => m.QuestionMessage (It.IsAny<string> (), null, It.IsAny<object> ())).Returns (AsyncHelpers.Return (true));
		}

		[SetUp]
		public void SetUp ()
		{
			var dashboard = Utils.DashboardDummy.Default ();
			dashboard.InsertTimer ();
			dashboardVM = new DashboardVM { Model = dashboard };
			var viewModel = new DummyDashboardManagerVM (dashboardVM);
			controller = new DashboardEditorController ();
			controller.SetViewModel (viewModel);
			controller.Start ();
			KeyContext context = new KeyContext ();
			foreach (KeyAction action in controller.GetDefaultKeyActions ()) {
				context.AddAction (action);
			}
			App.Current.KeyContextManager.NewKeyContexts (new List<KeyContext> { context });
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

		[Test]
		public void TestDelete_BackSpace ()
		{
			var first = dashboardVM.ViewModels.First ();
			dashboardVM.Select (first);

			App.Current.KeyContextManager.HandleKeyPressed (App.Current.Keyboard.ParseName ("Delete"));

			Assert.AreEqual (6, dashboardVM.ViewModels.Count);
			CollectionAssert.DoesNotContain (dashboardVM.ViewModels, first);
		}

		[Test]
		public void TestDelete_BackSpace_NoSelection ()
		{
			App.Current.KeyContextManager.HandleKeyPressed (App.Current.Keyboard.ParseName ("Delete"));

			Assert.AreEqual (7, dashboardVM.ViewModels.Count);
		}
	}
}
