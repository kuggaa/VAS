using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Core.Hotkeys
{
	/// <summary>
	/// Manager to Register KeyContexts, global or Current State app context
	/// Manages Actions when a Hotkey is pressed and compares it to the KeyContexts
	/// </summary>
	public sealed class KeyContextManager : DisposableBase
	{
		private static readonly KeyContextManager instance = new KeyContextManager ();
		List<KeyContext> currentKeyContexts;
		KeyContext globalKeyContext;
		ITimer contextTimer;

		private KeyContextManager ()
		{
			currentKeyContexts = new List<KeyContext> ();
			globalKeyContext = new KeyContext ();
			EnableGlobalContext = true;
			contextTimer = App.Current.DependencyRegistry.Retrieve<ITimer> (InstanceType.Default);
			contextTimer.Elapsed += OnElapsedTimer;
			contextTimer.Interval = 200;
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			contextTimer.Elapsed -= OnElapsedTimer;
			contextTimer.Dispose ();
		}

		/// <summary>
		/// Gets the instance.
		/// </summary>
		/// <value>The instance.</value>
		public static KeyContextManager Instance {
			get {
				return instance;
			}
		}

		/// <summary>
		/// Gets the global key context.
		/// </summary>
		/// <value>The global key context.</value>
		public KeyContext GlobalKeyContext {
			get {
				return globalKeyContext;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to enable global context.
		/// </summary>
		/// <value><c>true</c> if enable global context; otherwise, <c>false</c>.</value>
		public bool EnableGlobalContext {
			get;
			set;
		}

		/// <summary>
		/// Gets the current key contexts.
		/// </summary>
		/// <value>The current key contexts.</value>
		internal List<KeyContext> CurrentKeyContexts {
			get {
				return currentKeyContexts;
			}
		}

		/// <summary>
		/// Adds the context to the List of Current Contexts
		/// The Last Context Added the Higher the priority
		/// </summary>
		/// <param name="context">Context.</param>
		public void AddContext (KeyContext context)
		{
			if (context is KeyTemporalContext) {
				AddTemporalContext ((KeyTemporalContext)context);
			}

			currentKeyContexts.Add (context);
		}

		/// <summary>
		/// Removes the context from the Current Contexts
		/// </summary>
		/// <param name="context">Context.</param>
		public void RemoveContext (KeyContext context)
		{
			if (context is KeyTemporalContext && !currentKeyContexts.Any (x => x is KeyTemporalContext)) {
				contextTimer.Stop ();
			}
			
			currentKeyContexts.RemoveAll (ctx => ctx == context);
		}

		/// <summary>
		/// Replaces the Current Context Lists with a new List
		/// </summary>
		/// <param name="contexts">Contexts.</param>
		public void NewKeyContexts (List<KeyContext> contexts)
		{
			contexts.Where(x => x is KeyTemporalContext).ToList().ForEach (x => AddTemporalContext((KeyTemporalContext)x));
			currentKeyContexts = contexts;
		}

		/// <summary>
		/// Handles the key pressed.
		/// </summary>
		/// <param name="key">Key.</param>
		public void HandleKeyPressed (HotKey key)
		{
			bool handled = false;

			for (int i = currentKeyContexts.Count - 1; i >= 0; i--) {
				KeyContext context = currentKeyContexts [i];
				handled = ProcessKeyActions (context.KeyActions, key);
				if (handled) {
					if (context is KeyTemporalContext) {
						currentKeyContexts.Remove (context);
					}
					break;
				}
			}

			if (!handled && EnableGlobalContext) {
				handled = ProcessKeyActions (globalKeyContext.KeyActions, key);
			}

			if (!handled) {
				FallbackKeyPressedEvent (key);
			}
		}

		bool ProcessKeyActions (List<KeyAction> keyActions, HotKey key)
		{
			bool handled = false;

			foreach (KeyAction ka in keyActions.Where (ka => ka.KeyConfig.Key == key).OrderBy (ka => ka.Priority)) {
				ka.Action ();
				handled = true;
				if (!ka.ContinueChain) {
					break;
				}
			}
			return handled;
		}

		void FallbackKeyPressedEvent (HotKey key)
		{
			VAS.App.Current.EventsBroker.Publish<KeyPressedEvent> (
				new KeyPressedEvent {
					Key = key
				}
			);
		}

		void OnElapsedTimer (object sender, EventArgs e)
		{
			contextTimer.Stop ();
			currentKeyContexts.RemoveAll (x => CheckContextExpired(x));
			if (currentKeyContexts.Any (x => x is KeyTemporalContext)) {
				contextTimer.Start ();
			}
		}

		bool CheckContextExpired (KeyContext context)
		{
			bool expired = false;

			KeyTemporalContext tmpContext = context as KeyTemporalContext;
			if (tmpContext != null) {
				TimeSpan passedTime = DateTime.Now - tmpContext.StartedTime;
				expired =  passedTime.TotalMilliseconds >= tmpContext.Duration;
				if (expired) {
					App.Current.GUIToolkit.Invoke ((sender, e) => tmpContext.ExpiredTimeAction ());
				}
			}

			return expired;
		}

		void AddTemporalContext (KeyTemporalContext tmpContext)
		{
			tmpContext.StartedTime = DateTime.Now;
			if (!contextTimer.Enabled) {
				contextTimer.Start ();
			}
		}
	}
}