//
//  Copyright (C) 2016 Fluendo S.A.
using System;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using VAS.Core.Interfaces.GUI;

namespace VAS.UI.Dialog
{
	public partial class BusyDialog : Gtk.Dialog, IBusyDialog
	{
		VBox box;
		Label titleLabel;
		ProgressBar progressBar;
		uint timeout;
		object lockObject;

		public BusyDialog (Window parent)
		{
			TransientFor = parent;
			box = new VBox (false, 10);
			titleLabel = new Label ();
			progressBar = new ProgressBar ();
			box.PackStart (titleLabel, true, true, 0);
			box.PackStart (progressBar, true, true, 0);
			box.ShowAll ();
			VBox.PackStart (box);
			Icon = Helpers.Misc.LoadIcon ("longomatch", 28);
			TypeHint = Gdk.WindowTypeHint.Dialog;
			WindowPosition = WindowPosition.Center;
			Modal = true;
			Resizable = false;
			Gravity = Gdk.Gravity.Center; 
			SkipPagerHint = true;
			SkipTaskbarHint = true;
			DefaultWidth = 300;
			DefaultHeight = 100;
			timeout = 0;
		}

		protected override void OnDestroyed ()
		{
			if (timeout != 0) {
				GLib.Source.Remove (timeout);
			}
			base.OnDestroyed ();
		}

		public string Message {
			set {
				titleLabel.Text = value;
			}
		}

		public void Pulse ()
		{
			progressBar.Pulse ();
		}

		public void ShowSync (System.Action action, uint pulseIntervalMS = 100)
		{
			Exception ex = null;
			object lockObject = this.lockObject = new object ();

			if (pulseIntervalMS == 0) {
				pulseIntervalMS = 100;
			}
			Task task = new Task (() => {

				try {
					action.Invoke ();
				} catch (Exception e) {
					ex = e;
				} finally {
					App.Current.GUIToolkit.Invoke (delegate {
						lock (lockObject) {
							Destroy ();
						}
					});
				}
			});
			Monitor.Enter (lockObject);
			task.Start ();
			timeout = GLib.Timeout.Add (pulseIntervalMS, OnTimeout);
			Run ();
			if (ex != null)
				throw ex;
		}

		public void Show (uint pulseIntervalMS = 0)
		{
			timeout = GLib.Timeout.Add (pulseIntervalMS, OnTimeout);
		}

		bool OnTimeout ()
		{
			if (lockObject != null) {
				Monitor.Exit (lockObject);
				lockObject = null;
			}
			Pulse ();
			return true;
		}
	}

}

