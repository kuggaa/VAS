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
using System.ComponentModel;
using Pango;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Resources.Styles;
using VAS.Core.ViewModel;
using VAS.UI.Helpers;
using VAS.UI.Helpers.Bindings;

namespace VAS.UI.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class LicenseBannerView : Gtk.Bin, IView<LicenseBannerVM>
	{
		const int TEXT_MIN_SIZE = 370;
		const int TEXT_MAX_SIZE = 570;
		LicenseBannerVM viewModel;
		BindingContext ctx;

		public LicenseBannerView ()
		{
			this.Build ();
			licenseTextLabel.ModifyFont (FontDescription.FromString ($"{App.Current.Style.Font} {Sizes.LicenseTextFontSize}px"));
			licenseTextLabel.DynamicSize (TEXT_MIN_SIZE, TEXT_MAX_SIZE);
			upgradeButton.ApplyStyle (StyleConf.ButtonCallToActionRounded);
			upgradeButton.WidthRequest = Sizes.LicenseBannerUpgradeButtonWidth;
			upgradeButton.HeightRequest = Sizes.LicenseBannerUpgradeButtonHeight;
			alignment1.WidthRequest = Sizes.LicenseBannerUpgradeButtonWidth;
			Bind ();
		}

		public LicenseBannerVM ViewModel {
			get {
				return viewModel;
			}

			set {
				if (viewModel != null) {
					viewModel.PropertyChanged -= HandleViewModelPropertyChanged;
				}
				viewModel = value;
				ctx.UpdateViewModel (viewModel);
				if (viewModel != null) {
					viewModel.PropertyChanged += HandleViewModelPropertyChanged;
					viewModel.Sync ();
				}
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (LicenseBannerVM)viewModel;
		}

		void Bind ()
		{
			ctx = this.GetBindingContext ();
			ctx.Add (upgradeButton.Bind (vm => ((LicenseBannerVM)vm).UpgradeCommand));
		}

		void HandleViewModelPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (ViewModel.NeedsSync (e.PropertyName, nameof (ViewModel.Text))) {
				licenseTextLabel.LabelProp = ViewModel.Text;
			}
			if (ViewModel.NeedsSync (e.PropertyName, nameof (ViewModel.Visible))) {
				this.Visible = ViewModel.Visible;
			}
		}
	}
}
