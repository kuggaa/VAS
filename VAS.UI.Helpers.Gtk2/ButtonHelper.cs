//
//  Copyright (C) 2016 Fluendo S.A.
//

using System;
using System.Linq;
using Gdk;
using Gtk;
using VAS.Core.Common;
using VAS.Core.MVVMC;
using Image = Gtk.Image;

namespace VAS.UI.Helpers
{
	public static class ButtonHelper
	{
		/// <summary>
		/// Creates a button normal.
		/// </summary>
		/// <returns>The button normal.</returns>
		/// <param name="icon">Icon.</param>
		/// <param name="toolTip">Tool tip.</param>
		/// <param name="command">Command.</param>
		public static Button CreateButtonNormal (Pixbuf icon, string toolTip, Command command = null)
		{
			Button button;
			button = new Button ();
			ApplyStyleNormal (button, icon, toolTip, command);

			return button;
		}

		/// <summary>
		/// Creates a button tab.
		/// </summary>
		/// <returns>The button tab.</returns>
		/// <param name="icon">Icon.</param>
		/// <param name="toolTip">Tool tip.</param>
		/// <param name="otherbutton">Otherbutton.</param>
		/// <param name="command">Command.</param>
		public static Button CreateButtonTab (Pixbuf icon, string toolTip, RadioButton otherbutton, Command command = null)
		{
			Button button;
			button = new RadioButton (otherbutton);
			(button as RadioButton).DrawIndicator = false;
			ApplyStyleTab (button, icon, toolTip, command);

			return button;
		}

		/// <summary>
		/// Creates a button dialog.
		/// </summary>
		/// <returns>The button dialog.</returns>
		/// <param name="toolTip">Tool tip.</param>
		/// <param name="command">Command.</param>
		public static Button CreateButtonDialog (string toolTip, Command command = null)
		{
			Button button;
			button = new Button ();
			ApplyStyleDialog (button, toolTip, command);

			return button;
		}

		/// <summary>
		/// Applies style normal to the given button.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="icon">Icon.</param>
		/// <param name="toolTip">Tool tip.</param>
		/// <param name="action">Action.</param>
		public static void ApplyStyleNormal (this Button button, Pixbuf icon, string toolTip, EventHandler action)
		{
			ApplyStyleNormal (button, icon, toolTip, null, action);
		}

		/// <summary>
		/// Applies style normal to the given button.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="icon">Icon.</param>
		/// <param name="toolTip">Tool tip.</param>
		/// <param name="command">Command.</param>
		public static void ApplyStyleNormal (this Button button, Pixbuf icon, string toolTip, Command command = null, EventHandler action = null)
		{
			Image image = FillButtonWithImage (button, icon, toolTip, command, action, StyleConf.ButtonNormal);

			button.WidthRequest = App.Current.Style.ButtonNormalWidth;
			button.HeightRequest = App.Current.Style.ButtonNormalHeight;
			image.WidthRequest = App.Current.Style.IconLargeWidth;
			image.HeightRequest = App.Current.Style.IconLargeHeight;
		}

		/// <summary>
		/// Applies style tab to the given button.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="icon">Icon.</param>
		/// <param name="toolTip">Tool tip.</param>
		/// <param name="action">Action.</param>
		public static void ApplyStyleTab (this Button button, Pixbuf icon, string toolTip, EventHandler action)
		{
			ApplyStyleTab (button, icon, toolTip, null, action);
		}

		/// <summary>
		/// Applies style tab to the given button.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="icon">Icon.</param>
		/// <param name="toolTip">Tool tip.</param>
		/// <param name="command">Command.</param>
		public static void ApplyStyleTab (this Button button, Pixbuf icon, string toolTip, Command command = null, EventHandler action = null)
		{
			Image image = FillButtonWithImage (button, icon, toolTip, command, action, StyleConf.ButtonTab);

			button.WidthRequest = App.Current.Style.ButtonTabWidth;
			button.HeightRequest = App.Current.Style.ButtonTabHeight;
			image.WidthRequest = App.Current.Style.IconLargeWidth;
			image.HeightRequest = App.Current.Style.IconLargeHeight;
		}

		/// <summary>
		/// Applies style dialog to the given button.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="toolTip">Tool tip.</param>
		/// <param name="action">Action.</param>
		public static void ApplyStyleDialog (this Button button, string toolTip, EventHandler action)
		{
			ApplyStyleDialog (button, toolTip, null, action);
		}

		/// <summary>
		/// Applies style dialog to the given button.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="toolTip">Tool tip.</param>
		/// <param name="command">Command.</param>
		public static void ApplyStyleDialog (this Button button, string toolTip, Command command = null, EventHandler action = null)
		{
			FillButton (button, toolTip, command, action, StyleConf.ButtonDialog);

			button.HeightRequest = App.Current.Style.ButtonDialogHeight;
		}

		static void FillButton (Button button, string toolTip, Command command, EventHandler action, string buttonStyle)
		{
			button.Name = buttonStyle;
			button.TooltipMarkup = toolTip;

			if (command != null) {
				button.Bind (command);
			} else if (action != null) {
				button.Clicked += action;
			}
		}

		static Image FillButtonWithImage (Button button, Pixbuf icon, string toolTip, Command command, EventHandler action, string buttonStyle)
		{
			FillButton (button, toolTip, command, action, buttonStyle);

			if (icon == null) {
				return null;
			}

			Image image = button.Children.FirstOrDefault () as Image;
			if (image == null) {
				image = new Image ();
				button.Add (image);
			}
			image.Pixbuf = icon;

			return image;
		}
	}
}
