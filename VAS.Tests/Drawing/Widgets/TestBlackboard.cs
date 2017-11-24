//
//  Copyright (C) 2017 Fluendo S.A.
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
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;
using VAS.Drawing.Widgets;

namespace VAS.Tests.Drawing.Widgets
{
	[TestFixture]
	public class TestBlackboard
	{
		Mock<IWidget> widgetMock;
		Blackboard blackboard;

		[OneTimeSetUp]
		public void OneTimeSetUp ()
		{
			SetupClass.SetUp ();
			VAS.Drawing.DrawingInit.ScanViews ();
		}

		[SetUp]
		public void SetUp ()
		{
			widgetMock = new Mock<IWidget> ();
			widgetMock.SetupAllProperties ();
			IWidget widget = widgetMock.Object;
			widget.Width = 100;
			widget.Height = 100;
			blackboard = new Blackboard (widget);
			blackboard.Background = Utils.LoadImageFromFile ();
		}

		[Test]
		public void DeleteSelection_1ElementSelected_ElementDeletedFromCanvasAndModel ()
		{
			var drawing = new FrameDrawing ();
			var cross = new Cross ();
			var line = new Line ();
			drawing.Drawables.Add (cross);
			drawing.Drawables.Add (line);
			blackboard.Drawing = drawing;
			blackboard.UpdateSelection (new Selection (blackboard.Objects [0] as IMovableObject, SelectionPosition.All));

			blackboard.DeleteSelection ();

			Assert.AreEqual (1, blackboard.Objects.Count);
			Assert.AreEqual (1, drawing.Drawables.Count);
		}
	}
}
