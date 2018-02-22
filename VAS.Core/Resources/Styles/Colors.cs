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
using VAS.Core.Common;

namespace VAS.Core.Resources.Styles
{
	public class Colors
	{
		public static Color NewTeamsFontColor => App.Current.Style.ColorWhite;
		public static Color ButtonTagColor = Color.Parse ("#d8ffc7");
		public static Color ButtonTimerColor = Color.Parse ("#bebbff");
		public static Color ButtonScoreColor = Color.Parse ("#d8ffc7");
		public static Color ButtonPenaltyColor = Color.Parse ("#ffc7f0");
		public static Color ButtonEventColor = Color.Parse ("#c7e9ff");

		public static Color CanvasSelectionBorder => App.Current.Style.ColorGray;
		public static Color CanvasSelectionShadow => App.Current.Style.ColorShadow;
		public static Color CanvasSelectionAnchor => App.Current.Style.ColorWhite;
		public static Color DefaultShield => App.Current.Style.ThemeSecondary;

		// FIXME: add a method to obtain an alpha version of a color directly and a way to use it XAML
		public static Color NavigationTopBarContrast {
			get {
				Color color = App.Current.Style.ThemeContrastBase.Copy ();
				color.SetAlpha (0.95F);
				return color;
			}
		}

		// Alphas
		public static float AlphaImageSensitive = 1f;
		public static float AlphaImageNoSensitive = 0.4f;
	}
}
