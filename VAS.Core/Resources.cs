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
using System.IO;
using VAS.Core.Common;

namespace VAS.Core
{
	public static class Resources
	{
		public static Image LoadImage (string name)
		{
			var isTest = Environment.GetEnvironmentVariable (StyleConf.TEST) != null ?
									Boolean.Parse (Environment.GetEnvironmentVariable (StyleConf.TEST)) :
									false;
			if (isTest) {
				return null;
			}
			return new Image (Utils.GetDataFilePath (name));
		}
	}
}

