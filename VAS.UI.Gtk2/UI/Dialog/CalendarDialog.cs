//
//  Copyright (C) 2016 Fluendo S.A.
using System;
using Gtk;

namespace VAS.UI.Dialog
{
	public partial class CalendarDialog : Gtk.Dialog
	{
		DateTime selectedDate;

		public CalendarDialog (DateTime date)
		{
			this.Build ();
			SkipPagerHint = true;
			SkipTaskbarHint = true;
			//Decorated = false;
			Modal = true;
			calendar1.Date = date;
		}

		public DateTime Date {
			get {
				return selectedDate;
			}
		}

		protected virtual void OnCalendar1DaySelectedDoubleClick (object sender, System.EventArgs e)
		{
			selectedDate = calendar1.Date;
			this.Respond (ResponseType.Accept);
		}

		protected virtual void OnCalendar1DaySelected (object sender, System.EventArgs e)
		{
			DateTime d = calendar1.Date;
			selectedDate = new DateTime (d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Utc);
		}
	}
}

