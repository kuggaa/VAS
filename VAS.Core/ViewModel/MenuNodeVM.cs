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
using VAS.Core.Common;
using Newtonsoft.Json;
using PropertyChanged;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// Menu node vm, can be configured as a MenuNode or as a SubMenu
	/// </summary>
	public class MenuNodeVM : ViewModelBase
	{
		object commandParameter;

		private MenuNodeVM () 
		{
			ActiveColor = App.Current.Style.TextBase;
			DisableColor = App.Current.Style.TextBaseDisabled;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:VAS.Core.ViewModel.MenuNodeVM"/> as a MenuNode with a command.
		/// </summary>
		/// <param name="command">Command.</param>
		public MenuNodeVM (Command command, object commandParameter = null, string name = null) : this ()
		{
			Command = command;
			CommandParameter = commandParameter;
			if (name == null) {
				name = Command.Text;
			}
			Name = name;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:VAS.Core.ViewModel.MenuNodeVM"/> as a SubMenu
		/// </summary>
		/// <param name="menu">The Submenu.</param>
		public MenuNodeVM (MenuVM menu, string name) : this ()
		{
			Submenu = menu;
			Name = name;
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			Command = null;
			CommandParameter = null;
			Submenu?.Dispose ();
			Submenu = null;
		}

		/// <summary>
		/// Gets the command to execute
		/// </summary>
		/// <value>The command.</value>
		public Command Command {
			get;
			private set;
		}

		/// <summary>
		/// Gets the command parameter.
		/// </summary>
		/// <value>The command parameter.</value>
		public object CommandParameter {
			get => commandParameter;
			set {
				commandParameter = value;
				UpdateCanExecute ();
			}
		}

		/// <summary>
		/// Gets the color of the menu item.
		/// </summary>
		/// <value>The color of the menu item.</value>
		[DependsOn ("DisableColor", "ActiveColor", "Command", "CommandParameter")]
		public Color Color { get => Command?.CanExecute (CommandParameter) ?? true ? ActiveColor : DisableColor; }

		/// <summary>
		/// Gets the color used when the item is disabled
		/// </summary>
		/// <value>The color of the menu item.</value>
		public Color DisableColor { get; set; }

		/// <summary>
		/// Gets the color used when the item is active
		/// </summary>
		/// <value>The color of the menu item.</value>
		public Color ActiveColor { get; set; }

		/// <summary>
		/// Gets the submenu.
		/// </summary>
		/// <value>The submenu.</value>
		[JsonIgnore]
		public MenuVM Submenu {
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public string IconName {
			get {
				return Command?.IconName;
			}
		}

		/// <summary>
		/// Updates the Command can execute.
		/// </summary>
		public void UpdateCanExecute ()
		{
			if (Submenu != null) {
				Submenu.UpdateCanExecute ();
			} else {
				Command?.EmitCanExecuteChanged ();
			}
		}
	}
}
