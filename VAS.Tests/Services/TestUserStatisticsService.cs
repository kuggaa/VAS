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
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.License;
using VAS.Core.Store;

namespace VAS.Tests.Services
{
	[TestFixture]
	public class TestUserStatisticsService
	{
		DummyUserStatisticsService service;
		Mock<IStorageManager> storageManagerMock;
		Mock<IStorage> storageMock;
		Mock<IKpiService> kpiServiceMock;
		Mock<ILicenseStatus> mockLicenseStatus;
		Project Project;

		[SetUp]
		public void Setup ()
		{
            var mockLicenseManager = new Mock<ILicenseManager> ();
            mockLicenseStatus = new Mock<ILicenseStatus> ();
            mockLicenseManager.SetupGet (obj => obj.LicenseStatus).Returns (mockLicenseStatus.Object);
            mockLicenseStatus.SetupGet (obj => obj.PlanName).Returns ("PRO");
            App.Current.LicenseManager = mockLicenseManager.Object;

			service = new DummyUserStatisticsService ();
			service.Start ();
			Project = Utils.CreateProject (true);
			storageManagerMock = new Mock<IStorageManager> ();
			storageManagerMock.SetupAllProperties ();
			storageMock = new Mock<IStorage> ();
			storageManagerMock.Object.ActiveDB = storageMock.Object;
			App.Current.DatabaseManager = storageManagerMock.Object;

			kpiServiceMock = new Mock<IKpiService> ();
			App.Current.KPIService = kpiServiceMock.Object;
		}

		[Test]
		public void TestCountUsageEvents_PlaylistAmount ()
		{
			// Action
			App.Current.EventsBroker.Publish (new NewPlaylistEvent ());

			// Assert
			service.Stop ();
			kpiServiceMock.Verify (m => m.TrackEvent ("Sessions", It.Is<Dictionary<string, string>> (d => d ["Plan"] == "PRO"),
				It.Is<Dictionary<string, double>> (d => d ["Playlists"] == 1)), Times.Once ());
		}

		[Test]
		public void TestCountUsageEvents_RendersAmount ()
		{
			// Action
			App.Current.EventsBroker.Publish (new JobRenderedEvent ());

			// Assert
			service.Stop ();
			kpiServiceMock.Verify (m => m.TrackEvent ("Sessions", It.Is<Dictionary<string, string>> (d => d ["Plan"] == "PRO"),
			It.Is<Dictionary<string, double>> (d => d ["Renders"] == 1)), Times.Once ());
		}

		[Test]
		public void TestCountUsageEvents_NewDashboard ()
		{
			// Prepare
			App.Current.EventsBroker.Publish (new OpenedProjectEvent {
				Project = Project,
				ProjectType = ProjectType.EditProject,
			});

			// Action
			App.Current.EventsBroker.Publish (new NewDashboardEvent ());

			// Assert
			service.Stop ();
			kpiServiceMock.Verify (m => m.TrackEvent ("Project_usage",
													  It.Is<Dictionary<string, string>> (
														  d => d ["Project_id"] == Project.ID.ToString () &&
				                                         d["Plan"] == "PRO"),
													  It.Is<Dictionary<string, double>> (
														  d => (d ["Events"] == 1) && (d ["Drawings"] == 0))),
								  Times.Once);
		}

		[Test]
		public void TestCountUsageEvents_DrawingSavedToProject ()
		{
			// Prepare
			App.Current.EventsBroker.Publish (new OpenedProjectEvent {
				Project = Project,
				ProjectType = ProjectType.EditProject,
			});

			// Action
			App.Current.EventsBroker.Publish (new DrawingSavedToProjectEvent ());

			service.Stop ();
			kpiServiceMock.Verify (m => m.TrackEvent ("Project_usage",
													  It.Is<Dictionary<string, string>> (
														  d => d ["Project_id"] == Project.ID.ToString () &&
				                                          d ["Plan"] == "PRO"),
													  It.Is<Dictionary<string, double>> (
														  d => (d ["Events"] == 0) && (d ["Drawings"] == 1))),
								  Times.Once);
		}

		[Test]
		public void TestCountUsageEvents_CreateProject ()
		{
			// Action
			App.Current.EventsBroker.Publish (new ProjectCreatedEvent ());

			// Assert
			service.Stop ();
			kpiServiceMock.Verify (m => m.TrackEvent ("Sessions", It.Is<Dictionary<string, string>> (d => d["Plan"] == "PRO"),
				It.Is<Dictionary<string, double>> (d => d ["Projects"] == 1)), Times.Once ());
		}

		[Test]
		public void StartService_GetPlan ()
		{
			Assert.AreEqual ("PRO", service.GeneralProperties ["Plan"]);
		}

        [Test]
        public void LicenseChange_UserPlanUpdated ()
        {
			Assert.AreEqual ("PRO", service.GeneralProperties ["Plan"]);

			mockLicenseStatus.SetupGet (obj => obj.PlanName).Returns ("BASIC");

			App.Current.EventsBroker.Publish (new LicenseChangeEvent ());

			Assert.AreEqual ("BASIC", service.GeneralProperties ["Plan"]);
			service.Stop ();
        }

		[Test]
		public void UpgradeDialogShown_SendTrackEvent ()
		{
			App.Current.EventsBroker.Publish (new LimitationDialogShownEvent {
				LimitationName = "Projects",
				Source = "ProjectsManager"
			});

			kpiServiceMock.Verify (m => m.TrackEvent ("Limitation popup shown", It.Is<Dictionary<string, string>> (
				d => d ["Plan"] == "PRO" && d ["Source"] == "ProjectsManager" && d ["Name"] == "Projects"),
			                                          null),Times.Once);

			service.Stop ();
		}

		[Test]
		public void UpgradeLinkClicked_SendTrackEvent ()
		{
			App.Current.EventsBroker.Publish (new UpgradeLinkClickedEvent {
				LimitationName = "Projects",
				Source = "LimitationWidget"
			});

			kpiServiceMock.Verify (m => m.TrackEvent ("Upgrade link clicked", It.Is<Dictionary<string, string>> (
				d => d ["Plan"] == "PRO" && d ["Source"] == "LimitationWidget" && d ["Name"] == "Projects"),
			                                          null), Times.Once);

			service.Stop ();
		}
	}
}
