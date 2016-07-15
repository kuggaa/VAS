using System;
using VAS.Core.Store;

namespace VAS.Core.Hotkeys
{
	/// <summary>
	/// Object used to bind an Action to a Hotkey and actionName
	/// </summary>
	public class KeyAction : IComparable, IEquatable<KeyAction>
	{
		/// <summary>
		/// The GUID of the KeyAction
		/// </summary>
		public string guid;

		/// <summary>
		/// Gets or sets the name of the action
		/// </summary>
		/// <value>The name of the action.</value>
		public string ActionName { get; set; }

		/// <summary>
		/// Gets or sets the key that performs the action
		/// </summary>
		/// <value>The key.</value>
		public HotKey Key { get; set; }

		/// <summary>
		/// Gets or sets the action to perform
		/// </summary>
		/// <value>The action.</value>
		public Action Action { get; set; }

		/// <summary>
		/// Sets the KeyAction Priority
		/// Lower numbers = higher priority
		/// </summary>
		/// <value>The priority.</value>
		public int Priority { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this KeyAction continues the chain
		/// or breaks it and other KeyAction with the same ActionName doesn't execute
		/// </summary>
		/// <value><c>true</c> if continue chain; otherwise, <c>false</c>.</value>
		public bool ContinueChain { get; set; }

		public KeyAction (string actionName, HotKey key, Action action, int priority = 999, bool continueChain = true)
		{
			this.ActionName = actionName;
			this.Key = key;
			this.Action = action;
			this.Priority = priority;
			this.ContinueChain = continueChain;
			guid = Guid.NewGuid ().ToString ("N");
		}

		public bool Equals (KeyAction action)
		{
			if (action == null) {
				return false;
			}

			return this.guid == action.guid;
		}

		#region IComparable implementation

		int IComparable.CompareTo (object obj)
		{
			if (obj as KeyAction == null) {
				return 1;
			}
			return Priority - ((KeyAction)obj).Priority;
		}

		#endregion

		#region Operators

		static public bool operator == (KeyAction a, KeyAction b)
		{
			// If both are null, or both are same instance, return true.
			if (System.Object.ReferenceEquals (a, b)) {
				return true;
			}

			// If one is null, but not both, return false.
			if (((object)a == null) || ((object)b == null)) {
				return false;
			}
			return a.Equals (b);
		}

		static public bool operator != (KeyAction a, KeyAction b)
		{
			return !(a == b);
		}

		#endregion

		#region Overrides

		public override bool Equals (object obj)
		{
			if (obj is KeyAction) {
				KeyAction ka = obj as KeyAction;
				return Equals (ka);
			} else
				return false;
		}

		public override int GetHashCode ()
		{
			return guid.GetHashCode ();
		}

		#endregion
	}
}