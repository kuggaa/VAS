//
//  Copyright (C) 2016 Fluendo S.A.
//

using System;
using Gdk;
using Gtk;
using VAS.Core.Common;
using VAS.Core.MVVMC;
using Image = Gtk.Image;
using VAS.UI.Helpers.Gtk2;
using System.ComponentModel;
using VAS.Core;

namespace VAS.UI.Helpers
{
	public static class ButtonHelper
	{
		/// <summary>
		/// Creates a normal button from a <see cref="Command"/>.
		/// </summary>
		/// <returns>The button.</returns>
		/// <param name="command">Command.</param>
		public static Button CreateButton (Command command)
		{
			Button button = new Button ();
			button.ApplyStyleNormal (command);
			return button;
		}

		/// <summary>
		/// Creates a tab button from a <see cref="Command"/>.
		/// </summary>
		/// <returns>The button.</returns>
		/// <param name="otherButton">Other radio button in the group.</param>
		/// <param name="command">Command.</param>
		public static Button CreateTabButton (Command command, RadioButton otherButton)
		{
			RadioButton button = new RadioButton (otherButton);
			button.DrawIndicator = false;
			button.ApplyStyleTab (command);
			return button;
		}

		/// <summary>
		/// Creates a button dialog from a <see cref="Command"/>.
		/// </summary>
		/// <returns>The button dialog.</returns>
		/// <param name="command">Command.</param>
		public static Button CreateDialogButton (Command command)
		{
			Button button = new Button ();
			button.ApplyStyleDialog (command);
			return button;
		}

		/// <summary>
		/// Applies the normal style normal to the given button and binds it to a command.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="command">Command.</param>
		public static void ApplyStyleNormal (this Button button, Command command = null)
		{
			if (command != null) {
				button.Bind (command, App.Current.Style.IconMediumWidth);
			}
			button.ApplyStyle (StyleConf.ButtonNormal, App.Current.Style.ButtonNormalWidth, App.Current.Style.IconMediumWidth);
		}

		/// <summary>
		/// Applies the normal style normal to the given button and binds it to a command.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="command">Command.</param>
		public static void ApplyStyleTab (this Button button, Command command = null)
		{
			if (command != null) {
				button.Bind (command, App.Current.Style.IconLargeWidth);
			}
			button.ApplyStyle (StyleConf.ButtonTab, App.Current.Style.ButtonTabWidth, App.Current.Style.IconLargeWidth);
			button.ImagePosition = PositionType.Left;
		}

		/// <summary>
		/// Applies the dialog style to the given button and binds it to a command.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="command">Command.</param>
		public static void ApplyStyleDialog (this Button button, Command command = null)
		{
			if (command != null) {
				button.Bind (command, App.Current.Style.IconMediumWidth);
			}
			button.ApplyStyle (StyleConf.ButtonDialog, App.Current.Style.ButtonDialogHeight, App.Current.Style.IconMediumWidth);
		}

		/// <summary>
		/// Sets an icon image to a button.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="icon">Icon.</param>
		static public void SetImage (this Button button, VAS.Core.Common.Image icon)
		{
			if (icon == null) {
				return;
			}

			AspectImage image = new AspectImage ();
			image.Image = icon;
			button.Image = image;
		}

		/// <summary>
		/// Configures the specified button with an icon, text, tooltip text and clicked callback.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="icon">Icon.</param>
		/// <param name="text">Text.</param>
		/// <param name="tooltipText">Tooltip text.</param>
		/// <param name="callback">Callback.</param>
		public static void Configure (this Button button, VAS.Core.Common.Image icon, string text, string tooltipText, EventHandler callback)
		{
			button.SetImage (icon);
			if (text != null) {
				button.Label = text;
			}
			button.TooltipText = tooltipText;
			button.Clicked += callback;
		}

		/// <summary>
		/// Bind a button to  the specified button, command and parameter.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="command">Command.</param>
		/// <param name="parameter">Parameter.</param>
		public static void Bind (this Button button, Command command, int iconSize, object parameter = null)
		{
			var image = Resources.LoadIcon (command.IconName, iconSize);
			button.SetImage (image);
			button.Label = command.Text;
			button.TooltipText = command.ToolTipText;

			button.Sensitive = command.CanExecute (parameter);
			EventHandler handler = (sender, e) => {
				button.Sensitive = command.CanExecute (parameter);
			};
			command.CanExecuteChanged += handler;
			button.Destroyed += (sender, e) => {
				command.CanExecuteChanged -= handler;
			};
			button.Clicked += (sender, e) => {
				var radioButton = sender as RadioButton;
				if (radioButton != null && radioButton.Active == false) {
					return;
				}
				command.Execute (parameter);
			};
		}

		static void ApplyStyle (this Button button, string buttonStyle, int buttonSize, int imageSize)
		{
			button.Name = buttonStyle;
			button.WidthRequest = buttonSize;
			button.HeightRequest = buttonSize;
			if (button.Image != null) {
				button.Image.Name = buttonStyle;
				button.Image.WidthRequest = imageSize;
				button.Image.HeightRequest = imageSize;
			}
		}
	}
}
