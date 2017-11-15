//
//  Copyright (C) 2017 ${CopyrightHolder}
using System;
using VAS.Core.Interfaces.GUI;
using Xamarin.Forms;

namespace FormsGtkVideoPlayer.Component
{
	public class ViewPortView : Xamarin.Forms.View
	{
		IViewPort viewPort;

		public IViewPort ViewPort
		{
			get
			{
				return viewPort;
			}
			set
			{
				viewPort = value;
				this.OnPropertyChanged(nameof(ViewPort));
			}
		}

		public ViewPortView()
		{
		}
	}
}

