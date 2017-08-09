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
using VAS.Core.MVVMC;
using VAS.UI.Helpers.Bindings;
using Gtk;

namespace VAS.Tests.Gtk2.MVVMC
{
	[TestFixture]
	public class TestBinding
	{
		[Test]
		public void ButtonBinding_Trigger_CommandExecuted ()
		{
			// Arrange
			bool executed = false;
			Button button = new Button ();
			var binding = button.Bind ((vm) => new Command ((obj) => executed = true));
			binding.ViewModel = new ViewModelBase ();

			// Act
			button.Click ();

			// Assert
			Assert.IsTrue (executed);
		}

		[Test]
		public void ButtonBinding_CanExecuteCommand_ButtonEnabled ()
		{
			// Arrange
			bool executed = false;
			bool canExecute = false;
			Button button = new Button ();
			Command buttonCommand = new Command ((obj) => executed = true, (arg) => canExecute);
			var binding = button.Bind ((vm) => buttonCommand);
			binding.ViewModel = new ViewModelBase ();

			// Act
			canExecute = true;
			buttonCommand.EmitCanExecuteChanged ();

			// Assert
			Assert.IsTrue (button.Sensitive);
		}

		[Test]
		public void ButtonBinding_CanExecuteCommandFalse_ButtonDisabled ()
		{
			// Arrange
			bool executed = false;
			bool canExecute = true;
			Button button = new Button ();
			Command buttonCommand = new Command ((obj) => executed = true, (arg) => canExecute);
			var binding = button.Bind ((vm) => buttonCommand);
			binding.ViewModel = new ViewModelBase ();
			Assert.IsTrue (button.Sensitive);

			// Act
			canExecute = false;
			buttonCommand.EmitCanExecuteChanged ();

			// Assert
			Assert.IsFalse (button.Sensitive);
		}

		[Test]
		public void ButtonBinding_RemoveViewModel_ButtonDisconnectedFromCanExecute ()
		{
			// Arrange
			bool executed = false;
			bool canExecute = true;
			Button button = new Button ();
			Command buttonCommand = new Command ((obj) => executed = true, (arg) => canExecute);
			var binding = button.Bind ((vm) => buttonCommand);
			binding.ViewModel = new ViewModelBase ();
			Assert.IsTrue (button.Sensitive);

			// Act
			binding.ViewModel = null;
			canExecute = false;
			buttonCommand.EmitCanExecuteChanged ();

			// Assert
			Assert.IsTrue (button.Sensitive);
		}

		[Test]
		public void ButtonBinding_RemoveViewModel_ButtonDisconnectedFromCommand ()
		{
			// Arrange
			bool executed = false;
			bool canExecute = true;
			Button button = new Button ();
			Command buttonCommand = new Command ((obj) => executed = true, (arg) => canExecute);
			var binding = button.Bind ((vm) => buttonCommand);
			binding.ViewModel = new ViewModelBase ();
			Assert.IsTrue (button.Sensitive);

			// Act
			binding.ViewModel = null;
			button.Click ();

			// Assert
			Assert.IsFalse (executed);
		}
	}
}
