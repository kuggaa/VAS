//
//  Copyright (C) 2016 Fluendo S.A.
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
//

using System;
using System.Globalization;
using VAS;

namespace Microsoft.HockeyApp.Services
{
	internal class ApplicationService : IApplicationService
	{
		/// <summary>
		/// The default application version we will be returning if no application version is found.
		/// </summary>
		internal const string UnknownComponentVersion = "Unknown";

		/// <summary>
		/// The version for this component.
		/// </summary>
		private string version;

		private string fullPackageName;

		private bool initialized = false;

		public event EventHandler OnResuming;

		public event EventHandler OnSuspending;

		/// <summary>
		/// Initializes the service.
		/// </summary>
		public void Init ()
		{
			if (initialized) {
				return;
			}

			initialized = true;
		}

		/// <summary>
		/// Indicates whether the application is installed in development mode.
		/// Always false.
		/// </summary>
		/// <returns>false</returns>
		public bool IsDevelopmentMode ()
		{
			// IsDevelopmentMode API is supported only in UWP, for all others return false.
			return false;
		}

		/// <summary>
		/// Gets the version for the current application. If the version cannot be found, we will return the passed in default.
		/// </summary>
		/// <returns>The extracted data.</returns>
		public string GetVersion ()
		{
			if (this.version != null) {
				return this.version;
			}

			string temp = string.Format (
								CultureInfo.InvariantCulture,
								"{0}.{1}.{2}.{3}",
								App.Current.Version.Major,
								App.Current.Version.Minor,
								App.Current.Version.Build,
								App.Current.Version.Revision);

			if (string.IsNullOrEmpty (temp) == false) {
				return this.version = temp;
			}

			return this.version = UnknownComponentVersion;
		}

		/// <summary>
		/// Gets the application identifier, which is the namespace name for App class.
		/// </summary>
		/// <returns>Namespace name for App class.</returns>
		public string GetApplicationId ()
		{
			if (this.fullPackageName == null) {
				// FIXME: Doesn't match the comment, what should we return here?
				this.fullPackageName = App.Current.SoftwareName;
			}

			return this.fullPackageName;
		}

		/// <summary>
		/// Gets the store region.
		/// </summary>
		/// <returns>The two-letter identifier for the user's region.</returns>
		public string GetStoreRegion ()
		{
			return string.Empty;
		}
	}
}
