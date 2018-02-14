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
using VAS.Core.Interfaces;
using VAS.Core.Store;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// Class for the TimedDashBoardButton ViewModel.
	/// </summary>
	public class TimedDashboardButtonVM : DashboardButtonVM, ITimed
	{
		Time currentTime;

		/// <summary>
		/// Gets the correctly Typed Model
		/// </summary>
		/// <value>The button.</value>
		public TimedDashboardButton TypedModel {
			get {
				return (TimedDashboardButton)base.Model;
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
			}
		}

		/// <summary>
		/// Gets or sets the tag mode.
		/// </summary>
		/// <value>The tag mode.</value>
		public TagMode TagMode {
			get {
				return TypedModel.TagMode;
			}
			set {
				TypedModel.TagMode = value;
			}
		}

		/// <summary>
		/// Gets or sets the start time.
		/// </summary>
		/// <value>The start.</value>
		public Time Start {
			get {
				return TypedModel.Start;
			}
			set {
				TypedModel.Start = value;
			}
		}

		/// <summary>
		/// Gets or sets the stop time.
		/// </summary>
		/// <value>The stop.</value>
		public Time Stop {
			get {
				return TypedModel.Stop;
			}
			set {
				TypedModel.Stop = value;
			}
		}

		public Time RecordingStart {
			get;
			set;
		}

		[PropertyChanged.DoNotNotify]
		public Time CurrentTime {
			get {
				return currentTime;
			}
			set {
				if (value != null && Mode != DashboardMode.Edit
					&& RecordingStart != null && TagMode != TagMode.Predefined) {
					if (value.MSeconds + Constants.PLAYBACK_TOLERANCE < RecordingStart.MSeconds) {
						ButtonTime = null;
					} else if (currentTime != null &&
						currentTime.TotalSeconds != value.TotalSeconds) {
						ButtonTime = value - RecordingStart;
					}
				}
				currentTime = value;
			}
		}

		public Time ButtonTime {
			get;
			set;
		}
	}
}
