//
//  Copyright (C) 2018 Fluendo S.A.
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
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Tests.Core.ViewModel
{
	[TestFixture]
    public class TestProjectsManagerVM
    {
		ProjectsManagerVM<Project, DummyProjectVM> projectsManagerVM;
		Project project;
		Mock<ILicenseLimitationsService> mockLimitationService;

		[OneTimeSetUp]
		public void OneTimeSetUp ()
		{
			SetupClass.SetUp ();
			mockLimitationService = new Mock<ILicenseLimitationsService> ();
			App.Current.LicenseLimitationsService = mockLimitationService.Object;
		}

		[SetUp]
		public void SetUp ()
		{
			project = Utils.CreateProject ();

			projectsManagerVM = new ProjectsManagerVM<Project, DummyProjectVM> ();
			projectsManagerVM.Model = new RangeObservableCollection<Project> { project };
		}

		[TearDown]
		public void TearDown ()
		{
			mockLimitationService.ResetCalls ();
		}

		[Test]
		public void NewCommand_Limited_ShowLimitationDialog ()
		{
			mockLimitationService.Setup (ls => ls.CanExecute (VASCountLimitedObjects.Projects.ToString ())).Returns (false);

			projectsManagerVM.NewCommand.Execute ();

			mockLimitationService.Verify (ls => ls.MoveToUpgradeDialog (VASCountLimitedObjects.Projects.ToString ()), Times.Once);
		}

		[Test]
		public void NewCommand_NotLimited_SendsEvent ()
		{
			bool sendEvent = false;
			mockLimitationService.Setup (ls => ls.CanExecute (VASCountLimitedObjects.Projects.ToString ())).Returns (true);
			App.Current.EventsBroker.Subscribe<CreateEvent<Project>> ((e) => sendEvent = true);

			projectsManagerVM.NewCommand.Execute ();

			mockLimitationService.Verify (ls => ls.MoveToUpgradeDialog (VASCountLimitedObjects.Projects.ToString ()), Times.Never);
			Assert.IsTrue (sendEvent);
		}
    }
}
