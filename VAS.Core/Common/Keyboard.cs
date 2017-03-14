//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using Gdk;
using VAS.Core.Store;
using VAS.Core.Interfaces;

namespace VAS.Core.Common
{
	public class Keyboard : IKeyboard
	{
		public uint KeyvalFromName (string name)
		{
			return Keyval.FromName (name);
		}

		public string NameFromKeyval (uint keyval)
		{
			return Keyval.Name (keyval);
		}

		public HotKey ParseEvent (object obj)
		{
			int modifier = 0;
			EventKey evt = obj as EventKey;

			if (evt.State.HasFlag (Gdk.ModifierType.ShiftMask)) {
				modifier += (int)ModifierType.ShiftMask;
			}
			if (evt.State.HasFlag (Gdk.ModifierType.Mod1Mask) || evt.State.HasFlag (Gdk.ModifierType.Mod5Mask)) {
				modifier += (int)ModifierType.Mod1Mask;
			}
			// Use comand or control if we are in OSX
			// FIXME: we need to actually define this better. We want users to configure hotkeys with command? if so 
			// let that be command.
			if (Utils.OS == OperatingSystemID.OSX) {
				if (evt.State.HasFlag (Gdk.ModifierType.Mod2Mask | Gdk.ModifierType.MetaMask) || evt.State.HasFlag (Gdk.ModifierType.ControlMask)) {
					modifier += (int)ModifierType.ControlMask;
				}
			} else {
				if (evt.State.HasFlag (Gdk.ModifierType.ControlMask)) {
					modifier += (int)ModifierType.ControlMask;
				}
			}

			int keyval = (int)Gdk.Keyval.ToLower ((uint)evt.KeyValue);
			if (modifier != 0) {
				keyval = (int)NormalizeKeyVal (evt.HardwareKeycode);
			}

			return new HotKey {
				Key = keyval,
				Modifier = modifier
			};
		}

		public HotKey ParseName (string name)
		{
			int key = 0, modifier = 0;

			string [] keyNames = name.Split ('+');
			foreach (string keyName in keyNames) {
				string kName = keyName.Trim ();
				if (kName.StartsWith ("<") && kName.EndsWith (">")) {
					switch (kName.Substring (1, kName.Length - 2)) {
					case "Shift_L":
						modifier += (int)ModifierType.ShiftMask;
						break;
					case "Alt_L":
						modifier += (int)ModifierType.Mod1Mask;
						break;
					case "Control_L":
						modifier += (int)ModifierType.ControlMask;
						break;
					}
				} else {
					key = (int)KeyvalFromName (kName);
				}
			}
			return new HotKey { Key = key, Modifier = modifier };
		}

		public string HotKeyName (HotKey hotkey)
		{
			string name = "";
			if ((hotkey.Modifier & (int)ModifierType.ShiftMask) == (int)ModifierType.ShiftMask) {
				name += "<Shift_L>";
			}
			if ((hotkey.Modifier & (int)ModifierType.Mod1Mask) == (int)ModifierType.Mod1Mask) {
				if (name != "") {
					name += "+";
				}
				name += "<Alt_L>";
			}
			if ((hotkey.Modifier & (int)ModifierType.ControlMask) == (int)ModifierType.ControlMask) {
				if (name != "") {
					name += "+";
				}
				name += "<Control_L>";
			}
			if (name != "") {
				name += "+";
			}
			return name + NameFromKeyval ((uint)hotkey.Key);
		}

		/// <summary>
		/// Normalizes the key value. Only useful when having a modifier enabled with the key
		/// </summary>
		/// <returns>The key value.</returns>
		/// <param name="hardwareKey">Hardware key.</param>
		uint NormalizeKeyVal (uint hardwareKey)
		{
			KeymapKey [] keymapkey;
			uint [] keyvals;
			Gdk.Keymap.Default.GetEntriesForKeycode (hardwareKey, out keymapkey, out keyvals);
			return keyvals [0];
		}
	}
}
