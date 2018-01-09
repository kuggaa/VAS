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
using System.Dynamic;
using System.Linq;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.License;
using VAS.Core.License;
using VAS.Core.ViewModel;
using VAS.Services;
using VAS.Services.State;

namespace VAS.Tests.Services
{
	[TestFixture ()]
	public class TestLicenseLimitationService
	{
		ILicenseLimitationsService service;
		CountLicenseLimitation limitationPlayers;
		CountLicenseLimitation limitationPlayers2;
		CountLicenseLimitation limitationTeams;
		FeatureLicenseLimitation limitationFeature;
		FeatureLicenseLimitation limitationFeature2;
		FeatureLicenseLimitation limitationFeatureDisabled;
		Mock<ILicenseManager> mockLicenseManager;
		Mock<ILicenseStatus> mockLicenseStatus;
		IStateController currentStateController;
		Mock<IStateController> mockStateController;
		Mock<IScreenState> mockScreenState;

		[OneTimeSetUp]
		public void Init ()
		{
			currentStateController = App.Current.StateController;
			SetupClass.SetUp();
			limitationPlayers = new CountLicenseLimitation { Enabled = true, Maximum = 10, RegisterName = "RAPlayers" };
			limitationPlayers2 = new CountLicenseLimitation { Enabled = true, Maximum = 20, RegisterName = "RAPlayers" };
			limitationTeams = new CountLicenseLimitation { Enabled = true, Maximum = 5, RegisterName = "Teams" };
			limitationFeature = new FeatureLicenseLimitation { Enabled = true, RegisterName = "Feature 1" };
			limitationFeature2 = new FeatureLicenseLimitation { Enabled = true, RegisterName = "Feature 1" };
			limitationFeatureDisabled = new FeatureLicenseLimitation { Enabled = false, RegisterName = "Feature 2" };

			mockLicenseManager = new Mock<ILicenseManager> ();
			mockLicenseStatus = new Mock<ILicenseStatus> ();
			App.Current.LicenseManager = mockLicenseManager.Object;
			mockLicenseManager.SetupGet (obj => obj.LicenseStatus).Returns (mockLicenseStatus.Object);

			mockStateController = new Mock<IStateController> ();
			mockScreenState = new Mock<IScreenState> ();
			mockStateController.SetupGet (sc => sc.Current).Returns (mockScreenState.Object);
			App.Current.StateController = mockStateController.Object;
		}

		[OneTimeTearDown]
		public void OneTimeTearDown ()
		{
			App.Current.StateController = currentStateController;
		}

		[SetUp]
		public void Setup ()
		{
			service = new LicenseLimitationsService ();
			service.Start ();
		}

		[TearDown]
		public void TearDown ()
		{
			service.Stop ();
		}

		[Test]
		public void LimitationService_AddCountLimitations ()
		{
			service.Add (limitationPlayers);
			service.Add (limitationTeams);

			CountLimitationVM testLimitationPlayers = service.Get<CountLimitationVM> ("RAPlayers");
			CountLimitationVM testLimitationTeams = service.Get<CountLimitationVM> ("Teams");
			IEnumerable<LimitationVM> allLimitations = service.GetAll ();

			Assert.AreEqual (2, allLimitations.Count ());
			Assert.IsTrue (testLimitationPlayers.Enabled);
			Assert.AreEqual (10, testLimitationPlayers.Maximum);
			Assert.IsTrue (testLimitationTeams.Enabled);
			Assert.AreEqual (5, testLimitationTeams.Maximum);
		}

		[Test]
		public void LimitationService_AddFeatureLimitations ()
		{
			service.Add (limitationFeature);
			service.Add (limitationFeatureDisabled);

			FeatureLimitationVM testLimitationFeature1 = service.Get<FeatureLimitationVM> ("Feature 1");
			FeatureLimitationVM testLimitationFeature2 = service.Get<FeatureLimitationVM> ("Feature 2");
			IEnumerable<LimitationVM> allLimitations = service.GetAll ();

			Assert.AreEqual (2, allLimitations.Count ());
			Assert.IsTrue (testLimitationFeature1.Enabled);
			Assert.AreEqual (limitationFeature.RegisterName, testLimitationFeature1.RegisterName);
			Assert.IsFalse (testLimitationFeature2.Enabled);
			Assert.AreEqual (limitationFeatureDisabled.RegisterName, testLimitationFeature2.RegisterName);
		}

		[Test]
		public void LimitationService_AddCountLimitationsRepeated_ThrowsInvalidOperation ()
		{
			service.Add (limitationPlayers);
			Assert.Throws<InvalidOperationException> (() => service.Add (limitationPlayers2));

		}

		[Test]
		public void LimitationService_AddFeatureLimitationsRepeated_ThrowsInvalidOperation ()
		{
			service.Add (limitationFeature);
			Assert.Throws<InvalidOperationException> (() => service.Add (limitationFeature2));

		}

		[Test]
		public void LimitationService_AddFeatureCountLimitationsSameName_ThrowsInvalidOperation ()
		{
			service.Add (new CountLicenseLimitation {
				Enabled = false,
				Maximum = 10,
				Count = 8,
				RegisterName = limitationFeature.RegisterName
			});
			Assert.Throws<InvalidOperationException> (() => service.Add (limitationFeature));
		}

		[Test]
		public void LimitationService_GetNonExistingCountLimitation_ReturnsNull ()
		{
			CountLimitationVM limit = service.Get<CountLimitationVM> ("Non-existing limitation");
			IEnumerable<LimitationVM> allLimitations = service.GetAll ();

			Assert.AreEqual (0, allLimitations.Count ());
			Assert.IsNull (limit);
		}

		[Test]
		public void LimitationService_GetNonExistingFeatureLimitation_ReturnsNull ()
		{
			FeatureLimitationVM limit = service.Get<FeatureLimitationVM> ("Non-existing limitation");
			IEnumerable<LimitationVM> allLimitations = service.GetAll ();

			Assert.AreEqual (0, allLimitations.Count ());
			Assert.IsNull (limit);
		}

		[Test]
		public void LimitationService_GetDisabledLimitation ()
		{
			service.Add (new CountLicenseLimitation {
				Enabled = false,
				Maximum = 10,
				Count = 8,
				RegisterName = "Disabled"
			});

			CountLimitationVM limitation = service.Get<CountLimitationVM> ("Disabled");
			IEnumerable<CountLimitationVM> allLimitations = service.GetAll<CountLimitationVM> ();

			Assert.AreEqual (1, allLimitations.Count ());
			Assert.IsFalse (limitation.Enabled);
			Assert.AreEqual (int.MaxValue, limitation.Maximum);
			Assert.AreEqual (8, limitation.Count);
		}

		[Test]
		public void LimitationService_LicenseChangeEvent_EnablesLimitation ()
		{
			//Arrange
			mockLicenseStatus.SetupGet (obj => obj.Limitations).Returns ("TestLimitation".ToEnumerable ());

			//Act
			var limitation = new CountLicenseLimitation {
				Enabled = false,
				Maximum = 10,
				Count = 8,
				RegisterName = "TestLimitation"
			};
			service.Add (limitation);
			App.Current.EventsBroker.Publish (new LicenseChangeEvent ());

			//Assert
			Assert.IsTrue (limitation.Enabled);
		}

		[Test]
		public void LimitationService_LicenseChangeEvent_DisablesLimitation ()
		{
			//Arrange
			mockLicenseStatus.SetupGet (obj => obj.Limitations).Returns (new List<string> ());

			//Act
			var limitation = new CountLicenseLimitation {
				Enabled = true,
				Maximum = 10,
				Count = 8,
				RegisterName = "TestLimitation"
			};
			service.Add (limitation);
			App.Current.EventsBroker.Publish (new LicenseChangeEvent ());

			//Assert
			Assert.IsFalse (limitation.Enabled);
		}

		[Test]
		public void LimitationService_FeatureLimitationDisabled_CanExecuteFuture ()
		{
			service.Add (limitationFeatureDisabled);

			var featureLimitationVM = service.Get<FeatureLimitationVM> (limitationFeatureDisabled.RegisterName);

			Assert.IsTrue (service.CanExecute (featureLimitationVM.RegisterName));
		}

		[Test]
		public void LimitationService_FeatureLimitationEnabled_CanNOTExecuteFuture ()
		{
			service.Add (limitationFeature);

			var featureLimitationVM = service.Get<FeatureLimitationVM> (limitationFeature.RegisterName);

			Assert.IsFalse (service.CanExecute (featureLimitationVM.RegisterName));
		}

		[Test]
		public void LimitationService_FeatureLimitationNotExists_CanExecuteFuture ()
		{
			Assert.IsTrue (service.CanExecute ("Non-Existing-Limitation"));
		}

		[Test]
		public void LimitationService_FeatureLimitationNotExists_DoNotMoveToUpgradeDialog ()
		{
			mockStateController.Setup (sc => sc.MoveToModal (UpgradeLimitationState.NAME, It.IsAny<object> (), false)).Returns (AsyncHelpers.Return (true));

			service.MoveToUpgradeDialog ("Non-Existing-Limitation");

			mockStateController.Verify (sc => sc.MoveToModal (UpgradeLimitationState.NAME, It.IsAny<object> (), false), Times.Never);
		}

		[Test]
		public void LimitationService_LimitationEnabled_MoveToUpgradeDialogSuccess ()
		{
			string sourceState = null;
			string limitationName = null;
			service.Add (limitationFeature);
			mockScreenState.SetupGet (st => st.Name).Returns ("Home");
			mockStateController.Setup (sc => sc.MoveToModal (UpgradeLimitationState.NAME, It.IsAny<object> (), false)).Returns (AsyncHelpers.Return (true));
			var token = App.Current.EventsBroker.Subscribe<LimitationDialogShownEvent> ((e) => {
				sourceState = e.Source;
				limitationName = e.LimitationName;
			});

			var limitVM = service.Get<FeatureLimitationVM> (limitationFeature.RegisterName);
			service.MoveToUpgradeDialog (limitationFeature.RegisterName);

			mockStateController.Verify (sc => sc.MoveToModal (UpgradeLimitationState.NAME, It.Is<object> (
				obj => IslimitationVMEqual (obj, limitVM)), false), Times.Once);
			Assert.AreEqual ("Home", sourceState);
			Assert.AreEqual (limitationFeature.RegisterName, limitationName);

			App.Current.EventsBroker.Unsubscribe<LimitationDialogShownEvent> (token);
		}

		[Test]
		public void LimitationService_LimitationDisabled_DoNotMoveToUpgradeDialog ()
		{
			service.Add (limitationFeatureDisabled);
			mockStateController.Setup (sc => sc.MoveToModal (UpgradeLimitationState.NAME, It.IsAny<object> (), false)).Returns (AsyncHelpers.Return (true));

			service.MoveToUpgradeDialog (limitationFeatureDisabled.RegisterName);

			mockStateController.Verify (sc => sc.MoveToModal (UpgradeLimitationState.NAME, It.IsAny<object> (), false), Times.Never);
		}

		bool IslimitationVMEqual (object obj, LimitationVM limitVM)
		{
			var limitVMfirst = (obj as dynamic).limitationVM;

			return limitVMfirst == limitVM;
		}
	}
}
