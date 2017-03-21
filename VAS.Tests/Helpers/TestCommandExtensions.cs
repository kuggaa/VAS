//
//  Copyright (C) 2017 ${CopyrightHolder}
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
using Gtk;
using NUnit.Framework;
using VAS.Core.MVVMC;
using VAS.UI.Helpers;

namespace VAS.Tests.Helpers
{
	public class TestCommandExtensions
	{
		[Test]
		public void CreateMenuItem_ActivateItem_CommandExecuted ()
		{
			// Arrange
			bool commandExecuted = false;
			Command testcommand = new Command (exec => commandExecuted = !commandExecuted);
			MenuItem testItem = testcommand.CreateMenuItem ("test text");

			// Act
			testItem.Activate ();

			// Assert
			Assert.IsTrue (commandExecuted);
		}

		[Test]
		public void CreateMenuItem_CanExecuteChangeRaised_ItemUpdated ()
		{
			// Arrange
			bool canExecute = false;
			Command testcommand = new Command (exec => { }, can => { return canExecute; });
			MenuItem testItem = testcommand.CreateMenuItem ("test text");
			canExecute = true;

			// Act
			Assert.IsFalse (testItem.Sensitive);
			testcommand.EmitCanExecuteChanged ();

			// Assert
			Assert.IsTrue (testItem.Sensitive);
		}
	}
}
