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
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;

namespace VAS.Services.ViewModel
{
	/// <summary>
	/// Hotkeys configuration ViewModel for the HotkeysConfigurationView
	/// </summary>
	public class HotkeysConfigurationVM : CollectionViewModel<KeyConfig, KeyConfigVM>, IPreferencesVM
	{
		public const string VIEW = "HotkeysConfiguration";

		public HotkeysConfigurationVM ()
		{
			Model = new RangeObservableCollection<KeyConfig> (App.Current.HotkeysService.GetAll ());
		}

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name {
			get {
				return Catalog.GetString ("Keyboard shortcuts");
			}
		}

		/// <summary>
		/// Gets the view.
		/// </summary>
		/// <value>The view.</value>
		public string View {
			get {
				return VIEW;
			}
		}

		/// <summary>
		/// Gets the categories.
		/// </summary>
		/// <value>The categories.</value>
		public IEnumerable<string> Categories {
			get {
				return ViewModels.Select ((arg) => arg.Category).Distinct ();
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Services.ViewModel.HotkeysConfigurationVM"/> auto save.
		/// </summary>
		/// <value><c>true</c> if auto save; otherwise, <c>false</c>.</value>
		public bool AutoSave {
			get;
			set;
		}

		/// <summary>
		/// Cancel operation, only takes effect when AutoSave = false
		/// </summary>
		public void Cancel ()
		{
			foreach (var vm in ViewModels) {
				vm.Cancel ();
			}
		}

		/// <summary>
		/// Save operation, only takes effect when AutoSave = false
		/// </summary>
		public void Save ()
		{
			App.Current.EventsBroker.Publish (new SaveEvent<KeyConfig> ());
		}
	}
}
