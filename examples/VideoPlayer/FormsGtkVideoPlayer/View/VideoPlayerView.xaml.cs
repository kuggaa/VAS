//
//  Copyright (C) 2017 ${CopyrightHolder}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VideoPlayer.State;
using VideoPlayer.ViewModel;
using Xamarin.Forms;

namespace FormsGtkVideoPlayer.View
{
	[View(VideoPlayerState.NAME)]
	public partial class VideoPlayerView : ContentPage, IPanel<VideoPlayerVM>
	{
		public VideoPlayerView()
		{
			InitializeComponent();
			viewPortView.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == "ViewPort")
				{
					viewPortView.ViewPort.ReadyEvent += ViewPort_ReadyEvent;
				}
			};

		}
		VideoPlayerVM viewModel;

		public VideoPlayerVM ViewModel
		{
			get
			{
				return viewModel;
			}

			set
			{
				viewModel = value;
				if (value != null)
				{
					BindingContext = value;
					VideoControlBar.BindingContext = value.VideoPlayer;
				}
			}
		}

		void ViewPort_ReadyEvent(object sender, EventArgs e)
		{
			ViewModel.VideoPlayer.SetCamerasConfig(new ObservableCollection<CameraConfig>(){
				new CameraConfig(0)
			});
			ViewModel.VideoPlayer.ViewPorts = new List<IViewPort>() { viewPortView.ViewPort };
			ViewModel.VideoPlayer.ReadyCommand.Execute(true);
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
