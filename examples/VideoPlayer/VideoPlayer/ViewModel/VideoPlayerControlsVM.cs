//
//  Copyright (C) 2017 ${CopyrightHolder}
using System;
using VAS.Core.MVVMC;

namespace VideoPlayer.ViewModel
{
	public class VideoPlayerControlsVM
	{
		public VideoPlayerControlsVM()
		{
		}

		public Command Play { get; set; }

		public Command Next { get; set; }

		public Command Previous { get; set; }
	}
}
