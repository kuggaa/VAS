// Project.cs
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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using VAS.Core.Common;
using VAS.Core.Serialization;
using VAS.Core.Store.Playlists;
using VAS.Core.Store.Templates;

namespace VAS.Core.Store
{

	/// <summary>
	/// I hold the information needed by a project and provide persistency
	/// </summary>
	///
	[Serializable]
	abstract public class Project : StorableBase, IComparable, IDisposable
	{
		public const int CURRENT_VERSION = 1;
		ObservableCollection<TimelineEvent> timeline;
		ObservableCollection<Period> periods;
		ObservableCollection<Timer> timers;
		ObservableCollection<Playlist> playlists;
		ObservableCollection<EventType> eventTypes;
		DateTime lastModified;

		#region Constructors


		public Project ()
		{
			ID = System.Guid.NewGuid ();
			Timeline = new ObservableCollection<TimelineEvent> ();
			Timers = new ObservableCollection<Timer> ();
			Periods = new ObservableCollection<Period> ();
			Playlists = new ObservableCollection<Playlist> ();
			EventTypes = new ObservableCollection<EventType> ();
			Version = Constants.DB_VERSION;
			LastModified = DateTime.Now;
		}

		[OnDeserialized ()]
		internal void OnDeserializedMethod (StreamingContext context)
		{
			foreach (TimelineEvent evt in Timeline) {
				evt.Project = this;
			}
		}

		public virtual void Dispose ()
		{
			Dashboard?.Dispose ();
			foreach (TimelineEvent evt in Timeline) {
				evt.Dispose ();
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Media file asigned to this project
		/// </summary>
		virtual public MediaFileSet FileSet {
			get;
			set;
		}

		/// <value>
		/// Document version
		/// </value>
		[DefaultValue (0)]
		[JsonProperty (DefaultValueHandling = DefaultValueHandling.Populate)]
		public int Version {
			get;
			set;
		}

		public ObservableCollection<TimelineEvent> Timeline {
			get {
				return timeline;
			}
			set {
				if (timeline != null) {
					timeline.CollectionChanged -= ListChanged;
				}
				timeline = value;
				if (timeline != null) {
					timeline.CollectionChanged += ListChanged;
				}
			}
		}

		[JsonProperty (Order = -7)]
		public ObservableCollection<EventType> EventTypes {
			get {
				return eventTypes;
			}
			set {
				if (eventTypes != null) {
					eventTypes.CollectionChanged -= ListChanged;
				}
				eventTypes = value;
				if (eventTypes != null) {
					eventTypes.CollectionChanged += ListChanged;
				}
			}
		}

		/// <value>
		/// Categories template
		/// </value>
		[JsonProperty (Order = -10)]
		public Dashboard Dashboard {
			get;
			set;
		}

		public ObservableCollection<Period> Periods {
			get {
				return periods;
			}
			set {
				if (periods != null) {
					periods.CollectionChanged -= ListChanged;
				}
				periods = value;
				if (periods != null) {
					periods.CollectionChanged += ListChanged;
				}
			}
		}

		public ObservableCollection<Timer> Timers {
			get {
				return timers;
			}
			set {
				if (timers != null) {
					timers.CollectionChanged -= ListChanged;
				}
				timers = value;
				if (timers != null) {
					timers.CollectionChanged += ListChanged;
				}
			}
		}

		public ObservableCollection<Playlist> Playlists {
			get {
				return playlists;
			}
			set {
				if (playlists != null) {
					playlists.CollectionChanged -= ListChanged;
				}
				playlists = value;
				if (playlists != null) {
					playlists.CollectionChanged += ListChanged;
				}
			}
		}

		/// <summary>
		/// Date of project last modification
		/// </summary>
		[PropertyPreload]
		public virtual DateTime LastModified {
			get {
				return lastModified;
			}
			set {
				lastModified = value.ToUniversalTime ();
			}
		}


		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public IEnumerable<IGrouping<EventType, TimelineEvent>> EventsGroupedByEventType {
			get {
				return Timeline.GroupBy (play => play.EventType);
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public ProjectType ProjectType {
			get;
			set;
		}

		/// <summary>
		/// Gets a short description of the project.
		/// </summary>
		/// <value>The short description.</value>
		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public virtual string ShortDescription {
			get;
		}

		public virtual bool IsFakeCapture {
			get;
			set;
		}

		#endregion

		#region Public Methods

		public abstract TimelineEvent AddEvent (EventType type, Time start, Time stop, Time eventTime, Image miniature,
		                                        bool addToTimeline = true);

		public abstract void AddEvent (TimelineEvent play);

		/// <summary>
		/// Delete a play from the project
		/// </summary>
		/// <param name="tNode">
		/// A <see cref="MediaTimeNode"/>: play to be deleted
		/// </param>
		/// <param name="section">
		/// A <see cref="System.Int32"/>: category the play belongs to
		/// </param>
		public void RemoveEvents (List<TimelineEvent> plays)
		{
			foreach (TimelineEvent play in plays) {
				Timeline.Remove (play);
			}
		}

		public void CleanupTimers ()
		{
			foreach (Timer t in Timers) {
				t.Nodes.RemoveAll (tn => tn.Start == null || tn.Stop == null);
			}
		}

		virtual public void UpdateEventTypesAndTimers ()
		{
			IEnumerable<EventType> dashboardtypes = new List<EventType> ();
			IEnumerable<EventType> timelinetypes;

			if (Dashboard != null) {
				/* Timers */
				IEnumerable<Timer> timers = Dashboard.List.OfType<TimerButton> ().Select (b => b.Timer).OfType<Timer> ();
				Timers.AddRange (timers.Except (Timers));

				/* Update event types list that changes when the user adds or remove a
				 * a new button to the dashboard or after importing a project with events
				 * tagged with a different dashboard */
				dashboardtypes = Dashboard.List.OfType<EventButton> ().Select (b => b.EventType);
			}

			/* Remove event types that have no events and are not in the dashboard anymore */
			foreach (EventType evt in EventTypes.Except (dashboardtypes).ToList ()) {
				if (Timeline.Count (e => e.EventType == evt) == 0) {
					EventTypes.Remove (evt);
				}
			}
			EventTypes.AddRange (dashboardtypes.Except (EventTypes));
			timelinetypes = Timeline.Select (t => t.EventType).Distinct ().Except (EventTypes);
			EventTypes.AddRange (timelinetypes.Except (EventTypes));

			/* Remove null EventTypes just in case */
			EventTypes = new ObservableCollection<EventType> (EventTypes.Where (e => e != null));						
		}

		public List<TimelineEvent> EventsByType (EventType evType)
		{
			return Timeline.Where (p => p.EventType.ID == evType.ID).ToList ();
		}

		public Image GetBackground (FieldPositionType pos)
		{
			switch (pos) {
			case FieldPositionType.Field:
				return Dashboard.FieldBackground;
			case FieldPositionType.HalfField:
				return Dashboard.HalfFieldBackground;
			case FieldPositionType.Goal:
				return Dashboard.GoalBackground;
			}
			return null;
		}

		/// <summary>
		/// Resynchronize events with the periods synced with the video file.
		/// Imported projects or fake analysis projects create events assuming periods
		/// don't have gaps between them.
		/// After adding a file to the project and synchronizing the periods with the
		/// video file, all events must be offseted with the new start time of the period.
		/// 
		/// Before sync:
		///   Period 1: start=00:00:00 Period 2: start=00:30:00
		///   evt1 00:10:00            evt2 00:32:00
		/// After sync:
		///   Period 1: start=00:05:00 Period 2: start= 00:39:00
		///   evt1 00:15:00            evt2 00:41:00
		/// </summary>
		/// <param name="periods">The new periods syncrhonized with the video file.</param>
		public void ResyncEvents (IList<Period> periods)
		{
			ObservableCollection<TimelineEvent> newTimeline = new ObservableCollection<TimelineEvent> ();

			if (periods.Count != Periods.Count) {
				throw new IndexOutOfRangeException (
					"Periods count is different from the project's ones");
			}

			for (int i = 0; i < periods.Count; i++) {
				Period oldPeriod = Periods [i];
				TimeNode oldTN = oldPeriod.PeriodNode;
				TimeNode newTN = periods [i].PeriodNode;
				Time diff = newTN.Start - oldTN.Start;

				/* Find the events in this period */
				var periodEvents = Timeline.Where (e =>
					e.EventTime >= oldTN.Start &&
				                   e.EventTime <= oldTN.Stop).ToList ();

				/* Apply new offset and move the new timeline so that the next
				 * iteration for the following period does not use them anymore */
				periodEvents.ForEach (e => {
					e.Move (diff);
					newTimeline.Add (e);
					Timeline.Remove (e);
				});
				foreach (TimeNode tn in oldPeriod.Nodes) {
					tn.Move (diff);
				}
			}
			Timeline = newTimeline;
		}

		public int CompareTo (object obj)
		{
			if (obj is Project) {
				Project project = (Project)obj;
				return ID.CompareTo (project.ID);
			} else
				throw new ArgumentException ("object is not a Project and cannot be compared");
		}

		public static void Export (Project project, string file)
		{
			file = Path.ChangeExtension (file, App.Current.ProjectExtension);
			Serializer.Instance.Save (project, file);
		}

		public static Project Import (string file)
		{
			Project project = null;
			try {
				project = Serializer.Instance.Load<Project> (file);
			} catch (Exception e) {
				Log.Exception (e);
				throw new Exception (Catalog.GetString ("The file you are trying to load " +
				"is not a valid project"));
			}
			return project;
		}

		void ListChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}

		#endregion
	}
}
