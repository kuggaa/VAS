//
//  Copyright (C) 2017 ${CopyrightHolder}
using System;
using System.Collections.Generic;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VideoPlayer.State;
using VideoPlayer.ViewModel;
using Xamarin.Forms;

namespace FormsGtkVideoPlayer.View
{
	[View(VideoPlayerState.NAME)]
	public partial class VideoPlayerView : ContentPage, IPanel
	{
		public VideoPlayerView()
		{
			InitializeComponent();
			BindingContext = new VideoPlayerVM();
		}

		public VideoPlayerVM ViewModel
		{
			get;
			set;
		}

		public void Dispose()
		{
		}

		public KeyContext GetKeyContext()
		{
			return new KeyContext();
		}

		public void OnLoad()
		{

		}

		public void OnUnload()
		{

		}

		public void SetViewModel(object viewModel)
		{
			ViewModel = viewModel as VideoPlayerVM;
		}
	}
}
