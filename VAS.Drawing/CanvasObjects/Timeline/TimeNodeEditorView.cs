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
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;
using VAS.Core.Resources;
using VAS.Core.Store.Drawables;
using VAS.Core.ViewModel;

namespace VAS.Drawing.CanvasObjects.Timeline
{
	/// <summary>
	/// A View with a timerule and a time node with the currently loaded <see cref="IPlaylistPlayElement"/>
	/// to change the duration of the event by moving its boundaries.
	/// </summary>
	[View ("TimeNodeEditorView")]
	public class TimeNodeEditorView : TimeNodeView, ICanvasObjectView<TimeNodeVM>
	{
		ISurface trimGrabberLeftActive, trimGrabberLeftInactive;
		ISurface trimGrabberRightActive, trimGrabberRightInactive;

		public TimeNodeEditorView ()
		{
			trimGrabberLeftActive = App.Current.DrawingToolkit.CreateSurfaceFromIcon (Icons.PlayerControlTrimLeftActive);
			trimGrabberLeftInactive = App.Current.DrawingToolkit.CreateSurfaceFromIcon (Icons.PlayerControlTrimLeftInactive);
			trimGrabberRightActive = App.Current.DrawingToolkit.CreateSurfaceFromIcon (Icons.PlayerControlTrimRightActive);
			trimGrabberRightInactive = App.Current.DrawingToolkit.CreateSurfaceFromIcon (Icons.PlayerControlTrimRightInactive);
		}

		public TimeNodeVM ViewModel {
			get {
				return TimeNode;
			}
			set {
				TimeNode = value;
			}
		}

		double GrabberSize {
			get {
				return Height;
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (TimeNodeVM)viewModel;
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			ISurface leftGrabber = trimGrabberLeftInactive;
			ISurface rightGrabber = trimGrabberRightInactive;

			if (!UpdateDrawArea (tk, area, Area)) {
				return;
			}

			tk.Begin ();

			if (ViewModel.SelectedGrabber == SelectionPosition.Left) {
				leftGrabber = trimGrabberLeftActive;
			} else if (ViewModel.SelectedGrabber == SelectionPosition.Right) {
				rightGrabber = trimGrabberRightActive;
			}

			// We need to add 2 pixels so that the Grabbers match perfectly
			tk.DrawSurface (new Point (StartX - 2, 0), GrabberSize, GrabberSize, leftGrabber, ScaleMode.AspectFit);
			tk.DrawSurface (new Point (StopX - Height + 2, 0), GrabberSize, GrabberSize, rightGrabber, ScaleMode.AspectFit);

			tk.End ();
		}

		public override void ClickPressed (Point p, ButtonModifier modif, Selection selection)
		{
			base.ClickPressed (p, modif, selection);
			if (selection.Position == SelectionPosition.Left || selection.Position == SelectionPosition.Right) {
				ViewModel.SelectedGrabber = selection.Position;
			}
		}
	}
}
