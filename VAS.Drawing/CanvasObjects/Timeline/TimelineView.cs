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

using System.Collections.Generic;
using System.Linq;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;
using VAS.Core.ViewModel;
using VAS.Core.Resources;
using VAS.Core.Resources.Styles;

namespace VAS.Drawing.CanvasObjects.Timeline
{
	/// <summary>
	/// A base View for timelines working with <see cref="TimeNodeVM"/> objects.
	/// The sub class is responsible to listen for collection changes in its ViewModel and add/remode
	/// <see cref="TimeNodeView"/> to the timeline using the funtions provided in the base class. 
	/// </summary>
	public abstract class TimelineView : CanvasContainer
	{
		double secondsPerPixel;
		Time duration;
		protected ISurface selectionBorderL, selectionBorderR;

		public TimelineView ()
		{
			BackgroundColor = Color.Grey1;
			selectionBorderL = LoadBorder (Icons.TimelineSelectionLeft);
			selectionBorderR = LoadBorder (Icons.TimelineSelectionRight);
			Visible = true;
			Duration = new Time (0);
			CurrentTime = new Time (0);
			SecondsPerPixel = 0.1;
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			selectionBorderL?.Dispose ();
			selectionBorderR?.Dispose ();
		}

		public double SecondsPerPixel {
			set {
				secondsPerPixel = value;
				foreach (TimeNodeView to in this) {
					to.SecondsPerPixel = secondsPerPixel;
					to.ResetDrawArea ();
				}
			}
			protected get {
				return secondsPerPixel;
			}
		}

		/// <summary>
		/// Gets or sets the color of the background.
		/// </summary>
		/// <value>The color of the background.</value>
		public Color BackgroundColor {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the current time of the timeline.
		/// </summary>
		/// <value>The current time.</value>
		public Time CurrentTime {
			set;
			protected get;
		}

		/// <summary>
		/// Gets or sets the duration of the timeline.
		/// </summary>
		/// <value>The duration.</value>
		public Time Duration {
			get {
				return duration;
			}
			set {
				duration = value;
			}
		}

		/// <summary>
		/// Gets or sets the height of the timeline row.
		/// </summary>
		/// <value>The height.</value>
		public double Height {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the width of the timeline.
		/// </summary>
		/// <value>The width.</value>
		public double Width {
			set;
			get;
		}

		/// <summary>
		/// Gets or sets the y offset of the timeline.
		/// </summary>
		/// <value>The offset y.</value>
		public double OffsetY {
			set;
			get;
		}

		/// <summary>
		/// Removes a node from the timeline.
		/// </summary>
		/// <param name="node">Node.</param>
		protected void RemoveNode (TimeNodeVM node)
		{
			TimeNodeView to;

			to = this.OfType<TimeNodeView> ().FirstOrDefault (n => n.TimeNode == node);
			if (to != null) {
				Remove (to);
			}
		}

		/// <summary>
		/// Gets the node at a given position in X.
		/// </summary>
		/// <returns>The node at position.</returns>
		/// <param name="position">Position.</param>
		public TimeNodeView GetNodeAtPosition (double position)
		{
			TimeNodeView node = this.OfType<TimeNodeView> ().FirstOrDefault (n => position >= n.StartX && position <= n.StopX);
			if (node == null) {
				node = this.OfType<TimeNodeView> ().LastOrDefault ();
			}
			return node;
		}

		protected virtual void DrawBackground (IDrawingToolkit tk, Area area)
		{
			tk.FillColor = BackgroundColor;
			tk.StrokeColor = BackgroundColor;
			tk.LineWidth = 0;

			tk.DrawRectangle (new Point (area.Start.X, OffsetY), area.Width, Height);
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			double position;
			List<TimeNodeView> selected;

			selected = new List<TimeNodeView> ();

			if (!UpdateDrawArea (tk, area, new Area (new Point (0, OffsetY), Width, Height))) {
				return;
			}

			tk.Begin ();
			DrawBackground (tk, area);
			foreach (TimeNodeView p in this) {
				if (!p.Visible)
					continue;
				if (p.Selected) {
					selected.Add (p);
					continue;
				}
				p.OffsetY = OffsetY;
				p.Draw (tk, area);
			}
			foreach (TimeNodeView p in selected) {
				p.OffsetY = OffsetY;
				p.Draw (tk, area);
			}

			tk.FillColor = App.Current.Style.ColorPrimary;
			tk.StrokeColor = App.Current.Style.ColorPrimary;
			tk.LineWidth = Constants.TIMELINE_LINE_WIDTH;
			position = Utils.TimeToPos (CurrentTime, secondsPerPixel);
			tk.DrawLine (new Point (position, OffsetY),
				new Point (position, OffsetY + Height));

			tk.End ();
		}

		public override Selection GetSelection (Point point, double precision, bool inMotion = false)
		{
			if (point.Y >= OffsetY && point.Y < OffsetY + Height) {
				return base.GetSelection (point, precision, inMotion);
			}
			return null;
		}

		public override void Move (Selection s, Point p, Point start)
		{
			s.Drawable.Move (s, p, start);
		}

		ISurface LoadBorder (string name)
		{
			Image img = App.Current.ResourcesLocator.LoadIcon (name, Sizes.TimelineCategoryHeight);
			return App.Current.DrawingToolkit.CreateSurface (img.Width, img.Height, img);
		}
	}
}

