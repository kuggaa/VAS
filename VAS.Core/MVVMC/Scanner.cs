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
using System;
using System.Reflection;

namespace VAS.Core.MVVMC
{
	public static class Scanner
	{
		public static void ScanAll ()
		{
			Assembly assembly = Assembly.GetCallingAssembly ();
			foreach (Type type in assembly.GetTypes ()) {
				ScanDependencyServices (type);
				ScanViews (type);
				ScanControllers (type);
			}
		}

		/// <summary>
		/// Scans all the depdency services with the <see cref="DependencyServiceAttribute"/> attribute set.
		/// </summary>
		static void ScanDependencyServices (Type type)
		{
			foreach (var attribute in type.GetCustomAttributes (typeof (DependencyServiceAttribute), true)) {
				var regAttribute = (attribute as DependencyServiceAttribute);
				App.Current.DependencyRegistry.Register (regAttribute.InterfaceType, type, regAttribute.Priority);
			}
		}

		/// <summary>
		/// Scans and register Views from the calling assembly. This should be called from all of
		/// the assemblies containing Views in the initialization.
		/// </summary>
		static void ScanViews (Type type)
		{
			foreach (var attribute in type.GetCustomAttributes (typeof (ViewAttribute), true)) {
				App.Current.ViewLocator.Register ((attribute as ViewAttribute).ViewName, type);
			}
		}

		/// <summary>
		/// Scans and register Controllers from the calling assembly. This should be called from all of
		/// the assemblies containing Controllers in the initialization.
		/// </summary>
		/// <param name="controllerLocator">Controller locator.</param>
		static void ScanControllers (Type type)
		{
			foreach (var attribute in type.GetCustomAttributes (typeof (ControllerAttribute), true)) {
				App.Current.ControllerLocator.Register ((attribute as ControllerAttribute).ViewName, type);
			}
		}

		/// <summary>
		/// Scans and register Controllers from the referenced assemblies to the calling assembly. It also loads each
		/// referenced assembly This should be called from all tests that are testing states in the initialization.
		/// </summary>
		/// <param name="controllerLocator">Controller locator.</param>
		internal static void ScanReferencedControllers (ControllerLocator controllerLocator)
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly ();
			foreach (AssemblyName assemblyName in callingAssembly.GetReferencedAssemblies ()) {
				var assembly = Assembly.Load (assemblyName);
				foreach (Type type in assembly.GetTypes ()) {
					ScanControllers (type);
				}
			}
		}
	}
}

