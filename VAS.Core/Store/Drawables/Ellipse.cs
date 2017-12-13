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
	public class Ellipse : Rectangle
	{
		bool ignoreChanges;
		Point center;
		double axisX;
		double axisY;

		public Ellipse ()
		{
		}

		public Ellipse (Point center, double axisX, double axisY, string text = null)
		{
			ignoreChanges = true;
			Center = center;
			AxisX = axisX;
			AxisY = axisY;
			ignoreChanges = false;
			Update ();
			Text = text;
		}

		public new Point Center {
			get {
				return center;
			}
			set {
				center = value;
				Update ();
			}
		}

		public virtual double AxisX {
			get {
				return axisX;
			}
			set {
				axisX = value;
				Update ();
			}
		}

		public virtual double AxisY {
			get {
				return axisY;
			}

			set {
				axisY = value;
				Update ();
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public override Point TopLeft => base.TopLeft;

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public override Point TopRight => base.TopRight;

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public override Point BottomLeft => base.BottomLeft;

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public override Point BottomRight => base.BottomRight;

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public override Area Area {
			get {
				return new Area (new Point (Center.X - AxisX, Center.Y - AxisY),
					AxisX * 2, AxisY * 2);
			}
		}

		public string Text {
			get;
			set;
		}

		public Color TextColor {
			get;
			set;
		}

		private void Update ()
		{
			if (ignoreChanges)
				return;

			base.Update (new Point (Center.X - AxisX, Center.Y - AxisY),
								AxisX * 2,
								AxisY * 2);
		}

		public override void Reorder ()
		{
			base.Reorder ();

			ignoreChanges = true;
			AxisX = Width / 2;
			AxisY = Height / 2;
			Center = base.Center;
			ignoreChanges = false;

			Update ();
		}

		public override Selection GetSelection (Point p, double pr = 0.05, bool inMotion = false)
		{
			var selection = base.GetSelection (p, pr);

			if (selection != null && (Selected || selection.Position != SelectionPosition.All))
				return selection;

			/* Ellipse equation */
			double a = Math.Pow (p.X - Center.X, 2) / Math.Pow (AxisX, 2);
			double b = Math.Pow (p.Y - Center.Y, 2) / Math.Pow (AxisY, 2);
			if ((a + b) <= 1) {
				return new Selection (this, SelectionPosition.All, p.Distance (Center));
			} else {
				return null;
			}
		}

		public override void Move (Selection sel, Point p, Point moveStart)
		{
			base.Move (sel, p, moveStart);

			ignoreChanges = true;
			AxisX = Width / 2;
			AxisY = Height / 2;
			Center = base.Center;
			ignoreChanges = false;

			Update ();
		}
	}
}

