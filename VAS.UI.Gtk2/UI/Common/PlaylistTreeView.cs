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
//  Copyright (C) 2016 Fluendo S.A.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using Gtk;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store.Playlists;
using VAS.Services.ViewModel;
using VAS.UI.Common;
using Misc = VAS.UI.Helpers.Misc;

namespace VAS.UI.Common
{
	[System.ComponentModel.ToolboxItem (true)]
	public class PlaylistTreeView : TreeViewBase<PlaylistCollectionVM, Playlist, PlaylistVM>
	{
		public PlaylistTreeView ()
		{
			HasFocus = false;
			HeadersVisible = false;
			Selection.Mode = SelectionMode.Multiple;
			EnableGridLines = TreeViewGridLines.None;
			CreateViews ();
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
			CreateFilterAndSort ();
		}

		void CreateViews ()
		{
			CellRenderer descCell = new CellRendererText ();
			AppendColumn (null, descCell, RenderPlaylistDescription);
		}

		static string FormatDesc (IViewModel playlist)
		{
			PlaylistVM playlistVM = (PlaylistVM)playlist;
			string desc = playlistVM.Name + " - " + playlistVM.CreationDate;
			return desc;
		}

		void RenderPlaylistDescription (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
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
			// TODO: Load the playlist to the player
		}
	}
}

