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
using System.Threading.Tasks;
using VAS.Core.License;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;

namespace VAS.Core.Interfaces
{
	/// <summary>
	/// License limitations service.
	/// This is the only point in the application where you can get the Limitation objects
	/// </summary>
	public interface ILicenseLimitationsService : IService
	{
		/// <summary>
		/// Get the specified Limitation by name and type
		/// </summary>
		/// <param name="name">Limitation Name</param>
		/// <typeparam name="T">The Limitation Type</typeparam>
		T Get<T> (string name) where T : LimitationVM;

		/// <summary>
		/// Gets all the limitations.
		/// </summary>
		/// <returns>A collection with all the limitations.</returns>
		IEnumerable<LimitationVM> GetAll ();

		/// <summary>
		/// Gets all Count limitations.
		/// </summary>
		/// <returns>A collection with all the limitations.</returns>
		IEnumerable<T> GetAll<T> () where T : LimitationVM;

		/// <summary>
		/// Add the specified count limitation by name.
		/// </summary>
		/// <param name="limitation">Limitation.</param>
		void Add (CountLicenseLimitation limitation, Command command = null);

		/// <summary>
		/// Add the specified feature limitation by name.
		/// </summary>
		/// <param name="limitation">Limitation.</param>
		void Add (FeatureLicenseLimitation limitation);

		/// <summary>
		/// Checks if a limitation feature can be executed
		/// </summary>
		/// <param name="name">Name of the feature limitation</param>
		bool CanExecuteFeature (string name);

		/// <summary>
		/// Moves to the upgrade dialog
		/// </summary>
		/// <returns>The Task of the transition </returns>
		/// <param name="name">Name of the limitation</param>
		Task<bool> MoveToUpgradeDialog (string name);
	}
}
