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
using NUnit.Framework;
using VAS.Core.Common;
namespace VAS.Tests.Core.Common
{
	[TestFixture]
	public class TestKeyboard
	{
		Keyboard keyboard;
		string [] modifiers = {
			"Shift_L", "Shift_R", "Shift",
			"Alt_L", "Alt_R", "Alt",
			"Control_L", "Control_R", "Control",
			"Meta_L", "Meta_R", "Meta",
			"Primary"
		};

		[TestFixtureSetUp]
		public void SetUpOnce ()
		{
			keyboard = new Keyboard ();
		}

		[Test]
		public void ParseName_OnlyModifier_ReturnsNull ()
		{
			foreach (string modifier in modifiers) {
				Assert.IsNull (keyboard.ParseName ($"<{modifier}>"));
			}
		}

		[Test]
		public void ParseName_OnlyKey_OK ()
		{
			var hotkey = keyboard.ParseName ("s");

			Assert.AreEqual ((int)Gdk.Key.s, hotkey.Key);
		}

		[Test]
		public void ParseName_KeyAndPrimary_ConvertedToOSModifier ()
		{
			var hotkey = keyboard.ParseName ("<Primary>+s");

			Assert.AreEqual ((int)Gdk.Key.s, hotkey.Key);
			if (VAS.Core.Common.Utils.OS == OperatingSystemID.OSX) {
				Assert.IsTrue (keyboard.ContainsModifier (hotkey, Gdk.ModifierType.MetaMask));
			} else {
				Assert.IsTrue (keyboard.ContainsModifier (hotkey, Gdk.ModifierType.ControlMask));
			}
		}

		[Test]
		public void ParseName_KeyAndShift_ShiftModifier ()
		{
			foreach (var modifier in new string [] { "Shift_L", "Shift_R", "Shift" }) {
				var hotkey = keyboard.ParseName ($"<{modifier}>+s");
				Assert.IsTrue (keyboard.ContainsModifier (hotkey, Gdk.ModifierType.ShiftMask));
			}
		}

		[Test]
		public void ParseName_KeyAndControl_ControlModifier ()
		{
			foreach (var modifier in new string [] { "Control_L", "Control_R", "Control" }) {
				var hotkey = keyboard.ParseName ($"<{modifier}>+s");
				Assert.IsTrue (keyboard.ContainsModifier (hotkey, Gdk.ModifierType.ControlMask));
			}
		}

		[Test]
		public void ParseName_KeyAndMeta_MetaModifier ()
		{
			foreach (var modifier in new string [] { "Meta_L", "Meta_R", "Meta" }) {
				var hotkey = keyboard.ParseName ($"<{modifier}>+s");
				Assert.IsTrue (keyboard.ContainsModifier (hotkey, Gdk.ModifierType.MetaMask));
			}
		}

		[Test]
		public void ParseName_KeyAndAlt_AltModifier ()
		{
			foreach (var modifier in new string [] { "Alt_L", "Alt_R", "Alt" }) {
				var hotkey = keyboard.ParseName ($"<{modifier}>+s");
				Assert.IsTrue (keyboard.ContainsModifier (hotkey, Gdk.ModifierType.Mod1Mask));
			}
		}
	}
}
