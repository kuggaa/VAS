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
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Store;

namespace VAS.Tests.Services
{
	[TestFixture]
	public class TestUserStatisticsService
	{
		DummyUserStatisticsService Service;
		Mock<IStorageManager> storageManagerMock;
		Mock<IStorage> storageMock;
		Mock<IKpiService> kpiServiceMock;
		Project Project;

		[SetUp]
		public void Setup ()
		{
			Service = new DummyUserStatisticsService ();
			Service.Start ();
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
			Service.Stop ();
			kpiServiceMock.Verify (m => m.TrackEvent ("Sessions", null, It.Is<Dictionary<string, double>> (
				d => d ["Playlists"] == 1)), Times.Once ());
		}

		[Test]
		public void TestCountUsageEvents_RendersAmount ()
		{
			// Action
			App.Current.EventsBroker.Publish (new JobRenderedEvent ());

			// Assert
			Service.Stop ();
			kpiServiceMock.Verify (m => m.TrackEvent ("Sessions", null, It.Is<Dictionary<string, double>> (
				d => d ["Renders"] == 1)), Times.Once ());
		}

		[Test]
		public void TestCountUsageEvents_NewDashboard ()
		{
			// Action
			App.Current.EventsBroker.Publish (
				new NewDashboardEvent {
					ProjectId = Project.ID
				}
			);

			// Assert
			Service.Stop ();
			kpiServiceMock.Verify (m => m.TrackEvent ("Project_usage",
													  It.Is<Dictionary<string, string>> (
														  d => d ["Project_id"] == Project.ID.ToString ()),
													  It.Is<Dictionary<string, double>> (
														  d => (d ["Events"] == 1) && (d ["Drawings"] == 0))),
								  Times.Once);
		}

		[Test]
		public void TestCountUsageEvents_DrawingSavedToProject ()
		{
			// Action
			App.Current.EventsBroker.Publish (
				new DrawingSavedToProjectEvent {
					ProjectId = Project.ID
				}
			);

			Service.Stop ();
			kpiServiceMock.Verify (m => m.TrackEvent ("Project_usage",
													  It.Is<Dictionary<string, string>> (
														  d => d ["Project_id"] == Project.ID.ToString ()),
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
			Service.Stop ();
			kpiServiceMock.Verify (m => m.TrackEvent ("Sessions", null, It.Is<Dictionary<string, double>> (
				d => d ["Projects"] == 1)), Times.Once ());
		}
	}
}
