//
//  Copyright (C) 2018 Fluendo S.A.
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
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Drawing;
using VAS.Drawing.CanvasObjects;

namespace VAS.Tests.Drawing
{
	[TestFixture]
	public class TestCanvasContainer
	{
		[Test]
		public void Add_Child_ChildAddedCollectionChangedTriggered ()
		{
			int collectionChangedCount = 0;
			CanvasContainer container = new CanvasContainer ();
			container.CollectionChanged += (sender, e) => collectionChangedCount++;
			CanvasObject co = new ButtonObject ();

			container.Add (co);

			Assert.AreEqual (1, collectionChangedCount);
			Assert.AreEqual (1, container.Count);
		}

		[Test]
		public void Add_ChildRedraw_RedrawForwarded ()
		{
			int redrawCount = 0;
			CanvasContainer container = new CanvasContainer ();
			container.RedrawEvent += (c, area) => redrawCount++;
			CanvasObject co = new ButtonObject ();
			container.Add (co);

			co.ReDraw ();

			Assert.AreEqual (1, redrawCount);
		}

		[Test]
		public void Remove_Existing_ChildRemovedAndCollectionChangedTriggered ()
		{
			int collectionChangedCount = 0;
			CanvasContainer container = new CanvasContainer ();
			CanvasObject co = new ButtonObject ();
			container.Add (co);
			container.CollectionChanged += (sender, e) => collectionChangedCount++;

			container.Remove (co);

			Assert.AreEqual (1, collectionChangedCount);
			Assert.AreEqual (0, container.Count);
		}

		[Test]
		public void Clear_Existing_AllChildRemoved ()
		{
			int collectionChangedCount = 0;
			CanvasContainer container = new CanvasContainer ();
			CanvasObject co = new ButtonObject ();
			container.Add (co);
			container.CollectionChanged += (sender, e) => collectionChangedCount++;

			container.Clear ();

			Assert.AreEqual (1, collectionChangedCount);
			Assert.AreEqual (0, container.Count);
		}

		[Test]
		public void GetEnumerator_WithChildren_EnumeratorMatchesChildren ()
		{
			CanvasContainer container = new CanvasContainer ();
			CanvasObject co = new ButtonObject ();

			container.Add (co);

			Assert.AreEqual (1, container.Count ());
		}

		[Test]
		public void Draw_WithChildren_DrawForwarded ()
		{
			CanvasContainer container = new CanvasContainer ();
			DummyObject co = new DummyObject ();
			container.Add (co);

			container.Draw (null, null);

			Assert.AreEqual (1, co.drawCount);
		}
	}

	class DummyObject : CanvasObject
	{
		public int drawCount = 0;

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			drawCount++;
		}
	}
}
