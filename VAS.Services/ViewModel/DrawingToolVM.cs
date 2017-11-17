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
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Services.ViewModel
{
	/// <summary>
	/// ViewModel used in DrawingTool View
	/// </summary>
	public class DrawingToolVM : ViewModelBase
	{
		public DrawingToolVM ()
		{
			SetZoomCommand = new LimitationCommand<double> (VASFeature.Zoom.ToString (), SetZoom);
			ZoomLevel = 1;
		}

		/// <summary>
		/// Gets the zoom limited command
		/// </summary>
		/// <value>The set zoom.</value>
		public LimitationCommand<double> SetZoomCommand {
			get;
		}
		//FIXME: this should be migrated to MVVM ProjectVM, TimelineEventVM, etc.
		/// <summary>
		/// Gets or sets the project.
		/// </summary>
		/// <value>The project.</value>
		public virtual Project Project {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the timeline event.
		/// </summary>
		/// <value>The timeline event.</value>
		public TimelineEventVM TimelineEventVM {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the frame.
		/// </summary>
		/// <value>The frame.</value>
		public Image Frame {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the drawing.
		/// </summary>
		/// <value>The drawing.</value>
		public FrameDrawing Drawing {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the camera config.
		/// </summary>
		/// <value>The camera config.</value>
		public CameraConfig CameraConfig {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the zoom level.
		/// </summary>
		/// <value>The zoom level.</value>
		public double ZoomLevel {
			get;
			set;
		}

		/// <summary>
		/// Throws an event when a drawing has been saved to a project.
		/// </summary>
		public void DrawingSaved ()
		{
			App.Current.EventsBroker.Publish (new DrawingSavedToProjectEvent ());
		}

		void SetZoom (double zoom)
		{
			ZoomLevel = zoom;
		}
	}
}
