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
using NUnit.Framework;
using VAS.Core.Interfaces;
using VAS.Core.License;
using VAS.Core.ViewModel;
using VAS.Services;

namespace VAS.Tests.Services
{
	[TestFixture ()]
	public class TestLicenseLimitationService
	{
		ILicenseLimitationsService service;
		LicenseLimitation limitationPlayers;
		LicenseLimitation limitationPlayers2;
		LicenseLimitation limitationTeams;

		[TestFixtureSetUp]
		public void Init ()
		{
			limitationPlayers = new LicenseLimitation { Enabled = true, Maximum = 10, Name = "RAPlayers" };
			limitationPlayers2 = new LicenseLimitation { Enabled = true, Maximum = 20, Name = "RAPlayers" };
			limitationTeams = new LicenseLimitation { Enabled = true, Maximum = 5, Name = "Teams" };
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
		public void TestAddLimitations ()
		{
			service.Add (limitationPlayers);
			service.Add (limitationTeams);

			LicenseLimitationVM testLimitationPlayers = service.Get ("RAPlayers");
			LicenseLimitationVM testLimitationTeams = service.Get ("Teams");
			IEnumerable<LicenseLimitationVM> allLimitations = (service as DummyLicenseLimitationsService).GetAll ();

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
			service.Add (limitationPlayers2);
			service.Add (limitationTeams);

			LicenseLimitationVM testLimitationPlayers = service.Get ("RAPlayers");
			LicenseLimitationVM testLimitationTeams = service.Get ("Teams");
			IEnumerable<LicenseLimitationVM> allLimitations = (service as DummyLicenseLimitationsService).GetAll ();

			Assert.AreEqual (2, allLimitations.Count ());
			Assert.IsTrue (testLimitationPlayers.Enabled);
			Assert.AreEqual (20, testLimitationPlayers.Maximum);
			Assert.IsTrue (testLimitationTeams.Enabled);
			Assert.AreEqual (5, testLimitationTeams.Maximum);
		}

		[Test]
		public void TestGetNonExisting ()
		{
			LicenseLimitationVM limit = service.Get ("Non-existing limitation");
			IEnumerable<LicenseLimitationVM> allLimitations = (service as DummyLicenseLimitationsService).GetAll ();

			Assert.AreEqual (0, allLimitations.Count ());
			Assert.IsNull (limit);
		}

		[Test]
		public void TestDisabledLimitation ()
		{
			service.Add (new LicenseLimitation {
				Enabled = false,
				Maximum = 10,
				Count = 8,
				Name = "Disabled"
			});

			LicenseLimitationVM limitation = service.Get ("Disabled");
			IEnumerable<LicenseLimitationVM> allLimitations = (service as DummyLicenseLimitationsService).GetAll ();

			Assert.AreEqual (1, allLimitations.Count ());
			Assert.IsFalse (limitation.Enabled);
			Assert.AreEqual (int.MaxValue, limitation.Maximum);
			Assert.AreEqual (8, limitation.Count);
		}
	}
}
