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
using System.Collections.Generic;
using VAS.Core.Interfaces.Drawing;
using System.Linq;
using System.Runtime.Remoting;

namespace VAS.Drawing
{
	public class TypeAndAssembly
	{
		public Type Type { get; set; }

		public string AssemblyName { get; set; }
	}

	public static class CanvasFromDrawableObjectRegistry
	{
		public static void AddMapping (Type source, Type target, string targetAssemblyName)
		{
			CanvasFromDrawableObjectMap [source.ToString ()] = 
				new TypeAndAssembly {
				Type = target,
				AssemblyName = targetAssemblyName
			};
		}

		public static ICanvasDrawableObject CanvasFromDrawableObject (IBlackboardObject srcInstance)
		{
			TypeAndAssembly ta = CanvasFromDrawableObjectMap [srcInstance.GetType ().ToString ()];
			ObjectHandle handle = null;		
			handle = Activator.CreateInstance (ta.AssemblyName, ta.Type.ToString ());
			return (ICanvasDrawableObject)handle.Unwrap ();
		}

		static Dictionary<string, TypeAndAssembly> CanvasFromDrawableObjectMap { get; set; }
			= new Dictionary<string, TypeAndAssembly> ();
	}
}

