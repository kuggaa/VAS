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
using System.Collections.ObjectModel;
using System.ComponentModel;
using Newtonsoft.Json;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Serialization;

namespace VAS.Core.Store.Templates
{
	[Serializable]
	public abstract class Team : StorableBase, IDisposable, ITemplate<Player>
	{

		public Team ()
		{
			ID = Guid.NewGuid ();
			List = new RangeObservableCollection<Player> ();
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			Shield?.Dispose ();
			foreach (Player p in List) {
				p.Dispose ();
			}
			List.Clear ();
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool Static {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public virtual Color Color {
			get;
			set;
		}

		[PropertyIndex (0)]
		[PropertyPreload]
		public String Name {
			get;
			set;
		}

		[PropertyPreload]
		public Image Shield {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the list of players.
		/// </summary>
		/// <value>The list.</value>
		[JsonProperty]
		public RangeObservableCollection<Player> List { get; set; }

		/// <summary>
		/// Creates a deep copy of this team with new ID's for each player
		/// </summary>
		public virtual ITemplate Copy (string newName)
		{
			Load ();
			ITemplate<Player> newTeam = this.Clone ();
			newTeam.ID = Guid.NewGuid ();
			newTeam.DocumentID = null;
			newTeam.Name = newName;
			foreach (Player player in newTeam.List) {
				player.ID = Guid.NewGuid ();
			}
			return newTeam;
		}
	}
}
