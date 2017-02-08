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

namespace VAS.Core.Interfaces
{
	public interface IAppUpdater
	{
		/// <summary>
		/// Configure the updates checker and start checking for updates.
		/// </summary>
		/// <param name="companyName">Company name.</param>
		/// <param name="appName">App name.</param>
		/// <param name="version">Version.</param>
		/// <param name="castURL">Cast URL.</param>
		/// <param name="baseDir">Base dir.</param>
		void Start (string companyName, string appName, string version, string castURL, string baseDir);

		/// <summary>
		/// Stop checking for updates and performs cleanups.
		/// </summary>
		void Stop ();

		/// <summary>
		/// Manually checks for updates, for example from a menu item.
		/// </summary>
		void CheckForUpdates ();
	}
}
