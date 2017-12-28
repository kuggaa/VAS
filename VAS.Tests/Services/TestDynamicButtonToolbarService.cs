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
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.Services;
using VAS.Services.State;
using VAS.Tests.MVVMC;

namespace VAS.Tests.Services
{
	[TestFixture]
    public class TestDynamicButtonToolbarService
	{
		Mock<IStateController> mockStateController;
		DynamicButtonToolbarService service;
		DynamicButtonToolbarVM viewModel;
		DummyState state;
		Command command1;
		Command command2;

		[OneTimeSetUp]
		public void OneTimeSetUp ()
		{
			SetupClass.SetUp ();
		}

		[SetUp]
		public async Task SetUp ()
		{
			state = new DummyState ();
			mockStateController = new Mock<IStateController> ();
			mockStateController.SetupGet (sc => sc.Current).Returns (state);
			App.Current.StateController = mockStateController.Object;

			service = new DynamicButtonToolbarService ();
			viewModel = new DynamicButtonToolbarVM ();
			service.SetViewModel (viewModel);
			await service.Start ();

			command1 = new Command (() => { });
			command2 = new Command (() => { });
		}

		[TearDown]
		public async Task TearDown ()
		{
			await service.Stop ();
		}

		[Test]
		public async Task NavigationEvent_TwoToolbarCommands_ChangesViewModel ()
		{
			int timesCollectionChanged = 0;
			state.SetToolbarCommands (new List<Command> {command1, command2});
			viewModel.ToolbarCommands.CollectionChanged += (sender, e) => timesCollectionChanged++ ;

			await App.Current.EventsBroker.Publish (new NavigationEvent ());

			Assert.AreEqual (2, viewModel.ToolbarCommands.Count);
			Assert.AreSame (command1, viewModel.ToolbarCommands[0]);
			Assert.AreSame (command2, viewModel.ToolbarCommands[1]);
			Assert.AreEqual (1, timesCollectionChanged);
		}

		[Test]
		public async Task NavigationEvent_MultipleTimes_ChangesViewModel ()
		{
			int timesCollectionChanged = 0;
			state.SetToolbarCommands (new List<Command> ());
			viewModel.ToolbarCommands.CollectionChanged += (sender, e) => timesCollectionChanged++;

			await App.Current.EventsBroker.Publish (new NavigationEvent ());
			Assert.AreEqual (0, viewModel.ToolbarCommands.Count);

			state.SetToolbarCommands (new List<Command> { command1, command2 });
			await App.Current.EventsBroker.Publish (new NavigationEvent ());
			Assert.AreEqual (2, viewModel.ToolbarCommands.Count);

			state.SetToolbarCommands (new List<Command> {command2 });
			await App.Current.EventsBroker.Publish (new NavigationEvent ());

			Assert.AreEqual (1, viewModel.ToolbarCommands.Count);
			Assert.AreSame (command2, viewModel.ToolbarCommands [0]);
			Assert.AreEqual (2, timesCollectionChanged);
		}
	}


	class DummyState : ScreenState<IViewModel>
	{
		IEnumerable<Command> toolbarCommands;

		public override IEnumerable<Command> ToolbarCommands {
			get {
				return toolbarCommands;
			}
		}

		public void SetToolbarCommands (IEnumerable<Command> commands)
		{
			toolbarCommands = commands;
		}

		public override string Name => "DummyState";

		protected override void CreateViewModel (dynamic data)
		{
			ViewModel = new DummyViewModel ();
		}
	}
}
