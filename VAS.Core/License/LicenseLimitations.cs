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
using System.Linq;

namespace VAS.Core.License
{
	/// <summary>
	/// License limitations.
	/// </summary>
	public class LicenseLimitations<T> : ILicenseLimitations<T>
		where T : LicenseLimitation
	{
		protected List<T> Limitations { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:VAS.Core.License.LicenseLimitations`1"/> class.
		/// </summary>
		public LicenseLimitations ()
		{
			Limitations = new List<T> ();
		}

		public void AddLimitation (T limitation)
		{
			Limitations.Add (limitation);
		}

		public IEnumerable<T> GetLimitations ()
		{
			return Limitations.AsEnumerable ();
		}

		public IEnumerable<T> GetLimitations (string limitationName)
		{
			return Limitations.Where ((l) => l.LimitationName == limitationName);
		}

		public void SetLimitationsStatus (bool status)
		{
			Limitations.ForEach (l => l.Enabled = true);
		}

		public void SetLimitationsStatus (string limitationName, bool status)
		{
			foreach (var limit in GetLimitations (limitationName)) {
				limit.Enabled = status;
			}
		}
	}
}
