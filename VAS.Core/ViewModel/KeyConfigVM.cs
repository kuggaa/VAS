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
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Core.ViewModel
{
	public class KeyConfigVM : ViewModelBase<KeyConfig>
	{

		public KeyConfigVM ()
		{
			EditCommand = new Command (EditKey, () => { return true; });
			EditCommand.Icon = Resources.LoadIcon ("longomatch-control-draw");
			EditCommand.ToolTipText = Catalog.GetString ("Edit Shortcut");
		}

		/// <summary>
		/// Gets or sets the key that performs the action
		/// </summary>
		/// <value>The key.</value>
		public HotKey Key {
			get {
				return Model.Key;
			}
			set {
				Model.Key = value;
			}
		}

		/// <summary>
		/// Gets or sets the category.
		/// </summary>
		/// <value>The category.</value>
		public string Category {
			get {
				return Model.Category;
			}
			set {
				Model.Category = value;
			}
		}

		/// <summary>
		/// Gets or sets the description of the HotKey
		/// </summary>
		/// <value>The description.</value>
		public string Description {
			get {
				return Model.Description;
			}
			set {
				Model.Description = value;
			}
		}

		[PropertyChanged.DoNotNotify]
		public Command EditCommand {
			get;
			protected set;
		}

		async Task EditKey ()
		{
			await App.Current.EventsBroker.PublishWithReturn (new EditEvent<KeyConfig> {
				Object = Model
			});
		}
	}
}
