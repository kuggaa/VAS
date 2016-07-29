using System.Threading.Tasks;
using VAS.Core.Hotkeys;
﻿using VAS.Core.Interfaces.MVVMC;

namespace VAS.Core.Interfaces.GUI
{
	public interface IScreenState
	{
		IPanel Panel { get; set; }

		KeyContext PanelKeyContext { get; set; }

		Task<bool> PreTransition (object data);

		Task<bool> PostTransition ();
	}
}

