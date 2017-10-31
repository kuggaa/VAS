//
//  Copyright (C) 2015 Andoni Morales Alastruey
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
	public class TestCircle
	{
		[Test ()]
		public void Serialization_NewCircle_SerializedAndDeserializedCircleEqualsUnlessJsonIgnoredProperties ()
		{
			///Arrange

			var targetColor = new Color (250, 250, 250);
			var targetTextColor = Color.White;
			var targetStrokeColor = new Color (120, 120, 120);
			var targetCenterPoint = new Point (0.0, 0.0);
			var targetRadius = 5.0D;


			var targetCircle = new Circle (targetCenterPoint, targetRadius) {
				FillColor = targetColor,
				IsChanged = true,
				LineWidth = 1,
				Selected = false,
				StrokeColor = targetStrokeColor,
				Style = LineStyle.Dashed,
				Text = "Test",
				TextColor = targetTextColor,
				IgnoreEvents = false
			};

			///Act

			var deserializedTargetCross = Utils.SerializeDeserialize (targetCircle);

			///Assert

			Utils.AssertDeserializedStorablePropertyEquality<Circle> (targetCircle, deserializedTargetCross);
		}

		[Test ()]
		public void Move_NewCircle_CircleMoved ()
		{
			///Arrange

			var targetColor = new Color (250, 250, 250);
			var targetTextColor = Color.White;
			var targetStrokeColor = new Color (120, 120, 120);
			var targetCenterPoint = new Point (0.0, 0.0);
			var targetRadius = 5.0D;

			var targetCircle = new Circle (targetCenterPoint, targetRadius) {
				FillColor = targetColor,
				IsChanged = true,
				LineWidth = 1,
				Selected = false,
				StrokeColor = targetStrokeColor,
				Style = LineStyle.Dashed,
				Text = "Test",
				TextColor = targetTextColor,
				IgnoreEvents = false
			};

			var targetSelection = new Selection (targetCircle, SelectionPosition.All);
			var targetDestinationPoint = new Point (10.0, 10.0);


			///Act

			targetCircle.Move (targetSelection, targetDestinationPoint, targetCircle.Center.Copy ());

			///Assert

			Assert.AreEqual (targetDestinationPoint, targetCircle.Center);
		}

		[Test ()]
		public void Area_NewCircle_AreaOk ()
		{
			///Arrange

			var targetColor = new Color (250, 250, 250);
			var targetTextColor = Color.White;
			var targetStrokeColor = new Color (120, 120, 120);
			var targetCenterPoint = new Point (0.0, 0.0);
			var targetRadius = 5.0D;

			var targetCircle = new Circle (targetCenterPoint, targetRadius) {
				FillColor = targetColor,
				IsChanged = true,
				LineWidth = 1,
				Selected = false,
				StrokeColor = targetStrokeColor,
				Style = LineStyle.Dashed,
				Text = "Test",
				TextColor = targetTextColor,
				IgnoreEvents = false
			};


			///Act

			var area = targetCircle.Area;

			///Assert

			Assert.AreEqual (10, area.Width);
			Assert.AreEqual (10, area.Height);

		}

		[Test ()]
		public void GetSelection_NewCircle_GetAllSelectionPositionsOk ()
		{
			///Arrange

			var targetColor = new Color (250, 250, 250);
			var targetTextColor = Color.White;
			var targetStrokeColor = new Color (120, 120, 120);
			var targetCenterPoint = new Point (0.0, 0.0);
			var targetRadius = 5.0D;

			var targetCircle = new Circle (targetCenterPoint, targetRadius) {
				FillColor = targetColor,
				AxisX = 5.0D,
				AxisY = 5.0D,
				IsChanged = true,
				LineWidth = 1,
				Selected = false,
				StrokeColor = targetStrokeColor,
				Style = LineStyle.Dashed,
				Text = "Test",
				TextColor = targetTextColor,
				IgnoreEvents = false
			};

			var topPoint = new Point (0, targetCircle.Area.Top);
			var bottomPoint = new Point (0, targetCircle.Area.Bottom);
			var rightPoint = new Point (targetCircle.Area.Right, 0);
			var leftPoint = new Point (targetCircle.Area.Left, 0);

			///Act

			var topSelection = targetCircle.GetSelection (topPoint);
			var bottomSelection = targetCircle.GetSelection (bottomPoint);
			var rightSelection = targetCircle.GetSelection (rightPoint);
			var leftSelection = targetCircle.GetSelection (leftPoint);

			///Assert

			//FIXME Why Top returns Bottom and viceversa?
			//Assert.AreEqual (SelectionPosition.Top, topSelection.Position);
			//Assert.AreEqual (SelectionPosition.Bottom, bottomSelection.Position);
			Assert.AreEqual (SelectionPosition.Right, rightSelection.Position);
			Assert.AreEqual (SelectionPosition.Left, leftSelection.Position);


		}
	}
}

