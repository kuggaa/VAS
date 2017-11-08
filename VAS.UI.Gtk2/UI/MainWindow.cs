//
//  Copyright (C) 2017 Fluendo S.A.
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
using Gtk;
using VAS.Core.Common;
using VAS.Core.Interfaces.GUI;

namespace VAS.UI
{
	public partial class MainWindow : Window, IMainController
	{
		Widget currentPanel;

		public MainWindow () : base (WindowType.Toplevel)
		{
			this.Build ();
			Title = App.Current.SoftwareName;
			Hide ();
		}

		public void Initialize ()
		{
			Show ();
			// Default screen
			var screen = Gdk.Display.Default.DefaultScreen;
			// Which monitor is our window on
			int monitor = screen.GetMonitorAtWindow (this.GdkWindow);
			// Monitor size
			var monitor_geometry = screen.GetMonitorGeometry (monitor);
			// Resize to a convenient size
			this.Resize (monitor_geometry.Width * 80 / 100, monitor_geometry.Height * 80 / 100);
			if (Utils.OS == OperatingSystemID.OSX) {
				this.Move (monitor_geometry.Width * 10 / 100, monitor_geometry.Height * 10 / 100);
			}

			// Configure window icon
			Icon = App.Current.ResourcesLocator.LoadIcon (App.Current.SoftwareIconName).Value;
		}

		public bool SetPanel (IPanel panel)
		{
			if (panel == null) {
				return App.Current.StateController.MoveToHome ().Result;
			}

			if (currentPanel != null) {
				((IPanel)currentPanel).OnUnload ();
				centralBox.Remove (currentPanel);
			}
			Title = panel.Title;
			panel.OnLoad ();
			currentPanel = (Widget)panel;
			centralBox.PackStart (currentPanel, true, true, 0);
			currentPanel.Show ();
			return true;
		}

		protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
		{
			if (!base.OnKeyPressEvent (evnt) || !(Focus is Entry)) {
				App.Current.KeyContextManager.HandleKeyPressed (App.Current.Keyboard.ParseEvent (evnt));
			}
			return true;
		}

		protected override bool OnDeleteEvent (Gdk.Event evnt)
		{
			App.Current.GUIToolkit.Quit ();
			return true;
		}

	}
}
