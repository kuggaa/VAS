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
using NUnit.Framework;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;
using VAS.Core.Store;

namespace VAS.Tests.Core.ViewModel
{
	[TestFixture]
	public class TestPlaylistVM
	{
		[Test]
		public void TestProperties ()
		{
			var model = new Playlist {
				Name = "Playlist",
				CreationDate = DateTime.Today,
				LastModified = DateTime.Now,
			};
			var viewModel = new PlaylistVM {
				Model = model,
			};

			Assert.AreEqual (model.Name, viewModel.Name);
			Assert.AreEqual (model.CreationDate, viewModel.CreationDate);
			Assert.AreEqual (model.LastModified, viewModel.LastModified);
		}

		[Test]
		public void TestForwardProperties ()
		{
			int count = 0;

			var model = new Playlist {
				Name = "Playlist",
				CreationDate = DateTime.Today,
				LastModified = DateTime.Now,
			};
			var viewModel = new PlaylistVM {
				Model = model,
			};

			viewModel.PropertyChanged += (sender, e) => count++;
			model.Name = "Test";

			Assert.AreEqual (1, count);
		}

		[Test]
		public void TestAddElement ()
		{
			int count = 0;

			var model = new Playlist ();
			var viewModel = new PlaylistVM {
				Model = model,
			};

			viewModel.PropertyChanged += (sender, e) => {
				count++;
			};

			model.Elements.Add (new PlaylistPlayElement (new TimelineEvent ()));
			Assert.AreEqual (3, count);
			Assert.AreEqual (1, viewModel.ViewModels.Count);
		}
	}
}
