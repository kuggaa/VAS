using System;
using VAS.Core.Interfaces.GUI;
using System.Collections.Generic;

namespace VAS.Core
{
	public class StateController
	{
		Dictionary<string, Func<IScreenState>> destination;
		IScreenState current;

		public StateController ()
		{
			destination = new Dictionary<string, Func<IScreenState>> ();
		}

		public bool MoveTo (string transition)
		{
			if (current != null) {
				if (!current.PostTransition ()) {
					return false;
				}
			}
			Console.WriteLine ("Moving to " + transition);

			IScreenState panel = destination [transition] ();
			bool ok = panel.PreTransition () && App.Current.GUIToolkit.MainController.SetPanel (panel.Panel);// mainWindow.SetPanel (panel.Panel);
			if (ok) {
				current = panel;
			}
			return ok;
		}

		public void Register (string transition, Func<IScreenState> panel)
		{
			destination [transition] = panel;
		}

		public void UnRegister (string transition)
		{
			// It should recover the previous one - a stack?
			destination.Remove (transition);
		}
	}
}

