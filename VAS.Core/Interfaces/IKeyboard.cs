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
using VAS.Core.Store;

namespace VAS.Core.Interfaces
{
	public interface IKeyboard
	{
		/// <summary>
		/// Get a key value from its name.
		/// </summary>
		/// <returns>The key value.</returns>
		/// <param name="name">The key name.</param>
		uint KeyvalFromName (string name);

		/// <summary>
		/// Get the name of a key from its key value.
		/// </summary>
		/// <returns>The key name.</returns>
		/// <param name="keyval">The key value.</param>
		string NameFromKeyval (uint keyval);

		/// <summary>
		/// Parses a key name and returns a <see cref="HotKey"/>.
		/// </summary>
		/// <returns>The key name.</returns>
		/// <param name="name">A hotkey.</param>
		HotKey ParseName (string name);

		/// <summary>
		/// Gets the name of a hotkey.
		/// </summary>
		/// <returns>The key name.</returns>
		/// <param name="hotkey">Hotkey.</param>
		string HotKeyName (HotKey hotkey);

		/// <summary>
		/// Parses a native key pressed event from the UI toolkit and creates a hotkey.
		/// </summary>
		/// <returns>A hotkey.</returns>
		/// <param name="evt">The event to parse.</param>
		HotKey ParseEvent (object evt);

		/// <summary>
		/// Parses a native key or mouse pressed event from the UI toolkit and returns the modifiers used.
		/// </summary>
		/// <returns>A hotkey.</returns>
		/// <param name="evt">The event to parse.</param>
		int ParseModifier (object evt);
	}
}