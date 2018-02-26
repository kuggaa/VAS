// 
//  Copyright (C) 2012 Andoni Morales Alastruey
// 
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
// 
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Resources.Styles;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.UI.Helpers;

namespace VAS.UI
{
	[System.ComponentModel.Category ("VAS")]
	[System.ComponentModel.ToolboxItem (true)]
	public partial class VideoPlayerCapturerBin : Gtk.Bin, IView<VideoPlayerVM>
	{
		protected IVideoPlayerView playerview;
		protected VideoPlayerVM playerVM;
		protected PlayerViewOperationMode mode;

		public VideoPlayerCapturerBin ()
		{
			this.Build ();
			replayhbox.HeightRequest = livebox.HeightRequest = Sizes.PlayerCapturerControlsHeight;
			replayimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-replay", Sizes.PlayerCapturerIconSize);
			liveimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-live", Sizes.PlayerCapturerIconSize);
			livelabel.ModifyFg (Gtk.StateType.Normal, Misc.ToGdkColor (App.Current.Style.TextBaseDisabled));
			replaylabel.ModifyFg (Gtk.StateType.Normal, Misc.ToGdkColor (App.Current.Style.TextBaseDisabled));
			livebox.Visible = replayhbox.Visible = true;
			playerview = App.Current.GUIToolkit.GetPlayerView ();
			playerbox.PackEnd (playerview as Gtk.Widget);
			(playerview as Gtk.Widget).ShowAll ();
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (VideoPlayerVM)viewModel;
		}

		public VideoPlayerVM ViewModel {
			get {
				return playerVM;
			}
			set {
				if (playerVM != null) {
					playerVM.PropertyChanged -= HandlePlayerVMPropertyChanged;
				}
				(playerview as IView).SetViewModel (value);
				playerVM = value;
				if (playerVM != null) {
					playerVM.PropertyChanged += HandlePlayerVMPropertyChanged;
				}
			}
		}

		protected override void OnDestroyed ()
		{
			(playerview as Gtk.Widget).Destroy ();
			capturerbin.Destroy ();
			base.OnDestroyed ();
		}

		//public virtual ICapturerBin Capturer {
		//	get {
		//		return capturerbin;
		//	}
		//}

		public PlayerViewOperationMode Mode {
			set {
				mode = value;
				if (mode == PlayerViewOperationMode.Analysis) {
					ShowPlayer ();
				} else {
					ShowCapturer ();
				}
				ViewModel.ViewMode = value;
				Log.Debug ("CapturerPlayer setting mode " + value);
			}
		}

		public virtual void ShowPlayer ()
		{
			playerbox.Visible = true;
			replayhbox.Visible = false;
			if (playerVM.ViewMode == PlayerViewOperationMode.LiveAnalysisReview && App.Current.Config.ReviewPlaysInSameWindow)
				capturerbox.Visible = true;
			else
				capturerbox.Visible = false;
		}

		public virtual void ShowCapturer ()
		{
			playerbox.Visible = false;
			livebox.Visible = false;
			capturerbox.Visible = true;
		}

		public virtual void Detach (bool attached)
		{
			playerVM.PlayerAttached = attached;
		}

		protected virtual void OnBacktolivebuttonClicked (object sender, System.EventArgs e)
		{
			playerVM.PauseCommand.Execute (false);
			ShowCapturer ();
		}

		protected virtual Gtk.HBox Livebox {
			get {
				return livebox;
			}

			set {
				livebox = value;
			}
		}

		protected virtual Gtk.HBox Replayhbox {
			get {
				return replayhbox;
			}

			set {
				replayhbox = value;
			}
		}

		void HandlePlayerVMPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (ViewModel.NeedsSync (e.PropertyName, nameof(ViewModel.LoadedElement))) {
				if (playerVM.LoadedElement == null) {
					if (playerVM.ViewMode == PlayerViewOperationMode.Analysis) {
						return;
					}
					livebox.Visible = replayhbox.Visible = false;
					playerVM.PauseCommand.Execute (false);
					ShowCapturer ();
				} else {
					if (playerVM.LoadedElement is TimelineEvent && playerVM.ViewMode == PlayerViewOperationMode.LiveAnalysisReview) {
						ShowPlayer ();
						livebox.Visible = replayhbox.Visible = true;
					}
				}
			}
			if (ViewModel.NeedsSync (e.PropertyName, nameof(ViewModel.ViewMode))) {
				if (playerVM.ViewMode == PlayerViewOperationMode.Analysis) {
					ShowPlayer ();
				} else {
					ShowCapturer ();
				}
				Log.Debug ("CapturerPlayer setting mode " + playerVM.ViewMode);
			}
			if (ViewModel.NeedsSync(e.PropertyName, nameof (ViewModel.PrepareView))) {
				ShowPlayer ();
			}
		}
	}
}

