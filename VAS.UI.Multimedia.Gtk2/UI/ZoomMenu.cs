//
//  Copyright (C) 2017  Fluendo S.A.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
using System;
using Gtk;
using VAS.Core;

namespace VAS.UI
{
	public enum ZoomLevel
	{
		Original = 0,
		Level1 = 1,
		Level2 = 2,
		Level3 = 3,
		Level4 = 4,
		Level5 = 5
	}

	public class ZoomMenu : Gtk.Menu
	{
		public delegate void ZoomChangedHandler (ZoomLevel level);

		public event ZoomChangedHandler ZoomChanged;

		public ZoomMenu ()
		{
			foreach (ZoomLevel level in Enum.GetValues (typeof (ZoomLevel))) {
				MenuItem item;

				if (level != ZoomLevel.Original) {
					item = new MenuItem (String.Format ("{0} {1}", Catalog.GetString ("Level"), (int)level));
				} else {
					item = new MenuItem (Catalog.GetString ("Original"));
				}

				item.Activated += (object sender, EventArgs e) => {
					if (ZoomChanged != null) {
						ZoomChanged (level);
					}
				};

				Append (item);
			}

			ShowAll ();
		}
	}
}

