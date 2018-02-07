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
using System;
using Gdk;
using VAS.Core.Interfaces;
using VAS.Core.Store;

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
			EventKey eventKey = obj as EventKey;

			if (IsSupportedModifier (eventKey.Key)) {
				return null;
			}
			int keyval = (int)Gdk.Keyval.ToLower ((uint)eventKey.KeyValue);

			int modifier = ParseModifier (eventKey);
			if (Utils.OS == OperatingSystemID.OSX) {
				// Map Command + BackSpace to Delete
				if (eventKey.State.HasFlag (Gdk.ModifierType.MetaMask) && eventKey.Key == Key.BackSpace) {
					modifier = 0;
					keyval = (int)Gdk.Key.Delete;
				}
			}

			if (modifier != 0) {
				keyval = (int)NormalizeKeyVal (eventKey.HardwareKeycode);
			}

			return new HotKey {
				Key = keyval,
				Modifier = modifier
			};
		}

		public int ParseModifier (object originalEvent)
		{
			EventWrapper eventWrapper = new EventWrapper (originalEvent);

			int modifier = 0;
			if (eventWrapper.State.HasFlag (Gdk.ModifierType.ShiftMask)) {
				modifier += (int)ModifierType.ShiftMask;
			}
			if (eventWrapper.State.HasFlag (Gdk.ModifierType.Mod1Mask) || eventWrapper.State.HasFlag (Gdk.ModifierType.Mod5Mask)) {
				modifier += (int)ModifierType.Mod1Mask;
			}
			if (eventWrapper.State.HasFlag (Gdk.ModifierType.ControlMask)) {
				modifier += (int)ModifierType.ControlMask;
			}
			if (eventWrapper.State.HasFlag (Gdk.ModifierType.MetaMask)) {
				modifier += (int)ModifierType.MetaMask;
			}

			return modifier;
		}

		public HotKey ParseName (string name)
		{
			int key = 0, modifier = 0;

			string [] keyNames = name.Split ('+');
			foreach (string keyName in keyNames) {
				string kName = keyName.Trim ();
				if (kName.StartsWith ("<") && kName.EndsWith (">")) {
					switch (kName.Substring (1, kName.Length - 2)) {
					case "Primary": {
							if (Utils.OS == OperatingSystemID.OSX) {
								modifier += (int)ModifierType.MetaMask;
							} else {
								modifier += (int)ModifierType.ControlMask;
							}
							break;
						}
					case "Shift_L":
					case "Shift_R":
					case "Shift":
						modifier += (int)ModifierType.ShiftMask;
						break;
					case "Alt_L":
					case "Alt_R":
					case "Alt":
						modifier += (int)ModifierType.Mod1Mask;
						break;
					case "Control_L":
					case "Control_R":
					case "Control":
						modifier += (int)ModifierType.ControlMask;
						break;
					case "Meta_L":
					case "Meta_R":
					case "Meta":
						modifier += (int)ModifierType.MetaMask;
						break;
					}
				} else {
					key = (int)KeyvalFromName (kName);
				}
			}
			if (key == 0) {
				return null;
			}
			return new HotKey { Key = key, Modifier = modifier };
		}

		public string HotKeyName (HotKey hotkey)
		{
			string name = "";
			if (ContainsModifier (hotkey, ModifierType.ShiftMask)) {
				name += "⇧";
			}
			if (ContainsModifier (hotkey, ModifierType.Mod1Mask)) {
				if (name != "") {
					name += "+";
				}
				if (Utils.OS == OperatingSystemID.OSX) {
					name += "⌥";
				} else {
					name += "alt";
				}
			}
			if (ContainsModifier (hotkey, ModifierType.ControlMask)) {
				if (name != "") {
					name += "+";
				}
				name += "ctrl";
			}
			if (ContainsModifier (hotkey, ModifierType.MetaMask)) {
				if (name != "") {
					name += "+";
				}
				name += "⌘";
			}
			if (name != "") {
				name += "+";
			}
			return name + NameFromKeyval ((uint)hotkey.Key);
		}

		internal bool ContainsModifier (HotKey hotkey, ModifierType modifier)
		{
			return (hotkey.Modifier & (int)modifier) == (int)modifier;
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

		bool IsSupportedModifier (Key key)
		{
			return key == Key.Shift_L ||
							 key == Key.Shift_R ||
							 key == Key.Alt_L ||
							 key == Key.Alt_R ||
							 key == Key.Control_L ||
							 key == Key.Control_R ||
							 key == Key.Meta_L ||
							 key == Key.Meta_R ||
							 key == (Key)ModifierType.None;
		}
	}

	/// <summary>
	/// This class wraps over both EventKey and EventButton and exposes their State property.
	/// </summary>
	class EventWrapper
	{
		EventKey eventKey;
		Gdk.EventButton eventButton;

		public EventWrapper (object evnt)
		{
			eventKey = evnt as EventKey;
			eventButton = evnt as Gdk.EventButton;
			if (eventKey == null && eventButton == null) {
				throw new ArgumentException ("argument must be of a valid type", nameof (evnt));
			}
		}

		public ModifierType State {
			get {
				return eventKey?.State ?? eventButton.State;
			}
		}
	}
}
