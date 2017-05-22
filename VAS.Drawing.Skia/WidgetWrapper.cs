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
using System.Runtime.InteropServices;
using AppKit;
using Gdk;
using Gtk;
using SkiaSharp;
using SkiaSharp.Views.Mac;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Handlers.Drawing;
using VAS.Core.Interfaces.Drawing;
using CursorType = VAS.Core.Common.CursorType;
using GCursorType = Gdk.CursorType;
using Image = VAS.Core.Common.Image;
using Point = VAS.Core.Common.Point;
using Rectangle = Gdk.Rectangle;
using VASDrawing = VAS.Core.Handlers.Drawing;

namespace VAS.Drawing.Skia
{
	public class SkiaDrawingArea : DrawingArea, IWidget
	{
		public event DrawingHandler DrawEvent;
		public new event ButtonPressedHandler ButtonPressEvent;
		public event ButtonReleasedHandler ButtonReleasedEvent;
		public event MotionHandler MotionEvent;
		public event ShowTooltipHandler ShowTooltipEvent;
		public event VASDrawing.SizeChangedHandler SizeChangedEvent;

		SKCanvasLayer layer;
		int currentWidth, currentHeight;
		double lastX, lastY;
		bool canMove, inButtonPress;
		uint moveTimerID, hoverTimerID, lastButtonTime;

		public SkiaDrawingArea ()
		{
			MoveWaitMS = 200;
			AddEvents ((int)EventMask.PointerMotionMask);
			AddEvents ((int)EventMask.ButtonPressMask);
			AddEvents ((int)EventMask.ButtonReleaseMask);
			AddEvents ((int)EventMask.KeyPressMask);
			ExposeEvent += HandleExposeEvent;
			MotionNotifyEvent += HandleMotionNotifyEvent;
			Realized += HandleRealized;
		}

		public override void Dispose ()
		{
			base.Dispose ();
			if (moveTimerID != 0) {
				GLib.Source.Remove (moveTimerID);
				moveTimerID = 0;
			}
			if (hoverTimerID != 0) {
				GLib.Source.Remove (hoverTimerID);
				hoverTimerID = 0;
			}
		}

		/// <summary>
		/// Fast button clicks sometimes produced a small move that
		/// should be ignored. Start moving only when the button has been
		/// pressed for more than MoveWaitMS.
		/// </summary>
		/// <value>Milliseconds to wait before moving.</value>
		public uint MoveWaitMS {
			get;
			set;
		}

		public double Width {
			get {
				return currentWidth;
			}
			set {
				WidthRequest = (int)value;
			}
		}

		public double Height {
			get {
				return currentHeight;
			}
			set {
				HeightRequest = (int)value;
			}
		}

		public void ReDraw (Area area = null)
		{
			if (GdkWindow == null) {
				return;
			}
			if (area == null) {
				Gdk.Region region = GdkWindow.ClipRegion;
				GdkWindow.InvalidateRegion (region, true);
			} else {
				GdkWindow.InvalidateRect (
					new Gdk.Rectangle ((int)area.Start.X - 1, (int)area.Start.Y - 1,
						(int)Math.Ceiling (area.Width) + 2,
						(int)Math.Ceiling (area.Height) + 2),
					true);
			}
			GdkWindow.ProcessUpdates (true);
		}

		public void ReDraw (IMovableObject drawable)
		{
			/* FIXME: get region from drawable */
			ReDraw ();
		}

		public void ShowTooltip (string text)
		{
			HasTooltip = true;
			TooltipText = text;
		}

		public void SetCursor (CursorType type)
		{
			GCursorType gtype;
			switch (type) {
			case CursorType.Arrow:
				gtype = GCursorType.Arrow;
				break;
			case CursorType.LeftArrow:
				gtype = GCursorType.LeftPtr;
				break;
			case CursorType.DoubleArrow:
				gtype = GCursorType.SbHDoubleArrow;
				break;
			case CursorType.Selection:
				gtype = GCursorType.Fleur;
				break;
			case CursorType.Cross:
				gtype = GCursorType.Cross;
				break;
			default:
				gtype = GCursorType.Arrow;
				break;
			}
			GdkWindow.Cursor = new Cursor (gtype);
		}

		public void SetCursorForTool (DrawTool tool)
		{
			string cursorStr = null;
			Gdk.Cursor cursor = null;

			if (GdkWindow == null) {
				return;
			}

			switch (tool) {
			case DrawTool.Line:
				cursorStr = "arrow";
				break;
			case DrawTool.Cross:
				cursorStr = "cross";
				break;
			case DrawTool.Text:
				cursorStr = "text";
				break;
			case DrawTool.Counter:
				cursorStr = "number";
				break;
			case DrawTool.Ellipse:
			case DrawTool.CircleArea:
				cursorStr = "ellipse";
				break;
			case DrawTool.Rectangle:
			case DrawTool.RectangleArea:
				cursorStr = "rect";
				break;
			case DrawTool.Angle:
				cursorStr = "angle";
				break;
			case DrawTool.Pen:
				cursorStr = "freehand";
				break;
			case DrawTool.Eraser:
				cursorStr = "eraser";
				break;
			case DrawTool.Player:
				cursorStr = "player";
				break;
			case DrawTool.Zoom:
				cursorStr = "zoom";
				break;
			case DrawTool.CanMove:
				cursorStr = "hand_opened";
				break;
			case DrawTool.Move:
				cursorStr = "hand_closed";
				break;
			case DrawTool.Selection:
				cursorStr = "hand_select";
				break;
			default:
				cursor = null;
				break;
			}
			if (cursorStr == null) {
				GdkWindow.Cursor = cursor;
			} else {
				Image img = Resources.LoadImage (System.IO.Path.Combine ("images/cursors", cursorStr));
				Cursor c = new Cursor (Display, img.Value, 0, 0);
				GdkWindow.Cursor = c;
			}
		}

		void Draw (Area area)
		{
			if (DrawEvent != null) {

				using (var cc = CairoHelper.Create (GdkWindow)) {
					if (area == null) {
						Rectangle r = GdkWindow.ClipRegion.Clipbox;
						area = new Area (new Point (r.X, r.Y), r.Width, r.Height);
					}
					var sksurface = new SkiaSurface ((int)area.Width * 2, (int)area.Height * 2, null);
					var c = sksurface.Context;
					var bitmap = sksurface.bitmap;
					(c.Value as SKCanvas).Scale (2, 2);
					(c.Value as SKCanvas).Clear ();
					DrawEvent (c, area);
					IntPtr len;

					Cairo.Surface surface = new Cairo.ImageSurface (
						sksurface.bitmap.GetPixels (out len),
						Cairo.Format.ARGB32,
						bitmap.Width, bitmap.Height,
						bitmap.Width * 4);

					surface.MarkDirty ();
					cc.Rectangle (area.Start.X, area.Start.Y, area.Width, area.Height);
					cc.Clip ();
					cc.Scale (0.5, 0.5);
					cc.SetSourceSurface (surface, 0, 0);
					cc.Paint ();
					surface.Dispose ();
				}
			}
		}

		ButtonType ParseButtonType (uint button)
		{
			ButtonType bt;

			switch (button) {
			case 1:
				bt = ButtonType.Left;
				break;
			case 2:
				bt = ButtonType.Center;
				break;
			case 3:
				bt = ButtonType.Right;
				break;
			default:
				bt = ButtonType.None;
				break;
			}
			return bt;
		}

		ButtonModifier ParseButtonModifier (ModifierType modifier)
		{
			ButtonModifier bm;

			switch (modifier) {
#if OSTYPE_OS_X
			case ModifierType.Mod2Mask:
				bm = ButtonModifier.Control;
				break;
			case ModifierType.ControlMask:
				bm = ButtonModifier.Meta;
				break;
#else
			case ModifierType.ControlMask:
				bm = ButtonModifier.Control;
				break;
#endif
			case ModifierType.ShiftMask:
				bm = ButtonModifier.Shift;
				break;
			default:
				bm = ButtonModifier.None;
				break;
			}
			return bm;
		}

		ButtonRepetition ParseButtonRepetition (EventType type)
		{
			ButtonRepetition br;

			switch (type) {
			case EventType.TwoButtonPress:
				br = ButtonRepetition.Double;
				break;
			case EventType.ThreeButtonPress:
				br = ButtonRepetition.Triple;
				break;
			default:
				br = ButtonRepetition.Single;
				break;
			}
			return br;
		}

		bool ReadyToMove ()
		{
			canMove = true;
			moveTimerID = 0;
			return false;
		}

		bool EmitShowTooltip ()
		{
			if (ShowTooltipEvent != null) {
				ShowTooltipEvent (new Point (lastX, lastY));
			}
			hoverTimerID = 0;
			return false;
		}

		void HandleMotionNotifyEvent (object o, MotionNotifyEventArgs args)
		{
			if (hoverTimerID != 0) {
				GLib.Source.Remove (hoverTimerID);
				hoverTimerID = 0;
			}
			hoverTimerID = GLib.Timeout.Add (100, EmitShowTooltip);
			HasTooltip = false;

			lastX = args.Event.X;
			lastY = args.Event.Y;

			if (MotionEvent != null) {
				if (!inButtonPress || canMove) {
					MotionEvent (new Point (lastX, lastY));
				}
			}
		}

		protected override bool OnButtonReleaseEvent (EventButton evnt)
		{
			if (moveTimerID != 0) {
				GLib.Source.Remove (moveTimerID);
				moveTimerID = 0;
			}

			if (ButtonReleasedEvent != null) {
				ButtonType bt;
				ButtonModifier bm;

				bt = ParseButtonType (evnt.Button);
				bm = ParseButtonModifier (evnt.State);
				ButtonReleasedEvent (new Point (evnt.X, evnt.Y), bt, bm);
			}
			inButtonPress = false;

			/* Grab the focus if it's required */
			if (CanFocus && !HasFocus)
				GrabFocus ();
			return base.OnButtonReleaseEvent (evnt);
		}

		protected override bool OnButtonPressEvent (EventButton evnt)
		{
			/* Fast button clicks sometimes produced a small move that
			 * should be ignored. Start moving only when the button has been
			 * pressed for more than 200ms */
			canMove = false;
			inButtonPress = true;
			moveTimerID = GLib.Timeout.Add (MoveWaitMS, ReadyToMove);

			if (ButtonPressEvent != null) {
				ButtonType bt;
				ButtonModifier bm;
				ButtonRepetition br;
				uint time = evnt.Time;

				bt = ParseButtonType (evnt.Button);
				bm = ParseButtonModifier (evnt.State);
				br = ParseButtonRepetition (evnt.Type);

				// Ignore the second single click event when there's a double click
				var nextEvent = EventHelper.Peek ();
				if (nextEvent != null) {
					try {
						var nextEventButton = nextEvent as EventButton;
						if (nextEventButton?.Time == time &&
							ParseButtonRepetition (nextEventButton.Type) != ButtonRepetition.Single) {
							return true;
						}
					} finally {
						EventHelper.Free (nextEvent);
					}
				}

				ButtonPressEvent (new Point (evnt.X, evnt.Y),
					time, bt, bm, br);
			}
			return true;
		}

		void HandleExposeEvent (object o, ExposeEventArgs args)
		{
			Rectangle r;
			Area a;
			bool size_changed;

			size_changed = Allocation.Height != currentHeight;
			size_changed |= Allocation.Width != currentWidth;
			currentWidth = Allocation.Width;
			currentHeight = Allocation.Height;
			if (size_changed && SizeChangedEvent != null) {
				SizeChangedEvent ();
			}

			r = args.Event.Area;
			a = new Area (new Point (r.X, r.Y), r.Width, r.Height);
			Draw (a);
		}

		void Layer_PaintSurface (object sender, SKPaintSurfaceEventArgs e)
		{
			var c = new SkiaContext (e.Surface);
			Console.WriteLine (e.Info.Rect);
			DrawEvent (c, null);
		}

		[DllImport ("libgdk-quartz-2.0.0.dylib")]
		static extern IntPtr gdk_quartz_window_get_nsview (IntPtr handle);

		[DllImport ("libgdk-quartz-2.0.0.dylib")]
		static extern void gdk_window_ensure_native (IntPtr handle);

		void HandleRealized (object sender, EventArgs e)
		{
			gdk_window_ensure_native (GdkWindow.Handle);
			NSView view = new NSView ();
			view.Handle = gdk_quartz_window_get_nsview (GdkWindow.Handle);

			NSView subview = new NSView (view.Bounds);
			layer = new SKCanvasLayer ();
			layer.PaintSurface += Layer_PaintSurface;
			view.AddSubview (view);
			subview.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;
			subview.Layer = layer;
			subview.WantsLayer = true;
		}
	}
}

