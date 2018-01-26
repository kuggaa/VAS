//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System.Runtime.Serialization;
using Newtonsoft.Json;
using VAS.Core.Common;
using VAS.Core.Serialization;

namespace VAS.Core.Store
{
	[Serializable]
	public class EventButton : TimedDashboardButton
	{
		public EventType EventType {
			get;
			set;
		}

		[CloneIgnoreAttribute]
		[JsonIgnore]
		public override string Name {
			get {
				return EventType != null ? EventType.Name : null;
			}
			set {
				if (EventType != null) {
					EventType.Name = value;
				}
			}
		}

		[CloneIgnoreAttribute]
		[JsonIgnore]
		public override Color BackgroundColor {
			get {
				return EventType != null ? EventType.Color : null;
			}
			set {
				if (EventType != null) {
					EventType.Color = value;
				}
			}
		}

		[OnDeserialized ()]
		internal void OnDeserializedMethod (StreamingContext context)
		{
			if (EventType != null) {
				EventType.IsChanged = false;
			}
		}
	}
}

