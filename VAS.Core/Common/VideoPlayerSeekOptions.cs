//
//  Copyright (C) 2017 Fluendo S.A.
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
using VAS.Core.Store;

namespace VAS.Core.Common
{
	/// <summary>
	/// Video player seek options for <see cref="VAS/VAS.Core/ViewModels/VideoPlayerVM/Seek"/>
	/// Default parameter values are: Accurate = false, Synchronous = false, Throttled = false
	/// </summary>
	public class VideoPlayerSeekOptions
	{
		public VideoPlayerSeekOptions (Time time, bool accurate = false, bool synchronous = false, bool throttled = false)
		{
			Time = time;
			Accurate = accurate;
			Synchronous = synchronous;
			Throttled = throttled;
		}

		public Time Time {
			get;
			set;
		}

		public bool Accurate {
			get;
			set;
		}

		public bool Synchronous {
			get;
			set;
		}

		public bool Throttled {
			get;
			set;
		}
	}
}
