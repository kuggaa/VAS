using System;
using System.Collections.Generic;
using Mono.Cecil;

public class AssemblyProcessor
{
	MethodInjector loadedCheckerInjector;
	ImplementsInterfaceFinder implementsInterfaceFinder;
	Action<string> logInfo;

	public AssemblyProcessor (MethodInjector loadedCheckerInjector, ImplementsInterfaceFinder implementsInterfaceFinder, Action<string> logInfo)
	{
		this.loadedCheckerInjector = loadedCheckerInjector;
		this.implementsInterfaceFinder = implementsInterfaceFinder;
		this.logInfo = logInfo;
	}

	public void Execute (List<TypeDefinition> classes)
	{
		foreach (var type in classes) {
		}

		foreach (var type in classes) {
			var baseType = implementsInterfaceFinder.HierarchyImplementsIStorable (type);
			if (baseType == null) {
				continue;
			}
			var checkMethod = loadedCheckerInjector.Execute (baseType);

			var typeProcessor = new TypeProcessor (logInfo, checkMethod, type);
			typeProcessor.Execute ();
		}

	}
}