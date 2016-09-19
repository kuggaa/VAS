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
using System.Collections.Generic;

namespace VAS.Core.Events
{
	public class CRUDEvent<T>: Event
	{
		public T Object { get; set; }
	}

	public class CreateEvent<T>: CRUDEvent<T>
	{
		public T Source { get; set; }

		public int Count { get; set; }

		public string Name { get; set; }
	}

	public class DeleteEvent<T>: CRUDEvent<T>
	{
	}

	public class UpdateEvent<T>: CRUDEvent<T>
	{
		public bool Force { get; set; }
	}

	public class ExportEvent<T>: CRUDEvent<T>
	{
		/// <summary>
		/// Gets or sets the format in which we want to export the object
		/// </summary>
		/// <value>The format.</value>
		public string Format { get; set; }
	}

	public class ImportEvent<T>: CRUDEvent<T>
	{
	}

	public class ChangeNameEvent<T>: CRUDEvent<T>
	{
		public string NewName { get; set; }
	}

	public class OpenEvent<T> : CRUDEvent<T>
	{
	}
}

