//
//  Copyright (C) 2016 Fluendo S.A.
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
using VAS.Core;

namespace VAS.UI.UI.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class DatePicker : Gtk.Bin
	{
		public event EventHandler ValueChanged;

		DateTime date;

		public DatePicker ()
		{
			this.Build ();

			datebuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-calendar", 24);

			datebutton.Clicked += HandleClicked;
			dateentry.Changed += HandleChanged;
			Date = DateTime.UtcNow;
		}

		/// <summary>
		/// Gets or sets the date.
		/// </summary>
		/// <value>The date selected.</value>
		public DateTime Date {
			set {
				date = value;
				dateentry.Text = value.ToShortDateString ();
			}
			get {
				return date;
			}
		}

		/// <summary>
		/// Sets the background color for a DatePicker in a particular state.
		/// It also changes his childs background.
		/// </summary>
		/// <param name="state">Type.</param>
		/// <param name="color">Color.</param>
		public new void ModifyBg (Gtk.StateType state, Gdk.Color color)
		{
			base.ModifyBg (state, color);
			Child.ModifyBg (state, color);
		}

		void HandleChanged (object sender, EventArgs e)
		{
			// Proxy event to potential listeners
			if (this.ValueChanged != null) {
				this.ValueChanged (this, EventArgs.Empty);
			}
		}

		void HandleClicked (object sender, EventArgs e)
		{
			Date = App.Current.Dialogs.SelectDate (Date, this).Result;
		}
	}
}
