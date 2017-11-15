//
//  Copyright (C) 2017 ${CopyrightHolder}
using System;
using FormsGtkVideoPlayer.Component;
using FormsGtkVideoPlayer.Renderers;
using VAS.UI;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;

[assembly: ExportRenderer(typeof(ViewPortView), typeof(WindowVideoRenderer))]
namespace FormsGtkVideoPlayer.Renderers
{
	public class WindowVideoRenderer : ViewRenderer<ViewPortView, VideoWindow>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<ViewPortView> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					Control = new VideoWindow();
					e.NewElement.ViewPort = Control;
					SetNativeControl(Control);
				}
			}

			base.OnElementChanged(e);
		}
	}
}
