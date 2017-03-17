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
	public static class DrawingToolHotkeys
	{
		static List<KeyConfig> hotkeys;
		public const string CATEGORY = "Drawing tool";

		static DrawingToolHotkeys ()
		{
			hotkeys = new List<KeyConfig> {
				new KeyConfig {
					Name = "DRAWING_TOOL_SELECT",
					Key = App.Current.Keyboard.ParseName ("v"),
					Category = CATEGORY,
					Description = Catalog.GetString("Select")
				},
				new KeyConfig {
					Name = "DRAWING_TOOL_ERASER",
					Key = App.Current.Keyboard.ParseName ("e"),
					Category = CATEGORY,
					Description = Catalog.GetString("Eraser")
				},
				new KeyConfig {
					Name = "DRAWING_TOOL_PENCIL_TOOL",
					Key = App.Current.Keyboard.ParseName ("n"),
					Category = CATEGORY,
					Description = Catalog.GetString("Pencil tool")
				},
				new KeyConfig {
					Name = "DRAWING_TOOL_TEXT_TOOL",
					Key = App.Current.Keyboard.ParseName ("t"),
					Category = CATEGORY,
					Description = Catalog.GetString("Text tool")
				},
				new KeyConfig {
					Name = "DRAWING_TOOL_LINE_TOOL",
					Key = App.Current.Keyboard.ParseName ("a"),
					Category = CATEGORY,
					Description = Catalog.GetString("Line tool")
				},
				new KeyConfig {
					Name = "DRAWING_TOOL_X_TOOL",
					Key = App.Current.Keyboard.ParseName ("x"),
					Category = CATEGORY,
					Description = Catalog.GetString("X tool")
				},
				new KeyConfig {
					Name = "DRAWING_TOOL_SQUARE",
					Key = App.Current.Keyboard.ParseName ("s"),
					Category = CATEGORY,
					Description = Catalog.GetString("Square")
				},
				new KeyConfig {
					Name = "DRAWING_TOOL_FILLED_SQUARE",
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+s"),
					Category = CATEGORY,
					Description = Catalog.GetString("Filled square")
				},
				new KeyConfig {
					Name = "DRAWING_TOOL_CIRCLE",
					Key = App.Current.Keyboard.ParseName ("c"),
					Category = CATEGORY,
					Description = Catalog.GetString("Circle")
				},
				new KeyConfig {
					Name = "DRAWING_TOOL_FILLED_CIRCLE",
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+c"),
					Category = CATEGORY,
					Description = Catalog.GetString("Filled circle")
				},
				new KeyConfig {
					Name = "DRAWING_TOOL_COUNTER",
					Key = App.Current.Keyboard.ParseName ("1"),
					Category = CATEGORY,
					Description = Catalog.GetString("Counter")
				},
				//FIXME: this sould be added, now are not possible due to 
				//the actual functionality of the VAS DrawingTool
				/*new KeyConfig {
					Name = "DRAWING_TOOL_TAG_PLAYER",
					Key = App.Current.Keyboard.ParseName ("p"),
					Category = CATEGORY,
					Description = Catalog.GetString("Tag player")
				},
				new KeyConfig {
					Name = "DRAWING_TOOL_ZOOM_IN",
					Key = App.Current.Keyboard.ParseName ("<Control_L>+plus"),
					Category = CATEGORY,
					Description = Catalog.GetString("Zoom in")
				},
				new KeyConfig {
					Name = "DRAWING_TOOL_ZOOM_OUT",
					Key = App.Current.Keyboard.ParseName ("<Control_L>+minus"),
					Category = CATEGORY,
					Description = Catalog.GetString("Zoom out")
				},*/
				new KeyConfig {
					Name = "DRAWING_TOOL_CLEAR_ALL",
					Key = App.Current.Keyboard.ParseName ("<Control_L>+<Alt_L>+<Shift_L>+x"),
					Category = CATEGORY,
					Description = Catalog.GetString("Clear all")
				},
				new KeyConfig {
					Name = "DRAWING_TOOL_DELETE_SELECTION",
					Key = App.Current.Keyboard.ParseName ("Delete"),
					Category = CATEGORY,
					Description = Catalog.GetString("Delete selection")
				},
				new KeyConfig {
					Name = "DRAWING_TOOL_SAVE",
					Key = App.Current.Keyboard.ParseName ("<Control_L>+s"),
					Category = CATEGORY,
					Description = Catalog.GetString("Save")
				},
				new KeyConfig {
					Name = "DRAWING_TOOL_EXPORT_IMAGE",
					Key = App.Current.Keyboard.ParseName ("<Control_L>+<Shift_L>+s"),
					Category = CATEGORY,
					Description = Catalog.GetString("Export image")
				},
			};
		}

		/// <summary>
		/// Static Method to Registers the default Drawing Tool hotkeys.
		/// </summary>
		public static void RegisterDefaultHotkeys ()
		{
			App.Current.HotkeysService.Register (hotkeys);
		}
	}
}
