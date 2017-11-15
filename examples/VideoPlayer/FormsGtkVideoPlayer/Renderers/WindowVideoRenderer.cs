//
//  Copyright (C) 2017 ${CopyrightHolder}
using System;
using FormsGtkVideoPlayer.Component;
using FormsGtkVideoPlayer.Renderers;
using VAS.UI;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;

[assembly: ExportRenderer(typeof(FormsVideoPlayerView), typeof(WindowVideoRenderer))]
namespace FormsGtkVideoPlayer.Renderers
{
	public class WindowVideoRenderer : ViewRenderer<FormsVideoPlayerView, VideoWindow>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<FormsVideoPlayerView> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var view = new VideoWindow();
					// view.ViewModel = ((VideoPlayerVM)e.NewElement.BindingContext).VideoPlayer;
					SetNativeControl(view);
				}
			}

			base.OnElementChanged(e);
		}
	}
}
