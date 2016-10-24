//
//  Copyright (C) 2016 Fluendo S.A.
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using VAS.Core.Interfaces;
using VAS.Core.MVVMC;

namespace VAS.Core.Filters
{
	/// <summary>
	/// This is a simple predicate.
	/// Contains a settable function that receives a T object, and returns a boolean.
	/// If it is not set, it will return true.
	/// </summary>
	public class Predicate<T> : IPredicate<T>
	{
		#region IPredicate implementation

		public string Name {
			get;
			set;
		}

		public Expression<Func<T, bool>> Expression {
			get;
			set;
		} = (a) => true;

		public Func<T, bool> Filter {
			get {
				return Expression.Compile ();
			}
		}

		#endregion
	}

	/// <summary>
	/// Composite predicate. It contains other predicates
	/// </summary>
	public abstract class CompositePredicate<T> : BindableBase, IPredicate<T>, IList<IPredicate<T>>
	{
		public CompositePredicate ()
		{
			Elements.CollectionChanged += (sender, e) => {
				CollectionChanged (sender, e);
			};
		}

		public ObservableCollection<IPredicate<T>> Elements { get; set; } = new ObservableCollection<IPredicate<T>> ();

		#region IPredicate implementation

		public string Name {
			get;
			set;
		}

		public Func<T, bool> Filter {
			get {
				return Expression.Compile ();
			}
		}

		public abstract Expression<Func<T, bool>> Expression { get; }

		#endregion

		#region IList implementation

		public int IndexOf (IPredicate<T> item)
		{
			return Elements.IndexOf (item);
		}

		public void Insert (int index, IPredicate<T> item)
		{
			Elements.Insert (index, item);
		}

		public void RemoveAt (int index)
		{
			Elements.RemoveAt (index);
		}

		public IPredicate<T> this [int index] {
			get {
				return Elements [index];
			}
			set {
				Elements [index] = value;
			}
		}

		public void Add (IPredicate<T> item)
		{
			Elements.Add (item);
		}

		public void Clear ()
		{
			Elements.Clear ();
		}

		public bool Contains (IPredicate<T> item)
		{
			return Elements.Contains (item);
		}

		public void CopyTo (IPredicate<T> [] array, int arrayIndex)
		{
			Elements.CopyTo (array, arrayIndex);
		}

		public bool Remove (IPredicate<T> item)
		{
			return Elements.Remove (item);
		}

		public int Count {
			get {
				return Elements.Count;
			}
		}

		public bool IsReadOnly {
			get {
				return false;
			}
		}

		public bool IsVisible (T val)
		{
			return Filter.Invoke (val);
		}

		public IEnumerator<IPredicate<T>> GetEnumerator ()
		{
			return Elements.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return Elements.GetEnumerator ();
		}

		#endregion
	}

	/// <summary>
	/// This is a composite predicate.
	/// This predicate applies the OR operation to all the predicates it contains.
	/// </summary>
	public class OrPredicate<T> : CompositePredicate<T>
	{

		public override Expression<Func<T, bool>> Expression {
			get {
				Expression<Func<T, bool>> predicate = PredicateBuilder.True<T> ();
				foreach (var el in Elements) {
					predicate.Or (el.Expression);
				}
				return predicate;
			}
		}
	}

	/// <summary>
	/// This is a composite predicate.
	/// This predicate applies the AND operation to all the predicates it contains.
	/// </summary>
	public class AndPredicate<T> : CompositePredicate<T>
	{
		public override Expression<Func<T, bool>> Expression {
			get {
				Expression<Func<T, bool>> predicate = PredicateBuilder.True<T> ();
				foreach (var el in Elements) {
					predicate.Or (el.Expression);
				}
				return predicate;
			}
		}
	}

	public static class PredicateBuilder
	{
		public static Expression<Func<T, bool>> True<T> () { return f => true; }
		public static Expression<Func<T, bool>> False<T> () { return f => false; }

		public static Expression<Func<T, bool>> Or<T> (this Expression<Func<T, bool>> expr1,
															Expression<Func<T, bool>> expr2)
		{
			var invokedExpr = Expression.Invoke (expr2, expr1.Parameters.Cast<Expression> ());
			return Expression.Lambda<Func<T, bool>>
				  (Expression.OrElse (expr1.Body, invokedExpr), expr1.Parameters);
		}

		public static Expression<Func<T, bool>> And<T> (this Expression<Func<T, bool>> expr1,
															 Expression<Func<T, bool>> expr2)
		{
			var invokedExpr = Expression.Invoke (expr2, expr1.Parameters.Cast<Expression> ());
			return Expression.Lambda<Func<T, bool>>
				  (Expression.AndAlso (expr1.Body, invokedExpr), expr1.Parameters);
		}
	}
}

