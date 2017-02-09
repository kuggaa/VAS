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

using System.ComponentModel;
using Gdk;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.ViewModel;
using VAS.UI.Helpers;

namespace VAS.UI.Component
{
	/// <summary>
	/// Widget to show limitations in the form [count/max][upgrade button].
	/// </summary>
	[System.ComponentModel.ToolboxItem (true)]
	public partial class LimitationWidget : Gtk.Bin, IView<LicenseLimitationVM>
	{
		LicenseLimitationVM viewModel;

		public LimitationWidget ()
		{
			this.Build ();
			count_label.SetPadding (5, 0);
			limit_label.SetPadding (5, 0);
			limit_label.Name = StyleConf.LabelLimit;
			limit_box.Name = StyleConf.BoxLimit;
		}

		/// <summary>
		/// Gets or sets the view model.
		/// </summary>
		/// <value>The view model.</value>
		public LicenseLimitationVM ViewModel {
			get {
				return viewModel;
			}

			set {
				if (viewModel != null) {
					viewModel.PropertyChanged -= HandlePropertyChangedEventHandler;
				}
				viewModel = value;
				if (viewModel != null) {
					Visible = viewModel.Enabled;
					viewModel.PropertyChanged += HandlePropertyChangedEventHandler;
					count_label.Text = ViewModel.Count.ToString ();
					limit_label.Text = ViewModel.Maximum.ToString ();
					upgradeButton.ApplyStyleLimit (ViewModel.UpgradeCommand);
					CheckCount ();
				}
			}
		}

		/// <summary>
		/// Sets the view model.
		/// </summary>
		/// <param name="viewModel">View model.</param>
		public void SetViewModel (object viewModel)
		{
			ViewModel = (LicenseLimitationVM)(viewModel as dynamic);
		}

		void HandlePropertyChangedEventHandler (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (ViewModel.Count)) {
				count_label.Text = ViewModel.Count.ToString ();
				CheckCount ();
			} else if (e.PropertyName == nameof (ViewModel.Maximum)) {
				limit_label.Text = ViewModel.Maximum.ToString ();
				CheckCount ();
			} else if (e.PropertyName == nameof (ViewModel.Enabled)) {
				Visible = ViewModel.Enabled;
			}
		}

		void CheckCount ()
		{
			if (ViewModel.Count >= ViewModel.Maximum) {
				count_label.Name = StyleConf.LabelLimit;
				separator_label.Name = StyleConf.LabelLimit;
			} else {
				count_label.Name = "CountLabel";
				separator_label.Name = "SeparatorLabel";
			}
		}
	}
}
