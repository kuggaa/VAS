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
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Resources.Styles;

namespace VAS.Tests.Core.Common
{
	[TestFixture]
	public class TestWatermark
	{
		[Test]
		public void ConfigureWatermark_TopLeftP720 ()
		{
			VideoStandard videoStandard = VideoStandards.P720;
			Watermark watermark = Watermark.ConfigureNewWatermark (WatermarkPosition.TOP_LEFT, videoStandard);

			var expectedOffsetX = Sizes.WatermarkPadding / (double)videoStandard.Width;
			var expectedOffsetY = Sizes.WatermarkPadding / (double)videoStandard.Height;
			int expectedHeight = (int)(videoStandard.Height * Sizes.WatermarkHeightNormalization);

			Assert.AreEqual (expectedOffsetX, watermark.OffsetX);
			Assert.AreEqual (expectedOffsetY, watermark.OffsetY);
			Assert.AreEqual (expectedHeight, watermark.Image.Height);
		}

		[Test]
		public void ConfigureWatermark_TopRightP720 ()
		{
			int expectedHeight, expectedWidth;
			VideoStandard videoStandard = VideoStandards.P720;
			Watermark watermark = Watermark.ConfigureNewWatermark (WatermarkPosition.TOP_RIGHT, videoStandard);

			expectedHeight = expectedWidth = (int)(videoStandard.Height * Sizes.WatermarkHeightNormalization);
			var expectedOffsetX = (videoStandard.Width - expectedWidth - Sizes.WatermarkPadding) / (double)videoStandard.Width;
			var expectedOffsetY = Sizes.WatermarkPadding / (double)videoStandard.Height;


			Assert.AreEqual (expectedOffsetX, watermark.OffsetX);
			Assert.AreEqual (expectedOffsetY, watermark.OffsetY);
			Assert.AreEqual (expectedHeight, watermark.Image.Height);
		}

		[Test]
		public void ConfigureWatermark_BottomLeftP720 ()
		{
			VideoStandard videoStandard = VideoStandards.P720;
			Watermark watermark = Watermark.ConfigureNewWatermark (WatermarkPosition.BOTTOM_LEFT, videoStandard);

			int expectedHeight = (int)(videoStandard.Height * Sizes.WatermarkHeightNormalization);
			var expectedOffsetX = Sizes.WatermarkPadding / (double)videoStandard.Width;
			var expectedOffsetY = (videoStandard.Height - expectedHeight - Sizes.WatermarkPadding) / (double)videoStandard.Height;


			Assert.AreEqual (expectedOffsetX, watermark.OffsetX);
			Assert.AreEqual (expectedOffsetY, watermark.OffsetY);
			Assert.AreEqual (expectedHeight, watermark.Image.Height);
		}

		[Test]
		public void ConfigureWatermark_BottomRightP720 ()
		{
			int expectedHeight, expectedWidth;
			VideoStandard videoStandard = VideoStandards.P720;
			Watermark watermark = Watermark.ConfigureNewWatermark (WatermarkPosition.BOTTOM_RIGHT, videoStandard);

			expectedHeight = expectedWidth = (int)(videoStandard.Height * Sizes.WatermarkHeightNormalization);
			var expectedOffsetX = (videoStandard.Width - expectedWidth - Sizes.WatermarkPadding) / (double)videoStandard.Width;
			var expectedOffsetY = (videoStandard.Height - expectedHeight - Sizes.WatermarkPadding) / (double)videoStandard.Height;


			Assert.AreEqual (expectedOffsetX, watermark.OffsetX);
			Assert.AreEqual (expectedOffsetY, watermark.OffsetY);
			Assert.AreEqual (expectedHeight, watermark.Image.Height);
		}

		[Test]
		public void ConfigureWatermark_BottomRightP480 ()
		{
			int expectedHeight, expectedWidth;
			VideoStandard videoStandard = VideoStandards.P480;
			Watermark watermark = Watermark.ConfigureNewWatermark (WatermarkPosition.BOTTOM_RIGHT, videoStandard);

			expectedHeight = expectedWidth = (int)(videoStandard.Height * Sizes.WatermarkHeightNormalization);
			var expectedOffsetX = (videoStandard.Width - expectedWidth - Sizes.WatermarkPadding) / (double)videoStandard.Width;
			var expectedOffsetY = (videoStandard.Height - expectedHeight - Sizes.WatermarkPadding) / (double)videoStandard.Height;


			Assert.AreEqual (expectedOffsetX, watermark.OffsetX);
			Assert.AreEqual (expectedOffsetY, watermark.OffsetY);
			Assert.AreEqual (expectedHeight, watermark.Image.Height);
		}

		[Test]
		public void ConfigureWatermark_BottomRightP1080 ()
		{
			int expectedHeight, expectedWidth;
			VideoStandard videoStandard = VideoStandards.P1080;
			Watermark watermark = Watermark.ConfigureNewWatermark (WatermarkPosition.BOTTOM_RIGHT, videoStandard);

			expectedHeight = expectedWidth = (int)(videoStandard.Height * Sizes.WatermarkHeightNormalization);
			var expectedOffsetX = (videoStandard.Width - expectedWidth - Sizes.WatermarkPadding) / (double)videoStandard.Width;
			var expectedOffsetY = (videoStandard.Height - expectedHeight - Sizes.WatermarkPadding) / (double)videoStandard.Height;


			Assert.AreEqual (expectedOffsetX, watermark.OffsetX);
			Assert.AreEqual (expectedOffsetY, watermark.OffsetY);
			Assert.AreEqual (expectedHeight, watermark.Image.Height);
		}
	}
}
