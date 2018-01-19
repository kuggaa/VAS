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
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Templates;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// A ViewModel for <see cref="TimelineEvent"/> objects.
	/// </summary>
	public class TimelineEventVM : TimeNodeVM, IComparable, IViewModel<TimelineEvent>, IPlayableEvent
	{
		public virtual new TimelineEvent Model {
			get {
				return (TimelineEvent)base.Model;
			}
			set {
				base.Model = value;
			}
		}

		/// <summary>
		/// Gets the color.
		/// </summary>
		/// <value>The color.</value>
		public Color Color {
			get {
				return Model?.Color;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance has drawings.
		/// </summary>
		/// <value><c>true</c> if this instance has drawings; otherwise, <c>false</c>.</value>
		public bool HasDrawings {
			get {
				return Model.HasDrawings;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance has field position.
		/// </summary>
		/// <value><c>true</c> if this instance has field position; otherwise, <c>false</c>.</value>
		public bool HasFieldPosition {
			get {
				return (Model.FieldPosition != null);
			}
		}

		/// <summary>
		/// Gets or sets the notes.
		/// </summary>
		/// <value>The notes.</value>
		public string Notes {
			get {
				return Model.Notes;
			}
			set {
				Model.Notes = value;
			}
		}

		/// <summary>
		/// Gets the drawings.
		/// </summary>
		/// <value>The drawings.</value>
		public RangeObservableCollection<FrameDrawing> Drawings {
			get {
				return Model.Drawings;
			}
		}

		/// <summary>
		/// Gets or sets the opaque object used by the view to describe the cameras layout.
		/// </summary>
		public object CamerasLayout {
			get {
				return Model.CamerasLayout;
			}
			set {
				Model.CamerasLayout = value;
			}
		}

		/// <summary>
		/// A list of visible <see cref="CameraConfig"/> for this event.
		/// </summary>
		public RangeObservableCollection<CameraConfig> CamerasConfig {
			get {
				return Model.CamerasConfig;
			}
			set {
				Model.CamerasConfig = value;
			}
		}

		//FIXME: Use MediaFileSetVM instead of MediaFileset
		/// <summary>
		/// Gets or sets the file set for the model.
		/// </summary>
		/// <value>The file set.</value>
		public MediaFileSet FileSet {
			get {
				return Model.FileSet;
			}
			set {
				Model.FileSet = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="VAS.Services.ViewModel.TimelineEventVM`1"/> is selected.
		/// </summary>
		/// <value><c>true</c> if selected; otherwise, <c>false</c>.</value>
		public bool Selected {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Services.ViewModel.TimelineEventVM`1"/> has focus.
		/// </summary>
		/// <value><c>true</c> if focus; otherwise, <c>false</c>.</value>
		public bool Focus {
			get;
			set;
		}

		/// <summary>
		/// Position of this event in the field.
		/// </summary>
		/// <value>The field position.</value>
		public Coordinates FieldPosition {
			get {
				return Model.FieldPosition;
			}
			set {
				Model.FieldPosition = value;
			}
		}

		/// <summary>
		/// The <see cref="EventType"/> in wich this event is tagged
		/// </summary>
		public EventType EventType {
			get {
				return Model.EventType;
			}
			set {
				Model.EventType = value;
			}
		}

		/// <summary>
		/// List of tags describing this event.
		/// </summary>
		/// <value>The tags.</value>
		public RangeObservableCollection<Tag> Tags => Model.Tags;

		/// <summary>
		/// Gets the description.
		/// </summary>
		/// <value>The description.</value>
		public string Description => Model.Description;

		/// <summary>
		/// Gets the miniature.
		/// </summary>
		/// <value>The miniature.</value>
		public Image Miniature => Model.Miniature;

		/// <summary>
		/// List of players tagged in this event.
		/// </summary>
		public RangeObservableCollection<Player> Players => Model.Players;

		/// <summary>
		/// A list of teams tagged in this event.
		/// </summary>
		public RangeObservableCollection<Team> Teams => Model.Teams;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.TimelineEventVM"/> is playing.
		/// </summary>
		/// <value><c>true</c> if playing; otherwise, <c>false</c>.</value>
		public bool Playing { get; set; }

		/// <summary>
		/// Compare the specified TimelineEventVM with the calling one.
		/// </summary>
		/// <returns>The compare result.</returns>
		/// <param name="evt">Timeline event to be compared with.</param>
		public int CompareTo (object evt)
		{
			int ret;
			TimelineEventVM timelineEventB = evt as TimelineEventVM;
			switch (Model.EventType.SortMethod) {
			case (SortMethodType.SortByName):
				ret = string.Compare (Name, timelineEventB.Name);
				if (ret == 0) {
					ret = (Duration - timelineEventB.Duration).MSeconds;
				}
				break;
			case (SortMethodType.SortByStartTime):
				ret = (Start - timelineEventB.Start).MSeconds;
				if (ret == 0) {
					ret = string.Compare (Name, timelineEventB.Name);
				}
				break;
			case (SortMethodType.SortByStopTime):
				ret = (Stop - timelineEventB.Stop).MSeconds;
				if (ret == 0) {
					ret = string.Compare (Name, timelineEventB.Name);
				}
				break;
			case (SortMethodType.SortByDuration):
				ret = (Duration - timelineEventB.Duration).MSeconds;
				if (ret == 0) {
					ret = string.Compare (Name, timelineEventB.Name);
				}
				break;
			default:
				return 0;
			}
			if (ret == 0) {
				ret = timelineEventB.GetHashCode () - GetHashCode ();
			}
			return ret;
		}
	}

	/// <summary>
	/// Timeline event ViewModel Generic Base class
	/// </summary>
	public class TimelineEventVM<T> : TimelineEventVM, IViewModel<T>
		where T : TimelineEvent
	{
		public virtual new T Model {
			get {
				return (T)base.Model;
			}
			set {
				base.Model = value;
			}
		}
	}
}

