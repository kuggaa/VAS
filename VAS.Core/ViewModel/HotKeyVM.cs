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
using System.ComponentModel;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Core.ViewModel
{
	public class HotKeyVM : ViewModelBase<HotKey>
	{

		public HotKeyVM ()
		{
			UpdateHotkeyCommand = new Command ((Action)UpdateHotkey);
			UpdateHotkeyCommand.Executable = true;
		}

		/// <summary>
		/// Gets the name of the <see cref="Job"/>.
		/// </summary>
		/// <value>The name.</value>
		public string Name {
			get {
				return Model?.ToString ();
			}
		}

		/// <summary>
		/// Gets or sets the modifier.
		/// </summary>
		/// <value>The modifier.</value>
		public int Modifier {
			get {
				return Model.Modifier;
			}
			set {
				Model.Modifier = value;
			}
		}

		/// <summary>
		/// Gets or sets the key.
		/// </summary>
		/// <value>The key.</value>
		public int Key {
			get {
				return Model.Key;
			}
			set {
				Model.Key = value;
			}
		}

		/// <summary>
		/// Command to update the hotkey.
		/// </summary>
		/// <value>The upgrade command.</value>
		public Command UpdateHotkeyCommand {
			get;
			set;
		}

		/// <summary>
		/// Closes the window.
		/// </summary>
		public void UpdateHotkey ()
		{
			var hotkey = App.Current.GUIToolkit.SelectHotkey (Model);
			if (hotkey != null) {
				Model.Key = hotkey.Key;
				Model.Modifier = hotkey.Modifier;
			}
		}

		protected override void ForwardPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (NeedsSync (e, nameof (Model.Key)) || NeedsSync (e, nameof (Model.Modifier))) {
				RaisePropertyChanged (nameof (Name), this);
			}
		}
	}
}
