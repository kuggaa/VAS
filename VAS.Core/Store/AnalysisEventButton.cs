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
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace VAS.Core.Store
{
	[Serializable]
	public class AnalysisEventButton : EventButton
	{
		public AnalysisEventButton ()
		{
			TagsPerRow = 2;
			ShowSubcategories = true;
		}

		public bool ShowSubcategories {
			get;
			set;
		}

		public int TagsPerRow {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public AnalysisEventType AnalysisEventType {
			get {
				return EventType as AnalysisEventType;
			}
		}

		[OnDeserialized ()]
		internal new void OnDeserializedMethod (StreamingContext context)
		{
			base.OnDeserializedMethod (context);

			// update source action links with the tags instances in the button to spread changes
			// in both elements at the same time, without this when a tag is edited link is lost 
			// because it was not updated and it is considered a different one
			foreach (var link in ActionLinks) {
				var sourceTags = link.SourceTags.ToList ();
				link.SourceTags.IgnoreEvents = true;

				link.SourceTags.Clear ();
				foreach (var s in sourceTags) {
					var e = AnalysisEventType.Tags.FirstOrDefault (x => x.Equals (s));
					link.SourceTags.Add (e);
				}

				link.SourceTags.IgnoreEvents = false;
			}
		}
	}
}

