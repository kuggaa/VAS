using System.Linq;
using Mono.Cecil;

public class LoadedCheckerInjector
{
	ModuleDefinition moduleDefinition;
	public MethodDefinition CheckIsLoadedMethod;
	const string CHECK_IS_LOADED = "CheckIsLoaded";

	public LoadedCheckerInjector (ModuleDefinition moduleDefinition)
	{
		this.moduleDefinition = moduleDefinition;
	}

	public MethodReference Execute (TypeDefinition type)
	{
		CheckIsLoadedMethod = FindMethod (type);
		if (CheckIsLoadedMethod != null) {
			if (CheckIsLoadedMethod.IsStatic) {
				throw new WeavingException (CHECK_IS_LOADED + " method can no be static");
			}
			if (!CheckIsLoadedMethod.IsFamily) {
				throw new WeavingException (CHECK_IS_LOADED + " method needs to be protected");
			}
			return  moduleDefinition.Import (CheckIsLoadedMethod);
		} else {
			throw new WeavingException (CHECK_IS_LOADED + " method not found for type " + type.Name);
		}
	}

	MethodDefinition FindMethod (TypeDefinition type)
	{
		MethodDefinition method = type.Methods.FirstOrDefault (IsCheckMethod);
		if (method == null && type.BaseType != null) {
			return FindMethod (type.BaseType.Resolve ());
		}
		return method;
	}

	bool IsCheckMethod (MethodDefinition method)
	{
		return method.Name == CHECK_IS_LOADED &&
		method.Parameters.Count == 0;
	}

}