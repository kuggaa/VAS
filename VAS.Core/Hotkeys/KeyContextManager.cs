using System;
using System.Collections.Generic;
using System.Timers;
using VAS.Core.Events;
using VAS.Core.Store;

namespace VAS.Core.Hotkeys
{
	/// <summary>
	/// Manager to Register KeyContexts, global or Current State app context
	/// Manages Actions when a Hotkey is pressed and compares it to the KeyContexts
	/// </summary>
	public sealed class KeyContextManager
	{
		private static readonly KeyContextManager instance = new KeyContextManager ();
		List<KeyContext> currentKeyContexts;
		KeyContext globalKeyContext;

		private KeyContextManager ()
		{
			currentKeyContexts = new List<KeyContext> ();
			globalKeyContext = new KeyContext ();
			EnableGlobalContext = true;
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
		public List<KeyContext> CurrentKeyContexts {
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
			currentKeyContexts.Add (context);
		}

		/// <summary>
		/// Removes the context from the Current Contexts
		/// </summary>
		/// <param name="context">Context.</param>
		public void RemoveContext (KeyContext context)
		{
			currentKeyContexts.RemoveAll (ctx => ctx == context);
		}

		/// <summary>
		/// Replaces the Current Context Lists with a new List
		/// </summary>
		/// <param name="contexts">Contexts.</param>
		public void NewKeyContexts (List<KeyContext> contexts)
		{
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
				foreach (KeyAction ka in currentKeyContexts[i].KeyActions) {
					if (ka.Key == key) {
						ka.Action ();
						handled = true;
						if (!ka.ContinueChain) {
							break;
						}
					}
				}
				if (handled) {
					break;
				}
			}

			if (!handled && EnableGlobalContext) {
				foreach (KeyAction ka in globalKeyContext.KeyActions) {
					if (ka.Key == key) {
						ka.Action ();
						handled = true;
						if (!ka.ContinueChain) {
							break;
						}
					}
				}
			}

			if (!handled) {
				FallbackKeyPressedEvent (key);
			}
		}

		void FallbackKeyPressedEvent (HotKey key)
		{
			VAS.App.Current.EventsBroker.Publish<KeyPressedEvent> (
				new KeyPressedEvent {
					Key = key
				}
			);
		}
	}
}