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
namespace VAS.Core.Resources.Styles
{
	public class Sizes
	{
		public const int WelcomeBorder = 30;
		public const int WelcomeIconSize = 80;
		public const int WelcomeIconImageSize = 36;
		public const int WelcomeLogoWidth = 450;
		public const int WelcomeLogoHeight = 99;
		public const int WelcomeIconsHSpacing = 105;
		public const int WelcomeIconsVSpacing = 55;
		public const int WelcomeIconsTextSpacing = 5;
		public const int WelcomeIconsTextHeight = 20;
		public const int WelcomeIconsTotalRows = 2;
		public const int WelcomeIconsFirstRow = 3;
		public const int WelcomeTextHeight = 20;
		public const int WelcomeMinWidthBorder = 30;
		public const int WelcomeBarIconSize = 46;
		public const int WelcomeBarButtonSize = 60;

		public const int ProjectTypeIconSize = 80;

		public const int HeaderFontSize = 20;
		public const int HeaderHeight = 60;

		public const int TemplatesHeaderIconSize = 54;
		public const int TemplatesIconSize = 36;

		public const int PreferencesIconSize = 48;

		public const int NewHeaderSpacing = 10;
		public const int NewEntryWidth = 150;
		public const int NewEntryHeight = 30;
		public const int NewTableHSpacing = 5;
		public const int NewTableVSpacing = 5;
		public const int NewTeamsComboWidth = 245;
		public const int NewTeamsComboHeight = 60;
		public const int NewTeamsIconSize = 55;
		public const int NewTeamsFontSize = 16;
		public const int NewTeamsSpacing = 60;
		public const int NewTaggerSpacing = 35;

		public const int ListTextFontSize = 14;
		public const int ListSelectedWidth = 16;
		public const int ListRowSeparator = 10;
		public const int ListTextWidth = 180;
		public const int ListImageWidth = 50;
		public const int ListCategoryHeight = 50;
		public const int ListCountRadio = 10;
		public const int ListCountWidth = 20;
		public const int ListTextOffset = ListRowSeparator * 3 + ListCountRadio * 2 + ListCountWidth;
		public const int ListEyeIconOffset = 10;
		public const int ListEyeIconWidth = 36;
		public const int ListEyeIconHeight = 36;
		public const int ListArrowRightWidth = 12;
		public const int ListArrowRightHeight = 12;

		public const int TeamsShieldIconSize = 45;
		public const int TimelineNeedleBigWidth = 11;
		public const int TimelineNeedleBigHeight = 20;
		public const int TimelineNeedleUpWidth = 11;
		public const int TimelineNeedleUpHeight = 14;
		public const int TimelineCategoryHeight = 20;
		public const int TimelineCameraHeight = 30;
		public const int TimelineCameraMaxLines = 8;
		public const int TimelineCameraFontSize = 14;
		public const int TimelineCameraObjectLineWidth = 1;
		public const int TimelineLabelsWidth = 200;
		public const int TimelineLabelHSpacing = 10;
		public const int TimelineLabelVSpacing = 2;
		public const int TimelineLineSize = 6;
		public const int TimelineFontSize = 16;
		public const int TimelineRuleFontSize = 12;
		public const int TimelineRulePlayerFontSize = 7;
		public const int TimelineBackgroundLineSize = 4;
		public const int TimelinePadding = 30;
		public const int TimelineSelectionLeftWidth = 10;
		public const int TimelineSelectionLeftHeight = 20;
		public const int TimelineSelectionRightWidth = 10;
		public const int TimelineSelectionRightHeight = 20;

		public const int PlayerLineWidth = 2;
		public const int PlayerSize = 60;
		public const int PlayerNumberSize = 20;
		public const int PlayerArrowSize = PlayerNumberSize;
		public const int PlayerNumberX = 0;
		public const int PlayerNumberY = 60 - PlayerLineWidth - PlayerNumberSize + 1;
		public const int PlayerArrowX = PlayerNumberX;
		public const int PlayerArrowY = PlayerNumberY - PlayerArrowSize + 1;

		public const int NotebookTabIconSize = 18;
		public const int NotebookTabSize = NotebookTabIconSize + 14;

		public const int TopBarHeight = 44;
		public const int TopBarButtonIconSize = 28;
		public const int TopBarToggleButtonIconSize = 16;

		public const int PresentationManagerHeaderIconSize = 54;
		public const int PresentationManagerButtonIconSize = 34;

		public const int ButtonNormalSize = 24;
		public const int ButtonHeaderSize = 20;
		public const int ButtonHeaderHeight = 20;
		public const int ButtonHeaderWidth = 20;
		public const int ButtonRecWidth = 40;
		public const int ButtonLineWidth = 1;
		public const int ButtonActiveLineWidth = 2;
		public const int ButtonHeaderFontSize = 14;
		public const int ButtonNameFontSize = 18;
		public const int ButtonTimerFontSize = 24;
		public const int ButtonButtonsFontSize = 10;
		public const int ButtonMinWidth = 100;
		public const int ButtonDialogIconSize = 24;

		public const int DrawingSelectorAnchorSize = 6;
		public const int DrawingSelectorLineWidth = 1;

		public const int BenchLineWidth = 2;
		public const int TeamTaggerBenchBorder = 10;

		public const int FrameCornerRadius = 4;
		public const int FrameCornerPadding = 0;
		public const int FrameCornerMargin = 2;

		public const int LinkInHeight = 14;
		public const int LinkInWidth = 14;
		public const int LinkOutHeight = 14;
		public const int LinkOutWidth = 14;

		public const int FilterTreeViewToogleWidth = 30;
		public const int FilterTreeViewOnlyRightOffset = 10;
		public const int FilterTreeViewOnlyTopOffset = 2;

		//Watermark
		public const int WatermarkPadding = 20;
		public const double WatermarkHeightNormalization = 0.087;

		// Video Player
		public static int PlayerCapturerSmallIconSize = 15;
		public static int PlayerCapturerIconSize = 20;
		public static int PlayerCapturerControlsHeight = 30;

		// profile stats
		public static int ProfileSeasonChartLineWidth = 18;
		public static int ProfileSeasonChartRadius = 65;
		public static int ProfileLaneChartLineWidth = 14;
		public static int ProfileLaneChartRadius = 50;

		//license banner
		public static int LicenseBannerUpgradeButtonWidth = 170;
		public static int LicenseBannerUpgradeButtonHeight = 60;
		public static int LicenseTextFontSize = 16;
	}
}
