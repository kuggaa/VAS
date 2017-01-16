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
using VAS.Core.Interfaces;
using VAS.Core.License;

namespace VAS.Services
{
	public abstract class LicenseLimitationsService<T> : IService
		where T : LicenseLimitation
	{
		public ILicenseLimitations<T> LicenseLimitations { get; protected set; }

		bool started;

		protected LicenseLimitationsService ()
		{
			started = false;
		}

		#region IService

		public int Level {
			get {
				return 100;
			}
		}

		public string Name {
			get {
				return "LicenseLimitationsService";
			}
		}

		public bool Start ()
		{
			if (!started) {
				started = true;
			}
			return true;
		}

		public bool Stop ()
		{
			if (started) {
				started = false;
			}
			return true;
		}

		#endregion
	}
}
