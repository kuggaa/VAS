//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using Gtk;
using VAS.Core.Interfaces.GUI;
using Xamarin.Forms.Platform.GTK;

namespace VAS.UI.Forms
{
	public class GtkFormsWindow : FormsWindow, IMainController
	{
		public bool SetPanel(IPanel newPanel)
		{
			throw new NotImplementedException();
		}

		protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
		{
			if (!base.OnKeyPressEvent (evnt) || !(Focus is Entry)) {
				App.Current.KeyContextManager.HandleKeyPressed (App.Current.Keyboard.ParseEvent (evnt));
			}
			return true;
		}
	}
}
