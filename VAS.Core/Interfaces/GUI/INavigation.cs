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
		/// Loads a the navigation panel in the UI.
		/// </summary>
		/// <returns>The navigation panel.</returns>
		/// <param name="panel">Panel.</param>
		Task<bool> Push (IPanel panel);

		/// <summary>
		/// Removes the latest navigation panel from the UI.
		/// This API needs to consider the different UI toolkits implementation.
		/// In GTK+ there is no stacking support, so the action of popping an IPanel is loading the previous IPanel
		/// from the stack, and hence the <paramref name="newPanel"/> is needed.
		/// Instead Xamrin.Forms already supports stacking so we only need to pop the latest IPanel.
		/// </summary>
		/// <param name="newPanel">The panel loaded after popping.</param>
		Task<bool> Pop (IPanel newPanel);

		/// <summary>
		/// Loads a modal panel in the UI. The <paramref name="parentPanel"/> is needed to make the new window modal,
		/// like in the GTK+ toolkit.
		/// </summary>
		/// <param name="panel">The panel to load.</param>
		/// <param name="parent">The parent panel.</param>
		Task PushModal (IPanel panel, IPanel parent);

		/// <summary>
		/// Removes the latest modal panel.
		/// </summary>
		/// <param name="panel">The modal panel to remove.</param>
		Task PopModal (IPanel panel);
	}
}

