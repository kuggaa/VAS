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

		public CameraTimelineView ()
		{
			CameraNode = new CameraNodeView ();
		}

		public MediaFileVM ViewModel {
			get {
				return viewModel;
			}
			set {
				SetMediaFile (value);
			}
		}

		/// <summary>
		/// Gets or sets the view for the first camera node, the first <see cref="MediaFile"/>.
		/// </summary>
		/// <value>The camera node.</value>
		public CameraNodeView CameraNode {
			get;
			protected set;
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
			CameraNode.ViewModel = mediaFile;
			CameraNode.OffsetY = OffsetY;
			CameraNode.Height = Height;
			CameraNode.SecondsPerPixel = SecondsPerPixel;
			CameraNode.MaxTime = Duration;
			CameraNode.ShowName = ShowName;
			CameraNode.LineColor = LineColor;
			CameraNode = CameraNode;
			AddNode (CameraNode);
		}
	}
}
