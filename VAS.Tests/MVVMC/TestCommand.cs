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
using NUnit.Framework;
using VAS.Core;
using VAS.Core.MVVMC;

namespace VAS.Tests.MVVMC
{
	[TestFixture]
	public class TestCommand
	{
		[Test]
		public void TestCommandInitExecutable ()
		{
			Command command = new Command ((obj) => { });

			Assert.IsTrue (command.CanExecute ());
		}

		[Test]
		public void CanExecute_Command_OK ()
		{
			var command = new Command (() => { }, () => true);

			Assert.IsTrue (command.CanExecute (true));
		}

		[Test]
		public void CanExecute_CommandParametrizedCanExecute_OK ()
		{
			var command = new Command<bool> (o => { }, o => o);

			Assert.IsTrue (command.CanExecute (true));
		}

		[Test]
		public void CanExecute_GenricCommand_OK ()
		{
			var command = new Command<bool> ((o) => { }, () => true);

			Assert.IsTrue (command.CanExecute ());
		}

		[Test]
		public void CanExecute_GenericCommandParametrizedCanExecute_OK ()
		{
			var command = new Command<bool> (o => { }, o => o);

			Assert.IsTrue (command.CanExecute (true));
		}


		[Test]
		public void CanExecute_AsyncCommandParametrizedCanExecute_OK ()
		{
			var command = new AsyncCommand (AsyncHelpers.Return, (o) => true);

			Assert.IsTrue (command.CanExecute (true));
		}

		[Test]
		public void CanExecute_AsyncCommand_OK ()
		{
			var command = new AsyncCommand (AsyncHelpers.Return, () => true);

			Assert.IsTrue (command.CanExecute ());
		}

		[Test]
		public void CanExecute_GenericAsyncCommandParametrizedCanExecute_OK ()
		{
			var command = new AsyncCommand<bool> (o => { return AsyncHelpers.Return (o); }, (o) => true);

			Assert.IsTrue (command.CanExecute (true));
		}

		[Test]
		public void CanExecute_GenericAsyncCommand_OK ()
		{
			var command = new AsyncCommand<bool> (o => { return AsyncHelpers.Return (o); }, () => true);

			Assert.IsTrue (command.CanExecute ());
		}
	}
}
