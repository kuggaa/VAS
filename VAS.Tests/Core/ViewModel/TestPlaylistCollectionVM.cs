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
using NUnit.Framework;
using VAS.Core.ViewModel;

namespace VAS.Tests.Core.ViewModel
{
	[TestFixture]
	public class TestPlaylistCollectionVM
	{
		//Menu Names Constants
		const string PLAYLIST_EDIT = "Edit Name";
		const string PLAYLIST_RENDER = "Render";
		const string PLAYLIST_DELETE = "Delete";
		const string ELEMENT_EDIT = "Edit Properties";
		const string ELEMENT_INSERT_BEFORE = "Insert before";
		const string ELEMENT_INSERT_AFTER = "Insert after";
		const string ELEMENT_IMAGE = "External Image";
		const string ELEMENT_VIDEO = "External Video";
		const string ELEMENT_DELETE = "Delete";

		[Test]
		public void PlaylistMenu_ContainsAllCommands ()
		{
			var playlistCollectionVM = new PlaylistCollectionVM ();

			Assert.AreEqual (3, playlistCollectionVM.PlaylistMenu.ViewModels.Count);
			Assert.AreEqual (PLAYLIST_EDIT, playlistCollectionVM.PlaylistMenu.ViewModels [0].Name);
			Assert.AreEqual (PLAYLIST_RENDER, playlistCollectionVM.PlaylistMenu.ViewModels [1].Name);
			Assert.AreEqual (PLAYLIST_DELETE, playlistCollectionVM.PlaylistMenu.ViewModels [2].Name);
		}

		[Test]
		public void PlaylistElementMenu_ContainsAllCommands ()
		{
			var playlistCollectionVM = new PlaylistCollectionVM ();

			Assert.AreEqual (4, playlistCollectionVM.PlaylistElementMenu.ViewModels.Count);
			Assert.AreEqual (2, playlistCollectionVM.PlaylistElementMenu.ViewModels [1].Submenu.ViewModels.Count);
			Assert.AreEqual (2, playlistCollectionVM.PlaylistElementMenu.ViewModels [2].Submenu.ViewModels.Count);
			Assert.AreEqual (ELEMENT_EDIT, playlistCollectionVM.PlaylistElementMenu.ViewModels [0].Name);
			Assert.AreEqual (ELEMENT_INSERT_BEFORE, playlistCollectionVM.PlaylistElementMenu.ViewModels [1].Name);
			Assert.AreEqual (ELEMENT_INSERT_AFTER, playlistCollectionVM.PlaylistElementMenu.ViewModels [2].Name);
			Assert.AreEqual (ELEMENT_DELETE, playlistCollectionVM.PlaylistElementMenu.ViewModels [3].Name);
			Assert.AreEqual (ELEMENT_VIDEO, playlistCollectionVM.PlaylistElementMenu.ViewModels [1].Submenu.ViewModels [0].Name);
			Assert.AreEqual (ELEMENT_IMAGE, playlistCollectionVM.PlaylistElementMenu.ViewModels [1].Submenu.ViewModels [1].Name);
			Assert.AreEqual (ELEMENT_VIDEO, playlistCollectionVM.PlaylistElementMenu.ViewModels [2].Submenu.ViewModels [0].Name);
			Assert.AreEqual (ELEMENT_IMAGE, playlistCollectionVM.PlaylistElementMenu.ViewModels [2].Submenu.ViewModels [1].Name);
		}

	}
}
