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
using System.Linq;
using Mono.Cecil;

namespace Weavers
{
	/// <summary>
	/// Looks for ConnectChild and DisconnectChild methods in BindableBase.
	/// </summary>
	public class PropertyChangedForwarderInjector
	{
		ModuleDefinition moduleDefinition;
		public MethodDefinition ConnectChildMethod, DisconnectChildMethod;
		public MethodReference ConnectChildMethodRef, DisconnectChildMethodRef;
		const string CONNECT = "ConnectChild";
		const string DISCONNECT = "DisconnectChild";

		public PropertyChangedForwarderInjector (ModuleDefinition moduleDefinition)
		{
			this.moduleDefinition = moduleDefinition;
		}

		public void Execute (TypeDefinition type)
		{
			ConnectChildMethod = FindMethod (type, CONNECT);
			ConnectChildMethodRef = ImportMethod (type, ConnectChildMethod, CONNECT);
			DisconnectChildMethod = FindMethod (type, DISCONNECT);
			DisconnectChildMethodRef = ImportMethod (type, DisconnectChildMethod, CONNECT);
		}

		MethodDefinition FindMethod (TypeDefinition type, string methodName)
		{
			MethodDefinition method = type.Methods.FirstOrDefault (m => IsMethod (m, methodName));
			if (method == null && type.BaseType != null) {
				return FindMethod (type.BaseType.Resolve (), methodName);
			}
			return method;
		}

		MethodReference ImportMethod (TypeDefinition type, MethodDefinition method, string methodName)
		{
			if (method != null) {
				if (method.IsStatic) {
					throw new WeavingException (methodName + " method can no be static");
				}
				if (!method.IsFamily) {
					throw new WeavingException (methodName + " method needs to be protected");
				}
				return moduleDefinition.ImportReference (method);
			} else {
				throw new WeavingException (method + " method not found for type " + type.Name);
			}
		}

		bool IsMethod (MethodDefinition method, string methodName)
		{
			return method.Name == methodName && method.Parameters.Count == 1;
		}

	}
}

