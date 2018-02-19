//
//  Copyright (C) 2015 Fluendo S.A.
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
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Resources.Styles;
using Image = VAS.Core.Common.Image;

namespace VAS.UI.Helpers
{

	public class IconNotebookHelper
	{

		public IconNotebookHelper (Notebook notebook)
		{
			Notebook = notebook;
			TabIcons = new Dictionary<Widget, Tuple<Image, Image, Color>> (notebook.NPages);
			TabToolTips = new Dictionary<Widget, string> (notebook.NPages);
			CurrentPage = notebook.CurrentPage;

			notebook.ShowBorder = false;
			notebook.SwitchPage += HandleSwitchPage;
		}

		Notebook Notebook {
			get;
			set;
		}

		Dictionary<Widget, Tuple<Image, Image, Color>> TabIcons {
			get;
			set;
		}

		Dictionary<Widget, string> TabToolTips {
			get;
			set;
		}

		int CurrentPage {
			get;
			set;
		}

		/// <summary>
		/// Sets the tab icon.
		/// </summary>
		/// <param name="widget">Widget which is being modifyed</param>
		/// <param name="icon">Icon showed when the tab is not selected</param>
		/// <param name="activeIcon">Icon showed when the tab is selected</param>
		/// <param name="tooltiptext">Text to add to the tab of the widget as tooltip</param>
		public void SetTabIcon (Widget widget, string icon, string activeIcon, string tooltiptext, Color color = null)
		{
			var pixIcon = App.Current.ResourcesLocator.LoadIcon (icon);
			var pixActiveIcon = App.Current.ResourcesLocator.LoadIcon (activeIcon);
			SetTabIcon (widget, pixIcon, pixActiveIcon, tooltiptext, color);
		}

		/// <summary>
		/// Sets the tab icon.
		/// </summary>
		/// <param name="widget">Widget which is being modifyed</param>
		/// <param name="pixIcon">Icon showed when the tab is not selected</param>
		/// <param name="pixActiveIcon">Icon showed when the tab is selected</param>
		/// <param name="tooltiptext">Text to add to the tab of the widget as tooltip</param>
		public void SetTabIcon (Widget widget, Image pixIcon, Image pixActiveIcon, string tooltiptext, Color color = null)
		{
			TabIcons.Add (widget, new Tuple<Image, Image, Color> (pixIcon, pixActiveIcon, color));
			TabToolTips.Add (widget, tooltiptext);
		}

		/// <summary>
		/// Sets the tab icon.
		/// </summary>
		/// <param name="tabIndex">Index of the tab which is being modifyed</param>
		/// <param name="icon">Icon showed when the tab is not selected</param>
		/// <param name="activeIcon">Icon showed when the tab is selected</param>
		/// <param name="tooltiptext">Text to add to the tab of the widget as tooltip</param>
		public void SetTabIcon (int tabIndex, string icon, string activeIcon, string tooltiptext)
		{
			SetTabIcon (Notebook.GetNthPage (tabIndex), icon, activeIcon, tooltiptext, null);
		}

		public void UpdateTabs ()
		{
			for (int i = 0; i < Notebook.NPages; i++) {
				SetTabProps (Notebook.GetNthPage (i), i == Notebook.CurrentPage);
			}
		}

		void HandleSwitchPage (object o, SwitchPageArgs args)
		{
			SetTabProps (Notebook.GetNthPage (CurrentPage), false);
			SetTabProps (Notebook.GetNthPage ((int)args.PageNum), true);
			CurrentPage = Notebook.CurrentPage;
		}

		void SetTabProps (Widget widget, bool active)
		{
			if (widget == null) {
				return;
			}

			ImageView img;

			img = Notebook.GetTabLabel (widget) as ImageView;
			if (img == null) {
				img = new ImageView ();
				img.SetSize (Sizes.NotebookTabSize, Sizes.NotebookTabSize);
				img.Xpad = img.Ypad = (Sizes.NotebookTabSize - Sizes.NotebookTabIconSize) / 2;
				Notebook.SetTabLabel (widget, img);
			}
			try {
				var tuple = TabIcons [widget];
				img.Image = active ? tuple.Item2 : tuple.Item1;
				img.TooltipText = TabToolTips [widget];
				img.MaskColor = tuple.Item3;
			} catch (KeyNotFoundException ex) {
				Log.Warning ("No icon set for tab number <" + Notebook.PageNum (widget) + "> with child <" + widget + ">");
			}

		}
	}
}

