using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.GUI;

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
		NavigationState home;
		List<NavigationState> navigationStateStack;
		List<NavigationState> modalStateStack;
		Dictionary<string, Stack<Func<IScreenState>>> overwrittenTransitions;

		public StateController ()
		{
			destination = new Dictionary<string, Func<IScreenState>> ();
			navigationStateStack = new List<NavigationState> ();
			modalStateStack = new List<NavigationState> ();
			overwrittenTransitions = new Dictionary<string, Stack<Func<IScreenState>>> ();
		}

		public string Current {
			get {
				return LastState ()?.Name;
			}
		}

		/// <summary>
		/// Sets the home transition. Needs to be registered first.
		/// </summary>
		/// <returns>True if the home transition could be executed. False otherwise</returns>
		/// <param name="transition">Transition.</param>
		public async Task<bool> SetHomeTransition (string transition, dynamic properties)
		{
			try {
				Log.Debug ("Setting Home to " + transition);
				IScreenState homeState = destination [transition] ();
				home = new NavigationState (transition, homeState);
				return await MoveToHome ();
			} catch (Exception ex) {
				Log.Exception (ex);
				return false;
			}
		}

		/// <summary>
		/// Moves to a Panel inside the main window. If it has some previous modal windows
		/// It Pops them all.
		/// </summary>
		/// <returns>True if the move could be performed. False otherwise</returns>
		/// <param name="transition">Transition.</param>
		public async Task<bool> MoveTo (string transition, dynamic properties, bool emptyStack = false)
		{
			Log.Debug ("Moving to " + transition);

			if (!destination.ContainsKey (transition)) {
				Log.Debug ("Moving failed because transition " + transition + " is not in destination dictionary.");
				return false;
			}

			try {
				bool isModal = false;
				NavigationState lastState = LastState (out isModal);

				if (emptyStack) {
					if (!await EmptyStateStack ()) {
						return false;
					}
				} else if (isModal) {
					if (!await PopAllModalStates ()) {
						return false;
					}
				} else if (lastState != null) {
					if (!await lastState.ScreenState.PostTransition ()) {
						Log.Debug ("Moving failed because panel " + lastState.Name + " cannot move.");
						return false;
					}
				}
				IScreenState state;
				if (transition == home.Name) {
					state = home.ScreenState;
				} else {
					state = destination [transition] ();
				}

				if (!await state.PreTransition (properties)) {
					Log.Debug ("Moving failed because panel " + state.Name + " cannot move.");
				}
				return await PushNavigationState (transition, state);
			} catch (Exception ex) {
				Log.Exception (ex);
				return false;
			}
		}

		/// <summary>
		/// Moves to a Modal window
		/// </summary>
		/// <returns>True if the Move could be performed. False otherwise</returns>
		/// <param name="transition">Transition.</param>
		public async Task<bool> MoveToModal (string transition, dynamic properties)
		{
			Log.Debug ("Moving to " + transition + " in modal mode");

			if (!destination.ContainsKey (transition)) {
				Log.Debug ("Moving failed because transition " + transition + " is not in destination dictionary.");
				return false;
			}

			try {
				Log.Debug ("Moving to " + transition + " in modal mode");
				IScreenState state = destination [transition] ();
				bool ok = await state.PreTransition (properties);
				if (ok) {
					await PushModalState (transition, state, LastState ()?.ScreenState);
				} else {
					Log.Debug ("Moving failed because panel " + state.Name + " cannot move.");
				}
				return ok;
			} catch (Exception ex) {
				Log.Exception (ex);
				return false;
			}
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

			try {
				bool isModal;
				NavigationState current = LastState (out isModal);
				Log.Debug ("Moving Back");
				if (current != null) {
					if (!isModal) {
						return await PopNavigationState ();
					} else {
						return await PopModalState (current);
					}
				}
				return true;
			} catch (Exception ex) {
				Log.Exception (ex);
				return false;
			}
		}

		/// <summary>
		/// Moves the back to previous transition. It also considers Home name transition and goes back home
		/// </summary>
		/// <returns>Ture: If transition could be performed. False otherwise</returns>
		/// <param name="transition">Transition name</param>
		public async Task<bool> MoveBackTo (string transition)
		{
			try {
				NavigationState state = LastStateFromTransition (transition);
				if (state == null) {
					Log.Debug ("Moving failed because transition " + transition + " is not in history moves");
					return false;
				}

				if (home != null && state == home) {
					return await MoveToHome ();
				}

				//Check for modals to delete them.
				if (!await PopAllModalStates ()) {
					return false;
				}
				return await PopToNavigationState (state);
			} catch (Exception ex) {
				Log.Exception (ex);
				return false;
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
			}

			try {
				await MoveTo (home.Name, null, true);
				return true;
			} catch (Exception ex) {
				Log.Exception (ex);
				return false;
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
			// Delete all existing instances
			navigationStateStack.FirstOrDefault (x => x.Name == transition)?.ScreenState.Dispose ();

			// Remove from transitions
			bool ok = destination.Remove (transition);

			// Recover the previous one if exist, and remove from transitionsStack
			if (overwrittenTransitions.ContainsKey (transition) && overwrittenTransitions [transition].Any ()) {
				destination [transition] = overwrittenTransitions [transition].Pop ();
			}

			return ok;
		}

		/// <summary>
		/// Empties the state stack.
		/// </summary>
		/// <returns>The state stack.</returns>
		async Task<bool> EmptyStateStack ()
		{
			if (!await PopAllModalStates ()) {
				return false;
			}
			foreach (var navigationState in navigationStateStack.ToList ()) {
				if (!await navigationState.ScreenState.PostTransition ()) {
					return false;
				}
				navigationState.ScreenState.Dispose ();
				navigationStateStack.Remove (navigationState);
			}
			return true;
		}

		Task<bool> PushNavigationState (string transition, IScreenState state)
		{
			navigationStateStack.Add (new NavigationState (transition, state));
			App.Current.EventsBroker.Publish (new NavigationEvent { Name = transition });
			return App.Current.Navigation.Push (state.Panel);
		}

		async Task<bool> PopNavigationState ()
		{
			IScreenState screenToPop = navigationStateStack [navigationStateStack.Count - 1].ScreenState;
			navigationStateStack.RemoveAt (navigationStateStack.Count - 1);

			NavigationState lastState = LastNavigationState ();
			if (!await App.Current.Navigation.Pop (lastState?.ScreenState.Panel)) {
				return false;
			}
			App.Current.EventsBroker.Publish (new NavigationEvent { Name = Current });
			screenToPop.Dispose ();
			return true;
		}

		async Task<bool> PopToNavigationState (NavigationState state)
		{
			int index = navigationStateStack.FindIndex ((ns) => ns == state);
			if (index == -1) {
				return false;
			}
			for (int i = navigationStateStack.Count - 1; i > index; i--) {
				navigationStateStack [i].ScreenState.Dispose ();
				navigationStateStack.RemoveAt (i);
				if (!await App.Current.Navigation.Pop (null)) {
					return false;
				}
			}
			App.Current.EventsBroker.Publish (new NavigationEvent { Name = state.Name });
			return await App.Current.Navigation.Push (state.ScreenState.Panel);
		}

		Task PushModalState (string transition, IScreenState state, IScreenState current)
		{
			modalStateStack.Add (new NavigationState (transition, state));
			App.Current.EventsBroker.Publish (new NavigationEvent { Name = transition });
			return App.Current.Navigation.PushModal (state.Panel, current.Panel);
		}

		async Task<bool> PopModalState (NavigationState current)
		{
			IScreenState screenToPop = modalStateStack [modalStateStack.Count - 1].ScreenState;
			if (!await screenToPop.PostTransition ()) {
				return false;
			}
			modalStateStack.RemoveAt (modalStateStack.Count - 1);
			App.Current.EventsBroker.Publish (new NavigationEvent { Name = Current });
			await App.Current.Navigation.PopModal (screenToPop.Panel);
			screenToPop.Dispose ();
			return true;
		}

		async Task<bool> PopAllModalStates ()
		{
			NavigationState state;
			while ((state = LastModalState ()) != null) {
				if (!await PopModalState (state)) {
					return false;
				}
			}
			return true;
		}

		NavigationState LastStateFromTransition (string transition)
		{
			var tuple = navigationStateStack.FindLast ((ns) => ns.Name == transition);
			if (tuple != null) {
				return tuple;
			}
			if (home.Name == transition) {
				return home;
			}
			return null;
		}

		NavigationState LastState ()
		{
			bool isModal;
			return LastState (out isModal);
		}

		NavigationState LastState (out bool isModal)
		{
			NavigationState state;
			isModal = false;
			if ((state = LastModalState ()) != null) {
				isModal = true;
			} else {
				state = LastNavigationState ();
			}
			return state;
		}

		NavigationState LastNavigationState ()
		{
			if (navigationStateStack.Count > 0) {
				return navigationStateStack [navigationStateStack.Count - 1];
			}
			if (home != null) {
				return home;
			}
			return null;
		}

		NavigationState LastModalState ()
		{
			if (modalStateStack.Count > 0) {
				return modalStateStack [modalStateStack.Count - 1];
			}
			return null;
		}

	}

	class NavigationState
	{
		public NavigationState (string name, IScreenState screenState)
		{
			Name = name;
			ScreenState = screenState;
		}

		public string Name { get; set; }

		public IScreenState ScreenState { get; set; }

		public override bool Equals (object obj)
		{
			NavigationState navState = obj as NavigationState;

			if (navState == null) {
				return false;
			}
			return navState.Name == Name && navState.ScreenState == ScreenState;
		}

		public static bool operator == (NavigationState a, NavigationState b)
		{
			// If both are null, or both are same instance, return true.
			if (ReferenceEquals (a, b)) {
				return true;
			}

			// If one is null, but not both, return false.
			if (((object)a == null) || ((object)b == null)) {
				return false;
			}

			return a.Equals (b);
		}

		public static bool operator != (NavigationState a, NavigationState b)
		{
			return !(a == b);
		}
	}
}
