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
using System.Linq;
using NUnit.Framework;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;

namespace VAS.Tests.Core.ViewModel
{
	[TestFixture]
	public class TestProjectVM
	{
		[Test]
		public void TestModelSetup ()
		{
			var model = Utils.CreateProject (true);
			model.Timers.Add (new Timer ());
			model.Playlists.Add (new Playlist ());
			var viewModel = new ProjectVM<Project> {
				Model = model
			};

			Assert.AreEqual (1, viewModel.Timers.Count ());
			Assert.AreEqual (5, viewModel.EventTypes.Count ());
			Assert.AreEqual (1, model.Playlists.Count ());
		}

		[Test]
		public void TestProperties ()
		{
			var model = Utils.CreateProject (true);
			var viewModel = new ProjectVM<Project> {
				Model = model
			};

			Assert.AreEqual (model.FileSet, viewModel.FileSet.Model);
			Assert.AreEqual (model.ShortDescription, viewModel.ShortDescription);
		}

		[Test]
		public void TestForwardProperties ()
		{
			int count = 0;
			var model = Utils.CreateProject (true);
			model.Timers.Add (new Timer ());
			model.Playlists.Add (new Playlist ());
			var viewModel = new ProjectVM<Project> {
				Model = model
			};

			viewModel.PropertyChanged += (sender, e) => count++;
			model.FileSet.Add (new MediaFile ());

			Assert.AreNotEqual (0, count);
		}

		[Test]
		public void TestEdited ()
		{
			var model = Utils.CreateProject (true);
			var viewModel = new ProjectVM<Project> {
				Model = model
			};
			model.IsChanged = false;
			Assert.IsFalse (viewModel.Edited);

			model.Timeline.Clear ();

			Assert.IsTrue (viewModel.Edited);
		}
	}
}
