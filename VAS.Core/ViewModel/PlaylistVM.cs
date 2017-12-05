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
//

using System;
using System.Linq;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// ViewModel for a Playlist containing a collection of <see cref="IPlaylistElementVM"/>.
	/// </summary>
	public class PlaylistVM : NestedSubViewModel<Playlist, IPlaylistElement, PlayableElementVM<IPlaylistElement>>
	{
		int indexSelection = 0;

		public PlaylistVM ()
		{
			SubViewModel.TypeMappings.Add (typeof (TimelineEvent), typeof (TimelineEventVM));
			SubViewModel.TypeMappings.Add (typeof (PlaylistPlayElement), typeof (PlaylistPlayElementVM));
			SubViewModel.TypeMappings.Add (typeof (PlaylistVideo), typeof (PlaylistVideoVM));
			SubViewModel.TypeMappings.Add (typeof (PlaylistImage), typeof (PlaylistImageVM));
			SubViewModel.TypeMappings.Add (typeof (PlaylistDrawing), typeof (PlaylistDrawingVM));
		}

		/// <summary>
		/// Gets or sets the name of the playlist.
		/// </summary>
		/// <value>The name.</value>
		public string Name {
			get {
				return Model.Name;
			}
			set {
				Model.Name = value;
			}
		}

		/// <summary>
		/// Gets the creation date of the playlist.
		/// </summary>
		/// <value>The creation date.</value>
		public DateTime CreationDate {
			get {
				return Model.CreationDate;
			}
		}

		/// <summary>
		/// Gets or sets the last modification time of the playlist.
		/// </summary>
		/// <value>The last modified.</value>
		public DateTime LastModified {
			get {
				return Model.LastModified;
			}
			set {
				Model.LastModified = value;
			}
		}

		/// <summary>
		/// Duration in time for the playlist.
		/// </summary>
		public Time Duration {
			get {
				return Model.Duration;
			}
			set {
				Model.Duration = value;
			}
		}

		/// <summary>
		/// Gets the list of <see cref="IPlaylistElement"/> children.
		/// </summary>
		/// <value>The child models.</value>
		public override RangeObservableCollection<IPlaylistElement> ChildModels {
			get {
				return Model.Elements;
			}
		}

		public int CurrentIndex {
			get {
				return indexSelection;
			}
		}

		public PlayableElementVM<IPlaylistElement> Selected
		{
			get
			{
				if (ViewModels.Count == 0)
				{
					return null;
				}
				if (indexSelection >= ViewModels.Count)
				{
					indexSelection = 0;
				}
				return ViewModels[indexSelection];
			}
		}

		public PlayableElementVM<IPlaylistElement> Next()
		{
			if (HasNext())
				indexSelection++;

			return ViewModels[indexSelection];
		}

		public PlayableElementVM<IPlaylistElement> Prev()
		{
			if (HasPrev())
				indexSelection--;

			return ViewModels[indexSelection];
		}

		public void Reorder(int indexIn, int indexOut)
		{
			var play = ViewModels[indexIn];
			ViewModels.RemoveAt(indexIn);
			ViewModels.Insert(indexOut, play);

			/* adjust selection index */
			if (indexIn == indexSelection)
				indexSelection = indexOut;
			if (indexIn < indexOut)
			{
				if (indexSelection < indexIn || indexSelection > indexOut)
					return;
				indexSelection++;
			}
			else
			{
				if (indexSelection > indexIn || indexSelection < indexOut)
					return;
				indexSelection--;
			}
		}

		//FIXME: Use Viewmodels instead ChildModels
		public bool Remove(PlayableElementVM<IPlaylistElement> plNode)
		{
			bool ret = ViewModels.Remove(plNode);
			if (CurrentIndex >= ViewModels.Count)
				indexSelection--;
			return ret;
		}

		public PlayableElementVM<IPlaylistElement> Select(int index)
		{
			indexSelection = index;
			return ViewModels[index];
		}

		public void SetActive(PlayableElementVM<IPlaylistElement> play)
		{
			int newIndex;

			newIndex = Model.Elements.IndexOf(play.Model);
			if (newIndex >= 0)
			{
				indexSelection = newIndex;
			}
		}

		public bool HasNext()
		{
			return indexSelection < ViewModels.Count - 1;
		}

		public bool HasPrev()
		{
			return !indexSelection.Equals(0);
		}

		/// <summary>
		/// Gets the element and its start at the passed time.
		/// </summary>
		/// <returns>A tuple with the element at the passed time and its start time in the playlist.</returns>
		/// <param name="pos">Time to query.</param>
		public Tuple<PlayableElementVM<IPlaylistElement>, Time> GetElementAtTime(Time pos)
		{
			Time elementStart = new Time(0);
			PlayableElementVM<IPlaylistElement> element = null;
			foreach (var elem in ViewModels)
			{
				if (pos >= elementStart && pos < elementStart + (elem as PlayableElementVM<IPlaylistElement>).Model.Duration)
				{
					element = elem;
					break;
				}
				else if (pos >= elementStart + (elem as PlayableElementVM<IPlaylistElement>).Model.Duration)
				{
					elementStart += (elem as PlayableElementVM<IPlaylistElement>).Model.Duration;
				}
			}
			return new Tuple<PlayableElementVM<IPlaylistElement>, Time>(element, elementStart);
		}

		public Time GetStartTime(PlayableElementVM<IPlaylistElement> element)
		{
			return new Time(ChildModels.TakeWhile(elem => elem != element.Model).Sum(elem => elem.Duration.MSeconds));
		}

		public Time GetCurrentStartTime()
		{
			if (CurrentIndex >= 0 && CurrentIndex < ViewModels.Count)
			{
				return GetStartTime(ViewModels[CurrentIndex]);
			}
			return new Time(0);
		}
	}
}