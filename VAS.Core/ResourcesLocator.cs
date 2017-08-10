//
//  Copyright (C) 2015 Fluendo S.A.
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
using System.IO;
using VAS.Core.Common;

namespace VAS.Core
{
	class ResourcesLocator : ResourcesLocatorBase
	{
		/// <summary>
		/// Loads an icon <see cref="Image"/> using the icon name. If <paramref name="width"/> or <paramref name="height"/>
		/// are <c>null</c> it uses the size of the image. This is particularly useful with scalar images to
		/// scale them to the requeried size without loosing quality in the up/down scaling.
		/// </summary>
		/// <returns>The image.</returns>
		/// <param name="name">Name.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public override Image LoadImage (string name, int width = 0, int height = 0)
		{
			if (width != 0 && height != 0) {
				return new Image (Utils.GetDataFilePath (name), width, height);
			}
			return new Image (Utils.GetDataFilePath (name));
		}

		/// <summary>
		/// Loads the embedded image.
		/// </summary>
		/// <returns>The embedded image.</returns>
		/// <param name="resourceId">Resource identifier.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public override Image LoadEmbeddedImage (string resourceId, int width = 0, int height = 0)
		{
			var embeddedStream = GetEmbeddedResourceFileStream (resourceId);
			if (embeddedStream != null) {
				if (width != 0 && height != 0) {
					return new Image (embeddedStream, width, height);
				}
				return new Image (embeddedStream);
			}
			return null;
		}
	}
}