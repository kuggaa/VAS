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
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using VAS.Core.ViewModel;

namespace VAS.Core.MVVMC
{
	/// <summary>
	/// Limitation command, this commands adds an extra limitation
	/// and can only be executed if the limitation passed in the
	/// constructor is not enabled.
	/// </summary>
	public class LimitationCommand : Command
	{
		protected string limitationName;

		protected LimitationCommand ()
		{
		}

		/// <summary>
		/// Gets or sets an extra condition to meet in order to apply the limitation.
		/// </summary>
		/// <value>The condition.</value>
		public Func<bool> LimitationCondition { get; set; }

		public LimitationCommand (string limitationName, Action<object> execute) : base (execute)
		{
			Contract.Requires (execute != null);

			this.limitationName = limitationName;
		}

		public LimitationCommand (string limitationName, Action execute) : base (execute)
		{
			Contract.Requires (execute != null);

			this.limitationName = limitationName;
		}

		public LimitationCommand (string limitationName, Action<object> execute, Func<object, bool> canExecute)
			: base (execute, canExecute)
		{
			Contract.Requires (execute != null);
			Contract.Requires (canExecute != null);

			this.limitationName = limitationName;
		}

		public LimitationCommand (string limitationName, Action execute, Func<bool> canExecute, Func<bool> condition = null)
			: this (limitationName, o => execute (), o => canExecute ())
		{
			Contract.Requires (execute != null);
			Contract.Requires (canExecute != null);
		}

		protected override Task InternalExecute (object parameter)
		{
			if (!IsExecuteLimited ()) {
				return base.InternalExecute (parameter);
			}
			return App.Current.LicenseLimitationsService.MoveToUpgradeDialog (limitationName);
		}

		bool IsExecuteLimited ()
		{
			bool canExecuteFeature = App.Current.LicenseLimitationsService.CanExecute (limitationName);
			if (LimitationCondition != null && !canExecuteFeature) {
				return LimitationCondition ();
			}

			return !canExecuteFeature;
		}
	}

	public class LimitationAsyncCommand : LimitationCommand
	{
		public LimitationAsyncCommand (string limitationName, Func<object, Task> execute)
		{
			Contract.Requires (execute != null);

			this.execute = execute;
			Executable = true;
			this.limitationName = limitationName;
		}

		public LimitationAsyncCommand (string limitationName, Func<Task> execute)
			: this (limitationName, o => execute ())
		{
			Contract.Requires (execute != null);
		}

		public LimitationAsyncCommand (string limitationName, Func<object, Task> execute, Func<object, bool> canExecute)
			: this (limitationName, execute)
		{
			Contract.Requires (execute != null);
			Contract.Requires (canExecute != null);

			this.canExecute = canExecute;
		}

		public LimitationAsyncCommand (string limitationName, Func<Task> execute, Func<bool> canExecute)
			: this (limitationName, o => execute (), o => canExecute ())
		{
			Contract.Requires (execute != null);
			Contract.Requires (canExecute != null);
		}
	}

	public class LimitationCommand<T> : LimitationCommand
	{
		protected LimitationCommand ()
		{
		}

		public LimitationCommand (string limitationName, Action<T> execute)
			: base (limitationName, o => execute ((T)o))
		{
			Contract.Requires (execute != null);
		}

		public LimitationCommand (string limitationName, Action<T> execute, Func<T, bool> canExecute)
			: base (limitationName, o => execute ((T)o), o => canExecute ((T)o))
		{
			Contract.Requires (execute != null);
			Contract.Requires (canExecute != null);
		}

		public LimitationCommand (string limitationName, Action<T> execute, Func<bool> canExecute)
			: base (limitationName, o => execute ((T)o), (o) => canExecute ())
		{
			Contract.Requires (execute != null);
			Contract.Requires (canExecute != null);
		}
	}

	public class LimitationAsyncCommand<T> : LimitationCommand<T>
	{
		public LimitationAsyncCommand (string limitationName, Func<T, Task> execute)
		{
			Contract.Requires (execute != null);

			this.execute = o => execute ((T)o);
			this.limitationName = limitationName;
		}

		public LimitationAsyncCommand (string limitationName, Func<T, Task> execute, Func<T, bool> canExecute)
			: this (limitationName, execute)
		{
			Contract.Requires (execute != null);
			Contract.Requires (canExecute != null);

			this.canExecute = o => canExecute ((T)o);
		}

		public LimitationAsyncCommand (string limitationName, Func<T, Task> execute, Func<bool> canExecute)
			: this (limitationName, execute)
		{
			Contract.Requires (execute != null);
			Contract.Requires (canExecute != null);

			this.canExecute = o => canExecute ();
		}
	}
}
