//
//  Copyright (C) 2016 Fluendo S.A.
//

using System;
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
				button.Bind (command);
			}
			button.ApplyStyle (StyleConf.ButtonNormal, App.Current.Style.ButtonNormalWidth);
		}

		/// <summary>
		/// Applies the limit style to the given button and binds it to a command.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="command">Command.</param>
		public static void ApplyStyleLimit (this Button button, Command command = null)
		{
			if (command != null) {
				button.Bind (command);
			}
			button.ApplyStyle (StyleConf.ButtonLimit, App.Current.Style.ButtonLimitWidth);
		}

		/// <summary>
		/// Applies the normal style normal to the given button and binds it to a command.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="command">Command.</param>
		public static void ApplyStyleTab (this Button button, Command command = null)
		{
			if (command != null) {
				button.Bind (command);
			}
			button.ApplyStyle (StyleConf.ButtonTab, App.Current.Style.ButtonTabWidth);
		}

		public static void ApplyStyleRemove (this Button button, Command command = null)
		{
			if (command != null) {
				button.Bind (command);
			}
			button.ApplyStyle (StyleConf.ButtonRemove, App.Current.Style.ButtonRemoveWidth);
			button.HeightRequest = App.Current.Style.ButtonRemoveHeight;
			button.SetAlignment (0.0f, 0.5f);
		}

		/// <summary>
		/// Applies the dialog style to the given button and binds it to a command.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="command">Command.</param>
		public static void ApplyStyleDialog (this Button button, Command command = null)
		{
			if (command != null) {
				button.Bind (command);
			}
			button.Name = StyleConf.ButtonDialog;
			button.HeightRequest = App.Current.Style.ButtonDialogHeight;
		}

		/// <summary>
		/// Sets an icon image to a button.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="icon">Icon.</param>
		static public void SetImage (this Button button, Pixbuf icon)
		{
			if (icon == null) {
				return;
			}

			Image image = new Image ();
			button.Image = image;
			image.Pixbuf = icon;
		}

		/// <summary>
		/// Configures the specified button with an icon, text, tooltip text and clicked callback.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="icon">Icon.</param>
		/// <param name="text">Text.</param>
		/// <param name="tooltipText">Tooltip text.</param>
		/// <param name="callback">Callback.</param>
		public static void Configure (this Button button, Pixbuf icon, string text, string tooltipText, EventHandler callback)
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
		public static void Bind (this Button button, Command command, object parameter = null)
		{
			button.SetImage (command.Icon?.Value);
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

		static void ApplyStyle (this Button button, string buttonStyle, int buttonSize)
		{
			button.Name = buttonStyle;
			button.WidthRequest = buttonSize;
			button.HeightRequest = buttonSize;
			if (button.Image != null) {
				button.ImagePosition = PositionType.Left;
			}
		}
	}
}
