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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Couchbase.Lite;
using VAS.Core.Common;
using VAS.Core.Interfaces;

namespace VAS.DB
{
	public class CouchbaseManager: IStorageManager
	{
		protected readonly Manager manager;
		protected IStorage activeDB;

		public CouchbaseManager (string dbDir)
		{
			manager = new Manager (new System.IO.DirectoryInfo (dbDir),
				ManagerOptions.Default);
			Databases = new List<IStorage> ();
		}

		#region IDataBaseManager implementation

		public void SetActiveByName (string name)
		{
			// Couchbase doesn't accept uppercase databases.
			IStorage db = Databases.FirstOrDefault (d => d.Info.Name == SanitizeDBName (name));
			if (db != null) {
				ActiveDB = db;
			} else {
				ActiveDB = Add (name);
			}
		}

		public IStorage Add (string name)
		{
			// Couchbase doesn't accept uppercase databases.
			name = SanitizeDBName (name);
			var storage = Add (name, false);
			if (storage != null) {
				App.Current.EventsBroker?.EmitDatabaseCreated (name);
			}
			return storage;
		}


		public bool Delete (IStorage db)
		{
			// Leave at least one
			if (Databases.Count <= 1) {
				return false;
			}
			Databases.Remove (db);
			db.Reset ();
			return true;
		}

		public void UpdateDatabases ()
		{
			foreach (string dbName in manager.AllDatabaseNames) {
				if (dbName != "templates") {
					Add (dbName, false);
				}
			}
		}

		public IStorage ActiveDB {
			get {
				return activeDB;
			}
			set {
				activeDB = value;
				App.Current.Config.CurrentDatabase = value.Info.Name;
				App.Current.Config.Save ();
			}
		}

		public List<IStorage> Databases {
			get;
			set;
		}

		#endregion

		protected IStorage Add (string name, bool check)
		{
			if (check && manager.AllDatabaseNames.Contains (name)) {
				throw new Exception ("A database with the same name already exists");
			}
			try {
				Log.Information ("Creating new database " + name);
				IStorage db = CreateStorage (name);
				Databases.Add (db);
				return db;
			} catch (Exception ex) {
				Log.Exception (ex);
				return null;
			}
		}

		protected virtual IStorage CreateStorage (string name)
		{
			return new CouchbaseStorage (manager, name);
		}

		/// <summary>
		/// Sanitizes the name of the DB.
		/// The only legal characters are lowercase ASCII letters, digits, and the special characters _$()+-/
		/// http://developer.couchbase.com/documentation/mobile/1.1.0/develop/guides/couchbase-lite/native-api/database/index.html
		/// https://github.com/couchbase/couchbase-lite-net/blob/12cf591/src/Couchbase.Lite.Shared/Manager.cs#L95
		/// </summary>
		/// <returns>The sanitized DB name.</returns>
		/// <param name="originalName">name.</param>
		protected string SanitizeDBName (string name)
		{
			name = name.ToLower ();
			name = name.Normalize (NormalizationForm.FormD);

			name = Regex.Replace (name, "[^a-z0-9_$)+/(-]", "_");
			if (Regex.IsMatch (name, "(^[^a-z]+)|[^a-z0-9_\\$\\(\\)/\\+\\-]+")) {
				name = "db" + name;
			}
			return name;
		}
	}
}

