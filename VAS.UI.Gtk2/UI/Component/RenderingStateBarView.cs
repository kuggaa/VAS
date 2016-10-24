//
//  Copyright (C) 2016 Fluendo S.A.
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
using VAS.Core.Interfaces.MVVMC;
using VAS.Services.ViewModel;

namespace VAS.UI.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class RenderingStateBarView : Gtk.Bin, IView<RenderingStateBarVM>
	{
		RenderingStateBarVM viewModel;

		public RenderingStateBarView ()
		{
			this.Build ();
			progressbar.CanFocus = false;
			cancelbutton.CanFocus = false;
			statebutton.CanFocus = false;

			statebutton.Clicked += delegate (object sender, EventArgs e) {
				viewModel?.CommandManageJob ();
			};
			cancelbutton.Clicked += delegate (object sender, EventArgs e) {
				viewModel?.CommandCancelJob ();
			};
			progressbar.Fraction = 0;
		}

		public RenderingStateBarVM ViewModel {
			get {
				return viewModel;
			}

			set {
				if (viewModel != null) {
					viewModel.PropertyChanged -= HandlePropertyChanged;
				}
				viewModel = value;
				if (viewModel != null) {
					viewModel.PropertyChanged += HandlePropertyChanged;
					SyncVMValues ();
				}
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = viewModel as RenderingStateBarVM;
		}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			SyncVMValues ();
		}

		void SyncVMValues ()
		{
			if (viewModel == null)
				return;

			Visible = viewModel.JobRunning;
			statebutton.Label = viewModel.Text;
			progressbar.Text = viewModel.ProgressText;
			progressbar.Fraction = viewModel.Fraction;
		}
	}
}
