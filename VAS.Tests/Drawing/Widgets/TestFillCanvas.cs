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
using Gtk;
using Moq;
using NUnit.Framework;
using VAS.Core.Interfaces.Drawing;
using VAS.Drawing;
using VAS.Drawing.Cairo;
using VAS.Drawing.CanvasObjects.Statistics;

namespace VAS.Tests.Drawing.Widgets
{
	[TestFixture]
	public class TestFillCanvas
	{
		FillCanvas fillCanvas;
		Mock<IWidget> widgetMock;

		[SetUp]
		public void Setup ()
		{
			widgetMock = new Mock<IWidget> ();
			widgetMock.SetupGet (w => w.Height).Returns (100);
			widgetMock.SetupGet (w => w.Width).Returns (100);

			fillCanvas = new FillCanvas (widgetMock.Object);
		}

		[Test]
		public void SizeChangedEvent_FixedSizeCanvasObjectAdded_FixedSizeCanvasObjectFillsWidget ()
		{
			//Arrange
			var fixedSizeObject = new PercentCircularChartView ();
			fillCanvas.AddObject (fixedSizeObject);

			//Act
			widgetMock.Raise (obj => obj.SizeChangedEvent += null);

			//Assert
			Assert.AreEqual (1, fillCanvas.Objects.Count);
			Assert.AreEqual (100, fixedSizeObject.Width);
			Assert.AreEqual (100, fixedSizeObject.Height);
		}
	}
}
