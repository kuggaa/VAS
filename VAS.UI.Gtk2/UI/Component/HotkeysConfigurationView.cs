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
using System.Linq;
using Gtk;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services.ViewModel;

namespace VAS.UI.Component
{
	[ViewAttribute (HotkeysConfigurationVM.VIEW)]
	[System.ComponentModel.ToolboxItem (true)]
	public partial class HotkeysConfigurationView : Gtk.Bin, IView<HotkeysConfigurationVM>
	{
		SizeGroup sgroup;
		HotkeysConfigurationVM viewModel;

		public HotkeysConfigurationView ()
		{
			this.Build ();
			categoriesCombo.Changed += HandleCategoriesComboChanged;
		}


		public HotkeysConfigurationVM ViewModel {
			get {
				return viewModel;
			}

			set {
				viewModel = value;
				if (viewModel != null) {
					FillCategories ();
				}
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (HotkeysConfigurationVM)viewModel;
		}

		void FillCategories ()
		{
			foreach (var cat in viewModel.Categories) {
				categoriesCombo.AppendText (cat);
			}
			//select First
			TreeIter iter;
			categoriesCombo.Model.GetIterFirst (out iter);
			categoriesCombo.SetActiveIter (iter);
		}

		void FillKeyConfigs (IEnumerable<KeyConfigVM> keyConfigs)
		{
			foreach (var child in keyConfigVBox.Children.ToList ()) {
				child.Destroy ();
				keyConfigVBox.Remove (child);
			}

			foreach (var config in keyConfigs) {
				HBox box;
				Label descLabel, keyLabel;
				Button edit;
				Gtk.Image editImage;

				box = new HBox ();
				box.Homogeneous = false;
				descLabel = new Label ();
				descLabel.Markup = String.Format ("<b>{0}</b>", config.Description);
				descLabel.Justify = Justification.Left;
				descLabel.SetAlignment (0f, 0.5f);
				descLabel.WidthRequest = 200;
				keyLabel = new Label ();
				keyLabel.Markup = GLib.Markup.EscapeText (config.Key.ToString ());
				keyLabel.Justify = Justification.Left;
				keyLabel.SetAlignment (0f, 0.5f);
				keyLabel.WidthRequest = 200;
				edit = new Button ();
				editImage = new Gtk.Image (Helpers.Misc.LoadIcon ("longomatch-pencil", 24));
				edit.Add (editImage);
				box.PackStart (descLabel, false, false, 0);
				box.PackStart (keyLabel, false, true, 0);
				box.PackStart (edit, false, false, 0);
				box.ShowAll ();

				edit.Clicked += (sender, e) => {
					HotKey hotkey = App.Current.GUIToolkit.SelectHotkey (config.Key);
					if (hotkey != null) {
						if (viewModel.ViewModels.Where ((arg) => arg.Key == hotkey).Any ()) {
							App.Current.Dialogs.ErrorMessage (VAS.Core.Catalog.GetString ("Hotkey already in use: ") +
							GLib.Markup.EscapeText (hotkey.ToString ()), this);
						} else {
							config.Key = hotkey;
							keyLabel.Markup = GLib.Markup.EscapeText (hotkey.ToString ());
						}
					}
				};
				keyConfigVBox.PackStart (box, false, false, 0);
			}
		}

		void HandleCategoriesComboChanged (object sender, EventArgs e)
		{
			FillKeyConfigs (viewModel.ViewModels.Where ((arg) => arg.Category == categoriesCombo.ActiveText));
		}
	}
}
