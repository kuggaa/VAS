//
//  Copyright (C) 2017 ${CopyrightHolder}
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

namespace VAS.UI.Helpers
{
	public static class CommandExtensions
	{
		public static MenuItem CreateMenuItem (this Command command, string menuLabel, string key)
		{
			MenuItem item = new MenuItem(menuLabel);
			uint parsedKey = App.Current.Keyboard.KeyvalFromName (key);
			// item.AddAccelerator pending to use HotKeys
			item.Activated += (sender, e) => command.Execute ();
			item.Sensitive = command.CanExecute ();
			command.CanExecuteChanged += (sender, e) => item.Sensitive = command.CanExecute ();
			item.Show ();
			return item;
		}
	}
}
