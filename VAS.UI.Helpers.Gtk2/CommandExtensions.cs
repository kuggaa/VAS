//
//  Copyright (C) 2017 FLUENDO S.A.
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
using Gtk;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.UI.Helpers
{
	public static class CommandExtensions
	{
		public static MenuItem CreateMenuItem (this Command command, string menuLabel, AccelGroup group, string key)
		{
			MenuItem item = new MenuItem(menuLabel);

			if (!string.IsNullOrEmpty (key)) {
				HotKey keyconfig = App.Current.HotkeysService.GetByName (key).Key;
				AccelKey accelkey = new AccelKey ((Gdk.Key)keyconfig.Key, (Gdk.ModifierType)keyconfig.Modifier, AccelFlags.Visible);
				item.AddAccelerator ("activate", group, accelkey);
			}

			item.Activated += (sender, e) => command.Execute ();
			item.Sensitive = command.CanExecute ();
			command.CanExecuteChanged += (sender, e) => item.Sensitive = command.CanExecute ();
			item.Show ();
			return item;
		}
	}
}
