//
//  Copyright (C) 2015 Fluendo S.A.
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
using System;
using System.Collections.Generic;
using System.Linq;
using Gdk;
using Gtk;
using VAS.Core.Common;
using VAS.Core.Interfaces;

namespace VAS.UI.Dialog
{
	/// <summary>
	/// A splash creen with a progress bar that reports progress.
	/// </summary>
	public class SplashScreen : Gtk.Window, IProgressReport
	{

		Fixed fixed1;
		ProgressBar progressbar1;
		Gtk.Image splashimage;

		Dictionary<Guid, ProgressStatus> statusDict;
		IProgress<ProgressStatus> progress;

		public SplashScreen (VAS.Core.Common.Image image) : base (Gtk.WindowType.Toplevel)
		{

			Build (image.Width, image.Height);

			splashimage.Pixbuf = image.Value;
			Resizable = false;
			Decorated = false;
			SetPosition (Gtk.WindowPosition.CenterAlways);

			// HACK: Center window in OS X
			if (Utils.OS == OperatingSystemID.OSX) {
				Screen screen = Display.Default.DefaultScreen;
				int monitor = screen.GetMonitorAtWindow (this.GdkWindow);
				Rectangle monitor_geometry = screen.GetMonitorGeometry (monitor);
				Move (monitor_geometry.Width * 10 / 100, monitor_geometry.Height * 10 / 100);
			}

			statusDict = new Dictionary<Guid, ProgressStatus> ();
			progress = new Progress <ProgressStatus> (ProcessUpdate);
		}

		#region IProgressReport implementation

		public void Report (float percent, string message, Guid id = default(Guid))
		{
			progress.Report (new ProgressStatus (percent, message, id));
		}

		#endregion

		void ProcessUpdate (ProgressStatus status)
		{
			statusDict [status.ID] = status;
			progressbar1.Text = status.Message;
			progressbar1.Fraction = statusDict.Values.Sum (s => s.Percent) / statusDict.Count;
		}

		/// <summary>
		/// Build the UI manually to avoid resize glitches when we adjust the widget to the selected image.
		/// </summary>
		/// <param name="width">The image width.</param>
		/// <param name="height">The image height.</param>
		void Build (int width, int height)
		{
			WindowPosition = WindowPosition.CenterAlways;
			fixed1 = new Fixed ();
			fixed1.WidthRequest = width;
			fixed1.HeightRequest = height;
			fixed1.HasWindow = false;

			progressbar1 = new ProgressBar ();
			progressbar1.WidthRequest = width * 60 / 100;
			progressbar1.HeightRequest = 20;
			fixed1.Add (progressbar1);
			Fixed.FixedChild w1 = (Fixed.FixedChild)(fixed1 [progressbar1]);
			w1.X = width * 20 / 100;
			w1.Y = height - 50;

			splashimage = new Gtk.Image ();
			splashimage.WidthRequest = width;
			splashimage.HeightRequest = height;
			fixed1.Add (splashimage);

			Add (fixed1);
			if ((Child != null)) {
				Child.ShowAll ();
			}
			DefaultWidth = width;
			DefaultHeight = height;
			Show ();
		}

		class ProgressStatus
		{
			public ProgressStatus (float percent, string message, Guid id)
			{
				Percent = percent;
				Message = message;
				ID = id;
			}

			public float Percent {
				get;
				set;
			}

			public string Message {
				get;
				set;
			}

			public Guid ID {
				get;
				set;
			}
		}
	}

}

