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
using System.Linq;
using VAS.Core.MVVMC;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// Menu node vm, can be configured as a MenuNode or as a SubMenu
	/// </summary>
	public class MenuNodeVM : ViewModelBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:VAS.Core.ViewModel.MenuNodeVM"/> as a MenuNode with a command.
		/// </summary>
		/// <param name="command">Command.</param>
		public MenuNodeVM (Command command, object commandParameter = null, string name = null)
		{
			Command = command;
			CommandParameter = commandParameter;
			Name = name;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:VAS.Core.ViewModel.MenuNodeVM"/> as a SubMenu
		/// </summary>
		/// <param name="menu">The Submenu.</param>
		public MenuNodeVM (MenuVM menu, string name)
		{
			Submenu = menu;
			Name = name;
		}

		public Command Command {
			get;
		}

		public object CommandParameter {
			get;
		}

		public MenuVM Submenu {
			get;
		}

		public string Name {
			get;
			set;
		}
	}
}
