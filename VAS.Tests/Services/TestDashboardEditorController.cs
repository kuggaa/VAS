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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Common;
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
		public async Task SetUp ()
		{
			var dashboard = Utils.DashboardDummy.Default ();
			dashboard.InsertTimer ();
			dashboardVM = new DashboardVM { Model = dashboard };
			dashboardVM.Mode = DashboardMode.Edit;
			var viewModel = new DummyDashboardManagerVM (dashboardVM);
			controller = new DashboardEditorController ();
			controller.SetViewModel (viewModel);
			await controller.Start ();
			KeyContext context = new KeyContext ();
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
			dashboardVM.Mode = DashboardMode.Edit;
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

		[Test]
		public void RemoveDashboardButton_SourceButton_ActionLinkRemoved ()
		{
			bool removed = false;
			var link = CreateActionLink (dashboardVM.ViewModels [0].Model, dashboardVM.ViewModels [1].Model);
			ActionLinkVM actionLinkVM = new ActionLinkVM ();
			actionLinkVM.Model = link;
			actionLinkVM.SourceButton = dashboardVM.ViewModels [0];
			actionLinkVM.DestinationButton = dashboardVM.ViewModels [1];
			dashboardVM.ViewModels [0].ActionLinks.ViewModels.Add (actionLinkVM);
			dashboardVM.ViewModels [0].ActionLinks.GetNotifyCollection ().CollectionChanged += (sender, e) => {
				if (e.Action == NotifyCollectionChangedAction.Remove) {
					if ((e.OldItems[0] as ActionLinkVM).Model == link) {
						removed = true;
					}
				}
			};

			var vm = dashboardVM.ViewModels [0];
			dashboardVM.DeleteButton.Execute (vm);

			Assert.IsTrue (removed);
			Assert.IsFalse (vm.ActionLinks.ViewModels.Any ());
			CollectionAssert.DoesNotContain (dashboardVM.ViewModels, vm);
		}

		[Test]
		public void RemoveDashboardButton_DestinationButton_ActionLinkRemoved ()
		{
			bool removed = false;
			var link = CreateActionLink (dashboardVM.ViewModels [0].Model, dashboardVM.ViewModels [1].Model);
			ActionLinkVM actionLinkVM = new ActionLinkVM ();
			actionLinkVM.Model = link;
			actionLinkVM.SourceButton = dashboardVM.ViewModels [0];
			actionLinkVM.DestinationButton = dashboardVM.ViewModels [1];
			dashboardVM.ViewModels [0].ActionLinks.ViewModels.Add (actionLinkVM);
			dashboardVM.ViewModels [0].ActionLinks.GetNotifyCollection ().CollectionChanged += (sender, e) => {
				if (e.Action == NotifyCollectionChangedAction.Remove) {
					if ((e.OldItems [0] as ActionLinkVM).Model == link) {
						removed = true;
					}
				}
			};

			var vmWithLink = dashboardVM.ViewModels [0];
			var vmRemoved = dashboardVM.ViewModels [1];
			dashboardVM.DeleteButton.Execute (vmRemoved);

			Assert.IsTrue (removed);
			Assert.IsFalse (vmWithLink.ActionLinks.ViewModels.Any ());
			CollectionAssert.DoesNotContain (dashboardVM.ViewModels, vmRemoved);
		}

		[Test]
		public void RemoveEventTypesTags_SourceButton_ActionLinkRemoved ()
		{
			bool removed = false;
			var sourceTag = (dashboardVM.ViewModels [0].Model as AnalysisEventButton).AnalysisEventType.
			                                                                         Tags.FirstOrDefault ();
			var destTag = (dashboardVM.ViewModels [1].Model as AnalysisEventButton).AnalysisEventType.
			                                                                       Tags.FirstOrDefault ();
			var link = CreateActionLink (dashboardVM.ViewModels [0].Model, dashboardVM.ViewModels [1].Model,
			                             sourceTag, destTag);
			ActionLinkVM actionLinkVM = new ActionLinkVM ();
			actionLinkVM.Model = link;
			actionLinkVM.SourceButton = dashboardVM.ViewModels [0];
			actionLinkVM.DestinationButton = dashboardVM.ViewModels [1];
			dashboardVM.ViewModels [0].ActionLinks.ViewModels.Add (actionLinkVM);
			dashboardVM.ViewModels [0].ActionLinks.GetNotifyCollection ().CollectionChanged += (sender, e) => {
				if (e.Action == NotifyCollectionChangedAction.Remove) {
					if ((e.OldItems [0] as ActionLinkVM).Model == link) {
						removed = true;
					}
				}
			};

			var vm = dashboardVM.ViewModels [0];
			(vm.Model as AnalysisEventButton).AnalysisEventType.Tags.RemoveAt (0);

			Assert.IsTrue (removed);
			Assert.IsFalse (vm.ActionLinks.ViewModels.Any ());
			CollectionAssert.Contains (dashboardVM.ViewModels, vm);
			CollectionAssert.Contains (dashboardVM.ViewModels, dashboardVM.ViewModels [1]);
		}

		[Test]
		public void RemoveEventTypesTags_DestinationButton_ActionLinksRemoved ()
		{
			bool removed = false;
			var sourceTag = (dashboardVM.ViewModels [0].Model as AnalysisEventButton).AnalysisEventType.
																					 Tags.FirstOrDefault ();
			var destTag = (dashboardVM.ViewModels [1].Model as AnalysisEventButton).AnalysisEventType.
																				   Tags.FirstOrDefault ();
			var link = CreateActionLink (dashboardVM.ViewModels [0].Model, dashboardVM.ViewModels [1].Model,
										 sourceTag, destTag);
			ActionLinkVM actionLinkVM = new ActionLinkVM ();
			actionLinkVM.Model = link;
			actionLinkVM.SourceButton = dashboardVM.ViewModels [0];
			actionLinkVM.DestinationButton = dashboardVM.ViewModels [1];
			dashboardVM.ViewModels [0].ActionLinks.ViewModels.Add (actionLinkVM);
			dashboardVM.ViewModels [0].ActionLinks.GetNotifyCollection ().CollectionChanged += (sender, e) => {
				if (e.Action == NotifyCollectionChangedAction.Remove) {
					if ((e.OldItems [0] as ActionLinkVM).Model == link) {
						removed = true;
					}
				}
			};

			var vmWithLink = dashboardVM.ViewModels [0];
			var vmTagsRemoved = dashboardVM.ViewModels [1];
			(vmTagsRemoved.Model as AnalysisEventButton).AnalysisEventType.Tags.RemoveAt (0);

			Assert.IsTrue (removed);
			Assert.IsFalse (vmWithLink.ActionLinks.ViewModels.Any ());
			CollectionAssert.Contains (dashboardVM.ViewModels, vmWithLink);
			CollectionAssert.Contains (dashboardVM.ViewModels, vmTagsRemoved);
		}

		[Test]
		public void Duplicate_EvenTypeButton_DestinationButtonsSameReference ()
		{
			var link = CreateActionLink (dashboardVM.ViewModels [0].Model, dashboardVM.ViewModels [1].Model);
			ActionLinkVM actionLinkVM = new ActionLinkVM ();
			actionLinkVM.Model = link;
			actionLinkVM.SourceButton = dashboardVM.ViewModels [0];
			actionLinkVM.DestinationButton = dashboardVM.ViewModels [1];
			dashboardVM.ViewModels [0].ActionLinks.ViewModels.Add (actionLinkVM);

			dashboardVM.DuplicateButton.Execute (dashboardVM.ViewModels [0]);
			var duplicatedButton = dashboardVM.ViewModels.Last ();

			Assert.AreSame (dashboardVM.ViewModels[1].Model,
			                duplicatedButton.Model.ActionLinks[0].DestinationButton);
			Assert.AreSame (duplicatedButton.Model, duplicatedButton.Model.ActionLinks[0].SourceButton);
		}

		[Test]
		public async Task StartController_DashBoardModeIsCode_NoKeyContextAdded ()
		{
			await controller.Stop ();
			dashboardVM.Mode = DashboardMode.Code;
			App.Current.KeyContextManager.NewKeyContexts (new List<KeyContext> ());
			await controller.Start ();

			var editKeyContext = App.Current.KeyContextManager.CurrentKeyContexts.Where (
				k => k == controller.editDashboardKeyContext
			);

			Assert.IsFalse (editKeyContext.Any ());
		}

		[Test]
		public async Task StartController_DashBoardModeIsEdit_KeyContextAdded ()
		{
			await controller.Stop ();
			dashboardVM.Mode = DashboardMode.Edit;
			App.Current.KeyContextManager.NewKeyContexts (new List<KeyContext> ());
			await controller.Start ();

			var editKeyContext = App.Current.KeyContextManager.CurrentKeyContexts.Where (
				k => k == controller.editDashboardKeyContext
			);

			Assert.AreEqual (1, editKeyContext.Count ());
		}

		[Test]
		public async Task StartController_SeveralTimes_MaintainsOnlyOneKeyContext ()
		{
			await controller.Stop ();
			await controller.Start ();
			await controller.Stop ();
			await controller.Start ();

			var editKeyContext = App.Current.KeyContextManager.CurrentKeyContexts.Where (
				k => k == controller.editDashboardKeyContext
			);

			Assert.AreEqual (1, editKeyContext.Count ());
		}

		[Test]
		public async Task DashBoardModeChangeToEdit_KeyContextAdded ()
		{
			await controller.Stop ();
			dashboardVM.Mode = DashboardMode.Code;
			App.Current.KeyContextManager.NewKeyContexts (new List<KeyContext> ());
			await controller.Start ();

			dashboardVM.Mode = DashboardMode.Edit;

			var editKeyContext = App.Current.KeyContextManager.CurrentKeyContexts.Where (
				k => k == controller.editDashboardKeyContext
			);

			Assert.AreEqual (1, editKeyContext.Count ());
		}

		[Test]
		public void DashBoardModeChangeToCode_RemovesKeyContext ()
		{
			dashboardVM.Mode = DashboardMode.Code;

			var editKeyContext = App.Current.KeyContextManager.CurrentKeyContexts.Where (
				k => k == controller.editDashboardKeyContext
			);

			Assert.IsFalse (editKeyContext.Any ());
		}

		ActionLink CreateActionLink (DashboardButton source, DashboardButton dest)
		{
			ActionLink link = new ActionLink ();
			link.DestinationButton = dest;
			link.SourceButton = source;
			return link;
		}

		ActionLink CreateActionLink (DashboardButton source, DashboardButton dest, Tag sourceTag, Tag destTag)
		{
			ActionLink link = CreateActionLink (source, dest);
			link.SourceTags = new RangeObservableCollection<Tag> { sourceTag };
			link.DestinationTags = new RangeObservableCollection<Tag> { destTag };
			return link;
		}
	}
}
