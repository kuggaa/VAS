//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System.Threading.Tasks;

namespace VAS.Core
{
	/// <summary>
	/// Async helpers for return only Task from result
	/// </summary>
	public static class AsyncHelpers
	{
		/// <summary>
		/// Return typed Task and the specified value.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static Task<T> Return<T> (T value)
		{
			return Task<T>.FromResult (value);
		}

		/// <summary>
		/// Return Task.
		/// </summary>
		public static Task Return ()
		{
			return Task<bool>.FromResult (false);
		}
	}
}

