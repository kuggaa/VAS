//
//  Copyright (C) 2016 Fluendo S.A.
//

using System;
using System.Linq;
using Gdk;
using Gtk;
using VAS;
using VAS.Core.Common;
using VAS.Core.MVVMC;
using Image = Gtk.Image;

namespace VAS.UI.Helpers
{
	public static class ButtonHelper
	{
		public static Button CreateButtonNormal (Pixbuf icon, string toolTip, Command command = null)
		{
			Button button;
			button = new Button ();
			ApplyStyleNormal (button, icon, toolTip, command);
			return button;
		}

		public static Button CreateButtonTab (Pixbuf icon, string toolTip, RadioButton otherbutton, Command command = null)
		{
			Button button;
			button = new RadioButton (otherbutton);
			(button as RadioButton).DrawIndicator = false;
			ApplyStyleTab (button, icon, toolTip, command);

			return button;
		}

		public static void ApplyStyleNormal (Button button, Pixbuf icon, string toolTip, Command command = null)
		{
			Image image = FillButton (button, icon, toolTip, command, StyleConf.ButtonNormal);

			button.WidthRequest = App.Current.Style.ButtonNormalWidth;
			button.HeightRequest = App.Current.Style.ButtonNormalHeight;
			image.WidthRequest = App.Current.Style.IconXLargeWidth;
			image.HeightRequest = App.Current.Style.IconXLargeHeight;
		}

		public static void ApplyStyleTab (Button button, Pixbuf icon, string toolTip, Command command = null)
		{
			Image image = FillButton (button, icon, toolTip, command, StyleConf.ButtonTab);

			button.WidthRequest = App.Current.Style.ButtonTabWidth;
			button.HeightRequest = App.Current.Style.ButtonTabHeight;
			image.WidthRequest = App.Current.Style.IconLargeWidth;
			image.HeightRequest = App.Current.Style.IconLargeHeight;
		}

		static Image FillButton (Button button, Pixbuf icon, string toolTip, Command command, string buttonStyle)
		{
			button.Name = buttonStyle;
			button.TooltipMarkup = toolTip;
			Image image = button.Children.FirstOrDefault () as Image;
			if (image == null) {
				image = new Image ();
				button.Add (image);
			}
			image.Pixbuf = icon;
			if (command != null) {
				button.Bind (command);
			}
			return image;
		}
	}
}
