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
using VAS.Core.License;
using VAS.Core.MVVMC;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// Limitation ViewModel base class, used to Limit a something by using a LicenseLimitation as Model
	/// </summary>
	public class LimitationVM : ViewModelBase<LicenseLimitation>
	{
		/// <summary>
		/// Proxy property for DisplayName.
		/// </summary>
		/// <value>The name of the limitation.</value>
		public string DisplayName {
			get {
				return Model?.DisplayName;
			}
		}

		/// <summary>
		/// Proxy property for RegisterName.
		/// </summary>
		/// <value>The name registered for the limitation.</value>
		public string RegisterName {
			get {
				return Model?.RegisterName;
			}
		}

		/// <summary>
		/// Proxy property for Enabled.
		/// </summary>
		/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
		public bool Enabled {
			get {
				return Model?.Enabled ?? false;
			}
		}
	}

	/// <summary>
	/// Feature limitation ViewModel, to limit application features.
	/// It has the LimitationCommand that every ViewModel that has a FeatureLimitationVM
	/// should use.
	/// </summary>
	public class FeatureLimitationVM : LimitationVM
	{
		/// <summary>
		/// Gets or sets the model.
		/// </summary>
		/// <value>The model.</value>
		public new FeatureLicenseLimitation Model {
			get {
				return (FeatureLicenseLimitation)base.Model;
			}
			set {
				base.Model = value;
			}
		}

		/// <summary>
		/// Gets the detail info.
		/// </summary>
		/// <value>The detail info.</value>
		public string DetailInfo {
			get {
				return Model?.DetailInfo;
			}
		}
	}

	/// <summary>
	/// Count Limitation ViewModel, to limit the number of limited objects in the application
	/// For example: projects, playlists, events, etc.
	/// </summary>
	public class CountLimitationVM : LimitationVM
	{
		Command upgradeCommand;

		/// <summary>
		/// Gets or sets the model.
		/// </summary>
		/// <value>The model.</value>
		public new CountLicenseLimitation Model {
			get {
				return (CountLicenseLimitation)base.Model;
			}
			set {
				base.Model = value;
				if (UpgradeCommand != null) {
					UpgradeCommand.Executable = Model.Enabled;
				}
			}
		}

		/// <summary>
		/// Proxy property for Count.
		/// </summary>
		/// <value>The model's Count value.</value>
		public int Count {
			get {
				return Model?.Count ?? 0;
			}
			set {
				Model.Count = value;
			}
		}

		/// <summary>
		/// Proxy property for Maximum.
		/// </summary>
		/// <value>The maximum.</value>
		public int Maximum {
			get {
				return Model?.Maximum ?? 0;
			}
		}

		/// <summary>
		/// Proxy property for Remaining.
		/// Maximum - Count
		/// </summary>
		/// <value>Remaining number.</value>
		public int Remaining {
			get {
				return Maximum - Count;
			}
		}

		/// <summary>
		/// Command to upgrade away from this limitation.
		/// </summary>
		/// <value>The upgrade command.</value>
		public Command UpgradeCommand {
			get {
				return upgradeCommand;
			}
			set {
				upgradeCommand = value;
				upgradeCommand.Executable = Enabled;
			}
		}

		protected override void RaisePropertyChanged (PropertyChangedEventArgs args, object sender = null)
		{
			if (args.PropertyName == nameof (Model.Enabled)) {
				if (UpgradeCommand != null) {
					UpgradeCommand.Executable = Enabled;
				}
			}
			base.RaisePropertyChanged (args, sender);
		}
	}
}
