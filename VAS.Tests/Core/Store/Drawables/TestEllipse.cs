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
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Store.Drawables;

namespace VAS.Tests.Core.Store.Drawables
{
	[TestFixture ()]
	public class TestEllipse
	{
		[Test ()]
		public void TestSerialization ()
		{
			Point c = new Point (10, 10);
			Ellipse e = new Ellipse (c, 3, 2);
			Utils.CheckSerialization (e);
			Ellipse ne = Utils.SerializeDeserialize (e);
			Assert.AreEqual (ne.Center, e.Center);
			Assert.AreEqual (ne.AxisX, e.AxisX);
			Assert.AreEqual (ne.AxisY, e.AxisY);
		}

		[Test ()]
		public void TestAreaAxis ()
		{
			Point c = new Point (10, 10);
			Ellipse e = new Ellipse (c, 3, 2);
			Assert.AreEqual (new Point (7, 8), e.Area.TopLeft);
			Assert.AreEqual (new Point (13, 8), e.Area.TopRight);
			Assert.AreEqual (new Point (7, 12), e.Area.BottomLeft);
			Assert.AreEqual (new Point (13, 12), e.Area.BottomRight);
		}

		[Test ()]
		public void TestMove ()
		{
			// Move axis

			Point c = new Point (10, 10);
			Ellipse e = new Ellipse (c, 3, 2);

			Point p = new Point (5, 9);
			e.Move (SelectionPosition.Left, p, null);
			CompareEllipse (e, new Point (9, 10), 4, 2);

			p = new Point (7, 8);
			e.Move (SelectionPosition.Left, p, null);
			CompareEllipse (e, new Point (10, 10), 3, 2);

			p = new Point (9, 16);
			e.Move (SelectionPosition.Bottom, p, null);
			CompareEllipse (e, new Point (10, 12), 3, 4);

			p = new Point (6, 6);
			e.Move (SelectionPosition.Top, p, null);
			CompareEllipse (e, new Point (10, 11), 3, 5);

			p = new Point (11, 10);
			e.Move (SelectionPosition.TopLeft, p, null);
			CompareEllipse (e, new Point (12, 13), 1, 3);

			// Move all
			p = new Point (20, 12);
			e.Move (SelectionPosition.All, p, new Point (10, 10));
			CompareEllipse (e, new Point (22, 15), 1, 3);
		}

		[Test ()]
		public void TestMoveInvertAxis ()
		{
			// Move axis
			// Negative axis value means the axis is inverted

			Point c = new Point (10, 10);

			Ellipse e = new Ellipse (c, 3, 2);
			Point p = new Point (20, 12);
			e.Move (SelectionPosition.Left, p, null);
			CompareEllipse (e, new Point (16.5, 10), -3.5, 2);

			e = new Ellipse (c, 3, 2);
			p = new Point (12, 20);
			e.Move (SelectionPosition.Top, p, null);
			CompareEllipse (e, new Point (10, 16), 3, -4);

			e = new Ellipse (c, 3, 2);
			p = new Point (20, 20);
			e.Move (SelectionPosition.TopLeft, p, null);
			CompareEllipse (e, new Point (16.5, 16), -3.5, -4);
		}

		public void CompareEllipse (Ellipse e, Point p, double axisX, double axisY)
		{
			Ellipse e2 = new Ellipse (p, axisX, axisY);

			Assert.AreEqual (e2.Center.X, e.Center.X);
			Assert.AreEqual (e2.Center.Y, e.Center.Y);
			Assert.AreEqual (e2.AxisX, e.AxisX);
			Assert.AreEqual (e2.AxisY, e.AxisY);
		}
	}
}

