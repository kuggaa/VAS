//
//  Copyright (C) 2016 Fluendo S.A.
using System;
using System.IO;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Serialization;

namespace VAS.DB
{
	public class FileStorage : IFileStorage
	{
		/// <summary>
		/// Retrieves an object of type T from a file path
		/// </summary>
		/// <returns>The object found.</returns>
		/// <param name="from">The file path to retrieve the object from</param>
		/// <typeparam name="T">The type of the object.</typeparam>
		public T RetrieveFrom<T> (string from) where T : IStorable
		{
			Log.Information ("Loading " + from);
			T storable = Serializer.Instance.LoadSafe<T> (from);
			MigrateStorable (storable);
			return storable;
		}

		/// <summary>
		/// Retrieves an object of type T from a Stream
		/// </summary>
		/// <returns>The object found.</returns>
		/// <param name="from">The Stream to retrieve the object from</param>
		/// <typeparam name="T">The type of the object.</typeparam>
		public T RetrieveFrom<T> (Stream from) where T : IStorable
		{
			T storable = Serializer.Instance.Load<T> (from);
			MigrateStorable (storable);
			return storable;
		}

		/// <summary>
		/// Stores an object of type T at file at
		/// </summary>
		/// <param name="t">The object to store</param>
		/// <param name="at">The filename to store the object</param>
		/// <typeparam name="T">The type of the object.</typeparam>
		public void StoreAt<T> (T t, string at) where T : IStorable
		{
			Log.Information ("Saving " + t.ID.ToString () + " to " + at);

			if (App.Current.FileSystemManager.Exists (at)) {
				throw new Exception ("A file already exists at " + at);
			}

			if (!Directory.Exists (Path.GetDirectoryName (at))) {
				Directory.CreateDirectory (Path.GetDirectoryName (at));
			}

			/* Don't cach the Exception here to chain it up */
			Serializer.Instance.Save<T> ((T)t, at);
		}

		protected virtual void MigrateStorable (IStorable storable)
		{
			/* FIXME: to be implemented */
		}
	}
}
