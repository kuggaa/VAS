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
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.License;
using VAS.Core.License;
using VAS.Core.ViewModel;
using VAS.Services;

namespace VAS.Tests.Services
{
	[TestFixture ()]
	public class TestLicenseLimitationService
	{
		ILicenseLimitationsService service;
		CountLicenseLimitation limitationPlayers;
		CountLicenseLimitation limitationPlayers2;
		CountLicenseLimitation limitationTeams;
		Mock<ILicenseManager> mockLicenseManager;
		Mock<ILicenseStatus> mockLicenseStatus;

		[TestFixtureSetUp]
		public void Init ()
		{
			limitationPlayers = new CountLicenseLimitation { Enabled = true, Maximum = 10, Name = "RAPlayers" };
			limitationPlayers2 = new CountLicenseLimitation { Enabled = true, Maximum = 20, Name = "RAPlayers" };
			limitationTeams = new CountLicenseLimitation { Enabled = true, Maximum = 5, Name = "Teams" };

			mockLicenseManager = new Mock<ILicenseManager> ();
			mockLicenseStatus = new Mock<ILicenseStatus> ();
			App.Current.LicenseManager = mockLicenseManager.Object;
			mockLicenseManager.SetupGet (obj => obj.LicenseStatus).Returns (mockLicenseStatus.Object);
		}

		[SetUp]
		public void Setup ()
		{
			service = new DummyLicenseLimitationsService ();
			service.Start ();
		}

		[TearDown]
		public void TearDown ()
		{
			service.Stop ();
		}

		[Test]
		public void TestAddCountLimitations ()
		{
			service.Add (limitationPlayers);
			service.Add (limitationTeams);

			CountLimitationVM testLimitationPlayers = service.Get<CountLimitationVM> ("RAPlayers");
			CountLimitationVM testLimitationTeams = service.Get<CountLimitationVM> ("Teams");
			IEnumerable<LimitationVM> allLimitations = (service as DummyLicenseLimitationsService).GetAll ();

			Assert.AreEqual (2, allLimitations.Count ());
			Assert.IsTrue (testLimitationPlayers.Enabled);
			Assert.AreEqual (10, testLimitationPlayers.Maximum);
			Assert.IsTrue (testLimitationTeams.Enabled);
			Assert.AreEqual (5, testLimitationTeams.Maximum);
		}

		[Test]
		public void TestAddLimitationsRepeated ()
		{
			service.Add (limitationPlayers);
			Assert.Throws<InvalidOperationException> (() => service.Add (limitationPlayers2));

		}

		[Test]
		public void TestGetNonExisting_CountLimitation ()
		{
			CountLimitationVM limit = service.Get<CountLimitationVM> ("Non-existing limitation");
			IEnumerable<LimitationVM> allLimitations = (service as DummyLicenseLimitationsService).GetAll ();

			Assert.AreEqual (0, allLimitations.Count ());
			Assert.IsNull (limit);
		}

		[Test]
		public void TestDisabledLimitation ()
		{
			service.Add (new CountLicenseLimitation {
				Enabled = false,
				Maximum = 10,
				Count = 8,
				Name = "Disabled"
			});

			CountLimitationVM limitation = service.Get<CountLimitationVM> ("Disabled");
			IEnumerable<CountLimitationVM> allLimitations = (service as DummyLicenseLimitationsService).GetAll<CountLimitationVM> ();

			Assert.AreEqual (1, allLimitations.Count ());
			Assert.IsFalse (limitation.Enabled);
			Assert.AreEqual (int.MaxValue, limitation.Maximum);
			Assert.AreEqual (8, limitation.Count);
		}

		[Test]
		public void TestEnableLimitationWithLicenseChange ()
		{
			//Arrange
			mockLicenseStatus.SetupGet (obj => obj.Limited).Returns (true);

			//Act
			var limitation = new CountLicenseLimitation {
				Enabled = false,
				Maximum = 10,
				Count = 8,
				Name = "TestLimitation"
			};
			service.Add (limitation);
			App.Current.EventsBroker.Publish (new LicenseChangeEvent ());

			//Assert
			Assert.IsTrue (limitation.Enabled);
		}

		[Test]
		public void TestDisableLimitationWithLicenseChange ()
		{
			//Arrange
			mockLicenseStatus.SetupGet (obj => obj.Limited).Returns (false);

			//Act
			var limitation = new CountLicenseLimitation {
				Enabled = true,
				Maximum = 10,
				Count = 8,
				Name = "TestLimitation"
			};
			service.Add (limitation);
			App.Current.EventsBroker.Publish (new LicenseChangeEvent ());

			//Assert
			Assert.IsFalse (limitation.Enabled);
		}
	}
}
