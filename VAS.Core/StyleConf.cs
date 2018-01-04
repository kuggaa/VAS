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
using VAS.Core.Common;
using VAS.Core.Serialization;

namespace VAS.Core.Common
{
	// FIXME: Split this in LongoMatch-specific & VAS styles
	public class StyleConf
	{
		public const LineStyle DrawingSelectorLineStyle = LineStyle.Normal;

		//Button Styles
		public const string ButtonTimeline = "ButtonTimeline";
		public const string ButtonNormal = "ButtonNormal";
		public const string ButtonTab = "ButtonTab";
		public const string ButtonRATab = "ButtonRATab";
		public const string ButtonRemove = "ButtonRemove";
		public const string ButtonFocus = "ButtonFocus";
		public const string ButtonDialog = "ButtonDialog";
		public const string ButtonCallToActionRounded = "ButtonCallToActionRounded";
		public const string ButtonRegular = "ButtonRegular";

		//Limits Styles
		public const string LabelLimit = "LabelLimit";
		public const string BoxLimit = "BoxLimit";
		public const string ButtonLimit = "ButtonLimit";

		//Video Player Tooltips
		public static string PlayerTooltipZoom = Catalog.GetString ("Zoom");
		public static string PlayerTooltipClose = Catalog.GetString ("Close loaded event");
		public static string PlayerTooltipPrevious = Catalog.GetString ("Previous");
		public static string PlayerTooltipNext = Catalog.GetString ("Next");
		public static string PlayerTooltipPlay = Catalog.GetString ("Play");
		public static string PlayerTooltipPause = Catalog.GetString ("Pause");
		public static string PlayerTooltipDraw = Catalog.GetString ("Draw Frame");
		public static string PlayerTooltipVolume = Catalog.GetString ("Volume");
		public static string PlayerTooltipRate = Catalog.GetString ("Playback speed");
		public static string PlayerTooltipJumps = Catalog.GetString ("Jump in seconds. Hold the Shift key with the direction keys to activate it.");
		public static string PlayerTooltipDetach = Catalog.GetString ("Detach window");

		public StyleConf ()
		{
			HomeTeamColor = Color.Red;
			AwayTeamColor = Color.Blue;
			ScreenBase = Color.Black;
			ThemeContrastDisabled = Color.Black;
			PaletteBackgroundSemiLight = Color.Black;
			ThemeBase = Color.Black;
			ThemeContrastSecondary = Color.Black;
			TextBase = Color.Black;
			ColorPrimary = Color.Black;
			TextBase = Color.Black;
			SoftDim = 0.9;
		}

		#region Properties


		//Colors
		// New colors pallete --------------------------------------------------

		public Color ScreenBase { get; set; }

		public Color ThemeBase { get; set; }
		public Color ThemeSecondary { get; set; }
		public Color ThemeDisabled { get; set; }
		public Color ThemeContrastBase { get; set; }
		public Color ThemeContrastSecondary { get; set; }
		public Color ThemeContrastDisabled { get; set; }

		public Color TextBrand { get; set; }
		public Color TextPrimary { get; set; }
		public Color TextSecondary { get; set; }
		public Color TextAccentSuccess { get; set; }
		public Color TextAccentWarning { get; set; }
		public Color TextAccentError { get; set; }
		public Color TextBase { get; set; }
		public Color TextBaseDisabled { get; set; }
		public Color TextBaseSecondary { get; set; }
		public Color TextContrastBase { get; set; }
		public Color TextContrastSecondary { get; set; }
		public Color TextContrastDisabled { get; set; }

		public Color ColorBrand { get; set; }
		public Color ColorPrimary { get; set; }
		public Color ColorSecondary { get; set; }
		public Color ColorAccentSuccess { get; set; }
		public Color ColorAccentWarning { get; set; }
		public Color ColorAccentError { get; set; }
		public Color ColorShadow { get; set; }
		public Color ColorWhite { get; set; }
		public Color ColorGray { get; set; }

		public Color ChartBase { get; set; }

		public double SoftDim { get; set; }

		public Color HomeTeamColor { get; set; }
		public Color AwayTeamColor { get; set; }

		public Color PaletteBackgroundSemiLight { get; set; } // to be changed in presentations view because is used incorrectly

		//Sizes

		public int IconLargeHeight { get; set; }

		public int IconLargeWidth { get; set; }

		public int IconMediumHeight { get; set; }

		public int IconMediumWidth { get; set; }

		public int IconSmallHeight { get; set; }

		public int IconSmallWidth { get; set; }

		public int IconXSmallHeight { get; set; }

		public int IconXSmallWidth { get; set; }

		public int IconTinyHeight { get; set; }

		public int IconTinyWidth { get; set; }

		public int ButtonTimelineHeight { get; set; }

		public int ButtonTimelineWidth { get; set; }

		public int ButtonNormalHeight { get; set; }

		public int ButtonNormalWidth { get; set; }

		public int ButtonTabHeight { get; set; }

		public int ButtonTabWidth { get; set; }

		public int ButtonRemoveHeight { get; set; }

		public int ButtonRemoveWidth { get; set; }

		public int ButtonFocusHeight { get; set; }

		public int ButtonFocusWidth { get; set; }

		public int ButtonDialogHeight { get; set; }

		public int ButtonLimitWidth { get; set; }

		public int ButtonLimitHeight { get; set; }

		public int ButtonHeaderBoxHeight { get; set; }

		public int ButtonHeaderBoxWidth { get; set; }

		public int ContainerBigPadding { get; set; }

		public int ContainerRegularPadding { get; set; }

		public int ContainerTightPadding { get; set; }

		public int MainToolbarHeight { get; set; }

		public int SubToolbarHeight { get; set; } = 40;

		// Fonts

		public string Font {
			get;
			protected set;
		} = "Ubuntu";

		public string BigScoresFontFamily { get; set; }

		public int BigScoresFontSize { get; set; }

		public string BigScoresFont {
			get {
				return string.Concat (BigScoresFontFamily, " ", BigScoresFontSize, "px");
			}
		}

		public string TightScoresFontFamily { get; set; }

		public int TightScoresFontSize { get; set; }

		public string TightScoresFont {
			get {
				return string.Concat (TightScoresFontFamily, " ", TightScoresFontSize, "px");
			}
		}

		public string TitlesFontFamily { get; set; }

		public string TitlesFontSlant { get; set; }

		public int TitlesFontSize { get; set; }

		public string TitlesFont {
			get {
				return string.Concat (TitlesFontFamily, " ", TitlesFontSlant, " ", TitlesFontSize, "px");
			}
		}

		public string SubTitleFontFamily { get; set; }

		public int SubTitleFontSize { get; set; }

		public string SubTitleFont {
			get {
				return string.Concat (SubTitleFontFamily, " ", SubTitleFontSize, "px");
			}
		}

		public string NamesFontFamily { get; set; }

		public int NamesFontSize { get; set; }

		public string NamesFont {
			get {
				return string.Concat (NamesFontFamily, " ", NamesFontSize, "px");
			}
		}

		public string ContentFontFamily { get; set; }

		public int ContentFontSize { get; set; }

		public string ContentFont {
			get {
				return string.Concat (ContentFontFamily, " ", ContentFontSize, "px");
			}
		}

		public string LabelFontFamily { get; set; }

		public int LabelFontSize { get; set; }

		public string LabelFont {
			get {
				return string.Concat (LabelFontFamily, " ", LabelFontSize, "px");
			}
		}

		public string LabelTightFontFamily { get; set; }

		public string LabelTightFontSlant { get; set; }

		public int LabelTightFontSize { get; set; }

		public string LabelTightFont {
			get {
				return string.Concat (LabelTightFontFamily, " ", LabelTightFontSlant, " ", LabelTightFontSize, "px");
			}
		}

		#endregion

		public static StyleConf Load (string filename)
		{
			return Serializer.Instance.Load<StyleConf> (filename);
		}

	}
}
