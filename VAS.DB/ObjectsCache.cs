//
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
using System.Globalization;
using VAS.Core.Interfaces;

namespace VAS.DB
{
	public abstract class ObjectsCache<TId, TObject>
	{
		protected readonly Dictionary<TId, TObject> idToObjects;
		protected readonly Dictionary<TObject, TId> objectsToId;

		public ObjectsCache ()
		{
			idToObjects = new Dictionary<TId, TObject> ();
			objectsToId = new Dictionary<TObject, TId> ();
		}

		public bool IsCached (TId id)
		{
			return idToObjects.ContainsKey (id);
		}

		public bool IsCached (TObject obj)
		{
			return objectsToId.ContainsKey (obj);
		}

		public TObject ResolveReference (TId id)
		{
			TObject p;
			idToObjects.TryGetValue (id, out p);
			return p;
		}

		public void Clear ()
		{
			idToObjects.Clear ();
			objectsToId.Clear ();
		}

		public abstract TId GetReference (TObject obj);

		public void AddReference (TObject value)
		{
			if (value == null) {
				return;
			}
			TId id = GetReference (value);
			idToObjects [id] = value;
			objectsToId [value] = id;
		}
	}

	public class GenericObjectsCache: ObjectsCache<string, object>
	{
		int _references;

		public GenericObjectsCache ()
		{
			_references = 0;
		}

		public override string GetReference (object obj)
		{
			string idStr;

			if (!objectsToId.TryGetValue (obj, out idStr)) {
				_references++;
				idStr = _references.ToString (CultureInfo.InvariantCulture); 
			}

			return idStr;
		}
	}

	public class StorableObjectsCache: ObjectsCache<Guid, IStorable>, IStorableObjectsCache
	{
		public override Guid GetReference (IStorable obj)
		{
			return obj != null ? obj.ID : Guid.Empty;
		}
	}
}

