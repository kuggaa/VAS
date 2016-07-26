using VAS.Core.Hotkeys;
using System.Threading.Tasks;

namespace VAS.Core.Interfaces.GUI
{
	public interface IScreenState
	{
		IPanel Panel { get; set; }

		KeyContext PanelKeyContext { get; set; }

		Task<bool> PreTransition ();

		Task<bool> PostTransition ();
	}
}

