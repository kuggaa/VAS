using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAS.Core.Common;
using VAS.Core.Interfaces.GUI;

namespace VAS.Core
{
	public class StateController
	{
		Dictionary<string, Func<IScreenState>> destination;
		IScreenState current;
		List<string> stateStack;
		List<Transition> overwrittenTransitions;

		struct Transition
		{
			public string key;
			public Func<IScreenState> value;
		}

		public StateController ()
		{
			destination = new Dictionary<string, Func<IScreenState>> ();
			stateStack = new List<string> ();
			overwrittenTransitions = new List<Transition> ();
		}

		public Task<bool> MoveTo (string transition)
		{
			if (current != null) {
				if (!current.PostTransition ()) {
					return Task.Factory.StartNew (() => false);
				}
			}
			Log.Debug ("Moving to " + transition);

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
			if (destination.Keys.Contains (transition)) {
				overwrittenTransitions.Add (new Transition { key = transition, value = destination [transition] });
			}

			destination [transition] = panel;
		}

		public void UnRegister (string transition)
		{
			// Remove from transitions
			destination.Remove (transition);

			// Recover the previous one if exist, and remove from transitionsStack
			if (overwrittenTransitions.Any (x => x.key == transition)) {
				Transition last = overwrittenTransitions.FindLast (x => x.key == transition);
				destination [transition] = last.value;
				overwrittenTransitions.Remove (last);
			}
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
			overwrittenTransitions.Clear ();
		}
	}
}

