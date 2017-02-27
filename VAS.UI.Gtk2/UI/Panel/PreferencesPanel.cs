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
using VAS.Core.MVVMC;
using VAS.Services.State;
using VAS.UI.UI.Component;

namespace VAS.UI.UI.Panel
{
	[System.ComponentModel.ToolboxItem (true)]
	[ViewAttribute (PreferencesState.NAME)]
	public partial class PreferencesPanel : Gtk.Bin, IPanel
	{
		Widget selectedPanel;
		ListStore prefsStore;

		public PreferencesPanel ()
		{
			this.Build ();
			prefsStore = new ListStore (typeof (Pixbuf), typeof (string), typeof (Widget));
			treeview.AppendColumn ("Icon", new CellRendererPixbuf (), "pixbuf", 0);
			treeview.AppendColumn ("Desc", new CellRendererText (), "text", 1);
			treeview.CursorChanged += HandleCursorChanged;
			treeview.Model = prefsStore;
			treeview.HeadersVisible = false;
			treeview.EnableGridLines = TreeViewGridLines.None;
			treeview.EnableTreeLines = false;
			AddPanels ();
			treeview.SetCursor (new TreePath ("0"), null, false);
			panelheader1.ApplyVisible = false;
			panelheader1.Title = Title;
			panelheader1.BackClicked += (sender, e) => {
				App.Current.StateController.MoveBack ();
			};
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

		/// <summary>
		/// Sets the view model.
		/// </summary>
		/// <param name="viewModel">View model.</param>
		public void SetViewModel (object viewModel)
		{
		}

		/// <summary>
		/// Adds the specified panel.
		/// </summary>
		/// <param name="desc">Desc.</param>
		/// <param name="icon">Icon.</param>
		/// <param name="pane">Pane.</param>
		public void AddPanel (string desc, Pixbuf icon, Widget pane)
		{
			prefsStore.AppendValues (icon, desc, pane);
		}

		/// <summary>
		/// Adds the panels.
		/// </summary>
		public void AddPanels ()
		{
			AddPanel (Catalog.GetString ("Keyboard shortcuts"),
				Helpers.Misc.LoadIcon ("longomatch-shortcut", IconSize.Dialog, 0),
				new HotkeysConfiguration ());
		}

		/// <summary>
		/// Handles the cursor changed.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		void HandleCursorChanged (object sender, EventArgs e)
		{
			Widget newPanel;
			TreeIter iter;

			if (selectedPanel != null)
				propsvbox.Remove (selectedPanel);

			treeview.Selection.GetSelected (out iter);
			newPanel = prefsStore.GetValue (iter, 2) as Widget;
			newPanel.Visible = true;
			propsvbox.PackStart (newPanel, true, true, 0);
			selectedPanel = newPanel;
		}
	}
}
