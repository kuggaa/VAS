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
using System.ComponentModel;
using System.Linq;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// A ViewModel for <see cref="MediaFileSet"/>.
	/// </summary>
	public class MediaFileSetVM : CollectionViewModel<MediaFile, MediaFileVM>, IViewModel<MediaFileSet>
	{
		MediaFileSet model;

		public MediaFileSetVM ()
		{
			VisibleRegion = new TimeNodeVM ();
		}

		public new MediaFileSet Model {
			get {
				return model;
			}
			set {
				model = value;
				base.Model = model.MediaFiles;
				VisibleRegion.Model = model?.VisibleRegion ??
					new TimeNode () { Start = new Time (0), Stop = new Time (0) };
			}
		}

		/// <summary>
		/// Gets the duration of the media file set.
		/// </summary>
		/// <value>The duration.</value>
		public Time Duration {
			get {
				return Model?.Duration;
			}
		}

		/// <summary>
		/// Gets the duration of the media taking in account if it's stretched or not
		/// </summary>
		/// <value>The duration.</value>
		public Time VirtualDuration {
			get {
				if (IsStretched) {
					return VisibleRegion.Duration;
				} else {
					return Model.Duration;
				}
			}
		}

		/// <summary>
		/// Gets or sets the visible region.
		/// </summary>
		/// <value>The visible region.</value>
		public TimeNodeVM VisibleRegion {
			get;
			protected set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.MediaFileSetVM"/> is stretched
		/// and only the visible region is used.
		/// </summary>
		/// <value><c>true</c> if is stretched; otherwise, <c>false</c>.</value>
		public bool IsStretched {
			get {
				return Model.IsStretched;
			}
			set {
				Model.IsStretched = value;
			}
		}

		/// <summary>
		/// Gets the file path.
		/// </summary>
		/// <value>The file path.</value>
		public string FilePath {
			get {
				return (Model.FirstOrDefault ()).FilePath;
			}
		}

		protected override void ForwardPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "VisibleRegion") {
				VisibleRegion.Model = Model.VisibleRegion;
			}
			base.ForwardPropertyChanged (sender, e);
		}
	}
}
