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
using System.Linq;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
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
		}

		[TearDown ()]
		public void TearDown ()
		{
			Service.Stop ();
		}

		[Test]
		public void TestCountUsageEvents_PlaylistAmount ()
		{
			// Action
			App.Current.EventsBroker.Publish (new NewPlaylistEvent ());

			// Assert
			Assert.AreEqual (1, Service.PlaylistsAmount);
		}

		[Test]
		public void TestCountUsageEvents_RendersAmount ()
		{
			// Action
			App.Current.EventsBroker.Publish (new CreateEvent<Job> ());

			// Assert
			Assert.AreEqual (1, Service.RendersAmount);
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
			Assert.AreEqual (1, Service.ManualTagsAmount,
							 $"Manual tags amount was {Service.ManualTagsAmount} and expected 1");
			Assert.AreEqual (1, Service.ProjectDictionary.Count,
							 $"Project dictionary has {Service.ProjectDictionary.Count} entrace and expected 1");
			Assert.AreEqual (Project.ID, Service.ProjectDictionary.FirstOrDefault ().Key,
							 $"Project dictionary key was {Service.ProjectDictionary.FirstOrDefault ().Key} and expected {Project.ID}");
			Assert.AreEqual (1, Service.ProjectDictionary.FirstOrDefault ().Value.Item1,
							 $"Project dictionary manual tags value was {Service.ProjectDictionary.FirstOrDefault ().Value.Item1} and expected 1");
			Assert.AreEqual (0, Service.ProjectDictionary.FirstOrDefault ().Value.Item2,
							 $"Project dictionary drawings value was {Service.ProjectDictionary.FirstOrDefault ().Value.Item2} and expected 0");
		}

		[Test]
		public void TestCountUsageEvents_NewDashboard_Reset ()
		{
			// Arrange
			App.Current.EventsBroker.Publish (
				new NewDashboardEvent {
					ProjectId = Project.ID
				}
			);

			// Action
			App.Current.EventsBroker.Publish (new CreateProjectEvent ());

			// Assert
			Assert.AreEqual (0, Service.ManualTagsAmount,
							 $"Manual tags amount was {Service.ManualTagsAmount} and expected 0");
			Assert.AreEqual (1, Service.ProjectDictionary.Count,
							 $"Project dictionary has {Service.ProjectDictionary.Count} entrace and expected 1");
			Assert.AreEqual (Project.ID, Service.ProjectDictionary.FirstOrDefault ().Key,
							 $"Project dictionary key was {Service.ProjectDictionary.FirstOrDefault ().Key} and expected {Project.ID}");
			Assert.AreEqual (1, Service.ProjectDictionary.FirstOrDefault ().Value.Item1,
							 $"Project dictionary manual tags value was {Service.ProjectDictionary.FirstOrDefault ().Value.Item1} and expected 1");
			Assert.AreEqual (0, Service.ProjectDictionary.FirstOrDefault ().Value.Item2,
							 $"Project dictionary drawings value was {Service.ProjectDictionary.FirstOrDefault ().Value.Item2} and expected 0");
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

			// Assert
			Assert.AreEqual (1, Service.DrawingsAmount,
							 $"Drawing amount was {Service.DrawingsAmount} and expected 1");
			Assert.AreEqual (1, Service.ProjectDictionary.Count,
							 $"Project dictionary has {Service.ProjectDictionary.Count} entrace and expected 1");
			Assert.AreEqual (Project.ID, Service.ProjectDictionary.FirstOrDefault ().Key,
							 $"Project dictionary key was {Service.ProjectDictionary.FirstOrDefault ().Key} and expected {Project.ID}");
			Assert.AreEqual (0, Service.ProjectDictionary.FirstOrDefault ().Value.Item1,
							 $"Project dictionary manual tags value was {Service.ProjectDictionary.FirstOrDefault ().Value.Item1} and expected 0");
			Assert.AreEqual (1, Service.ProjectDictionary.FirstOrDefault ().Value.Item2,
							 $"Project dictionary drawings value was {Service.ProjectDictionary.FirstOrDefault ().Value.Item2} and expected 1");
		}

		[Test]
		public void TestCountUsageEvents_DrawingSavedToProject_Reset ()
		{
			// Arrange
			App.Current.EventsBroker.Publish (
				new DrawingSavedToProjectEvent {
					ProjectId = Project.ID
				}
			);

			// Action
			App.Current.EventsBroker.Publish (new CreateProjectEvent ());

			// Assert
			Assert.AreEqual (0, Service.DrawingsAmount,
							 $"Drawing amount was {Service.DrawingsAmount} and expected 0");
			Assert.AreEqual (1, Service.ProjectDictionary.Count,
							 $"Project dictionary has {Service.ProjectDictionary.Count} entrace and expected 1");
			Assert.AreEqual (Project.ID, Service.ProjectDictionary.FirstOrDefault ().Key,
							 $"Project dictionary key was {Service.ProjectDictionary.FirstOrDefault ().Key} and expected {Project.ID}");
			Assert.AreEqual (0, Service.ProjectDictionary.FirstOrDefault ().Value.Item1,
							 $"Project dictionary manual tags value was {Service.ProjectDictionary.FirstOrDefault ().Value.Item1} and expected 0");
			Assert.AreEqual (1, Service.ProjectDictionary.FirstOrDefault ().Value.Item2,
							 $"Project dictionary drawings value was {Service.ProjectDictionary.FirstOrDefault ().Value.Item2} and expected 1");
		}

		[Test]
		public void TestCountUsageEvents_CreateProject ()
		{
			// Action
			App.Current.EventsBroker.Publish (new CreateProjectEvent ());

			// Assert
			Assert.AreEqual (1, Service.CreatedProjects);
		}
	}
}
