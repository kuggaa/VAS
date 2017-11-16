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

		// Keep this sorted alphabetically
		public const string CLOSE_LOADED_EVENT = "CLOSE_LOADED_EVENT";
		public const string EDIT_EVENT_DURATION = "EDIT_EVENT_DURATION";
		public const string OPEN_DRAWING_TOOL = "OPEN_DRAWING_TOOL";
		public const string PLAYER_NEXT_ELEMENT = "PLAYER_NEXT_ELEMENT";
		public const string PLAYER_PREVIOUS_ELEMENT = "PLAYER_PREVIOUS_ELEMENT";
		public const string PLAYER_RATE_DECREMENT = "PLAYER_RATE_DECREMENT";
		public const string PLAYER_RATE_DEFAULT = "PLAYER_RATE_DEFAULT";
		public const string PLAYER_RATE_INCREMENT = "PLAYER_RATE_INCREMENT";
		public const string PLAYER_RATE_MAX = "PLAYER_RATE_MAX";
		public const string PLAYER_SEEK_LEFT_LONG = "PLAYER_SEEK_LEFT_LONG";
		public const string PLAYER_SEEK_LEFT_SHORT = "PLAYER_SEEK_LEFT_SHORT";
		public const string PLAYER_SEEK_RIGHT_LONG = "PLAYER_SEEK_RIGHT_LONG";
		public const string PLAYER_SEEK_RIGHT_SHORT = "PLAYER_SEEK_RIGHT_SHORT";
		public const string PLAYER_TOGGLE_PLAY = "PLAYER_TOGGLE_PLAY";

		public const string ZOOM_RESTORE = "ZOOM_RESTORE";
		public const string ZOOM_DECREASE = "ZOOM_DECREASE";
		public const string ZOOM_INCREASE = "ZOOM_INCREASE";

		static PlaybackHotkeys ()
		{
			playbackHotkeys = new List<KeyConfig> {
				new KeyConfig {
					Name = PLAYER_TOGGLE_PLAY,
					Key = App.Current.Keyboard.ParseName ("space"),
					Category = CATEGORY,
					Description = Catalog.GetString("Play/Pause")
				},
				new KeyConfig {
					Name = PLAYER_SEEK_RIGHT_LONG,
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+Right"),
					Category = CATEGORY,
					Description = Catalog.GetString("Jump right")
				},
				new KeyConfig {
					Name = PLAYER_SEEK_LEFT_LONG,
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+Left"),
					Category = CATEGORY,
					Description = Catalog.GetString("Jump left")
				},
				new KeyConfig {
					Name = PLAYER_SEEK_LEFT_SHORT,
					Key = App.Current.Keyboard.ParseName ("Left"),
					Category = CATEGORY,
					Description = Catalog.GetString("Frame step left")
				},
				new KeyConfig {
					Name = PLAYER_SEEK_RIGHT_SHORT,
					Key = App.Current.Keyboard.ParseName ("Right"),
					Category = CATEGORY,
					Description = Catalog.GetString("Frame step right")
				},
				new KeyConfig {
					Name = PLAYER_RATE_INCREMENT,
					Key = App.Current.Keyboard.ParseName ("Up"),
					Category = CATEGORY,
					Description = Catalog.GetString("Speed up")
				},
				new KeyConfig {
					Name = PLAYER_RATE_DECREMENT,
					Key = App.Current.Keyboard.ParseName ("Down"),
					Category = CATEGORY,
					Description = Catalog.GetString("Speed down")
				},
				new KeyConfig {
					Name = PLAYER_RATE_MAX,
					Key = App.Current.Keyboard.ParseName ("<Alt_L>+<Shift_L>+Up"),
					Category = CATEGORY,
					Description = Catalog.GetString("Max speed")
				},
				new KeyConfig {
					Name = PLAYER_RATE_DEFAULT,
					Key = App.Current.Keyboard.ParseName ("<Alt_L>+<Shift_L>+Down"),
					Category = CATEGORY,
					Description = Catalog.GetString("Speed 1x")
				},
				new KeyConfig {
					Name = PLAYER_NEXT_ELEMENT,
					Key = App.Current.Keyboard.ParseName ("<Control_L>+x"),
					Category = CATEGORY,
					Description = Catalog.GetString("Next event")
				},
				new KeyConfig {
					Name = PLAYER_PREVIOUS_ELEMENT,
					Key = App.Current.Keyboard.ParseName ("<Control_L>+z"),
					Category = CATEGORY,
					Description = Catalog.GetString("Previous event")
				},
				new KeyConfig {
					Name = OPEN_DRAWING_TOOL,
					Key = App.Current.Keyboard.ParseName ("<Control_L>+d"),
					Category = CATEGORY,
					Description = Catalog.GetString("Open drawing tool")
				},
				new KeyConfig {
					Name = ZOOM_RESTORE,
					Key = App.Current.Keyboard.ParseName ("<Primary>+0"),
					Category = CATEGORY,
					Description = Catalog.GetString("Restore zoom")
				},
				new KeyConfig {
					Name = ZOOM_DECREASE,
					Key = App.Current.Keyboard.ParseName ("<Primary>+minus"),
					Category = CATEGORY,
					Description = Catalog.GetString("Decrease zoom")
				},
				new KeyConfig {
					Name = ZOOM_INCREASE,
					Key = App.Current.Keyboard.ParseName ("<Primary>+plus"),
					Category = CATEGORY,
					Description = Catalog.GetString("Increase zoom")
				},
				new KeyConfig {
					Name = EDIT_EVENT_DURATION,
					Key = App.Current.Keyboard.ParseName ("<Primary>+j"),
					Category = CATEGORY,
					Description = Catalog.GetString("Edit event duration")
				},
				new KeyConfig {
					Name = CLOSE_LOADED_EVENT,
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+a"),
					Category = CATEGORY,
					Description = Catalog.GetString("Close loaded event")
				}
			};
		}

		/// <summary>
		/// Registers the default playback hotkeys.
		/// </summary>
		public static void RegisterDefaultHotkeys ()
		{
			App.Current.HotkeysService.Register (playbackHotkeys);
		}
	}
}
