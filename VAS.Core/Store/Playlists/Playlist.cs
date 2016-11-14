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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
		DateTime creationDate, lastModified;
		int indexSelection = 0;
		RangeObservableCollection<IPlaylistElement> elements;

		#region Constructors

		public Playlist ()
		{
			ID = Guid.NewGuid ();
			Elements = new RangeObservableCollection<IPlaylistElement> ();
			CreationDate = LastModified = DateTime.Now;
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

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public int CurrentIndex {
			get {
				return indexSelection;
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public IPlaylistElement Selected {
			get {
				if (Elements.Count == 0) {
					return null;
				}
				if (indexSelection >= Elements.Count) {
					indexSelection = 0;
				}
				return Elements [indexSelection];
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
		/// Gets the creation date. Set on creation to DateTime.Now.
		/// </summary>
		/// <value>The creation date.</value>
		[PropertyPreload]
		public DateTime CreationDate {
			get {
				return creationDate;
			}
			set {
				creationDate = value.ToUniversalTime ();
			}
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

		public IPlaylistElement Next ()
		{
			if (HasNext ())
				indexSelection++;
			return Elements [indexSelection];
		}

		public IPlaylistElement Prev ()
		{
			if (HasPrev ())
				indexSelection--;
			return Elements [indexSelection];
		}

		public void Reorder (int indexIn, int indexOut)
		{
			var play = Elements [indexIn];
			Elements.RemoveAt (indexIn);
			Elements.Insert (indexOut, play);

			/* adjust selection index */
			if (indexIn == indexSelection)
				indexSelection = indexOut;
			if (indexIn < indexOut) {
				if (indexSelection < indexIn || indexSelection > indexOut)
					return;
				indexSelection++;
			} else {
				if (indexSelection > indexIn || indexSelection < indexOut)
					return;
				indexSelection--;
			}
		}

		public bool Remove (IPlaylistElement plNode)
		{
			bool ret = Elements.Remove (plNode);
			if (CurrentIndex >= Elements.Count)
				indexSelection--;
			return ret;
		}

		public IPlaylistElement Select (int index)
		{
			indexSelection = index;
			return Elements [index];
		}

		public void SetActive (IPlaylistElement play)
		{
			int newIndex;

			newIndex = Elements.IndexOf (play);
			if (newIndex >= 0) {
				indexSelection = newIndex;
			}
		}

		public bool HasNext ()
		{
			return indexSelection < Elements.Count - 1;
		}

		public bool HasPrev ()
		{
			return !indexSelection.Equals (0);
		}

		public Playlist Copy ()
		{
			return (Playlist)(MemberwiseClone ());
		}

		/// <summary>
		/// Gets the element and its start at the passed time.
		/// </summary>
		/// <returns>A tuple with the element at the passed time and its start time in the playlist.</returns>
		/// <param name="pos">Time to query.</param>
		public Tuple<IPlaylistElement, Time> GetElementAtTime (Time pos)
		{
			Time elementStart = new Time (0);
			IPlaylistElement element = null;
			foreach (var elem in Elements) {
				if (pos >= elementStart && pos < elementStart + elem.Duration) {
					element = elem;
					break;
				} else if (pos >= elementStart + elem.Duration) {
					elementStart += elem.Duration;
				}
			}
			return new Tuple<IPlaylistElement, Time> (element, elementStart);
		}

		public Time GetStartTime (IPlaylistElement element)
		{
			return new Time (Elements.TakeWhile (elem => elem != element).Sum (elem => elem.Duration.MSeconds));
		}

		public Time GetCurrentStartTime ()
		{
			if (CurrentIndex >= 0 && CurrentIndex < Elements.Count) {
				return GetStartTime (Elements [CurrentIndex]);
			}
			return new Time (0);
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
	}
}
