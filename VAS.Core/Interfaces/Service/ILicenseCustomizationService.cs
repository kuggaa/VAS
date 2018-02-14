//
//  Copyright (C) 2018 Fluendo S.A.
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

namespace VAS.Core.Interfaces.Service
{
	/// <summary>
	/// License customization service interface. Service to customize objects based on current license status
	/// </summary>
	public interface ILicenseCustomizationService
	{
		/// <summary>
		/// Gets the logo name.
		/// </summary>
		/// <returns>The logo name.</returns>
		string GetLogoName ();

		/// <summary>
		/// Gets the license information text.
		/// </summary>
		/// <returns>The license information text.</returns>
		string GetLicenseInformationText ();

		/// <summary>
		/// Opens the upgrade URL.
		/// </summary>
		void OpenUpgradeURL ();

		/// <summary>
		/// Gets the license banner visibility.
		/// </summary>
		/// <returns><c>true</c>, if license banner is visible, <c>false</c> otherwise.</returns>
		bool GetLicenseBannerVisibility ();
	}
}
