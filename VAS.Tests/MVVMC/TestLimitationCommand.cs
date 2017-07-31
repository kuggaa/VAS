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
		public void LimitationCommand_InitExecutable ()
		{
			var command = new LimitationCommand (limitationName, (obj) => { });

			Assert.IsTrue (command.CanExecute ());
		}

		[Test]
		public void LimitationCommand_CanExecute_CommandOK ()
		{
			var command = new LimitationCommand (limitationName, () => { }, () => true);

			Assert.IsTrue (command.CanExecute (true));
		}

		[Test]
		public void LimitationCommand_CanExecute_CommandParametrizedCanExecuteOK ()
		{
			var command = new LimitationCommand<bool> (limitationName, o => { }, o => o);

			Assert.IsTrue (command.CanExecute (true));
		}

		[Test]
		public void LimitationCommand_CanExecute_GenricCommandOK ()
		{
			var command = new LimitationCommand<bool> (limitationName, (o) => { }, () => true);

			Assert.IsTrue (command.CanExecute ());
		}

		[Test]
		public void LimitationCommand_CanExecute_GenericCommandParametrizedCanExecuteOK ()
		{
			var command = new LimitationCommand<bool> (limitationName, o => { }, o => o);

			Assert.IsTrue (command.CanExecute (true));
		}


		[Test]
		public void LimitationCommand_CanExecute_AsyncCommandParametrizedCanExecuteOK ()
		{
			var command = new LimitationAsyncCommand (limitationName, AsyncHelpers.Return, (o) => true);

			Assert.IsTrue (command.CanExecute (true));
		}

		[Test]
		public void LimitationCommand_CanExecute_AsyncCommandOK ()
		{
			var command = new LimitationAsyncCommand (limitationName, AsyncHelpers.Return, () => true);

			Assert.IsTrue (command.CanExecute ());
		}

		[Test]
		public void LimitationCommand_CanExecute_GenericAsyncCommandParametrizedCanExecuteOK ()
		{
			var command = new LimitationAsyncCommand<bool> (limitationName, o => { return AsyncHelpers.Return (o); }, (o) => true);

			Assert.IsTrue (command.CanExecute (true));
		}

		[Test]
		public void LimitationCommand_CanExecute_GenericAsyncCommandOK ()
		{
			var command = new LimitationAsyncCommand<bool> (limitationName, o => { return AsyncHelpers.Return (o); }, () => true);

			Assert.IsTrue (command.CanExecute ());
		}

		[Test]
		public void LimitationCommand_LimitationNotInitialized_Executed ()
		{
			bool executed = false;
			App.Current.LicenseLimitationsService = new DummyLicenseLimitationsService ();
			var command = new LimitationCommand (limitationName, () => { executed = true; });

			command.Execute ();
			Assert.IsTrue (executed);
		}

		[Test]
		public void LimitationCommand_LimitationCanExecute_Executed ()
		{
			bool executed = false;
			mockLimitationService.Setup (lim => lim.CanExecuteFeature (limitationName)).Returns (true);
			var command = new LimitationCommand (limitationName, () => { executed = true; });

			command.Execute ();

			Assert.IsTrue (executed);
			mockLimitationService.Verify (lim => lim.MoveToUpgradeDialog (limitationName), Times.Never);
		}

		[Test]
		public void LimitationCommand_LimitationCanNotExecute_MoveToUpgradeDialog ()
		{
			bool executed = false;
			mockLimitationService.Setup (lim => lim.CanExecuteFeature (limitationName)).Returns (false);
			var command = new LimitationCommand (limitationName, () => { executed = true; });

			command.Execute ();

			Assert.IsFalse (executed);
			mockLimitationService.Verify (lim => lim.MoveToUpgradeDialog (limitationName), Times.Once);
		}
	}
}
