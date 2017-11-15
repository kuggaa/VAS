//
//  Copyright (C) 2017 ${CopyrightHolder}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using Xamarin.Forms;

namespace FormsGtkVideoPlayer.View
{
	public partial class VideoControlBar : ContentView
	{
		public VideoControlBar()
		{
			InitializeComponent();
		}

		void Handle_Clicked(object sender, System.EventArgs e)
		{
			((VideoPlayerVM)BindingContext).SetCamerasConfig(new ObservableCollection<CameraConfig>(){
				new CameraConfig(0)
			});
			((VideoPlayerVM)BindingContext).ReadyCommand.Execute(true);
		}
	}
}
