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

		// Keep this sorted alphabetically
		public const string CANCEL = "CANCEL";
		public const string CLOSE = "CLOSE";
		public const string CLOSE_APP = "CLOSE_APP";
		public const string COPY = "COPY";
		public const string DELETE = "DELETE";
		public const string FIT_TIMELINE = "FIT_TIMELINE";
		public const string OK = "OK";
		public const string OPEN_PREFERENCES = "OPEN_PREFERENCES";
		public const string PASTE = "PASTE";
		public const string SAVE = "SAVE";
		public const string ZOOM_IN = "ZOOM_IN";
		public const string ZOOM_OUT = "ZOOM_OUT";

		static GeneralUIHotkeys ()
		{
			hotkeys = new List<KeyConfig> {
				new KeyConfig {
					Name = SAVE,
					Key = App.Current.Keyboard.ParseName ("<Primary>+s"),
					Category = CATEGORY,
					Description = Catalog.GetString("Save")
				},
				new KeyConfig {
					Name = CLOSE,
					Key = App.Current.Keyboard.ParseName ("<Primary>+w"),
					Category = CATEGORY,
					Description = Catalog.GetString("Close")
				},
				new KeyConfig {
					Name = DELETE,
					Key = App.Current.Keyboard.ParseName ("Delete"),
					Category = CATEGORY,
					Description = Catalog.GetString("Delete selected element")
				},
				new KeyConfig {
					Name = OK,
					Key = App.Current.Keyboard.ParseName ("Return"),
					Category = CATEGORY,
					Description = Catalog.GetString("OK")
				},
				new KeyConfig {
					Name = CANCEL,
					Key = App.Current.Keyboard.ParseName ("Escape"),
					Category = CATEGORY,
					Description = Catalog.GetString("Cancel")
				},
				new KeyConfig {
					Name = CLOSE_APP,
					Key = App.Current.Keyboard.ParseName ("<Primary>+q"),
					Category = CATEGORY,
					Description = Catalog.GetString("Close application")
				},
				new KeyConfig {
					Name = OPEN_PREFERENCES,
					Key = App.Current.Keyboard.ParseName ("<Primary>+p"),
					Category = CATEGORY,
					Description = Catalog.GetString("Open preferences")
				},
				new KeyConfig {
					Name = ZOOM_IN,
					Key = App.Current.Keyboard.ParseName ("plus"),
					Category = CATEGORY,
					Description = Catalog.GetString("Zoom in")
				},
				new KeyConfig {
					Name = ZOOM_OUT,
					Key = App.Current.Keyboard.ParseName ("minus"),
					Category = CATEGORY,
					Description = Catalog.GetString("Zoom out")
				},
				new KeyConfig {
					Name = FIT_TIMELINE,
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+t"),
					Category = CATEGORY,
					Description = Catalog.GetString("Adjust timeline to current position")
				},
				new KeyConfig {
					Name = COPY,
					Key = App.Current.Keyboard.ParseName ("<Primary>+c"),
					Category = CATEGORY,
					Description = Catalog.GetString("Copy")
				},
				new KeyConfig {
					Name = PASTE,
					Key = App.Current.Keyboard.ParseName ("<Primary>+v"),
					Category = CATEGORY,
					Description = Catalog.GetString("Paste")
				},
				//FIXME: Not entering on first release, but should be
				//done in a future
				/*
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
