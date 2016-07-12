using System;
using VAS.Core.Interfaces.GUI;
using System.Collections.Generic;
using System.Threading.Tasks;

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

		public Task<bool> MoveTo (string transition)
		{
			if (current != null) {
				if (!current.PostTransition ()) {
					return Task.Factory.StartNew (() => false);
				}
			}
			Console.WriteLine ("Moving to " + transition);

			IScreenState panel = destination [transition] ();
			bool ok = panel.PreTransition ();
			if (ok) {
				App.Current.GUIToolkit.LoadPanel (panel.Panel);
				current = panel;
			}
			return Task.Factory.StartNew (() => ok);
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

