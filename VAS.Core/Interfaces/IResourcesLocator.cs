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
using System.IO;
using System.Reflection;
using VAS.Core.Common;

namespace VAS.Core.Interfaces
{
	/// <summary>
	/// Interface to be implemented by the embeddedResourceLocator
	/// </summary>
	public interface IResourcesLocator
	{

		bool TestMode { get; set; }
		/// <summary>
		/// Register the specified assembly with embedded resources
		/// </summary>
		/// <param name="assembly">Assembly.</param>
		void Register (Assembly assembly);

		/// <summary>
		/// Gets the embedded resource file stream.
		/// </summary>
		/// <returns>The embedded resource file stream.</returns>
		/// <param name="resourceId">Resource identifier.</param>
		Stream GetEmbeddedResourceFileStream (string resourceId);

		/// <summary>
		/// Loads the embedded image.
		/// </summary>
		/// <returns>The embedded image.</returns>
		/// <param name="resourceId">Resource identifier.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		Image LoadEmbeddedImage (string resourceId, int width = 0, int height = 0);

		/// <summary>
		/// Loads an icon <see cref="Image"/> using the icon name. If <paramref name="width"/> or <paramref name="height"/>
		/// are <c>null</c> it uses the size of the image. This is particularly useful with scalar images to
		/// scale them to the requeried size without loosing quality in the up/down scaling.
		/// </summary>
		/// <returns>The image.</returns>
		/// <param name="name">Name.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		Image LoadImage (string name, int width = 0, int height = 0);

		/// <summary>
		/// Loads an icon <see cref="Image"/> using the icon name. If <paramref name="size"/>
		/// is <c>null</c> it uses the original size of the image.
		/// </summary>
		/// <returns>The icon.</returns>
		/// <param name="name">Name.</param>
		/// <param name="size">Desired size.</param>
		Image LoadIcon (string name, int size = 0);
	}
}

