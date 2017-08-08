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
using Gtk;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using Image = VAS.Core.Common.Image;

namespace VAS.UI.Helpers.Bindings
{
	public class ToggleButtonBinding : CommandBinding
	{
		ToggleButton button;
		object parameterInactive;

		public ToggleButtonBinding (ToggleButton button, Func<IViewModel, Command> commandFunc, object paramActive, object parameterInactive) : base (commandFunc, paramActive)
		{
			this.button = button;
			this.parameterInactive = parameterInactive;
		}

		protected override void BindView ()
		{
			button.Toggled += HandleToggled;
			UpdateButton ();
		}

		protected override void UnbindView ()
		{
			button.Toggled -= HandleToggled;
		}

		protected override void BindViewModel ()
		{
			UnbindViewModel ();
			base.BindViewModel ();
			if (Command != null) {
				UpdateButton ();
				Command.CanExecuteChanged += HandleCanExecuteChanged;
			}
		}

		protected override void UnbindViewModel ()
		{
			if (Command != null) {
				Command.CanExecuteChanged -= HandleCanExecuteChanged;
			}
			base.UnbindViewModel ();
		}

		void UpdateButton ()
		{
			Image icon;

			if (button.Active) {
				icon = Command.Icon;
			} else {
				icon = Command.IconInactive ?? Command.Icon;
			}

			button.Configure (icon, Command.Text, Command.ToolTipText, null);
			button.Sensitive = Command.CanExecute ();
		}

		void HandleCanExecuteChanged (object sender, EventArgs args)
		{
			button.Sensitive = Command.CanExecute ();
		}

		void HandleToggled (object sender, EventArgs args)
		{
			Command.Execute (button.Active ? Parameter : parameterInactive);
			UpdateButton ();
		}
	}
}
