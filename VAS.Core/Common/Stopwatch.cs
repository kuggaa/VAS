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
using System;
using VAS.Core.Interfaces;
using VAS.Core.MVVMC;
using SystemStopwatch = System.Diagnostics.Stopwatch;

namespace VAS.Core.Common
{
	/// <summary>
	/// Wrapper for System.Diagnostics.Stopwatch
	/// </summary>
	[DependencyService (typeof (IStopwatch), 0)]
	public class Stopwatch : IStopwatch
	{
		/// <summary>
		/// The frequency of the timer as the number of ticks per second
		/// </summary>
		public static readonly long Frequency = SystemStopwatch.Frequency;

		SystemStopwatch stopwatch;

		public Stopwatch ()
		{
			stopwatch = new SystemStopwatch ();
		}

		/// <summary>
		/// Gets the elapsed milliseconds.
		/// </summary>
		/// <value>The elapsed milliseconds.</value>
		public long ElapsedMilliseconds {
			get {
				return stopwatch.ElapsedMilliseconds;
			}
		}

		/// <summary>
		/// Gets the elapsed ticks.
		/// </summary>
		/// <value>The elapsed ticks.</value>
		public long ElapsedTicks {
			get {
				return stopwatch.ElapsedTicks;
			}
		}

		/// <summary>
		/// Gets the elapsed seconds.
		/// </summary>
		/// <value>The elapsed seconds.</value>
		public double ElapsedSeconds {
			get {
				return (double)stopwatch.ElapsedTicks / Frequency;
			}
		}

		/// <summary>
		/// Check if the Stopwatch is running.
		/// </summary>
		/// <returns><c>true</c>, Stopwatch running <c>false</c> otherwise.</returns>
		public bool IsRunning {
			get {
				return stopwatch.IsRunning;
			}
		}

		/// <summary>
		/// Reset the Stopwatch.
		/// </summary>
		public void Reset ()
		{
			stopwatch.Reset ();
		}

		/// <summary>
		/// Start the Stopwatch.
		/// </summary>
		public void Start ()
		{
			stopwatch.Start ();
		}

		/// <summary>
		/// Stop the Stopwatch.
		/// </summary>
		public void Stop ()
		{
			stopwatch.Stop ();
		}
	}
}
