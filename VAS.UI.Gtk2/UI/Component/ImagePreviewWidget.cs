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
using VAS.UI.Helpers;

namespace VAS.UI.UI.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ImagePreviewWidget : Gtk.Bin
	{
		string title;
		public event ButtonPressEventHandler ImageButtonPressEvent;

		public ImagePreviewWidget ()
		{
			this.Build ();
			this.fieldeventbox.ButtonPressEvent += HandleImageButtonPress;
		}

		public ImageView ImageView {
			get {
				return image;
			}
		}

		public Button ResetButton {
			get {
				return resetbutton;
			}
		}

		public string Title {
			get {
				return title;
			}
			set {
				title = value;
				imageTitleLabel.Text = value;
			}
		}

		void HandleImageButtonPress (object o, ButtonPressEventArgs args)
		{
			if (ImageButtonPressEvent != null) {
				ImageButtonPressEvent (this, args);
			}
		}

	}
}
