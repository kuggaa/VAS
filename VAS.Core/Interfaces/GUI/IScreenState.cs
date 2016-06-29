using VAS.Core.Hotkeys;

namespace VAS.Core.Interfaces.GUI
{
	public interface IScreenState
	{
		IPanel Panel { get; set; }

		KeyContext PanelKeyContext { get; set; }

		bool PreTransition ();

		bool PostTransition ();
	}
}

