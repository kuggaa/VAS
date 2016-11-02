// <copyright file="HttpCluentTransmission.cs" company="Microsoft">
// Copyright © Microsoft. All Rights Reserved.
// </copyright>


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
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Microsoft.HockeyApp.Services
{
	internal class HttpClientTransmission : IHttpService
	{
		internal const string ContentTypeHeader = "Content-Type";
		internal const string ContentEncodingHeader = "Content-Encoding";

		public async Task PostAsync (Uri address, byte [] content, string contentType, string contentEncoding, TimeSpan timeout = default (TimeSpan))
		{
#if DEBUG
			string result = System.Text.Encoding.UTF8.GetString (content, 0, content.Length);
#endif
			HttpClient client = new HttpClient () { Timeout = timeout };
			using (MemoryStream contentStream = new MemoryStream (content)) {
				var request = new HttpRequestMessage (HttpMethod.Post, address);
				request.Content = new StreamContent (contentStream);
				if (!string.IsNullOrEmpty (contentType)) {
					request.Content.Headers.ContentType = new MediaTypeHeaderValue (contentType);
				}

				if (!string.IsNullOrEmpty (contentEncoding)) {
					request.Content.Headers.Add (ContentEncodingHeader, contentEncoding);
				}

				await client.SendAsync (request);
			}
		}

		public Stream CreateCompressedStream (Stream stream)
		{
			return new GZipStream (stream, CompressionMode.Compress);
		}

		public string GetContentEncoding ()
		{
			return "gzip";
		}
	}
}
