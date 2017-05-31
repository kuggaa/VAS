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
using Newtonsoft.Json;
using VAS.Core.Common;

namespace VAS.Core.Store.Drawables
{
	[Serializable]
	public class Ellipse : Drawable
	{
		// When creating a new Ellipse, we store here the originating point so that we
		// can calculate more easilly the center and radius based on this start point.
		Point startPoint;

		public Ellipse ()
		{
		}

		public Ellipse (Point center, double axisX, double axisY, string text = null)
		{
			startPoint = center.Copy ();
			Center = center;
			AxisX = axisX;
			AxisY = axisY;
			Text = text;
		}

		public Point Center {
			get;
			set;
		}

		public virtual double AxisX {
			get;
			set;
		}

		public virtual double AxisY {
			get;
			set;
		}

		public string Text {
			get;
			set;
		}

		public Color TextColor {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Point Top {
			get {
				return new Point (Center.X, Center.Y + AxisY);
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Point Bottom {
			get {
				return new Point (Center.X, Center.Y - AxisY);
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Point Left {
			get {
				return new Point (Center.X - AxisX, Center.Y);
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Point Right {
			get {
				return new Point (Center.X + AxisX, Center.Y);
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public override Area Area {
			get {
				return new Area (new Point (Center.X - AxisX, Center.Y - AxisY),
					AxisX * 2, AxisY * 2);
			}
		}

		public override Selection GetSelection (Point p, double pr = 0.05, bool inMotion = false)
		{
			double d;

			if (Selected) {
				return base.GetSelection (p, pr);
			}

			if (MatchPoint (Top, p, pr, out d)) {
				return new Selection (this, SelectionPosition.Top, d);
			} else if (MatchPoint (Bottom, p, pr, out d)) {
				return new Selection (this, SelectionPosition.Bottom, d);
			} else if (MatchPoint (Left, p, pr, out d)) {
				return new Selection (this, SelectionPosition.Left, d);
			} else if (MatchPoint (Right, p, pr, out d)) {
				return new Selection (this, SelectionPosition.Right, d);
			} else {
				/* Ellipse equation */
				double a = Math.Pow (p.X - Center.X, 2) / Math.Pow (AxisX, 2);
				double b = Math.Pow (p.Y - Center.Y, 2) / Math.Pow (AxisY, 2);
				if ((a + b) <= 1) {
					return new Selection (this, SelectionPosition.All, p.Distance (Center));
				} else {
					return null;
				}
			}
		}

		public override void Move (Selection sel, Point p, Point moveStart)
		{
			switch (sel.Position) {
			case SelectionPosition.Top:
			case SelectionPosition.Bottom: {
					AxisY = Math.Abs (p.Y - Center.Y);
					break;
				}
			case SelectionPosition.Left:
			case SelectionPosition.Right: {
					AxisX = Math.Abs (p.X - Center.X);
					break;
				}
			case SelectionPosition.TopLeft:
			case SelectionPosition.TopRight:
			case SelectionPosition.BottomLeft:
			case SelectionPosition.BottomRight: {
					AxisX = Math.Abs (p.X - Center.X);
					AxisY = Math.Abs (p.Y - Center.Y);
					break;
				}
			case SelectionPosition.All: {
					Center.X += p.X - moveStart.X;
					Center.Y += p.Y - moveStart.Y;
					break;
				}
			case SelectionPosition.New: {
					if (startPoint == null) {
						throw new Exception ("Unsupported move for ellipse, start point was not set :  " + sel.Position);
					}
					var ax = (p.X - startPoint.X) / 2;
					var ay = (p.Y - startPoint.Y) / 2;
					Center.X = startPoint.X + ax;
					Center.Y = startPoint.Y + ay;

					AxisX = Math.Max (1, Math.Abs (ax));
					AxisY = Math.Max (1, Math.Abs (ay));
					break;
				}
			default:
				throw new Exception ("Unsupported move for line:  " + sel.Position);
			}
		}
	}
}

