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
using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services.Controller;

namespace VAS.Tests.Services
{
	[TestFixture]
	public class TestProjectsController
	{
		ProjectsController<Project, DummyProjectVM> controller;
		Mock<IStorage> storageMock;

		[OneTimeSetUp]
		public void OneTimeSetUp ()
		{
			var dialogMock = new Mock<IDialogs> ();
			dialogMock.Setup (d => d.QuestionMessage (It.IsAny<string> (), null, It.IsNotNull<object> ())).Returns (AsyncHelpers.Return (true));
			dialogMock.Setup (d => d.BusyDialog (It.IsAny<string> (), It.IsAny<object> ())).Returns (new DummyBusyDialog ());
			App.Current.Dialogs = dialogMock.Object;
			var storageManagerMock = new Mock<IStorageManager> ();
			storageManagerMock.SetupAllProperties ();
			storageMock = new Mock<IStorage> ();
			storageManagerMock.Object.ActiveDB = storageMock.Object;
			App.Current.DatabaseManager = storageManagerMock.Object;
		}

		[SetUp]
		public void SetUp ()
		{
			controller = new ProjectsController<Project, DummyProjectVM> ();
			controller.ViewModel = new ProjectsManagerVM<Project, DummyProjectVM> {
				Model = new RangeObservableCollection<Project> {
					Utils.CreateProject (true, "Champions"),
					Utils.CreateProject (true, "Liga game"),
					Utils.CreateProject (true, "Training session"),
				}
			};
		}

		[TearDown]
		public async Task TearDown ()
		{
			if (controller != null) {
				try {
					await controller.Stop ();
				} catch (InvalidOperationException) {
					// Already stopped
				}
			}
		}

		[Test]
		public void Start_NoViewModel_Throws ()
		{
			controller.ViewModel = null;
			Assert.ThrowsAsync<InvalidOperationException> (controller.Start);
		}

		[Test]
		public void Start_ViewModel_DoesntThrow ()
		{
			Assert.DoesNotThrowAsync (controller.Start);
		}

		[Test]
		public async Task SelectionChanged_ProjectSelected_ProjectLoadedStateful ()
		{

			await controller.Start ();

			controller.ViewModel.Select (controller.ViewModel.ViewModels.First ());

			Assert.AreNotEqual (controller.ViewModel.ViewModels.First (), controller.ViewModel.LoadedProject);
			Assert.AreSame (controller.ViewModel.ViewModels.First ().Model, controller.ViewModel.LoadedProject.Model);
			Assert.IsTrue (controller.ViewModel.LoadedProject.Stateful);
		}

		[Test]
		public async Task SelectionChanged_ProjectUnselected_ProjectUnloaded ()
		{
			controller.ViewModel.Select (controller.ViewModel.ViewModels.First ());

			await controller.Start ();

			controller.ViewModel.Selection.Replace (Enumerable.Empty<DummyProjectVM> ());

			Assert.IsNull (controller.ViewModel.LoadedProject.Model);
			Assert.IsNotNull (controller.ViewModel.LoadedProject);
		}

		[Test]
		public async Task SelectionChanged_WithChanges_ProjectSaved ()
		{
			// Arrange
			await controller.Start ();

			DummyProjectVM firstLoadedProject = controller.ViewModel.ViewModels.First ();
			controller.ViewModel.Select (firstLoadedProject);
			controller.ViewModel.LoadedProject.ProjectType = ProjectType.URICaptureProject;

			// Act
			controller.ViewModel.Select (controller.ViewModel.ViewModels.Skip (1).First ());

			// Assert
			storageMock.Verify (s => s.Store (firstLoadedProject.Model, false), Times.Once ());
			Assert.AreNotEqual (firstLoadedProject, controller.ViewModel.LoadedProject);
			Assert.AreNotSame (firstLoadedProject.Model, controller.ViewModel.LoadedProject.Model);
			Assert.IsTrue (controller.ViewModel.LoadedProject.Stateful);
			Assert.IsFalse (firstLoadedProject.Model.IsChanged);
			Assert.AreEqual (ProjectType.URICaptureProject, firstLoadedProject.ProjectType);
			Assert.AreEqual (ProjectType.URICaptureProject, firstLoadedProject.Model.ProjectType);
		}

		[Test]
		public async Task Search_EmptySearchStringNoProjects_NoResultFalse ()
		{
			await controller.Start ();
			controller.ViewModel.ViewModels.Clear ();

			controller.ViewModel.SearchCommand.Execute ("");

			Assert.IsFalse (controller.ViewModel.NoResults);
		}

		[Test]
		public async Task Search_NoResults_EmptyViewModelsAndNoResultTrue ()
		{
			await controller.Start ();

			controller.ViewModel.SearchCommand.Execute ("Pedro");

			Assert.IsTrue (controller.ViewModel.NoResults);
			Assert.IsEmpty (controller.ViewModel.VisibleViewModels);
		}

		[Test]
		public async Task Search_MatchingProjectNo_VisibleViewModelUpdateAndNoResultFalse ()
		{
			await controller.Start ();

			controller.ViewModel.SearchCommand.Execute ("Champions");

			Assert.IsFalse (controller.ViewModel.NoResults);
			Assert.AreEqual (controller.ViewModel.ViewModels [0], controller.ViewModel.VisibleViewModels [0]);
			Assert.AreEqual (1, controller.ViewModel.VisibleViewModels.Count ());
		}

		[Test]
		public async Task Search_EmptySearchString_AllProjectsVisible ()
		{
			await controller.Start ();

			controller.ViewModel.SearchCommand.Execute ("");

			Assert.IsFalse (controller.ViewModel.NoResults);
			Assert.AreEqual (3, controller.ViewModel.VisibleViewModels.Count ());
		}

		[Test]
		public async Task Search_NullSearchString_AllProjectsVisible ()
		{
			await controller.Start ();

			controller.ViewModel.SearchCommand.Execute (null);

			Assert.IsFalse (controller.ViewModel.NoResults);
			Assert.AreEqual (3, controller.ViewModel.VisibleViewModels.Count ());
		}
	}
}
