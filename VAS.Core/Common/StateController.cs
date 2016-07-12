using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAS.Core.Interfaces.GUI;

namespace VAS.Core
{
	public class StateController
	{
		Dictionary<string, Func<IScreenState>> destination;
		IScreenState current;
		List<string> stateStack;

		public StateController ()
		{
			destination = new Dictionary<string, Func<IScreenState>> ();
			stateStack = new List<string> ();
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
				PushState (transition);
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

		public void PushState (string state)
		{
			stateStack.Add (state);
		}

		public void PopState (string state)
		{
			int position = stateStack.LastIndexOf (state);
			stateStack.RemoveAt (position);
		}

		public void EmptyStateStack ()
		{
			stateStack.Clear ();
		}
	}
}

