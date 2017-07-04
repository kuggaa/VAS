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
using System.Collections.Generic;
using Gtk;
using Pango;
using VAS.Core.Common;
using Handlers = VAS.Core.Handlers;

namespace VAS.UI
{
	public partial class SliderView : Window
	{
		public event Handlers.ValueChangedHandler ValueChanged;
		double buttonIncrement;
		double lowerValue;

		public SliderView (double lowerValue, double upperValue, double pageIncrement, double buttonIncrement)
			: base (WindowType.Toplevel)
		{
			this.Build ();
			this.buttonIncrement = buttonIncrement;
			this.lowerValue = lowerValue;
			moreButtonImage.Pixbuf = Helpers.Misc.LoadIcon ("vas-plus", StyleConf.PlayerCapturerIconSize);
			lessButtonImage.Pixbuf = Helpers.Misc.LoadIcon ("vas-minus", StyleConf.PlayerCapturerIconSize);
			morebutton.Clicked += OnMorebuttonClicked;
			lessbutton.Clicked += OnLessbuttonClicked;
			scale.Adjustment.Upper = upperValue;
			scale.Adjustment.Lower = lowerValue;
			scale.Adjustment.PageIncrement = buttonIncrement;
			scale.Adjustment.StepIncrement = pageIncrement;
			scale.ModifyFont (FontDescription.FromString (App.Current.Style.Font + " 8"));
			scale.ValueChanged += OnScaleValueChanged;
			scale.FormatValue += HandleScaleFormatValue;
			scale.ButtonPressEvent += HandleScaleFormatValue;
			scale.ButtonReleaseEvent += HandleScaleFormatValue;
		}

		/// <summary>
		/// Gets or sets the function that formats the returned value.
		/// </summary>
		/// <value>The format value.</value>
		public Func<double, string> FormatValue { get; set; }

		public void SetValue (double value)
		{
			scale.Value = value;
		}

		protected void OnMorebuttonClicked (object sender, EventArgs e)
		{
			scale.Value = scale.Value + buttonIncrement;
		}

		protected void OnLessbuttonClicked (object sender, EventArgs e)
		{
			scale.Value = scale.Value - buttonIncrement;
		}

		protected void OnScaleValueChanged (object sender, EventArgs e)
		{
			ValueChanged (scale.Value);
		}

		protected void OnFocusOutEvent (object o, FocusOutEventArgs args)
		{
			this.Hide ();
		}

		void HandleScaleFormatValue (object o, FormatValueArgs args)
		{
			args.RetVal = FormatValue (scale.Value);
		}

		/// <summary>
		/// Handles the ratescale button press.
		/// Default button 1 action is used in button 2 and button 3
		/// </summary>
		/// <param name="o">source</param>
		/// <param name="args">Arguments.</param>
		[GLib.ConnectBefore]
		void HandleScaleFormatValue (object o, ButtonPressEventArgs args)
		{
			if (args.Event.Button == 1) {
				args.Event.SetButton (2);
			} else {
				args.Event.SetButton (1);
			}
		}

		/// <summary>
		/// Handles the ratescale button release.
		/// Default button 1 action is used in button 2 and button 3
		/// </summary>
		/// <param name="o">source</param>
		/// <param name="args">Arguments.</param>
		[GLib.ConnectBefore]
		void HandleScaleFormatValue (object o, ButtonReleaseEventArgs args)
		{
			if (args.Event.Button == 1) {
				args.Event.SetButton (2);
			} else {
				args.Event.SetButton (1);
			}
		}
	}
}
