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
using System.Linq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Drawing;
using VAS.Drawing.Cairo;
using VAS.Drawing.CanvasObjects.Dashboard;

namespace VAS.Tests.Core.ViewModel
{
	[TestFixture]
	public class TestDashboardVM
	{
		DashboardVM dashboard;
		Utils.DashboardDummy model;

		[SetUp]
		public void Setup ()
		{
			model = Utils.DashboardDummy.Default ();
			dashboard = new DashboardVM { Model = model };
		}

		[Test]
		public void TestSetModel ()
		{
			Assert.AreEqual (6, dashboard.ViewModels.Count);
			Assert.AreSame (model.List [0], dashboard.ViewModels [0].Model);
		}

		[Test]
		public void TestModelSyncVM ()
		{
			model.InsertTimer ();
			model.InsertTimer ();
			model.InsertTimer ();

			Assert.AreEqual (9, dashboard.ViewModels.Count);
			Assert.AreSame (model.List [7], dashboard.ViewModels [7].Model);
		}

		[Test]
		public void TestButtonVMInstanceCreationType ()
		{
			model.InsertTimer ();
			model.AddDefaultItem (model.List.Count);

			Assert.AreEqual ("TimerButtonVM", dashboard.ViewModels [6].GetType ().Name);
			Assert.AreEqual ("AnalysisEventButtonVM", dashboard.ViewModels [7].GetType ().Name);
		}

		[Test]
		public void TestViewInstanceCreation ()
		{
			AnalysisEventButtonView analysisButtonView;
			TimerButtonView timerButtonView;

			DrawingInit.ScanViews ();
			App.Current.DrawingToolkit = new CairoBackend ();

			Assert.DoesNotThrow (() => analysisButtonView = (AnalysisEventButtonView)App.Current.ViewLocator.Retrieve (dashboard.ViewModels [0].View));
			Assert.DoesNotThrow (() => timerButtonView = (TimerButtonView)App.Current.ViewLocator.Retrieve (dashboard.ViewModels [5].View));
		}

		[Test]
		public void TestPropagateCurrentTime ()
		{
			Time time = new Time (5000);
			dashboard.CurrentTime = time;

			foreach (var button in dashboard.ViewModels.OfType<TimedDashboardButtonVM> ()) {
				Assert.AreEqual (time, button.CurrentTime);
			}
		}

		[Test]
		public void TestAddButton_PropertiesInSync ()
		{
			TimedDashboardButtonVM viewModel = new TimedDashboardButtonVM { Model = new TimedDashboardButton () };
			viewModel.Mode = DashboardMode.Edit;
			dashboard.Mode = DashboardMode.Code;
			Time time = new Time (5000);
			dashboard.CurrentTime = time;
			dashboard.ViewModels.Add (viewModel);

			Assert.AreEqual (DashboardMode.Code, viewModel.Mode);
			Assert.AreEqual (time, viewModel.CurrentTime);
		}

		[Test]
		public void DeleteButton_RemovedFromSelection ()
		{
			DashboardButtonVM button = dashboard.ViewModels [0];
			dashboard.SelectionReplace (button.ToEnumerable ());

			dashboard.ViewModels.Remove (button);

			Assert.IsEmpty (dashboard.Selection);
		}
	}
}
