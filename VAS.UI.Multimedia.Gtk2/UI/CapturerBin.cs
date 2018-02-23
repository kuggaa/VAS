// CapturerBin.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Gtk;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Resources.Styles;
using VAS.Core.ViewModel;
using VAS.Drawing.Cairo;
using VAS.UI.Helpers.Bindings;
using Action = System.Action;
using Misc = VAS.UI.Helpers.Misc;
using TextView = VAS.Drawing.Widgets.TextView;

namespace VAS.UI
{
	[System.ComponentModel.Category ("VAS")]
	[System.ComponentModel.ToolboxItem (true)]

	public partial class CapturerBin : Gtk.Bin, IView<VideoRecorderVM>
	{
		CapturerType type;
		TextView hourText;
		TextView minutesText;
		TextView secondsText;
		VideoRecorderVM viewModel;

		Action delayedRun;
		bool capturerBinReady = false;

		public CapturerBin ()
		{
			this.Build ();
			Misc.SetFocus (vbox1, false);
			videowindow.ReadyEvent += HandleReady;
			videowindow.ExposeEvent += HandleExposeEvent;
			videowindow.CanFocus = true;

			hourText = new TextView (new WidgetWrapper (hourArea)) { Text = "00" };
			minutesText = new TextView (new WidgetWrapper (minutesArea)) { Text = "00" };
			secondsText = new TextView (new WidgetWrapper (secondsArea)) { Text = "00" };
		}

		public VideoRecorderVM ViewModel {
			get {
				return viewModel;
			}

			set {
				if (viewModel != null) {
					viewModel.PropertyChanged -= HandlePropertyChanged;
				}
				viewModel = value;
				this.GetBindingContext ().UpdateViewModel (viewModel);
				if (viewModel != null) {
					viewModel.PropertyChanged += HandlePropertyChanged;
					viewModel?.Sync ();
				}
			}
		}

		CapturerType Mode {
			set {
				type = value;
				videowindow.Visible = value == CapturerType.Live;
				if (type == CapturerType.Fake) {
					SetStyle (Sizes.PlayerCapturerControlsHeight * 2, 24 * 2, 40 * 2);
					playlastbutton.Visible = false;
					controllerbox.SetChildPacking (vseparator1, false, false, 20, PackType.Start);
					controllerbox.SetChildPacking (vseparator2, false, false, 20, PackType.Start);
				} else {
					playlastbutton.Visible = true;
					SetStyle (Sizes.PlayerCapturerControlsHeight, 24, 40);
					controllerbox.SetChildPacking (vseparator1, true, true, 0, PackType.Start);
					controllerbox.SetChildPacking (vseparator2, true, true, 0, PackType.Start);
				}
			}
		}

		void SetStyle (int height, int fontSize, int timeWidth)
		{
			string font = String.Format ("{0} {1}px", App.Current.Style.Font, fontSize);
			Pango.FontDescription desc = Pango.FontDescription.FromString (font);

			lastlabel.ModifyFont (Pango.FontDescription.FromString (App.Current.Style.Font + " 8px"));

			controllerbox.HeightRequest = height;
			hourseventbox.ModifyBg (StateType.Normal, Misc.ToGdkColor (App.Current.Style.ThemeBase));

			hourText.FontSize = fontSize;
			hourText.TextColor = App.Current.Style.TextBase;
			hourText.FontSlant = FontSlant.Normal;
			minutesText.FontSize = fontSize;
			minutesText.TextColor = App.Current.Style.TextBase;
			minutesText.FontSlant = FontSlant.Normal;
			secondsText.FontSize = fontSize;
			secondsText.TextColor = App.Current.Style.TextBase;
			secondsText.FontSlant = FontSlant.Normal;

			hourseventbox.WidthRequest = timeWidth;
			minuteseventbox.ModifyBg (StateType.Normal, Misc.ToGdkColor (App.Current.Style.ThemeBase));
			minuteseventbox.WidthRequest = timeWidth;
			secondseventbox.ModifyBg (StateType.Normal, Misc.ToGdkColor (App.Current.Style.ThemeBase));
			secondseventbox.WidthRequest = timeWidth;
			label1.ModifyFont (desc);
			label1.ModifyFg (StateType.Normal, Misc.ToGdkColor (App.Current.Style.TextBase));
			label2.ModifyFont (desc);
			label2.ModifyFg (StateType.Normal, Misc.ToGdkColor (App.Current.Style.TextBase));
			periodlabel.ModifyFont (desc);
			periodlabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (App.Current.Style.TextBase));
		}

		void Bind ()
		{
			var ctx = this.GetBindingContext ();
			ctx.Add (savebutton.Bind (vm => ((VideoRecorderVM)vm).SaveCommand));
			ctx.Add (cancelbutton.Bind (vm => ((VideoRecorderVM)vm).CancelCommand));
			ctx.Add (recbutton.Bind (vm => ((VideoRecorderVM)vm).StartRecordingCommand));
			ctx.Add (stopbutton.Bind (vm => ((VideoRecorderVM)vm).StopRecordingCommand));
			ctx.Add (pausebutton.Bind (vm => ((VideoRecorderVM)vm).PauseClockCommand));
			ctx.Add (resumebutton.Bind (vm => ((VideoRecorderVM)vm).ResumeClockCommand));
			ctx.Add (periodlabel.Bind (vm => ((VideoRecorderVM)vm).PeriodName));
			ctx.Add (videowindow.Bind (vw => vw.MessageVisible, vm => ((VideoRecorderVM)vm).RecorderIsReady));
			ctx.Add (videowindow.Bind (vw => vw.Ratio, vm => ((VideoRecorderVM)vm).SourcePAR));
			ctx.Add (this.Bind (cb => cb.Mode, vm => ((VideoRecorderVM)vm).RecorderMode));
		}

		void HandleExposeEvent (object o, ExposeEventArgs args)
		{
			ViewModel.ExposeCommand.Execute ();
		}

		void HandleReady (object sender, EventArgs e)
		{
			ViewModel.ViewReadyCommand.Execute ();
		}

		void HandleMediaInfo (int width, int height, int parN, int parD)
		{
			Application.Invoke (delegate {
				videowindow.Ratio = (float)width / height * parN / parD;
			});
		}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs args)
		{

			if (ViewModel.NeedsSync (args, nameof (ViewModel.LastCreatedEvent))) {
				lasteventbox.Visible = ViewModel.LastCreatedEvent != null;
				if (ViewModel.LastCreatedEvent != null) {
					lastlabel.Text = ViewModel.LastCreatedEvent.Name;
					lastlabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (ViewModel.LastCreatedEvent.Color));
				}
			}
			if (ViewModel.NeedsSync (args, nameof (ViewModel.RecorderMode))) {
				Mode = ViewModel.RecorderMode;
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (viewModel as IVideoRecorderDealer).VideoRecorder;
		}
	}
}
