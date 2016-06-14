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
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using VAS.UI.Helpers;
using VAS.Core;

namespace VAS.UI
{
	[System.ComponentModel.Category ("VAS")]
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PlayerCapturerBin : Gtk.Bin
	{
		protected IPlayerView playerview;
		protected PlayerViewOperationMode mode;

		public PlayerCapturerBin ()
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
			Player = playerview.Player;

			App.Current.EventsAggregator.Subscribe<LoadVideoEvent> (HandleLoadVideoEvent, ThreadMethod.UIThread);
			App.Current.EventsAggregator.Subscribe<CloseVideoEvent> (HandleCloseVideoEvent, ThreadMethod.UIThread);
		}

		protected override void OnDestroyed ()
		{
			(playerview as Gtk.Widget).Destroy ();
			capturerbin.Destroy ();
			base.OnDestroyed ();
		}

		public virtual IPlayerController Player {
			private set {
				Player.ElementLoadedEvent += HandleElementLoadedEvent;
				Player.PrepareViewEvent += HandlePrepareViewEvent;
			}

			get {
				return playerview.Player;
			}
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
				playerview.Mode = value;
				Log.Debug ("CapturerPlayer setting mode " + value);
			}
		}

		public virtual void ShowPlayer ()
		{
			playerbox.Visible = true;
			replayhbox.Visible = false;
			if (mode == PlayerViewOperationMode.LiveAnalysisReview && App.Current.Config.ReviewPlaysInSameWindow)
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
			playerview.PlayerAttached = attached;
		}

		protected virtual void HandleLoadVideoEvent (LoadVideoEvent loadVideoEvent)
		{
			App.Current.EventsAggregator.Publish<ChangeVideoMessageEvent> (
				new ChangeVideoMessageEvent () {
					message = null
				});

			ShowDetachButtonInPlayer (false);
			Player.IgnoreTicks = false;
			Player.Open (loadVideoEvent.mfs);
			Player.Play ();
		}

		protected virtual void HandleCloseVideoEvent (CloseVideoEvent closeVideoEvent)
		{
			if (Player is VAS.Services.PlayerController) {
				(Player as VAS.Services.PlayerController).ResetCounter ();

				App.Current.EventsAggregator.Publish<ChangeVideoMessageEvent> (
					new ChangeVideoMessageEvent () {
						message = Catalog.GetString ("No video loaded")
					});
			}
		}

		protected virtual void HandlePrepareViewEvent ()
		{
			ShowPlayer ();
		}

		protected virtual void HandleElementLoadedEvent (object element, bool hasNext)
		{
			if (element == null) {
				if (mode == PlayerViewOperationMode.Analysis) {
					return;
				}
				livebox.Visible = replayhbox.Visible = false;
				Player.Pause ();
				ShowCapturer ();
			} else {
				if (element is TimelineEvent && mode == PlayerViewOperationMode.LiveAnalysisReview) {
					ShowPlayer ();
					livebox.Visible = replayhbox.Visible = true;
				}
			}
		}

		protected virtual void OnBacktolivebuttonClicked (object sender, System.EventArgs e)
		{
			Player.Pause ();
			ShowCapturer ();
		}

		public void ShowDetachButtonInPlayer (bool show)
		{
			playerview.ShowDetachButton (show);
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
	}
}

