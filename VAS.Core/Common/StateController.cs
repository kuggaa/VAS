using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;

namespace VAS.Core
{
	/// <summary>
	/// State controller. Class that performs all the application Navigation.
	/// It works with transition names and Screen states. It then calls the
	/// corresponding GUI toolkit to replace panels in the MainWindow and to
	/// create modal windows
	/// </summary>
	public class StateController : IStateController
	{
		Dictionary<string, Func<IScreenState>> destination;
		NavigationState home;
		List<NavigationState> navigationStateStack;
		List<NavigationState> modalStateStack;
		Dictionary<string, Stack<Func<IScreenState>>> overwrittenTransitions;
		Dictionary<string, Command> transitionCommands;

		public StateController ()
		{
			destination = new Dictionary<string, Func<IScreenState>> ();
			navigationStateStack = new List<NavigationState> ();
			modalStateStack = new List<NavigationState> ();
			overwrittenTransitions = new Dictionary<string, Stack<Func<IScreenState>>> ();
			transitionCommands = new Dictionary<string, Command> ();
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
				if (!await homeState.LoadState (properties)) {
					return false;
				}
				home = new NavigationState (transition, homeState);
				return await MoveToHome (true);
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
		public async Task<bool> MoveTo (string transition, dynamic properties, bool emptyStack = false, bool forceMove = false)
		{
			Log.Debug ("Moving to " + transition);

			if (!destination.ContainsKey (transition)) {
				Log.Debug ("Moving failed because transition " + transition + " is not in destination dictionary.");
				return false;
			}

			try {
				bool isModal = false;
				NavigationState lastState = LastState (out isModal);
				if (!forceMove && lastState != null && lastState.Name == transition) {
					Log.Debug ("Not moved to " + transition + "because we're already there");
					return true;
				}
				if (!CanMove (lastState) && !forceMove) return false;
				if (emptyStack) {
					if (!await EmptyStateStack ()) {
						return false;
					}
					if (lastState == home && home.Name != transition) {
						if (!await lastState.Hide ()) {
							Log.Debug ("Moving failed because home panel " + lastState.Name + " cannot move.");
							return false;
						}
					}
				} else if (isModal) {
					if (!await PopAllModalStates ()) {
						return false;
					}
				} else if (lastState != null) {
					if (!await lastState.Hide ()) {
						Log.Debug ("Moving failed because panel " + lastState.Name + " cannot move.");
						return false;
					}
				}

				IScreenState state;
				bool isHome = transition == home?.Name;
				if (isHome) {
					state = home.ScreenState;
				} else {
					state = destination [transition] ();
					if (!await state.LoadState (properties)) {
						// If the transition failed and the stack is empty, load the home,
						// otherwise show again the last state that was hidden at the start of the
						// MoveTo
						if (emptyStack) {
							await PushNavigationState (home.Name, home.ScreenState);
						} else {
							if (!await lastState.Show ()) {
								// This shouldn't fail... but just in case
								Log.Error ("Last state couldn't be shown again, we'll move back home");
								await PushNavigationState (home.Name, home.ScreenState);
							}
						}
						return false;
					}
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
		public async Task<bool> MoveToModal (string transition, dynamic properties, bool waitUntilClose = false)
		{
			Log.Debug ("Moving to " + transition + " in modal mode");

			if (!destination.ContainsKey (transition)) {
				Log.Debug ("Moving failed because transition " + transition + " is not in destination dictionary.");
				return false;
			}

			try {
				IScreenState state = destination [transition] ();
				NavigationState transitionState = new NavigationState (transition, state, !waitUntilClose);

				NavigationState lastState = LastState ();
				if (!CanMove (lastState)) return false;

				if (!await lastState.Freeze (transitionState)) {
					return false;
				}

				bool ok = await state.LoadState (properties);
				if (ok) {
					await PushModalState (transitionState, LastState ()?.ScreenState);
					NavigationState resultantState = LastState ();
					if (waitUntilClose && resultantState.Name == transition) {
						await resultantState.Completion.Task;
					}
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
				bool triggeredFromModal;
				NavigationState current = LastState (out triggeredFromModal);
				if (!CanMove (current)) return false;
				Log.Debug ("Moving Back");
				if (current != null) {
					if (!triggeredFromModal) {
						if (!await PopNavigationState ()) {
							return false;
						}
					} else {
						if (!await PopModalState (current)) {
							return false;
						}
					}

					current = LastState ();
					if (triggeredFromModal) {
						return await current.Unfreeze ();
					} else {
						return await current.Show ();
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
				if (!CanMove (state)) return false;

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
		public async Task<bool> MoveToHome (bool forceMove = false)
		{
			if (home == null) {
				Log.Debug ("Moving failed because transition to home doesn't exist");
				return false;
			}

			try {
				return await MoveTo (home.Name, null, true, forceMove);
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
		/// Register the specified transition and panel Initialization function with a param name="toolbar"
		/// </summary>
		/// <param name="transition">Transition.</param>
		/// <param name="toolbar">Toolbar Information</param>
		/// <param name="panel">Panel.</param>
		public void Register (string transition, Command command, Func<IScreenState> panel)
		{
			Register (transition, panel);
			transitionCommands [transition] = command;
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

			if (transitionCommands.ContainsKey (transition)) {
				transitionCommands.Remove (transition);
			}

			return ok;
		}

		public Dictionary<string, Command> GetTransitionCommands ()
		{
			return transitionCommands;
		}

		// FIXME: Enqueue instead of blocking navigation
		bool CanMove (NavigationState current)
		{
			bool canMove = current == null ||
				(current.CurrentStatus == NavigationStateStatus.Shown ||
				 current.CurrentStatus == NavigationStateStatus.Hidden);
			if (canMove) App.Current.EventsBroker.Publish<NavigatingEvent> ();
			return canMove;
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
				if (!await navigationState.Hide ()) {
					return false;
				}
				if (!await navigationState.Unload ()) {
					return false;
				}
				navigationState.ScreenState.Dispose ();
				navigationStateStack.Remove (navigationState);
			}
			return true;
		}

		async Task<bool> PushNavigationState (string transition, IScreenState state)
		{
			NavigationState navState;
			if (transition == home?.Name) {
				navState = home;
			} else {
				navState = new NavigationState (transition, state);
				navigationStateStack.Add (navState);
			}
			if (!await App.Current.Navigation.Push (state.Panel)) {
				return false;
			}
			if (!await navState.Show ()) {
				return false;
			}
			await App.Current.EventsBroker.Publish (new NavigationEvent { Name = transition });
			return true;
		}

		async Task<bool> PopNavigationState ()
		{
			NavigationState navigationState = navigationStateStack [navigationStateStack.Count - 1];
			IScreenState screenToPop = navigationState.ScreenState;
			navigationStateStack.RemoveAt (navigationStateStack.Count - 1);

			NavigationState lastState = LastNavigationState ();
			if (!await screenToPop.HideState ()) {
				return false;
			}
			if (!await App.Current.Navigation.Pop (lastState?.ScreenState.Panel)) {
				return false;
			}
			await App.Current.EventsBroker.Publish (new NavigationEvent { Name = Current });
			if (!await navigationState.Unload ()) {
				return false;
			}
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
				if (!await navigationStateStack [i].Hide ()) {
					return false;
				}
				if (!await navigationStateStack [i].Unload ()) {
					return false;
				}
				navigationStateStack [i].ScreenState.Dispose ();
				navigationStateStack.RemoveAt (i);
				if (!await App.Current.Navigation.Pop (null)) {
					return false;
				}
			}
			if (!await state.Show ()) {
				return false;
			}
			await App.Current.EventsBroker.Publish (new NavigationEvent { Name = state.Name });
			return await App.Current.Navigation.Push (state.ScreenState.Panel);
		}

		async Task<bool> PushModalState (NavigationState state, IScreenState current)
		{
			modalStateStack.Add (state);
			if (!await state.Show ()) {
				return false;
			}

			await App.Current.EventsBroker.Publish (new NavigationEvent { Name = state.ScreenState.Name, IsModal = true });
			await App.Current.Navigation.PushModal (state.ScreenState.Panel, current.Panel);
			return true;
		}

		async Task<bool> PopModalState (NavigationState current)
		{
			NavigationState navigationState = modalStateStack [modalStateStack.Count - 1];
			IScreenState screenToPop = navigationState.ScreenState;

			if (!await screenToPop.HideState ()) {
				return false;
			}
			if (!await navigationState.Unload ()) {
				return false;
			}
			modalStateStack.RemoveAt (modalStateStack.Count - 1);
			await App.Current.EventsBroker.Publish (new NavigationEvent { Name = Current, IsModal = modalStateStack.Any () });
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

		internal NavigationState LastState ()
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

	enum NavigationStateStatus
	{
		Loaded,
		Shown,
		Frozen,
		Hidden,
		Unloaded
	}

	class NavigationState
	{
		NavigationState freezingState;
		bool completeWhenUnload;


		public NavigationState (string name, IScreenState screenState, bool completeWhenUnload = true)
		{
			Name = name;
			ScreenState = screenState;
			Completion = new TaskCompletionSource<bool> ();
			this.completeWhenUnload = completeWhenUnload;
		}

		public string Name { get; set; }

		public IScreenState ScreenState { get; set; }

		public TaskCompletionSource<bool> Completion { get; }

		public NavigationStateStatus CurrentStatus { get; private set; }

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

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public async Task<bool> Load (dynamic data)
		{
			NavigationStateStatus newStatus = NavigationStateStatus.Loaded;
			if (IsAlreadyInSameStatus (newStatus)) {
				return true;
			}

			bool result = await ScreenState.LoadState (data);
			if (result) {
				UpdateState (newStatus);
			}
			return result;
		}

		public async Task<bool> Show ()
		{
			NavigationStateStatus newStatus = NavigationStateStatus.Shown;
			if (IsAlreadyInSameStatus (newStatus)) {
				return true;
			}

			bool result = await ScreenState.ShowState ();
			if (result) {
				UpdateState (newStatus);
			}
			return result;
		}

		public async Task<bool> Unfreeze ()
		{
			NavigationStateStatus newStatus = NavigationStateStatus.Shown;
			if (IsAlreadyInSameStatus (newStatus)) {
				return true;
			}

			bool result = await ScreenState.UnfreezeState ();
			if (result) {
				UpdateState (newStatus);
			}
			if (freezingState.Completion.Task.Status != TaskStatus.RanToCompletion) {
				freezingState.Completion.SetResult (result);
			}

			freezingState = null;
			return result;
		}

		public async Task<bool> Freeze (NavigationState freezingState)
		{
			NavigationStateStatus newStatus = NavigationStateStatus.Frozen;
			if (IsAlreadyInSameStatus (newStatus)) {
				return true;
			}

			this.freezingState = freezingState;
			bool result = await ScreenState.FreezeState ();
			if (result) {
				UpdateState (newStatus);
			}
			return result;
		}

		public async Task<bool> Hide ()
		{
			NavigationStateStatus newStatus = NavigationStateStatus.Hidden;
			if (IsAlreadyInSameStatus (newStatus)) {
				return true;
			}

			bool result = await ScreenState.HideState ();
			if (result) {
				UpdateState (newStatus);
			}
			return result;
		}

		public async Task<bool> Unload ()
		{
			NavigationStateStatus newStatus = NavigationStateStatus.Unloaded;
			if (IsAlreadyInSameStatus (newStatus)) {
				return true;
			}

			bool result = await ScreenState.UnloadState ();
			if (result) {
				UpdateState (newStatus);
			}

			if (freezingState != null && freezingState.Completion.Task.Status != TaskStatus.RanToCompletion) {
				freezingState.Completion.SetResult (true);
				freezingState = null;
			}

			// FIXME: when quit is done a double home navigation is done
			// this causes that the hide/unload of the state is done twice
			// because the stack is not empty, the hide state is the one creating 
			// the second home transition
			if (Completion.Task.Status != TaskStatus.RanToCompletion && completeWhenUnload) {
				Completion.SetResult (result);
			}

			return result;
		}

		bool IsAlreadyInSameStatus (NavigationStateStatus status)
		{
			if (CurrentStatus == status) {
				Log.Warning ($"NavigationState has already to desired status {CurrentStatus}");
				return true;
			}
			return false;
		}

		void UpdateState (NavigationStateStatus status)
		{
			Log.Verbose ($"Transitioning navigation state {Name} from {CurrentStatus} to {status}");
			if (Math.Abs (status - CurrentStatus) > 1 &&
				!((CurrentStatus == NavigationStateStatus.Hidden && status == NavigationStateStatus.Shown) ||
				  (CurrentStatus == NavigationStateStatus.Shown && status == NavigationStateStatus.Hidden))) {
				Log.Warning ($"Possible inconsistent status transition from {CurrentStatus} to {status}");
			}
			CurrentStatus = status;
		}
	}
}
