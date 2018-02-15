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

namespace VAS.Core.Interfaces.License
{
	public interface ILicenseManager
	{
		/// <summary>
		/// Identifier of the license container (serial number)
		/// </summary>
		/// <value>The container identifier.</value>
		string ContainerId { get; }

		/// <summary>
		/// Gets the license status.
		/// </summary>
		/// <value>The license status.</value>
		ILicenseStatus LicenseStatus {
			get;
		}

		/// <summary>
		/// Init the license manager
		/// </summary>
		Task Init ();

		/// <summary>
		/// Updates the subscription license.
		/// </summary>
		/// <returns>The subscription license.</returns>
		/// <param name="ticketID">Ticket identifier.</param>
		Task<bool> UpdateSubscriptionLicense (string ticketID);

		/// <summary>
		/// Checks if this application needs to request the user for a license key.
		/// </summary>
		/// <returns><c>true</c>, if license key was requested, <c>false</c> otherwise.</returns>
		bool NeedsLicenseKey ();
	}
}
