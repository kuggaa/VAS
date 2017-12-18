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
using System;
namespace VAS.Core.Interfaces.MVVMC
{
	/// <summary>
	/// Interface indicating if a class (typically a ViewModel) is stateful.
	/// </summary>
	public interface IStateful
	{
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.Interfaces.MVVMC.IStateful"/> is stateful.
		/// This allows a class to be used in both ways (stateful and stateless).
		/// </summary>
		/// <value><c>true</c> if stateful; otherwise, <c>false</c>.</value>
		bool Stateful { get; set; }

		/// <summary>
		/// Commits the state stored.
		/// </summary>
		void CommitState ();
	}
}
