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
using VAS.Core.Common;

namespace VAS.Core.Interfaces.License
{
	public interface ILicenseStatus
	{
		/// <summary>
		/// Gets a value indicating whether this <see cref="T:VAS.Core.Interfaces.License.ILicenseStatus"/> is valid.
		/// </summary>
		/// <value><c>true</c> if is valid; otherwise, <c>false</c>.</value>
		bool Valid { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.Interfaces.License.ILicenseStatus"/> is limited.
		/// </summary>
		/// <value><c>true</c> if limited; otherwise, <c>false</c>.</value>
		bool Limited { get; }

		/// <summary>
		/// Gets the protected data.
		/// </summary>
		/// <value>The protected data.</value>
		string ProtectedData {
			get;
		}

		/// <summary>
		/// Gets the name of the license.
		/// </summary>
		/// <value>The name of the license.</value>
		string PlanName {
			get;
		}

		/// <summary>
		/// Gets the limitations.
		/// </summary>
		/// <value>The limitations.</value>
		IEnumerable<string> Limitations {
			get;
		}

		/// <summary>
		/// Gets or sets the message to display.
		/// </summary>
		/// <value>The message to display.</value>
		string Message {
			get;
			set;
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:VAS.Core.Interfaces.License.ILicenseStatus"/> is trial only,
		/// this means that the user does not have any license in the device except for the trial
		/// </summary>
		/// <value><c>true</c> if trial; otherwise, <c>false</c>.</value>
		bool TrialOnly {
			get;
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:VAS.Core.Interfaces.License.ILicenseStatus"/> is trial.
		/// This means that the user can have more licenses but the trial prevails
		/// </summary>
		/// <value><c>true</c> if trial; otherwise, <c>false</c>.</value>
		bool Trial {
			get;
		}

		/// <summary>
		/// Gets the remaining days.
		/// </summary>
		/// <value>The remaining days.</value>
		int RemainingDays {
			get;
		}

		/// Gets or sets a value indicating whether this <see cref="T:FluVAS.License.Wibu.LicenseStatus"/> is a subscription license.
		/// </summary>
		/// <value><c>true</c> if old license; otherwise, <c>false</c>.</value>
		bool SubsciptionLicense {
			get;
		}
	}
}
