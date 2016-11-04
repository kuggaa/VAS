


using System;

namespace Prism.Events
{
	/// <summary>
	/// Represents a reference to a <see cref="Delegate"/>.
	/// </summary>
	internal interface IDelegateReference
	{
		/// <summary>
		/// Gets the referenced <see cref="Delegate" /> object.
		/// </summary>
		/// <value>A <see cref="Delegate"/> instance if the target is valid; otherwise <see langword="null"/>.</value>
		Delegate Target { get; }
	}
}