using System;
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

	public class ChangeVideoMessageEvent : Event
	{
		public string message { get; set; }
	}
}
