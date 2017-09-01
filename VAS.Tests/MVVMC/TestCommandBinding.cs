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
using NUnit.Framework;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
namespace VAS.Tests.MVVMC
{

	class DummyViewModel : ViewModelBase<BindableBase>
	{
		public object param;

		public DummyViewModel ()
		{
			TestCommand = new Command ((obj) => this.param = obj);
		}

		public Command TestCommand { get; set; }

		public bool TestProperty { get; set; }
	}

	class DummyTestBinding : CommandBinding
	{
		public DummyTestBinding (Func<IViewModel, Command> commandFunc, object parameter) : base (commandFunc, parameter)
		{
		}

		protected override void BindView ()
		{
		}

		protected override void UnbindView ()
		{
		}

		protected override void HandleCanExecuteChanged (object sender, EventArgs args)
		{
		}

		protected override void UpdateView ()
		{
		}

		public void Trigger ()
		{
			Command.Execute (Parameter);
		}
	}

	[TestFixture]
	public class TestCommandBinding
	{
		[Test]
		public void TestBindCommand ()
		{
			DummyViewModel viewModel = new DummyViewModel ();
			DummyTestBinding binding = new DummyTestBinding (vm => ((DummyViewModel)vm).TestCommand, "test");
			binding.ViewModel = viewModel;
			binding.Trigger ();

			Assert.AreEqual ("test", viewModel.param);
		}

		[Test]
		public void TestBindCommand_ReplacingViewModel ()
		{
			DummyViewModel viewModel1 = new DummyViewModel ();
			DummyViewModel viewModel2 = new DummyViewModel ();
			DummyTestBinding binding = new DummyTestBinding (vm => ((DummyViewModel)vm).TestCommand, "test");
			binding.ViewModel = viewModel1;
			binding.Trigger ();
			binding.ViewModel = viewModel2;
			binding.Trigger ();

			Assert.AreEqual ("test", viewModel1.param);
			Assert.AreEqual ("test", viewModel2.param);
		}
	}
}
