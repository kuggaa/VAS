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
using VAS.Core.MVVMC;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// A ViewModel for empty cards with information to help the user in the process of creating a new element
	/// </summary>
	public class EmptyCardVM : ViewModelBase
	{
		/// <summary>
		/// Gets or sets the header text for the empty card.
		/// </summary>
		/// <value>The header text.</value>
		public string HeaderText { get; set; }

		/// <summary>
		/// Gets or sets the description text of the empty card.
		/// </summary>
		/// <value>The description text.</value>
		public string DescriptionText { get; set; }

		/// <summary>
		/// Gets or sets the tip text of the empty card.
		/// </summary>
		/// <value>The tip text.</value>
		public string TipText { get; set; }
	}
}
