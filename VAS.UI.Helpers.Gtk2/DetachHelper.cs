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
using Gtk;
using VAS.Core.Common;
using VAS.Core.MVVMC;
using Action = System.Action;

namespace VAS.UI.Helpers
{
	/// <summary>
	/// Helper to detach any widget and re-attach it again to the specified one.
	/// </summary>
	public class DetachHelper : DisposableBase
	{
		ExternalWindow externalWindow;
		Widget widgetToDetach, widgetWhereReattach;
		Action callbackFunction;
		string windowTitle;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:VAS.UI.Helpers.Gtk2.DetachHelper"/> class.
		/// </summary>
		/// <param name="widgetToDetach">Widget to detach.</param>
		/// <param name="windowTitle">Window title.</param>
		/// <param name="widgetWhereReattach">Widget where reattach.</param>
		/// <param name="callbackFunction">Function executed detaching and attaching by the view in order to do specific work as
		public DetachHelper (Widget widgetToDetach, string windowTitle, Widget widgetWhereReattach, Action callbackFunction)
		{
			this.widgetToDetach = widgetToDetach;
			this.widgetWhereReattach = widgetWhereReattach;
			this.windowTitle = windowTitle;
			this.callbackFunction = callbackFunction;
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			if (externalWindow != null) {
				externalWindow.Destroy ();
			}
		}

		/// <summary>
		/// Detach the specified widget to an external window or re-attach it again to its previous container.
		/// </summary>
		public void Detach ()
		{
			if (externalWindow == null) {
				Log.Debug ("Detaching widget");
				externalWindow = new ExternalWindow ();
				externalWindow.Title = windowTitle;
				int window_width = widgetToDetach.Allocation.Width;
				int window_height = widgetToDetach.Allocation.Height;
				externalWindow.SetDefaultSize (window_width, window_height);
				externalWindow.DeleteEvent += (o, args) => Detach ();
				externalWindow.Show ();
				widgetToDetach.Reparent (externalWindow.Box);
				// Hack to reposition widget window in widget for OSX
				externalWindow.Resize (window_width + 10, window_height);
			} else {
				Log.Debug ("Attaching widget again");
				widgetToDetach.Reparent (widgetWhereReattach);
				externalWindow.Destroy ();
				externalWindow = null;
			}
			callbackFunction.Invoke ();
		}
	}
}
