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
	public static class GeneralUIHotkeys
	{
		static List<KeyConfig> hotkeys;
		public const string CATEGORY = "General Interface";

		static GeneralUIHotkeys ()
		{
			hotkeys = new List<KeyConfig> {
				new KeyConfig {
					Name = "TOGGLE_MAP_VISIBILITY",
					Key = App.Current.Keyboard.ParseName ("F9"),
					Category = CATEGORY,
					Description = Catalog.GetString("Hide map")
				},
				new KeyConfig {
					Name = "SAVE_PROJECT",
					Key = App.Current.Keyboard.ParseName ("<Control_L>+s"),
					Category = CATEGORY,
					Description = Catalog.GetString("Save project")
				},
				new KeyConfig {
					Name = "CLOSE_APP",
					Key = App.Current.Keyboard.ParseName ("<Control_L>+q"),
					Category = CATEGORY,
					Description = Catalog.GetString("Close application")
				},
				new KeyConfig {
					Name = "OPEN_PREFERENCES",
					Key = App.Current.Keyboard.ParseName ("<Control_L>+p"),
					Category = CATEGORY,
					Description = Catalog.GetString("Open preferences")
				},
				new KeyConfig {
					Name = "FIND_PLAYHEAD",
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+p"),
					Category = CATEGORY,
					Description = Catalog.GetString("Find playhead")
				},
				new KeyConfig {
					Name = "ZOOM_IN",
					Key = App.Current.Keyboard.ParseName ("plus"),
					Category = CATEGORY,
					Description = Catalog.GetString("Zoom in")
				},
				new KeyConfig {
					Name = "ZOOM_OUT",
					Key = App.Current.Keyboard.ParseName ("minus"),
					Category = CATEGORY,
					Description = Catalog.GetString("Zoom out")
				},
				//FIXME: Not entering on first release, but should be
				//done in a future
				/*new KeyConfig {
					Name = "DELETE_SELECTED_EVENT",
					Key = App.Current.Keyboard.ParseName ("Delete"),
					Category = CATEGORY,
					Description = Catalog.GetString("Delete selected event")
				},
				new KeyConfig {
					Name = "LOCK_SELECTION_MODIFIER",
					Key = App.Current.Keyboard.ParseName ("<Control_L>"),
					Category = CATEGORY,
					Description = Catalog.GetString("Lock selection modifier")
				}*/
			};
		}

		/// <summary>
		/// Registers the default UI hotkeys
		/// </summary>
		public static void RegisterDefaultHotkeys ()
		{
			App.Current.HotkeysService.Register (hotkeys);
		}
	}
}
