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

using System.Collections.Generic;
using System.Linq;
using Gtk;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;
using VAS.UI.Menus;

namespace VAS.UI.Common
{
	[System.ComponentModel.ToolboxItem (true)]
	public class PlaylistTreeView : TreeViewBase<PlaylistCollectionVM, Playlist, PlaylistVM>
	{
		protected PlaylistMenu playlistMenu;

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

		public override void Dispose ()
		{
			if (ViewModel != null) {
				ViewModel.PropertyChanged -= HandleViewModelPropertyChanged;
			}
			base.Dispose ();
		}

		public override void SetViewModel (object viewModel)
		{
			if (ViewModel != null) {
				ViewModel.PropertyChanged -= HandleViewModelPropertyChanged;
			}
			base.SetViewModel (viewModel);
			ViewModel.PropertyChanged += HandleViewModelPropertyChanged;
		}

		protected virtual void CreateMenu ()
		{
			playlistMenu = new PlaylistMenu ();
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
		}

		protected override void ShowMenu ()
		{
			if (ViewModel.Selection.Count () > 0)
				playlistMenu.ShowMenu (null, ViewModel.Selection [0].Model, true);
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
			}
			return false;
		}
	}
}

