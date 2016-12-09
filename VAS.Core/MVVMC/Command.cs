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
using System.Windows.Input;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Runtime.CompilerServices;
using VAS.Core.Common;

namespace VAS.Core.MVVMC
{
	public class Command : ICommand
	{
		public event EventHandler CanExecuteChanged;
		protected Func<object, bool> canExecute;
		readonly Action<object> execute;
		bool executable;

		public Command (Action<object> execute)
		{
			Contract.Requires (execute != null);

			this.execute = execute;
		}

		public Command (Action execute) : this (o => execute ())
		{
			Contract.Requires (execute != null);
		}

		public Command (Action<object> execute, Func<object, bool> canExecute) : this (execute)
		{
			Contract.Requires (execute != null);
			Contract.Requires (canExecute != null);

			this.canExecute = canExecute;
		}

		public Command (Action execute, Func<bool> canExecute) : this (o => execute (), o => canExecute ())
		{
			Contract.Requires (execute != null);
			Contract.Requires (canExecute != null);
		}

		public bool Executable {
			private get {
				return executable;
			}
			set {
				if (canExecute != null) {
					throw new InvalidOperationException ("Executable property can't be used with a Command initialized "
														 + "with a canExecute function");
				}
				if (value != executable) {
					executable = value;
					EmitCanExecuteChanged ();
				}
			}
		}

		/// <summary>
		/// Gets or sets the icon related to that command
		/// </summary>
		/// <value>The icon.</value>
		public Image Icon {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the text related to that command
		/// </summary>
		/// <value>The text.</value>
		public string Text {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the tool tip text related to that command
		/// </summary>
		/// <value>The tool tip text.</value>
		public string ToolTipText {
			get;
			set;
		}

		public bool CanExecute (object parameter)
		{
			if (canExecute != null) {
				return canExecute (parameter);
			} else {
				return Executable;
			}
		}

		public void Execute (object parameter)
		{
			execute (parameter);
		}

		public void EmitCanExecuteChanged ()
		{
			if (CanExecuteChanged != null) {
				CanExecuteChanged (this, EventArgs.Empty);
			}
		}
	}

	public sealed class Command<T> : Command
	{
		public Command (Action<T> execute) : base (o => execute ((T)o))
		{
			Contract.Requires (execute != null);
		}

		public Command (Action<T> execute, Func<T, bool> canExecute) : base (o => execute ((T)o), o => canExecute ((T)o))
		{
			Contract.Requires (execute != null);
			Contract.Requires (canExecute != null);
		}
	}
}