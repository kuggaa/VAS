//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System.ComponentModel;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Drawing.CanvasObjects.Timeline
{
	/// <summary>
	/// A view to display camera boundaries and adjust the offset of the camera.
	/// </summary>
	[View ("CameraView")]
	public class CameraView : TimeNodeView, ICanvasObjectView<MediaFileVM>
	{
		MediaFileVM viewModel;

		public CameraView ()
		{
			// Video boundaries can't be changed, only the segment can move.
			DraggingMode = NodeDraggingMode.Segment;
			SelectionMode = NodeSelectionMode.Segment;
			ClippingMode = NodeClippingMode.LeftStrict;
		}

		public MediaFileVM ViewModel {
			get {
				return viewModel;
			}
			set {
				if (TimeNode != null) {
					TimeNode.PropertyChanged -= HandleChildNodePropertyChanged;
				}
				if (ViewModel != null) {
					ViewModel.PropertyChanged -= HandleVMPropertyChanged;
				}
				viewModel = value;
				if (viewModel != null) {
					TimeNode = new TimeNodeVM {
						Model = new TimeNode {
							Start = new Time (-viewModel.Offset.MSeconds),
							Stop = viewModel.Duration - viewModel.Offset, Name = viewModel.Name
						}
					};
					TimeNode.PropertyChanged += HandleChildNodePropertyChanged;
					ViewModel.PropertyChanged += HandleVMPropertyChanged;
				}
			}
		}

		/// <summary>
		/// Gets the description.
		/// </summary>
		/// <value>The description.</value>
		public override string Description {
			get {
				return ViewModel?.Name;
			}
		}

		public override Time MaxTime {
			get {
				return ViewModel?.Duration;
			}
		}

		public override Area Area {
			get {
				return new Area (new Point (StartX, OffsetY), (StopX - StartX), Height);
			}
		}

		public override bool Selected {
			get {
				return base.Selected;
			}
			set {
				base.Selected = value;
				ViewModel.SelectedGrabber = value ? SelectionPosition.All : SelectionPosition.None;
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (MediaFileVM)viewModel;
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			if (!UpdateDrawArea (tk, area, Area)) {
				return;
			}

			tk.Begin ();

			tk.StrokeColor = App.Current.Style.PaletteBackgroundDark;
			if (Selected) {
				tk.FillColor = App.Current.Style.PaletteActive;
			} else {
				tk.FillColor = LineColor;
			}
			tk.LineWidth = 1;

			tk.DrawRoundedRectangle (new Point (StartX, OffsetY), StopX - StartX, Height, 5);

			if (ShowName) {
				tk.FontSize = 16;
				tk.FontWeight = FontWeight.Bold;
				tk.FillColor = App.Current.Style.PaletteActive;
				tk.StrokeColor = App.Current.Style.PaletteActive;
				tk.DrawText (new Point (StartX, OffsetY), StopX - StartX,
					Height - StyleConf.TimelineLineSize,
					TimeNode.Name);
			}
			tk.End ();
		}

		void HandleChildNodePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (TimeNodeVM.Start)) {
				viewModel.Offset = new Time (-TimeNode.Start.MSeconds);
			}
		}

		void HandleVMPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (ViewModel.NeedsSync (e, nameof (ViewModel.Offset))) {
				ReDraw ();
			}
		}
	}
}

