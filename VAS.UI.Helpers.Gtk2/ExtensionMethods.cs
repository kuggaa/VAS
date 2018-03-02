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
using Gdk;
using Gtk;
using VAS.Core.Common;

namespace VAS.UI.Helpers
{
	/// <summary>
	/// Provides extension methods for Gtk+
	/// </summary>
	public static class ExtensionMethods
	{
		/// <summary>
		/// Autocomplete the specified entry and list.
		/// </summary>
		/// <returns>The autocomplete.</returns>
		/// <param name="entry">Entry.</param>
		/// <param name="list">List.</param>
		public static void Autocomplete (this Entry entry, List<string> list)
		{
			EntryCompletion completionSeasons = new EntryCompletion ();
			ListStore storeSeasons = new ListStore (typeof (string));

			foreach (string item in list) {
				storeSeasons.AppendValues (item);
			}

			entry.Completion = new EntryCompletion {
				Model = storeSeasons,
				TextColumn = 0
			};
		}

		/// <summary>
		/// Centers a dialog on its parent. Dialogs are centered in Show () only if TransientFor is set,
		/// which has to be done in the constructor with Stetic. When we can't this function can be used instead.
		/// This implementation is a C# port of the C implementation used in from gtk/gtkwindow.c
		/// </summary>
		/// <param name="dialog">Widget to center.</param>
		public static void Center (this Gtk.Dialog dialog)
		{
			int monitorNum;
			int ox, oy, x, y, w, h;
			Widget parent;

			//We need to make sure that there aren't any size allocations pending to the dialog, before trying to center it
			while (Application.EventsPending ()) {
				Application.RunIteration ();
			}
			parent = dialog.TransientFor;

			if (parent.GdkWindow != null) {
				monitorNum = Screen.Default.GetMonitorAtWindow (parent.GdkWindow);
			} else {
				monitorNum = -1;
			}

			w = dialog.Allocation.Width;
			h = dialog.Allocation.Height;

			parent.GdkWindow.GetOrigin (out ox, out oy);
			x = ox + (parent.Allocation.Width - dialog.Allocation.Width) / 2;
			y = oy + (parent.Allocation.Height - dialog.Allocation.Height) / 2;

			if (monitorNum >= 0) {
				var monitor = Screen.Default.GetMonitorGeometry (monitorNum);
				ClamWindowToRectangle (ref x, ref y, w, h, monitor);
			}

			dialog.Move (x, y);
		}

		/// <summary>
		/// Resizes the window, by specifiyng the total percentage of the current active screen to be used by this window
		/// </summary>
		/// <param name="window">Window.</param>
		/// <param name="screenPercentage">Screen percentage.</param>
		public static void ResizeWindow (this Gtk.Window window, double screenPercentage, Gtk.Window parentWindow = null)
		{
			Gdk.Rectangle monitor_geometry = new Rectangle ();
			if (parentWindow != null) {
				//Get Rectangle Area for parent window
				monitor_geometry = parentWindow.GdkWindow.FrameExtents;
			} else {
				// Default screen
				Gdk.Screen screen = Gdk.Display.Default.DefaultScreen;
				// Which monitor is our window on
				int monitor = screen.GetMonitorAtWindow (window.GdkWindow);
				// Monitor size
				monitor_geometry = screen.GetMonitorGeometry (monitor);
			}
			// Resize to a convenient size
			var width = (int)(monitor_geometry.Width * screenPercentage);
			var height = (int)(monitor_geometry.Height * screenPercentage);
			window.Resize (width, height);

			if (Utils.OS == OperatingSystemID.OSX) {
				var position = (1 - screenPercentage) / 2;
				window.Move ((int)(monitor_geometry.X + monitor_geometry.Width * position),
							 (int)(monitor_geometry.Y + monitor_geometry.Height * position));
			}
		}

		static void Clamp (ref int @base, int extent, int clampBase, int clampExtent)
		{
			if (extent > clampExtent) {
				/* Center */
				@base = clampBase + clampExtent / 2 - extent / 2;
			} else if (@base < clampBase) {
				@base = clampBase;
			} else if (@base + extent > clampBase + clampExtent) {
				@base = clampBase + clampExtent - extent;
			}
		}

		static void ClamWindowToRectangle (ref int x, ref int y, int width, int height, Rectangle rect)
		{
			Clamp (ref x, width, rect.X, rect.Width);
			Clamp (ref y, height, rect.Y, rect.Height);
		}
	}
}
