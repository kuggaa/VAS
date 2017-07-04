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
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Tests.Core.Hotkeys
{
	[TestFixture ()]
	public class TestHotkeyVM
	{
		Mock<IGUIToolkit> guiToolkitMock;
		HotKey hotkey;
		HotKeyVM viewModel;

		[TestFixtureSetUp]
		public void TestsInit ()
		{
			hotkey = new HotKey { Key = (int)App.Current.Keyboard.KeyvalFromName ("w"), Modifier = 100 };
			guiToolkitMock = new Mock<IGUIToolkit> ();
			guiToolkitMock.Setup (m => m.SelectHotkey (It.IsAny<HotKey> (), null)).Returns (hotkey);
			guiToolkitMock.SetupGet (o => o.DeviceScaleFactor).Returns (1.0f);
			App.Current.GUIToolkit = guiToolkitMock.Object;
		}

		[SetUp]
		public void SetUp ()
		{
			HotKey defaultHotKey = new HotKey { Key = (int)App.Current.Keyboard.KeyvalFromName ("m"), Modifier = 0 };
			viewModel = new HotKeyVM { Model = defaultHotKey };
		}

		[Test ()]
		public void TestChangeHotKeyProperties ()
		{
			//Action
			viewModel.UpdateHotkeyCommand.Execute ();

			//Assert
			Assert.AreEqual (hotkey.Modifier, viewModel.Modifier);
			Assert.AreEqual (hotkey.Key, viewModel.Key);
		}
	}
}
