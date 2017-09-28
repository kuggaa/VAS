//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using VAS.Core.Events;
using VAS.Core.Store;

namespace VAS.Core.Common
{
	public static class ExtensionMethods
	{
		public static void Swap<T> (this IList<T> list, T e1, T e2)
		{
			int index1, index2;

			index1 = list.IndexOf (e1);
			index2 = list.IndexOf (e2);
			T temp = list [index1];
			list [index1] = list [index2];
			list [index2] = temp;
		}

		public static T [] Merge<T> (this List<T []> list)
		{
			var res = new List<T> ();

			foreach (T [] t in list) {
				res.AddRange (t);
			}
			return res.ToArray ();
		}

		public static TKey GetKeyByValue<TKey, TValue> (this Dictionary<TKey, TValue> dict, TValue value)
		{
			return dict.SingleOrDefault (x => x.Value.Equals (value)).Key;
		}

		public static void RemoveKeysByValue<TKey, TValue> (this Dictionary<TKey, TValue> dict, TValue value)
		{
			foreach (var item in dict.Where (k => k.Value.Equals (value)).ToList ()) {
				try {
					dict.Remove (item.Key);
				} catch {
				}
			}
		}

		public static bool SequenceEqualSafe<T> (this IList<T> first, IList<T> second)
		{
			if (first == null && second == null) {
				return true;
			} else if (first == null || second == null) {
				return false;
			} else {
				return first.SequenceEqual (second);
			}
		}

		public static bool SequenceEqualNoOrder<T> (this IEnumerable<T> first, IEnumerable<T> second)
		{
			if (first == null && second == null) {
				return true;
			} else if (first == null || second == null) {
				return false;
			} else {
				return (!first.Except (second).Any () && !second.Except (first).Any ());
			}
		}

		public static int RemoveAll<T> (this ObservableCollection<T> coll, Func<T, bool> condition)
		{
			var itemsToRemove = coll.Where (condition).ToList ();
			foreach (var itemToRemove in itemsToRemove) {
				coll.Remove (itemToRemove);
			}
			return itemsToRemove.Count;
		}

		public static void AddRange<T> (this ObservableCollection<T> coll, IEnumerable<T> range)
		{
			foreach (var item in range) {
				coll.Add (item);
			}
		}

		public static T Clamp<T> (this T val, T min, T max) where T : IComparable<T>
		{
			if (val.CompareTo (min) < 0) {
				return min;
			} else if (val.CompareTo (max) > 0) {
				return max;
			} else {
				return val;
			}
		}

		public static void PublishEvent<TEvent> (this object sender, TEvent evt)
			where TEvent : Event
		{
			evt.Sender = sender;
			App.Current.EventsBroker.Publish (evt);
		}

		/// <summary>
		/// Copies all the properties with same name from source to destination. 
		/// Only do the copy if both Getter in source property and Setter in destination are public.
		/// </summary>
		/// <param name="destObject">Destination object.</param>
		/// <param name="srcObject">Source object.</param>
		public static void CopyPropertiesFrom (this object destObject, object srcObject)
		{
			Type srcType = srcObject.GetType ();
			foreach (var srcPropertyInfo in srcType.GetProperties ()) {
				CopyProperty (destObject, srcObject, srcPropertyInfo);
			}
		}

		/// <summary>
		/// Copies specified property with same name from source to destination.
		/// Only do the copy if both Getter in source property and Setter in destination are public.
		/// </summary>
		/// <param name="destObject">Destination object.</param>
		/// <param name="srcObject">Source object.</param>
		/// <param name="propertyName">Property name.</param>
		public static void CopyPropertyFrom (this object destObject, object srcObject, string propertyName)
		{
			Type srcType = srcObject.GetType ();
			var srcPropertyInfo = srcType.GetProperties ().FirstOrDefault (p => p.Name == propertyName);
			if (srcPropertyInfo != null) {
				CopyProperty (destObject, srcObject, srcPropertyInfo);
			}
		}

		static void CopyProperty (object destObject, object srcObject, PropertyInfo property)
		{
			Type destType = destObject.GetType ();
			if (destType.GetProperties ().Any (d => d.Name == property.Name)) {
				object srcValue = property.GetGetMethod ()?.Invoke (srcObject, null);
				if (srcValue == null) {
					return;
				}
				PropertyInfo destProperty = destType.GetProperties ().First<PropertyInfo> (p => p.Name == property.Name);
				if (destProperty == null) {
					return;
				}
				object srcConvertedVal = null;
				if (srcValue is DateTime && destProperty.PropertyType.IsAssignableFrom (typeof (long))) {
					srcConvertedVal = ((DateTime)srcValue).ToUnixTime ();
				} else if (!destProperty.PropertyType.IsAssignableFrom (property.PropertyType)) {
					srcConvertedVal = Convert.ChangeType (srcValue, destProperty.PropertyType);
				} else {
					srcConvertedVal = srcValue;
				}
				if (srcConvertedVal == null) {
					return;
				}
				destProperty.GetSetMethod ()?.Invoke (
					destObject,
					new [] { srcConvertedVal }
				);
			}
		}

		/// <summary>
		/// Publishes a new event with sender set.
		/// </summary>
		/// <param name="sourceObject">Source object. This is the Sender.</param>
		/// <typeparam name="T">The Event type.</typeparam>
		public static void PublishNewEvent<T> (this object sourceObject) where T : Event, new()
		{
			App.Current.EventsBroker.Publish<T> (new T { Sender = sourceObject });
		}

		/// <summary>
		/// Convert datime to unix time.
		/// </summary>
		/// <returns>The unix time.</returns>
		/// <param name="date">Date.</param>
		public static long ToUnixTime (this DateTime date)
		{
			var epoch = new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return Convert.ToInt64 ((date.ToUniversalTime () - epoch).TotalSeconds);
		}

		/// <summary>
		/// Sorts the IEnumerable<StorableBase> elements by creation date.
		/// </summary>
		/// <returns>A sorted IEnumerable<StorableBase>.</returns>
		/// <param name="source">Source.</param>
		/// <param name="descending">If set to <c>true</c> sorting is descending, else ascending.</param>
		public static IEnumerable<T> SortByCreationDate<T> (this IEnumerable<T> source, bool descending)
			where T : StorableBase
		{
			return source.Sort (s => s.CreationDate, descending);
		}

		/// <summary>
		/// Sort the specified source using a keySelector and ordering flag.
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="keySelector">Key selector.</param>
		/// <param name="descending">If set to <c>true</c> descending, else ascending.</param>
		/// <typeparam name="TSource">The 1st type parameter.</typeparam>
		/// <typeparam name="TKey">The 2nd type parameter.</typeparam>
		public static IEnumerable<TSource> Sort<TSource, TKey>
			(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, bool descending)
		{
			return descending ? source.OrderByDescending (keySelector) : source.OrderBy (keySelector);
		}

		/// <summary>
		/// Converts an item into an enumerable consisting in this single item.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static IEnumerable<T> ToEnumerable<T> (this T item)
		{
			yield return item;
		}

		/// <summary>
		/// Compare 2 dynamic objects.
		/// </summary>
		/// <returns><c>true</c> if both objects are equal</returns>
		/// <param name="obj1">The first object to compare.</param>
		/// <param name="obj2">The second object to compare.</param>
		public static bool Compare (this ExpandoObject obj1, ExpandoObject obj2)
		{
			var obj1Coll = (ICollection<KeyValuePair<string, object>>)obj1;
			var obj2Dict = (IDictionary<string, object>)obj2;

			if (obj1Coll.Count != obj2Dict.Count)
				return false;

			foreach (var pair in obj1Coll) {
				object o;
				if (!obj2Dict.TryGetValue (pair.Key, out o))
					return false;

				if (!Equals (o, pair.Value))
					return false;
			}
			return true;
		}
	}
}
