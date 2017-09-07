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
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.Drawing.CanvasObjects.Dashboard;
using VAS.Drawing.Widgets;

namespace VAS.Tests.Drawing.Widgets
{
	[TestFixture]
	public class TestDashboardCanvas
	{
		DashboardVM dashboard;
		DashboardCanvas dashboardCanvas;

		[TestFixtureSetUp]
		public void Init ()
		{
			App.Current.ViewLocator = new ViewLocator ();
			App.Current.ViewLocator.Register ("AnalysisEventButtonView", typeof (DummyDashboardButtonView));
			App.Current.ViewLocator.Register ("TimerButtonView", typeof (DummyDashboardButtonView));
			var drawingToolkitMock = new Mock<IDrawingToolkit> ();
			drawingToolkitMock.Setup (d => d.CreateSurfaceFromResource (It.IsAny<string> (), It.IsAny<bool> (), It.IsAny<bool> ())).
				  Returns (Mock.Of<ISurface> ());
			App.Current.DrawingToolkit = drawingToolkitMock.Object;
		}

		[SetUp]
		public void SetUp ()
		{
			dashboard = new DashboardVM { Model = Utils.DashboardDummy.Default (), Mode = DashboardMode.Code };
			dashboardCanvas = new DashboardCanvas ();
			dashboardCanvas.ViewModel = dashboard;
		}

		[Test]
		public void TestFillDashboardCanvas ()
		{
			Assert.AreEqual (6, dashboardCanvas.Objects.Count);
		}

		[Test]
		public void TestAddButton ()
		{
			dashboard.Model.AddDefaultItem (0);
			Assert.AreEqual (7, dashboardCanvas.Objects.Count);
		}

		[Test]
		public void TestRemoveButton ()
		{
			dashboard.Model.List.Remove (dashboard.Model.List [0]);
			Assert.AreEqual (5, dashboardCanvas.Objects.Count);
		}

		[Test]
		public void TestClear ()
		{
			dashboard.Model.List.Clear ();
			Assert.AreEqual (0, dashboardCanvas.Objects.Count);
		}

		[Test]
		public void TestUpdateMode ()
		{
			dashboard.Mode = DashboardMode.Edit;

			Assert.IsTrue (dashboardCanvas.ObjectsCanMove);
		}

		[Test]
		public void UpdateShowLinksToTrue_InEditMode_ShowLinks ()
		{
			dashboard.ShowLinks = true;
			dashboard.Mode = DashboardMode.Edit;

			foreach (DashboardButtonView to in dashboardCanvas.Objects.OfType<DashboardButtonView> ()) {
				Assert.IsTrue (to.ShowLinks);
			}
		}

		[Test]
		public void UpdateShowLinksToTrue_InCodeMode_LinksNotShowed ()
		{
			dashboard.ShowLinks = true;
			dashboard.Mode = DashboardMode.Code;

			foreach (DashboardButtonView to in dashboardCanvas.Objects.OfType<DashboardButtonView> ()) {
				Assert.IsFalse (to.ShowLinks);
			}
		}

		[Test]
		public void SelectionChanged_EmptySelection_SelectionSynced ()
		{
			DashboardButtonVM button = dashboard.ViewModels [0];
			dashboard.Selection.Replace (button.ToEnumerable ());

			Assert.AreEqual (1, dashboardCanvas.Objects.OfType<DashboardButtonView> ().
							 Count (b => b.Selected == true));
		}

		[Test]
		public void SeletionChanged_PreviousSelection_SelectionSynced ()
		{
			DashboardButtonVM button = dashboard.ViewModels [1];
			dashboard.Selection.Replace (button.ToEnumerable ());
			dashboard.Selection.Replace (dashboard.ViewModels);

			Assert.AreEqual (dashboard.ViewModels.Count,
							 dashboardCanvas.Objects.OfType<DashboardButtonView> ().
							 Count (b => b.Selected == true));
		}

	}
}
