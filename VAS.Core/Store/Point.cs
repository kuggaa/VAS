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
using VAS.Core.Interfaces;
using VAS.Core.MVVMC;

namespace VAS.Core.Common
{

	[Serializable]
	public class Point : BindableBase
	{

		public Point (double x, double y)
		{
			X = x;
			Y = y;
		}

		public double X {
			get;
			set;
		}

		public double Y {
			get;
			set;
		}

		public double Distance (Point p)
		{
			return Math.Sqrt (Math.Pow (X - p.X, 2) + Math.Pow (Y - p.Y, 2));
		}

		public Point Normalize (int width, int height)
		{
			return new Point (
				X.Clamp (0, width) / width,
				Y.Clamp (0, height) / height
			);

		}

		public Point Denormalize (int width, int height)
		{
			return new Point (X * width, Y * height);
		}

		public Point Copy ()
		{
			return new Point (X, Y);
		}

		public override string ToString ()
		{
			return string.Format ("[Point: X={0}, Y={1}]", X, Y);
		}

		public override bool Equals (object obj)
		{
			Point p = obj as Point;
			if (p == null)
				return false;

			return p.X == X && p.Y == Y;
		}

		/// <summary>
		/// Determinates if the point is inside the given area.
		/// </summary>
		/// <returns><c>true</c>, if is inside area, <c>false</c> otherwise.</returns>
		/// <param name="area">Area.</param>
		public bool IsInsideArea (Area area)
		{
			return IsInsideArea (area.Start, area.Width, area.Height);
		}

		/// <summary>
		/// Determinates if the point is inside the area determinated by point, width and height.
		/// </summary>
		/// <returns><c>true</c>, if is inside area, <c>false</c> otherwise.</returns>
		/// <param name="p">Start point.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public bool IsInsideArea (Point p, double width, double height)
		{
			if (this.X > p.X && this.X < p.X + width &&
				this.Y > p.Y && this.Y < p.Y + height) {
				return true;
			}
			return false;
		}

		public static bool operator == (Point p1, Point p2)
		{
			if (Object.ReferenceEquals (p1, p2)) {
				return true;
			}

			if ((object)p1 == null || (object)p2 == null) {
				return false;
			}

			return p1.Equals (p2);
		}

		public static bool operator != (Point p1, Point p2)
		{
			return !(p1 == p2);
		}

		public override int GetHashCode ()
		{
			return (X.ToString () + "-" + Y.ToString ()).GetHashCode ();
		}

		public static Point operator + (Point p1, Point p2)
		{
			return new Point (p1.X + p2.X, p1.Y + p2.Y);
		}

		public static Point operator - (Point p1, Point p2)
		{
			return new Point (p1.X - p2.X, p1.Y - p2.Y);
		}
	}
}
