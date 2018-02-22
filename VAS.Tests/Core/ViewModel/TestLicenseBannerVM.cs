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
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.Services;
using VAS.Core.Resources;
using VAS.Core.ViewModel;

namespace VAS.Tests.Core.ViewModel
{
	[TestFixture]
	public class TestLicenseBannerVM
	{
		LicenseBannerVM viewModel;
		IRegistry currentRegistry;
		Mock<IRegistry> mockRegistry;
		Mock<ILicenseCustomizationService> mockLicenseCustom;

		[SetUp]
		public void SetUp ()
		{
			currentRegistry = App.Current.DependencyRegistry;
			mockRegistry = new Mock<IRegistry> ();
			App.Current.DependencyRegistry = mockRegistry.Object;
			mockLicenseCustom = new Mock<ILicenseCustomizationService> ();
			mockRegistry.Setup (reg => reg.Retrieve<ILicenseCustomizationService> (InstanceType.New)).
						Returns (mockLicenseCustom.Object);
			viewModel = new LicenseBannerVM ();
		}

		[TearDown]
		public void TearDown ()
		{
			App.Current.DependencyRegistry = currentRegistry;
		}

		[Test]
		public void LicenseBannerVM_UpgradeCommand_InUpperCase ()
		{
			Assert.AreEqual (Strings.UpgradeNow.ToUpper (), viewModel.UpgradeCommand.Text);
		}

		[Test]
		public void LicenseBannerVM_ExecuteUpgradeCommand_OpensUrl ()
		{
			viewModel.UpgradeCommand.Execute ();

			mockLicenseCustom.Verify (lc => lc.OpenUpgradeURL (), Times.Once);
		}
	}
}
