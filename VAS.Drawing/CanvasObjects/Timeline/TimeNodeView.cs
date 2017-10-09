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
using VAS.Core.Store;
using VAS.Core.Store.Drawables;
using VAS.Core.ViewModel;

namespace VAS.Drawing.CanvasObjects.Timeline
{
	/// <summary>
	/// Time node object.
	/// </summary>
	public class TimeNodeView : CanvasObject, ICanvasSelectableObject
	{
		protected ISurface needle;
		protected SelectionPosition movingPos;
		protected const int MAX_TIME_SPAN = 1000;
		TimeNodeVM timeNode;

		public TimeNodeView ()
		{
			SelectionMode = NodeSelectionMode.All;
			DraggingMode = NodeDraggingMode.All;
			LineColor = App.Current.Style.PaletteBackgroundLight;
			Height = StyleConf.TimelineCategoryHeight;
			ClippingMode = NodeClippingMode.Strict;
			ScrollX = 0;
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			if (needle != null) {
				needle.Dispose ();
				needle = null;
			}
			TimeNode = null;
		}

		public override bool Visible {
			get {
				return TimeNode.Visible;
			}
#pragma warning disable RECS0029
			set {
				// NOTE: there is a Visible property in the parent that we need
				// but this object's visibility depends on the VM
			}
#pragma warning restore RECS0029
		}

		/// <summary>
		/// Gets or sets the time node.
		/// </summary>
		/// <value>The time node.</value>
		public TimeNodeVM TimeNode {
			get {
				return timeNode;
			}

			set {
				if (timeNode != null) {
					timeNode.PropertyChanged -= HandleTimeNodePropertyChanged;
				}
				timeNode = value;
				if (timeNode != null) {
					timeNode.PropertyChanged += HandleTimeNodePropertyChanged;
				}
			}
		}

		/// <summary>
		/// Gets or sets the selection mode.
		/// </summary>
		/// <value>The selection mode.</value>
		public NodeSelectionMode SelectionMode {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the dragging mode.
		/// </summary>
		/// <value>The dragging mode.</value>
		public NodeDraggingMode DraggingMode {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the color of the line.
		/// </summary>
		/// <value>The color of the line.</value>
		public virtual Color LineColor {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating if Name should be showed
		/// </summary>
		/// <value><c>true</c> if show name; otherwise, <c>false</c>.</value>
		public bool ShowName {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the max time.
		/// </summary>
		/// <value>The max time.</value>
		public virtual Time MaxTime {
			set;
			get;
		}

		/// <summary>
		/// Gets or sets the offset y.
		/// </summary>
		/// <value>The offset y.</value>
		public double OffsetY {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the height.
		/// </summary>
		/// <value>The height.</value>
		public double Height {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the seconds per pixel.
		/// </summary>
		/// <value>The seconds per pixel.</value>
		public double SecondsPerPixel {
			set;
			protected get;
		}

		/// <summary>
		/// Gets the start X position.
		/// </summary>
		/// <value>The start x.</value>
		public double StartX {
			get {
				return Utils.TimeToPos (TimeNode.Start, SecondsPerPixel) - ScrollX;
			}
		}

		/// <summary>
		/// Gets the stop X position.
		/// </summary>
		/// <value>The stop x.</value>
		public double StopX {
			get {
				return Utils.TimeToPos (TimeNode.Stop, SecondsPerPixel) - ScrollX;
			}
		}

		/// <summary>
		/// Gets the center X position.
		/// </summary>
		/// <value>The center x.</value>
		public double CenterX {
			get {
				return Utils.TimeToPos (TimeNode.Start + TimeNode.Duration / 2,
					SecondsPerPixel);
			}
		}

		/// <summary>
		/// Gets or sets the clipping mode.
		/// </summary>
		/// <value>The clipping mode.</value>
		public NodeClippingMode ClippingMode {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the scroll applied to the view in the X coordinates.
		/// </summary>
		/// <value>The scroll x.</value>
		public double ScrollX {
			get;
			set;
		}

		public virtual Area Area {
			get {
				double ls = StyleConf.TimelineLineSize;
				return new Area (new Point (StartX - ls, OffsetY),
					(StopX - StartX) + 2 * ls, Height);
			}
		}

		/// <summary>
		/// Gets the selection.
		/// </summary>
		/// <returns>The selection.</returns>
		/// <param name="point">Point.</param>
		/// <param name="precision">Precision.</param>
		/// <param name="inMotion">If set to <c>true</c> in motion.</param>
		public virtual Selection GetSelection (Point point, double precision, bool inMotion = false)
		{
			if (SelectionMode == NodeSelectionMode.Borders || SelectionMode == NodeSelectionMode.All) {
				double accuracy;
				if (point.Y >= OffsetY && point.Y < OffsetY + Height) {
					if (Drawable.MatchAxis (point.X, StartX, precision, out accuracy)) {
						return new Selection (this, SelectionPosition.Left, accuracy);
					} else if (Drawable.MatchAxis (point.X, StopX, precision, out accuracy)) {
						return new Selection (this, SelectionPosition.Right, accuracy);
					}
				}
			}

			if (SelectionMode == NodeSelectionMode.Segment || SelectionMode == NodeSelectionMode.All) {
				if (point.Y >= OffsetY && point.Y < OffsetY + Height) {
					if (point.X > StartX && point.X < StopX) {
						return new Selection (this, SelectionPosition.All, Math.Abs (CenterX - point.X));
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Move the TimeNodeObject with a given selection sel
		/// from point start to point p.
		/// </summary>
		/// <param name="sel">Selection.</param>
		/// <param name="p">Target point.</param>
		/// <param name="start">Start point.</param>
		public virtual void Move (Selection sel, Point p, Point start)
		{
			Time newTime;
			double diffX, posX;

			// Apply dragging restrictions
			if (DraggingMode == NodeDraggingMode.None)
				return;
			switch (sel.Position) {
			case SelectionPosition.Left:
			case SelectionPosition.Right:
				if (DraggingMode == NodeDraggingMode.Segment)
					return;
				break;
			case SelectionPosition.All:
				if (DraggingMode == NodeDraggingMode.Borders)
					return;
				break;
			}

			newTime = Utils.PosToTime (p, SecondsPerPixel);
			diffX = p.X - start.X;
			p = p.Copy ();

			if (p.X < 0) {
				p.X = 0;
			} else if (newTime > MaxTime) {
				p.X = Utils.TimeToPos (MaxTime, SecondsPerPixel);
			}
			p.X += ScrollX;
			newTime = Utils.PosToTime (p, SecondsPerPixel);

			switch (sel.Position) {
			case SelectionPosition.Left:
				if (ClippingMode == NodeClippingMode.EventTime && !(newTime <= TimeNode.EventTime &&
																	TimeNode.EventTime <= TimeNode.Stop)) {
					break;
				}
				if (newTime.MSeconds + MAX_TIME_SPAN > TimeNode.Stop.MSeconds) {
					TimeNode.Start.MSeconds = TimeNode.Stop.MSeconds - MAX_TIME_SPAN;
				} else {
					TimeNode.Start = newTime;
				}
				break;
			case SelectionPosition.Right:
				if (ClippingMode == NodeClippingMode.EventTime && !(TimeNode.Start <= TimeNode.EventTime &&
																	TimeNode.EventTime <= newTime)) {
					break;
				}
				if (newTime.MSeconds - MAX_TIME_SPAN < TimeNode.Start.MSeconds) {
					TimeNode.Stop.MSeconds = TimeNode.Start.MSeconds + MAX_TIME_SPAN;
				} else {
					TimeNode.Stop = newTime;
				}
				break;
			case SelectionPosition.All:
				Time tstart, tstop;
				Time diff = Utils.PosToTime (new Point (diffX, p.Y), SecondsPerPixel);
				bool ok = false;

				tstart = TimeNode.Start;
				tstop = TimeNode.Stop;

				switch (ClippingMode) {
				case NodeClippingMode.None:
					ok = true;
					break;
				case NodeClippingMode.NoStrict:
					ok = ((tstop + diff) >= new Time (0) && (tstart + diff) < MaxTime);
					break;
				case NodeClippingMode.LeftStrict:
					ok = ((tstart + diff) >= new Time (0) && (tstart + diff) < MaxTime);
					break;
				case NodeClippingMode.RightStrict:
					ok = (tstop + diff) >= new Time (0) && ((tstop + diff) < MaxTime);
					break;
				case NodeClippingMode.Strict:
					ok = ((tstart + diff) >= new Time (0) && (tstop + diff) < MaxTime);
					break;
				case NodeClippingMode.EventTime:
					ok = ((tstart + diff) <= TimeNode.EventTime && (tstop + diff) >= TimeNode.EventTime) &&
						(tstop + diff) - (tstart + diff) >= new Time (MAX_TIME_SPAN);
					break;
				}

				if (ok) {
					TimeNode.Start += diff;
					TimeNode.Stop += diff;
				}
				break;
			}
			movingPos = sel.Position;
		}

		/// <summary>
		/// Draw the specified area with the specified Drawing toolkit tk.
		/// </summary>
		/// <param name="tk">IDrawingToolkit.</param>
		/// <param name="area">Area.</param>
		public override void Draw (IDrawingToolkit tk, Area area)
		{
			double linepos;

			if (!UpdateDrawArea (tk, area, Area)) {
				return;
			}

			tk.Begin ();
			if (needle == null) {
				needle = tk.CreateSurfaceFromResource (StyleConf.TimelineNeedleUP);
			}

			if (Selected) {
				tk.FillColor = App.Current.Style.PaletteActive;
				tk.StrokeColor = App.Current.Style.PaletteActive;
			} else {
				tk.FillColor = LineColor;
				tk.StrokeColor = LineColor;
			}
			tk.LineWidth = StyleConf.TimelineLineSize;

			linepos = OffsetY + Height / 2 + StyleConf.TimelineLineSize / 2;

			if (StopX - StartX <= needle.Width / 2) {
				double c = movingPos == SelectionPosition.Left ? StopX : StartX;
				tk.DrawSurface (new Point (c - needle.Width / 2, linepos - 9), StyleConf.TimelineNeedleUpWidth,
								StyleConf.TimelineNeedleUpHeight, needle, ScaleMode.AspectFit);
			} else {
				tk.DrawLine (new Point (StartX, linepos),
					new Point (StopX, linepos));
				tk.DrawSurface (new Point (StartX - needle.Width / 2, linepos - 9), StyleConf.TimelineNeedleUpWidth,
								StyleConf.TimelineNeedleUpHeight, needle, ScaleMode.AspectFit);
				tk.DrawSurface (new Point (StopX - needle.Width / 2, linepos - 9), StyleConf.TimelineNeedleUpWidth,
								StyleConf.TimelineNeedleUpHeight, needle, ScaleMode.AspectFit);
			}

			if (ShowName) {
				tk.FontSize = StyleConf.TimelineFontSize;
				tk.FontWeight = FontWeight.Bold;
				tk.FillColor = App.Current.Style.PaletteActive;
				tk.StrokeColor = App.Current.Style.PaletteActive;
				tk.DrawText (new Point (StartX, OffsetY), StopX - StartX, Height / 2, TimeNode.Name);
			}
			tk.End ();
		}

		void HandleTimeNodePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (TimeNodeVM.Visible) ||
				e.PropertyName == nameof (TimeNodeVM.SelectedGrabber)) {
				ReDraw ();
			}
		}
	}
}
