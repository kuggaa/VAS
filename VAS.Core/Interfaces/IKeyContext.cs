using System;
using VAS.Core.Hotkeys;


namespace VAS.Core.Interfaces
{
	/// <summary>
	/// Interface to implement every class that has HotKey actions in it
	/// Serves to retrieve the KeyContext
	/// </summary>
	public interface IKeyContext
	{
		KeyContext GetKeyContext ();
	}
}

