//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using System.Threading.Tasks;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VideoPlayer.State;
using VideoPlayer.ViewModel;

namespace VideoPlayer.Controller
{
	[Controller (FileSelectionState.NAME)]
	public class FileSelectionController : ControllerBase<FileSelectionVM>
	{
		public override Task Start ()
		{
			App.Current.EventsBroker.SubscribeAsync<OpenFileEvent> (HandleOpenFileEvent);
			return base.Start ();
		}

		public override Task Stop ()
		{
			App.Current.EventsBroker.UnsubscribeAsync<OpenFileEvent> (HandleOpenFileEvent);
			return base.Stop ();
		}

		public override void SetViewModel (IViewModel viewModel)
		{
			ViewModel = (FileSelectionVM)viewModel;
		}

		public async Task HandleOpenFileEvent (OpenFileEvent evt)
		{
			if (evt.File == null) {
				var mediaFile = App.Current.Dialogs.OpenMediaFile ();
				if (mediaFile == null) {
					return;
				}
				ViewModel.File.Model = mediaFile;
			}
			await App.Current.StateController.MoveTo (VideoPlayerState.NAME, ViewModel.File, true);
		}
	}
}
