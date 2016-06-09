//
//  Copyright (C) 2015 Andoni Morales Alastruey
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
using Couchbase.Lite;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Filters;
using VAS.Core.Interfaces;
using VAS.Core.Store;
using VAS.DB;
using VAS.DB.Views;

namespace VAS.Tests.DB
{
	class StorableContainerTest: StorableBase
	{
		public StorableContainerTest ()
		{
			ID = Guid.NewGuid ();
		}

		public StorableImageTest Image { get; set; }
	}

	class StorableListTest: StorableBase
	{
		public StorableListTest ()
		{
			ID = Guid.NewGuid ();
		}

		public List<StorableImageTest> Images { get; set; }
	}

	class StorableListNoChildrenTest: StorableListTest
	{
		public override bool DeleteChildren {
			get {
				return false;
			}
		}
	}

	class StorableImageTest : StorableBase
	{
		public StorableImageTest ()
		{
			ID = Guid.NewGuid ();
		}

		public Image Image1 { get; set; }

		public Image Image2 { get; set; }

		public List<Image> Images { get; set; }
	}

	class StorableImageTest2: StorableImageTest
	{
	}

	class StorableView: GenericView <IStorable>
	{
		public StorableView (CouchbaseStorage storage) : base (storage)
		{
		}

		protected override string ViewVersion {
			get {
				return "1";
			}
		}
	}

	[TestFixture ()]
	public class TestStorage
	{
		Database db;
		IStorage storage;

		[TestFixtureSetUp]
		public void InitDB ()
		{
			string tmpPath = Path.GetTempPath ();
			string homePath = Path.Combine (tmpPath, "VAS");
			string dbPath = Path.Combine (homePath, "db");
			if (Directory.Exists (dbPath)) {
				Directory.Delete (dbPath, true);
			}

			Directory.CreateDirectory (tmpPath);
			Directory.CreateDirectory (homePath);
			Directory.CreateDirectory (dbPath);

			storage = new CouchbaseStorage (dbPath, "test-db");
			db = ((CouchbaseStorage)storage).Database;
			// Remove the StorageInfo doc to get more understandable document count results
			db.GetDocument (Guid.Empty.ToString ()).Delete ();

			App.Current.baseDirectory = tmpPath;
			App.Current.homeDirectory = homePath;

			/* Check default folders */
			CheckDirs ();
		}

		private static void CheckDirs ()
		{
			if (!System.IO.Directory.Exists (App.Current.HomeDir))
				System.IO.Directory.CreateDirectory (App.Current.HomeDir);
			if (!System.IO.Directory.Exists (App.Current.SnapshotsDir))
				System.IO.Directory.CreateDirectory (App.Current.SnapshotsDir);
			if (!System.IO.Directory.Exists (App.Current.PlayListDir))
				System.IO.Directory.CreateDirectory (App.Current.PlayListDir);
			if (!System.IO.Directory.Exists (App.Current.DBDir))
				System.IO.Directory.CreateDirectory (App.Current.DBDir);
			if (!System.IO.Directory.Exists (App.Current.VideosDir))
				System.IO.Directory.CreateDirectory (App.Current.VideosDir);
			if (!System.IO.Directory.Exists (App.Current.TempVideosDir))
				System.IO.Directory.CreateDirectory (App.Current.TempVideosDir);
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

		[Test]
		public void TestDeleteError ()
		{
			Assert.Throws<StorageException> (() => storage.Delete<Project> (null));
		}

		[Test]
		public void TestStoreError ()
		{
			Assert.Throws<StorageException> (() => storage.Store<Project> (null));
		}

		[Test ()]
		public void TestDocType ()
		{
			StorableImageTest t = new StorableImageTest {
				ID = Guid.NewGuid (),
			};
			Document doc = db.CreateDocument ();
			SerializationContext context = new SerializationContext (db, t.GetType ());
			JObject jo = DocumentsSerializer.SerializeObject (t, doc.CreateRevision (), context);
			Assert.AreEqual (t.ID, jo.Value<Guid> ("ID"));
			Assert.AreEqual ("StorableImageTest", jo.Value<string> ("DocType"));
		}

		[Test ()]
		public void TestStoreImages ()
		{
			Image img = Utils.LoadImageFromFile ();
			StorableImageTest t = new StorableImageTest {
				Image1 = img,
				Image2 = img,
				ID = Guid.NewGuid (),
			};
			Document doc = db.CreateDocument ();
			UnsavedRevision rev = doc.CreateRevision ();
			SerializationContext context = new SerializationContext (db, t.GetType ());
			JObject jo = DocumentsSerializer.SerializeObject (t, rev, context);
			Assert.IsNotNull (jo ["ID"]);
			Assert.AreEqual ("attachment::Image1_1", jo ["Image1"].Value<string> ());
			Assert.AreEqual ("attachment::Image2_1", jo ["Image2"].Value<string> ());
			int i = 0;
			foreach (string name in rev.AttachmentNames) {
				i++;
				Assert.AreEqual (string.Format ("Image{0}_1", i), name);
			}
		}

		[Test ()]
		public void TestStoreImagesList ()
		{
			Image img = Utils.LoadImageFromFile ();
			StorableImageTest t = new StorableImageTest {
				Images = new List<Image> { img, img, img },
				ID = Guid.NewGuid (),
			};
			Document doc = db.CreateDocument ();
			UnsavedRevision rev = doc.CreateRevision ();
			SerializationContext context = new SerializationContext (db, t.GetType ());
			JObject jo = DocumentsSerializer.SerializeObject (t, rev, context);
			int i = 0;
			foreach (string name in rev.AttachmentNames) {
				i++;
				Assert.AreEqual ("Images_" + i, name);
			}
			Assert.AreEqual (3, i);
			Assert.AreEqual ("attachment::Images_1", jo ["Images"] [0].Value<string> ());
			Assert.AreEqual ("attachment::Images_2", jo ["Images"] [1].Value<string> ());
			Assert.AreEqual ("attachment::Images_3", jo ["Images"] [2].Value<string> ());
		}

		[Test ()]
		public void TestDeleteChildren ()
		{
			StorableListTest list = new StorableListTest ();
			list.Images = new List<StorableImageTest> ();
			list.Images.Add (new StorableImageTest ());
			list.Images.Add (new StorableImageTest ());
			storage.Store (list);
			Assert.AreEqual (3, db.DocumentCount);
			storage.Delete (list);
			Assert.AreEqual (0, db.DocumentCount);

			StorableListNoChildrenTest list2 = new StorableListNoChildrenTest ();
			list2.Images = new List<StorableImageTest> ();
			list2.Images.Add (new StorableImageTest ());
			list2.Images.Add (new StorableImageTest ());
			storage.Store (list2);
			Assert.AreEqual (3, db.DocumentCount);
			storage.Delete (list2);
			Assert.AreEqual (2, db.DocumentCount);
		}

		[Test ()]
		public void TestDeleteOrphanedChildrenOnDelete ()
		{
			StorableListTest list = new StorableListTest ();
			list.Images = new List<StorableImageTest> ();
			list.Images.Add (new StorableImageTest ());
			list.Images.Add (new StorableImageTest ());
			storage.Store (list);
			Assert.AreEqual (3, db.DocumentCount);
			list = storage.Retrieve<StorableListTest> (list.ID);
			list.Images.Remove (list.Images [0]);
			storage.Delete (list);
			Assert.AreEqual (0, db.DocumentCount);
		}

		[Test ()]
		public void TestDeleteOrphanedChildrenOnUpdate ()
		{
			StorableListTest list = new StorableListTest ();
			list.Images = new List<StorableImageTest> ();
			list.Images.Add (new StorableImageTest ());
			list.Images.Add (new StorableImageTest ());
			storage.Store (list);
			Assert.AreEqual (3, db.DocumentCount);
			list = storage.Retrieve<StorableListTest> (list.ID);
			list.Images.Remove (list.Images [0]);
			storage.Store (list);
			Assert.AreEqual (2, db.DocumentCount);
		}

		[Test ()]
		public void TestRetrieveImages ()
		{
			Image img = Utils.LoadImageFromFile ();
			StorableImageTest t = new StorableImageTest {
				Image1 = img,
				Image2 = img,
				ID = Guid.NewGuid (),
			};
			storage.Store (t);
			var test2 = storage.Retrieve<StorableImageTest> (t.ID);
			Assert.AreEqual (t.Image1.Width, test2.Image1.Width);
			Assert.AreEqual (t.Image1.Height, test2.Image1.Height);
			Assert.AreEqual (t.Image2.Width, test2.Image2.Width);
			Assert.AreEqual (t.Image2.Height, test2.Image2.Height);
		}

		[Test ()]
		public void TestStoreStorableByReference ()
		{
			StorableImageTest img = new StorableImageTest {
				ID = Guid.NewGuid (),
				Image1 = Utils.LoadImageFromFile (),
			};
			StorableContainerTest cont = new StorableContainerTest {
				ID = Guid.NewGuid (),
				Image = img,
			};
			Assert.AreEqual (0, db.DocumentCount);
			Document doc = db.CreateDocument ();
			UnsavedRevision rev = doc.CreateRevision ();
			SerializationContext context = new SerializationContext (db, cont.GetType ());
			JObject jo = DocumentsSerializer.SerializeObject (cont, rev, context);
			Assert.AreEqual (img.ID.ToString (), jo ["Image"].Value<String> ());
			Assert.AreEqual (1, db.DocumentCount);
			Assert.IsNotNull (storage.Retrieve<StorableImageTest> (img.ID));
			rev.Save ();
			Assert.AreEqual (2, db.DocumentCount);
		}

		[Test ()]
		public void TestRetrieveStorableByReference ()
		{
			StorableImageTest img = new StorableImageTest {
				ID = Guid.NewGuid (),
				Image1 = Utils.LoadImageFromFile (),
			};
			StorableContainerTest cont = new StorableContainerTest {
				ID = Guid.NewGuid (),
				Image = img,
			};
			storage.Store (cont);
			Assert.AreEqual (2, db.DocumentCount);
			var cont2 = storage.Retrieve <StorableContainerTest> (cont.ID);
			Assert.IsNotNull (cont2.Image);
			Assert.AreEqual (img.ID, cont2.Image.ID);
		}

		[Test ()]
		public void TestRetrieveStorableListByReference ()
		{
			StorableListTest list = new StorableListTest ();
			list.Images = new List<StorableImageTest> ();
			list.Images.Add (new StorableImageTest ());
			list.Images.Add (new StorableImageTest2 ());

			storage.Store (list);
			Assert.AreEqual (3, db.DocumentCount);

			StorableListTest list2 = storage.Retrieve<StorableListTest> (list.ID);
			Assert.AreEqual (2, list2.Images.Count);
			Assert.AreEqual (typeof(StorableImageTest), list2.Images [0].GetType ());
			Assert.AreEqual (typeof(StorableImageTest2), list2.Images [1].GetType ());
		}

		[Test ()]
		public void TestStorableIDUsesRootStorableID ()
		{
			StorableImageTest img = new StorableImageTest {
				ID = Guid.NewGuid (),
				Image1 = Utils.LoadImageFromFile (),
			};
			StorableContainerTest cont = new StorableContainerTest {
				ID = Guid.NewGuid (),
				Image = img,
			};
			Assert.AreEqual (0, db.DocumentCount);
			string newID = String.Format ("{0}&{1}", cont.ID, img.ID); 
			storage.Store (cont);
			Assert.AreEqual (2, db.DocumentCount);
			Assert.IsNotNull (db.GetExistingDocument (cont.ID.ToString ()));
			Assert.IsNotNull (db.GetExistingDocument (newID));
			cont = storage.Retrieve<StorableContainerTest> (cont.ID);
			Assert.AreEqual (img.ID, cont.Image.ID);
			storage.Delete (cont);
			Assert.AreEqual (0, db.DocumentCount);
		}


		[Test ()]
		public void TestRetrieveErrors ()
		{
			// ID does not exists
			Assert.IsNull (storage.Retrieve<Project> (Guid.Empty));
			// ID exists but for a different type;
			StorableImageTest t = new StorableImageTest {
				ID = Guid.NewGuid (),
			};
			storage.Store (t);
			Assert.IsNull (storage.Retrieve<Project> (t.ID));
		}

		[Test ()]
		public void TestBackup ()
		{
			var res = storage.Backup ();
			Assert.IsTrue (res);

			string outputPath = Path.Combine (App.Current.DBDir, storage.Info.Name + ".tar.gz");
			Assert.IsTrue (File.Exists (outputPath));
		}

		[Test ()]
		public void TestAddView ()
		{
			// Initially we don't have a view for IStorable
			Assert.Throws<KeyNotFoundException> (() => storage.Retrieve<IStorable> (new QueryFilter ()));

			((CouchbaseStorage)storage).AddView (typeof(IStorable), new StorableView (((CouchbaseStorage)storage)));

			Assert.DoesNotThrow (() => storage.Retrieve<IStorable> (new QueryFilter ()));
		}
	}
}
