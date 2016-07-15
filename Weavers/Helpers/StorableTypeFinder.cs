using System.Linq;
using Mono.Cecil;

public class TypeFinder
{
	public TypeReference Type;
	public MethodReference GetIsLoadedMethod;
	IAssemblyResolver assemblyResolver;
	ModuleDefinition moduleDefinition;
	string typeName;

	public TypeFinder (ModuleDefinition moduleDefinition, IAssemblyResolver assemblyResolver, string typeName)
	{
		this.moduleDefinition = moduleDefinition;
		this.assemblyResolver = assemblyResolver;
		this.typeName = typeName;
	}

	public void Execute ()
	{
		Type = moduleDefinition.Types.FirstOrDefault (x => x.Name == typeName);
		if (Type != null) {
			return;
		}
		foreach (var reference in moduleDefinition.AssemblyReferences) {
			var mainModule = assemblyResolver.Resolve (reference).MainModule;
			Type = mainModule.Types.FirstOrDefault (x => x.Name == typeName);
			if (Type != null) {
				return;
			}
		}
	}
}