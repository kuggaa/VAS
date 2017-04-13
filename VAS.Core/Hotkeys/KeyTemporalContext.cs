//
//  Copyright (C) 2017 FLUENDO S.A.
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
namespace VAS.Core.Hotkeys
{
	/// <summary>
	/// Key context that is temporal in the application
	/// </summary>
	public class KeyTemporalContext : KeyContext
	{
		/// <summary>
		/// The context duration in miliseconds
		/// </summary>
		/// <value>The duration in ms.</value>
		public int Duration { get; set; }

		/// <summary>
		/// Gets or sets the started time of the context in the application
		/// </summary>
		/// <value>The started time.</value>
		public DateTime StartedTime { get; set; }

		/// <summary>
		/// Gets or sets the action to perform when the context has expired
		/// </summary>
		/// <value>The action.</value>
		public Action ExpiredTimeAction { get; set; }
	}
}
