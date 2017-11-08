//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using VAS.Core.Events;
using VAS.Core.ViewModel;

namespace VideoPlayer
{
	public class OpenFileEvent : Event
	{
		public MediaFileVM File { get; set; }
	}
}
