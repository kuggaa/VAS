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
	public class TestCounter
	{
		[Test ()]
		public void Serialization_NewCounter_SerializedAndDeserializedCounterEqualsUnlessJsonIgnoredProperties ()
		{
			Point targetCenterPoint = new Point (0, 0);
			var targetRadius = 5.0D;
			var count = 2;
			var targetColor = Color.White;
			var targetStrokeColor = new Color (120, 120, 120);
			///Arrange

			var targetCounter = new Counter (targetCenterPoint, targetRadius, count) {
				FillColor = targetColor,
				IsChanged = true,
				LineWidth = 1,
				Selected = false,
				StrokeColor = targetStrokeColor,
				Style = LineStyle.Dashed,
				IgnoreEvents = false
			};

			///Act

			var deserializedTargetCounter = Utils.SerializeDeserialize (targetCounter);

			///Assert

			Utils.AssertDeserializedStorablePropertyEquality<Counter> (targetCounter, deserializedTargetCounter);
		}

		[Test ()]
		public void Count_NewCounter_CountSetOk ()
		{
			Point targetCenterPoint = new Point (0, 0);
			var targetRadius = 5.0D;
			var count = 2;
			var targetColor = Color.White;
			var targetStrokeColor = new Color (120, 120, 120);
			///Arrange

			var targetCounter = new Counter (targetCenterPoint, targetRadius, count) {
				FillColor = targetColor,
				IsChanged = true,
				LineWidth = 1,
				Selected = false,
				StrokeColor = targetStrokeColor,
				Style = LineStyle.Dashed,
				IgnoreEvents = false
			};

			///Act


			///Assert

			Assert.IsTrue (targetCounter.Count == 2);
		}
	}
}

