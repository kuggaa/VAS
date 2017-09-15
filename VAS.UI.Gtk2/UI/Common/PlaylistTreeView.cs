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
using System.Collections.Generic;
using System.Linq;
using Gtk;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;
using VAS.UI.Menus;

namespace VAS.UI.Common
{
	[System.ComponentModel.ToolboxItem (true)]
	public class PlaylistTreeView : TreeViewBase<PlaylistCollectionVM, Playlist, PlaylistVM>
	{
		protected MenuBase playlistMenu;
		protected MenuBase playlistElementMenu;

		public PlaylistTreeView ()
		{
			HasFocus = false;
			HeadersVisible = false;
			Selection.Mode = SelectionMode.Multiple;
			EnableGridLines = TreeViewGridLines.None;
			CreateMenu ();
			CreateViews ();
			CreateDragDest (new [] { new TargetEntry (Constants.PlaylistElementsDND, TargetFlags.App, 0),
				new TargetEntry (Constants.TimelineEventsDND, TargetFlags.App, 0)});
			CreateDragSource (new [] { new TargetEntry (Constants.PlaylistElementsDND, TargetFlags.App, 0) });
		}

		protected override void OnDestroyed ()
		{
			playlistMenu.Dispose ();
			base.OnDestroyed ();
		}

		public override PlaylistCollectionVM ViewModel {
			get {
				return base.ViewModel;
			}
			set {
				base.ViewModel = value;
				playlistMenu.ViewModel = value?.PlaylistMenu;
				playlistElementMenu.ViewModel = value?.PlaylistElementMenu;
			}
		}

		protected virtual void CreateMenu ()
		{
			playlistMenu = new MenuBase ();
			playlistElementMenu = new MenuBase ();
		}

		protected virtual void CreateViews ()
		{
			CellRenderer descCell = new CellRendererText ();
			AppendColumn (null, descCell, RenderPlaylist);
		}

		static string FormatDesc (IViewModel playlist)
		{
			PlaylistVM playlistVM = (PlaylistVM)playlist;
			string desc = playlistVM.Name + " - " + playlistVM.CreationDate;
			return desc;
		}

		protected virtual void RenderPlaylist (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			string desc;
			var obj = model.GetValue (iter, COL_DATA);
			PlaylistVM playlistVM = obj as PlaylistVM;
			if (playlistVM == null) {
				PlaylistElementVM plElement = obj as PlaylistElementVM;
				if (plElement == null) {
					desc = "";
				} else {
					desc = plElement.Description;
				}
			} else {
				desc = FormatDesc (playlistVM);
			}
			(cell as CellRendererText).Text = desc;
		}

		protected override void HandleTreeviewRowActivated (object o, RowActivatedArgs args)
		{
			TreeIter iter;
			TreeIter parent;
			Model.GetIter (out iter, args.Path);
			var element = Model.GetValue (iter, COL_DATA) as PlaylistElementVM;
			if (element != null) {
				if (Model.IterParent (out parent, iter)) {
					var playlist = Model.GetValue (parent, COL_DATA) as PlaylistVM;
					ViewModel.LoadPlaylist (playlist, element, true);
				}
			}
		}

		protected override void HandleTreeviewSelectionChanged (object sender, EventArgs e)
		{
			if (ViewModel == null) {
				return;
			}
			TreeIter iter;
			TreeIter parent;
			base.HandleTreeviewSelectionChanged (sender, e);
			if (!ViewModel.Selection.Any ()) {
				if (Selection.GetSelectedRows ().Count () == 1) {
					var path = Selection.GetSelectedRows ().First ();
					Model.GetIter (out iter, path);
					var element = Model.GetValue (iter, COL_DATA) as PlaylistElementVM;
					if (element != null) {
						if (Model.IterParent (out parent, iter)) {
							var playlist = Model.GetValue (parent, COL_DATA) as PlaylistVM;
							ViewModel.LoadPlaylist (playlist, element, false);
						}
					}
				}
			}
			UpdatePlaylistElementsSelection ();
		}

		protected override void ShowMenu ()
		{
			if (ViewModel.Selection.Count () > 0) {
				playlistMenu.ShowMenu ();
			} else {
				playlistElementMenu.ShowMenu ();
			}
		}

		protected override bool MoveElements (Dictionary<INestedViewModel, List<IViewModel>> elementsToRemove,
											  KeyValuePair<INestedViewModel, List<IViewModel>> elementsToAdd, int index)
		{
			if (elementsToRemove.Keys.OfType<PlaylistVM> ().Count () == elementsToRemove.Count &&
				elementsToAdd.Key is PlaylistVM) {

				var toRemove = elementsToRemove.ToDictionary (e => e.Key as PlaylistVM, e => e.Value.OfType<PlaylistElementVM> ());
				var toAdd = new KeyValuePair<PlaylistVM, IEnumerable<PlaylistElementVM>> (
					elementsToAdd.Key as PlaylistVM, elementsToAdd.Value.OfType<PlaylistElementVM> ());

				ViewModel.MovePlaylistElements (toRemove, toAdd, index);
				return true;
			} else if (elementsToAdd.Key is PlaylistCollectionVM){
				PlaylistVM toMove = (PlaylistVM)elementsToRemove.Values.First ().First ();
				App.Current.EventsBroker.Publish (new MoveElementsEvent<PlaylistVM> {
					ElementToMove = toMove,
					Index = index
				});
			}
			return false;
		}

		void UpdatePlaylistElementsSelection ()
		{
			TreeIter iter;
			Dictionary<PlaylistVM, List<PlaylistElementVM>> selected =
					new Dictionary<PlaylistVM, List<PlaylistElementVM>> ();
			IViewModel parentVM, childVM;
			foreach (var path in Selection.GetSelectedRows ()) {
				parentVM = childVM = null;
				Model.GetIterFromString (out iter, path.ToString ());
				FillParentAndChild (iter, out parentVM, out childVM);
				PlaylistElementVM selectedViewModel = childVM as PlaylistElementVM;
				if (selectedViewModel != null) {
					PlaylistVM playlist = parentVM as PlaylistVM;
					if (!selected.ContainsKey (playlist)) {
						selected.Add (playlist, new List<PlaylistElementVM> ());
					}
					selected [playlist].Add (selectedViewModel);
				}
			}

			IEnumerable<PlaylistVM> unselected = ViewModel.Selection.Except (selected.Keys);
			foreach (var playlist in unselected) {
				if (playlist.Selection.Any ()) {
					playlist.Selection.Clear ();
				}
			}
			foreach (var selections in selected) {
				selections.Key.Selection.Replace (selections.Value);
			}

			if (!selected.Any ()) {
				foreach (var playlist in ViewModel.ViewModels.Where ((arg) => arg.Selection.Any ())) {
					playlist.Selection.Clear ();
				}
			}
		}
	}
}

