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
using VAS.Core.Common;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
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
			replayhbox.HeightRequest = livebox.HeightRequest = StyleConf.PlayerCapturerControlsHeight;
			replayimage.Pixbuf = Misc.LoadIcon ("longomatch-replay", StyleConf.PlayerCapturerIconSize);
			liveimage.Pixbuf = Misc.LoadIcon ("longomatch-live", StyleConf.PlayerCapturerIconSize);
			livelabel.ModifyFg (Gtk.StateType.Normal, Misc.ToGdkColor (App.Current.Style.PaletteActive));
			replaylabel.ModifyFg (Gtk.StateType.Normal, Misc.ToGdkColor (App.Current.Style.PaletteActive));
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

		public virtual ICapturerBin Capturer {
			get {
				return capturerbin;
			}
		}

		public PlayerViewOperationMode Mode {
			set {
				mode = value;
				if (mode == PlayerViewOperationMode.Analysis) {
					ShowPlayer ();
				} else {
					ShowCapturer ();
				}
				ViewModel.Mode = value;
				Log.Debug ("CapturerPlayer setting mode " + value);
			}
		}

		public virtual void ShowPlayer ()
		{
			playerbox.Visible = true;
			replayhbox.Visible = false;
			if (playerVM.Mode == PlayerViewOperationMode.LiveAnalysisReview && App.Current.Config.ReviewPlaysInSameWindow)
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

		public virtual void AttachPlayer (bool attached)
		{
			playerVM.PlayerAttached = attached;
		}

		protected virtual void OnBacktolivebuttonClicked (object sender, System.EventArgs e)
		{
			playerVM.Pause ();
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
			if (e.PropertyName == "PlayElement") {
				if (playerVM.PlayElement == null) {
					if (playerVM.Mode == PlayerViewOperationMode.Analysis) {
						return;
					}
					livebox.Visible = replayhbox.Visible = false;
					playerVM.Pause ();
					ShowCapturer ();
				} else {
					if (playerVM.PlayElement is TimelineEvent && playerVM.Mode == PlayerViewOperationMode.LiveAnalysisReview) {
						ShowPlayer ();
						livebox.Visible = replayhbox.Visible = true;
					}
				}
			} else if (e.PropertyName == "Mode") {
				if (playerVM.Mode == PlayerViewOperationMode.Analysis) {
					ShowPlayer ();
				} else {
					ShowCapturer ();
				}
				Log.Debug ("CapturerPlayer setting mode " + playerVM.Mode);
			} else if (e.PropertyName == "PrepareView") {
				ShowPlayer ();
			}
		}
	}
}

