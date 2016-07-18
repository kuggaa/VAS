using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAS.Core.Interfaces.GUI;
using System.Linq;

namespace VAS.Core
{
	public class StateController
	{
		Dictionary<string, Func<IScreenState>> destination;
		IScreenState current;
		List<string> stateStack;
		List<TransitionStruct> transitionsStack;

		struct TransitionStruct
		{
			public string key;
			public Func<IScreenState> value;
		}

		public StateController ()
		{
			destination = new Dictionary<string, Func<IScreenState>> ();
			stateStack = new List<string> ();
			transitionsStack = new List<TransitionStruct> ();
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
			var a = new TransitionStruct { key = transition, value = panel };
			transitionsStack.Add (a);
		}

		public void UnRegister (string transition)
		{
			// Remove from transitions
			destination.Remove (transition);

			// Remove from transitions Stack
			int position = transitionsStack.FindLastIndex (x => x.key == transition);
			transitionsStack.RemoveAt (position);

			// Recover the previous one if exist
			if (transitionsStack.Any (x => x.key == transition)) {
				TransitionStruct last = transitionsStack.FindLast (x => x.key == transition);
				destination [transition] = last.value;
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
			transitionsStack.Clear ();
		}
	}
}

