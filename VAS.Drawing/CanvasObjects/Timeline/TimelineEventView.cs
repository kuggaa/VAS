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
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Resources.Styles;
using VAS.Core.ViewModel;

namespace VAS.Drawing.CanvasObjects.Timeline
{
	public class TimelineEventView : TimeNodeView
	{
		public TimelineEventView ()
		{
			// Only event boundaries can be dragged
			DraggingMode = NodeDraggingMode.Borders;
		}

		public TimelineEventVM TimelineEvent {
			get {
				return (TimelineEventVM)TimeNode;
			}
			set {
				TimeNode = value;
			}
		}

		public ISurface SelectionLeft {
			get;
			set;
		}

		public ISurface SelectionRight {
			get;
			set;
		}

		public override string Description {
			get {
				return TimelineEvent.Name;
			}
		}

		public override Area Area {
			get {
				double ls = 0;
				if (SelectionLeft != null) {
					ls = SelectionLeft.Width / 2;
				}
				return new Area (new Point (StartX - ls, OffsetY),
					(StopX - StartX) + 2 * ls, Height);
			}
		}

		protected void DrawLine (IDrawingToolkit tk, double start, double stop, int lineWidth)
		{
			double y;

			y = OffsetY + Height / 2;
			tk.LineWidth = lineWidth;
			tk.FillColor = TimelineEvent.Color;
			tk.StrokeColor = TimelineEvent.Color;
			if (stop - start <= lineWidth) {
				tk.LineWidth = 0;
				tk.DrawCircle (new Point (start + (stop - start) / 2, y), 3);
			} else {
				tk.DrawLine (new Point (start + lineWidth / 2, y),
					new Point (stop - lineWidth / 2, y));
			}
		}

		protected virtual void DrawBorders (IDrawingToolkit tk, double start, double stop, int lineWidth)
		{
			double y1, y2;

			tk.LineWidth = lineWidth;
			y1 = OffsetY + 6;
			y2 = OffsetY + Height - 6;
			tk.DrawLine (new Point (start, y1), new Point (start, y2));
			tk.DrawLine (new Point (stop, y1), new Point (stop, y2));
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			double start, stop;
			int lineWidth = Sizes.TimelineLineSize;

			if (!UpdateDrawArea (tk, area, Area)) {
				return;
			}

			tk.Begin ();

			start = StartX;
			stop = StopX;

			if (stop - start <= lineWidth) {
				DrawBorders (tk, start, stop, lineWidth);
				DrawLine (tk, start, stop, lineWidth);
			} else {
				DrawLine (tk, start, stop, lineWidth);
				DrawBorders (tk, start, stop, lineWidth);
			}
			if (Selected) {
				tk.DrawSurface (new Point (start - SelectionLeft.Width / 2, OffsetY), Sizes.TimelineSelectionLeftWidth, Sizes.TimelineSelectionLeftHeight, SelectionLeft, ScaleMode.AspectFit);
				tk.DrawSurface (new Point (stop - SelectionRight.Width / 2, OffsetY), Sizes.TimelineSelectionRightWidth, Sizes.TimelineSelectionRightHeight, SelectionRight, ScaleMode.AspectFit);
			}
			tk.End ();
		}
	}
}

