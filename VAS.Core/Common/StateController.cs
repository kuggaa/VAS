using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAS.Core.Common;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;

namespace VAS.Core
{
	/// <summary>
	/// State controller. Class that performs all the application Navigation.
	/// It works with transition names and Screen states. It then calls the
	/// corresponding GUI toolkit to replace panels in the MainWindow and to
	/// create modal windows
	/// </summary>
	public class StateController
	{
		Dictionary<string, Func<IScreenState>> destination;
		Tuple<string, IScreenState> home;
		List<Tuple<string, IScreenState>> navigationStateStack;
		List<Tuple<string, IScreenState>> modalStateStack;
		Dictionary<string, Stack<Func<IScreenState>>> overwrittenTransitions;

		public StateController ()
		{
			destination = new Dictionary<string, Func<IScreenState>> ();
			navigationStateStack = new List<Tuple<string, IScreenState>> ();
			modalStateStack = new List<Tuple<string, IScreenState>> ();
			overwrittenTransitions = new Dictionary<string, Stack<Func<IScreenState>>> ();
		}

		/// <summary>
		/// Sets the home transition. Needs to be registered first.
		/// </summary>
		/// <returns>True if the home transition could be executed. False otherwise</returns>
		/// <param name="transition">Transition.</param>
		public async Task<bool> SetHomeTransition (string transition, object data)
		{
			Log.Debug ("Setting Home to " + transition);
			IScreenState homeState = destination [transition] ();
			home = new Tuple<string, IScreenState> (transition, homeState);
			bool ok = await home.Item2.PreTransition (data);
			if (ok) {
				ok = await App.Current.Navigation.LoadNavigationPanelAsync (home.Item2.Panel);
			}
			return ok;
		}

		/// <summary>
		/// Moves to a Panel inside the main window. If it has some previous modal windows
		/// It Pops them all.
		/// </summary>
		/// <returns>True if the move could be performed. False otherwise</returns>
		/// <param name="transition">Transition.</param>
		public async Task<bool> MoveTo (string transition, object data)
		{
			Log.Debug ("Moving to " + transition);

			if (!destination.ContainsKey (transition)) {
				Log.Debug ("Moving failed because transition" + transition + " is not in destination dictionary.");

				return false;
			}

			bool isModal = false;
			IScreenState lastState = LastState (out isModal);
			if (lastState != null) {
				bool postTransition = await lastState.PostTransition ();
				if (!postTransition) {
					Log.Debug ("Moving failed because panel " + lastState.Panel.PanelName + " cannot move.");
					return false;
				}
			}

			if (isModal) {
				await PopAllModalStates ();
			}

			IScreenState state = destination [transition] ();
			bool ok = await state.PreTransition (data);
			if (ok) {
				await PushNavigationState (transition, state);
			} else {
				Log.Debug ("Moving failed because panel " + state.Panel.PanelName + " cannot move.");
			}
			return ok;
		}

		/// <summary>
		/// Moves to a Modal window
		/// </summary>
		/// <returns>True if the Move could be performed. False otherwise</returns>
		/// <param name="transition">Transition.</param>
		public async Task<bool> MoveToModal (string transition, object data)
		{
			Log.Debug ("Moving to " + transition + " in modal mode");

			if (!destination.ContainsKey (transition)) {
				Log.Debug ("Moving failed because transition" + transition + " is not in destination dictionary.");
				return false;
			}

			IScreenState current;
			if ((current = LastModalState ()) != null ||
			    (current = LastNavigationState ()) != null) {
				bool postTransition = await current.PostTransition ();
				if (!postTransition) {
					Log.Debug ("Moving failed because panel " + current.Panel.PanelName + " cannot move.");
					return false;
				}
			}
			Log.Debug ("Moving to " + transition + " in modal mode");
			IScreenState state = destination [transition] ();
			bool ok = await state.PreTransition (data);
			if (ok) {
				await PushModalState (transition, state, current);
			} else {
				Log.Debug ("Moving failed because panel " + state.Panel.PanelName + " cannot move.");
			}
			return ok;
		}

		/// <summary>
		/// Moves Back to the previous transition Panel or Modal.
		/// </summary>
		/// <returns>True: If the transition could be performed. False Otherwise</returns>
		public async Task<bool> MoveBack ()
		{
			if (modalStateStack.Count == 0 && navigationStateStack.Count <= 1 && home == null) {
				Log.Debug ("Moving back failed because is last transition and there isn't any home");
				return false;
			}

			bool isModal;
			IScreenState current = LastState (out isModal);
			if (current == null || !(await current.PostTransition ())) {
				Log.Debug ("Moving failed because panel " + current.Panel.PanelName + " cannot move.");
				return false;
			}

			if (!isModal) {
				await PopNavigationState ();
			} else {
				await PopModalState (current);
			}

			Log.Debug ("Moved Back");

			return true;
		}

		/// <summary>
		/// Moves the back to previous transition. It also considers Home name transition and goes back home
		/// </summary>
		/// <returns>Ture: If transition could be performed. False otherwise</returns>
		/// <param name="transition">Transition name</param>
		public async Task<bool> MoveBackTo (string transition)
		{
			IScreenState state = GetLastScreenStateFromTransition (transition);
			if (state == null) {
				Log.Debug ("Moving failed because transition " + transition + " is not in history moves");
				return false;
			} else if (home != null && state == home.Item2) {
				return await MoveToHome ();
			} else {
				//Check for modals to delete them.
				await PopAllModalStates ();
				await PopToNavigationState (state);
				return true;
			}
		}

		/// <summary>
		/// Moves to home. It clears all Modal windows and Panels in the Main Window.
		/// </summary>
		/// <returns>True if the move to the home could be performed. False otherwise</returns>
		public async Task<bool> MoveToHome ()
		{
			if (home == null) {
				Log.Debug ("Moving failed because transition to home doesn't exist");
				return false;
			} else {
				//Check for modals to delete them.
				await PopAllModalStates ();
				navigationStateStack.Clear ();
				await App.Current.Navigation.LoadNavigationPanelAsync (home.Item2.Panel);
				return true;
			}
		}

		/// <summary>
		/// Register the specified transition and panel Initialization function.
		/// </summary>
		/// <param name="transition">Transition.</param>
		/// <param name="panel">Panel.</param>
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

		/// <summary>
		/// Removes a registered transition.
		/// </summary>
		/// <param name="transition">Transition.</param>
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
		//Empties the stacks
		public async Task EmptyStateStack ()
		{
			await PopAllModalStates ();
			navigationStateStack.Clear ();
			overwrittenTransitions.Clear ();
		}

		async Task PushNavigationState (string transition, IScreenState state)
		{
			navigationStateStack.Add (new Tuple<string, IScreenState> (transition, state));
			await App.Current.Navigation.LoadNavigationPanelAsync (state.Panel);
		}

		async Task PopNavigationState ()
		{
			navigationStateStack.RemoveAt (navigationStateStack.Count - 1);
			IScreenState lastState = LastNavigationState ();
			if (lastState != null) {
				await App.Current.Navigation.LoadNavigationPanelAsync (lastState.Panel);
			}
		}

		async Task PopToNavigationState (IScreenState state)
		{
			int index = navigationStateStack.FindIndex ((ns) => ns.Item2 == state);
			if (index == -1) {
				return;
			}
			for (int i = navigationStateStack.Count - 1; i > index; i--) {
				navigationStateStack.RemoveAt (i);
			}
			await App.Current.Navigation.LoadNavigationPanelAsync (state.Panel);
		}

		async Task PushModalState (string transition, IScreenState state, IScreenState current)
		{
			modalStateStack.Add (new Tuple<string, IScreenState> (transition, state));
			await App.Current.Navigation.LoadModalPanelAsync (state.Panel, current.Panel);
		}

		async Task PopModalState (IScreenState current)
		{
			modalStateStack.RemoveAt (modalStateStack.Count - 1);
			await App.Current.Navigation.RemoveModalWindow (current.Panel);
		}

		async Task PopAllModalStates ()
		{
			IScreenState state;
			while ((state = LastModalState ()) != null) {
				await PopModalState (state);
			}
		}

		IScreenState GetLastScreenStateFromTransition (string transition)
		{
			IScreenState retState = null;
			var tuple = navigationStateStack.FindLast ((ns) => ns.Item1 == transition);
			if (tuple != null) {
				retState = tuple.Item2;
			} else if (home.Item1 == transition) {
				retState = home.Item2;
			}
			return retState;
		}

		IScreenState LastState (out bool isModal)
		{
			IScreenState state;
			isModal = false;
			if ((state = LastModalState ()) != null) {
				isModal = true;
			} else {
				state = LastNavigationState ();
			}
			return state;
		}

		IScreenState LastNavigationState ()
		{
			if (navigationStateStack.Count > 0) {
				return navigationStateStack [navigationStateStack.Count - 1].Item2;
			} else if (home != null) {
				return home.Item2;
			} else {
				return null;
			}
		}

		IScreenState LastModalState ()
		{
			if (modalStateStack.Count > 0) {
				return modalStateStack [modalStateStack.Count - 1].Item2;
			} else {
				return null;
			}
		}
	}
}
