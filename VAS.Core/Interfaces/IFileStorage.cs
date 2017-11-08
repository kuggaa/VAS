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
using System.IO;
using VAS.Core.Interfaces;

namespace VAS.Core.Interfaces
{
	public interface IFileStorage
	{
		/// <summary>
		/// Retrieves an object of type T from a file path
		/// </summary>
		/// <returns>The object found.</returns>
		/// <param name="from">The file path to retrieve the object from</param>
		/// <typeparam name="T">The type of the object.</typeparam>
		T RetrieveFrom<T> (string from) where T : IStorable;

		/// <summary>
		/// Retrieves an object of type T from a Stream
		/// </summary>
		/// <returns>The object found.</returns>
		/// <param name="from">The Stream to retrieve the object from</param>
		/// <typeparam name="T">The type of the object.</typeparam>
		T RetrieveFrom<T> (Stream from) where T : IStorable;

		/// <summary>
		/// Stores an object of type T at file at
		/// </summary>
		/// <param name="t">The object to store</param>
		/// <param name="at">The filename to store the object</param>
		/// <typeparam name="T">The type of the object.</typeparam>
		void StoreAt<T> (T t, string at) where T : IStorable;
	}
}
