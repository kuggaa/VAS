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
using Gtk;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Store;
using VAS.UI.Helpers;
using Image = VAS.Core.Common.Image;
using Misc = VAS.UI.Helpers.Misc;

namespace VAS.UI
{
	[System.ComponentModel.Category ("VAS")]
	[System.ComponentModel.ToolboxItem (true)]

	public partial class CapturerBin : Gtk.Bin, ICapturerBin
	{
		protected CapturerType type;
		protected CaptureSettings settings;
		protected bool delayStart;
		protected Period currentPeriod;
		protected uint timeoutID;
		protected TimeNode currentTimeNode;
		protected Time accumTime;
		protected DateTime currentPeriodStart;
		protected List<string> gamePeriods;
		protected TimelineEvent lastevent;
		protected MediaFile outputFile;
		protected bool readyToCapture;

		public CapturerBin ()
		{
			this.Build ();
			Misc.SetFocus (vbox1, false);
			videowindow.ReadyEvent += HandleReady;
			videowindow.ExposeEvent += HandleExposeEvent;
			videowindow.CanFocus = true;
			recbutton.Clicked += (sender, e) => StartPeriod ();
			stopbutton.Clicked += (sender, e) => StopPeriod ();
			pausebutton.Clicked += (sender, e) => PausePeriod ();
			resumebutton.Clicked += (sender, e) => ResumePeriod ();
			savebutton.Clicked += HandleSaveClicked;
			cancelbutton.Clicked += HandleCloseClicked;

			// Force tooltips to be translatable as there seems to be a bug in stetic 
			// code generation for translatable tooltips.
			recbutton.TooltipMarkup = Catalog.GetString ("Start recording period");
			stopbutton.TooltipMarkup = Catalog.GetString ("Stop recording period");
			pausebutton.TooltipMarkup = Catalog.GetString ("Pause clock");
			resumebutton.TooltipMarkup = Catalog.GetString ("Resume clock");
			savebutton.TooltipMarkup = Catalog.GetString ("Save project");
			cancelbutton.TooltipMarkup = Catalog.GetString ("Cancel capture");
			deletelastbutton.TooltipMarkup = Catalog.GetString ("Delete event");
			playlastbutton.TooltipMarkup = Catalog.GetString ("Replay event");

			recimage.Pixbuf = Misc.LoadIcon ("longomatch-control-record",
				StyleConf.PlayerCapturerIconSize);
			stopimage.Pixbuf = Misc.LoadIcon ("longomatch-stop",
				StyleConf.PlayerCapturerIconSize);
			pauseimage.Pixbuf = Misc.LoadIcon ("longomatch-pause-clock",
				StyleConf.PlayerCapturerIconSize);
			resumeimage.Pixbuf = Misc.LoadIcon ("longomatch-resume-clock",
				StyleConf.PlayerCapturerIconSize);
			saveimage.Pixbuf = Misc.LoadIcon ("longomatch-save",
				StyleConf.PlayerCapturerIconSize);
			cancelimage.Pixbuf = Misc.LoadIcon ("longomatch-cancel-rec",
				StyleConf.PlayerCapturerIconSize);
			deletelastimage.Pixbuf = Misc.LoadIcon ("longomatch-delete",
				StyleConf.PlayerCapturerIconSize);
			playlastimage.Pixbuf = Misc.LoadIcon ("longomatch-control-play",
				StyleConf.PlayerCapturerIconSize);
			lasteventbox.Visible = false;
			deletelastbutton.Clicked += HandleDeleteLast;
			playlastbutton.Clicked += HandlePlayLast;
			Periods = new List<Period> ();
			Reset ();
			Mode = CapturerType.Live;
			Config.EventsBroker.EventCreatedEvent += HandleEventCreated;
			lastlabel.ModifyFont (Pango.FontDescription.FromString (Config.Style.Font + " 8px"));
			ReadyToCapture = false;
		}

		protected override void OnDestroyed ()
		{
			if (timeoutID != 0) {
				GLib.Source.Remove (timeoutID);
			}
			Config.EventsBroker.EventCreatedEvent -= HandleEventCreated;
			base.OnDestroyed ();
		}

		public virtual CapturerType Mode {
			set {
				type = value;
				videowindow.Visible = value == CapturerType.Live;
				if (type == CapturerType.Fake) {
					SetStyle (StyleConf.PlayerCapturerControlsHeight * 2, 24 * 2, 40 * 2);
					playlastbutton.Visible = false;
					controllerbox.SetChildPacking (vseparator1, false, false, 20, PackType.Start);
					controllerbox.SetChildPacking (vseparator2, false, false, 20, PackType.Start);
				} else {
					playlastbutton.Visible = true;
					SetStyle (StyleConf.PlayerCapturerControlsHeight, 24, 40);
					controllerbox.SetChildPacking (vseparator1, true, true, 0, PackType.Start);
					controllerbox.SetChildPacking (vseparator2, true, true, 0, PackType.Start);
				}
			}
		}

		public virtual bool Capturing {
			get;
			set;
		}

		public virtual CaptureSettings CaptureSettings {
			get {
				return settings;
			}
		}

		public virtual List<string> PeriodsNames {
			set {
				gamePeriods = value;
				if (gamePeriods != null && gamePeriods.Count > 0) {
					periodlabel.Markup = gamePeriods [0];
				} else {
					periodlabel.Markup = "1";
				}
			}
			get {
				return gamePeriods;
			}
		}

		public virtual ICapturer Capturer {
			get;
			set;
		}

		public List<Period> Periods {
			set;
			get;
		}

		public virtual Time CurrentCaptureTime {
			get {
				int timeDiff;
				
				timeDiff = (int)(DateTime.UtcNow - currentPeriodStart).TotalMilliseconds; 
				return new Time (accumTime.MSeconds + timeDiff);
			}
		}

		protected virtual Time ElapsedTime {
			get {
				if (currentPeriod != null) {
					return currentPeriod.TotalTime;
				} else {
					return new Time (0);
				}
				
			}
		}

		protected virtual bool ReadyToCapture {
			get {
				return readyToCapture;
			}
			set {
				readyToCapture = value;
				recbutton.Sensitive = readyToCapture;
			}
		}

		public virtual void StartPeriod ()
		{
			string periodName;
			if (!ReadyToCapture) {
				return;
			}
			
			if (currentPeriod != null) {
				string msg = Catalog.GetString ("Period recording already started");
				Config.GUIToolkit.WarningMessage (msg, this);
				return;
			}
			recbutton.Visible = false;
			pausebutton.Visible = savebutton.Visible = stopbutton.Visible = true;
			
			if (PeriodsNames != null && PeriodsNames.Count > Periods.Count) {
				periodName = PeriodsNames [Periods.Count];
			} else {
				periodName = (Periods.Count + 1).ToString ();
			}
			currentPeriod = new Period { Name = periodName };
			
			currentTimeNode = currentPeriod.Start (accumTime, periodName);
			currentTimeNode.Stop = currentTimeNode.Start;
			currentPeriodStart = DateTime.UtcNow;
			timeoutID = GLib.Timeout.Add (20, UpdateTime);
			if (Capturer != null) {
				if (Periods.Count == 0) {
					Capturer.Start ();
				} else {
					Capturer.TogglePause ();
				}
			}
			periodlabel.Markup = currentPeriod.Name;
			Capturing = true;
			Periods.Add (currentPeriod);
			Log.Debug ("Start new period start=", currentTimeNode.Start.ToMSecondsString ());
		}

		public virtual void StopPeriod ()
		{
			if (currentPeriod == null) {
				string msg = Catalog.GetString ("Period recording not started");
				Config.GUIToolkit.WarningMessage (msg, this);
				return;
			}
			
			GLib.Source.Remove (timeoutID);
			currentPeriod.Stop (CurrentCaptureTime);
			accumTime = CurrentCaptureTime;
			Log.Debug ("Stop period stop=", accumTime.ToMSecondsString ());
			currentTimeNode = null;
			currentPeriod = null;
			recbutton.Visible = true;
			pausebutton.Visible = resumebutton.Visible = stopbutton.Visible = false;
			if (Capturer != null && Capturing) {
				Capturer.TogglePause ();
			}
			Capturing = false;
		}

		public virtual void PausePeriod ()
		{
			if (currentPeriod == null) {
				string msg = Catalog.GetString ("Period recording not started");
				Config.GUIToolkit.WarningMessage (msg, this);
				return;
			}
			Log.Debug ("Pause period at currentTime=", CurrentCaptureTime.ToMSecondsString ());
			currentPeriod.Stop (CurrentCaptureTime);
			currentTimeNode = null;
			pausebutton.Visible = false;
			resumebutton.Visible = true;
			Capturing = false;
		}

		public virtual void ResumePeriod ()
		{
			if (currentPeriod == null) {
				string msg = Catalog.GetString ("Period recording not started");
				Config.GUIToolkit.WarningMessage (msg, this);
				return;
			}
			Log.Debug ("Resume period at currentTime=", CurrentCaptureTime.ToMSecondsString ());
			currentTimeNode = currentPeriod.Start (CurrentCaptureTime);
			pausebutton.Visible = true;
			resumebutton.Visible = false;
			Capturing = true;
		}

		protected virtual void SetStyle (int height, int fontSize, int timeWidth)
		{
			string font = String.Format ("{0} {1}px", Config.Style.Font, fontSize);
			Pango.FontDescription desc = Pango.FontDescription.FromString (font);

			controllerbox.HeightRequest = height;
			hourseventbox.ModifyBg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteBackgroundDark));
			hourlabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
			hourlabel.ModifyFont (desc);
			hourseventbox.WidthRequest = timeWidth;
			minuteseventbox.ModifyBg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteBackgroundDark));
			minuteslabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
			minuteslabel.ModifyFont (desc);
			minuteseventbox.WidthRequest = timeWidth;
			secondseventbox.ModifyBg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteBackgroundDark));
			secondslabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
			secondslabel.ModifyFont (desc);
			secondseventbox.WidthRequest = timeWidth;
			label1.ModifyFont (desc);
			label1.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
			label2.ModifyFont (desc);
			label2.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
			periodlabel.ModifyFont (desc);
			periodlabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));

		}

		protected virtual bool UpdateTime ()
		{
			if (currentTimeNode != null) {
				currentTimeNode.Stop = CurrentCaptureTime;
			}
			hourlabel.Markup = ElapsedTime.Hours.ToString ("d2");
			minuteslabel.Markup = ElapsedTime.Minutes.ToString ("d2");
			secondslabel.Markup = ElapsedTime.Seconds.ToString ("d2");
			Config.EventsBroker.EmitCapturerTick (CurrentCaptureTime);
			return true;
		}

		protected virtual void HandleSaveClicked (object sender, EventArgs e)
		{
			string msg = Catalog.GetString ("Do you want to finish the current capture?");
			if (MessagesHelpers.QuestionMessage (this, msg)) {
				Config.EventsBroker.EmitCaptureFinished (false, true);
			}
		}

		protected virtual void HandleCloseClicked (object sender, EventArgs e)
		{
			string msg = Catalog.GetString ("Do you want to close and cancel the current capture?");
			if (MessagesHelpers.QuestionMessage (this, msg)) {
				Config.EventsBroker.EmitCaptureFinished (true, false);
			}
		}

		public virtual void Run (CaptureSettings settings, MediaFile outputFile)
		{
			Reset ();
			if (type == CapturerType.Live) {
				ReadyToCapture = false;
				videowindow.Message = Catalog.GetString ("Loading");
				Capturer = Config.MultimediaToolkit.GetCapturer ();
				this.outputFile = outputFile;
				this.settings = settings;
				videowindow.Ratio = (float)outputFile.VideoWidth / outputFile.VideoHeight;
				Capturer.Error += OnError;
				Capturer.MediaInfo += HandleMediaInfo;
				Capturer.DeviceChange += OnDeviceChange;
				Capturer.ReadyToCapture += HandleReadyToCapture;
				Periods = new List<Period> ();
				if (videowindow.Ready) {
					Configure ();
				} else {
					delayStart = true;
				}
			} else {
				ReadyToCapture = true;
			}
		}

		public virtual void Close ()
		{
			if (currentPeriod != null) {
				StopPeriod ();
			}
			/* stopping and closing capturer */
			if (Capturer != null) {
				try {
					Capturer.Close ();
					Capturer.Error -= OnError;
					Capturer.DeviceChange -= OnDeviceChange;
					Capturer.Dispose ();
				} catch (Exception ex) {
					Log.Exception (ex);
				}
			}
			Capturer = null;
		}

		public virtual Image CurrentCaptureFrame {
			get {
				if (Capturer == null)
					return null;

				Image image = Capturer.CurrentFrame;

				if (image.Value == null)
					return null;
				image.ScaleInplace (Constants.MAX_THUMBNAIL_SIZE,
					Constants.MAX_THUMBNAIL_SIZE);
				return image;
			}
		}

		protected virtual void Reset ()
		{
			currentPeriod = null;
			currentTimeNode = null;
			currentPeriodStart = DateTime.UtcNow;
			accumTime = new Time (0);
			Capturing = false;
			Capturer = null;
			recbutton.Visible = true;
			stopbutton.Visible = false;
			pausebutton.Visible = false;
			savebutton.Visible = false;
			cancelbutton.Visible = true;
			resumebutton.Visible = false;
			lasteventbox.Visible = false;
		}

		protected virtual void Configure ()
		{
			VideoMuxerType muxer;

			if (Capturer == null) {
				videowindow.Visible = false;
				return;
			}
			
			/* We need to use Matroska for live replay and remux when the capture is done */
			muxer = settings.EncodingSettings.EncodingProfile.Muxer;
			if (muxer == VideoMuxerType.Avi || muxer == VideoMuxerType.Mp4) {
				settings.EncodingSettings.EncodingProfile.Muxer = VideoMuxerType.Matroska;
			}
			Capturer.Configure (settings, videowindow.WindowHandle); 
			settings.EncodingSettings.EncodingProfile.Muxer = muxer;
			delayStart = false;
			Capturer.Run ();
			videowindow.MessageVisible = true;
		}

		protected virtual void HandleReady (object sender, EventArgs e)
		{
			if (delayStart) {
				Configure ();
			}
		}

		protected virtual void DeviceChanged (int deviceID)
		{
			string msg;
			/* device disconnected, pause capture */
			if (deviceID == -1) {
				PausePeriod ();
				msg = Catalog.GetString ("Device disconnected. " + "The capture will be paused");
				MessagesHelpers.WarningMessage (this, msg);
			} else {
				msg = Catalog.GetString ("Device reconnected. " + "Do you want to restart the capture?");
				if (MessagesHelpers.QuestionMessage (this, msg, null)) {
					ResumePeriod ();
				}
			}
		}

		protected virtual void OnError (object sender, string message)
		{
			Application.Invoke (delegate {
				Config.EventsBroker.EmitCaptureError (sender, message);
			});
		}

		protected virtual void OnDeviceChange (int deviceID)
		{
			Application.Invoke (delegate {
				DeviceChanged (deviceID);
			});
		}

		protected virtual void HandleExposeEvent (object o, ExposeEventArgs args)
		{
			if (Capturer != null) {
				Capturer.Expose ();
			}
		}

		protected virtual void HandleEventCreated (TimelineEvent evt)
		{
			lasteventbox.Visible = true;
			lastlabel.Text = evt.Name;
			lastlabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (evt.Color));
			lastevent = evt;
		}

		protected virtual void HandlePlayLast (object sender, EventArgs e)
		{
			if (lastevent != null) {
				Config.EventsBroker.EmitLoadEvent (lastevent);
			}
		}

		protected virtual void HandleDeleteLast (object sender, EventArgs e)
		{
			if (lastevent != null) {
				Config.EventsBroker.EmitEventsDeleted (new List<TimelineEvent> { lastevent });
				lastevent = null;
				lasteventbox.Visible = false;
			}
			
		}

		protected virtual void HandleMediaInfo (int width, int height, int parN, int parD)
		{
			Application.Invoke (delegate {
				videowindow.Ratio = (float)width / height * parN / parD;
				outputFile.VideoWidth = (uint)width;
				outputFile.VideoHeight = (uint)height;
				outputFile.Par = (float)parN / parD;
			});
		}

		protected virtual void HandleReadyToCapture (object sender)
		{
			videowindow.MessageVisible = false;
			ReadyToCapture = true;
		}



	}
}
