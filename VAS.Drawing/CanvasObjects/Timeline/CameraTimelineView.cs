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
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;

namespace VAS.Drawing.CanvasObjects.Timeline
{
	/// <summary>
	/// A row timeline that  timeline view.
	/// </summary>
	[View ("CameraTimelineView")]
	public class CameraTimelineView : TimelineView, ICanvasObjectView<MediaFileVM>
	{
		MediaFileVM viewModel;

		public MediaFileVM ViewModel {
			get {
				return viewModel;
			}
			set {
				SetMediaFile (value);
			}
		}

		public Color LineColor {
			get;
			set;
		}

		public bool ShowLine {
			get;
			set;
		}

		public bool ShowName {
			get;
			set;
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (MediaFileVM)viewModel;
		}

		public void SetMediaFile (MediaFileVM mediaFile)
		{
			var cameraNodeView = new CameraView ();
			cameraNodeView.ViewModel = mediaFile;
			cameraNodeView.OffsetY = OffsetY;
			cameraNodeView.Height = Height;
			cameraNodeView.SecondsPerPixel = SecondsPerPixel;
			cameraNodeView.MaxTime = Duration;
			cameraNodeView.ShowName = ShowName;
			cameraNodeView.LineColor = LineColor;
			AddNode (cameraNodeView);
		}
	}
}
