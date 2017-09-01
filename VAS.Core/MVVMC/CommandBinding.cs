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
using VAS.Core.Interfaces.MVVMC;

namespace VAS.Core.MVVMC
{

	/// <summary>
	/// Base class for Commands UI bindings.
	/// </summary>
	public abstract class CommandBinding : Binding
	{
		readonly Func<IViewModel, Command> commandFunc;

		public CommandBinding (Func<IViewModel, Command> commandFunc, object parameter)
		{
			this.commandFunc = commandFunc;
			Parameter = parameter;
		}

		protected Command Command {
			get;
			private set;
		}

		protected object Parameter {
			get;
			private set;
		}

		protected override void BindViewModel ()
		{
			UnbindViewModel ();
			Command = commandFunc (ViewModel);
			if (Command != null) {
				UpdateView ();
				Command.CanExecuteChanged += HandleCanExecuteChanged;
			}
		}

		protected override void UnbindViewModel ()
		{
			if (Command != null) {
				Command.CanExecuteChanged -= HandleCanExecuteChanged;
			}
			Command = null;
		}

		protected abstract void UpdateView ();

		protected abstract void HandleCanExecuteChanged (object sender, EventArgs args);
	}
}
