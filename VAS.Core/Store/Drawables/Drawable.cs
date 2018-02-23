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
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;

namespace VAS.Core.Store.Drawables
{
	[Serializable]
	public abstract class Drawable : BindableBase, IBlackboardObject
	{
		public Drawable ()
		{
		}

		public virtual Color StrokeColor {
			get;
			set;
		}

		public virtual int LineWidth {
			get;
			set;
		}

		public virtual Color FillColor {
			get;
			set;
		}

		public virtual bool Selected {
			get;
			set;
		}

		public virtual LineStyle Style {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public virtual Area Area {
			get;
			protected set;
		}

		/// <summary>
		/// This method should be overridden by drawables that requires reordering its axis after one of them has been moved,
		/// like for example a rectangle where after moving the bottom right it can become to top left.
		/// </summary>
		public virtual void Reorder ()
		{
		}

		/// <summary>
		/// Gets the selection of drawable depending on point position respect the drawable bounding box
		/// </summary>
		/// <returns>The selection.</returns>
		/// <param name="point">Point.</param>
		/// <param name="pr">Pr.</param>
		/// <param name="inMotion">If set to <c>true</c> in motion.</param>
		public virtual Selection GetSelection (Point point, double pr, bool inMotion = false)
		{
			Point [] vertices;
			double d;

			if (Area == null) {
				return null;
			}

			if ((point.X < Area.Start.X - pr) ||
				(point.X > Area.Start.X + Area.Width + pr) ||
				(point.Y < Area.Start.Y - pr) ||
				(point.Y > Area.Start.Y + Area.Height + pr)) {
				return null;
			}

			/* Check vertices */
			vertices = Area.Vertices;
			if (MatchPoint (vertices [0], point, pr, out d)) {
				return new Selection (this, SelectionPosition.TopLeft, d);
			} else if (MatchPoint (vertices [1], point, pr, out d)) {
				return new Selection (this, SelectionPosition.TopRight, d);
			} else if (MatchPoint (vertices [2], point, pr, out d)) {
				return new Selection (this, SelectionPosition.BottomRight, d);
			} else if (MatchPoint (vertices [3], point, pr, out d)) {
				return new Selection (this, SelectionPosition.BottomLeft, d);
			}

			vertices = Area.VerticesCenter;
			if (MatchPoint (vertices [0], point, pr, out d)) {
				return new Selection (this, SelectionPosition.Top, d);
			} else if (MatchPoint (vertices [1], point, pr, out d)) {
				return new Selection (this, SelectionPosition.Right, d);
			} else if (MatchPoint (vertices [2], point, pr, out d)) {
				return new Selection (this, SelectionPosition.Bottom, d);
			} else if (MatchPoint (vertices [3], point, pr, out d)) {
				return new Selection (this, SelectionPosition.Left, d);
			}

			return new Selection (this, SelectionPosition.All, point.Distance (Area.Center));
		}

		/// <summary>
		/// Moves drawable from start Point to dst Point given a Selection.
		/// This method is mandatory to be overriden.
		/// </summary>
		/// <returns>The move.</returns>
		/// <param name="s">S.</param>
		/// <param name="dst">Dst.</param>
		/// <param name="start">Start.</param>
		public abstract void Move (Selection s, Point dst, Point start);

		public void Move (SelectionPosition s, Point dst, Point start)
		{
			Move (new Selection (null, s, 0), dst, start);
		}

		/// <summary>
		/// Returns if Point p1 and Point p2 are equivalent by comparing their respective distances with a given precision.
		/// </summary>
		/// <returns><c>true</c>, if point was matched, <c>false</c> otherwise.</returns>
		/// <param name="p1">P1.</param>
		/// <param name="p2">P2.</param>
		/// <param name="precision">Precision.</param>
		/// <param name="accuracy">Accuracy.</param>
		public static bool MatchPoint (Point p1, Point p2, double precision, out double accuracy)
		{
			accuracy = p1.Distance (p2);
			return accuracy <= precision;
		}

		public static bool MatchAxis (double c1, double c2, double precision, out double accuracy)
		{
			accuracy = Math.Abs (c1 - c2);
			return accuracy <= precision;
		}
	}
}

