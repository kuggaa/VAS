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
	public static class UrlHelper
	{
		/// <summary>
		/// Navigates to the specified url.
		/// </summary>
		/// <param name="url">URL.</param>
		/// <param name="track">If set to <c>true</c> track.</param>
		public static void Start (string url, string sourcePoint = null)
		{
			try {
				if (!string.IsNullOrEmpty (sourcePoint)) {
					url += $"?utm_source=RiftAnalyst&utm_medium={sourcePoint}&sessionid={000000}&userid={000000}";
				}
				Process.Start (url);
			} catch (Exception ex) {
				Log.Debug ("Failed opening ad: " + ex);
			}
		}
	}
}
