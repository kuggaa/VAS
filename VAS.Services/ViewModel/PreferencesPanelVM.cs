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
using System.Threading.Tasks;
using VAS.Core;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;

namespace VAS.Services.ViewModel
{
	/// <summary>
	/// ViewModel to use in the PreferencesPanel, it contains a collection of different PreferencesPanelVM
	/// </summary>
	public class PreferencesPanelVM : NestedViewModel<IPreferencesVM>
	{
		bool autoSave;
		bool closing;

		public PreferencesPanelVM ()
		{
			CancelCommand = new Command (Cancel, () => { return true; });
			CancelCommand.Text = Catalog.GetString ("Cancel");
			OkCommand = new Command (Accept, () => { return true; });
			OkCommand.Text = Catalog.GetString ("Ok");
		}

		public bool AutoSave {
			get {
				return autoSave;
			}

			set {
				autoSave = value;
				foreach (var vm in ViewModels) {
					vm.AutoSave = value;
				}
			}
		}

		public Command CancelCommand {
			get;
			set;
		}

		public Command OkCommand {
			get;
			set;
		}

		public void Close ()
		{
			if (!closing && !AutoSave) {
				foreach (var vm in ViewModels) {
					vm.Cancel ();
				}
			}
		}

		async Task Cancel ()
		{
			foreach (var vm in ViewModels) {
				vm.Cancel ();
			}
			closing = true;
			await App.Current.StateController.MoveBack ();
		}

		async Task Accept ()
		{
			foreach (var vm in ViewModels) {
				vm.Save ();
			}
			closing = true;
			await App.Current.StateController.MoveBack ();
		}
	}
}
