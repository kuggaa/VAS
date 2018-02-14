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
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using VAS.Core.Interfaces;

namespace VAS.Core.Common
{
	/// <summary>
	/// Network manager, access to the network always trough this manager
	/// Extend this manager to add the necessary functionality
	/// </summary>
	public class NetworkManager : INetworkManager
	{
		/// <summary>
		/// Checks the network connection.
		/// </summary>
		/// <returns><c>true</c>, if internet connection is OK, <c>false</c> otherwise.</returns>
		public bool CheckNetworkConnection ()
		{
			try {
				Log.Debug ("Checking Network Connection");
				Ping myPing = new Ping ();
				String host = "8.8.8.8";
				byte [] buffer = new byte [32];
				int timeout = 1000;
				PingOptions pingOptions = new PingOptions ();
				PingReply reply = myPing.Send (host, timeout, buffer, pingOptions);
				bool result = (reply.Status == IPStatus.Success);
				Log.Debug ($"Ping to 8.8.8.8 was {result}");
				return result;
			} catch (Exception) {
				try {
					//Try with NTP server
					Log.Debug ("Error, checking ping connection, trying with NTP Server");
					return GetNetworkTime () > new DateTime (1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
				} catch (Exception ex) {
					Log.Warning ("Error checking network time: " + ex);
					return false;
				}
			}
		}

		/// <summary>
		/// Gets the network time.
		/// </summary>
		/// <returns>The network time in UTC</returns>
		public DateTime GetNetworkTime ()
		{
			//From: http://stackoverflow.com/a/20157068
			const string ntpServer = "pool.ntp.org";
			var ntpData = new byte [48];
			ntpData [0] = 0x1B; //LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)

			var addresses = Dns.GetHostEntry (ntpServer).AddressList;
			var ipEndPoint = new IPEndPoint (addresses [0], 123);
			var socket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			//Stops code hang if NTP is blocked
			socket.ReceiveTimeout = 1000;
			socket.Connect (ipEndPoint);
			socket.Send (ntpData);
			socket.Receive (ntpData);
			socket.Close ();

			ulong intPart = (ulong)ntpData [40] << 24 | (ulong)ntpData [41] << 16 | (ulong)ntpData [42] << 8 | (ulong)ntpData [43];
			ulong fractPart = (ulong)ntpData [44] << 24 | (ulong)ntpData [45] << 16 | (ulong)ntpData [46] << 8 | (ulong)ntpData [47];

			var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
			var networkDateTime = (new DateTime (1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds ((long)milliseconds);

			return networkDateTime;
		}

		/// <summary>
		/// Opens the URL. if sourcePoint is specified it appends to the URL the necessary parameters for tracking
		/// </summary>
		/// <param name="url">URL.</param>
		/// <param name="sourcePoint">Source point.</param>
		public void OpenURL (string url, string sourcePoint = null)
		{
			try {
				// FIXME: If there is no ticketId pass the serialId until the web supports retrieving the ticket id
				// in case is not in the configuration
				string ticketIdValue = App.Current.Config.LicenseCode ?? App.Current.LicenseManager.ContainerId;
				if (url.Contains ("?")) {
					url += "&";
				} else {
					url += "?";
				}
				url += $"ticketID={ticketIdValue}";
#if !DEBUG
				if (!string.IsNullOrEmpty (sourcePoint)) {
					url += $"&utm_source={App.Current.SoftwareName}&utm_medium={sourcePoint}&sessionid={App.Current.KPIService.SessionID}&userid={App.Current.Device.ID}";
				}
#endif
				Process.Start (url);
			} catch (Exception ex) {
				Log.Debug ("Failed opening url: " + ex);
			}
		}

		/// <summary>
		/// Downloads the image from the specified uri.
		/// </summary>
		/// <returns>The image.</returns>
		/// <param name="uri">URI.</param>
		/// <param name="ct">Ct.</param>
		public async Task<Stream> DownloadImage (string uri, CancellationToken? ct = null)
		{
			try {
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create (uri);
				HttpWebResponse response;
				if (!ct.HasValue) {
					response = (HttpWebResponse)await request.GetResponseAsync ();
				} else {
					using (ct.Value.Register (() => request.Abort (), useSynchronizationContext: false)) {
						response = (HttpWebResponse)await request.GetResponseAsync ();
					}
				}

				// Check that the remote file was found. The ContentType
				// check is performed since a request for a non-existent
				// image file might be redirected to a 404-page, which would
				// yield the StatusCode "OK", even though the image was not
				// found.
				if ((response.StatusCode == HttpStatusCode.OK ||
					response.StatusCode == HttpStatusCode.Moved ||
					response.StatusCode == HttpStatusCode.Redirect) &&
					response.ContentType.StartsWith ("image", StringComparison.OrdinalIgnoreCase)) {

					var ms = new MemoryStream ();
					response.GetResponseStream ().CopyTo (ms);
					response.GetResponseStream ().Close ();
					response.GetResponseStream ().Dispose ();
					ms.Seek (0, SeekOrigin.Begin);
					return ms;
				}
				return null;
			} catch (Exception ex) {
				Log.Exception (ex);
				return null;
			}
		}

	}
}
