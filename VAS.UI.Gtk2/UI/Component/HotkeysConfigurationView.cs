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
using System.Linq;
using Gtk;
using VAS.Core.Hotkeys;
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
			sgroup = new SizeGroup (SizeGroupMode.Horizontal);
		}


		public HotkeysConfigurationVM ViewModel {
			get {
				return viewModel;
			}

			set {
				viewModel = value;
				if (viewModel != null) {
					int i = 0;
					foreach (KeyConfigVM key in viewModel.ViewModels) {
						AddWidget (key, i);
						i++;
					}
				}
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (HotkeysConfigurationVM)viewModel;
		}

		public void AddWidget (KeyConfigVM keyconfig, int position)
		{
			uint row_top, row_bottom, col_left, col_right;
			HBox box;
			Label descLabel, keyLabel;
			Button edit;
			Gtk.Image editImage;

			box = new HBox ();
			box.Spacing = 5;
			descLabel = new Label ();
			descLabel.Markup = String.Format ("<b>{0}</b>", keyconfig.Description);
			keyLabel = new Label ();
			keyLabel.Markup = GLib.Markup.EscapeText (keyconfig.Key.ToString ());
			edit = new Button ();
			editImage = new Gtk.Image (Helpers.Misc.LoadIcon ("longomatch-pencil", 24));
			edit.Add (editImage);
			box.PackStart (descLabel, true, true, 0);
			box.PackStart (keyLabel, false, true, 0);
			box.PackStart (edit, false, true, 0);
			box.ShowAll ();

			sgroup.AddWidget (keyLabel);
			descLabel.Justify = Justification.Left;
			descLabel.SetAlignment (0f, 0.5f);
			edit.Clicked += (sender, e) => {
				HotKey hotkey = App.Current.GUIToolkit.SelectHotkey (keyconfig.Key);
				if (hotkey != null) {
					if (viewModel.ViewModels.Where ((arg) => arg.Key == hotkey).Any ()) {
						App.Current.Dialogs.ErrorMessage (VAS.Core.Catalog.GetString ("Hotkey already in use: ") +
						GLib.Markup.EscapeText (hotkey.ToString ()), this);
					} else {
						keyconfig.Key = hotkey;
						keyLabel.Markup = GLib.Markup.EscapeText (hotkey.ToString ());
					}
				}
			};

			row_top = (uint)(position / table.NColumns);
			row_bottom = (uint)row_top + 1;
			col_left = (uint)position % table.NColumns;
			col_right = (uint)col_left + 1;
			table.Attach (box, col_left, col_right, row_top, row_bottom);
		}
	}
}
