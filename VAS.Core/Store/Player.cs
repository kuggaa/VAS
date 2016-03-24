//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
using Newtonsoft.Json;
using VAS.Core.Common;
using VAS.Core.Serialization;

namespace VAS.Core.Store
{
	/// <summary>
	/// Player of a team
	/// </summary>
	[Serializable]
	abstract public class Player: StorableBase, IDisposable
	{

		#region Constructors

		public Player ()
		{
			ID = Guid.NewGuid ();
		}

		public void Dispose ()
		{
			Photo?.Dispose ();
		}

		#endregion

		#region Properties

		/// <summary>
		/// My name
		/// </summary>
		[LongoMatchPropertyIndex (0)]
		[LongoMatchPropertyPreload]
		public string Name {
			get;
			set;
		}

		[LongoMatchPropertyIndex (1)]
		[LongoMatchPropertyPreload]
		public string LastName {
			get;
			set;
		}

		[LongoMatchPropertyIndex (2)]
		[LongoMatchPropertyPreload]
		public string NickName {
			get;
			set;
		}

		/// <summary>
		/// My photo
		/// </summary>
		[LongoMatchPropertyPreload]
		public Image Photo {
			get;
			set;
		}

		/// <summary>
		/// Nationality
		/// </summary>
		public String Nationality {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the player e-mail.
		/// </summary>
		/// <value>
		/// The e-mail.
		/// </value>
		public string Mail {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Color Color {
			get;
			set;
		}

		#endregion


		public virtual string ToString ()
		{
			string displayName;

			if (NickName != null) {
				displayName = NickName;
			} else {
				displayName = Name + " " + LastName;
			}
			return displayName;
		}
	}
}
