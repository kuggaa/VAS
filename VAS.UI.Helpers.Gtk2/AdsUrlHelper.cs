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
using System.Diagnostics;
using VAS.Core.Common;

namespace VAS.UI.Helpers
{
	/// <summary>
	/// Helper for navigation to url's when ads are clicked.
	/// </summary>
	public static class AdsUrlHelper
	{
		/// <summary>
		/// Navigates to the specified url.
		/// </summary>
		/// <param name="url">URL.</param>
		/// <param name="track">If set to <c>true</c> track.</param>
		public static void Start (string url, string sourcePoint = null)
		{
			try {
				// FIXME: If there is no ticketId pass the serialId until the web supports retrieving the ticket id
				// in case is not in the configuration
				string ticketIdValue = App.Current.Config.LicenseCode ?? App.Current.LicenseManager.ContainerId;
				url += $"?ticketID={ticketIdValue}";
#if !DEBUG
				if (!string.IsNullOrEmpty (sourcePoint)) {
					url += $"&utm_source=RiftAnalyst&utm_medium={sourcePoint}&sessionid={App.Current.KPIService.SessionID}&userid={App.Current.Device.ID}";
				}
#endif
				Process.Start (url);
			} catch (Exception ex) {
				Log.Debug ("Failed opening ad: " + ex);
			}
		}
	}
}
