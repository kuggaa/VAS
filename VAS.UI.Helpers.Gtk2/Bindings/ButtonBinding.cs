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
	/// <summary>
	/// Command binding for buttons.
	/// </summary>
	public class ButtonBinding : CommandBinding
	{
		protected Button button;
		Image image;
		string text;

		public ButtonBinding (Button button, Func<IViewModel, Command> commandFunc,
							  object param = null, Image image = null, string text = null) : base (commandFunc, param)
		{
			this.button = button;
			this.image = image;
			this.text = text;
		}

		protected override void BindView ()
		{
			button.Clicked += HandleClicked;
			UpdateButton ();
		}

		protected override void UnbindView ()
		{
			button.Clicked -= HandleClicked;
		}

		protected override void BindViewModel ()
		{
			if (Command != null) {
				Command.CanExecuteChanged -= HandleCanExecuteChanged;
			}
			base.BindViewModel ();
			if (Command != null) {
				UpdateButton ();
				Command.CanExecuteChanged += HandleCanExecuteChanged;
			}
		}

		void UpdateButton ()
		{
			button.Configure ((image ?? Command.Icon)?.Value,
							  text ?? Command.Text,
							  Command.ToolTipText, null);
			button.Sensitive = Command.CanExecute ();
		}

		void HandleCanExecuteChanged (object sender, EventArgs args)
		{
			button.Sensitive = Command.CanExecute ();
		}

		void HandleClicked (object sender, EventArgs args)
		{
			var radioButton = sender as RadioButton;
			if (radioButton != null && radioButton.Active == false) {
				return;
			}
			Command.Execute (Parameter);
		}
	}
}
