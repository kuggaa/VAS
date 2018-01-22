//
//  Copyright (C) 2018 
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
using VAS.Core.ViewModel.Statistics;

namespace VAS.Tests.Core.ViewModel
{
	[TestFixture]
	public class TestPercentCircularChartVM
	{
		PercentCircularChartVM sut;

		[SetUp]
		public void SetUp () 
		{
			var percent = new SeriesVM ();
			sut = new PercentCircularChartVM (percent);
		}

		[Test]
		public void ChangeElements_TotalNotChanged_PercentValueWellUpdated ()
		{
			// Arrange
			sut.TotalElements = 10;
			sut.PercentSerie.Elements = 4;

			// Act
			sut.PercentSerie.Elements = 5;

			// Assert
			Assert.AreEqual (50.0f, sut.PercentValue);
		}

		[Test]
		public void ChangeTotal_ElementsNotChanged_PercentValueWellUpdated ()
		{
			// Arrange
			sut.TotalElements = 10;
			sut.PercentSerie.Elements = 5;

			// Act
			sut.TotalElements = 20;

			// Assert
			Assert.AreEqual (25, sut.PercentValue);
		}
	}
}