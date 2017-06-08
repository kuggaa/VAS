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
	public class ResourcesLocator : IResourcesLocator
	{
		HashSet<Assembly> assemblies;

		/// <summary>
		/// Set this value to <c>true</c> in unit test to create dummy images.
		/// </summary>
		static bool TEST_MODE = false;

		public bool TestMode {
			get {
				return TEST_MODE;
			}

			set {
				TEST_MODE = value;
			}
		}

		public ResourcesLocator ()
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
			foreach (var assembly in assemblies) {
				if (assembly.GetManifestResourceNames ().Contains (resourceId)) {
					return assembly.GetManifestResourceStream (resourceId);
				}
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
		public Image LoadEmbeddedImage (string resourceId, int width = 0, int height = 0)
		{
			if (TestMode) {
				string svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16px\" height=\"16px\"/>";
				using (Stream s = new MemoryStream (Encoding.UTF8.GetBytes (svg))) {
					return new Image (s);
				}
			}
			var embeddedStream = GetEmbeddedResourceFileStream (resourceId);
			if (embeddedStream != null) {
				if (width != 0 && height != 0) {
					return new Image (embeddedStream, width, height);
				}
				return new Image (embeddedStream);
			}
			return null;
		}

		/// <summary>
		/// Loads an icon <see cref="Image"/> using the icon name. If <paramref name="width"/> or <paramref name="height"/>
		/// are <c>null</c> it uses the size of the image. This is particularly useful with scalar images to
		/// scale them to the requeried size without loosing quality in the up/down scaling.
		/// </summary>
		/// <returns>The image.</returns>
		/// <param name="name">Name.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Image LoadImage (string name, int width = 0, int height = 0)
		{
			if (TestMode) {
				string svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16px\" height=\"16px\"/>";
				using (Stream s = new MemoryStream (Encoding.UTF8.GetBytes (svg))) {
					return new Image (s);
				}
			}
			if (width != 0 && height != 0) {
				return new Image (Utils.GetDataFilePath (name), width, height);
			}
			return new Image (Utils.GetDataFilePath (name));
		}

		/// <summary>
		/// Loads an icon <see cref="Image"/> using the icon name. If <paramref name="size"/>
		/// is <c>null</c> it uses the original size of the image.
		/// </summary>
		/// <returns>The icon.</returns>
		/// <param name="name">Name.</param>
		/// <param name="size">Desired size.</param>
		public Image LoadIcon (string name, int size = 0)
		{
			return LoadImage ("icons/hicolor/scalable/actions/" + name + StyleConf.IMAGE_EXT, size, size);
		}
	}
}