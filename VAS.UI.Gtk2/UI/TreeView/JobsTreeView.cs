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
using Gtk;
using Stetic;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.ViewModel;
using VAS.Services.ViewModel;
using VAS.UI.Common;

namespace VAS.UI.TreeViews
{
	[System.ComponentModel.ToolboxItem (true)]
	public class JobsTreeView : TreeViewBase<JobsManagerVM, Job, JobVM>
	{
		public JobsTreeView ()
		{
			HasFocus = false;
			HeadersVisible = true;
			Selection.Mode = SelectionMode.Multiple;
			EnableGridLines = TreeViewGridLines.None;
			CreateViews ();
		}

		protected override void OnDestroyed ()
		{
			SetViewModel (null);
			base.OnDestroyed ();
		}

		public override void SetViewModel (object viewModel)
		{
			if (ViewModel != null) {
				ViewModel.PropertyChanged -= HandleViewModelPropertyChanged;
			}
			base.SetViewModel (viewModel);
			if (viewModel != null) {
				ViewModel.PropertyChanged += HandleViewModelPropertyChanged;
			}
		}

		void CreateViews ()
		{
			TreeViewColumn nameColumn = new TreeViewColumn ();
			nameColumn.Title = Catalog.GetString ("Job name");
			CellRendererText nameCell = new CellRendererText ();
			nameColumn.PackStart (nameCell, true);

			TreeViewColumn stateColumn = new TreeViewColumn ();
			stateColumn.Title = Catalog.GetString ("State");
			CellRendererPixbuf stateCell = new CellRendererPixbuf ();
			stateColumn.PackStart (stateCell, false);

			nameColumn.SetCellDataFunc (nameCell, new TreeCellDataFunc (RenderName));
			stateColumn.SetCellDataFunc (stateCell, new TreeCellDataFunc (RenderState));

			AppendColumn (nameColumn);
			AppendColumn (stateColumn);
		}

		void RenderName (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			JobVM jobVM = model.GetValue (iter, 0) as JobVM;
			if (jobVM != null) {
				(cell as CellRendererText).Text = jobVM.Name;
			}
		}

		void RenderState (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			string iconName = "";
			JobVM jobVM = model.GetValue (iter, 0) as JobVM;

			switch (jobVM.State) {
			case JobState.Error:
				iconName = "gtk-dialog-error";
				break;
			case JobState.Finished:
				iconName = "gtk-ok";
				break;
			case JobState.Cancelled:
				iconName = "gtk-cancel";
				break;
			case JobState.Pending:
				iconName = "gtk-execute";
				break;
			case JobState.Running:
				iconName = "gtk-media-record";
				break;
			}
			(cell as CellRendererPixbuf).Pixbuf = IconLoader.LoadIcon (this, iconName, IconSize.Button);
		}
	}
}
