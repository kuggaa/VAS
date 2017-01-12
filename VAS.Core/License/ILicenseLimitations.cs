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
using System.Collections.Generic;

namespace VAS.Core.License
{
	/// <summary>
	/// License limitations Interface.
	/// </summary>
	public interface ILicenseLimitations<T> where T : LicenseLimitation
	{
		/// <summary>
		/// Adds the limitation.
		/// </summary>
		/// <param name="limitation">Limitation.</param>
		void AddLimitation (T limitation);

		/// <summary>
		/// Gets the limitations.
		/// </summary>
		/// <returns>The limitations.</returns>
		IEnumerable<T> GetLimitations ();

		/// <summary>
		/// Gets the limitations.
		/// </summary>
		/// <returns>The limitations.</returns>
		/// <param name="limitationName">Limitation name.</param>
		IEnumerable<T> GetLimitations (string limitationName);

		/// <summary>
		/// Sets the limitations status.
		/// </summary>
		/// <param name="status">If set to <c>true</c> status.</param>
		void SetLimitationsStatus (bool status);

		/// <summary>
		/// Sets the limitations status.
		/// </summary>
		/// <param name="limitationName">Limitation name.</param>
		/// <param name="status">If set to <c>true</c> status.</param>
		void SetLimitationsStatus (string limitationName, bool status);
	}
}
