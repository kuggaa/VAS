//
//  Copyright (C) 2016 Fluendo S.A.
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
using System.Diagnostics.Contracts;
using System.Windows.Input;
using Gtk;

namespace VAS.UI.Helpers
{
	public static class Bindings
	{
		static public void Bind (this Button button, ICommand command, object parameter = null)
		{
			Contract.Requires (button != null);
			Contract.Requires (command != null);

			button.Sensitive = command.CanExecute (parameter);
			EventHandler handler = (sender, e) => {
				button.Sensitive = command.CanExecute (parameter);
			};
			command.CanExecuteChanged += handler;
			button.Destroyed += (sender, e) => {
				command.CanExecuteChanged -= handler;
			};
			button.Clicked += (sender, e) => {
				command.Execute (parameter);
			};
		}
	}
}
