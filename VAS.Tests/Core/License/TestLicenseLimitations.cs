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
using NUnit.Framework;
using VAS.Core.License;
using VAS.Core.Store;
using VAS.Core.Store.Templates;

namespace VAS.Tests.Core.License
{
	[TestFixture ()]
	public class TestLicenseLimitations
	{
		LicenseLimitations<LicenseLimitation> limitations;
		LicenseLimitation limitationPlayers;
		LicenseLimitation limitationPlayers2;
		LicenseLimitation limitationTeams;

		[TestFixtureSetUp]
		public void Setup ()
		{
			limitations = new LicenseLimitations<LicenseLimitation> ();
			limitationPlayers = new LicenseLimitation { Enabled = true, Maximum = 10, LimitationName = "RAPlayers" };
			limitationPlayers2 = new LicenseLimitation { Enabled = true, Maximum = 20, LimitationName = "RAPlayers" };
			limitationTeams = new LicenseLimitation { Enabled = true, Maximum = 5, LimitationName = "Teams" };
		}

		[Test ()]
		public void TestAddLimitations ()
		{
			limitations.AddLimitation (limitationPlayers);
			limitations.AddLimitation (limitationPlayers2);
			limitations.AddLimitation (limitationTeams);

			List<LicenseLimitation> limitationsPlayers =
				new List<LicenseLimitation> (limitations.GetLimitations ("RAPlayers"));
			List<LicenseLimitation> limitationsTeams =
				new List<LicenseLimitation> (limitations.GetLimitations ("Teams"));
			List<LicenseLimitation> allLlimitations =
				new List<LicenseLimitation> (limitations.GetLimitations ());

			Assert.AreEqual (2, limitationsPlayers.Count);
			Assert.IsTrue (limitationsPlayers [0].Enabled);
			Assert.AreEqual (10, limitationsPlayers [0].Maximum);
			Assert.IsTrue (limitationsPlayers [1].Enabled);
			Assert.AreEqual (20, limitationsPlayers [1].Maximum);
			Assert.AreEqual (1, limitationsTeams.Count);
			Assert.IsTrue (limitationsTeams [0].Enabled);
			Assert.AreEqual (5, limitationsTeams [0].Maximum);
			Assert.AreEqual (3, allLlimitations.Count);
		}
	}
}
