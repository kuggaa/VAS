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

namespace VAS.Core.Events
{
	public class GenericEvent<T> : ReturningValueEvent
	{
		public T Object { get; set; }
	}

	public class CreateEvent<T> : GenericEvent<T>
	{
		public T Source { get; set; }

		public int Count { get; set; }

		public string Name { get; set; }
	}

	public class DeleteEvent<T> : GenericEvent<T>
	{
	}

	public class UpdateEvent<T> : GenericEvent<T>
	{
		public bool Force { get; set; }
	}

	public class ExportEvent<T> : GenericEvent<T>
	{
		/// <summary>
		/// Gets or sets the format in which we want to export the object
		/// </summary>
		/// <value>The format.</value>
		public string Format { get; set; }
	}

	public class ImportEvent<T> : GenericEvent<T>
	{
	}

	public class ChangeNameEvent<T> : GenericEvent<T>
	{
		public string NewName { get; set; }
	}

	public class OpenEvent<T> : GenericEvent<T>
	{
	}

	/// <summary>
	/// Event sent to notify the cancellation of <typeparam name="T">.
	/// </summary>
	public class CancelEvent<T> : ReturningValueEvent
	{
		/// <summary>
		/// Gets or sets the object to cancel.
		/// </summary>
		/// <value>The object.</value>
		public T Object { get; set; }
	}

	/// <summary>
	/// Event sent to request the retry of a <typeparam name="T">.
	/// </summary>
	public class RetryEvent<T> : ReturningValueEvent
	{
		/// <summary>
		/// Gets or sets the object to retry.
		/// </summary>
		/// <value>The object.</value>
		public T Object { get; set; }
	}

	/// <summary>
	/// Event sent to request clearing a <typeparam name="T">.
	/// </summary>
	public class ClearEvent<T> : ReturningValueEvent
	{
	}
}

