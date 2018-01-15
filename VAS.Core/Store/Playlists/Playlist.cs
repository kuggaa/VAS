// PlayList.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Serialization;

namespace VAS.Core.Store.Playlists
{
	[Serializable]
	public class Playlist : StorableBase
	{
		DateTime lastModified;
		RangeObservableCollection<IPlaylistElement> elements;

		#region Constructors

		public Playlist ()
		{
			ID = Guid.NewGuid ();
			Elements = new RangeObservableCollection<IPlaylistElement> ();
			CreationDate = LastModified = DateTime.Now;
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			foreach (var element in Elements.OfType<IDisposable> ()) {
				element.Dispose ();
			}
			Elements.Clear ();
		}

		#endregion

		#region Properties

		[PropertyPreload]
		[PropertyIndex (0)]
		public string Name {
			get;
			set;
		}

		public RangeObservableCollection<IPlaylistElement> Elements {
			get {
				return elements;
			}
			set {
				elements = value;
				UpdateDuration ();
			}
		}

		/// <summary>
		/// Duration in time for the playlist.
		/// </summary>
		[PropertyPreload]
		public Time Duration {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the last modified.
		/// </summary>
		/// <value>The last modified.</value>
		[PropertyPreload]
		public DateTime LastModified {
			get {
				return lastModified;
			}
			set {
				lastModified = value.ToUniversalTime ();
			}
		}

		#endregion

		#region Public methods

		public Playlist Copy ()
		{
			return (Playlist)(MemberwiseClone ());
		}

		#endregion

		void UpdateDuration ()
		{
			if (Elements != null && Elements.Count > 0) {
				Duration = new Time (Elements.Sum (elem => elem.Duration.MSeconds));
			} else {
				Duration = new Time (0);
			}
		}

		protected override void CollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			base.CollectionChanged (sender, e);
			UpdateDuration ();
		}

		protected override void ForwardPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Start" || e.PropertyName == "Stop") {
				UpdateDuration ();
			}
			base.ForwardPropertyChanged (sender, e);
		}
	}
}
