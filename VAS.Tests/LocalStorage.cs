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
using System.Linq;
using VAS.Core.Interfaces;
using VAS.Core.Store;
using VAS.Core.Filters;
using VAS;

namespace Tests
{
	public class LocalStorage : IStorage
	{
		Dictionary<Guid, IStorable> localStorage;

		public LocalStorage ()
		{
			localStorage = new Dictionary<Guid, IStorable> ();
		}

		#region IStorage implementation

		public StorageInfo Info {
			get {
				return new StorageInfo {
					Name = "LocalStorage",
					LastBackup = DateTime.UtcNow,
					LastCleanup = DateTime.UtcNow,
					Version = App.Current.Version
				};
			}
		}

		public IEnumerable<T> RetrieveAll<T> () where T : IStorable
		{
			return localStorage.Values.OfType<T> ();
		}

		public T Retrieve<T> (Guid id) where T : IStorable
		{
			try {
				return (T)localStorage [id];
			} catch {
				return default (T);
			}
		}

		public void Fill (IStorable storable)
		{
			// nothing to do here
		}

		public IEnumerable<T> Retrieve<T> (QueryFilter filter) where T : IStorable
		{
			throw new NotImplementedException ();
		}

		public IEnumerable<T> RetrieveFull<T> (QueryFilter filter, IStorableObjectsCache cache) where T : IStorable
		{
			throw new NotImplementedException ();
		}

		public void Store<T> (T t, bool forceUpdate = false) where T : IStorable
		{
			localStorage [t.ID] = t;
		}

		public void Store<T> (IEnumerable<T> storableEnumerable, bool forceUpdate = false) where T : IStorable
		{
			foreach (var t in storableEnumerable) {
				Store (t, forceUpdate);
			}
		}

		public void Delete<T> (T t) where T : IStorable
		{
			localStorage.Remove (t.ID);
		}

		public void Reset ()
		{
			// nothing to do here
		}

		public bool Exists<T> (T t) where T : IStorable
		{
			return localStorage.ContainsKey (t.ID);
		}

		public int Count<T> () where T : IStorable
		{
			return localStorage.Count;
		}

		public bool Backup ()
		{
			return true;
		}

		public bool Delete ()
		{
			return true;
		}

		#endregion
	}
}
