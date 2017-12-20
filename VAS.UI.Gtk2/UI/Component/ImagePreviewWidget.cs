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
using VAS.UI.Helpers;

namespace VAS.UI.Component
{
	/// <summary>
	/// Image preview widget.
	/// It shows an image that can be clicked and a reset button.
	/// </summary>
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ImagePreviewWidget : Gtk.Bin
	{
		public event ButtonPressEventHandler ImageButtonPressEvent;

		int resetButtonHeight;
		string title;

		public ImagePreviewWidget ()
		{
			this.Build ();
			this.fieldeventbox.ButtonPressEvent += HandleImageButtonPress;
			resetbutton.ApplyStyle (StyleConf.ButtonRegular);
		}

		/// <summary>
		/// Gets the image view.
		/// </summary>
		/// <value>The image view.</value>
		public ImageView ImageView {
			get {
				return image;
			}
		}

		/// <summary>
		/// Gets the reset button.
		/// </summary>
		/// <value>The reset button.</value>
		public Button ResetButton {
			get {
				return resetbutton;
			}
		}

		/// <summary>
		/// Title to show under the image.
		/// </summary>
		/// <value>The title.</value>
		public string Title {
			get {
				return title;
			}
			set {
				title = value;
				imageTitleLabel.Text = value;
			}
		}

		/// <summary>
		/// Gets or sets the height of the reset button.
		/// Setting this applies the regular style.
		/// </summary>
		/// <value>The height of the reset button.</value>
		public int ResetButtonHeight {
			get {
				return resetButtonHeight;
			}
			set {
				resetButtonHeight = value;
				resetbutton.ApplyStyle (StyleConf.ButtonRegular, resetButtonHeight);
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
