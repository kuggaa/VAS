//
//  Copyright (C) 2017 FLUENDO S.A.
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
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;

namespace VAS.Core.Interfaces
{
	/// <summary>
	/// State controller. Class that performs all the application Navigation.
	/// It works with transition names and Screen states. It then calls the
	/// corresponding GUI toolkit to replace panels in the MainWindow and to
	/// create modal windows
	/// </summary>
	public interface IStateController
	{
		/// <summary>
		/// Gets the current screen state of the application.
		/// </summary>
		/// <value>The current screen state.</value>
		IScreenState Current { get; }

		/// <summary>
		/// Sets the home transition. Needs to be registered first.
		/// </summary>
		/// <returns>True if the home transition could be executed. False otherwise</returns>
		/// <param name="transition">Transition.</param>
		Task<bool> SetHomeTransition (string transition, dynamic properties);

		/// <summary>
		/// Moves to a Panel inside the main window. If it has some previous modal windows
		/// It Pops them all.
		/// </summary>
		/// <returns>True if the move could be performed. False otherwise</returns>
		/// <param name="transition">Transition.</param>
		/// <param name="emptyStack">Cleans the transition stack.</param>
		/// <param name="forceMove">Forces the transition.</param>
		Task<bool> MoveTo (string transition, dynamic properties, bool emptyStack = false, bool forceMove = false);

		/// <summary>
		/// Moves to a Modal window
		/// </summary>
		/// <returns>True if the Move could be performed. False otherwise</returns>
		/// <param name="transition">Transition.</param>
		/// <param name="properties">State properties.</param>
		/// <param name="waitUntilClose">Flow is not returned until the transition has been completed.</param>
		Task<bool> MoveToModal (string transition, dynamic properties, bool waitUntilClose = false);

		/// <summary>
		/// Moves Back to the previous transition Panel or Modal.
		/// </summary>
		/// <returns>True: If the transition could be performed. False Otherwise</returns>
		Task<bool> MoveBack ();

		/// <summary>
		/// Moves the back to previous transition. It also considers Home name transition and goes back home
		/// </summary>
		/// <returns>Ture: If transition could be performed. False otherwise</returns>
		/// <param name="transition">Transition name</param>
		Task<bool> MoveBackTo (string transition);

		/// <summary>
		/// Moves to home. It clears all Modal windows and Panels in the Main Window.
		/// </summary>
		/// <returns>True if the move to the home could be performed. False otherwise</returns>
		Task<bool> MoveToHome (bool forceMove = false);

		/// <summary>
		/// Register the specified transition and panel Initialization function with a param name="toolbar"
		/// </summary>
		/// <param name="transition">Transition.</param>
		/// <param name="panel">Panel.</param>
		void Register (string transition, Func<IScreenState> panel);

		/// <summary>
		/// Register the specified transition and panel Initialization function with a param name="toolbar"
		/// </summary>
		/// <param name="transition">Transition.</param>
		/// <param name="command">Command.</param>
		/// <param name="panel">Panel.</param>
		void Register (string transition, Command command, Func<IScreenState> panel);

		/// <summary>
		/// Removes a registered transition.
		/// </summary>
		/// <param name="transition">Transition.</param>
		bool UnRegister (string transition);

		/// <summary>
		/// Gets the transition commands.
		/// </summary>
		/// <returns>The transition commands.</returns>
		Dictionary<string, Command> GetTransitionCommands ();
	}
}
