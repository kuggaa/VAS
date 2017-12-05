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
using VAS.Core.ViewModel;
using VAS.Services.State;
using VAS.Services.ViewModel;
using VAS.UI.Helpers.Bindings;
using VAS.UI.TreeViews;

namespace VAS.UI.UI.Component
{
	[ViewAttribute (JobsManagerState.NAME)]
	[System.ComponentModel.ToolboxItem (true)]
	public partial class JobsManagerView : Gtk.Bin, IPanel<JobsManagerVM>
	{
		BindingContext ctx;
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
			acceptButton.Clicked += async (sender, e) => await App.Current.StateController.MoveBack ();
			Bind ();
		}

		public override void Dispose ()
		{
			Dispose (true);
			base.Dispose ();
		}

		protected virtual void Dispose (bool disposing)
		{
			if (Disposed) {
				return;
			}
			if (disposing) {
				Destroy ();
			}
			Disposed = true;
		}

		protected override void OnDestroyed ()
		{
			Log.Verbose ($"Destroying {GetType ()}");
			ViewModel = null;
			ViewModel?.Dispose ();
			ctx.Dispose ();
			ctx = null;
			treeview.Dispose ();
			treeview = null;
			base.OnDestroyed ();
			Disposed = true;
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
				if (viewModel != null) {
					viewModel.Selection.CollectionChanged -= HandleCollectionChanged;
				}
				viewModel = value;
				if (viewModel != null) {
					viewModel.Selection.CollectionChanged += HandleCollectionChanged;
					treeview.ViewModel = value;
				}
				ctx.UpdateViewModel (viewModel);
			}
		}

		protected bool Disposed { get; private set; } = false;

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

		void Bind ()
		{
			ctx = this.GetBindingContext ();
			ctx.Add (clearbutton.Bind (vm => ((JobsManagerVM)vm).ClearFinishedCommand));
			ctx.Add (cancelbutton.Bind (vm => ((JobsManagerVM)vm).CancelSelectedCommand));
			ctx.Add (retrybutton.Bind (vm => ((JobsManagerVM)vm).RetryCommand));
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
