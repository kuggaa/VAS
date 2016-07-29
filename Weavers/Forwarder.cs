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
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

namespace Weavers
{

	/// <summary>
	/// A Fody Weaver to inject BindableBase.ConnectChild () in property's setter that notifies
	/// about property changes or collection changes, in our case those of type BindableBase or
	/// ObservableCollection.
	/// 
	/// Original code:
	/// <code>
	/// BindableBase element;
	/// public BindableBase Element {
	///    get {
	///        return element;
	///    }
	///    set {
	///        element = value;
	///    }
	/// }
	/// <code>
	/// 
	/// Converted code:
	/// <code>
	/// 
	/// BindableBase element;
	/// public BindableBase Element {
	///    get {
	///        return element;
	///    }
	///    set {
	///        ConnectChild (base.Element, value);
	///        element = value;
	///    }
	/// }
	/// <code>
	/// 
	/// </summary>
	public class Forwarder
	{
		public Action<string> LogInfo { get; set; }

		public ModuleDefinition ModuleDefinition { get; set; }

		public IAssemblyResolver AssemblyResolver { get; set; }

		public const string BINDABLE_BASE = "BindableBase";
		public const string CONNECT = "ConnectChild";

		MethodReference connectMethod;
		TypeDefinition bindableBaseType;
		HashSet<TypeDefinition> bindableTypes;

		public void Execute ()
		{
			var bindableBaseTypeFinder = new TypeFinder (ModuleDefinition, AssemblyResolver, BINDABLE_BASE);
			bindableBaseType = bindableBaseTypeFinder.Execute ();

			var exceptionFinder = new ExceptionFinder (ModuleDefinition, AssemblyResolver);
			exceptionFinder.Execute ();

			// Find the ConnectChild () method
			var connectFinder = new MethodFinder (ModuleDefinition, CONNECT, 2);
			connectMethod = connectFinder.Execute (bindableBaseType);

			// Get a list of all the classes deriving from BindableBase
			bindableTypes = new HashSet<TypeDefinition> (
				ModuleDefinition.GetTypes ().Where (x => x.IsClass && IsBindableBaseType (x)));

			// Process BindableBase types to inject the ConnectChild () function
			foreach (var type in bindableTypes) {
				ProcessType (type);
			}
		}

		bool IsBindableBaseType (TypeDefinition type)
		{
			if (type == bindableBaseType) {
				return true;
			} else if (type.BaseType == bindableBaseType) {
				return true;
			} else if (type.BaseType != null) {
				return IsBindableBaseType (type.BaseType.Resolve ());
			} else {
				return false;
			}
		}

		void ProcessType (TypeDefinition type)
		{
			LogInfo ("\t" + type.FullName);
			foreach (var property in type.Properties) {
				if (!bindableTypes.Contains (property.PropertyType.Resolve ()) &&
				    !property.PropertyType.Name.StartsWith ("ObservableCollection")) {
					continue;
				}

				var setMethod = property.SetMethod;
				if (setMethod == null) {
					continue;
				}

				var getMethod = property.GetMethod;
				if (getMethod == null) {
					continue;
				}

				if (setMethod.IsPrivate) {
					continue;
				}
				if (setMethod.IsAbstract) {
					continue;
				}

				if (property.CustomAttributes.FirstOrDefault (
					    a => a.AttributeType.Name == "JsonIgnoreAttribute" ||
					    a.AttributeType.Name == "DoNotNotify") != null) {
					continue;
				}

				LogInfo ("\t\t" + property.PropertyType);
				ProcessProperty (setMethod, getMethod);
			}
		}

		void ProcessProperty (MethodDefinition setMethod, MethodDefinition getMethod)
		{
			int i = 0;
			LogInfo ("\t\t" + setMethod.Name);
			var instructions = setMethod.Body.Instructions;
			if (AlreadyContainsCheck (instructions)) {
				return;
			}
			setMethod.Body.SimplifyMacros ();

			// First call ConnectChild ().
			setMethod.Body.Instructions.Insert (i++, Instruction.Create (OpCodes.Ldarg_0));
			setMethod.Body.Instructions.Insert (i++, Instruction.Create (OpCodes.Ldarg_0));
			if (getMethod.IsVirtual) {
				setMethod.Body.Instructions.Insert (i++, Instruction.Create (OpCodes.Callvirt, getMethod));
			} else {
				setMethod.Body.Instructions.Insert (i++, Instruction.Create (OpCodes.Call, getMethod));
			}
			setMethod.Body.Instructions.Insert (i++, Instruction.Create (OpCodes.Ldarg_1));
			setMethod.Body.Instructions.Insert (i++, Instruction.Create (OpCodes.Call, connectMethod));

			setMethod.Body.OptimizeMacros ();
		}

		static bool AlreadyContainsCheck (Collection<Instruction> instructions)
		{
			return instructions
				.Select (instruction => instruction.Operand)
				.OfType<MethodReference> ()
				.Any (operand => operand.Name == CONNECT);
		}
	}
}

