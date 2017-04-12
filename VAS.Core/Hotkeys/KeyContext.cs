using System;
using System.Collections.Generic;

namespace VAS.Core.Hotkeys
{
	/// <summary>
	/// Object to store all KeyActions in a Context
	/// </summary>
	public class KeyContext
	{
		/// <summary>
		/// Gets the KeyActions
		/// </summary>
		/// <value>The key actions.</value>
		public List<KeyAction> KeyActions {
			get;
		}

		public KeyContext ()
		{
			KeyActions = new List<KeyAction> ();
		}

		/// <summary>
		/// Adds a KeyAction
		/// </summary>
		/// <param name="keyAction">Key action.</param>
		public void AddAction (KeyAction keyAction)
		{
			if (!KeyActions.Contains (keyAction)) {
				KeyActions.Add (keyAction);
				KeyActions.Sort ();
			}
		}

		/// <summary>
		/// Removes a KeyAction
		/// </summary>
		/// <param name="keyAction">Key action.</param>
		public void RemoveAction (KeyAction keyAction)
		{
			KeyActions.RemoveAll (x => x == keyAction);
			KeyActions.Sort ();
		}
	}
}