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
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace VAS.Core.Interfaces
{
	/// <summary>
	/// Network manager, access to the network always trough this manager
	/// Extend this manager to add the necessary functionality
	/// </summary>
	public interface INetworkManager
	{
		/// <summary>
		/// Checks the network connection.
		/// </summary>
		/// <returns><c>true</c>, if internet connection is OK, <c>false</c> otherwise.</returns>
		bool CheckNetworkConnection ();

		/// <summary>
		/// Gets the network time.
		/// </summary>
		/// <returns>The network time in UTC</returns>
		DateTime GetNetworkTime ();

		/// <summary>
		/// Opens the URL. if sourcePoint is specified it appends to the URL the necessary parameters for tracking
		/// </summary>
		/// <param name="url">URL.</param>
		/// <param name="sourcePoint">Source point.</param>
		void OpenURL (string url, string sourcePoint = null);

		/// <summary>
		/// Downloads the image from the specified uri.
		/// </summary>
		/// <returns>The image.</returns>
		/// <param name="uri">URI.</param>
		/// <param name="ct">Ct.</param>
		Task<Stream> DownloadImage (string uri, CancellationToken? ct = null);
	}
}
