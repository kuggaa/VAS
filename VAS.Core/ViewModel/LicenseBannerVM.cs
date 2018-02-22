//
//  Copyright (C) 2018 Fluendo S.A.
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
using VAS.Core.Interfaces.Services;
using VAS.Core.MVVMC;
using VAS.Core.Resources;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// ViewModel to use in the LicenseBannerView, it contains the text to display based on the license current state
	/// and an UpgradeCommand
	/// </summary>
	public class LicenseBannerVM : ViewModelBase
	{
		readonly ILicenseCustomizationService licenseService;

		public LicenseBannerVM ()
		{
			licenseService = App.Current.DependencyRegistry.Retrieve<ILicenseCustomizationService> ();
			UpgradeCommand = new Command (HandleUpgradeCommand) {
				Text = Strings.UpgradeNow.ToUpper ()
			};
		}

		/// <summary>
		/// Gets or sets the text.
		/// </summary>
		/// <value>The text.</value>
		public string Text { get; set; }

		/// <summary>
		/// Gets or sets the upgrade command.
		/// </summary>
		/// <value>The upgrade command.</value>
		public Command UpgradeCommand { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.LicenseBannerVM"/> is visible.
		/// </summary>
		/// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
		public bool Visible { get; set; }

		void HandleUpgradeCommand ()
		{
			licenseService.OpenUpgradeURL ();
		}
	}
}
