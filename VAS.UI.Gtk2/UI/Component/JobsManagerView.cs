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
using System.Collections.Specialized;
using System.Linq;
using VAS.Core.Common;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VAS.Services.State;
using VAS.Services.ViewModel;
using VAS.UI.TreeViews;

namespace VAS.UI.UI.Component
{
	[ViewAttribute (JobsManagerState.NAME)]
	[System.ComponentModel.ToolboxItem (true)]
	public partial class JobsManagerView : Gtk.Bin, IPanel<JobsManagerVM>
	{
		JobsManagerVM viewModel;
		JobsTreeView treeview;

		public JobsManagerView ()
		{
			this.Build ();
			treeview = new JobsTreeView ();
			treeview.ShowAll ();
			treeviewbox.PackStart (treeview, true, true, 0);
			cancelbutton.Visible = false;
			retrybutton.Visible = false;
			cancelbutton.Clicked += OnCancelbuttonClicked;
			clearbutton.Clicked += OnClearbuttonClicked;
			retrybutton.Clicked += OnRetrybuttonClicked;
			acceptButton.Clicked += async (sender, e) => await App.Current.StateController.MoveBack ();
		}

		protected override void OnDestroyed ()
		{
			OnUnload ();
			base.OnDestroyed ();
		}

		public override void Dispose ()
		{
			Destroy ();
			base.Dispose ();
		}

		public string Title {
			get {
				return JobsManagerState.NAME;
			}
		}

		public JobsManagerVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
				viewModel.Selection.CollectionChanged += HandleCollectionChanged;
				treeview.ViewModel = value;
			}
		}

		public void OnLoad ()
		{
		}

		public void OnUnload ()
		{
		}

		public KeyContext GetKeyContext ()
		{
			return new KeyContext ();
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = viewModel as JobsManagerVM;
		}

		protected virtual void OnClearbuttonClicked (object sender, EventArgs e)
		{
			ViewModel.ClearFinished ();
		}

		protected virtual void OnCancelbuttonClicked (object sender, EventArgs e)
		{
			ViewModel.Cancel (ViewModel.Selection.Select (jvm => jvm.Model).ToList ());
		}

		protected virtual void OnRetrybuttonClicked (object sender, EventArgs e)
		{
			ViewModel.Retry (ViewModel.Selection.Select (jvm => jvm.Model).ToList ());
		}

		void HandleCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			// FIXME: Remove when we start using ICommand's
			JobVM job = ViewModel.Selection.FirstOrDefault ();
			cancelbutton.Visible = job != null && job.State == JobState.Running;
			retrybutton.Visible = job != null && (job.State == JobState.Error || job.State == JobState.Cancelled);
		}

	}
}
