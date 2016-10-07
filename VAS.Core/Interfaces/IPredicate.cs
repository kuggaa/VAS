//
//  Copyright (C) 2016 Fluendo S.A.
using System;

namespace VAS.Core.Interfaces
{
	/// <summary>
	/// Generic interface for predicates.
	/// Intended as base interface for Composite pattern.
	/// </summary>
	public interface IPredicate<T>
	{
		/// <summary>
		/// Filter is a function that receives an object of type T and returns a boolean.
		/// </summary>
		Func<T, bool> Filter{ get; }

		/// <summary>
		/// Gets or sets the name. Used to show this predicate in the UI.
		/// </summary>
		/// <value>The name.</value>
		string Name { get; set; }
	}
}

