//  Copyright (C) 2016 Fluendo S.A.
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

