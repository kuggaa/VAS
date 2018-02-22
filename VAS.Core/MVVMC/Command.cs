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
using System.Threading.Tasks;
using System.Windows.Input;
using VAS.Core.Common;

namespace VAS.Core.MVVMC
{
	/// <summary>
	/// Command implementation of <see cref="ICommand"/>.
	/// </summary>
	public class Command : ICommand
	{
		public event EventHandler CanExecuteChanged;
		protected Func<object, bool> canExecute;
		protected Func<object, Task> callback;
		bool executable;
		bool isExecuting;
		string iconName;
		string iconInactiveName;

		public Command ()
		{
			callback = o => {
				Log.Debug ("Command called without a callback");
				return AsyncHelpers.Return ();
			};
		}

		public Command (Action<object> execute)
		{
			Contract.Requires (execute != null);

			SetCallback (execute);
			Executable = true;
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
		/// Gets or sets the icon name related to that command
		/// </summary>
		/// <value>The icon name.</value>
		public string IconName {
			get {
				return iconName;
			}
			set {
				iconName = value;
				//FIXME: Use only iconName, so that we can load the same icon with different sizes in different places
				Icon = App.Current.ResourcesLocator.LoadIcon (iconName);
			}
		}

		/// <summary>
		/// Gets or sets the icon related to that command when the command can't be executed
		/// </summary>
		/// <value>The icon.</value>
		public Image IconInactive {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the icon name related to that command when the command can't be executed
		/// </summary>
		/// <value>The inactive icon name.</value>
		public string IconInactiveName {
			get {
				return iconInactiveName;
			}
			set {
				iconInactiveName = value;
				//FIXME: Use only IconInactiveName, so that we can load the same icon with different sizes in different places
				IconInactive = App.Current.ResourcesLocator.LoadIcon (iconInactiveName);
			}
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

		public void SetCallback (Action<object> execute)
		{
			Contract.Requires (execute != null);
			this.callback = (o) => { execute (o); return AsyncHelpers.Return (); };
		}

		public void SetCallback (Action execute)
		{
			Contract.Requires (execute != null);
			SetCallback (o => execute ());
		}

		public void SetCallback (Action<object> execute, Func<object, bool> canExecute)
		{
			Contract.Requires (execute != null);
			Contract.Requires (canExecute != null);
			SetCallback (execute);
			this.canExecute = canExecute;
			EmitCanExecuteChanged ();
		}

		public void SetCallback (Action execute, Func<object, bool> canExecute)
		{
			Contract.Requires (execute != null);
			Contract.Requires (canExecute != null);
			SetCallback (o => execute (), canExecute);
		}

		public void SetCallback (Action execute, Func<bool> canExecute)
		{
			Contract.Requires (execute != null);
			Contract.Requires (canExecute != null);
			SetCallback (o => execute (), o => canExecute ());
		}

		public void SetCallback (Action<object> execute, Func<bool> canExecute)
		{
			Contract.Requires (execute != null);
			Contract.Requires (canExecute != null);
			SetCallback (execute, o => canExecute ());
		}

		public bool CanExecute (object parameter = null)
		{
			if (canExecute != null) {
				return canExecute (parameter);
			} else {
				return Executable;
			}
		}

		public void Execute (object parameter = null)
		{
			InternalExecute (parameter);
		}

		/// <summary>
		/// Executes the command asynchronously.
		/// </summary>
		/// <returns>The task.</returns>
		/// <param name="parameter">Parameter.</param>
		public Task ExecuteAsync (object parameter = null)
		{
			return InternalExecute (parameter);
		}

		public void EmitCanExecuteChanged ()
		{
			if (CanExecuteChanged != null) {
				CanExecuteChanged (this, EventArgs.Empty);
			}
		}

		protected virtual Task InternalExecute (object parameter)
		{
			if (!isExecuting) {
				isExecuting = true;
				Task result = callback (parameter);
				isExecuting = false;
				return result;
			} else {
				Log.Verbose ("Command is already under execution, execute operation skipped");
			}

			return AsyncHelpers.Return ();
		}
	}

	/// <summary>
	/// Command implementation of <see cref="ICommand"/> for async functions.
	/// </summary>
	public class AsyncCommand : Command
	{
		public AsyncCommand ()
		{
		}

		public AsyncCommand (Func<object, Task> execute)
		{
			Contract.Requires (execute != null);

			this.callback = execute;
			Executable = true;
		}

		public AsyncCommand (Func<Task> execute) : this (o => execute ())
		{
			Contract.Requires (execute != null);
		}

		public AsyncCommand (Func<object, Task> execute, Func<object, bool> canExecute) : this (execute)
		{
			Contract.Requires (execute != null);
			Contract.Requires (canExecute != null);

			this.canExecute = canExecute;
		}

		public AsyncCommand (Func<Task> execute, Func<bool> canExecute) : this (o => execute (), o => canExecute ())
		{
			Contract.Requires (execute != null);
			Contract.Requires (canExecute != null);
		}
	}

	/// <summary>
	/// Command implementation of <see cref="ICommand"/> using generics for the type of the first command argument
	/// </summary>
	public class Command<T> : Command
	{
		public Command ()
		{
		}

		public Command (Action<T> execute) : base (o => execute ((T)o))
		{
			Contract.Requires (execute != null);
		}

		public Command (Action<T> execute, Func<T, bool> canExecute) : base (o => execute ((T)o), o => canExecute ((T)o))
		{
			Contract.Requires (execute != null);
			Contract.Requires (canExecute != null);
		}

		public Command (Action<T> execute, Func<bool> canExecute) : base (o => execute ((T)o), (o) => canExecute ())
		{
			Contract.Requires (execute != null);
			Contract.Requires (canExecute != null);
		}
	}

	/// <summary>
	/// Command implementation of <see cref="ICommand"/> for async functions using generics for the type of the first
	/// command argument
	/// </summary>
	public class AsyncCommand<T> : Command<T>
	{
		public AsyncCommand (Func<T, Task> execute)
		{
			Contract.Requires (execute != null);

			this.callback = o => execute ((T)o);
		}

		public AsyncCommand (Func<T, Task> execute, Func<T, bool> canExecute) : this (execute)
		{
			Contract.Requires (execute != null);
			Contract.Requires (canExecute != null);

			this.canExecute = o => canExecute ((T)o);
		}

		public AsyncCommand (Func<T, Task> execute, Func<bool> canExecute) : this (execute)
		{
			Contract.Requires (execute != null);
			Contract.Requires (canExecute != null);

			this.canExecute = o => canExecute ();
		}
	}
}