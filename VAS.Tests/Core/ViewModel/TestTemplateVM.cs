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
using System.Linq;
using NUnit.Framework;
using VAS.Core.ViewModel;

namespace VAS.Tests.Core.ViewModel
{
	[TestFixture]
	public class TestTemplateVM
	{
		[Test]
		public void TestProperties ()
		{
			var model = new Utils.DashboardDummy {
				Name = "dash",
			};
			var viewModel = new DummyDashboardViewModel {
				Model = model
			};
			model.IsChanged = false;

			Assert.AreEqual (false, viewModel.Edited);
			Assert.AreEqual ("dash", viewModel.Name);
			Assert.AreEqual (null, viewModel.Icon);
		}

		[Test]
		public void TestEdited ()
		{
			var model = new Utils.DashboardDummy {
				Name = "dash",
			};
			var viewModel = new DummyDashboardViewModel {
				Model = model
			};
			model.IsChanged = false;
			Assert.AreEqual (false, viewModel.Edited);

			model.Name = "Test";
			Assert.AreEqual (true, viewModel.Edited);
		}

		[Test]
		public void TestModelSync ()
		{
			var model = new DummyTeam {
				Name = "dash",
			};
			model.List.Add (new Utils.PlayerDummy ());
			model.List.Add (new Utils.PlayerDummy ());
			model.List.Add (new Utils.PlayerDummy ());

			var viewModel = new DummyTeamVM {
				Model = model
			};

			Assert.AreEqual (3, viewModel.Count ());
			Assert.IsInstanceOf (typeof (PlayerVM), viewModel.First ());
		}

		[Test]
		public void TestAddChild ()
		{
			var model = new DummyTeam {
				Name = "dash",
			};
			model.List.Add (new Utils.PlayerDummy ());
			model.List.Add (new Utils.PlayerDummy ());
			model.List.Add (new Utils.PlayerDummy ());
			var viewModel = new DummyTeamVM {
				Model = model
			};

			model.List.Add (new Utils.PlayerDummy ());

			Assert.AreEqual (4, viewModel.Count ());
			Assert.IsInstanceOf (typeof (PlayerVM), viewModel.Last ());
		}

		[Test]
		public void TestRemoveChild ()
		{
			var model = new DummyTeam {
				Name = "dash",
			};
			model.List.Add (new Utils.PlayerDummy ());
			model.List.Add (new Utils.PlayerDummy ());
			var viewModel = new DummyTeamVM {
				Model = model
			};

			model.List.Remove (model.List [0]);

			Assert.AreEqual (1, viewModel.Count ());
		}

		[Test]
		public void TestSelectionInit ()
		{
			var model = new DummyTeam {
				Name = "dash",
			};
			var viewModel = new DummyTeamVM {
				Model = model
			};

			Assert.IsNotNull (viewModel.Selection);
		}

		[Test]
		public void TestSelect ()
		{
			var model = new DummyTeam {
				Name = "dash",
			};
			var player = new Utils.PlayerDummy ();
			model.List.Add (player);
			var viewModel = new DummyTeamVM {
				Model = model
			};

			viewModel.Select (viewModel.ViewModels.First ());

			Assert.AreEqual (1, viewModel.Selection.Count);
			Assert.AreEqual (player, viewModel.Selection.First ().Model);
		}
	}
}
