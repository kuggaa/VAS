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
using VAS.Core.Common;
using VAS.Core.MVVMC;

namespace VAS.Services.ViewModel
{
	/// <summary>
	/// Upgrade limitation ViewModel, used in the UpgradeLimitation Dialog
	/// Set of properties to configure the UpgradeLimitation view
	/// </summary>
	public class UpgradeLimitationVM : ViewModelBase
	{
		/// <summary>
		/// Gets or sets the upgrade command.
		/// The command that opens the upgrade url
		/// </summary>
		/// <value>The upgrade command.</value>
		public Command UpgradeCommand {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the header.
		/// </summary>
		/// <value>The header.</value>
		public string Header {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the features header.
		/// </summary>
		/// <value>The features header.</value>
		public string FeaturesHeader {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the features.
		/// </summary>
		/// <value>The features.</value>
		public RangeObservableCollection<string> Features {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the features caption.
		/// </summary>
		/// <value>The features caption.</value>
		public string FeaturesCaption {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the undecided.
		/// </summary>
		/// <value>The undecided.</value>
		public string Undecided {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the other plans URL.
		/// </summary>
		/// <value>The other plans URL.</value>
		public string OtherPlansURL {
			get;
			set;
		}
	}
}
