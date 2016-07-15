using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

public class Loader
{
	public Action<string> LogInfo { get; set; }

	public ModuleDefinition ModuleDefinition { get; set; }

	public IAssemblyResolver AssemblyResolver { get; set; }

	public const string CHECK_IS_LOADED = "CheckIsLoaded";
	public const string ID = "ID";
	public const string ISTORABLE = "IStorable";

	public Loader ()
	{
		LogInfo = s => {
		};
	}

	public void Execute ()
	{
		var storableTypeFinder = new TypeFinder (ModuleDefinition, AssemblyResolver, ISTORABLE);
		storableTypeFinder.Execute ();

		var exceptionFinder = new ExceptionFinder (ModuleDefinition, AssemblyResolver);
		exceptionFinder.Execute ();

		var checkLoadedInjector = new MethodFinder (ModuleDefinition, CHECK_IS_LOADED, 0);

		var typeResolver = new TypeResolver ();
		var implementsInterfaceFinder = new ImplementsInterfaceFinder (typeResolver);

		var classes = ModuleDefinition.GetTypes ()
			.Where (x => x.IsClass)
			.ToList ();

		foreach (var type in classes) {
			var baseType = implementsInterfaceFinder.HierarchyImplementsIStorable (type);
			if (baseType == null) {
				continue;
			}
			var checkMethod = checkLoadedInjector.Execute (baseType);
			ProcessType (type, checkMethod);
		}
	}

	void ProcessType (TypeDefinition type, MethodReference checkIsLoadedMethod)
	{
		LogInfo ("\t" + type.FullName);

		foreach (var property in type.Properties) {
			if (property.Name == Loader.CHECK_IS_LOADED ||
			    property.Name == String.Format ("{0}.{1}", Loader.ISTORABLE, Loader.CHECK_IS_LOADED) ||
			    property.Name == Loader.ID ||
			    property.Name == String.Format ("{0}.{1}", Loader.ISTORABLE, Loader.ID)) {
				LogInfo ("\tSkip property: " + property.Name);
				continue;
			}

			var getMethod = property.GetMethod;
			if (getMethod == null) {
				continue;
			}
			if (getMethod.IsPrivate) {
				continue;
			}
			if (getMethod.IsAbstract) {
				continue;
			}

			if (property.CustomAttributes.
				FirstOrDefault (a => a.AttributeType.Name == "JsonIgnoreAttribute") != null) {
				LogInfo ("\tSkip ignored property: " + property.Name);
				continue;
			}

			if (property.CustomAttributes.
				FirstOrDefault (a => a.AttributeType.Name == "PropertyPreload") != null) {
				LogInfo ("\tSkip preloaded property: " + property.Name);
				continue;
			}

			ProcessProperty (getMethod, checkIsLoadedMethod);
		}
	}

	void ProcessProperty (MethodDefinition method, MethodReference checkIsLoadedMethod)
	{
		LogInfo ("\tProcessing property: " + method.Name);
		var instructions = method.Body.Instructions;
		if (AlreadyContainsCheck (instructions)) {
			return;
		}
		method.Body.SimplifyMacros ();
		method.Body.Instructions.Insert (0, Instruction.Create (OpCodes.Callvirt, checkIsLoadedMethod));
		method.Body.Instructions.Insert (0, Instruction.Create (OpCodes.Ldarg_0));
		method.Body.OptimizeMacros ();
	}

	static bool AlreadyContainsCheck (Collection<Instruction> instructions)
	{
		return instructions
			.Select (instruction => instruction.Operand)
			.OfType<MethodReference> ()
			.Any (operand => operand.Name == Loader.CHECK_IS_LOADED);
	}
}