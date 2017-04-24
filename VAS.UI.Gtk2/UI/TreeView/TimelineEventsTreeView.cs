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
using System.Collections.Generic;
using System.Linq;
using Gtk;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.UI.Common;

namespace VAS.UI.Component
{
	/// <summary>
	/// A base class <see cref="TreeView"/> that lists <see cref="TimelineEventVM"/>'s grouped by different types, like
	/// <see cref="EventTypeVM"/> or <see cref="PlayerVM"/> 
	/// </summary>
	public abstract class TimelineEventsTreeView<TViewModel, TModel> : TreeViewBase<NestedViewModel<TViewModel>, TModel, TViewModel>
		where TViewModel : class, IViewModel<TModel>, new()
	{
		protected bool isSelecting;
		TimelineVM viewModel;

		public TimelineEventsTreeView ()
		{
			HasFocus = false;
			HasTooltip = true;
			HeadersVisible = false;
			Selection.Mode = SelectionMode.Multiple;
			EnableGridLines = TreeViewGridLines.None;

			AppendColumn (null, CreateCellRenderer (), RenderEvents);
			CreateFilterAndSort ();
		}

		protected override void OnDestroyed ()
		{
			ViewModel?.Dispose ();
			ViewModel = null;
			base.OnDestroyed ();
		}

		public new TimelineVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
				if (viewModel != null) {
					base.ViewModel = GetSubTimeline (viewModel);
				}
				filter?.Refilter ();
			}
		}

		public override void SetViewModel (object viewModel)
		{
			ViewModel = viewModel as TimelineVM;
		}

		/// <summary>
		/// Return the subtimeline used by this treeview, like the event types's or the player's treeview.
		/// </summary>
		/// <returns>The sub timeline.</returns>
		/// <param name="viewModel">View model.</param>
		protected abstract NestedViewModel<TViewModel> GetSubTimeline (TimelineVM viewModel);

		/// <summary>
		/// Creates the cell renderer used in this tree view.
		/// </summary>
		/// <returns>The cell renderer.</returns>
		protected abstract CellRenderer CreateCellRenderer ();

		/// <summary>
		/// Shows the menu for the a list of events.
		/// </summary>
		/// <param name="events">Events.</param>
		protected abstract void ShowMenu (IEnumerable<TimelineEventVM> events);

		/// <summary>
		/// Sets the ViewModel in the cell renderer before drawing.
		/// </summary>
		/// <param name="cell">Cell.</param>
		/// <param name="iter">Iter.</param>
		/// <param name="viewModel">View model.</param>
		protected abstract void SetCellViewModel (CellRenderer cell, TreeIter iter, IViewModel viewModel);


		protected override void ShowMenu ()
		{
			IEnumerable<IViewModel> viewModels = GetSelectedViewModels ();
			IEnumerable<TimelineEventVM> events = viewModels.OfType<TimelineEventVM> ();

			if (!events.Any ()) {
				events = viewModels.OfType<EventTypeTimelineVM> ().SelectMany (p => p.ViewModels).Where (vm => vm.Visible);
			}

			ShowMenu (events);
		}

		void RenderEvents (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			IViewModel viewModel = model.GetValue (iter, COL_DATA) as IViewModel;
			SetCellViewModel (cell, iter, viewModel);
		}

		protected override void HandleTreeviewSelectionChanged (object sender, EventArgs e)
		{
			List<TimelineEventVM> events = new List<TimelineEventVM> ();

			if (!isSelecting) {
				isSelecting = true;
				ViewModel.UnselectAll ();
				foreach (TimelineEventVM eventVM in GetSelectedViewModels ().OfType<TimelineEventVM> ()) {
					eventVM.Selected = true;
					events.Add (eventVM);
					foreach (TreeIter iter in dictionaryStore[eventVM]) {
						Selection.SelectIter (iter);
					}
				}
				if (events.Any ()) {
					ViewModel.LoadEvents (events, false);
				}
				isSelecting = false;
			}
			if (Selection.CountSelectedRows () == 0) {
				ViewModel.UnloadEvents ();
			}

			// update selection
			ViewModel.FullTimeline.Selection.Replace (events);
		}

		protected override void HandleTreeviewRowActivated (object o, RowActivatedArgs args)
		{
			TimelineEventVM viewModel = GetViewModelAtPath (args.Path) as TimelineEventVM;
			if (viewModel != null) {
				ViewModel.LoadEvent (viewModel, true);
			}
		}
	}
}
