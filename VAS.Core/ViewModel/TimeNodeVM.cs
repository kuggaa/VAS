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
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Core.ViewModel
{
	public class TimeNodeVM : ViewModelBase<TimeNode>
	{

		/// <summary>
		/// Gets or sets the name.
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
	}
}
