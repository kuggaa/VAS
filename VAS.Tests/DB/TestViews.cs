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
using System.IO;
using System.Linq;
using Couchbase.Lite;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Filters;
using VAS.Core.Serialization;
using VAS.Core.Store;
using VAS.DB;
using VAS.DB.Views;

namespace VAS.Tests.DB
{
	public class PropertiesTest: StorableBase
	{
		[PropertyIndex (1)]
		[PropertyPreload]
		public string Key1 { get; set; }

		[PropertyIndex (0)]
		public string Key2 { get; set; }

		[PropertyPreload]
		public string Key3 { get; set; }

		protected override void CheckIsLoaded ()
		{
			IsLoaded = true;
		}
	}

	public class TestView: GenericView<PropertiesTest>
	{

		public TestView (CouchbaseStorage storage) : base (storage)
		{
		}

		protected override string ViewVersion { get { return "1"; } }

		public List<string> PreloadProperties { get { return PreviewProperties; } }

		public List<string> IndexedProperties { get { return FilterProperties.Keys.OfType<string> ().ToList (); } }
	}


	[TestFixture ()]
	public class TestViews
	{
		CouchbaseStorage storage;
		Database db;

		[TestFixtureSetUp]
		public void InitDB ()
		{
			string dbPath = Path.Combine (Path.GetTempPath (), "TestDB");
			if (Directory.Exists (dbPath)) {
				Directory.Delete (dbPath, true);
			}
			try {
				storage = new CouchbaseStorage (dbPath, "test-db");
			} catch (Exception ex) {
				throw ex;
			}
			db = storage.Database;
		}

		[TestFixtureTearDown]
		public void DeleteDB ()
		{
			Directory.Delete (db.Manager.Directory, true);
		}

		[TearDown]
		public void CleanDB ()
		{
			db.RunInTransaction (() => {
				foreach (var d in db.CreateAllDocumentsQuery ().Run()) {
					db.GetDocument (d.DocumentId).Delete ();
				}
				return true;
			});
		}

		[Test ()]
		public void TestIndexing ()
		{
			TestView view = new TestView (storage);

			Assert.AreEqual (new List<string> { "Parent", "Key2", "Key1" }, view.IndexedProperties);
			PropertiesTest test = new PropertiesTest { Key1 = "key1", Key2 = "key2", Key3 = "key3" };
			test.IsChanged = true;
			storage.Store (test);

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Key3", "key3");
			Assert.Throws<InvalidQueryException> (
				delegate {
					view.Query (filter).Count ();
				});
			filter.Remove ("Key3");
			filter.Add ("Key2", "key2");
			Assert.AreEqual (1, view.Query (filter).Count ());
		}

		[Test ()]
		public void TestPreload ()
		{
			TestView view = new TestView (storage);

			Assert.AreEqual (new List<string> { "Key1", "Key3" }, view.PreloadProperties);

			PropertiesTest test = new PropertiesTest { Key1 = "key1", Key2 = "key2", Key3 = "key3" };
			test.IsChanged = true;
			storage.Store (test);

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Key2", "key2");
			var test1 = view.Query (filter).First ();
			Assert.IsFalse (test1.IsLoaded);
			Assert.AreEqual (test.Key1, test1.Key1);
			Assert.AreEqual (test.Key3, test1.Key3);
			Assert.IsNull (test1.Key2);
			Assert.NotNull (test1.DocumentID);
		}
	}
}
