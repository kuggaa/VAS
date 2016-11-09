using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Store;

namespace VAS.Core.Events
{
	public class LoadVideoEvent : Event
	{
		public MediaFileSet mfs { get; set; }
	}

	public class CloseVideoEvent : Event
	{
		public MediaFileSet mfs { get; set; }
	}

	/// <summary>
	/// Event that indicates that video has been stretched.
	/// </summary>
	public class StretchVideoEvent : Event
	{
		/// <summary>
		/// Gets or sets the MediaFileSet.
		/// </summary>
		/// <value>The mfs.</value>
		public MediaFileSet mfs { get; set; }
	}

	/// <summary>
	/// Event that indicates that video size has been changed.
	/// </summary>
	public class ChangeVideoSizeEvent : Event
	{
		/// <summary>
		/// Gets or sets the time.
		/// </summary>
		/// <value>The time.</value>
		public Time Time { get; set; }

		/// <summary>
		/// Gets or sets the type of the seek.
		/// </summary>
		/// <value>The type of the seek.</value>
		public SeekType SeekType { get; set; }

		/// <summary>
		/// Gets or sets the player.
		/// </summary>
		/// <value>The player.</value>
		public IPlayerController player { get; set; }
	}

	/// <summary>
	/// Events that indicates that video Timeline mode has changed
	/// </summary>
	public class VideoTimelineModeChangedEvent : Event
	{
		/// <summary>
		/// Gets or sets the video timeline mode.
		/// </summary>
		/// <value>The video tl mode.</value>
		public VideoTimelineMode videoTlMode { get; set; }
	}

	public class ChangeVideoMessageEvent : Event
	{
		public string message { get; set; }
	}

	public class LoadTimelineEvent<T> : Event
	{
		public T Object { get; set; }

		public bool Playing { get; set; }
	}
}
