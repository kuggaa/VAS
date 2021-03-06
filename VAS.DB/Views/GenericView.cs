﻿//
//  Copyright (C) 2015 Fluendo S.A.
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
using System.Collections.Specialized;
using System.Linq;
using Couchbase.Lite;
using Newtonsoft.Json.Linq;
using VAS.Core.Common;
using VAS.Core.Filters;
using VAS.Core.Interfaces;
using VAS.Core.Serialization;

namespace VAS.DB.Views
{
	/// <summary>
	/// Generic View for the Couchbase database that indexes properties with
	/// the <see cref="PropertyIndex"/> attribute and make it possible
	/// to perform queries using a <see cref="QueryFilter"/>.
	/// The view also stores a preloaded version of the object that is returned in the
	/// query using the properties with the attribute <see cref="PropertyPreload"/>
	/// </summary>
	public abstract class GenericView<T> : GenericView<T, T> where T : IStorable
	{
		protected GenericView (CouchbaseStorage storage) : base (storage)
		{
		}
	}

	/// <summary>
	/// Generic View for the Couchbase database that indexes properties with
	/// the <see cref="PropertyIndex"/> attribute and make it possible
	/// to perform queries using a <see cref="QueryFilter"/>.
	/// The view also stores a preloaded version of the object that is returned in the
	/// query using the properties with the attribute <see cref="PropertyPreload"/>
	/// The real type created is the one passed through the generic TReal.
	/// </summary>
	public abstract class GenericView<TBase, TReal> : IQueryView<TBase> where TBase : IStorable where TReal : TBase
	{
		readonly Database db;
		readonly CouchbaseStorage storage;

		protected GenericView (CouchbaseStorage storage)
		{
			this.storage = storage;
			db = storage.Database;

			// List all properties that will are included in the preloaded version of the object
			// returned in the queries
			PreviewProperties = typeof (TReal).GetProperties ().
				Where (prop => Attribute.IsDefined (prop, typeof (PropertyPreload))).
				Select (p => p.Name).ToList ();

			// List all properties that are indexed for the queries sorted by Index
			FilterProperties = new OrderedDictionary ();
			FilterProperties.Add (DocumentsSerializer.PARENT_PROPNAME, true);
			foreach (var prop in typeof (TReal).GetProperties ().
				Select (p => new { P = p, A = p.GetCustomAttributes (typeof (PropertyIndex), true) }).
				Where (x => x.A.Length == 1).
				OrderBy (x => (x.A [0] as PropertyIndex).Index)) {
				FilterProperties.Add (prop.P.Name, typeof (IStorable).IsAssignableFrom (prop.P.PropertyType));
			}
			DocumentType = typeof (TBase).Name;
		}

		/// <summary>
		/// Return the object type
		/// </summary>
		/// <value>The type.</value>
		public Type Type {
			get {
				return typeof (TBase);
			}
		}

		/// <summary>
		/// The document type associated with this view.
		/// </summary>
		/// <value>The type of the document.</value>
		public string DocumentType {
			get;
			protected set;
		}

		/// <summary>
		/// An ordered dictionary that store as keys the name of the property
		/// and as value a <c>boolean</c> indicating if the property returns an
		/// <see cref="IStorable"/>.
		/// </summary>
		protected virtual OrderedDictionary FilterProperties {
			get;
			private set;
		}

		/// <summary>
		/// A list with the names of the properties that are included in the preloaded
		/// version of the object.
		/// </summary>
		protected virtual List<string> PreviewProperties {
			get;
			private set;
		}

		/// <summary>
		/// The version of the view. It needs to be changed each time the Map function changes
		/// to re-index the view when this function changes.
		/// </summary>
		abstract protected string ViewVersion {
			get;
		}

		/// <summary>
		/// Creates a list of values to be indexed for the queries as a <see cref="PropertyKey"/>
		/// </summary>
		/// <returns>The key to emit in the map function.</returns>
		/// <param name="document">The database document.</param>
		virtual protected object GenKeys (IDictionary<string, object> document)
		{
			List<object> keys;

			if (FilterProperties.Count == 0)
				return null;

			keys = new List<object> ();
			foreach (string propName in FilterProperties.Keys) {
				object value;

				if (!document.TryGetValue (propName, out value)) {
					keys.Add (null);
					continue;
				}
				// If the property is an IStorable, store the object ID which will be used in the queries
				if ((bool)FilterProperties [propName]) {
					keys.Add (DocumentsSerializer.IDStringFromString (value as string));
				} else {
					keys.Add (value);
				}
			}
			return new PropertyKey (keys);
		}

		/// <summary>
		/// Return a serialized string with the preloaded version of the object using the properties in
		/// <see cref="PreviewProperties"/>
		/// </summary>
		/// <returns>A JSON string representation of  the object.</returns>
		/// <param name="document">The database document.</param>
		virtual protected string GenValue (IDictionary<string, object> document)
		{
			JObject jo;

			if (FilterProperties.Count == 0)
				return null;

			jo = new JObject ();
			foreach (string propName in PreviewProperties) {
				object obj;
				if (document.TryGetValue (propName, out obj)) {
					if (obj is JObject) {
						jo [propName] = obj as JObject;
					} else {
						jo [propName] = new JValue (obj);
					}
				}
			}
			return jo.ToString ();
		}

		/// <summary>
		/// Gets the map function to be run in this view.
		/// </summary>
		/// <returns>The map function.</returns>
		/// <param name="docType">Document type.</param>
		virtual protected MapDelegate GetMap (string docType)
		{
			return (document, emitter) => {
				if (docType.Equals (document [DocumentsSerializer.DOC_TYPE])) {
					emitter (GenKeys (document), GenValue (document));
				}
			};
		}

		/// <summary>
		/// Creates a new view in the database if it does not exists and it sets the map funcion on it.
		/// </summary>
		/// <returns>The view.</returns>
		View GetView ()
		{
			View view = db.GetView (DocumentType);
			if (view.Map == null) {
				view.SetMap (GetMap (DocumentType), ViewVersion);
			}
			return view;
		}

		/// <summary>
		/// Performs a query on the view with a <see cref="QueryFilter"/> whose keys
		/// must be in the list of <see cref="FilterProperties"/> returning a pre-loaded object
		/// </summary>
		/// <param name="filter">Filter.</param>
		public IEnumerable<TBase> Query (QueryFilter filter)
		{
			return Query (filter, null, false);
		}

		/// <summary>
		/// Performs a query on the view with a <see cref="QueryFilter"/> whose keys
		/// must be in the list of <see cref="FilterProperties"/> returning the full object.
		/// </summary>
		/// <param name="filter">Filter.</param>
		public IEnumerable<TBase> QueryFull (QueryFilter filter, IStorableObjectsCache cache)
		{
			return Query (filter, cache, true);
		}

		/// <summary>
		/// Performs a Count on the view with a <see cref="QueryFilter"/> whose keys
		/// must be in the list of <see cref="FilterProperties"/> returning the count.
		/// </summary>
		/// <param name="filter">Filter.</param>
		public int Count (QueryFilter filter)
		{
			View view = GetView ();
			view.SetMapReduce (GetMap (DocumentType), (keys, values, rereduce) => {
				return values.ToList ().Count;
			}, ViewVersion);

			Query q = view.CreateQuery ();
			q.MapOnly = false;

			q.SQLSearch = QueryFilterToSql (filter);
			QueryEnumerator ret = q.Run ();
			int count = 0;
			if (ret.Any ()) {
				count = Convert.ToInt32 (ret.First ().Value);
			}

			view.SetMap (GetMap (DocumentType), ViewVersion);
			return count;
		}

		string KeyToKeyIndex (string key)
		{
			int index = 0;
			foreach (string dictKey in FilterProperties.Keys) {
				if (dictKey == key) {
					if (index == 0) {
						return "key";
					} else {
						return "key" + index;
					}
				}
				index++;
			}
			return null;
		}

		internal string QueryFilterToSql (QueryFilter filter)
		{
			List<string> filters = new List<string> ();

			if (filter == null || filter.Count == 0 || FilterProperties.Count == 0) {
				return null;
			}

			foreach (var kv in filter) {
				string key = kv.Key;
				List<object> values = kv.Value;

				string keyIndex = KeyToKeyIndex (key);
				if (keyIndex == null) {
					throw new InvalidQueryException (String.Format ("Key {0} not found", key));
				}

				values = ConvertValues (values);
				if (values.Count == 1) {
					filters.Add (String.Format ("{0}='{1}'", keyIndex, values [0]));
				} else {
					string vals = String.Join (" , ", values.Select (x => "'" + x + "'"));
					filters.Add (String.Format ("{0} IN ({1})", keyIndex, vals));
				}
			}

			foreach (QueryFilter childFilter in filter.Children) {
				string sql = QueryFilterToSql (childFilter);
				if (sql != null) {
					filters.Add (String.Format ("( {0} )", sql));
				}
			}

			string oper = filter.Operator == QueryOperator.And ? " AND " : " OR ";
			return String.Join (oper, filters);
		}

		IEnumerable<TBase> Query (QueryFilter filter, IStorableObjectsCache cache = null, bool full = false)
		{
			SerializationContext context = null;
			HashSet<Guid> uids = new HashSet<Guid> ();
			View view = GetView ();

			Query q = view.CreateQuery ();
			q.SQLSearch = QueryFilterToSql (filter);

			QueryEnumerator ret = q.Run ();
			if (full) {
				context = new SerializationContext (storage.Database, typeof (TBase));
				if (cache != null) {
					context.Cache = cache;
				}
			}

			foreach (QueryRow row in ret) {
				Revision rev = row.Document.CurrentRevision;
				Guid id = DocumentsSerializer.IDFromString (row.DocumentId);

				// If we don't have a filter assume we want to remove duplicates by ID which is the default
				if ((filter == null || filter.RemoveDuplicatesByID) && uids.Contains (id)) {
					continue;
				}

				TReal doc = default (TReal);
				bool noErrors = false;

				uids.Add (id);
				try {
					if (full) {
						doc = (TReal)DocumentsSerializer.LoadObject (typeof (TReal), row.DocumentId,
							context.DB, context);
					} else {
						doc = DocumentsSerializer.DeserializeFromJson<TReal> (row.Value as string, db, rev);
						doc.DocumentID = row.DocumentId;
						doc.ID = id;
						doc.IsChanged = false;
						doc.IsLoaded = false;
					}
					doc.Storage = storage;
					noErrors = true;
				} catch (Exception ex) {
					Log.Error ("Error deserializing document of type " + typeof (TReal) + " with ID: " + row.DocumentId);
					Log.Exception (ex);
				}
				if (noErrors) {
					yield return doc;
				}
			}
		}

		/// <summary>
		/// Converts IStorables into ID's for the query since they are indexed with their ID.
		/// </summary>
		/// <returns>The values from the filter.</returns>
		/// <param name="values">The values with IStorable objects converted to ID's strings.</param>
		List<object> ConvertValues (List<object> values)
		{
			List<object> ret = new List<object> ();
			foreach (object val in values) {
				if (val == null) {
					ret.Add ("null");
				} else {
					IStorable storable = val as IStorable;
					if (storable != null) {
						ret.Add ("\"" + storable.ID + "\"");
					} else {
						ret.Add ("\"" + val + "\"");
					}
				}
			}
			return ret;
		}
	}
}

