//
//  Copyright (C) 2017 FLUENDO S.A.
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
using System.ComponentModel;
using Gtk;
using Moq;
using NUnit.Framework;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.UI.Helpers.Bindings;

namespace VAS.Tests.Gtk2.MVVMC
{
	[TestFixture]
	public class TestToggleButtonBinding
	{
		Mock<ILicenseLimitationsService> mockLimitationService;

		[SetUp]
		public void SetUp ()
		{
			App.Current = new AppDummy ();
			mockLimitationService = new Mock<ILicenseLimitationsService> ();
			App.Current.LicenseLimitationsService = mockLimitationService.Object;
		}

		[Test]
		public void BindView_LimitationEnabledCommandNotExecuted_ButtonNotActiveOrToggled ()
		{
			// Arrange
			bool limitedCommandExecuted = false;
			LimitationCommand command = new LimitationCommand ("Test", (obj) => limitedCommandExecuted = true);
			ToggleButton button = new ToggleButton ();
			ToggleButtonBinding binding = new ToggleButtonBinding (button, (vm) => command, true, false);
			binding.ViewModel = new DummyViewModel ();

			int toggled = 0;
			button.Toggled += (sender, e) => toggled++;

			mockLimitationService.Setup (s => s.CanExecute ("Test")).Returns (false);

			// Act
			button.Toggle ();

			// Assert
			Assert.AreEqual (limitedCommandExecuted, command.Executed);
			Assert.IsFalse (button.Active);
			Assert.AreEqual (2, toggled);
		}

		[Test]
		public void BindView_LimitationNotEnabledCommandExecuted_ButtonActive ()
		{
			// Arrange
			bool limitedCommandExecuted = false;
			LimitationCommand command = new LimitationCommand ("Test", (obj) => limitedCommandExecuted = true);
			ToggleButton button = new ToggleButton ();
			ToggleButtonBinding binding = new ToggleButtonBinding (button, (vm) => command, true, false);
			binding.ViewModel = new DummyViewModel ();

			int toggled = 0;
			button.Toggled += (sender, e) => toggled++;

			mockLimitationService.Setup (s => s.CanExecute ("Test")).Returns (true);

			// Act
			button.Toggle ();

			// Assert
			Assert.AreEqual (limitedCommandExecuted, command.Executed);
			Assert.AreEqual (1, toggled);
		}
	}

	class DummyViewModel : IViewModel
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
	}

	public class AppDummy : App
	{
		//Dummy class for VAS.App
	}
}
