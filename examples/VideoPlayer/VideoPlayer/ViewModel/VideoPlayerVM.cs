//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;

namespace VideoPlayer.ViewModel
{
	public class VideoPlayerVM : ViewModelBase, IVideoPlayerDealer
	{
		public VAS.Core.ViewModel.VideoPlayerVM VideoPlayer { get; set; }

		public MediaFileSetVM File { get; set; }
	}
}