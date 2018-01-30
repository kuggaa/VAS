//
//  Copyright (C) 2017 Fluendo S.A.
//
using System.ComponentModel;
using System.Linq;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.ViewModel.Statistics;

namespace VAS.Drawing.CanvasObjects.Statistics
{
	[System.ComponentModel.ToolboxItem (true)]
	public class BarChartView : FixedSizeCanvasObject, ICanvasObjectView<BarChartVM>
	{
		BarChartVM viewModel;

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			ViewModel?.Dispose ();
			ViewModel = null;
		}

		public BarChartVM ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.PropertyChanged -= HandlePropertyChanged;
				}
				viewModel = value;
				if (viewModel != null) {
					viewModel.PropertyChanged += HandlePropertyChanged;
					CallRedraw ();
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

			double height = ViewModel.Height < 0.1f ? Height : ViewModel.Height;
			double width = Width - ViewModel.LeftPadding - ViewModel.RightPadding;

			tk.Begin ();

			double posX = Position.X + ViewModel.LeftPadding;
			double posY = Position.Y + ViewModel.TopPadding;

			double end;
			double totalX = ViewModel.Series.ViewModels.Sum (x => x.Elements);

			if (ViewModel.Background != null) {
				tk.DrawImage (Position, Width, Height, ViewModel.Background, ScaleMode.Fill);
			}

			foreach (var serie in ViewModel.Series.Where (x => x.Elements != 0)) {
				tk.FillColor = serie.Color;
				tk.StrokeColor = serie.Color;
				end = (serie.Elements / totalX) * width;
				tk.DrawRectangle (new Point (posX, posY), end, height);
				posX += end;
			}

			tk.End ();
		}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			CallRedraw ();
		}

		void CallRedraw ()
		{
			if (App.IsMainThread) {
				ReDraw ();
			} else {
				App.Current.GUIToolkit.Invoke ((sender, e) => ReDraw ());
			}
		}
	}
}
