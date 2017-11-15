//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using FormsGtkVideoPlayer.Components;
using FormsGtkVideoPlayer.Renderers;
using VideoPlayer.ViewModel;
using VAS.UI;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;

[assembly: ExportRenderer (typeof (FormsVideoPlayerView), typeof (VideoPlayerViewRenderer))]
namespace FormsGtkVideoPlayer.Renderers
{
	public class VideoPlayerViewRenderer : ViewRenderer<FormsVideoPlayerView, VideoPlayerView>
	{
		protected override void OnElementChanged (ElementChangedEventArgs<FormsVideoPlayerView> e)
		{
			if (e.NewElement != null) {
				if (Control == null) {
					var view = new VideoPlayerView ();
					view.ViewModel = ((VideoPlayerVM)e.NewElement.BindingContext).VideoPlayer;
					SetNativeControl (view);
				}
			}

			base.OnElementChanged (e);
		}
	}
}
