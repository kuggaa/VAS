//
//  Copyright (C) 2016 Fluendo S.A.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.MVVMC;

namespace VAS.Core.Interfaces.GUI
{
	/// <summary>
	/// Interface for screen states. An IScreenState defines a location where the user can navigate too.
	/// </summary>
	public interface IScreenState : IDisposable
	{
		/// <summary>
		/// The name of the location. This name should be used when registering Views and Controllers to a given state.
		/// </summary>
		/// <value>The name.</value>
		string Name { get; }

		/// <summary>
		/// Gets the panel registered to this state.
		/// </summary>
		/// <value>The panel.</value>
		IPanel Panel { get; }

		/// <summary>
		/// Gets the list of controllers associated to this state.
		/// </summary>
		/// <value>The controllers.</value>
		List<IController> Controllers { get; }

		/// <summary>
		/// Gets the key context associated to this state.
		/// </summary>
		/// <value>The key context.</value>
		KeyContext KeyContext { get; }

		/// <summary>
		/// Method called once when transition is loaded for first time.
		/// This method creates the controllers. And receives data.
		/// </summary>
		/// <returns>The task result.</returns>
		/// <param name="data">ViewModel to be used</param>
		Task<bool> LoadState (dynamic data);

		/// <summary>
		/// Method called once when transition is unloaded.
		/// This method disposes the controllers.
		/// </summary>
		/// <returns>The task result.</returns>
		Task<bool> UnloadState ();

		/// <summary>
		/// Method called to show the state.
		/// </summary>
		/// <returns>The task result.</returns>
		Task<bool> ShowState ();

		/// <summary>
		/// Method called to hide the state.
		/// </summary>
		/// <returns>The task result.</returns>
		Task<bool> HideState ();

	}
}

