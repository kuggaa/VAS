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
using System.Threading.Tasks;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;

namespace VAS.Services
{
	/// <summary>
	/// Dynamic button toolbar service, listens to navigation events in order to change the collection of Toolbar commands
	/// </summary>
	public class DynamicButtonToolbarService : ControllerBase<DynamicButtonToolbarVM>, IService
    {
		/// <summary>
		/// Gets the level of the service
		/// </summary>
		/// <value>The level.</value>
		public int Level => 0;

		/// <summary>
		/// Gets the name of the service
		/// </summary>
		/// <value>The name</value>
		public string Name => nameof (DynamicButtonToolbarService);

		/// <summary>
		/// Sets the view model.
		/// </summary>
		/// <param name="viewModel">View model.</param>
		public override void SetViewModel (IViewModel viewModel)
		{
			ViewModel = (DynamicButtonToolbarVM)viewModel;
		}

		/// <summary>
		/// Start this service.
		/// </summary>
		/// <returns>The start Task</returns>
		public override async Task Start ()
		{
			await base.Start ();
			App.Current.EventsBroker.Subscribe<NavigationEvent> (HandleNavigationEvent);
		}

		/// <summary>
		/// Stop this service.
		/// </summary>
		/// <returns>The stop Task</returns>
		public override async Task Stop ()
		{
			await base.Stop ();
			App.Current.EventsBroker.Unsubscribe<NavigationEvent> (HandleNavigationEvent);
		}

		bool IService.Start ()
		{
			Start ();
			return true;
		}

		bool IService.Stop ()
		{
			Stop ();
			return true;
		}

		void HandleNavigationEvent (NavigationEvent e)
		{
			ViewModel.ToolbarCommands.Reset (App.Current.StateController.Current.ToolbarCommands);
		}
	}
}
