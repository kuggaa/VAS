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
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Common;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// ViewModel for <see cref="TimeNode"/> objects.
	/// </summary>
	public class TimeNodeVM : PlayableElementVM<TimeNode>, IVisible
	{
		public TimeNodeVM ()
		{
			Visible = true;
		}

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public virtual string Name {
			get {
				return Model.Name;
			}
			set {
				Model.Name = value;
			}
		}

		/// <summary>
		/// Gets or sets the start Time
		/// </summary>
		public Time Start {
			get {
				return Model.Start;
			}
			set {
				Model.Start = value;
			}
		}

		/// <summary>
		/// Gets or sets the stop time
		/// </summary>
		public Time Stop {
			get {
				return Model.Stop;
			}
			set {
				Model.Stop = value;
			}
		}

		/// <summary>
		/// Get or set the event time
		/// </summary>
		public Time EventTime {
			get {
				return Model.EventTime;
			}
			set {
				Model.EventTime = value;
			}
		}

		/// <summary>
		/// Gets the duration.
		/// </summary>
		/// <value>The duration.</value>
		public Time Duration {
			get {
				return Model.Duration;
			}
		}

		/// <summary>
		/// Play rate
		/// </summary>
		public virtual float Rate {
			get {
				return Model.Rate;
			}
			set {
				Model.Rate = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.TimeNodeVM"/> is visible.
		/// </summary>
		/// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
		public virtual bool Visible {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the selected grabber, which can be Left, Right or All.
		/// </summary>
		/// <value>The selected grabber.</value>
		public SelectionPosition SelectedGrabber {
			get;
			set;
		}
	}
}
