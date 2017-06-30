//
//  Copyright (C) 2007-2010 Andoni Morales Alastruey
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
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//

namespace VAS.Core.Common
{
	public class Constants
	{
		public const string FAKE_PROJECT = "@Fake Project@";
		public const string EMPTY_OR_NULL = "$%&EMPTY$%&NULL$$%&";

#if OSTYPE_ANDROID || OSTYPE_IOS
				public const string IMAGE_EXT = ".png";



#else
		public const string IMAGE_EXT = ".svg";
#endif

		// FIXME: Fields, goals, etc are from LongoMatch, we need a more flexible structure.
		public const string FIELD_BACKGROUND = "images/fields/lm-field-full" + IMAGE_EXT;
		public const string HALF_FIELD_BACKGROUND = "images/fields/lm-field-half" + IMAGE_EXT;
		public const string HHALF_FIELD_BACKGROUND = "images/fields/lm-field-full-teameditor" + IMAGE_EXT;
		public const string GOAL_BACKGROUND = "images/fields/lm-field-goal" + IMAGE_EXT;

		public const int DB_VERSION = 1;

		// FIXME: These are style constants, they should be somewhere style-specific
		public const int BUTTON_WIDTH = 120;
		public const int BUTTON_HEIGHT = 80;

		public const int MAX_THUMBNAIL_SIZE = 200;
		public const int MAX_PLAYER_ICON_SIZE = 200;
		public const int MAX_SHIELD_ICON_SIZE = 200;
		public const int MAX_BACKGROUND_WIDTH = 800;
		public const int MAX_BACKGROUND_HEIGHT = 800;

		public const string LINE_NORMAL = "images/tools/vas-line" + IMAGE_EXT;
		public const string LINE_DASHED = "images/tools/vas-dash-line" + IMAGE_EXT;
		public const string LINE_ARROW = "images/tools/vas-line-arrow" + IMAGE_EXT;
		public const string LINE_DOUBLE_ARROW = "images/tools/vas-line-double-arrow" + IMAGE_EXT;
		public const string LINE_DOT = "images/tools/vas-line-dot" + IMAGE_EXT;
		public const string LINE_DOUBLE_DOT = "images/tools/vas-line-double-dot" + IMAGE_EXT;

		public const string TimelineEventsDND = "timeline-events-dnd";
		public const string PlaylistElementsDND = "playlist-elements-dnd";

		public const int PLAYBACK_TOLERANCE = 100;
		public const int TEMP_TAGGING_DURATION = 500;

		public const string WATERMARK_RESOURCE_ID = "logo-watermark.svg";
	}
}
