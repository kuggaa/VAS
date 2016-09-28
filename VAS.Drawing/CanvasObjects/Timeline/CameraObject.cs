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
using System.Timers;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;
using SysTimer = System.Timers.Timer;

namespace VAS.Drawing.CanvasObjects.Timeline
{
	/// <summary>
	/// Camera object for the Timeline.
	/// </summary>
	public class CameraObject: TimeNodeObject
	{
		MediaFile mediaFile;
		VideoTimelineMode videoTLmode;
		SysTimer stretchedAndEditingTimer;

		public CameraObject (MediaFile mf) :
			base (new TimeNode { Start = new Time (-mf.Offset.MSeconds),
				Stop = mf.Duration - mf.Offset, Name = mf.Name
			})
		{
			videoTLmode = VideoTimelineMode.Default;
			mediaFile = mf;
			DraggingMode = NodeDraggingMode.Borders;
			SelectionMode = NodeSelectionMode.Borders;
			ClippingMode = NodeClippingMode.LeftStrict;
			stretchedAndEditingTimer = new SysTimer (2000);
			stretchedAndEditingTimer.Elapsed += HandleTimeOut;
			App.Current.EventsBroker.Subscribe<StretchVideoEvent> (HandleStrechVideoEvent);
			App.Current.EventsBroker.Subscribe<ChangeVideoSizeEvent> (HandleChangeVideoSizeEvent);
		}

		public void Dispose ()
		{
			stretchedAndEditingTimer.Dispose ();
			App.Current.EventsBroker.Unsubscribe<StretchVideoEvent> (HandleStrechVideoEvent);
			App.Current.EventsBroker.Unsubscribe<ChangeVideoSizeEvent> (HandleChangeVideoSizeEvent);
		}

		/// <summary>
		/// Gets the media file.
		/// </summary>
		/// <value>The media file.</value>
		public MediaFile MediaFile {
			get {
				return mediaFile;
			}
		}

		/// <summary>
		/// Gets the description.
		/// </summary>
		/// <value>The description.</value>
		public override string Description {
			get {
				return mediaFile.Name;
			}
		}

		/// <summary>
		/// Gets the drawable area.
		/// </summary>
		/// <value>The area.</value>
		Area Area {
			get {
				return new Area (new Point (0, OffsetY),
					StopX, Height);
			}
		}

		public bool SelectedLeft {
			get;
			set;
		}

		public bool SelectedRight {
			get;
			set;
		}

		/// <summary>
		/// Draw the specified area with the specified Drawing toolkit tk.
		/// </summary>
		/// <param name="tk">IDrawingToolkit.</param>
		/// <param name="area">Area.</param>
		public override void Draw (IDrawingToolkit tk, Area area)
		{
			if (!UpdateDrawArea (tk, area, Area) && videoTLmode != VideoTimelineMode.Edit &&
			    !(area.Left <= StartX || area.Left >= StopX)) {
				return;
			}

			tk.Begin ();

			tk.StrokeColor = App.Current.Style.PaletteBackgroundDark;
			tk.LineWidth = StyleConf.TimelineCameraObjectLineWidth;

			Image limitArrowL;
			Image limitArrowR;

			if (IsStretched ()) {
				// Draw Video rectangle
				tk.FillColor = LineColor;
				tk.DrawRoundedRectangle (new Point (0, OffsetY), StopX - StartX, Height, 5);

				// Draw left arrow
				limitArrowL = videoTLmode == VideoTimelineMode.StretchedAndEditing ?
					Resources.LoadImage (StyleConf.LimitArrowRedL) : Resources.LoadImage (StyleConf.LimitArrowWhiteL);
				tk.DrawImage (new Point (0, OffsetY), limitArrowL.Width, Height, limitArrowL, ScaleMode.AspectFit);

				// Draw right arrow
				limitArrowR = videoTLmode == VideoTimelineMode.StretchedAndEditing ?
					Resources.LoadImage (StyleConf.LimitArrowRedR) : Resources.LoadImage (StyleConf.LimitArrowWhiteR);
				tk.DrawImage (new Point (StopX - StartX - limitArrowR.Width, OffsetY), limitArrowR.Width, Height, limitArrowR, ScaleMode.AspectFit);

				if (videoTLmode == VideoTimelineMode.StretchedAndEditing) {
					stretchedAndEditingTimer.Enabled = true;
				}
			} else {
				Color lineColorA25 = new Color (LineColor.R, LineColor.G, LineColor.B, (byte)(LineColor.A * 0.25));

				// Draw previous non-selected video rectangle
				if (StartX != 0) {
					tk.FillColor = lineColorA25;
					tk.DrawRoundedRectangle (new Point (0, OffsetY), StartX, Height, 5);
				}

				// Draw Video rectangle
				tk.FillColor = LineColor;
				tk.DrawRoundedRectangle (new Point (StartX, OffsetY), StopX - StartX, Height, 5);

				// Draw left arrow
				if (videoTLmode == VideoTimelineMode.Edit) {
					limitArrowL = SelectedLeft ? Resources.LoadImage (StyleConf.LimitArrowGreenSelectedL) : Resources.LoadImage (StyleConf.LimitArrowGreenL);
				} else {
					limitArrowL = SelectedLeft ? Resources.LoadImage (StyleConf.LimitArrowRedSelectedL) : Resources.LoadImage (StyleConf.LimitArrowRedL);
				}
				tk.DrawImage (new Point (StartX, OffsetY), limitArrowL.Width, Height, limitArrowL, ScaleMode.AspectFit);

				// Draw right arrow
				if (videoTLmode == VideoTimelineMode.Edit) {
					limitArrowR = SelectedRight ? Resources.LoadImage (StyleConf.LimitArrowGreenSelectedR) : Resources.LoadImage (StyleConf.LimitArrowGreenR);
				} else {
					limitArrowR = SelectedRight ? Resources.LoadImage (StyleConf.LimitArrowRedSelectedR) : Resources.LoadImage (StyleConf.LimitArrowRedR);
				}
				tk.DrawImage (new Point (StopX - limitArrowR.Width, OffsetY), limitArrowR.Width, Height + 2, limitArrowR, ScaleMode.AspectFit);

				// Draw after non-selected video rectangle
				double maxX = Utils.TimeToPos (this.MaxTime, SecondsPerPixel);
				if (StopX != maxX) {
					tk.FillColor = lineColorA25;
					tk.DrawRoundedRectangle (new Point (StopX, OffsetY), maxX - StopX, Height, 5);
					tk.FillColor = LineColor;
				}
			}

			// Draw Text
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

		/// <summary>
		/// Move the CameraObject with a given selection sel
		/// from point start to point target.
		/// </summary>
		/// <param name="sel">Selection.</param>
		/// <param name="target">The desired point.</param>
		/// <param name="start">The start point.</param>
		public override void Move (Selection sel, Point target, Point start)
		{
			// If video is stretched, cannot move borders
			if (IsStretched ()) {
				videoTLmode = VideoTimelineMode.StretchedAndEditing;
				return;
			}

			double diffX;

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

			Time newTime = Utils.PosToTime (target, SecondsPerPixel);
			diffX = target.X - start.X;

			// Assure that p is in the bounds
			if (target.X < 0) {
				target.X = 0;
			} else if (newTime > MaxTime) {
				target.X = Utils.TimeToPos (MaxTime, SecondsPerPixel);
			}

			// Move to target point
			newTime = Utils.PosToTime (target, SecondsPerPixel);

			switch (sel.Position) {
			case SelectionPosition.Left:
				if (newTime.MSeconds + MAX_TIME_SPAN > TimeNode.Stop.MSeconds) {
					TimeNode.Start.MSeconds = TimeNode.Stop.MSeconds - MAX_TIME_SPAN;
				} else {
					TimeNode.Start = newTime;
				}
				break;
			case SelectionPosition.Right:
				if (newTime.MSeconds - MAX_TIME_SPAN < TimeNode.Start.MSeconds) {
					TimeNode.Stop.MSeconds = TimeNode.Start.MSeconds + MAX_TIME_SPAN;
				} else {
					TimeNode.Stop = newTime;
				}
				break;
			case SelectionPosition.All:
				Time tstart, tstop;
				Time diff = Utils.PosToTime (new Point (diffX, target.Y), SecondsPerPixel);
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
				}

				if (ok) {
					TimeNode.Start += diff;
					TimeNode.Stop += diff;
				}
				break;
			}
			movingPos = sel.Position;

			// Set current videoTLmode.
			videoTLmode = (StartX == 0) && StopX == Utils.TimeToPos (this.MaxTime, SecondsPerPixel) ?
							 VideoTimelineMode.Default : VideoTimelineMode.Edit;
		}

		/// <summary>
		/// Determines if the videoTLmode is stretched
		/// </summary>
		/// <returns><c>true</c> if the videoTLmode is stretched ; otherwise, <c>false</c>.</returns>
		public bool IsStretched ()
		{
			return (videoTLmode == VideoTimelineMode.Stretched || videoTLmode == VideoTimelineMode.StretchedAndEditing);
		}

		/// <summary>
		/// Handles the strech video event.
		/// </summary>
		/// <param name="e">Event.</param>
		protected virtual void HandleStrechVideoEvent (StretchVideoEvent e)
		{
			var ev = new TimelineEvent ();
			if (IsStretched ()) {
				videoTLmode = VideoTimelineMode.Edit;
				ev.Start = new Time (0);
				ev.Stop = MaxTime;
			} else {
				videoTLmode = VideoTimelineMode.Stretched;
				ev.Start = Utils.PosToTime (new Point (StartX, 0), SecondsPerPixel);
				ev.Stop = Utils.PosToTime (new Point (StopX, 0), SecondsPerPixel);
			}

			ev.FileSet = e.mfs;
			ev.EventTime = ev.Duration;

			App.Current.EventsBroker.Publish<LoadCameraEvent> (
				new LoadCameraEvent {
					CameraTlEvent = ev
				});
		}

		protected virtual void HandleChangeVideoSizeEvent (ChangeVideoSizeEvent e)
		{
			if (IsStretched () || !Selected || e.Time == null) {
				return;
			}

			if (SelectedLeft) {
				Time startTime = TimeNode.Start + e.Time;
				if (startTime >= new Time (0)) {
					// Almost 1s must be showed in the timeline
					TimeNode.Start = startTime <= TimeNode.Stop - new Time (1000) ? startTime : TimeNode.Stop - new Time (1000);
					e.player.Seek (TimeNode.Start);
				}
			}

			if (SelectedRight) {
				Time stopTime = TimeNode.Stop + e.Time;
				if (stopTime <= MaxTime) {
					// Almost 1s must be showed in the timeline
					TimeNode.Stop = stopTime >= TimeNode.Start + new Time (1000) ? stopTime : TimeNode.Start + new Time (1000);
					e.player.Seek (TimeNode.Stop);
				}
			}

			videoTLmode = TimeNode.Start == new Time (0) && TimeNode.Stop == MaxTime ?
				VideoTimelineMode.Default : VideoTimelineMode.Edit;

			ReDraw ();
		}

		protected virtual void HandleTimeOut (Object source, ElapsedEventArgs e)
		{
			videoTLmode = VideoTimelineMode.Stretched;
			stretchedAndEditingTimer.Stop ();

			ReDraw ();
		}
	}
}
