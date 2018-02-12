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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using VAS.Core.Common;
using VAS.Core.Interfaces;

namespace VAS.Core
{
	/// <summary>
	/// Base class for the platform resource locators
	/// </summary>
	public abstract class ResourcesLocatorBase : IResourcesLocator
	{
		protected HashSet<Assembly> assemblies;

		public ResourcesLocatorBase ()
		{
			assemblies = new HashSet<Assembly> ();
		}

		/// <summary>
		/// Register the specified assembly with embedded resources
		/// </summary>
		/// <param name="assembly">Assembly.</param>
		public void Register (Assembly assembly)
		{
			assemblies.Add (assembly);
		}

		/// <summary>
		/// Gets the embedded resource file stream.
		/// </summary>
		/// <returns>The embedded resource file stream.</returns>
		/// <param name="resourceId">Resource identifier.</param>
		public Stream GetEmbeddedResourceFileStream (string resourceId)
		{
			var assembly = Assembly.GetCallingAssembly ();
			if (assembly.GetManifestResourceNames ().Contains (resourceId)) {
				return assembly.GetManifestResourceStream (resourceId);
			}
			return null;
		}

		/// <summary>
		/// Loads the embedded image.
		/// </summary>
		/// <returns>The embedded image.</returns>
		/// <param name="resourceId">Resource identifier.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public abstract Image LoadEmbeddedImage (string resourceId, int width = 0, int height = 0);

		/// <summary>
		/// Loads an icon <see cref="Image"/> using the icon name. If <paramref name="width"/> or <paramref name="height"/>
		/// are <c>null</c> it uses the size of the image. This is particularly useful with scalar images to
		/// scale them to the requeried size without loosing quality in the up/down scaling.
		/// </summary>
		/// <returns>The image.</returns>
		/// <param name="name">Name.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public abstract Image LoadImage (string name, int width = 0, int height = 0);

		/// <summary>
		/// Loads an icon <see cref="Image"/> using the icon name. If <paramref name="size"/>
		/// is <c>null</c> it uses the original size of the image.
		/// </summary>
		/// <returns>The icon.</returns>
		/// <param name="name">Name.</param>
		/// <param name="size">Desired size.</param>
		public Image LoadIcon (string name, int size = 0)
		{
			try {
				return LoadImage ("icons/hicolor/scalable/actions/" + name + Constants.IMAGE_EXT, size, size);
			} catch (FileNotFoundException) {
				return LoadImage ("icons/hicolor/scalable/apps/" + name + Constants.IMAGE_EXT, size, size);
			}
		}

		/// <summary>
		/// Tos the name of the resource.
		/// </summary>
		/// <returns>The resource name.</returns>
		/// <param name="name">Name.</param>
		public virtual string ToResourceName (string name)
		{
			return name;
		}
	}
}