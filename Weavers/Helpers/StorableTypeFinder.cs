using System.Linq;
using Mono.Cecil;

public class TypeFinder
{
	IAssemblyResolver assemblyResolver;
	ModuleDefinition moduleDefinition;
	string typeName;

	public TypeFinder (ModuleDefinition moduleDefinition, IAssemblyResolver assemblyResolver, string typeName)
	{
		this.moduleDefinition = moduleDefinition;
		this.assemblyResolver = assemblyResolver;
		this.typeName = typeName;
	}

	public TypeDefinition Execute ()
	{
		var type = moduleDefinition.Types.FirstOrDefault (x => x.Name == typeName);
		if (type != null) {
			return type;
		}
		foreach (var reference in moduleDefinition.AssemblyReferences) {
			var mainModule = assemblyResolver.Resolve (reference).MainModule;
			type = mainModule.Types.FirstOrDefault (x => x.Name == typeName);
			if (type != null) {
				return type;
			}
		}
		return null;
	}
}