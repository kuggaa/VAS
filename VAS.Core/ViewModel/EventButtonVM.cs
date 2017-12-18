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
using System.Collections.Generic;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Store;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// Class for the EventButton ViewModel
	/// </summary>
	public class EventButtonVM : TimedDashboardButtonVM
	{
		public EventButtonVM ()
		{
			EventType = new EventTypeVM ();
		}

		/// <summary>
		/// Gets the correctly Typed Model
		/// </summary>
		/// <value>The button.</value>
		public new EventButton TypedModel {
			get {
				return (EventButton)base.Model;
			}
		}

		/// <summary>
		/// Gets or sets the model.
		/// </summary>
		/// <value>The model.</value>
		[PropertyChanged.DoNotCheckEquality]
		public override DashboardButton Model {
			get {
				return TypedModel;
			}
			set {
				base.Model = value;
				EventType.Model = ((EventButton)value)?.EventType;
			}
		}

		/// <summary>
		/// Gets the type of the event.
		/// </summary>
		/// <value>The type of the event.</value>
		public EventTypeVM EventType {
			get;
			private set;
		}

		public virtual void Click ()
		{
			if (Mode == DashboardMode.Edit) {
				return;
			}

			Time start, stop, eventTime;

			if (TagMode == TagMode.Predefined || RecordingStart == null) {
				start = CurrentTime - Start;
				stop = CurrentTime + Stop;
				eventTime = CurrentTime;
			} else {
				stop = CurrentTime;
				start = RecordingStart - Start;
				eventTime = RecordingStart;
			}

			App.Current.EventsBroker.Publish (new NewTagEvent {
				EventType = TypedModel.EventType,
				Start = start,
				Stop = stop,
				EventTime = eventTime,
				Button = Model,
				Tags = GetTags ()
			});

			RecordingStart = null;
			ButtonTime = null;
		}

		protected virtual IEnumerable<Tag> GetTags ()
		{
			return new List<Tag> ();
		}
	}
}
