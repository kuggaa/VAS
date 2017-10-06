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
		ISurface trimHandleLeftActive, trimHandleLeftInactive;
		ISurface trimHandleRightActive, trimHandleRightInactive;

		public TimeNodeEditorView ()
		{
			trimHandleLeftActive = App.Current.DrawingToolkit.CreateSurfaceFromIcon (StyleConf.PlayerControlTrimLeftActive);
			trimHandleLeftInactive = App.Current.DrawingToolkit.CreateSurfaceFromIcon (StyleConf.PlayerControlTrimLeftInactive);
			trimHandleRightActive = App.Current.DrawingToolkit.CreateSurfaceFromIcon (StyleConf.PlayerControlTrimRightActive);
			trimHandleRightInactive = App.Current.DrawingToolkit.CreateSurfaceFromIcon (StyleConf.PlayerControlTrimRightInactive);
		}

		public TimeNodeVM ViewModel {
			get {
				return TimeNode;
			}
			set {
				TimeNode = value;
			}
		}

		double HandleSize {
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
			ISurface leftHandle = trimHandleLeftInactive;
			ISurface rightHandle = trimHandleRightInactive;

			if (!UpdateDrawArea (tk, area, Area)) {
				return;
			}

			tk.Begin ();

			if (ViewModel.SelectedHandle == SelectionPosition.Left) {
				leftHandle = trimHandleLeftActive;
			} else if (ViewModel.SelectedHandle == SelectionPosition.Right) {
				rightHandle = trimHandleRightActive;
			}

			// We need to add 2 pixels so that the handles match perfectly
			tk.DrawSurface (new Point (StartX - 2, 0), HandleSize, HandleSize, leftHandle, ScaleMode.AspectFit);
			tk.DrawSurface (new Point (StopX - Height + 2, 0), HandleSize, HandleSize, rightHandle, ScaleMode.AspectFit);

			tk.End ();
		}

		public override void ClickPressed (Point p, ButtonModifier modif, Selection selection)
		{
			base.ClickPressed (p, modif, selection);
			if (selection.Position == SelectionPosition.Left || selection.Position == SelectionPosition.Right) {
				ViewModel.SelectedHandle = selection.Position;
			}
		}
	}
}
