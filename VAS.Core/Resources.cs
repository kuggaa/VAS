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
using System.Text;
using VAS.Core.Common;

namespace VAS.Core
{
	public static class Resources
	{
		/// <summary>
		/// Set this value to <c>true</c> in unit test to create dummy images.
		/// </summary>
		public static bool TEST_MODE = false;

		/// <summary>
		/// Loads an icon <see cref="Image"/> using the icon name. If <paramref name="width"/> or <paramref name="height"/>
		/// are <c>null</c> it uses the size of the image. This is particularly useful with scalar images to
		/// scale them to the requeried size without loosing quality in the up/down scaling.
		/// </summary>
		/// <returns>The image.</returns>
		/// <param name="name">Name.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public static Image LoadImage (string name, int width = 0, int height = 0)
		{
			if (TEST_MODE) {
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
	}


}

