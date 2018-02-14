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

using System;
using VAS.Core.Common;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Core.Interfaces.Services
{
	/// <summary>
	/// Service to manage drawings over video frames.
	/// </summary>
	public interface IDrawingsService
	{
		/// <summary>
		/// Draws the frame.
		/// </summary>
		/// <param name="play">When set in combination with <see cref="drawingIndex"/> it will use the drawing stored
		/// in the event to edit it.</param>
		/// <param name="drawingIndex">When set in combination with <see cref="Play"/> it defines the index of
		/// the event's drawing to use.</param>
		/// <param name="cameraConfig">Camera config.</param>
		/// <param name="frame">the frame to draw on. If <c>null</c> a new <see cref="IFramesCapturer"/> will be used
		/// to retrieve the video frame</param>
		void DrawFrame(VideoPlayerVM videoPlayer, ProjectVM project, TimelineEventVM play, int drawingIndex, CameraConfig cameraConfig, Image frame);

		/// <summary>
		/// Creates a series of snapshots from an event.
		/// </summary>
		/// <param name="timelineEvent">Timeline event.</param>
		void CreateSnapshotSeries(VideoPlayerVM videoPlayer, TimelineEventVM timelineEvent);
	}
}
