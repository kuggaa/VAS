//
//  Copyright (C) 2017 Fluendo S.A.
//
using System;
using System.Linq;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.ViewModel;

namespace VAS.Drawing.CanvasObjects.Statistics
{
	[System.ComponentModel.ToolboxItem (true)]
	public class BarChartView : CanvasObject, ICanvasObjectView<BarChartVM>
	{
		BarChartVM viewModel;

		public BarChartVM ViewModel {
			get {
				return viewModel;
			}

			set {
				viewModel = value;
				if (App.IsMainThread) {
					ReDraw ();
				} else {
					App.Current.GUIToolkit.Invoke ((sender, e) => ReDraw ());
				}
			}
		}

		public void SetViewModel (object viewmodel)
		{
			ViewModel = (BarChartVM)viewmodel;
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			if (ViewModel == null) {
				return;
			}

			tk.Begin ();

			double posX = 0;
			double end;
			double totalX = ViewModel.Series.ViewModels.Sum (x => x.Elements);

			foreach (var serie in ViewModel.Series.Where (x => x.Elements != 0)) {
				tk.FillColor = serie.Color;
				end = (serie.Elements / totalX) * area.Width;
				tk.DrawRectangle (new Point (posX, 0), end, area.Height);
				posX += end;
			}

			tk.End ();
		}
	}
}
