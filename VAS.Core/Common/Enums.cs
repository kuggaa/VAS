//
//  Copyright (C) 2009 Andoni Morales Alastruey
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
using System;

namespace VAS.Core.Common
{
	public enum SerializationType
	{
		Binary,
		Xml,
		Json
	}

	public enum ProjectType
	{
		CaptureProject,
		URICaptureProject,
		FakeCaptureProject,
		FileProject,
		EditProject,
		None,
	}

	public enum CapturerType
	{
		Fake,
		Live,
	}

	public enum EndCaptureResponse
	{
		Return = 234,
		Quit = 235,
		Save = 236
	}

	public enum TagMode
	{
		Predefined,
		Free,
		Edit
	}

	public enum SortMethodType
	{
		SortByName = 0,
		SortByStartTime = 1,
		SortByStopTime = 2,
		SortByDuration = 3
	}

	public enum ProjectSortType
	{
		SortByName = 0,
		SortByDate = 1,
		SortByModificationDate = 2,
		SortBySeason = 3,
		SortByCompetition = 4
	}



	public enum JobState
	{
		None,
		Pending,
		Running,
		Finished,
		Cancelled,
		Error,
	}

	public enum VideoEncoderType
	{
		Mpeg4,
		Xvid,
		Theora,
		H264,
		Mpeg2,
		VP8,
	}

	public enum AudioEncoderType
	{
		Mp3,
		Aac,
		Vorbis,
	}

	public enum VideoMuxerType
	{
		Avi,
		Mp4,
		Matroska,
		Ogg,
		MpegPS,
		WebM,
	}

	public enum DrawTool
	{
		None,
		Pen,
		Line,
		Ellipse,
		Rectangle,
		Angle,
		Cross,
		Eraser,
		Selection,
		RectangleArea,
		CircleArea,
		Player,
		Text,
		Counter,
		Zoom,
		CanMove,
		Move,
	}

	public enum CaptureSourceType
	{
		None,
		DV,
		System,
		URI,
	}

	public enum GameUnitEventType
	{
		Start,
		Stop,
		Cancel
	}

	public enum EditorState
	{
		START = 0,
		FINISHED = 1,
	}

	public enum JobType
	{
		VideoEdition,
		VideoConversion
	}

	public enum VideoAnalysisMode
	{
		PredefinedTagging,
		ManualTagging,
		Timeline,
		GameUnits,
	}

	/// <summary>
	/// Video timeline mode.
	/// </summary>
	public enum VideoTimelineMode
	{
		/// <summary>
		/// Video timeline shows all the length.
		/// Is possible to change duration and borders are green.
		/// </summary>
		Default,
		/// <summary>
		/// Video timeline shows all the length, but some of it will not be when streched. 
		/// Is possible to change duration and borders are red.
		/// </summary>
		Edit,
		/// <summary>
		/// Video timeline shows part of the length.
		/// Is not possible to change duration and borders are white.
		/// </summary>
		Stretched,
		/// <summary>
		/// Video timeline shows part of the length.
		/// User has tried to change Duration and
		/// borders are red to indicate that is not possible.
		/// </summary>
		StretchedAndEditing
	}

	/// <summary>
	/// Node selection mode.
	/// </summary>
	public enum NodeSelectionMode
	{
		/// <summary>
		/// The node is not selectable at all.
		/// </summary>
		None,
		/// <summary>
		/// Only borders of the node can be selected.
		/// </summary>
		Borders,
		/// <summary>
		/// Only the inner segment of the node can be selected.
		/// </summary>
		Segment,
		/// <summary>
		/// Both borders and inner segment can be selected.
		/// </summary>
		All,
	}

	/// <summary>
	/// Node border clipping mode.
	/// </summary>
	public enum NodeClippingMode
	{
		/// <summary>
		/// The node can expand freely.
		/// </summary>
		None,
		/// <summary>
		/// The node can expand freely, without disappearing completely.
		/// </summary>
		NoStrict,
		/// <summary>
		/// The node can expand freely to the right, but not to the left.
		/// </summary>
		LeftStrict,
		/// <summary>
		/// The node can expand freely to the left, but not to the right.
		/// </summary>
		RightStrict,
		/// <summary>
		/// The node cannot expand beyond its limits.
		/// </summary>
		Strict,
		/// <summary>
		/// The node can expand freely, without disappearing completely and cannot expand beyond Event Time
		/// </summary>
		EventTime,
	}

	/// <summary>
	/// Node dragging mode.
	/// </summary>
	public enum NodeDraggingMode
	{
		/// <summary>
		/// The node is not draggable at all.
		/// </summary>
		None,
		/// <summary>
		/// Only borders of the node can be dragged.
		/// </summary>
		Borders,
		/// <summary>
		/// Only the inner segment of the node can be dragged.
		/// </summary>
		Segment,
		/// <summary>
		/// Both borders and inner segment can be dragged.
		/// </summary>
		All,
	}

	public enum SelectionPosition
	{
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight,
		Left,
		Right,
		Top,
		Bottom,
		LineStart,
		LineStop,
		AngleStart,
		AngleStop,
		AngleCenter,
		CircleBorder,
		All,
		None,
		New,
	}

	public enum LineStyle
	{
		Normal,
		Dashed,
		Pointed
	}

	public enum LineType
	{
		Simple,
		Arrow,
		DoubleArrow,
		Dot,
		DoubleDot
	}

	public enum FontSlant
	{
		Italic,
		Normal,
		Oblique,
	}

	public enum FontWeight
	{
		Light,
		Normal,
		Bold
	}

	public enum FontAlignment
	{
		Left,
		Right,
		Center,
	}

	public enum ToolbarAlignemt
	{
		Left,
		Right,
		Center
	}

	public enum ButtonType
	{
		None,
		Left,
		Center,
		Right
	}

	public enum ButtonModifier
	{
		None,
		Shift,
		Control,
		Meta
	}

	public enum ButtonRepetition
	{
		Single,
		Double,
		Triple
	}

	public enum CursorType
	{
		Arrow,
		LeftArrow,
		DoubleArrow,
		Selection,
		Cross,
	}

	public enum MultiSelectionMode
	{
		Single,
		Multiple,
		MultipleWithModifier,
	}

	public enum PlayersIconSize
	{
		Smallest = 20,
		Small = 30,
		Medium = 40,
		Large = 50,
		ExtraLarge = 60
	}

	public enum FieldPositionType
	{
		Field,
		HalfField,
		Goal
	}

	public enum CardShape
	{
		Rectangle,
		Triangle,
		Circle
	}

	public enum FitMode
	{
		Fill,
		Fit,
		Original
	}

	[Flags]
	public enum CellState
	{
		Selected = 1,
		Prelit = 2,
		Insensitive = 4,
		Sorted = 8,
		Focused = 16
	}

	public enum FileChooserMode
	{
		MediaFile,
		File,
		Directory,
	}

	[Obsolete]
	public enum MediaFileAngle
	{
		Angle1,
		Angle2,
		Angle3,
		Angle4,
	}

	public enum SeekType
	{
		Keyframe,
		Accurate,
		StepUp,
		StepDown,
		None
	}

	// FIXME: VAS doesn't have Seasons or Competitions, that should go to LongoMatch
	// FIXME: This should be more extensible: some way to allow the final application to add elements.
	public enum ProjectSortMethod
	{
		Name,
		Date,
		ModificationDate,
		Season,
		Competition
	}

	public enum PlayerViewOperationMode
	{
		Synchronization,
		LiveAnalysisReview,
		Analysis,
		SimpleWithControls,
	}

	public enum DashboardMode
	{
		Code,
		Edit,
	}

	public enum QueryOperator
	{
		None,
		And,
		Or,
	}

	/// <summary>
	/// Defines how images are scalled.
	/// </summary>
	public enum ScaleMode
	{
		/// <summary>
		/// The image is scalled to fill the defined area without keeping DAR.
		/// </summary>
		Fill,
		/// <summary>
		/// The image is scalled keeping DAR to fill the defined area, clipping the image if needed.
		/// </summary>
		AspectFill,
		/// <summary>
		/// The image is scalled keeping DAR to fit in the defined area, adding black borders if needed.
		/// </summary>
		AspectFit,
		/// <summary>
		/// Respect the original image size, centering it in the available space.
		/// </summary>
		Keep,
	}

	public enum OperatingSystemID
	{
		None,
		Linux,
		Windows,
		OSX,
		iOS,
		Android,
	}

	public enum FileChangeType
	{
		/// <summary>
		/// A new file is added.
		/// </summary>
		Created,
		/// <summary>
		/// An existing file is deleted.
		/// </summary>
		Deleted,
	}

	/* The values must be kept in sync with the combobox in
	 * LongoMatch.GUI/Gui/Component/LinkProperties.cs */
	public enum LinkAction
	{
		Toggle = 0,
		Replicate = 1,
	}

	/* The values must be kept in sync with the combobox in
	 * LongoMatch.GUI/Gui/Component/LinkProperties.cs */
	public enum TeamLinkAction
	{
		Clear = 0,
		Keep = 1,
		Invert = 2
	}

	/// <summary>
	/// Specifies the type of instance used to retrieve an element from the <see cref="Registry"/>.
	/// </summary>
	public enum InstanceType
	{
		New,
		Default,
	}

	/// <summary>
	/// Controller status enum.
	/// </summary>
	public enum ControllerStatus
	{
		/// <summary>
		/// Controller is started
		/// </summary>
		Started,
		/// <summary>
		/// Controller is stopped.
		/// </summary>
		Stopped
	}

	/// <summary>
	/// Video player operation mode.
	/// </summary>
	public enum VideoPlayerOperationMode
	{
		/// <summary>
		/// Regular operation mode.
		/// </summary>
		Normal,
		/// <summary>
		/// Limits the playback region the media file visible region.
		/// </summary>
		Stretched,
		/// <summary>
		/// Works with the full length of the current playlist loaded instead of just the length of the event loaded,
		/// emiting ticks relative to the full duration of the loaded playlist.
		/// </summary>
		Presentation
	}

	/// <summary>
	/// The Application License Level
	/// </summary>
	[Flags]
	public enum LicenseLevel
	{
		/// <summary>
		/// No Licenses Found in the system
		/// </summary>
		NONE = 0,
		/// <summary>
		/// Freemium License Found
		/// </summary>
		FREEMIUM = 1,
		/// <summary>
		/// Pro Licenses Based on Subscription level
		/// </summary>
		LEVEL_1 = 3,
		LEVEL_2 = 7,
		LEVEL_3 = 15,
		LEVEL_4 = 31,
		LEVEL_5 = 63,
		LEVEL_6 = 127,
		LEVEL_7 = 255,
		LEVEL_8 = 511,
		LEVEL_9 = 1023,
		LEVEL_10 = 2047,
		LEVEL_11 = 4095,
		LEVEL_12 = 8191,
		LEVEL_13 = 16383,
		LEVEL_14 = 32767,
		LEVEL_15 = 65535
	}

	/// <summary>
	/// Watermark position.
	/// </summary>
	public enum WatermarkPosition
	{
		TOP_LEFT,
		TOP_RIGHT,
		BOTTOM_LEFT,
		BOTTOM_RIGHT
	}
}
