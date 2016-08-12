//
//  Copyright (C) 2014 Andoni Morales Alastruey
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

namespace VAS.Core.Interfaces.GUI
{
	public interface INavigation
	{
		/// <summary>
		/// Loads the navigation panel. It just removes the last panel from the main window
		/// and replaces it with the new IPanel provided as argument.
		/// </summary>
		/// <returns>The navigation panel.</returns>
		/// <param name="panel">Panel.</param>
		Task<bool> LoadNavigationPanel (IPanel panel);

		/// <summary>
		/// Loads the modal panel. It creates a new ExternalWindow
		/// </summary>
		/// <returns>The modal panel.</returns>
		/// <param name="panel">Panel.</param>
		/// <param name="parent">Parent.</param>
		Task LoadModalPanel (IPanel panel, IPanel parent);

		/// <summary>
		/// Removes the modal window.
		/// </summary>
		/// <returns>The modal window.</returns>
		/// <param name="panel">Panel.</param>
		Task RemoveModalWindow (IPanel panel);
	}
}

