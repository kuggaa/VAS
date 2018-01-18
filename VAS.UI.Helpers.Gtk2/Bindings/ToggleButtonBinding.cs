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
		bool silenceToggledEvent;
		string text;

		public ToggleButtonBinding (ToggleButton button, Func<IViewModel, Command> commandFunc, object paramActive,
									object parameterInactive, string text = null) : base (commandFunc, paramActive)
		{
			this.button = button;
			this.parameterInactive = parameterInactive;
			this.text = text;
		}

		protected override void BindView ()
		{
			button.Toggled += HandleToggled;
			UpdateView ();
		}

		protected override void UnbindView ()
		{
			button.Toggled -= HandleToggled;
		}

		protected override void UpdateView ()
		{
			Image icon;

			if (button.Active) {
				icon = Command.Icon;
			} else {
				icon = Command.IconInactive ?? Command.Icon;
			}

			button.Configure (icon, text ?? Command.Text, Command.ToolTipText, null);
			button.Sensitive = Command.CanExecute ();
		}

		protected override void HandleCanExecuteChanged (object sender, EventArgs args)
		{
			button.Sensitive = Command.CanExecute ();
		}

		void HandleToggled (object sender, EventArgs args)
		{
			if (!silenceToggledEvent) {
				Command.Execute (button.Active ? Parameter : parameterInactive);

				LimitationCommand limitedCommand = Command as LimitationCommand;
				if (limitedCommand != null && !limitedCommand.Executed) {
					silenceToggledEvent = true;
					button.Toggle ();
					button.Active = false;
					silenceToggledEvent = false;
				}

				UpdateView ();
			}
		}
	}
}
