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
using System.Linq;
using Newtonsoft.Json;
using VAS.Core.Common;

namespace VAS.Core.Store.Drawables
{
	[Serializable]
	public class Cross : Drawable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:VAS.Core.Store.Drawables.Cross"/> class.
		/// </summary>
		public Cross ()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:VAS.Core.Store.Drawables.Cross"/> class.
		/// </summary>
		/// <param name="start">Start.</param>
		/// <param name="stop">Stop.</param>
		/// <param name="style">Style.</param>
		public Cross (Point start, Point stop, LineStyle style)
		{
			Start = start;
			Stop = stop;
			Style = style;
		}

		/// <summary>
		/// X,Y Point where Cross starts
		/// </summary>
		/// <value>The start.</value>
		public Point Start {
			set;
			get;
		}

		/// <summary>
		/// X,Y Point where Cross ends
		/// </summary>
		/// <value>The stop.</value>
		public Point Stop {
			set;
			get;
		}

		/// <summary>
		/// Gets the cross area.
		/// </summary>
		/// <value>The area.</value>
		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public override Area Area {
			get {
				Point tl = new Point (Math.Min (Start.X, Stop.X),
							   Math.Min (Start.Y, Stop.Y));
				return new Area (tl, Math.Abs (Start.X - Stop.X),
					Math.Abs (Start.Y - Stop.Y));
			}
		}

		/// <summary>
		/// Start Inverse Point
		/// X is Stop.X and Y is Start.Y
		/// </summary>
		/// <value>The start inverse point.</value>
		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Point StartI {
			get {
				return new Point (Stop.X, Start.Y);
			}
		}

		/// <summary>
		/// Stop Inverse Point
		/// X is Start.X and Y is Stop.Y
		/// </summary>
		/// <value>The stop i.</value>
		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Point StopI {
			get {
				return new Point (Start.X, Stop.Y);
			}
		}

		public override void Reorder ()
		{
			Point [] array = new Point [] { Start, Stop, StartI, StopI };

			array = array.OrderBy (p => p.X).ThenBy (p => p.Y).ToArray ();
			Start = array [0];
			Stop = array [3];
		}

		public override Selection GetSelection (Point p, double pr = 0.05, bool inMotion = false)
		{
			double d;
			Selection sel;

			if (Selected) {
				return base.GetSelection (p, pr);
			}

			if (MatchPoint (Start, p, pr, out d)) {
				return new Selection (this, SelectionPosition.TopLeft, d);
			} else if (MatchPoint (Stop, p, pr, out d)) {
				return new Selection (this, SelectionPosition.BottomRight, d);
			} else if (MatchPoint (StartI, p, pr, out d)) {
				return new Selection (this, SelectionPosition.TopRight, d);
			} else if (MatchPoint (StopI, p, pr, out d)) {
				return new Selection (this, SelectionPosition.BottomLeft, d);
			} else {
				Line aline = new Line { Start = Start, Stop = Stop };
				sel = aline.GetSelection (p, pr);
				if (sel == null) {
					Line bline = new Line { Start = StartI, Stop = StopI };
					sel = bline.GetSelection (p, pr);
				}
				if (sel != null) {
					sel.Drawable = this;
				}
				return sel;
			}
		}

		public override void Move (Selection sel, Point p, Point moveStart)
		{
			switch (sel.Position) {
			case SelectionPosition.TopLeft:
				Start = p;
				break;
			case SelectionPosition.BottomRight:
				Stop = p;
				break;
			case SelectionPosition.TopRight:
				Start.Y = p.Y;
				Stop.X = p.X;
				break;
			case SelectionPosition.BottomLeft:
				Start.X = p.X;
				Stop.Y = p.Y;
				break;
			case SelectionPosition.Top:
				Start.Y = p.Y;
				break;
			case SelectionPosition.Bottom:
				Stop.Y = p.Y;
				break;
			case SelectionPosition.Left:
				Start.X = p.X;
				break;
			case SelectionPosition.Right:
				Stop.X = p.X;
				break;
			case SelectionPosition.All:
				Start.X += p.X - moveStart.X;
				Start.Y += p.Y - moveStart.Y;
				Stop.X += p.X - moveStart.X;
				Stop.Y += p.Y - moveStart.Y;
				break;
			default:
				throw new Exception ("Unsupported move for line:  " + sel.Position);
			}
		}
	}
}

