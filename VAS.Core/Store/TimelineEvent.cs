// MediaTimeNode.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Newtonsoft.Json;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Serialization;

namespace VAS.Core.Store
{
	/// <summary>
	/// Represents a tagged event in the game at a specific position in the timeline.
	/// </summary>

	[Serializable]
	public class TimelineEvent : PixbufTimeNode, IStorable, IDisposable
	{
		[NonSerialized]
		IStorage storage;
		ObservableCollection<FrameDrawing> drawings;
		ObservableCollection<Tag> tags;
		ObservableCollection<CameraConfig> camerasConfig;

		#region Constructors

		public TimelineEvent ()
		{
			IsLoaded = true;
			Drawings = new ObservableCollection<FrameDrawing> ();
			Tags = new ObservableCollection<Tag> ();
			Rate = 1.0f;
			ID = Guid.NewGuid ();
			CamerasConfig = new ObservableCollection<CameraConfig> { new CameraConfig (0) };
		}

		public void Dispose ()
		{
			Miniature?.Dispose ();
			foreach (var drawing in Drawings) {
				drawing.Miniature?.Dispose ();
			}
		}

		#endregion

		#region Properties

		#region IStorable

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool IsLoaded {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		bool IsLoading {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public IStorage Storage {
			get {
				return storage;
			}
			set {
				storage = value;
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool DeleteChildren {
			get {
				return false;
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public List<IStorable> SavedChildren {
			get;
			set;
		}

		public Guid ID {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public string DocumentID {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Guid ParentID {
			get;
			set;
		}


		#endregion

		// All properties that are not preload must be overriden so that Fody.Loader can process
		// this properties and inject the CheckIsLoaded method
		public override Time Start {
			get {
				return base.Start;
			}
			set {
				base.Start = value;
			}
		}

		public override Time Stop {
			get {
				return base.Stop;
			}
			set {
				base.Stop = value;
			}
		}

		public override Time EventTime {
			get {
				return base.EventTime;
			}
			set {
				base.EventTime = value;
			}
		}

		public override Image Miniature {
			get {
				return base.Miniature;
			}
			set {
				base.Miniature = value;
			}
		}

		public override string Name {
			get {
				return base.Name;
			}
			set {
				base.Name = value;
			}
		}

		[PropertyChanged.DoNotNotify]
		[JsonIgnore]
		public Project Project {
			get;
			set;
		}

		/// <summary>
		/// The <see cref="EventType"/> in wich this event is tagged
		/// </summary>
		[PropertyPreload]
		[PropertyIndex (1)]
		public EventType EventType {
			get;
			set;
		}

		/// <summary>
		/// Event notes
		/// </summary>
		public string Notes {
			get;
			set;
		}

		/// <summary>
		/// Whether this event is currently selected.
		/// </summary>
		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool Selected {
			get;
			set;
		}

		/// <summary>
		/// List of drawings for this event
		/// </summary>
		public ObservableCollection<FrameDrawing> Drawings {
			get {
				return drawings;
			}
			set {
				if (drawings != null) {
					drawings.CollectionChanged -= ListChanged;
				}
				drawings = value;
				if (drawings != null) {
					drawings.CollectionChanged += ListChanged;
				}
			}
		}

		/// <summary>
		/// Whether this event has at least one <see cref="FrameDrawing"/>
		/// </summary>
		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool HasDrawings {
			get {
				return Drawings.Count > 0;
			}
		}

		/// <summary>
		/// Position of this event in the field.
		/// </summary>
		/// <value>The field position.</value>
		public Coordinates FieldPosition {
			get;
			set;
		}

		/// <summary>
		/// Position of this event in the half field.
		/// </summary>
		public Coordinates HalfFieldPosition {
			get;
			set;
		}

		/// <summary>
		/// Position of this event in the goal.
		/// </summary>
		public Coordinates GoalPosition {
			get;
			set;
		}

		/// <summary>
		/// An opaque object used by the view to describe the cameras layout.
		/// </summary>
		public object CamerasLayout {
			get;
			set;
		}

		/// <summary>
		/// A list of visible <see cref="CameraConfig"/> for this event.
		/// </summary>
		public ObservableCollection<CameraConfig> CamerasConfig {
			get {
				return camerasConfig;
			}
			set {
				if (camerasConfig != null) {
					camerasConfig.CollectionChanged -= ListChanged;
				}
				camerasConfig = value;
				if (camerasConfig != null) {
					camerasConfig.CollectionChanged += ListChanged;
				}
			}
		}

		/// <summary>
		/// Gets or sets the file set for this event.
		/// </summary>
		/// <value>The file set.</value>
		public MediaFileSet FileSet {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public virtual string Description {
			get {
				return 
					(Name + "\n" +
				TagsDescription () + "\n" +
				TimesDesription ());
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public virtual Color Color {
			get {
				return EventType.Color;
			}
		}

		#endregion

		#region Public methods

		protected void CheckIsLoaded ()
		{
			if (!IsLoaded && !IsLoading) {
				IsLoading = true;
				if (Storage == null) {
					throw new StorageException ("Storage not set in preloaded object");
				}
				Storage.Fill (this);
				IsLoaded = true;
				IsLoading = false;
			}
		}

		/// <summary>
		/// List of tags describing this event.
		/// </summary>
		/// <value>The tags.</value>
		public ObservableCollection<Tag> Tags {
			get {
				return tags;
			}
			set {
				if (tags != null) {
					tags.CollectionChanged -= ListChanged;
				}
				tags = value;
				if (tags != null) {
					tags.CollectionChanged += ListChanged;
				}
			}
		}

		public string TagsDescription ()
		{
			return String.Join ("-", Tags.Select (t => t.Value));
		}

		public string TimesDesription ()
		{
			if (Start != null && Stop != null) {
				if (Rate != 1) {
					return Start.ToMSecondsString () + " - " + Stop.ToMSecondsString () + " (" + RateString + ")";
				} else {
					return Start.ToMSecondsString () + " - " + Stop.ToMSecondsString ();
				}
			} else if (EventTime != null) {
				return EventTime.ToMSecondsString ();
			} else {
				return "";
			}
		}

		public void UpdateMiniature ()
		{
			if (Drawings.Count == 0) {
				Miniature = null;
			} else {
				Miniature = Drawings [0].Miniature;
			}
		}

		public void AddDefaultPositions ()
		{
			if (EventType.TagFieldPosition) {
				if (FieldPosition == null) {
					FieldPosition = new Coordinates ();
					FieldPosition.Points.Add (new Point (0.5, 0.5));
					if (EventType.FieldPositionIsDistance) {
						FieldPosition.Points.Add (new Point (0.5, 0.1));
					}
				}
			}
			if (EventType.TagHalfFieldPosition) {
				if (HalfFieldPosition == null) {
					HalfFieldPosition = new Coordinates ();
					HalfFieldPosition.Points.Add (new Point (0.5, 0.5));
					if (EventType.HalfFieldPositionIsDistance) {
						HalfFieldPosition.Points.Add (new Point (0.5, 0.1));
					}
				}
			}
			
			if (EventType.TagGoalPosition) {
				if (GoalPosition == null) {
					GoalPosition = new Coordinates ();
					GoalPosition.Points.Add (new Point (0.5, 0.5));
				}
			}
		}

		public Coordinates CoordinatesInFieldPosition (FieldPositionType pos)
		{
			switch (pos) {
			case FieldPositionType.Field:
				return FieldPosition;
			case FieldPositionType.HalfField:
				return HalfFieldPosition;
			case FieldPositionType.Goal:
				return GoalPosition;
			}
			return null;
		}

		public void UpdateCoordinates (FieldPositionType pos, ObservableCollection<Point> points)
		{
			Coordinates co = new Coordinates ();
			co.Points = points;
			
			switch (pos) {
			case FieldPositionType.Field:
				FieldPosition = co;
				break;
			case FieldPositionType.HalfField:
				HalfFieldPosition = co;
				break;
			case FieldPositionType.Goal:
				GoalPosition = co;
				break;
			}
		}

		// Prevents the deprecated Team field to be serialized, but allowing the field to be deserialized
		// for migrations.
		public bool ShouldSerializeTeam ()
		{
			return false;
		}

		public override string ToString ()
		{
			return Name;
		}

		public override bool Equals (object obj)
		{
			TimelineEvent evt = obj as TimelineEvent;
			if (evt == null)
				return false;
			return ID.Equals (evt.ID);
		}

		public override int GetHashCode ()
		{
			return ID.GetHashCode ();
		}

		#endregion

		protected void ListChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}
	}
}
