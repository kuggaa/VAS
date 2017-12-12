//
//  Copyright (C) 2017 
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
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Interfaces;

namespace VAS.Tests.Core.Common
{
	[TestFixture]
	public class TestLog
	{
		Mock<IKpiService> mock;

		[OneTimeSetUp]
		public void FixtureSetUp ()
		{
			mock = new Mock<IKpiService> ();
			App.Current.KPIService = mock.Object;
		}

		[TearDown]
		public void TearDown ()
		{
			mock.ResetCalls ();
		}

		[Test]
		public void LogException_TrackFalse_TrackedAsEvent ()
		{
			Exception e = new InvalidCastException ();

			Log.Exception (e, false);

			mock.Verify (kpi => kpi.TrackEvent ("LogException", It.IsAny<Dictionary<string, string>> (), null),
						 Times.Once ());
			mock.Verify (kpi => kpi.TrackException (It.IsAny<Exception> (), It.IsAny<Dictionary<string, string>> ()),
						 Times.Never ());
		}

		[Test]
		public void LogException_TrackTrue_TrackedAsException ()
		{
			Exception e = new InvalidCastException ();

			try {
				throw e;
			} catch {
				Log.Exception (e, true);
			}

			mock.Verify (kpi => kpi.TrackEvent ("LogException", It.IsAny<Dictionary<string, string>> (), null),
						 Times.Never ());
			mock.Verify (kpi => kpi.TrackException (e, It.IsAny<Dictionary<string, string>> ()),
						 Times.Once ());
		}
	}
}
