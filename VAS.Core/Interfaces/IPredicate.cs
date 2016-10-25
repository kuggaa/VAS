//
//  Copyright (C) 2016 Fluendo S.A.
using System;
using System.Linq.Expressions;

namespace VAS.Core.Interfaces
{
	/// <summary>
	/// Generic interface for predicates.
	/// Intended as base interface for Composite pattern.
	/// </summary>
	public interface IPredicate<T>
	{
		/// <summary>
		/// Expression that will filter when called
		/// </summary>
		/// <value>The expression.</value>
		Expression<Func<T, bool>> Expression { get; }

		/// <summary>
		/// Gets or sets the name. Used to show this predicate in the UI.
		/// </summary>
		/// <value>The name.</value>
		string Name { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.Interfaces.IPredicate`1"/> is active.
		/// </summary>
		/// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
		bool Active { get; set; }

		/// <summary>
		/// Filter is a function that receives an object of type T and returns a boolean.
		/// </summary>
		bool Filter (T obj);

	}
}

