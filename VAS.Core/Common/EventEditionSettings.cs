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
namespace VAS.Core.Common
{
	/// <summary>
	/// List of settings that defines the possible editions that user can do when a play event is raised.
	/// </summary>
	public class PlayEventEditionSettings
	{
		/// <summary>
		/// Gets or sets a value indicating whether tags can or not be edited
		/// </summary>
		/// <value><c>true</c> if edit tags; otherwise, <c>false</c>.</value>
		public bool EditTags { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether player positions can be edited
		/// </summary>
		/// <value><c>true</c> if edit positions; otherwise, <c>false</c>.</value>
		public bool EditPositions { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether players can be edited
		/// </summary>
		/// <value><c>true</c> if edit players; otherwise, <c>false</c>.</value>
		public bool EditPlayers { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether Notes can be edtited
		/// </summary>
		/// <value><c>true</c> if edit notes; otherwise, <c>false</c>.</value>
		public bool EditNotes { get; set; }
	}
}
