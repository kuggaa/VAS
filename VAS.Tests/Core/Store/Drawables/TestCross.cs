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
	public class TestCross
	{
		[Test ()]
		public void Serialization_NewCross_SerializedAndDeserializedCrossEqualsUnlessJsonIgnoredProperties ()
		{
			///Arrange

			var targetColor = new Color (250, 250, 250);
			var targetStrokeColor = new Color (120, 120, 120);
			var targetStartPoint = new Point (0.0, 0.0);
			var targetEndPoint = new Point (10.0, 10.0);

			var targetCross = new Cross (targetStartPoint, targetEndPoint, LineStyle.Dashed) {
				FillColor = targetColor,
				IsChanged = true,
				LineWidth = 1,
				Selected = false,
				StrokeColor = targetStrokeColor,
				Style = LineStyle.Dashed,
				IgnoreEvents = false
			};

			///Act

			var deserializedTargetCross = Utils.SerializeDeserialize (targetCross);

			///Assert

			Utils.AssertDeserializedStorablePropertyEquality<Cross> (targetCross, deserializedTargetCross);
		}

		[Test ()]
		public void Move_NewCross_CrossMoved ()
		{
			///Arrange

			var targetColor = new Color (250, 250, 250);
			var targetStartPoint = new Point (0.0, 0.0);
			var targetEndPoint = new Point (10.0, 10.0);

			var targetCross = new Cross (targetStartPoint, targetEndPoint, LineStyle.Dashed) {
				FillColor = targetColor,
			};

			///Act

			var selection = new Selection (targetCross, SelectionPosition.All);
			targetCross.Move (selection, targetEndPoint, targetCross.Start.Copy ());

			///Assert

			Assert.AreEqual (10.0, targetCross.Start.X);
			Assert.AreEqual (10.0, targetCross.Start.Y);
			Assert.AreEqual (20.0, targetCross.Stop.X);
			Assert.AreEqual (20.0, targetCross.Stop.Y);
		}

		[Test ()]
		public void Area_NewCross_AreaOk ()
		{
			///Arrange

			var targetColor = new Color (250, 250, 250);
			var targetStartPoint = new Point (0.0, 0.0);
			var targetEndPoint = new Point (10.0, 10.0);

			var targetCross = new Cross (targetStartPoint, targetEndPoint, LineStyle.Dashed) {
				FillColor = targetColor,
			};

			///Act

			var area = targetCross.Area;

			///Assert

			Assert.AreEqual (10.0, area.Bottom);
			Assert.AreEqual (0.0, area.Top);
			Assert.AreEqual (0.0, area.Left);
			Assert.AreEqual (10.0, area.Right);
		}

		[Test ()]
		public void GetSelection_NewCross_GetAllSelectionPositionsOk ()
		{
			///Arrange

			var targetColor = new Color (250, 250, 250);
			var targetStartPoint = new Point (0.0, 0.0);
			var targetEndPoint = new Point (10.0, 10.0);

			var targetCross = new Cross (targetStartPoint, targetEndPoint, LineStyle.Dashed) {
				FillColor = targetColor,
			};

			var topLeftPoint = targetCross.Area.TopLeft;
			var bottomLeftPoint = targetCross.Area.BottomLeft;
			var topRightPoint = targetCross.Area.TopRight;
			var bottomRightPoint = targetCross.Area.BottomRight;

			///Act

			var topLeftSelection = targetCross.GetSelection (topLeftPoint);
			var bottomLeftSelection = targetCross.GetSelection (bottomLeftPoint);
			var topRightSelection = targetCross.GetSelection (topRightPoint);
			var bottomRightSelection = targetCross.GetSelection (bottomRightPoint);

			///Assert

			Assert.AreEqual (SelectionPosition.TopLeft, topLeftSelection.Position);
			Assert.AreEqual (SelectionPosition.BottomLeft, bottomLeftSelection.Position);
			Assert.AreEqual (SelectionPosition.TopRight, topRightSelection.Position);
			Assert.AreEqual (SelectionPosition.BottomRight, bottomRightSelection.Position);
		}

	}
}

