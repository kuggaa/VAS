//
//  Copyright (C) 2016 Fluendo S.A.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//

using System;
using Gtk;
using VAS.Core.Common;
using VAS.Core.MVVMC;
using Image = VAS.Core.Common.Image;

namespace VAS.UI.Helpers
{
	public static class ButtonHelper
	{
		/// <summary>
		/// Creates a button with the normal style.
		/// </summary>
		/// <returns>The button.</returns>
		public static Button CreateButton ()
		{
			Button button = new Button ();
			button.ApplyStyleNormal ();
			return button;
		}

		/// <summary>
		/// Creates a radio button with the tab style.
		/// </summary>
		/// <returns>The button.</returns>
		/// <param name="otherButton">Other radio button in the group.</param>
		public static Button CreateTabButton (RadioButton otherButton)
		{
			RadioButton button = new RadioButton (otherButton);
			button.DrawIndicator = false;
			button.ApplyStyleTab ();
			return button;
		}

		/// <summary>
		/// Creates a button with the dialog style.
		/// </summary>
		/// <returns>The button dialog.</returns>
		public static Button CreateDialogButton ()
		{
			Button button = new Button ();
			button.ApplyStyleDialog ();
			return button;
		}

		/// <summary>
		/// Applies the normal style normal to the given button.
		/// </summary>
		/// <param name="button">Button.</param>
		public static void ApplyStyleNormal (this Button button)
		{
			button.ApplyStyle (StyleConf.ButtonNormal, App.Current.Style.ButtonNormalWidth);
		}

		/// <summary>
		/// Applies the limit style to the given button.
		/// </summary>
		/// <param name="button">Button.</param>
		public static void ApplyStyleLimit (this Button button)
		{
			button.ApplyStyle (StyleConf.ButtonLimit, App.Current.Style.ButtonLimitWidth);
		}

		/// <summary>
		/// Applies the normal style normal to the given button.
		/// </summary>
		/// <param name="button">Button.</param>
		public static void ApplyStyleTab (this Button button)
		{
			button.ApplyStyle (StyleConf.ButtonTab, App.Current.Style.ButtonTabWidth);
		}

		public static void ApplyStyleRemove (this Button button)
		{
			button.ApplyStyle (StyleConf.ButtonRemove, App.Current.Style.ButtonRemoveWidth);
		}

		/// <summary>
		/// Applies the dialog style to the given button.
		/// </summary>
		/// <param name="button">Button.</param>
		public static void ApplyStyleDialog (this Button button)
		{
			button.Name = StyleConf.ButtonDialog;
			button.HeightRequest = App.Current.Style.ButtonDialogHeight;
			button.CanFocus = true;
			button.FocusOnClick = true;
		}

		/// <summary>
		/// Sets an icon image to a button.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="icon">Icon.</param>
		public static void SetImage (this Button button, Image icon, uint size = 0)
		{
			if (icon == null) {
				return;
			}

			ImageView image = null;

			foreach (var container in button.Children) {
				if (container is ImageView) {
					image = (ImageView)container;
					break;
				}
			}

			if (image == null) {
				image = new ImageView ();
				button.Image = image;
			}
			if (size != 0) {
				image.SetSize ((int)size);
			}
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
		public static void Configure (this Button button, Image icon, string text, string tooltipText, EventHandler callback)
		{
			if (icon != null) {
				button.SetImage (icon);
			}
			if (text != null) {
				button.Label = text;
			}
			if (tooltipText != null) {
				button.TooltipText = tooltipText;
			}
			if (callback != null) {
				button.Clicked += callback;
			}
			button.CanFocus = false;
			button.FocusOnClick = false;
		}

		// FIXME: this method should be gone when we use the ButtonBindings everywhere
		public static void BindManually (this Button button, Command command, object parameter = null)
		{
			button.Configure (command.Icon, command.Text, command.ToolTipText, null);

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

		/// <summary>
		/// Links a group of toggle buttons to act like a radio button group.
		/// </summary>
		/// <param name="buttons">Buttons.</param>
		public static void LinkToggleButtons (params ToggleButton [] buttons)
		{
			foreach (ToggleButton button in buttons) {
				button.Toggled += (sender, e) => {
					if (button.Active) {
						foreach (var otherButton in buttons) {
							if (otherButton != button) {
								otherButton.Active = false;
							}
						}
					}
				};
			}
		}

		/// <summary>
		/// Applies the style to a button with the option to also define the size.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="buttonStyle">Button style.</param>
		/// <param name="buttonSize">Button size.</param>
		public static void ApplyStyle (this Button button, string buttonStyle, int buttonSize = 0)
		{
			ApplyStyle (button, buttonStyle, buttonSize, buttonSize);
		}

		/// <summary>
		/// Applies the style to a button with the option to also define the size in height and width.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="buttonStyle">Button style.</param>
		/// <param name="buttonHeight">Button height.</param>
		/// <param name="buttonWidth">Button width.</param>
		public static void ApplyStyle (this Button button, string buttonStyle, int buttonWidth, int buttonHeight)
		{
			button.Name = buttonStyle;
			button.WidthRequest = buttonWidth;
			button.HeightRequest = buttonHeight;
			var imageView = button.Image as ImageView ?? button.Child as ImageView;
			if (imageView != null) {
				button.ImagePosition = PositionType.Left;
				if (buttonWidth != 0 && buttonHeight != 0) {
					imageView.SetSize (buttonWidth - 5, buttonHeight - 5);
				}
			}
			button.CanFocus = false;
			button.FocusOnClick = false;
		}
	}
}
