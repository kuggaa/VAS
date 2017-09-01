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
using System;
using Gtk;
using Moq;
using NUnit.Framework;
using VAS.Core.Interfaces;
using VAS.Core.MVVMC;
using VAS.UI.Helpers.Bindings;

namespace VAS.Tests.Gtk2.MVVMC
{
	[TestFixture]
	public class TestHScaleCommandBinding
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
		public void HScale_LimitationNotEnabled_CommandExecuted ()
		{
			double val = -1;
			LimitationCommand<double> command = new LimitationCommand<double> ("Test", (obj) => val = obj);
			HScale scale = new HScale (1,4,1);
			HScaleCommandBinding binding = new HScaleCommandBinding (scale, (vm) => command, 1);
			binding.ViewModel = new DummyViewModel ();
			mockLimitationService.Setup (s => s.CanExecute ("Test")).Returns (true);

			scale.Value = 2;

			Assert.AreEqual (2, val);
		}

		[Test]
		public void HScale_LimitationEnabled_CommandNotExecuted ()
		{
			double val = -1;
			LimitationCommand<double> command = new LimitationCommand<double> ("Test", (obj) => val = obj);
			HScale scale = new HScale (1, 4, 1);
			HScaleCommandBinding binding = new HScaleCommandBinding (scale, (vm) => command, 1);
			binding.ViewModel = new DummyViewModel ();
			mockLimitationService.Setup (s => s.CanExecute ("Test")).Returns (false);

			scale.Value = 2;

			Assert.AreEqual (-1, val);
		}

		[Test]
		public void HScale_LimitationEnabled_ReturnsToDefaultValue ()
		{
			LimitationCommand<double> command = new LimitationCommand<double> ("Test", (obj) => {});
			HScale scale = new HScale (1, 4, 1);
			HScaleCommandBinding binding = new HScaleCommandBinding (scale, (vm) => command, 1);
			binding.ViewModel = new DummyViewModel ();
			mockLimitationService.Setup (s => s.CanExecute ("Test")).Returns (false);

			scale.Value = 2;

			Assert.AreEqual (1, scale.Value);
		}

		[Test]
		public void HScale_LimitationEnabled_ExecuteCalledOnce ()
		{
			LimitationCommand<double> command = new LimitationCommand<double> ("Test", (obj) => { });
			HScale scale = new HScale (1, 4, 1);
			HScaleCommandBinding binding = new HScaleCommandBinding (scale, (vm) => command, 1);
			binding.ViewModel = new DummyViewModel ();
			mockLimitationService.Setup (s => s.CanExecute ("Test")).Returns (false);

			scale.Value = 2;

			mockLimitationService.Verify ((s) => s.MoveToUpgradeDialog ("Test"),Times.Once);
		}
	}
}
