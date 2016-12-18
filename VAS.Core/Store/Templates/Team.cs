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
using System.ComponentModel;
using Newtonsoft.Json;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Serialization;

namespace VAS.Core.Store.Templates
{
	[Serializable]
	public abstract class Team : StorableBase, IDisposable, ITemplate
	{
		public const int CURRENT_VERSION = 1;

		public Team ()
		{
			ID = Guid.NewGuid ();
			Version = Constants.DB_VERSION;
		}
		protected override void Dispose (bool disposing)
		{
			if (Disposed)
				return;
			base.Dispose (disposing);
			if (disposing) {
				Shield?.Dispose ();
			}
		}

		/// <summary>
		/// Gets or sets the document version.
		/// </summary>
		/// <value>The version.</value>
		[DefaultValue (0)]
		[JsonProperty (DefaultValueHandling = DefaultValueHandling.Populate)]
		public int Version {
			get;
			set;
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
		/// Creates a deep copy of this team with new ID's for each player
		/// </summary>
		public abstract ITemplate Copy (string newName);
	}

	public class Team<TPlayer> : Team, ITemplate<TPlayer>
		where TPlayer : StorableBase
	{
		public Team ()
		{
			List = new RangeObservableCollection<TPlayer> ();
		}

		protected override void Dispose (bool disposing)
		{
			if (Disposed) {
				return;
			}
			base.Dispose (disposing);
			foreach (TPlayer p in List) {
				p.Dispose ();
			}
		}

		[JsonProperty]
		public RangeObservableCollection<TPlayer> List {
			get;
			protected set;
		}

		/// <summary>
		/// Creates a deep copy of this team with new ID's for each player
		/// </summary>
		public override ITemplate Copy (string newName)
		{
			Load ();
			ITemplate<TPlayer> newTeam = this.Clone ();
			newTeam.ID = Guid.NewGuid ();
			newTeam.DocumentID = null;
			newTeam.Name = newName;
			foreach (TPlayer player in newTeam.List) {
				player.ID = Guid.NewGuid ();
			}
			return newTeam;
		}
	}
}