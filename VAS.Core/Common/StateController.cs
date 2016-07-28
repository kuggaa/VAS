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
		Dictionary<string, Stack<Func<IScreenState>>> overwrittenTransitions;

		public StateController ()
		{
			destination = new Dictionary<string, Func<IScreenState>> ();
			stateStack = new List<string> ();
			overwrittenTransitions = new Dictionary<string, Stack<Func<IScreenState>>> ();
		}

		public async Task<bool> MoveTo (string transition)
		{
			Log.Debug ("Moving to " + transition);

			if (!destination.ContainsKey (transition)) {
				Log.Debug ("Moving failed because transition" + transition + " is not in destination dictionary.");

				return await Task.Factory.StartNew (() => false);
			}

			if (current != null) {
				bool postTransition = await current.PostTransition ();
				if (!postTransition) {
					Log.Debug ("Moving failed because panel " + current.Panel.PanelName + " cannot move.");

					return await Task.Factory.StartNew (() => false);
				}
			}

			IScreenState panel = destination [transition] ();
			bool ok = await panel.PreTransition ();
			if (ok) {
				App.Current.GUIToolkit.LoadPanel (panel.Panel);
				current = panel;
				PushState (transition);
			} else {
				Log.Debug ("Moving failed because panel " + panel.Panel.PanelName + " cannot move.");
			}

			return await Task.Factory.StartNew (() => ok);
		}

		public void Register (string transition, Func<IScreenState> panel)
		{
			if (destination.Keys.Contains (transition)) {
				if (!overwrittenTransitions.ContainsKey (transition)) {
					overwrittenTransitions [transition] = new Stack<Func<IScreenState>> ();
				}
				overwrittenTransitions [transition].Push (panel);
			}

			destination [transition] = panel;
		}

		public bool UnRegister (string transition)
		{
			// Remove from transitions
			bool ok = destination.Remove (transition);

			// Recover the previous one if exist, and remove from transitionsStack
			if (overwrittenTransitions.ContainsKey (transition) && overwrittenTransitions [transition].Any ()) {
				destination [transition] = overwrittenTransitions [transition].Pop ();
			}

			return ok;
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
