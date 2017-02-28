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
using VAS.Core.Hotkeys;
using VAS.Services;

namespace VAS.Tests.Services
{
	[TestFixture]
	public class TestHotkeysService
	{
		HotkeysService hotkeysService;
		KeyConfig keyConfig1;
		KeyConfig keyConfig2;
		KeyConfig keyConfig1Dup;

		[TestFixtureSetUp]
		public void Init ()
		{
			keyConfig1 = new KeyConfig {
				Name = "KC1",
				Description = "KeyConfig 1 Description",
				Category = "Cat1",
				Key = App.Current.Keyboard.ParseName ("q")
			};
			keyConfig2 = new KeyConfig {
				Name = "KC2",
				Description = "KeyConfig 2 Description",
				Category = "Cat2",
				Key = App.Current.Keyboard.ParseName ("w")
			};
			keyConfig1Dup = new KeyConfig {
				Name = "KC1",
				Description = "KeyConfig 1 Duplicated",
				Category = "Cat1",
				Key = App.Current.Keyboard.ParseName ("t")
			};
		}

		[SetUp]
		public void Setup ()
		{
			hotkeysService = new HotkeysService ();
			hotkeysService.Start ();
		}

		[TearDown]
		public void TearDown ()
		{
			hotkeysService.Stop ();
		}

		[Test]
		public void TestRegisterKeyConfig ()
		{
			hotkeysService.Register (keyConfig1);

			Assert.AreSame (keyConfig1, hotkeysService.GetByName (keyConfig1.Name));
		}

		[Test]
		public void TestRegisterKeyConfigList ()
		{
			List<KeyConfig> list = new List<KeyConfig> { keyConfig1, keyConfig2 };

			hotkeysService.Register (list);

			Assert.AreSame (keyConfig1, hotkeysService.GetByName (keyConfig1.Name));
			Assert.AreSame (keyConfig2, hotkeysService.GetByName (keyConfig2.Name));
		}

		[Test]
		public void TestRegisterSameNameKeyThrowsException ()
		{
			hotkeysService.Register (keyConfig1);

			Assert.Throws<InvalidOperationException> (() => hotkeysService.Register (keyConfig1Dup));
			Assert.AreSame (keyConfig1, hotkeysService.GetByName (keyConfig1.Name));
		}

		[Test]
		public void TestRegisterSameNameKeyConfigListThrowsException ()
		{
			List<KeyConfig> list = new List<KeyConfig> { keyConfig2, keyConfig1Dup };

			hotkeysService.Register (keyConfig1);

			Assert.Throws<InvalidOperationException> (() => hotkeysService.Register (list));
			Assert.AreSame (keyConfig1, hotkeysService.GetByName (keyConfig1.Name));
		}

		[Test]
		public void TestGetByName ()
		{
			hotkeysService.Register (keyConfig1);
			Assert.AreSame (keyConfig1, hotkeysService.GetByName (keyConfig1.Name));
		}

		[Test]
		public void TestGetByNameUnexistant ()
		{
			hotkeysService.Register (keyConfig1);
			Assert.IsNull (hotkeysService.GetByName ("test"));
		}

		[Test]
		public void TestGetByCategory ()
		{
			var KeyConfigCat1 = new KeyConfig {
				Name = "KC1-2",
				Description = "KeyConfig 1-2 Description",
				Category = "Cat1",
				Key = App.Current.Keyboard.ParseName ("y")
			};
			List<KeyConfig> list = new List<KeyConfig> { keyConfig1, keyConfig2, KeyConfigCat1 };

			hotkeysService.Register (list);
			var cat1KeyConfigs = hotkeysService.GetByCategory ("Cat1");
			var cat2KeyConfigs = hotkeysService.GetByCategory ("Cat2");

			Assert.AreEqual (2, cat1KeyConfigs.Count ());
			Assert.AreEqual (1, cat2KeyConfigs.Count ());
			Assert.AreSame (cat2KeyConfigs.First (), keyConfig2);
		}

		[Test]
		public void TestGetByCategoryUnexistant ()
		{
			hotkeysService.Register (keyConfig1);

			var cat1KeyConfigs = hotkeysService.GetByCategory ("Cat2");

			Assert.IsNotNull (cat1KeyConfigs);
			Assert.IsFalse (cat1KeyConfigs.Any ());
		}
	}
}
