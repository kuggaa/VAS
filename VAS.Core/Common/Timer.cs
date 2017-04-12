//
//  Copyright (C) 2017 FLUENDO S.A.
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
using System.Timers;
using VAS.Core.Interfaces;
using SystemTimer = System.Timers.Timer;

namespace VAS.Core.Common
{
	/// <summary>
	/// Timer wrapper to add an interface
	/// </summary>
	public class Timer : ITimer
	{
		SystemTimer timer;

		public Timer ()
		{
			timer = new SystemTimer ();
		}

		/// <summary>
		/// Eent raised when the time assigned to the interval has expired
		/// </summary>
		public event ElapsedEventHandler Elapsed {
			add { timer.Elapsed += value; }
			remove { timer.Elapsed -= value; }
		}

		/// <summary>
		/// Gets a value indicating whether timer is running
		/// </summary>
		/// <value><c>true</c> if timer running; otherwise, <c>false</c>.</value>
		public bool Enabled { get { return timer.Enabled; } }

		/// <summary>
		/// Gets or sets the interval.
		/// </summary>
		/// <value>The interval.</value>
		public double Interval {
			get {
				return timer.Interval;
			}

			set {
				timer.Interval = value;
			}
		}

		/// <summary>
		/// Start the timer.
		/// </summary>
		public void Start ()
		{
			timer.Start ();
		}

		/// <summary>
		/// Stop the timer.
		/// </summary>
		public void Stop ()
		{
			timer.Stop ();
		}

		/// <summary>
		/// Releases all resource used by the <see cref="T:VAS.Core.Interfaces.ITimer"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="T:VAS.Core.Interfaces.ITimer"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="T:VAS.Core.Interfaces.ITimer"/> in an unusable state. After
		/// calling <see cref="Dispose"/>, you must release all references to the <see cref="T:VAS.Core.Interfaces.ITimer"/>
		/// so the garbage collector can reclaim the memory that the <see cref="T:VAS.Core.Interfaces.ITimer"/> was occupying.</remarks>
		public void Dispose ()
		{
			timer.Dispose ();
		}
	}
}
