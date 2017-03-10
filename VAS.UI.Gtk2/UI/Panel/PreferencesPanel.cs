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
using Gdk;
using Gtk;
using VAS.Core;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Services.State;
using VAS.Services.ViewModel;

namespace VAS.UI.Panel
{
	[System.ComponentModel.ToolboxItem (true)]
	[ViewAttribute (PreferencesState.NAME)]
	public partial class PreferencesPanel : Gtk.Bin, IPanel<PreferencesPanelVM>
	{
		Widget selectedPanel;
		ListStore prefsStore;
		PreferencesPanelVM viewModel;

		public PreferencesPanel ()
		{
			this.Build ();
			prefsStore = new ListStore (typeof (IPreferencesVM), typeof (string), typeof (Widget));
			treeview.AppendColumn ("Desc", new CellRendererText (), "text", 1);
			treeview.Model = prefsStore;
			treeview.HeadersVisible = false;
			treeview.EnableGridLines = TreeViewGridLines.None;
			treeview.EnableTreeLines = false;
			treeview.Selection.Mode = SelectionMode.Single;
			treeview.Selection.Changed += HandleSelectionChanged; ;
		}

		/// <summary>
		/// Occurs when the view is destroyed.
		/// </summary>
		protected override void OnDestroyed ()
		{
			OnUnload ();
			base.OnDestroyed ();
		}

		/// <summary>
		/// Releases all resource used by the <see cref="T:VAS.UI.UI.Panel.PreferencesPanel"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="T:VAS.UI.UI.Panel.PreferencesPanel"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="T:VAS.UI.UI.Panel.PreferencesPanel"/> in an unusable state.
		/// After calling <see cref="Dispose"/>, you must release all references to the
		/// <see cref="T:VAS.UI.UI.Panel.PreferencesPanel"/> so the garbage collector can reclaim the memory that the
		/// <see cref="T:VAS.UI.UI.Panel.PreferencesPanel"/> was occupying.</remarks>
		public override void Dispose ()
		{
			Destroy ();
			base.Dispose ();
		}

		/// <summary>
		/// Gets the title.
		/// </summary>
		/// <value>The title.</value>
		public string Title {
			get {
				return Catalog.GetString ("PREFERENCES");
			}
		}

		/// <summary>
		/// Gets the key context.
		/// </summary>
		/// <returns>The key context.</returns>
		public KeyContext GetKeyContext ()
		{
			return new KeyContext ();
		}

		/// <summary>
		/// Occurs when the view is loaded.
		/// </summary>
		public void OnLoad ()
		{
		}

		/// <summary>
		/// Occurs when the view is unloaded.
		/// </summary>
		public void OnUnload ()
		{
		}

		public PreferencesPanelVM ViewModel {
			get {
				return viewModel;
			}

			set {
				if (viewModel != null) {
					RemovePanels ();
				}
				viewModel = value;
				if (viewModel != null) {
					AddPanels ();
					//Select First Panel
					treeview.Selection.SelectPath (new TreePath ("0"));
				}
			}
		}

		/// <summary>
		/// Sets the view model.
		/// </summary>
		/// <param name="viewModel">View model.</param>
		public void SetViewModel (object viewModel)
		{
			ViewModel = (PreferencesPanelVM)viewModel;
		}

		/// <summary>
		/// Adds the specified panel.
		/// </summary>
		/// <param name="desc">Desc.</param>
		/// <param name="icon">Icon.</param>
		/// <param name="pane">Pane.</param>
		public void AddPanel (IPreferencesVM prefViewModel)
		{
			IView view = App.Current.ViewLocator.Retrieve (prefViewModel.View);
			view.SetViewModel (prefViewModel);
			prefsStore.AppendValues (prefViewModel, prefViewModel.Name,
									 view as Widget);
		}

		/// <summary>
		/// Adds the panels.
		/// </summary>
		public void AddPanels ()
		{
			foreach (var vm in viewModel.ViewModels) {
				AddPanel (vm);
			}
		}

		void RemovePanels ()
		{
			prefsStore.Foreach ((model, path, iter) => prefsStore.Remove (ref iter));
		}

		void HandleSelectionChanged (object sender, EventArgs e)
		{
			Widget newPanel;
			TreeIter iter;

			if (selectedPanel != null)
				propsvbox.Remove (selectedPanel);

			treeview.Selection.GetSelected (out iter);
			newPanel = prefsStore.GetValue (iter, 2) as Widget;
			if (newPanel != null) {
				newPanel.Visible = true;
				propsvbox.PackStart (newPanel, true, true, 0);
				selectedPanel = newPanel;
			}
		}
	}
}
