//
//  Copyright (C) 2016 
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

namespace VAS.Core.Interfaces
{
	public interface IKpiService
	{
		/// <summary>
		/// Init the KPIService with the specified appId, user and email.
		/// </summary>
		/// <param name="appId">App identifier.</param>
		/// <param name="user">User.</param>
		/// <param name="email">Email.</param>
		Task Init (string appId, string user, string email);

		/// <summary>
		/// Tracks the event.
		/// </summary>
		/// <param name="eventName">Event name.</param>
		/// <param name="properties">Properties.</param>
		/// <param name="metrics">Metrics.</param>
		void TrackEvent (string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null);

		/// <summary>
		/// Tracks the exception.
		/// </summary>
		/// <param name="ex">Ex.</param>
		void TrackException (Exception ex, IDictionary<string, string> properties = null);

		/// <summary>
		/// Flush on current HockeyCLient when called.
		/// </summary>
		void Flush ();
	}
}
