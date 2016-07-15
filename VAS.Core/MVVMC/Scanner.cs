﻿//
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
using System;
using System.Reflection;

namespace VAS.Core.MVVMC
{
	public static class Scanner
	{

		/// <summary>
		/// Scans and register Views from the calling assembly. This should be called from all of
		/// the assemblies containing Views in the initialization.
		/// </summary>
		/// <param name="viewLocator">View locator.</param>
		public static void ScanViews (ViewLocator viewLocator)
		{
			Assembly assembly = Assembly.GetCallingAssembly ();
			foreach (Type type in assembly.GetTypes()) {
				foreach (var attribute in type.GetCustomAttributes (typeof(ViewAttribute), true)) {
					viewLocator.Register ((attribute as ViewAttribute).ViewName, type);
				}
			}
		}

		/// <summary>
		/// Scans and register Controllers from the calling assembly. This should be called from all of
		/// the assemblies containing Controllers in the initialization.
		/// </summary>
		/// <param name="viewLocator">View locator.</param>
		public static void ScanControllers (ControllerLocator controllerLocator)
		{
			Assembly assembly = Assembly.GetCallingAssembly ();
			foreach (Type type in assembly.GetTypes()) {
				foreach (var attribute in type.GetCustomAttributes (typeof(ControllerAttribute), true)) {
					controllerLocator.Register ((attribute as ControllerAttribute).ViewName, type);
				}
			}
		}
	}
}

