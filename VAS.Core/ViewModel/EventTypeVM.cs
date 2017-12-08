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

using VAS.Core.Common;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// View model for <see cref="EventType"/> objects.
	/// </summary>
	public class EventTypeVM : ViewModelBase<EventType>
	{
		public EventTypeVM ()
		{
			Tags = new KeyUpdaterCollectionViewModel<Tag, TagVM> ();
		}

		public override EventType Model {
			get {
				return base.Model;
			}
			set {
				base.Model = value;
				var analysisEventType = base.Model as AnalysisEventType;
				if (analysisEventType != null) {
					Tags.Model = analysisEventType.Tags;
				}
			}
		}

		/// <summary>
		/// Gets or sets the name of the EventType.
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
		/// Gets or sets the color of the event type.
		/// </summary>
		/// <value>The color.</value>
		public Color Color {
			get {
				return Model.Color;
			}
			set {
				Model.Color = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.AnalysisEventButtonVM"/> locate event.
		/// </summary>
		/// <value><c>true</c> if locate event; otherwise, <c>false</c>.</value>
		public bool AllowLocation {
			get {
				return Model.AllowLocation;
			}
			set {
				Model.AllowLocation = value;
			}
		}

		/// <summary>
		/// Gets the possible subcategories of the event type
		/// </summary>
		/// <value>The subcategories.</value>
		public KeyUpdaterCollectionViewModel<Tag, TagVM> Tags {
			get;
		}
	}

}
