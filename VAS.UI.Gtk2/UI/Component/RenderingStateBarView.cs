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
using VAS.Core;
using VAS.Core.Interfaces.MVVMC;
using VAS.Services.ViewModel;
using VAS.Core.Common;
using VAS.Services.State;

namespace VAS.UI.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class RenderingStateBarView : Gtk.Bin, IView<JobsManagerVM>
	{
		JobsManagerVM viewModel;

		public RenderingStateBarView ()
		{
			this.Build ();
			progressbar.CanFocus = false;
			cancelbutton.CanFocus = false;
			statebutton.CanFocus = false;

			statebutton.Clicked += async (s, e) => {
				await App.Current.StateController.MoveToModal (JobsManagerState.NAME, null);
			};
			cancelbutton.Clicked += (s, e) => {
				viewModel.Cancel ();
			};
			progressbar.Fraction = 0;
		}

		public JobsManagerVM ViewModel {
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
			ViewModel = viewModel as JobsManagerVM;
		}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			SyncVMValues (e.PropertyName);
		}

		void SyncVMValues (string propertyName = null)
		{
			if (viewModel == null)
				return;

			if (propertyName == null || propertyName == "State") {
				Visible = viewModel.CurrentJob.Model != null && viewModel.CurrentJob.State == JobState.Running;
			}
			if (propertyName == null || propertyName == "Collection") {
				statebutton.Label = string.Format ("{0} ({1} {2})",
												   Catalog.GetString ("Rendering queue"),
												   ViewModel.PendingJobs.Count,
												   Catalog.GetString ("Pending"));
			}
			if (propertyName == null || propertyName == "Progress") {
				progressbar.Fraction = viewModel.CurrentJob.Progress;
				progressbar.Text = string.Format ("{0}... {1:0.0}%",
					Catalog.GetString ("Rendering"), viewModel.CurrentJob.Progress * 100);
			}
		}
	}
}
