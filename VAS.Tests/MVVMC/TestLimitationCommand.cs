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
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Interfaces;
using VAS.Core.MVVMC;

namespace VAS.Tests.MVVMC
{
	[TestFixture]
	public class TestLimitationCommand
	{
		string limitationName = "limitationTest";
		Mock<ILicenseLimitationsService> mockLimitationService;
		ILicenseLimitationsService currentService;

		[TestFixtureSetUp]
		public void TestFixtureSetUp ()
		{
			currentService = App.Current.LicenseLimitationsService;
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown ()
		{
			App.Current.LicenseLimitationsService = currentService;
		}

		[SetUp]
		public void SetUp ()
		{
			mockLimitationService = new Mock<ILicenseLimitationsService> ();
			App.Current.LicenseLimitationsService = mockLimitationService.Object;
		}

		[Test]
		public void CanExecute_InitExecutable_Ok ()
		{
			var command = new LimitationCommand (limitationName, (obj) => { });

			Assert.IsTrue (command.CanExecute ());
		}

		[Test]
		public void CanExecute_Command_OK ()
		{
			var command = new LimitationCommand (limitationName, () => { }, () => true);

			Assert.IsTrue (command.CanExecute (true));
		}

		[Test]
		public void CanExecute_CommandParametrized_CanExecuteOK ()
		{
			var command = new LimitationCommand<bool> (limitationName, o => { }, o => o);

			Assert.IsTrue (command.CanExecute (true));
		}

		[Test]
		public void CanExecute_GenricCommand_OK ()
		{
			var command = new LimitationCommand<bool> (limitationName, (o) => { }, () => true);

			Assert.IsTrue (command.CanExecute ());
		}

		[Test]
		public void CanExecute_GenericCommandParametrized_CanExecuteOK ()
		{
			var command = new LimitationCommand<bool> (limitationName, o => { }, o => o);

			Assert.IsTrue (command.CanExecute (true));
		}


		[Test]
		public void CanExecute_AsyncCommandParametrized_OK ()
		{
			var command = new LimitationAsyncCommand (limitationName, AsyncHelpers.Return, (o) => true);

			Assert.IsTrue (command.CanExecute (true));
		}

		[Test]
		public void CanExecute_AsyncCommand_OK ()
		{
			var command = new LimitationAsyncCommand (limitationName, AsyncHelpers.Return, () => true);

			Assert.IsTrue (command.CanExecute ());
		}

		[Test]
		public void CanExecute_GenericAsyncCommandParametrized_OK ()
		{
			var command = new LimitationAsyncCommand<bool> (limitationName, o => { return AsyncHelpers.Return (o); }, (o) => true);

			Assert.IsTrue (command.CanExecute (true));
		}

		[Test]
		public void CanExecute_GenericAsyncCommand_OK ()
		{
			// Arrange
			var command = new LimitationAsyncCommand<bool> (limitationName, o => { return AsyncHelpers.Return (o); }, () => true);

			// Act & Assert
			Assert.IsTrue (command.CanExecute ());
		}

		[Test]
		public void Execute_LimitationNotInitialized_Executed ()
		{
			// Arrange
			bool executed = false;
			App.Current.LicenseLimitationsService = new DummyLicenseLimitationsService ();
			var command = new LimitationCommand (limitationName, () => { executed = true; });

			// Act
			command.Execute ();

			// Assert
			Assert.IsTrue (executed);
		}

		[Test]
		public void Execute_LimitationCanExecute_Executed ()
		{
			// Arrange
			bool executed = false;
			mockLimitationService.Setup (lim => lim.CanExecute (limitationName)).Returns (true);
			var command = new LimitationCommand (limitationName, () => { executed = true; });

			// Act
			command.Execute ();

			// Assert
			Assert.IsTrue (executed);
			mockLimitationService.Verify (lim => lim.MoveToUpgradeDialog (limitationName), Times.Never);
		}

		[Test]
		public void Execute_LimitationCanNotExecute_MoveToUpgradeDialog ()
		{
			// Arrange
			bool executed = false;
			mockLimitationService.Setup (lim => lim.CanExecute (limitationName)).Returns (false);
			var command = new LimitationCommand (limitationName, () => { executed = true; });

			// Act
			command.Execute ();

			// assert
			Assert.IsFalse (executed);
			mockLimitationService.Verify (lim => lim.MoveToUpgradeDialog (limitationName), Times.Once);
		}

		[Test]
		public void Execute_ConditionSetToApplyLimit_MoveToUpgradeDialog ()
		{
			// Arrange
			bool executed = false;
			mockLimitationService.Setup (lim => lim.CanExecuteFeature (limitationName)).Returns (false);
			var command = new LimitationCommand (limitationName, () => { executed = true; });
			command.LimitationCondition = () => true;

			// Act
			command.Execute ();

			// assert
			Assert.IsFalse (executed);
			mockLimitationService.Verify (lim => lim.MoveToUpgradeDialog (limitationName), Times.Once);
		}

		[Test]
		public void Execute_ConditionSetToApplyLimitButFeatureCan_ExecuteOk ()
		{
			// Arrange
			bool executed = false;
			mockLimitationService.Setup (lim => lim.CanExecuteFeature (limitationName)).Returns (true);
			var command = new LimitationCommand (limitationName, () => { executed = true; });
			command.LimitationCondition = () => true;

			// Act
			command.Execute ();

			// assert
			Assert.IsTrue (executed);
			mockLimitationService.Verify (lim => lim.MoveToUpgradeDialog (limitationName), Times.Never);
		}

		[Test]
		public void Execute_FeatureCanExecuteButConditionSetToNotApplyLimit_ExecuteOk ()
		{
			// Arrange
			bool executed = false;
			mockLimitationService.Setup (lim => lim.CanExecuteFeature (limitationName)).Returns (false);
			var command = new LimitationCommand (limitationName, () => { executed = true; });
			command.LimitationCondition = () => false;

			// Act
			command.Execute ();

			// assert
			Assert.IsTrue (executed);
			mockLimitationService.Verify (lim => lim.MoveToUpgradeDialog (limitationName), Times.Never);
		}
	}
}
