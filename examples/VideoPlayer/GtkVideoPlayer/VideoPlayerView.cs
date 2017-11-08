//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VideoPlayer.State;
using VideoPlayer.ViewModel;

namespace VideoPlayer
{
	[System.ComponentModel.ToolboxItem (true)]
	[View (VideoPlayerState.NAME)]
	public partial class VideoPlayerView : Gtk.Bin, IPanel<VideoPlayerVM>
	{
		VideoPlayerVM viewModel;

		public VideoPlayerView ()
		{
			this.Build ();
		}

		public string Title => "Video player";

		public VideoPlayerVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
				videoplayerview1.ViewModel = value.VideoPlayer;
			}
		}

		public KeyContext GetKeyContext ()
		{
			return new KeyContext ();
		}

		public void OnLoad ()
		{
		}

		public void OnUnload ()
		{
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (VideoPlayerVM)viewModel;
		}
	}
}
