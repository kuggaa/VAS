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

namespace VAS.Core.Hotkeys
{
	/// <summary>
	/// Playback default hotkeys
	/// </summary>
	public static class PlaybackHotkeys
	{
		static List<KeyConfig> playbackHotkeys;
		public const string CATEGORY = "Playback";

		static PlaybackHotkeys ()
		{
			playbackHotkeys = new List<KeyConfig> {
				new KeyConfig {
					Name = "PLAYER_TOGGLE_PLAY",
					Key = App.Current.Keyboard.ParseName ("space"),
					Category = CATEGORY,
					Description = Catalog.GetString("Play/Pause")
				},
				new KeyConfig {
					Name = "PLAYER_SEEK_RIGHT_LONG",
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+Right"),
					Category = CATEGORY,
					Description = Catalog.GetString("Jump right")
				},
				new KeyConfig {
					Name = "PLAYER_SEEK_LEFT_LONG",
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+Left"),
					Category = CATEGORY,
					Description = Catalog.GetString("Jump left")
				},
				new KeyConfig {
					Name = "PLAYER_SEEK_LEFT_SHORT",
					Key = App.Current.Keyboard.ParseName ("Left"),
					Category = CATEGORY,
					Description = Catalog.GetString("Frame step left")
				},
				new KeyConfig {
					Name = "PLAYER_SEEK_RIGHT_SHORT",
					Key = App.Current.Keyboard.ParseName ("Right"),
					Category = CATEGORY,
					Description = Catalog.GetString("Frame step right")
				},
				new KeyConfig {
					Name = "PLAYER_RATE_INCREMENT",
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+<Alt_L>+Right"),
					Category = CATEGORY,
					Description = Catalog.GetString("Speed up")
				},
				new KeyConfig {
					Name = "PLAYER_RATE_DECREMENT",
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+<Alt_L>+Left"),
					Category = CATEGORY,
					Description = Catalog.GetString("Speed down")
				},
				new KeyConfig {
					Name = "PLAYER_RATE_MAX",
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+<Alt_L>+Up"),
					Category = CATEGORY,
					Description = Catalog.GetString("Max speed")
				},
				new KeyConfig {
					Name = "PLAYER_RATE_DEFAULT",
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+<Alt_L>+Down"),
					Category = CATEGORY,
					Description = Catalog.GetString("Speed 1x")
				},
				new KeyConfig {
					Name = "PLAYER_NEXT_ELEMENT",
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+x"),
					Category = CATEGORY,
					Description = Catalog.GetString("Next event")
				},
				new KeyConfig {
					Name = "PLAYER_PREVIOUS_ELEMENT",
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+z"),
					Category = CATEGORY,
					Description = Catalog.GetString("Previous event")
				},
				new KeyConfig {
					Name = "OPEN_DRAWING_TOOL",
					Key = App.Current.Keyboard.ParseName (""),
					Category = CATEGORY,
					Description = Catalog.GetString("Open drawing tool")
				}
			};
		}

		public static void RegisterDefaultHotkeys ()
		{
			App.Current.HotkeysService.Register (playbackHotkeys);
		}
	}
}
