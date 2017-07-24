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

namespace VAS.Core.MVVMC
{
	public class LimitationCommand : Command
	{
		protected LimitationCommand ()
		{
		}

		public LimitationCommand (string limitationName, Action<object> execute) : base (execute)
		{
			LimitationName = limitationName;
		}

		public LimitationCommand (string limitationName, Action execute) : base (execute)
		{
			LimitationName = limitationName;
		}

		protected string LimitationName { get; set; }

		protected override Task InternalExecute (object parameter)
		{

			return base.InternalExecute (parameter);
		}
	}

	public class LimitationAsyncCommand : LimitationCommand
	{
		public LimitationAsyncCommand (string limitationName, Func<object, Task> execute)
		{
			Contract.Requires (execute != null);

			this.execute = execute;
			Executable = true;
			LimitationName = limitationName;
		}

		public LimitationAsyncCommand (string limitationName, Func<Task> execute) : this (limitationName, o => execute ())
		{
			Contract.Requires (execute != null);
		}
	}

	public class LimitationCommand<T> : LimitationCommand
	{
		protected LimitationCommand ()
		{
		}

		public LimitationCommand (string limitationName, Action<T> execute) : base (limitationName, o => execute ((T)o))
		{
			Contract.Requires (execute != null);
		}
	}

	public class LimitationAsyncCommand<T> : LimitationCommand<T>
	{
		public LimitationAsyncCommand (string limitationName, Func<T, Task> execute)
		{
			Contract.Requires (execute != null);
			this.execute = o => execute ((T)o);
			LimitationName = limitationName;
		}
	}
}
