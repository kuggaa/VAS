//
//  Copyright (C) 2017 Fluendo S.A.
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

using System.Collections.Generic;
using System.Linq;
using VAS.Core.Interfaces;

namespace Tests
{

	public class LocalDatabaseManager : IStorageManager
	{
		Dictionary<string, IStorage> databases;

		public LocalDatabaseManager ()
		{
			databases = new Dictionary<string, IStorage> ();
			Add ("Test");
			SetActiveByName ("Test");
		}

		public IStorage ActiveDB {
			get;
			set;
		}

		public List<IStorage> Databases {
			get {
				return databases.Values.ToList ();
			}
			set {
			}
		}

		public void SetActiveByName (string name)
		{
			ActiveDB = databases [name];
		}

		public IStorage Add (string name)
		{
			var db = new LocalStorage ();
			db.Info.Name = name;
			databases.Add (name, db);
			return db;
		}

		public bool Delete (IStorage db)
		{
			databases.Remove (db.Info.Name);
			return true;
		}

		public void UpdateDatabases ()
		{
		}

	}
}
