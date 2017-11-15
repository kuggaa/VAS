//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using System.Threading.Tasks;
using VAS.Core.Common;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services;
using VAS.Services.State;

namespace VideoPlayer.State
{
	public class VideoPlayerState : ScreenState<VideoPlayer.ViewModel.VideoPlayerVM>
	{
		public const string NAME = "Video player";
		public override string Name => NAME;

		protected override void CreateViewModel (dynamic data)
		{
			MediaFileVM file = data as MediaFileVM;
			if (file == null) {
				var mediaFile = App.Current.MultimediaToolkit.DiscoverFile ("/Users/vmartos/Documents/Sample_Videos/Madrid_2_Barcelona_6.mp4");
				file = new MediaFileVM {
					Model = mediaFile
				};
			}
			ViewModel = new VideoPlayer.ViewModel.VideoPlayerVM ();
			ViewModel.File = new MediaFileSetVM { Model = new MediaFileSet () };
			ViewModel.File.ViewModels.Add (file);
			ViewModel.VideoPlayer = new VAS.Core.ViewModel.VideoPlayerVM ();
			ViewModel.VideoPlayer.ViewMode = PlayerViewOperationMode.Analysis;
			ViewModel.VideoPlayer.ShowDetachButton = false;
			ViewModel.VideoPlayer.ShowCenterPlayHeadButton = false;
		}

		protected override void CreateControllers (dynamic data)
		{
			Controllers.Add (new VideoPlayerController ());
		}

		public async override Task<bool> LoadState (dynamic data)
		{
			await Initialize (data);
			ViewModel.VideoPlayer.OpenFileSet (ViewModel.File);
			return true;
		}
	}
}
