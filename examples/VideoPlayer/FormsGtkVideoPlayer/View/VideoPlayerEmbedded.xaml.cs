//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VideoPlayer.State;
using VideoPlayer.ViewModel;
using Xamarin.Forms;

namespace FormsGtkVideoPlayer.View
{
	[View (VideoPlayerState.NAME)]
	public partial class VideoPlayerEmbedded : ContentPage, IPanel<VideoPlayerVM>
	{
		public VideoPlayerEmbedded ()
		{
			InitializeComponent ();
			player.PropertyChanged += (sender, e) => 
			{
				if (e.PropertyName == nameof(player.NativeWidget)) {
					if (viewModel != null) {
						(player.NativeWidget as IView)?.SetViewModel (viewModel.VideoPlayer);
					}
				}
			};
		}
		VideoPlayerVM viewModel;

		public VideoPlayerVM ViewModel {
			get {
				return viewModel;
			}

			set {
				viewModel = value;
				BindingContext = viewModel;
				if (viewModel != null) {
					(player.NativeWidget as IView)?.SetViewModel (viewModel.VideoPlayer);
				}
			}
		}

		public void Dispose ()
		{
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
