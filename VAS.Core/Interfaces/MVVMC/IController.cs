//
//  Copyright (C) 2016 Fluendo S.A.
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
using VAS.Core.Hotkeys;
using VAS.Core.Services;

namespace VAS.Core.Interfaces.MVVMC
{
	/// <summary>
	/// Interface the for the Controller components in the MVVMC framework.
	/// </summary>
	public interface IController : IDisposable
	{
		/// <summary>
		/// Starts the Controller subscribing to the required events from the <see cref="EventsBroker"/>.
		/// </summary>
		void Start ();

		/// <summary>
		/// Stop the Controller by un-subscribing to the events and resetting its state.
		/// </summary>
		void Stop ();

		/// <summary>
		/// Sets the ViewModel associated to this Controller.
		/// </summary>
		/// <param name="viewModel"> The ViewModel.</param>
		void SetViewModel (IViewModel viewModel);

		/// <summary>
		/// Gets the default key actions to be added to IPanel in pre-transition.
		/// </summary>
		/// <returns>The default key actions.</returns>
		IEnumerable<KeyAction> GetDefaultKeyActions ();
	}
}

