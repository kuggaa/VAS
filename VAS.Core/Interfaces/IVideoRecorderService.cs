//
//  Copyright (C) 2018 Fluendo S.A.
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
using System.Threading.Tasks;
using VAS.Core.Common;

namespace VAS.Core.Interfaces
{
	/// <summary>
	/// A service that records video from an input source.
	/// </summary>
	public interface IVideoRecorderService
	{

		/// <summary>
		/// Gets the current frame.
		/// </summary>
		/// <value>The current frame.</value>
		Image CurrentFrame { get; }

		/// <summary>
		///	Start the recording preview without recording.
		/// </summary>
		void Run ();

		/// <summary>
		/// Starts the recording, starting a new period if it was requested
		/// </summary>
		/// <param name="newPeriod">If set to <c>true</c>, a new period.</param>
		void StartRecording (bool newPeriod = true);

		/// <summary>
		/// Stops recording.
		/// </summary>
		void StopRecording ();

		/// <summary>
		/// Pauses the clock, without stopping the recording.
		/// </summary>
		void PauseClock ();

		/// <summary>
		/// Resumes the clock.
		/// </summary>
		void ResumeClock ();

		/// <summary>
		/// Close the recording session.
		/// </summary>
		void Close ();
	}
}
